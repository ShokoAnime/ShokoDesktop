using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for MultipleFilesControl.xaml
    /// </summary>
    public partial class MultipleFilesControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        BackgroundWorker workerFiles = new BackgroundWorker();
        public ICollectionView ViewEpisodes { get; set; }
        public ObservableCollection<AnimeEpisodeVM> CurrentEpisodes { get; set; }

        public static readonly DependencyProperty EpisodeCountProperty = DependencyProperty.Register("EpisodeCount",
            typeof(int), typeof(MultipleFilesControl), new UIPropertyMetadata(0, null));

        public int EpisodeCount
        {
            get { return (int)GetValue(EpisodeCountProperty); }
            set { SetValue(EpisodeCountProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(false, null));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set
            {
                SetValue(IsLoadingProperty, value);
                IsNotLoading = !IsLoading;
            }
        }

        public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get { return (bool)GetValue(IsNotLoadingProperty); }
            set { SetValue(IsNotLoadingProperty, value); }
        }

        public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("", null));

        public string StatusMessage
        {
            get { return (string)GetValue(StatusMessageProperty); }
            set { SetValue(StatusMessageProperty, value); }
        }

        private List<JMMServerBinary.Contract_AnimeEpisode> contracts = new List<JMMServerBinary.Contract_AnimeEpisode>();

        public MultipleFilesControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            IsLoading = false;

            CurrentEpisodes = new ObservableCollection<AnimeEpisodeVM>();
            ViewEpisodes = CollectionViewSource.GetDefaultView(CurrentEpisodes);
            ViewEpisodes.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
            ViewEpisodes.SortDescriptions.Add(new SortDescription("EpisodeTypeAndNumberAbsolute", ListSortDirection.Ascending));

            workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
            workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            chkOnlyFinished.IsChecked = AppSettings.MultipleFilesOnlyFinished;

            chkOnlyFinished.Checked += new RoutedEventHandler(chkOnlyFinished_Checked);
        }

        void chkOnlyFinished_Checked(object sender, RoutedEventArgs e)
        {
            AppSettings.MultipleFilesOnlyFinished = chkOnlyFinished.IsChecked.Value;
        }

        void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                contracts = e.Result as List<JMMServerBinary.Contract_AnimeEpisode>;
                foreach (JMMServerBinary.Contract_AnimeEpisode ep in contracts)
                    CurrentEpisodes.Add(new AnimeEpisodeVM(ep));

                EpisodeCount = contracts.Count;

                btnRefresh.IsEnabled = true;
                IsLoading = false;
                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void workerFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MultipleFilesRefreshOptions opt = e.Argument as MultipleFilesRefreshOptions;
                List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllEpisodesWithMultipleFiles(
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value, opt.OnlyFinishedSeries, opt.IgnoreVariations);
                e.Result = eps;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void RefreshMultipleFiles()
        {
            if (workerFiles.IsBusy) return;

            IsLoading = true;
            btnRefresh.IsEnabled = false;
            CurrentEpisodes.Clear();
            EpisodeCount = 0;

            StatusMessage = Properties.Resources.Loading;


            MultipleFilesRefreshOptions opt = new MultipleFilesRefreshOptions()
            {
                OnlyFinishedSeries = chkOnlyFinished.IsChecked.Value,
                IgnoreVariations = chkIgnoreVariations.IsChecked.Value
            };

            workerFiles.RunWorkerAsync(opt);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshMultipleFiles();
        }

        private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VideoDetailedVM))
                {
                    VideoDetailedVM vid = obj as VideoDetailedVM;

                    if (File.Exists(vid.FullPath))
                    {
                        Utils.OpenFolderAndSelectFile(vid.FullPath);
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayVideo(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VideoDetailedVM))
                {
                    VideoDetailedVM vid = obj as VideoDetailedVM;
                    //AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.VideoLocal_ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid,force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }



    }

    public class MultipleFilesRefreshOptions
    {
        public bool OnlyFinishedSeries { get; set; }
        public bool IgnoreVariations { get; set; }
    }
}
