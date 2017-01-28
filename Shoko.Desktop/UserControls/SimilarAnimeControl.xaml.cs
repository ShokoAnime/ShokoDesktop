using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;


namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for SimilarAnimeControl.xaml
    /// </summary>
    public partial class SimilarAnimeControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private BackgroundWorker missingDataWorker = new BackgroundWorker();

        public static readonly DependencyProperty DataExistsProperty = DependencyProperty.Register("DataExists",
            typeof(bool), typeof(SimilarAnimeControl), new UIPropertyMetadata(false, null));

        public bool DataExists
        {
            get { return (bool)GetValue(DataExistsProperty); }
            set { SetValue(DataExistsProperty, value); }
        }

        public static readonly DependencyProperty DataMissingProperty = DependencyProperty.Register("DataMissing",
            typeof(bool), typeof(SimilarAnimeControl), new UIPropertyMetadata(false, null));

        public bool DataMissing
        {
            get { return (bool)GetValue(DataMissingProperty); }
            set { SetValue(DataMissingProperty, value); }
        }

        public string SeriesName
        {
            get
            {
                if (DataContext == null) { return ""; }
                VM_AnimeSeries_User series = (VM_AnimeSeries_User)DataContext;
                return series.SeriesName + " " + Shoko.Commons.Properties.Resources.NoSimilarAnime;
            }
        }

        public ObservableCollection<VM_AniDB_Anime_Similar> SimilarAnimeLinks { get; set; }

        public SimilarAnimeControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            SimilarAnimeLinks = new ObservableCollection<VM_AniDB_Anime_Similar>();

            DataContextChanged += new DependencyPropertyChangedEventHandler(SimilarAnimeControl_DataContextChanged);

            btnGetSimMissingInfo.Click += new RoutedEventHandler(btnGetSimMissingInfo_Click);

            missingDataWorker.DoWork += new DoWorkEventHandler(missingDataWorker_DoWork);
            missingDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(missingDataWorker_RunWorkerCompleted);
        }

        void missingDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Window wdw = Window.GetWindow(this);
            wdw.Cursor = Cursors.Arrow;
            wdw.IsEnabled = true;
        }

        void missingDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (VM_AniDB_Anime_Similar sim in SimilarAnimeLinks)
                {
                    if (!sim.AnimeInfoExists)
                    {
                        string result = VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(sim.SimilarAnimeID);
                        if (string.IsNullOrEmpty(result))
                        {
                            VM_AniDB_Anime animeContract = (VM_AniDB_Anime)VM_ShokoServer.Instance.ShokoServices.GetAnime(sim.SimilarAnimeID);
                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)delegate ()
                            {
                                sim.PopulateAnime(animeContract);
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void SimilarAnimeControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public void RefreshData()
        {
            try
            {
                VM_AniDB_Anime anime = null;

                VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
                if (animeSeries == null)
                {
                    SimilarAnimeLinks.Clear();
                    return;
                }
                SimilarAnimeLinks.Clear();
                anime = animeSeries.AniDBAnime.AniDBAnime;

                if (anime == null) return;




                List<VM_AniDB_Anime_Similar> tempList = VM_ShokoServer.Instance.ShokoServices.GetSimilarAnimeLinks(anime.AnimeID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).Cast<VM_AniDB_Anime_Similar>().OrderByDescending(a => a.ApprovalPercentage).ToList();
                foreach (VM_AniDB_Anime_Similar sim in tempList)
                    SimilarAnimeLinks.Add(sim);

                if (SimilarAnimeLinks.Count == 0)
                {
                    DataExists = false;
                    DataMissing = true;
                }
                else
                {
                    DataExists = true;
                    DataMissing = false;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void GetMissingSimilarData()
        {
            Window wdw = Window.GetWindow(this);
            wdw.Cursor = Cursors.Wait;
            wdw.IsEnabled = false;

            missingDataWorker.RunWorkerAsync();
        }

        void btnGetSimMissingInfo_Click(object sender, RoutedEventArgs e)
        {
            GetMissingSimilarData();
        }
    }
}
