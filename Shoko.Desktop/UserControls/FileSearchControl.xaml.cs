using System;
using System.Collections;
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
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

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
        
        public static readonly DependencyProperty OneVideoSelectedProperty = DependencyProperty.Register("OneVideoSelected",
            typeof(bool), typeof(FileSearchControl), new UIPropertyMetadata(false, null));
        
        public static readonly DependencyProperty MultipleVideosSelectedProperty = DependencyProperty.Register("MultipleVideosSelected",
            typeof(bool), typeof(FileSearchControl), new UIPropertyMetadata(false, null));

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

        private readonly string SearchTypeFileName = Shoko.Commons.Properties.Resources.Search_FileName;
        private readonly string SearchTypeHash = Shoko.Commons.Properties.Resources.Hash;
        private readonly string SearchTypeTopOneHundred = Shoko.Commons.Properties.Resources.Search_Last100;

        private readonly string SearchTypeFileSize = Shoko.Commons.Properties.Resources.Search_FileSize;

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
            
            txtFileSearch.KeyDown += TxtFileSearchOnKeyDown;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboSearchType.Items.Clear();
            cboSearchType.Items.Add(Shoko.Commons.Properties.Resources.Search_FileName);
            cboSearchType.Items.Add(Shoko.Commons.Properties.Resources.Hash);
            cboSearchType.Items.Add(Shoko.Commons.Properties.Resources.Search_Last100);
            cboSearchType.SelectedIndex = 0;

            cboSearchType.SelectionChanged += new SelectionChangedEventHandler(cboSearchType_SelectionChanged);

            getDetailsWorker.DoWork += new DoWorkEventHandler(getDetailsWorker_DoWork);
            getDetailsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDetailsWorker_RunWorkerCompleted);
        }

        private void TxtFileSearchOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;

            btnSearch_Click(sender, null);
        }

        void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get detailed video, episode and series info
            if (lbVideos.SelectedItems.Count == 0)
            {
                ccDetail.Content = null;
                ccSeriesDetail.Content = null;
                OneVideoSelected = false;
                MultipleVideosSelected = false;
                return;
            }

            if (lbVideos.SelectedItems.Count == 1)
            {
                VM_VideoLocal vid = lbVideos.SelectedItem as VM_VideoLocal;
                OneVideoSelected = true;
                MultipleVideosSelected = false;
                displayingVidID = vid.VideoLocalID;
                EnableDisableControls(false);

                try
                {
                    Cursor = Cursors.Wait;

                    ccDetail.Content = vid;

                    // get the episode(s)
                    VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) VM_ShokoServer.Instance.ShokoServices
                        .GetEpisodesForFile(vid.VideoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                        .FirstOrDefault();

                    // whether it's null or not
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
            else
            {
                try
                {
                    Cursor = Cursors.Wait;
                    var vids = lbVideos.SelectedItems.Cast<VM_VideoLocal>().ToList();
                    OneVideoSelected = false;
                    MultipleVideosSelected = true;
                    MultipleVideos mv = new MultipleVideos();
                    mv.SelectedCount = vids.Count;
                    mv.VideoLocalIDs = new List<int>();
                    mv.VideoLocals = new List<VM_VideoLocal>();

                    var eps = new List<VM_AnimeEpisode_User>();

                    foreach (var vid in vids)
                    {
                        mv.VideoLocalIDs.Add(vid.VideoLocalID);
                        mv.VideoLocals.Add(vid);
                        // get the episode(s)
                        VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) VM_ShokoServer.Instance.ShokoServices
                            .GetEpisodesForFile(vid.VideoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                            .FirstOrDefault();
                        if (ep != null) eps.Add(ep);
                    }

                    ccDetailMultiple.Content = mv;
                    displayingVidID = vids.First().VideoLocalID;

                    if (eps.GroupBy(a => a?.AnimeSeriesID ?? 0).Distinct().Count(a => a.Key != 0) == 1)
                    {
                        ccSeriesDetail.Content = eps.FirstOrDefault();
                    }
                    else
                    {
                        ccSeriesDetail.Content = null;
                    }

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
                string searchText = txtFileSearch.Text.Trim();
                if (searchText.Length == 0 && searchType != FileSearchCriteria.LastOneHundred)
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.Seach_Criteria, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtFileSearch.Focus();
                    return;
                }
                else if(searchType == FileSearchCriteria.LastOneHundred)
                {
                    searchText = "null";
                }

                FileResults.Clear();
                ViewFiles.Refresh();
                FileCount = 0;

                Cursor = Cursors.Wait;
                EnableDisableControls(false);

                List<VM_VideoLocal> rawVids = VM_ShokoServer.Instance.ShokoServices.SearchForFiles(
                    (int)searchType, searchText, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoLocal>();

                if (searchType == FileSearchCriteria.LastOneHundred)
                {
                    rawVids = rawVids.OrderByDescending(a => a.DateTimeCreated).ToList();
                }
                else
                {
                    rawVids = rawVids.OrderByNatural(a => a.Places.First().FilePath).ToList();
                }

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
                    MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_FileNotFound, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;

                    AskDeleteFile dlg = new AskDeleteFile(string.Format(Shoko.Commons.Properties.Resources.DeleteFile_Title, vid.FileName),
                        string.Format(Shoko.Commons.Properties.Resources.Unrecognized_ConfirmDelete, vid.FileName) + "\r\n\r\n" + Shoko.Commons.Properties.Resources.DeleteFile_Confirm,
                        vid.Places);
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        string tresult = string.Empty;
                        Cursor = Cursors.Wait;
                        foreach (CL_VideoLocal_Place lv in dlg.Selected)
                        {
                            string result =
                                VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(
                                    lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        lbVideos.Items.Remove(e.Parameter);
                    }


                }
                else if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    AskDeleteFile dlg = new AskDeleteFile(Shoko.Commons.Properties.Resources.DeleteFile_Multiple,
                        Shoko.Commons.Properties.Resources.Unrecognized_DeleteSelected + "\r\n\r\n" + Shoko.Commons.Properties.Resources.DeleteFile_Confirm,
                        mv.VideoLocals.SelectMany(a => a.Places).ToList());
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        string tresult = string.Empty;
                        Cursor = Cursors.Wait;
                        foreach (CL_VideoLocal_Place lv in dlg.Selected)
                        {
                            string result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        mv.VideoLocals.ForEach(a => lbVideos.Items.Remove(a));
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
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
                    if (vid.IsLocalFile())
                    {
                        EnableDisableControls(false);

                        VM_ShokoServer.Instance.ShokoServices.RehashFile(vid.VideoLocalID);
                    }
                } else if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    foreach(VM_VideoLocal v in mv.VideoLocals)
                    {
                        if (v.IsLocalFile())
                            VM_ShokoServer.Instance.ShokoServices.RehashFile(v.VideoLocalID);
                    }
                }

                MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_INFO_AddedQueueCmds, Shoko.Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
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
                    if (vid.IsHashed())
                        VM_ShokoServer.Instance.ShokoServices.RescanFile(vid.VideoLocalID);
                } else if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    foreach (VM_VideoLocal v in mv.VideoLocals)
                    {
                        if (v.IsHashed())
                            VM_ShokoServer.Instance.ShokoServices.RescanFile(v.VideoLocalID);
                    }
                }

                MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_INFO_AddedQueueCmds, Shoko.Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }
    }
}
