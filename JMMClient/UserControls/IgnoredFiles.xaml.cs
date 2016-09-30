using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for IgnoredFiles.xaml
    /// </summary>
    public partial class IgnoredFiles : UserControl
    {
        public ICollectionView ViewFiles { get; set; }
        public ObservableCollection<VideoLocalVM> IgnoredFilesCollection { get; set; }

        public static readonly DependencyProperty OneVideoSelectedProperty = DependencyProperty.Register("OneVideoSelected",
            typeof(bool), typeof(IgnoredFiles), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty MultipleVideosSelectedProperty = DependencyProperty.Register("MultipleVideosSelected",
            typeof(bool), typeof(IgnoredFiles), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(IgnoredFiles), new UIPropertyMetadata(0, null));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        public bool OneVideoSelected
        {
            get { return (bool)GetValue(OneVideoSelectedProperty); }
            set { SetValue(OneVideoSelectedProperty, value); }
        }

        public bool MultipleVideosSelected
        {
            get { return (bool)GetValue(MultipleVideosSelectedProperty); }
            set { SetValue(MultipleVideosSelectedProperty, value); }
        }

        public IgnoredFiles()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            IgnoredFilesCollection = new ObservableCollection<VideoLocalVM>();
            ViewFiles = CollectionViewSource.GetDefaultView(IgnoredFilesCollection);
            ViewFiles.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Ascending));

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            lbVideos.SelectionChanged += new SelectionChangedEventHandler(lbVideos_SelectionChanged);
            OneVideoSelected = lbVideos.SelectedItems.Count == 1;
            MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;
        }

        void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ccDetail.Content = null;
                ccDetailMultiple.Content = null;

                OneVideoSelected = lbVideos.SelectedItems.Count == 1;
                MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;

                // if only one video selected
                if (OneVideoSelected)
                {
                    VideoLocalVM vid = lbVideos.SelectedItem as VideoLocalVM;
                    ccDetail.Content = vid;
                }

                // if only one video selected
                if (MultipleVideosSelected)
                {
                    MultipleVideos mv = new MultipleVideos();
                    mv.SelectedCount = lbVideos.SelectedItems.Count;
                    mv.VideoLocalIDs = new List<int>();

                    foreach (object obj in lbVideos.SelectedItems)
                    {
                        VideoLocalVM vid = obj as VideoLocalVM;
                        mv.VideoLocalIDs.Add(vid.VideoLocalID);
                    }

                    ccDetailMultiple.Content = mv;
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
                if (obj.GetType() == typeof(VideoLocalVM))
                {
                    VideoLocalVM vid = obj as VideoLocalVM;
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid, force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VideoLocalVM))
                {
                    VideoLocalVM vid = obj as VideoLocalVM;

                    if (File.Exists(vid.LocalFileSystemFullPath))
                    {
                        Utils.OpenFolderAndSelectFile(vid.LocalFileSystemFullPath);
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

        private void EnableDisableControls(bool val)
        {
            lbVideos.IsEnabled = val;
            btnRefresh.IsEnabled = val;
            ccDetail.IsEnabled = val;
            ccDetailMultiple.IsEnabled = val;
        }

        private void CommandBinding_RestoreFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VideoLocalVM))
                {
                    VideoLocalVM vid = obj as VideoLocalVM;
                    EnableDisableControls(false);

                    string result = JMMServerVM.Instance.clientBinaryHTTP.SetIgnoreStatusOnFile(vid.VideoLocalID, false);
                    if (result.Length > 0)
                        MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        RefreshIgnoredFiles();

                }
                if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    foreach (int id in mv.VideoLocalIDs)
                    {
                        string result = JMMServerVM.Instance.clientBinaryHTTP.SetIgnoreStatusOnFile(id, false);
                        if (result.Length > 0)
                            MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    RefreshIgnoredFiles();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VideoLocalVM))
                {
                    VideoLocalVM vid = obj as VideoLocalVM;


                    AskDeleteFile dlg = new AskDeleteFile(string.Format(Properties.Resources.DeleteFile_Title, vid.FileName), Properties.Resources.Unrecognized_ConfirmDelete + "\r\n\r\n" + Properties.Resources.DeleteFile_Confirm, vid.Places);
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        EnableDisableControls(false);
                        string tresult = string.Empty;
                        this.Cursor = Cursors.Wait;
                        foreach (VideoLocal_PlaceVM lv in dlg.Selected)
                        {
                            string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        RefreshIgnoredFiles();
                    }


                }

                if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    AskDeleteFile dlg = new AskDeleteFile(Properties.Resources.DeleteFile_Multiple, Properties.Resources.Unrecognized_DeleteSelected + "\r\n\r\n" + Properties.Resources.DeleteFile_Confirm, mv.VideoLocals.SelectMany(a=>a.Places).ToList());
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        EnableDisableControls(false);
                        string tresult = string.Empty;
                        this.Cursor = Cursors.Wait;
                        foreach (VideoLocal_PlaceVM lv in dlg.Selected)
                        {
                            string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        RefreshIgnoredFiles();
                    }
                }


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshIgnoredFiles();
        }

        public void RefreshIgnoredFiles()
        {
            try
            {
                IgnoredFilesCollection.Clear();

                List<JMMServerBinary.Contract_VideoLocal> vids = JMMServerVM.Instance.clientBinaryHTTP.GetIgnoredFiles(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                FileCount = vids.Count;

                foreach (JMMServerBinary.Contract_VideoLocal vid in vids)
                {
                    IgnoredFilesCollection.Add(new VideoLocalVM(vid));
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
