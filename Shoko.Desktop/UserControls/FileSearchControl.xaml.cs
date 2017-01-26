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
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for FileSearchControl.xaml
    /// </summary>
    public partial class FileSearchControl : UserControl
    {
        public ICollectionView ViewFiles { get; set; }
        public ObservableCollection<VM_VideoLocal> FileResults { get; set; }

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(FileSearchControl), new UIPropertyMetadata(0, null));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        private readonly string SearchTypeFileName = Properties.Resources.Search_FileName;
        private readonly string SearchTypeHash = Properties.Resources.Hash;
        private readonly string SearchTypeTopOneHundred = Properties.Resources.Search_Last100;

        private readonly string SearchTypeFileSize = Properties.Resources.Search_FileSize;

        BackgroundWorker getDetailsWorker = new BackgroundWorker();
        private int displayingVidID = 0;

        public FileSearchControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            FileResults = new ObservableCollection<VM_VideoLocal>();
            ViewFiles = CollectionViewSource.GetDefaultView(FileResults);

            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
            lbVideos.SelectionChanged += new SelectionChangedEventHandler(lbVideos_SelectionChanged);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboSearchType.Items.Clear();
            cboSearchType.Items.Add(Properties.Resources.Search_FileName);
            cboSearchType.Items.Add(Properties.Resources.Hash);
            cboSearchType.Items.Add(Properties.Resources.Search_Last100);
            cboSearchType.SelectedIndex = 0;

            cboSearchType.SelectionChanged += new SelectionChangedEventHandler(cboSearchType_SelectionChanged);

            getDetailsWorker.DoWork += new DoWorkEventHandler(getDetailsWorker_DoWork);
            getDetailsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDetailsWorker_RunWorkerCompleted);
        }

        void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get detailed video, episode and series info
            ccDetail.Content = null;
            ccSeriesDetail.Content = null;
            if (lbVideos.SelectedItems.Count == 0) return;
            if (lbVideos.SelectedItem == null) return;

            VM_VideoLocal vid = lbVideos.SelectedItem as VM_VideoLocal;
            displayingVidID = vid.VideoLocalID;
            EnableDisableControls(false);

            try
            {
                Cursor = Cursors.Wait;

                ccDetail.Content = vid;

                // get the episode(s)
                VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(vid.VideoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID).FirstOrDefault();


                if (ep!=null)
                    ccSeriesDetail.Content = ep;

                Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                EnableDisableControls(true);
            }
        }

        void getDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        void getDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        void cboSearchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileSearchCriteria searchType = FileSearchCriteria.Name;
            if (cboSearchType.SelectedItem.ToString() == SearchTypeHash) searchType = FileSearchCriteria.ED2KHash;
            if (cboSearchType.SelectedItem.ToString() == SearchTypeTopOneHundred) searchType = FileSearchCriteria.LastOneHundred;

            if (searchType == FileSearchCriteria.LastOneHundred)
                txtFileSearch.Visibility = Visibility.Collapsed;
            else
                txtFileSearch.Visibility = Visibility.Visible;
        }



        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!VM_ShokoServer.Instance.ServerOnline) return;

                FileSearchCriteria searchType = FileSearchCriteria.Name;
                if (cboSearchType.SelectedItem.ToString() == SearchTypeHash) searchType = FileSearchCriteria.ED2KHash;
                if (cboSearchType.SelectedItem.ToString() == SearchTypeTopOneHundred) searchType = FileSearchCriteria.LastOneHundred;

                if (txtFileSearch.Text.Trim().Length == 0 && searchType != FileSearchCriteria.LastOneHundred)
                {
                    MessageBox.Show(Properties.Resources.Seach_Criteria, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtFileSearch.Focus();
                    return;
                }

                FileResults.Clear();
                ViewFiles.Refresh();
                FileCount = 0;

                Cursor = Cursors.Wait;
                EnableDisableControls(false);

                List<VM_VideoLocal> rawVids = VM_ShokoServer.Instance.ShokoServices.SearchForFiles(
                    (int)searchType, txtFileSearch.Text, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoLocal>();

                foreach (VM_VideoLocal raw in rawVids)
                    FileResults.Add(raw);

                FileCount = rawVids.Count;

                Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                EnableDisableControls(true);
            }
        }

        private void EnableDisableControls(bool val)
        {
            lbVideos.IsEnabled = val;
            btnSearch.IsEnabled = val;
            ccDetail.IsEnabled = val;
            ccSeriesDetail.IsEnabled = val;
        }

        private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            if (obj.GetType() == typeof(VM_VideoLocal))
            {
                VM_VideoLocal vid = obj as VM_VideoLocal;

                if (File.Exists(vid.GetLocalFileSystemFullPath()))
                {
                    Utils.OpenFolderAndSelectFile(vid.GetLocalFileSystemFullPath());
                }
                else
                {
                    MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void CommandBinding_RehashFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;
                    EnableDisableControls(false);

                    VM_ShokoServer.Instance.ShokoServices.RehashFile(vid.VideoLocalID);
                }

                MessageBox.Show(Properties.Resources.MSG_INFO_AddedQueueCmds, "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }

        private void CommandBinding_RescanFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;
                    EnableDisableControls(false);

                    VM_ShokoServer.Instance.ShokoServices.RescanFile(vid.VideoLocalID);
                }

                MessageBox.Show(Properties.Resources.MSG_INFO_AddedQueueCmds, Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }
    }
}
