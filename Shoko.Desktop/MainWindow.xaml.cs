using Infralution.Localization.Wpf;
using Shoko.Desktop.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Shoko.Commons.Extensions;
using Shoko.Desktop.AutoUpdates;
using Shoko.Commons.Downloads;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.UserControls;
using Shoko.Desktop.UserControls.Community;
using Shoko.Desktop.VideoPlayers;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using AnimeSeries = Shoko.Desktop.UserControls.AnimeSeries;
using PlaylistItemType = Shoko.Models.Enums.PlaylistItemType;

namespace Shoko.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly int TAB_MAIN_Dashboard = 0;
        public static readonly int TAB_MAIN_Collection = 1;
        public static readonly int TAB_MAIN_Playlists = 2;
        public static readonly int TAB_MAIN_Bookmarks = 3;
        public static readonly int TAB_MAIN_Server = 4;
        public static readonly int TAB_MAIN_FileManger = 5;
        public static readonly int TAB_MAIN_Settings = 6;
        public static readonly int TAB_MAIN_Pinned = 7;
        public static readonly int TAB_MAIN_Downloads = 8;
        public static readonly int TAB_MAIN_Search = 9;
        public static readonly int TAB_MAIN_Help = 10;
        public static readonly int TAB_MAIN_Community = 11;

        public static int CurrentMainTabIndex = TAB_MAIN_Dashboard;

        private static readonly int TAB_FileManger_Unrecognised = 0;
        private static readonly int TAB_FileManger_Ignored = 1;
        private static readonly int TAB_FileManger_ManuallyLinked = 2;
        private static readonly int TAB_FileManger_DuplicateFiles = 3;
        private static readonly int TAB_FileManger_MultipleFiles = 4;
        private static readonly int TAB_FileManger_MissingMyList = 5;
        private static readonly int TAB_FileManger_SeriesNoFiles = 6;
        private static readonly int TAB_FileManger_MissingEps = 7;
        private static readonly int TAB_FileManger_IgnoredAnime = 8;
        private static readonly int TAB_FileManger_Avdump = 9;
        private static readonly int TAB_FileManger_FileSearch = 10;
        private static readonly int TAB_FileManger_Rename = 11;
        private static readonly int TAB_FileManger_UpdateData = 12;
        private static readonly int TAB_FileManger_Rankings = 13;

        public static readonly int TAB_Settings_Essential = 0;
        public static readonly int TAB_Settings_AniDB = 1;
        public static readonly int TAB_Settings_TvDB = 2;
        public static readonly int TAB_Settings_WebCache = 3;
        public static readonly int TAB_Settings_Display = 4;

        private static System.Timers.Timer postStartTimer = null;

        private int lastFileManagerTab = TAB_FileManger_Unrecognised;

        public static VM_GroupFilter groupFilterVM = null;
        public static List<UserCulture> userLanguages = new List<UserCulture>();
        public static ImageDownloader imageHelper = null;

        private VM_AnimeGroup_User groupBeforeChanges = null;
        private VM_GroupFilter groupFilterBeforeChanges = null;



        BackgroundWorker showChildWrappersWorker = new BackgroundWorker();
        BackgroundWorker refreshGroupsWorker = new BackgroundWorker();
        BackgroundWorker downloadImagesWorker = new BackgroundWorker();
        BackgroundWorker toggleStatusWorker = new BackgroundWorker();
        BackgroundWorker moveSeriesWorker = new BackgroundWorker();

        BackgroundWorker showDashboardWorker = new BackgroundWorker();

        // Locks
        private Object lockDashBoardTab = new Object();
        private Object lockCollectionsTab = new Object();
        private Object lockPlaylistsTab = new Object();
        private Object lockBookmarksTab = new Object();
        private Object lockServerTab = new Object();
        private Object lockUtilitiesTab = new Object();
        private Object lockSettingsTab = new Object();
        private Object lockPinnedTab = new Object();
        private Object lockDownloadsTab = new Object();
        private Object lockSearchTab = new Object();

        public static VideoHandler videoHandler = new VideoHandler();
        private bool _blockTabControlChanged;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            }
            catch (Exception ex)
            {
                File.WriteAllText(@"shoko_error.txt", ex.ToString());
            }

            try
            {
                UnhandledExceptionManager.AddHandler();

                //AppSettings.DebugSettingsToLog();

                if (AppSettings.AutoStartLocalJMMServer)
                    Utils.StartJMMServer();

                lbGroupsSeries.MouseDoubleClick += new MouseButtonEventHandler(lbGroupsSeries_MouseDoubleClick);
                lbGroupsSeries.SelectionChanged += new SelectionChangedEventHandler(lbGroupsSeries_SelectionChanged);
                grdMain.LayoutUpdated += new EventHandler(grdMain_LayoutUpdated);
                LayoutUpdated += new EventHandler(MainWindow_LayoutUpdated);

                lbPlaylists.SelectionChanged += new SelectionChangedEventHandler(lbPlaylists_SelectionChanged);



                showChildWrappersWorker.DoWork += new DoWorkEventHandler(showChildWrappersWorker_DoWork);
                showChildWrappersWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(showChildWrappersWorker_RunWorkerCompleted);

                downloadImagesWorker.DoWork += new DoWorkEventHandler(downloadImagesWorker_DoWork);

                refreshGroupsWorker.DoWork += new DoWorkEventHandler(refreshGroupsWorker_DoWork);
                refreshGroupsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshGroupsWorker_RunWorkerCompleted);

                toggleStatusWorker.DoWork += new DoWorkEventHandler(toggleStatusWorker_DoWork);
                toggleStatusWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(toggleStatusWorker_RunWorkerCompleted);

                moveSeriesWorker.DoWork += new DoWorkEventHandler(moveSeriesWorker_DoWork);
                moveSeriesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(moveSeriesWorker_RunWorkerCompleted);

                txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);

                showDashboardWorker.DoWork += new DoWorkEventHandler(showDashboardWorker_DoWork);
                showDashboardWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(showDashboardWorker_RunWorkerCompleted);
                showDashboardWorker.WorkerSupportsCancellation = true;

                VM_MainListHelper.Instance.ViewGroups.Filter = GroupSearchFilter;
                cboLanguages.SelectionChanged += new SelectionChangedEventHandler(cboLanguages_SelectionChanged);

                if (VM_MainListHelper.Instance.SeriesSearchTextBox == null) VM_MainListHelper.Instance.SeriesSearchTextBox = seriesSearch.txtSeriesSearch;

                //grdSplitEps.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler(grdSplitEps_DragCompleted);


                imageHelper = new ImageDownloader();
                imageHelper.Init();

                videoHandler.Init();
                //videoHandler.HandleFileChange(AppSettings.MPCFolder + "\\mpc-hc.ini");

                InitCulture();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                imageHelper.QueueUpdateEvent += new ImageDownloader.QueueUpdateEventHandler(imageHelper_QueueUpdateEvent);

                cboGroupSort.Items.Clear();
                foreach (string sType in Commons.Extensions.Models.GetAllSortTypes())
                    cboGroupSort.Items.Add(sType);
                cboGroupSort.SelectedIndex = 0;
                btnToolbarSort.Click += new RoutedEventHandler(btnToolbarSort_Click);

                tabControl1.SelectionChanged += new SelectionChangedEventHandler(tabControl1_SelectionChanged);
                tabFileManager.SelectionChanged += new SelectionChangedEventHandler(tabFileManager_SelectionChanged);
                tabSettingsChild.SelectionChanged += new SelectionChangedEventHandler(tabSettingsChild_SelectionChanged);

                Loaded += new RoutedEventHandler(MainWindow_Loaded);
                StateChanged += new EventHandler(MainWindow_StateChanged);

                // Have commented this out because it is no good when Desktop and Server are sharing
                // the same base image path
                //DeleteAvatarImages();

                AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(CloseTab));

                btnUpdateMediaInfo.Click += new RoutedEventHandler(btnUpdateMediaInfo_Click);
                btnFeed.Click += new RoutedEventHandler(btnFeed_Click);
                btnDiscord.Click += new RoutedEventHandler(btnDiscord_Click);
                btnAbout.Click += new RoutedEventHandler(btnAbout_Click);
                btnClearHasherQueue.Click += new RoutedEventHandler(btnClearHasherQueue_Click);
                btnClearGeneralQueue.Click += new RoutedEventHandler(btnClearGeneralQueue_Click);
                btnClearServerImageQueue.Click += new RoutedEventHandler(btnClearServerImageQueue_Click);
                btnAdminMessages.Click += new RoutedEventHandler(btnAdminMessages_Click);

                VM_ShokoServer.Instance.BaseImagePath = Utils.GetBaseImagesPath();

                // timer for automatic updates
                postStartTimer = new System.Timers.Timer();
                postStartTimer.AutoReset = false;
                postStartTimer.Interval = 5 * 1000; // 15 seconds
                postStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(postStartTimer_Elapsed);

                btnSwitchUser.Click += new RoutedEventHandler(btnSwitchUser_Click);

                //videoHandler.HandleFileChange(@"C:\Program Files (x86)\Combined Community Codec Pack\MPC\mpc-hc.ini");

                videoHandler.VideoWatchedEvent += new VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

                if (AppSettings.DashboardType == DashboardType.Normal)
                    dash.Visibility = Visibility.Visible;
                else
                    dashMetro.Visibility = Visibility.Visible;

                VM_UserSettings.Instance.SetDashMetro_Image_Width();
                VM_MainListHelper.Instance.Refreshed += Instance_Refreshed;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        private void CollView_CurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            if (_blockTabControlChanged)
            {
                int previousIndex = tabControl1.Items.IndexOf(tabControl1.SelectedContent);
                tabControl1.SelectedIndex = previousIndex;

                e.Cancel = true;
            }
        }

        private void Instance_Refreshed(object sender, EventArgs e)
        {
            _blockTabControlChanged = true;

            var oldMainIndex = tabControl1.SelectedIndex;

            RefreshPinnedSeries();
            RefreshPlayList();

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
            {
                var curMainIndex = tabControl1.SelectedIndex;

                if (oldMainIndex != curMainIndex)
                    tabControl1.SelectedIndex = oldMainIndex;

                _blockTabControlChanged = false;
            }));
        }

       void btnSwitchUser_Click(object sender, RoutedEventArgs e)
       {
          // authenticate user
          if (VM_ShokoServer.Instance.ServerOnline)
          {
             if (VM_ShokoServer.Instance.AuthenticateUser())
             {
                VM_MainListHelper.Instance.ClearData();
                VM_MainListHelper.Instance.ShowChildWrappers(null);

                RecentAdditionsType addType = RecentAdditionsType.Episode;
                if (dash.cboDashRecentAdditionsType.SelectedIndex == 0) addType = RecentAdditionsType.Episode;
                if (dash.cboDashRecentAdditionsType.SelectedIndex == 1) addType = RecentAdditionsType.Series;

                RefreshOptions opt = new RefreshOptions();
                opt.RecentAdditionType = addType;
                opt.RefreshRecentAdditions = true;
                opt.RefreshContinueWatching = true;
                opt.RefreshOtherWidgets = true;

                // Check if worker is busy and cancel if needed
                if (showDashboardWorker.IsBusy)
                   showDashboardWorker.CancelAsync();

                if (!showDashboardWorker.IsBusy)
                  showDashboardWorker.RunWorkerAsync(opt);
               else
                  logger.Error("Failed to start showDashboardWorker for btnSwitchUser");

                tabControl1.SelectedIndex = TAB_MAIN_Dashboard;
             }
          }
       }

       void videoHandler_VideoWatchedEvent(VideoWatchedEventArgs ev)
        {
            if (tabControl1.SelectedIndex == TAB_MAIN_Collection || tabControl1.SelectedIndex == TAB_MAIN_Pinned)
            {


                //RefreshView();
            }
        }

        void btnClearServerImageQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.ClearImagesQueue();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            Cursor = Cursors.Arrow;
        }

        void btnClearGeneralQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.ClearGeneralQueue();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            Cursor = Cursors.Arrow;
        }

        void btnClearHasherQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.ClearHasherQueue();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            Cursor = Cursors.Arrow;
        }

        void btnFeed_Click(object sender, RoutedEventArgs e)
        {
            FeedForm frm = new FeedForm();
            frm.Owner = this;
            frm.ShowDialog();
        }

        void btnDiscord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://discord.gg/0XKJW7TObKLajoKc");
            }
            catch { }
        }

        void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutForm frm = new AboutForm();
            frm.Owner = this;
            frm.ShowDialog();
        }

        void btnAdminMessages_Click(object sender, RoutedEventArgs e)
        {
            AdminMessagesForm frm = new AdminMessagesForm();
            frm.Owner = this;
            frm.ShowDialog();
        }

        void btnUpdateMediaInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.RefreshAllMediaInfo();
                MessageBox.Show(Shoko.Commons.Properties.Resources.Main_ProcessRunning, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void DeleteAvatarImages()
        {
            try
            {
                string path = Utils.GetTraktImagePath_Avatars();
                if (!Directory.Exists(path)) return;

                string[] imageFiles = Directory.GetFiles(path, "*.jpg");
                foreach (string filename in imageFiles)
                    File.Delete(filename);
            }
            catch { }
        }

        private void CloseTab(object source, RoutedEventArgs args)
        {
            TabItem tabItem = args.Source as TabItem;
            if (tabItem != null)
            {
                TabControl tabControl = tabItem.Parent as TabControl;
                if (tabControl != null)
                    tabControl.Items.Remove(tabItem);
            }
        }


        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            //if (this.WindowState == System.Windows.WindowState.Minimized) this.Hide();

            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
                AppSettings.DefaultWindowState = WindowState;
        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.WindowState = AppSettings.DefaultWindowState;

            if (AppSettings.WindowFullScreen)
                SetWindowFullscreen();
            else
                SetWindowNormal();

            Utils.ClearAutoUpdateCache();

            // validate settings
            VM_ShokoServer.Instance.Test();

            bool loggedIn = false;
            if (VM_ShokoServer.Instance.ServerOnline)
                loggedIn = VM_ShokoServer.Instance.LoginAsLastUser();

            if (!loggedIn)
            {
                // authenticate user
                if (VM_ShokoServer.Instance.ServerOnline && !VM_ShokoServer.Instance.AuthenticateUser())
                {
                    Close();
                    return;
                }
            }

            if (VM_ShokoServer.Instance.ServerOnline)
            {
                tabControl1.SelectedIndex = TAB_MAIN_Dashboard;
                DisplayMainTab(TAB_MAIN_Dashboard);
                DownloadAllImages();
            }
            else
                tabControl1.SelectedIndex = TAB_MAIN_Settings;


            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            if (a != null)
            {
                VM_ShokoServer.Instance.ApplicationVersion = Utils.GetApplicationVersion(a);
            }

            VM_UTorrentHelper.Instance.ValidateCredentials();

            postStartTimer.Start();

            CheckForUpdatesNew(false);

            var collView = CollectionViewSource.GetDefaultView(tabControl1.Items);
            collView.CurrentChanging += CollView_CurrentChanging;
        }

        void postStartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            postStartTimer.Stop();

            VM_UTorrentHelper.Instance.Init();

            if (VM_ShokoServer.Instance.ServerOnline)
                DownloadAllImages();
        }

        public void CheckForUpdatesNew(bool forceShowForm)
        {
            try
            {
                long verCurrent = 0;
                long verNew = 0;

                // get the user's version
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                if (a == null)
                {
                    logger.Error("Could not get current version");
                    return;
                }
                System.Reflection.AssemblyName an = a.GetName();

                verNew =
                        ShokoAutoUpdatesHelper.ConvertToAbsoluteVersion(
                            ShokoAutoUpdatesHelper.GetLatestVersionNumber(AppSettings.UpdateChannel));

                //verNew = verInfo.versions.DesktopVersionAbs;
                verCurrent = (an.Version.Revision * 100) +
                    (an.Version.Build * 100 * 100) +
                    (an.Version.Minor * 100 * 100 * 100) +
                    (an.Version.Major * 100 * 100 * 100 * 100);

                if (forceShowForm || verNew > verCurrent)
                {
                    UpdateForm frm = new UpdateForm();
                    frm.Owner = this;
                    frm.ShowDialog();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }

        }


        void MainWindow_LayoutUpdated(object sender, EventArgs e)
        {
            // Why am I doing this?
            // Basically there is weird problem if you try and set the content control's width to the exact
            // ViewportWidth of the parent scroller.
            // On some resolutions, when you maximise the window it will cause UI glitches
            // By setting it slightly less than the max width, these problems go away
            try
            {
                //Debug.Print("Scroller width = {0}", Scroller.ActualWidth);
                //Debug.Print("Scroller ViewportWidth = {0}", Scroller.ViewportWidth);

                double tempWidth = ccDetail.ActualWidth - 8;
                double tempHeight = ccDetail.ActualHeight - 8;
                if (tempWidth > 0)
                {
                    VM_MainListHelper.Instance.MainScrollerWidth = tempWidth;
                }

                tempWidth = tabControl1.ActualWidth - 20;
                //tempWidth = tabControl1.ActualWidth - 300;
                if (tempWidth > 0)
                    VM_MainListHelper.Instance.FullScrollerWidth = tempWidth;

                tempHeight = tabControl1.ActualHeight - 50;
                if (tempHeight > 0)
                    VM_MainListHelper.Instance.FullScrollerHeight = tempHeight;

                tempWidth = ccPlaylist.ActualWidth - 8;
                if (tempWidth > 0)
                    VM_MainListHelper.Instance.PlaylistWidth = tempWidth;

                tempWidth = tabcDownloads.ActualWidth - 130;
                if (tempWidth > 0)
                    VM_MainListHelper.Instance.DownloadRecScrollerWidth = tempWidth;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        void tabFileManager_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.Source is TabControl)
                {
                    TabControl tab = e.Source as TabControl;
                    Cursor = Cursors.Wait;
                    if (tab.SelectedIndex == TAB_FileManger_Unrecognised)
                    {
                        if (unRecVids.UnrecognisedFiles.Count == 0) unRecVids.RefreshUnrecognisedFiles();
                        if (unRecVids.AllSeries.Count == 0) unRecVids.RefreshSeries();
                        lastFileManagerTab = TAB_FileManger_Unrecognised;
                    }

                    if (tab.SelectedIndex == TAB_FileManger_Ignored)
                    {
                        if (ignoredFiles.IgnoredFilesCollection.Count == 0) ignoredFiles.RefreshIgnoredFiles();
                        lastFileManagerTab = TAB_FileManger_Ignored;
                    }

                    if (tab.SelectedIndex == TAB_FileManger_ManuallyLinked)
                    {
                        if (linkedFiles.ManuallyLinkedFiles.Count == 0) linkedFiles.RefreshLinkedFiles();
                        lastFileManagerTab = TAB_FileManger_ManuallyLinked;
                    }

                    if (tab.SelectedIndex == TAB_FileManger_DuplicateFiles)
                    {
                        if (duplicateFiles.DuplicateFilesCollection.Count == 0) duplicateFiles.RefreshDuplicateFiles();
                        lastFileManagerTab = TAB_FileManger_DuplicateFiles;
                    }
                    if (tab.SelectedIndex == TAB_FileManger_MultipleFiles)
                    {
                        //if (multipleFiles.CurrentEpisodes.Count == 0) multipleFiles.RefreshMultipleFiles();
                        lastFileManagerTab = TAB_FileManger_MultipleFiles;
                    }

                    if (tab.SelectedIndex == TAB_FileManger_Rename)
                    {
                        if (fileRenaming.RenameScripts.Count == 0) fileRenaming.RefreshScripts();
                        lastFileManagerTab = TAB_FileManger_Rename;
                    }

                    if (tab.SelectedIndex == TAB_FileManger_Rankings)
                    {
                        if (rankings.AllAnime.Count == 0) rankings.Init();
                        lastFileManagerTab = TAB_FileManger_Rankings;
                    }
                    Cursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        void showDashboardWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshOptions opt = e.Argument as RefreshOptions;
            
            VM_Dashboard.Instance.RefreshData(opt.RefreshContinueWatching, opt.RefreshRecentAdditions,
                opt.RefreshOtherWidgets, opt.RecentAdditionType);
        }

        void showDashboardWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Arrow;
            tabControl1.IsEnabled = true;
        }

        private void DisplayMainTab(int tabIndex)
        {
            try
            {
                CurrentMainTabIndex = tabIndex;

                if (tabIndex == TAB_MAIN_Dashboard)
                {
                    lock (lockDashBoardTab)
                    {
                        if (dash.Visibility == Visibility.Visible)
                        {
                            if (VM_Dashboard.Instance.EpsWatchNext_Recent.Count == 0 &&
                                VM_Dashboard.Instance.SeriesMissingEps.Count == 0
                                && VM_Dashboard.Instance.MiniCalendar.Count == 0 &&
                                VM_Dashboard.Instance.RecommendationsWatch.Count == 0
                                && VM_Dashboard.Instance.RecommendationsDownload.Count == 0)
                            {
                                tabControl1.IsEnabled = false;
                                Cursor = Cursors.Wait;

                                RecentAdditionsType addType = RecentAdditionsType.Episode;
                                if (dash.cboDashRecentAdditionsType.SelectedIndex == 0)
                                    addType = RecentAdditionsType.Episode;
                                if (dash.cboDashRecentAdditionsType.SelectedIndex == 1)
                                    addType = RecentAdditionsType.Series;

                                RefreshOptions opt = new RefreshOptions();
                                opt.RecentAdditionType = addType;
                                opt.RefreshRecentAdditions = true;
                                opt.RefreshContinueWatching = true;
                                opt.RefreshOtherWidgets = true;

                                // Check if worker is busy and cancel if needed
                                if (showDashboardWorker.IsBusy)
                                    showDashboardWorker.CancelAsync();

                                if (!showDashboardWorker.IsBusy)
                                    showDashboardWorker.RunWorkerAsync(opt);
                                else
                                    logger.Error("Failed to start showDashboardWorker for TAB_MAIN_Dashboard");
                            }
                        }
                        else
                        {
                            if (VM_DashboardMetro.Instance.ContinueWatching.Count == 0)
                                dashMetro.RefreshAllData();
                        }

                        if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                    }
                }

                if (tabIndex == TAB_MAIN_Collection)
                {
                    lock (lockCollectionsTab)
                    {
                        if (VM_MainListHelper.Instance.AllGroupsDictionary.Count == 0)
                        {
                            VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                        }

                        if (VM_MainListHelper.Instance.CurrentWrapper == null && lbGroupsSeries.Items.Count == 0)
                        {
                            VM_MainListHelper.Instance.SearchTextBox = txtGroupSearch;
                            VM_MainListHelper.Instance.CurrentGroupFilter = VM_MainListHelper.Instance.AllGroupFilter;
                            VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
                            lbGroupsSeries.SelectedIndex = 0;
                        }
                    }
                    if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                }


                if (tabIndex == TAB_MAIN_FileManger)
                {
                    if (unRecVids.UnrecognisedFiles.Count == 0) unRecVids.RefreshUnrecognisedFiles();
                    if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                }

                if (tabIndex == TAB_MAIN_Playlists)
                {
                    lock (lockPlaylistsTab)
                    {
                        if (VM_PlaylistHelper.Instance.Playlists == null ||
                            VM_PlaylistHelper.Instance.Playlists.Count == 0) VM_PlaylistHelper.Instance.RefreshData();
                        if (lbPlaylists.Items.Count > 0 && lbPlaylists.SelectedIndex < 0)
                            lbPlaylists.SelectedIndex = 0;
                    }

                }

                if (tabIndex == TAB_MAIN_Bookmarks)
                {
                    lock (lockBookmarksTab)
                    {
                        if (VM_MainListHelper.Instance.BookmarkedAnime == null ||
                            VM_MainListHelper.Instance.BookmarkedAnime.Count == 0)
                            VM_MainListHelper.Instance.RefreshBookmarkedAnime();

                        if (ucBookmarks.lbBookmarks.Items.Count > 0)
                            ucBookmarks.lbBookmarks.SelectedIndex = 0;
                    }

                }

                if (tabIndex == TAB_MAIN_Search)
                {
                    lock (lockSearchTab)
                    {
                        if (VM_MainListHelper.Instance.AllSeriesDictionary == null ||
                            VM_MainListHelper.Instance.AllSeriesDictionary.Count == 0)
                            VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                    }
                }

                if (tabIndex == TAB_MAIN_Server)
                {
                    lock (lockServerTab)
                    {
                        if (VM_ShokoServer.Instance.FolderProviders.Count == 0)
                            VM_ShokoServer.Instance.RefreshCloudAccounts();
                        if (VM_ShokoServer.Instance.ImportFolders.Count == 0) VM_ShokoServer.Instance.RefreshImportFolders();
                    }
                }

                if (tabIndex == TAB_MAIN_Settings)
                {
                    lock (lockSettingsTab)
                    {
                        if (VM_ShokoServer.Instance.FolderProviders.Count == 0)
                            VM_ShokoServer.Instance.RefreshCloudAccounts();
                        if (VM_ShokoServer.Instance.ImportFolders.Count == 0) VM_ShokoServer.Instance.RefreshImportFolders();
                        if (VM_ShokoServer.Instance.SelectedLanguages.Count == 0)
                            VM_ShokoServer.Instance.RefreshNamingLanguages();
                        if (VM_ShokoServer.Instance.AllUsers.Count == 0) VM_ShokoServer.Instance.RefreshAllUsers();
                        if (VM_ShokoServer.Instance.AllTags.Count == 0) VM_ShokoServer.Instance.RefreshAllTags();
                        if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                    }
                }

                if (tabIndex == TAB_MAIN_Pinned)
                {
                    lock (lockPinnedTab)
                    {
                        if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                    }
                }

                if (tabIndex == TAB_MAIN_Downloads)
                {
                    lock (lockDownloadsTab)
                    {
                        if (VM_UserSettings.Instance.SelectedTorrentSources.Count == 0 ||
                            VM_UserSettings.Instance.UnselectedTorrentSources.Count == 0)
                            VM_UserSettings.Instance.RefreshTorrentSources();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                tabControl1.IsEnabled = true;
            }
        }

        void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetColours();

                //if (!this.IsLoaded || !VM_ShokoServer.Instance.UserAuthenticated) return;
                if (!VM_ShokoServer.Instance.UserAuthenticated) return;


                TabControl tab = e.Source as TabControl;
                if (tab == null) return;

                if (!tab.Name.Equals("tabControl1", StringComparison.InvariantCultureIgnoreCase)) return;

                DisplayMainTab(tabControl1.SelectedIndex);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                tabControl1.IsEnabled = true;
            }
        }

        void tabSettingsChild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.Source is TabControl)
                {
                    TabControl tab = e.Source as TabControl;
                    if (tab.SelectedIndex == TAB_Settings_Display)
                    {
                        if (VM_ShokoServer.Instance.SelectedLanguages.Count == 0) VM_ShokoServer.Instance.RefreshNamingLanguages();
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnToolbarSort_Click(object sender, RoutedEventArgs e)
        {
            VM_MainListHelper.Instance.ViewGroups.SortDescriptions.Clear();
            GroupFilterSorting sortType = Commons.Extensions.Models.GetEnumForText_Sorting(cboGroupSort.SelectedItem.ToString());
            VM_MainListHelper.Instance.ViewGroups.SortDescriptions.Add(sortType.GetSortDescription(GroupFilterSortDirection.Asc));
        }



        void imageHelper_QueueUpdateEvent(QueueUpdateEventArgs ev)
        {
            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                {
                    tbImageDownloadQueueStatus.Text = ev.queueCount.ToString();
                }));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        void cboLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCulture();
        }

        private void InitCulture()
        {
            try
            {
                string currentCulture = AppSettings.Culture;

                cboLanguages.ItemsSource = UserCulture.SupportedLanguages;

                for (int i = 0; i < cboLanguages.Items.Count; i++)
                {
                    UserCulture ul = cboLanguages.Items[i] as UserCulture;
                    if (ul.Culture.Trim().ToUpper() == currentCulture.Trim().ToUpper())
                    {
                        cboLanguages.SelectedIndex = i;
                        break;
                    }

                }
                if (cboLanguages.SelectedIndex < 0)
                    cboLanguages.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void SetCulture()
        {
            if (cboLanguages.SelectedItem == null) return;
            UserCulture ul = cboLanguages.SelectedItem as UserCulture;
            bool isLanguageChanged = AppSettings.Culture != ul.Culture;
            System.Windows.Forms.DialogResult result;

            try
            {
                CultureInfo ci = new CultureInfo(ul.Culture);
                CultureInfo.DefaultThreadCurrentUICulture = ci;
                CultureManager.UICulture = ci;
                AppSettings.Culture = ul.Culture;
                ConfigurationManager.RefreshSection("appSettings");

                if (isLanguageChanged)
                {
                    result = System.Windows.Forms.MessageBox.Show(Shoko.Commons.Properties.Resources.Language_Info, Shoko.Commons.Properties.Resources.Language_Switch, System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        System.Windows.Forms.Application.Restart();
                        Application.Current.Shutdown();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SetDetailBinding(null);

                // move to all groups
                if (VM_MainListHelper.Instance.CurrentWrapper == null)
                    VM_MainListHelper.Instance.ShowAllGroups();


                VM_MainListHelper.Instance.ViewGroups.Refresh();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private bool GroupSearchFilter(object obj)
        {
            IListWrapper grp = obj as IListWrapper;
            if (grp == null) return true;

            if (obj.GetType() != typeof(VM_AnimeGroup_User) && obj.GetType() != typeof(VM_AnimeSeries_User))
                return true;

            // first evaluate the group filter
            // if the group doesn't match the group filter we won't continue
            if (obj.GetType() == typeof(VM_AnimeGroup_User))
            {
                VM_AnimeGroup_User grpvm = obj as VM_AnimeGroup_User;
                //if (!GroupSearchFilterHelper.EvaluateGroupFilter(VM_MainListHelper.Instance.CurrentGroupFilter, grpvm)) return false;

                return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
            }

            if (obj.GetType() == typeof(VM_AnimeSeries_User))
            {
                VM_AnimeSeries_User ser = obj as VM_AnimeSeries_User;
                //if (!GroupSearchFilterHelper.EvaluateGroupFilter(VM_MainListHelper.Instance.CurrentGroupFilter, ser)) return false;

                return GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, txtGroupSearch.Text);
            }

            return true;
        }

        void refreshGroupsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            showChildWrappersWorker.RunWorkerAsync(VM_MainListHelper.Instance.CurrentWrapper);
        }

        void refreshGroupsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                DownloadAllImages();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void showChildWrappersWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                EnableDisableGroupControls(true);
                if (lbGroupsSeries.Items.Count > 0)
                {
                    HighlightMainListItem();
                }
                else
                    SetDetailBinding(null);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void showChildWrappersWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VM_MainListHelper.Instance.ShowChildWrappers(e.Argument as IListWrapper);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void toggleStatusWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableDisableGroupControls(true);
            Cursor = Cursors.Arrow;
        }


        void toggleStatusWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object obj = e.Argument;
                VM_AnimeSeries_User ser = null;
                bool newStatus = false;

                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    newStatus = !vid.Watched;
                    VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_MainListHelper.Instance.UpdateHeirarchy(vid);

                    ser = VM_MainListHelper.Instance.GetSeriesForVideo(vid.VideoLocalID);
                }

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    newStatus = !ep.Watched;
                    CL_Response<CL_AnimeEpisode_User> response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    ser = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                if (newStatus == true && ser != null)
                {
                    Utils.PromptToRateSeries(ser, this);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void moveSeriesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableDisableGroupControls(true);
            SetDetailBinding(null);
            VM_MainListHelper.Instance.RefreshGroupsSeriesData();
            VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
            Cursor = Cursors.Arrow;

            VM_MainListHelper.Instance.ViewGroups.Refresh();
            showChildWrappersWorker.RunWorkerAsync(VM_MainListHelper.Instance.CurrentWrapper);
        }

        void moveSeriesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object obj = e.Argument;
                if (obj.GetType() != typeof(VM_MoveSeriesDetails)) return;

                VM_MoveSeriesDetails request = obj as VM_MoveSeriesDetails;
                DateTime start = DateTime.Now;

                //request.UpdatedSeries.Save();
                CL_Response<CL_AnimeSeries_User> response =
                    VM_ShokoServer.Instance.ShokoServices.MoveSeries(request.UpdatedSeries.AnimeSeriesID, request.UpdatedSeries.AnimeGroupID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal, (Action)
                            delegate()
                            {
                                Cursor = Cursors.Arrow;
                                MessageBox.Show(response.ErrorMessage);
                            });
                    return;
                }
                else
                {
                    request.UpdatedSeries.Populate((VM_AnimeSeries_User)response.Result);
                }




                // update all the attached groups

                /*Dictionary<int, JMMServerBinary.Contract_AnimeGroup> grpsDict = new Dictionary<int, JMMServerBinary.Contract_AnimeGroup>();
				List<JMMServerBinary.Contract_AnimeGroup> grps = VM_ShokoServer.Instance.clientBinaryHTTP.GetAllGroupsAboveGroupInclusive(request.UpdatedSeries.AnimeGroupID,
					VM_ShokoServer.Instance.CurrentUser.JMMUserID.Value);
				List<JMMServerBinary.Contract_AnimeGroup> grpsOld = VM_ShokoServer.Instance.clientBinaryHTTP.GetAllGroupsAboveGroupInclusive(request.OldAnimeGroupID,
					VM_ShokoServer.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_AnimeGroup tempGrp in grps)
					grpsDict[tempGrp.AnimeGroupID] = tempGrp;

				foreach (JMMServerBinary.Contract_AnimeGroup tempGrp in grpsOld)
					grpsDict[tempGrp.AnimeGroupID] = tempGrp;
				
				foreach (VM_AnimeGroup_User grp in VM_MainListHelper.Instance.AllGroups)
				{
					if (grpsDict.ContainsKey(grp.AnimeGroupID.Value))
					{
						grp.Populate(grpsDict[grp.AnimeGroupID.Value]);
					}

				}
				TimeSpan ts = DateTime.Now - start;
				Console.Write(ts.TotalMilliseconds);*/



            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void DownloadAllImages()
        {
            //if (!downloadImagesWorker.IsBusy)
            //	downloadImagesWorker.RunWorkerAsync();
        }

        void downloadImagesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // 1. Download posters from AniDB
            List<VM_AniDB_Anime> contracts = VM_ShokoServer.Instance.ShokoServices.GetAllAnime().CastList<VM_AniDB_Anime>();
            
            int i = 0;
            foreach (VM_AniDB_Anime anime in contracts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadAniDBCover(anime, false);
                i++;

                //if (i == 80) break;
            }

            // 2. Download posters from TvDB
            List<VM_TvDB_ImagePoster> posters = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBPosters(null).CastList<VM_TvDB_ImagePoster>();
            foreach (VM_TvDB_ImagePoster poster in posters)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBPoster(poster, false);
            }

            // 2a. Download posters from MovieDB
            List<VM_MovieDB_Poster> moviePosters = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBPosters(null).CastList<VM_MovieDB_Poster>();
            foreach (VM_MovieDB_Poster poster in moviePosters)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBPoster(poster, false);
            }

            // 3. Download wide banners from TvDB
            List<VM_TvDB_ImageWideBanner> banners = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBWideBanners(null).CastList<VM_TvDB_ImageWideBanner>();
            foreach (VM_TvDB_ImageWideBanner banner in banners)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBWideBanner(banner, false);
            }

            // 4. Download fanart from TvDB
            List<VM_TvDB_ImageFanart> fanarts = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBFanart(null).CastList<VM_TvDB_ImageFanart>();
            foreach (VM_TvDB_ImageFanart fanart in fanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBFanart(fanart, false);
            }

            // 4a. Download fanart from MovieDB
            List<VM_MovieDB_Fanart> movieFanarts = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBFanart(null).CastList<VM_MovieDB_Fanart>();
            foreach (VM_MovieDB_Fanart fanart in movieFanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBFanart(fanart, false);
            }

            // 5. Download episode images from TvDB
            List<VM_TvDB_Episode> eps = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBEpisodes(null).CastList<VM_TvDB_Episode>();
            foreach (VM_TvDB_Episode episode in eps)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBEpisode(episode, false);
            }

            // 6. Download posters from Trakt
            List<VM_Trakt_ImagePoster> traktPosters = VM_ShokoServer.Instance.ShokoServices.GetAllTraktPosters(null).CastList<VM_Trakt_ImagePoster>();
            foreach (VM_Trakt_ImagePoster traktposter in traktPosters)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktposter.ImageURL)) continue;
                imageHelper.DownloadTraktPoster(traktposter, false);
            }

            // 7. Download fanart from Trakt
            List<VM_Trakt_ImageFanart> traktFanarts = VM_ShokoServer.Instance.ShokoServices.GetAllTraktFanart(null).CastList<VM_Trakt_ImageFanart>();
            foreach (VM_Trakt_ImageFanart traktFanart in traktFanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktFanart.ImageURL)) continue;
                imageHelper.DownloadTraktFanart(traktFanart, false);
            }

            // 8. Download episode images from Trakt
            List<VM_Trakt_Episode> traktEpisodes = VM_ShokoServer.Instance.ShokoServices.GetAllTraktEpisodes(null).CastList<VM_Trakt_Episode>();
            foreach (VM_Trakt_Episode traktEp in traktEpisodes)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktEp.EpisodeImage)) continue;

                // special case for trak episodes
                // Trakt will return the fanart image when no episode image exists, but we don't want this
                int pos = traktEp.EpisodeImage.IndexOf(@"episodes/");
                if (pos <= 0) continue;

                //logger.Trace("Episode image: {0} - {1}/{2}", traktEp.Trakt_ShowID, traktEp.Season, traktEp.EpisodeNumber);

                imageHelper.DownloadTraktEpisode(traktEp, false);
            }
        }

        private void RefreshView()
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            EnableDisableGroupControls(false);

            try
            {
                Cursor = Cursors.Wait;

                // we are look at all the group filters
                if (VM_MainListHelper.Instance.CurrentWrapper == null)
                {
                    VM_MainListHelper.Instance.SearchTextBox = txtGroupSearch;
                    VM_MainListHelper.Instance.CurrentGroupFilter = VM_MainListHelper.Instance.AllGroupFilter;

                    //refreshGroupsWorker.RunWorkerAsync(null);

                    VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                    DownloadAllImages();

                    VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)HighlightMainListItem);
                }

                // we are inside one of the group filters, groups or series
                if (VM_MainListHelper.Instance.CurrentWrapper != null)
                {
                    // refresh the groups and series data
                    refreshGroupsWorker.RunWorkerAsync(null);

                    // refresh the episodes
                    if (lbGroupsSeries.SelectedItem is VM_AnimeSeries_User)
                    {
                        VM_AnimeSeries_User ser = lbGroupsSeries.SelectedItem as VM_AnimeSeries_User;
                        ser.RefreshEpisodes();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                EnableDisableGroupControls(true);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                EnableDisableGroupControls(true);
            }
        }



        #region Command Bindings

        private void CommandBinding_EditTraktCredentials(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl1.SelectedIndex = TAB_MAIN_Settings;
            tabSettingsChild.SelectedIndex = TAB_Settings_TvDB;
        }

        public void ShowPinnedFileAvDump(VM_VideoLocal vid)
        {
            try
            {
                foreach (VM_AVDump dumpTemp in VM_MainListHelper.Instance.AVDumpFiles)
                {
                    if (dumpTemp.FullPath == vid.GetLocalFileSystemFullPath()) return;
                }

                VM_AVDump dump = new VM_AVDump(vid);
                VM_MainListHelper.Instance.AVDumpFiles.Add(dump);

                tabControl1.SelectedIndex = TAB_MAIN_FileManger;
                tabFileManager.SelectedIndex = TAB_FileManger_Avdump;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowPinnedSeriesOld(VM_AnimeSeries_User series)
        {
            Cursor = Cursors.Wait;

            CloseableTabItem cti = new CloseableTabItem();
            //TabItem cti = new TabItem();

            // if the pinned tab already has this, don't open it again.
            int curTab = -1;
            foreach (object obj in tabPinned.Items)
            {
                curTab++;
                CloseableTabItem ctiTemp = obj as CloseableTabItem;
                if (ctiTemp == null) continue;

                AnimeSeries ctrl = ctiTemp.Content as AnimeSeries;
                if (ctrl == null) continue;

                VM_AnimeSeries_User ser = ctrl.DataContext as VM_AnimeSeries_User;
                if (ser == null) continue;

                if (ser.AnimeSeriesID == series.AnimeSeriesID)
                {
                    tabControl1.SelectedIndex = TAB_MAIN_Pinned;
                    tabPinned.SelectedIndex = curTab;
                    Cursor = Cursors.Arrow;
                    return;
                }
            }

            string tabHeader = series.SeriesName;
            if (tabHeader.Length > 30)
                tabHeader = tabHeader.Substring(0, 30) + "...";
            cti.Header = tabHeader;

            //AnimeSeries_Hulu seriesControl = new AnimeSeries_Hulu();
            AnimeSeries seriesControl = new AnimeSeries();
            seriesControl.DataContext = series;
            cti.Content = seriesControl;

            tabPinned.Items.Add(cti);

            tabControl1.SelectedIndex = TAB_MAIN_Pinned;
            tabPinned.SelectedIndex = tabPinned.Items.Count - 1;

            Cursor = Cursors.Arrow;
        }

        public void ShowPinnedSeries(VM_AnimeSeries_User series, bool isMetroDash = false)
        {
            Cursor = Cursors.Wait;

            CloseableTabItem cti = new CloseableTabItem();
            //TabItem cti = new TabItem();

            // if the pinned tab already has this, don't open it again.
            int curTab = -1;
            foreach (object obj in tabPinned.Items)
            {
                curTab++;
                CloseableTabItem ctiTemp = obj as CloseableTabItem;
                if (ctiTemp == null) continue;

                VM_AnimeSeries_User ser = null;
                ContentControl ctrl = ctiTemp.Content as AnimeSeriesContainerControl;
                if (ctrl == null)
                {
                    ContentControl subControl = ctrl.Content as AnimeSeriesSimplifiedControl;
                    if (subControl == null)
                        subControl = ctrl.Content as AnimeSeries;

                    if (subControl != null)
                        ctrl = subControl;
                }
                else
                {
                    ContentControl subControl = ctrl.DataContext as AnimeSeriesSimplifiedControl;
                    if (subControl == null)
                        subControl = ctrl.DataContext as AnimeSeries;

                    if (subControl != null)
                        ctrl = subControl;
                }

                if (ctrl == null)
                    continue;

                ser = ctrl.DataContext as VM_AnimeSeries_User;
                if (ser == null) continue;

                if (ser.AnimeSeriesID == series.AnimeSeriesID)
                {
                    tabControl1.SelectedIndex = TAB_MAIN_Pinned;
                    tabPinned.SelectedIndex = curTab;
                    Cursor = Cursors.Arrow;
                    return;
                }
            }

            string tabHeader = series.SeriesName;
            if (tabHeader.Length > 30)
                tabHeader = tabHeader.Substring(0, 30) + "...";
            cti.Header = tabHeader;

            if (AppSettings.DisplaySeriesSimple)
            {
                AnimeSeriesSimplifiedControl ctrl = new AnimeSeriesSimplifiedControl();
                ctrl.DataContext = series;

                AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl();
                cont.IsMetroDash = false;
                cont.DataContext = ctrl;

                cti.Content = cont;

                tabPinned.Items.Add(cti);
            }
            else
            {
                AnimeSeries seriesControl = new AnimeSeries();
                seriesControl.DataContext = series;

                AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl();
                cont.IsMetroDash = false;
                cont.DataContext = seriesControl;

                cti.Content = cont;

                tabPinned.Items.Add(cti);
            }

            tabControl1.SelectedIndex = TAB_MAIN_Pinned;
            tabPinned.SelectedIndex = tabPinned.Items.Count - 1;

            Cursor = Cursors.Arrow;
        }


        public void RefreshPinnedSeries()
        {
            Cursor = Cursors.Wait;

            foreach (object obj in tabPinned.Items)
            {
                CloseableTabItem ctiTemp = obj as CloseableTabItem;
                if (ctiTemp == null) continue;

                VM_AnimeSeries_User ser = null;
                ContentControl ctrl = ctiTemp.Content as AnimeSeriesContainerControl;
                if (ctrl == null)
                {
                    ContentControl subControl = ctrl.Content as AnimeSeriesSimplifiedControl;
                    if (subControl == null)
                        subControl = ctrl.Content as AnimeSeries;

                    if (subControl != null)
                        ctrl = subControl;
                }
                else
                {
                    ContentControl subControl = ctrl.DataContext as AnimeSeriesSimplifiedControl;
                    if (subControl == null)
                        subControl = ctrl.DataContext as AnimeSeries;

                    if (subControl != null)
                        ctrl = subControl;
                }

                if (ctrl == null)
                    continue;

                ser = ctrl.DataContext as VM_AnimeSeries_User;
                if (ser == null) continue;

                if (ser.AnimeSeriesID!=0)
                    if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(ser.AnimeSeriesID, out ser))
                        ctrl.DataContext = ser;
            }

            Cursor = Cursors.Arrow;
        }
        public void RefreshPlayList()
        {
            Cursor = Cursors.Wait;

            foreach (object obj in lbPlaylists.Items)
            {
                var playListVM = obj as VM_Playlist;
                if (playListVM == null)
                    continue;

                var playListObjects = playListVM.PlaylistObjects;
                if (playListObjects != null)
                {
                    foreach (var item in playListObjects)
                    {
                        if (item.ItemType == PlaylistItemType.AnimeSeries)
                        {
                            var itemSer = item.Series;
                            if (itemSer == null) continue;
                            if (itemSer.AnimeSeriesID==0) continue;

                            var itemSeriesId = itemSer.AnimeSeriesID;
                            if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(itemSeriesId, out itemSer))
                                item.Series = itemSer;
                        }
                    }
                }

                var ser = playListVM.Series;
                if (ser == null) continue;
                if (playListVM.Series.AnimeSeriesID==0) continue;

                var seriesId = playListVM.Series.AnimeSeriesID;
                if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(seriesId, out ser))
                    playListVM.Series = ser;

                //if (playListVM.ite)
            }

            Cursor = Cursors.Arrow;
        }

        private void SetColours()
        {
            if (tabControl1.SelectedIndex == TAB_MAIN_Dashboard)
            {
                if (dash.Visibility == Visibility.Visible)
                {
                    tabControl1.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    var bc = new BrushConverter();
                    tabControl1.Background = (Brush)bc.ConvertFrom("#F1F1F1");
                    //tabControl1.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
            else
                tabControl1.Background = new SolidColorBrush(Colors.Transparent);
        }

        public void ShowDashMetroView(MetroViews viewType, object data)
        {
            tileContinueWatching.Visibility = Visibility.Collapsed;
            dash.Visibility = Visibility.Collapsed;
            dashMetro.Visibility = Visibility.Collapsed;

            switch (viewType)
            {
                case MetroViews.MainNormal:
                    dash.Visibility = Visibility.Visible;
                    DisplayMainTab(TAB_MAIN_Dashboard);
                    AppSettings.DashboardType = DashboardType.Normal;
                    break;
                case MetroViews.MainMetro:
                    dashMetro.Visibility = Visibility.Visible;
                    DisplayMainTab(TAB_MAIN_Dashboard);
                    AppSettings.DashboardType = DashboardType.Metro;
                    break;
                case MetroViews.ContinueWatching:
                    tileContinueWatching.Visibility = Visibility.Visible;
                    tileContinueWatching.DataContext = data;
                    break;
            }

            SetColours();
        }

        public void ShowDashMetroView(MetroViews viewType)
        {
            ShowDashMetroView(viewType, null);
        }

        private void CommandBinding_CreateSeriesFromAnime(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AniDB_Anime) || obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                {
                    VM_AniDB_Anime anime = null;

                    if (obj.GetType() == typeof(VM_AniDB_Anime))
                        anime = (VM_AniDB_Anime)obj;

                    if (obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                        anime = ((VM_AniDB_Anime_Similar)obj).AniDB_Anime;

                    // check if a series already exists
                    bool seriesExists = VM_ShokoServer.Instance.ShokoServices.GetSeriesExistingForAnime(anime.AnimeID);
                    if (seriesExists)
                    {
                        MessageBox.Show(Shoko.Commons.Properties.Resources.ERROR_SeriesExists, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    NewSeries frmNewSeries = new NewSeries();
                    frmNewSeries.Owner = this;
                    frmNewSeries.Init(anime, anime.FormattedTitle);

                    bool? result = frmNewSeries.ShowDialog();
                    if (result.HasValue && result.Value == true)
                    {

                    }
                }
                else
                {
                    NewSeries frm = new NewSeries();
                    frm.Owner = this;
                    frm.Init(0, "");
                    bool? result = frm.ShowDialog();
                    if (result.HasValue && result.Value == true)
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_AvdumpFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;

                    foreach (VM_AVDump dumpTemp in VM_MainListHelper.Instance.AVDumpFiles)
                    {
                        if (dumpTemp.FullPath == vid.GetLocalFileSystemFullPath()) return;
                    }

                    VM_AVDump dump = new VM_AVDump(vid);
                    VM_MainListHelper.Instance.AVDumpFiles.Add(dump);

                }

                if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    foreach (VM_VideoLocal vid in mv.VideoLocals)
                    {
                        bool alreadyExists = false;
                        foreach (VM_AVDump dumpTemp in VM_MainListHelper.Instance.AVDumpFiles)
                        {
                            if (dumpTemp.FullPath == vid.GetLocalFileSystemFullPath())
                            {
                                alreadyExists = true;
                                break;
                            }

                        }

                        if (alreadyExists) continue;

                        VM_AVDump dump = new VM_AVDump(vid);
                        VM_MainListHelper.Instance.AVDumpFiles.Add(dump);
                    }
                }

                tabControl1.SelectedIndex = TAB_MAIN_FileManger;
                tabFileManager.SelectedIndex = TAB_FileManger_Avdump;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void CommandBinding_BookmarkAnime(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = obj as VM_Recommendation;
                    if (rec == null) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
                    bookmark.AnimeID = rec.RecommendedAnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();

                }

                if (obj.GetType() == typeof(VM_MissingEpisode))
                {
                    VM_MissingEpisode rec = obj as VM_MissingEpisode;
                    if (rec == null) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
                    bookmark.AnimeID = rec.AnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime))
                {
                    VM_AniDB_Anime rec = obj as VM_AniDB_Anime;
                    if (rec == null) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
                    bookmark.AnimeID = rec.AnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                {
                    VM_AniDB_Anime_Similar sim = (VM_AniDB_Anime_Similar)obj;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
                    bookmark.AnimeID = sim.SimilarAnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Relation))
                {
                    VM_AniDB_Anime_Relation rel = (VM_AniDB_Anime_Relation)obj;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
                    bookmark.AnimeID = rel.RelatedAnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RefreshBookmarks(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            try
            {
                VM_MainListHelper.Instance.RefreshBookmarkedAnime();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowTorrentSearch(DownloadSearchCriteria crit)
        {
            Cursor = Cursors.Wait;

            tabControl1.SelectedIndex = TAB_MAIN_Downloads;
            tabcDownloads.SelectedIndex = 1;
            ucTorrentSearch.PerformSearch(crit);

            Cursor = Cursors.Arrow;
        }

        private void CommandBinding_ShowTorrentSearch(object sender, ExecutedRoutedEventArgs e)
        {
            //object obj = lbGroupsSeries.SelectedItem;
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Episode, ep.ToSearchParameters(), ep.AniDB_Anime, ep);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(VM_MissingEpisode))
                {
                    VM_MissingEpisode rec = obj as VM_MissingEpisode;
                    if (rec == null) return;

                    VM_AnimeEpisode_User contract = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetEpisodeByAniDBEpisodeID(rec.EpisodeID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (contract != null)
                    {
                        DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Episode, contract.ToSearchParameters(), contract.AniDB_Anime, contract);
                        ShowTorrentSearch(crit);
                    }
                }

                /*if (obj.GetType() == typeof(MissingFileVM))
				{
					MissingFileVM mis = (MissingFileVM)obj;
					ShowPinnedSeries(mis.AnimeSeries);
				}*/


                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = obj as VM_Recommendation;
                    if (rec == null) return;

                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, rec.Recommended_AniDB_Anime.ToSearchParameters(),rec.Recommended_AniDB_Anime,null);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime))
                {
                    VM_AniDB_Anime anime = (VM_AniDB_Anime)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, anime.ToSearchParameters(),anime, null);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {
                    VM_AnimeSeries_User ser = (VM_AnimeSeries_User)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, ser.AniDBAnime.AniDBAnime.ToSearchParameters(), ser.AniDBAnime.AniDBAnime, null);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                {
                    VM_AniDB_Anime_Similar sim = (VM_AniDB_Anime_Similar)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, sim.AniDB_Anime.ToSearchParameters(), sim.AniDB_Anime,null);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Relation))
                {
                    VM_AniDB_Anime_Relation rel = (VM_AniDB_Anime_Relation)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, rel.AniDB_Anime.ToSearchParameters(),rel.AniDB_Anime,null);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(VM_BookmarkedAnime))
                {
                    VM_BookmarkedAnime ba = (VM_BookmarkedAnime)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, ba.Anime.ToSearchParameters(), ba.Anime,null);
                    ShowTorrentSearch(crit);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowWebCacheAdmin(VM_AniDB_Anime anime)
        {
            try
            {
                SearchCriteria crit = new SearchCriteria();

                crit = new SearchCriteria();
                crit.ExtraInfo = string.Empty;
                crit.AnimeID = anime.AnimeID;

                tabControl1.SelectedIndex = TAB_MAIN_Community;
                tabcCommunity.SelectedIndex = 0;

                ucComLinks.PerformTvDBSearch(crit);
                ucComLinks.PerformTraktSearch(crit);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ShowWebCacheAdmin(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                SearchCriteria crit = null;

                if (obj.GetType() == typeof(VM_AniDB_Anime))
                {
                    VM_AniDB_Anime anime = (VM_AniDB_Anime)obj;
                    crit = new SearchCriteria();
                    crit.ExtraInfo = string.Empty;
                    crit.AnimeID = anime.AnimeID;
                }

                if (crit != null)
                {
                    tabControl1.SelectedIndex = TAB_MAIN_Community;
                    tabcCommunity.SelectedIndex = 0;

                    ucComLinks.PerformTvDBSearch(crit);
                    ucComLinks.PerformTraktSearch(crit);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ShowPinnedSeries(object sender, ExecutedRoutedEventArgs e)
        {
            //object obj = lbGroupsSeries.SelectedItem;
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                int? objID = null;

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)obj;
                    objID = ep.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {
                    VM_AnimeSeries_User ser = (VM_AnimeSeries_User)obj;
                    objID = ser.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                {
                    VM_AniDB_Anime_Similar sim = (VM_AniDB_Anime_Similar)obj;
                    objID = sim.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Relation))
                {
                    VM_AniDB_Anime_Relation rel = (VM_AniDB_Anime_Relation)obj;
                    objID = rel.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = (VM_Recommendation)obj;
                    objID = rec.Recommended_AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_MissingFile))
                {
                    VM_MissingFile mis = (VM_MissingFile)obj;
                    objID = mis.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_MissingEpisode))
                {
                    VM_MissingEpisode misEp = (VM_MissingEpisode)obj;
                    objID = misEp.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(VM_PlaylistItem))
                {
                    VM_PlaylistItem pli = (VM_PlaylistItem)obj;
                    if (pli.ItemType == PlaylistItemType.AnimeSeries)
                    {
                        var ser = (VM_AnimeSeries_User)pli.PlaylistItem;
                        objID = ser.AnimeSeriesID;
                    }
                    else if (pli.ItemType == PlaylistItemType.Episode)
                    {
                        VM_AnimeEpisode_User ep = pli.PlaylistItem as VM_AnimeEpisode_User;
                        objID = ep.AnimeSeriesID;
                    }
                }

                if (obj.GetType() == typeof(VM_AnimeSearch))
                {
                    VM_AnimeSearch search = (VM_AnimeSearch)obj;
                    objID = search.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(TraktSeriesData))
                {
                    TraktSeriesData trakt = (TraktSeriesData)obj;
                    objID = trakt.AnimeSeriesID;
                }

                if (objID != null)
                {
                    var valObjID = objID.Value;

                    VM_AnimeSeries_User ser;
                    if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(valObjID, out ser) == false)
                    {
                        // get the series
                        ser = (VM_AnimeSeries_User)VM_ShokoServer.Instance.ShokoServices.GetSeries(valObjID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                        if (ser != null)
                        {
                            VM_MainListHelper.Instance.AllSeriesDictionary[valObjID] = ser;
                        }
                    }

                    if (ser != null)
                        ShowPinnedSeries(ser);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_Refresh(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshView();
        }

        private void CommandBinding_Search(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // move to all groups
                VM_MainListHelper.Instance.ShowAllGroups();

                if (e.Parameter is CL_AnimeTag)
                {
                    CL_AnimeTag obj = e.Parameter as CL_AnimeTag;
                    txtGroupSearch.Text = obj.TagName;
                }

                tabControl1.SelectedIndex = TAB_MAIN_Collection;
                HighlightMainListItem();


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_Back(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_MainListHelper.Instance.MoveBackUpHeirarchy();
                HighlightMainListItem();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_Edit(object sender, ExecutedRoutedEventArgs e)
        {
            //object obj = lbGroupsSeries.SelectedItem;
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = (VM_AnimeGroup_User)obj;

                    if (grp.AnimeGroupID!=0)
                    {
                        groupBeforeChanges = new VM_AnimeGroup_User();

                        grp.Clone(groupBeforeChanges);
                    }

                    grp.IsReadOnly = false;
                    grp.IsBeingEdited = true;

                }

                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = (VM_GroupFilter)obj;

                    if (gf.GroupFilterID!=0)
                    {
                        groupFilterBeforeChanges = new VM_GroupFilter();
                        groupFilterBeforeChanges.GroupFilterName = gf.GroupFilterName;
                        groupFilterBeforeChanges.BaseCondition = gf.BaseCondition;
                        groupFilterBeforeChanges.ApplyToSeries = gf.ApplyToSeries;

                        foreach (VM_GroupFilterCondition gfc_cur in gf.Obs_FilterConditions)
                        {
                            VM_GroupFilterCondition gfc = new VM_GroupFilterCondition();
                            gfc.ConditionOperator = gfc_cur.ConditionOperator;
                            gfc.ConditionParameter = gfc_cur.ConditionParameter;
                            gfc.ConditionType = gfc_cur.ConditionType;
                            gfc.GroupFilterConditionID = gfc_cur.GroupFilterConditionID;
                            gfc.GroupFilterID = gfc_cur.GroupFilterID;
                            groupFilterBeforeChanges.Obs_FilterConditions.Add(gfc);
                        }

                        foreach (VM_GroupFilterSortingCriteria gfcs_cur in gf.SortCriteriaList)
                        {
                            VM_GroupFilterSortingCriteria gfsc = new VM_GroupFilterSortingCriteria();
                            gfsc.GroupFilterID = gfcs_cur.GroupFilterID;
                            gfsc.SortDirection = gfcs_cur.SortDirection;
                            gfsc.SortType = gfcs_cur.SortType;
                            groupFilterBeforeChanges.SortCriteriaList.Add(gfsc);
                        }
                        //Cloner.Clone(gf, groupFilterBeforeChanges);
                    }

                    gf.Locked = 0;
                    gf.IsBeingEdited = true;

                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableGroupControls(false);
        }

        private void CommandBinding_Save(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = (VM_AnimeGroup_User)obj;
                    bool isnew = grp.AnimeGroupID==0;
                    if (grp.Validate())
                    {
                        grp.IsReadOnly = true;
                        grp.IsBeingEdited = false;
                        if (grp.Save() && isnew)
                        {
                            VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                            VM_MainListHelper.Instance.ViewGroups.Refresh();
                            //VM_MainListHelper.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;

                            if (!grp.AnimeGroupParentID.HasValue)
                            {
                                // move to all groups
                                // only if it is a top level group
                                VM_MainListHelper.Instance.ShowAllGroups();
                                HighlightMainListItem();
                            }
                            else
                            {
                                VM_AnimeGroup_User parentGroup = grp.ParentGroup;
                                if (parentGroup != null)
                                    showChildWrappersWorker.RunWorkerAsync(parentGroup);
                            }
                        }

                    }
                    //BindingExpression be = ccDetail.GetBindingExpression(ContentControl.ContentProperty);
                    //be.UpdateSource();
                }

                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = (VM_GroupFilter)obj;


                    bool isnew = gf.GroupFilterID == 0;
					if (gf.Validate())
					{
					    gf.Locked = 0;
                        gf.IsBeingEdited = false;
						if (gf.Save() && isnew)
						{
						    VM_MainListHelper.Instance.AllGroupFiltersDictionary.Remove(0);
						    VM_MainListHelper.Instance.AllGroupFiltersDictionary.Add(gf.GroupFilterID, gf);
                            gf.GetDirectChildren();
							//VM_MainListHelper.Instance.LastGroupFilterID = gf.GroupFilterID.Value;
							showChildWrappersWorker.RunWorkerAsync(null);
						}
						//showChildWrappersWorker.RunWorkerAsync(null);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

            EnableDisableGroupControls(true);
        }

        private void CommandBinding_ScanFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_ImportFolder))
                {
                    VM_ImportFolder fldr = (VM_ImportFolder)obj;

                    VM_ShokoServer.Instance.ShokoServices.ScanFolder(fldr.ImportFolderID);
                    MessageBox.Show(Shoko.Commons.Properties.Resources.Import_Running, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void CommandBinding_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            EnableDisableGroupControls(false);

            try
            {
                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = (VM_GroupFilter)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.Filter_DeleteGroup, gf.GroupFilterName),
                    Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        // remove from group list
                        gf.Delete();





                        // lets try and find where we are in the list so we can highlight that item
                        // when deleting a a group, we should always have it highlighted
                        // in the main list on the left
                        int idx = lbGroupsSeries.SelectedIndex;
                        if (idx >= 0)
                        {
                            if (idx > 0)
                            {
                                // we will move to the item above the item being deleted
                                idx = idx - 1;
                            }
                            // otherwise just move to the first item
                            lbGroupsSeries.SelectedIndex = idx;
                            lbGroupsSeries.Focus();
                            lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[idx]);
                        }

                        // find the group filter
                        int pos = -1;
                        int i = 0;
                        foreach (IListWrapper wrapper in VM_MainListHelper.Instance.CurrentWrapperList)
                        {
                            if (wrapper is VM_GroupFilter)
                            {
                                VM_GroupFilter gfTemp = wrapper as VM_GroupFilter;
                                if (gfTemp.GroupFilterID!=0 && gf.GroupFilterID == gfTemp.GroupFilterID)
                                {
                                    pos = i;
                                    break;
                                }
                            }
                            i++;
                        }

						// remove from group filter list
                        if (gf.GroupFilterID!=0 && VM_MainListHelper.Instance.AllGroupFiltersDictionary.ContainsKey(gf.GroupFilterID))
                            VM_MainListHelper.Instance.AllGroupFiltersDictionary.Remove(gf.GroupFilterID);
                        // remove from current wrapper list
                        if (pos >= 0)
						{
							VM_MainListHelper.Instance.CurrentWrapperList.RemoveAt(pos);
							//VM_MainListHelper.Instance.ViewGroups.Refresh();
						}

                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableGroupControls(true);
        }

        private void CommandBinding_Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = (VM_AnimeGroup_User)obj;
                    grp.IsReadOnly = true;
                    grp.IsBeingEdited = false;

                    // copy all editable properties
                    if (grp.AnimeGroupID!=0) // an existing group
                    {
                        grp.GroupName = groupBeforeChanges.GroupName;
                        grp.IsFave = groupBeforeChanges.IsFave;

                        //grp.AnimeGroupParentID = groupBeforeChanges.AnimeGroupParentID;
                        grp.Description = groupBeforeChanges.Description;
                        grp.SortName = groupBeforeChanges.SortName;

                        VM_MainListHelper.Instance.ViewGroups.Refresh();
                        EnableDisableGroupControls(true);
                        //VM_MainListHelper.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;
                        HighlightMainListItem();
                    }
                    else
                    {
                        HighlightMainListItem();
                        SetDetailBinding(null);
                    }


                }

                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = (VM_GroupFilter)obj;
                    gf.Locked = 1;
                    gf.IsBeingEdited = false;

                    // copy all editable properties
                    if (gf.GroupFilterID!=0) // an existing group filter
                    {
                        gf.GroupFilterName = groupFilterBeforeChanges.GroupFilterName;
                        gf.ApplyToSeries = groupFilterBeforeChanges.ApplyToSeries;
                        gf.BaseCondition = groupFilterBeforeChanges.BaseCondition;
                        gf.Obs_FilterConditions.Clear();
                        gf.SortCriteriaList.Clear();

                        foreach (VM_GroupFilterCondition gfc_old in groupFilterBeforeChanges.Obs_FilterConditions)
                        {
                            VM_GroupFilterCondition gfc = new VM_GroupFilterCondition();
                            gfc.ConditionOperator = gfc_old.ConditionOperator;
                            gfc.ConditionParameter = gfc_old.ConditionParameter;
                            gfc.ConditionType = gfc_old.ConditionType;
                            gfc.GroupFilterConditionID = gfc_old.GroupFilterConditionID;
                            gfc.GroupFilterID = gfc_old.GroupFilterID;
                            gf.Obs_FilterConditions.Add(gfc);
                        }

                        foreach (VM_GroupFilterSortingCriteria gfsc_old in groupFilterBeforeChanges.SortCriteriaList)
                        {
                            VM_GroupFilterSortingCriteria gfsc = new VM_GroupFilterSortingCriteria();
                            gfsc.GroupFilterID = gfsc_old.GroupFilterID;
                            gfsc.SortDirection = gfsc_old.SortDirection;
                            gfsc.SortType = gfsc_old.SortType;
                            gf.SortCriteriaList.Add(gfsc);
                        }

                        //VM_MainListHelper.Instance.LastGroupFilterID = gf.GroupFilterID.Value;
                    }
                    else
                    {
                        SetDetailBinding(null);
                    }
                    EnableDisableGroupControls(true);
                    HighlightMainListItem();
                }

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_NewGroupFilter(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_GroupFilter gfNew = new VM_GroupFilter();
                gfNew.Locked = 0;

                gfNew.GroupFilterName = Shoko.Commons.Properties.Resources.Filter_New;
                gfNew.ApplyToSeries = 0;
                gfNew.BaseCondition = (int)GroupFilterBaseCondition.Include;
               
                VM_MainListHelper.Instance.AllGroupFiltersDictionary[0]=gfNew;
                
                groupFilterVM = gfNew;
                VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gfNew);

                SetDetailBinding(gfNew);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteFilterCondition(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_GroupFilterCondition))
                {
                    VM_GroupFilterCondition gfc = (VM_GroupFilterCondition)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.Filter_DeleteCondition, gfc.NiceDescription),
                    Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        // remove from group list
                        //gfc.Delete();


                        // fund the GroupFilter

                        foreach (VM_GroupFilter gf in VM_MainListHelper.Instance.AllGroupFiltersDictionary.Values)
                        {
                            if (!gf.AllowEditing) continue; // all filter
                            if (gf.GroupFilterID == gfc.GroupFilterID)
                            {
                                int pos = -1;
                                for (int i = 0; i < gf.Obs_FilterConditions.Count; i++)
                                {
                                    if (gfc.ConditionOperator == gf.Obs_FilterConditions[i].ConditionOperator &&
                                        gfc.ConditionParameter == gf.Obs_FilterConditions[i].ConditionParameter &&
                                        gfc.ConditionType == gf.Obs_FilterConditions[i].ConditionType)
                                    {
                                        pos = i;
                                        break;
                                    }
                                }
                                if (pos >= 0)
                                    gf.Obs_FilterConditions.RemoveAt(pos);

                                groupFilterVM = gf;
                                VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                                VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_NewFilterCondition(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                object obj = e.Parameter;
                if (obj == null) return;

                VM_GroupFilter gf = (VM_GroupFilter)obj;
                VM_GroupFilterCondition gfc = new VM_GroupFilterCondition();

                GroupFilterConditionForm frm = new GroupFilterConditionForm();
                frm.Owner = this;
                frm.Init(gf, gfc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    gf.Obs_FilterConditions.Add(gfc);

                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_NewFilterSorting(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                object obj = e.Parameter;
                if (obj == null) return;

                VM_GroupFilter gf = (VM_GroupFilter)obj;
                VM_GroupFilterSortingCriteria gfsc = new VM_GroupFilterSortingCriteria();

                GroupFilterSortingForm frm = new GroupFilterSortingForm();
                frm.Owner = this;
                frm.Init(gf, gfsc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    gf.SortCriteriaList.Add(gfsc);

                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_MoveUpFilterSort(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_GroupFilterSortingCriteria))
                {
                    VM_GroupFilterSortingCriteria gfsc = (VM_GroupFilterSortingCriteria)obj;
                    GroupFilterSortMoveUpDown(gfsc, 1);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_MoveDownFilterSort(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_GroupFilterSortingCriteria))
                {
                    VM_GroupFilterSortingCriteria gfsc = (VM_GroupFilterSortingCriteria)obj;
                    GroupFilterSortMoveUpDown(gfsc, 2);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Moves group sorting up and down
        /// </summary>
        /// <param name="gfsc"></param>
        /// <param name="direction">1 = up, 2 = down</param>
        private void GroupFilterSortMoveUpDown(VM_GroupFilterSortingCriteria gfsc, int direction)
        {
            // find the sorting condition
            foreach (VM_GroupFilter gf in VM_MainListHelper.Instance.AllGroupFiltersDictionary.Values)
            {
                if (!gf.AllowEditing) continue; // all filter
                if (gf.GroupFilterID == gfsc.GroupFilterID)
                {
                    int pos = -1;
                    for (int i = 0; i < gf.SortCriteriaList.Count; i++)
                    {
                        if (gfsc.SortType == gf.SortCriteriaList[i].SortType)
                        {
                            pos = i;
                            break;
                        }
                    }

                    if (direction == 1) // up
                    {
                        if (pos > 0)
                        {
                            gf.SortCriteriaList.Move(pos, pos - 1);
                            groupFilterVM = gf;
                            VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                            VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                        }
                    }
                    else
                    {
                        if (pos + 1 < gf.SortCriteriaList.Count)
                        {
                            gf.SortCriteriaList.Move(pos, pos + 1);
                            groupFilterVM = gf;
                            VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                            VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                        }
                    }
                }
            }
        }



        private void CommandBinding_DeleteFilterSort(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_GroupFilterSortingCriteria))
                {
                    VM_GroupFilterSortingCriteria gfsc = (VM_GroupFilterSortingCriteria)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.Filter_DeleteSort),
                    Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        // find the sorting condition
                        foreach (VM_GroupFilter gf in VM_MainListHelper.Instance.AllGroupFiltersDictionary.Values)
                        {
                            if (!gf.AllowEditing) continue; // all filter
                            if (gf.GroupFilterID == gfsc.GroupFilterID)
                            {
                                int pos = -1;
                                for (int i = 0; i < gf.SortCriteriaList.Count; i++)
                                {
                                    if (gfsc.SortType == gf.SortCriteriaList[i].SortType)
                                    {
                                        pos = i;
                                        break;
                                    }
                                }
                                if (pos >= 0)
                                    gf.SortCriteriaList.RemoveAt(pos);

                                groupFilterVM = gf;
                                VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                                VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_NewGroup(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_AnimeGroup_User grpNew = new VM_AnimeGroup_User();
                grpNew.IsReadOnly = false;
                grpNew.IsBeingEdited = true;
                SetDetailBinding(grpNew);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteGroup(object sender, ExecutedRoutedEventArgs e)
        {
            EnableDisableGroupControls(false);

            try
            {
                VM_AnimeGroup_User grp = e.Parameter as VM_AnimeGroup_User;
                if (grp == null) return;

                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
                frm.Owner = this;
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    bool deleteFiles = frm.DeleteFiles;

                    Cursor = Cursors.Wait;
                    VM_ShokoServer.Instance.ShokoServices.DeleteAnimeGroup(grp.AnimeGroupID, deleteFiles);

                    VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                    VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
                    SetDetailBinding(null);
                    Cursor = Cursors.Arrow;
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableGroupControls(true);
        }

        private void CommandBinding_ViewGroup(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_AnimeGroup_User grp = e.Parameter as VM_AnimeGroup_User;
                if (grp == null) return;

                SetDetailBinding(grp);

                //DisplayMainTab(TAB_MAIN_Collection);
                tabControl1.SelectedIndex = TAB_MAIN_Collection;


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_AddSubGroup(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeGroup_User grp = e.Parameter as VM_AnimeGroup_User;
            if (grp == null) return;

            try
            {
                VM_AnimeGroup_User grpNew = new VM_AnimeGroup_User();
                grpNew.IsReadOnly = false;
                grpNew.IsBeingEdited = true;
                grpNew.AnimeGroupParentID = grp.AnimeGroupID;
                SetDetailBinding(grpNew);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_AddPlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            VM_PlaylistHelper.CreatePlaylist(this);
        }

        private void CommandBinding_DeletePlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            VM_Playlist pl = e.Parameter as VM_Playlist;
            if (pl == null) return;



            try
            {
                MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.Playlist_Delete, pl.PlaylistName),
                    Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Cursor = Cursors.Wait;

                    if (pl.PlaylistID!=0)
                    {
                        string msg = VM_ShokoServer.Instance.ShokoServices.DeletePlaylist(pl.PlaylistID);
                        if (!string.IsNullOrEmpty(msg))
                            Utils.ShowErrorMessage(msg);
                    }

                    SetDetailBindingPlaylist(null);

                    // refresh data
                    VM_PlaylistHelper.Instance.RefreshData();
                    if (lbPlaylists.Items.Count > 0)
                        lbPlaylists.SelectedIndex = 0;



                    Cursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeletePlaylistItem(object sender, ExecutedRoutedEventArgs e)
        {

            try
            {
                VM_PlaylistItem pli = e.Parameter as VM_PlaylistItem;
                if (pli == null) return;

                Cursor = Cursors.Wait;

                // get the playlist
                VM_Playlist pl = (VM_Playlist)VM_ShokoServer.Instance.ShokoServices.GetPlaylist(pli.PlaylistID);
                if (pl == null)
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show(Shoko.Commons.Properties.Resources.Filter_PlaylistMissing, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (pli.ItemType == PlaylistItemType.AnimeSeries)
                {
                    VM_AnimeSeries_User ser = pli.PlaylistItem as VM_AnimeSeries_User;
                    pl.RemoveSeries(ser.AnimeSeriesID);
                }
                if (pli.ItemType == PlaylistItemType.Episode)
                {
                    VM_AnimeEpisode_User ep = pli.PlaylistItem as VM_AnimeEpisode_User;
                    pl.RemoveEpisode(ep.AnimeEpisodeID);
                }

                pl.Save();
                pl = (VM_Playlist)VM_ShokoServer.Instance.ShokoServices.GetPlaylist(pli.PlaylistID);

                // refresh data
                if (lbPlaylists.Items.Count > 0)
                {
                    // get the current playlist
                    VM_Playlist selPL = lbPlaylists.SelectedItem as VM_Playlist;
                    if (selPL != null && pl != null && selPL.PlaylistID == pl.PlaylistID)
                    {
                        selPL.Populate(pl);
                        selPL.PopulatePlaylistObjects();
                        VM_PlaylistHelper.Instance.OnPlaylistModified(new PlaylistModifiedEventArgs(pl.PlaylistID));
                    }
                    else
                    {
                        VM_PlaylistHelper.Instance.RefreshData();
                    }
                }

                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RefreshPlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // refresh data
                VM_PlaylistHelper.Instance.RefreshData();
                if (lbPlaylists.Items.Count > 0)
                    lbPlaylists.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }



        private void CommandBinding_IncrementSeriesImageSize(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.SeriesGroup_Image_Height = VM_UserSettings.Instance.SeriesGroup_Image_Height + 10;
        }

        private void CommandBinding_DecrementSeriesImageSize(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.SeriesGroup_Image_Height = VM_UserSettings.Instance.SeriesGroup_Image_Height - 10;
        }

        private void CommandBinding_NewSeries(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private int prevgf = -1;
        private void CommandBinding_DeleteSeries(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Parameter as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
                frm.Owner = this;
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    Cursor = Cursors.Wait;

                    VM_ShokoServer.Instance.ShokoServices.DeleteAnimeSeries(ser.AnimeSeriesID, frm.DeleteFiles, frm.DeleteGroups);
                    VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                    VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
                    SetDetailBinding(null);
                    Cursor = Cursors.Arrow;

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
 

        private void CommandBinding_MoveSeries(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Parameter as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                MoveSeries frm = new MoveSeries();
                frm.Owner = this;
                frm.Init(ser);
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    VM_AnimeGroup_User grpSelected = frm.SelectedGroup;
                    if (grpSelected == null) return;

                    VM_MoveSeriesDetails request = new VM_MoveSeriesDetails();
                    request.OldAnimeGroupID = ser.AnimeGroupID;

                    ser.AnimeGroupID = grpSelected.AnimeGroupID;
                    request.UpdatedSeries = ser;

                    Cursor = Cursors.Wait;
                    EnableDisableGroupControls(false);
                    moveSeriesWorker.RunWorkerAsync(request);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ClearSearch(object sender, ExecutedRoutedEventArgs e)
        {
            txtGroupSearch.Text = "";
            HighlightMainListItem();
        }

        private void CommandBinding_RunImport(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.RunImport();
                MessageBox.Show(Shoko.Commons.Properties.Resources.Import_Running, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void CommandBinding_RemoveMissingFiles(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.Main_RunProcess),
                    Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    VM_ShokoServer.Instance.RemoveMissingFiles();
                    MessageBox.Show(Shoko.Commons.Properties.Resources.Process_Running, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_SyncMyList(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.SyncMyList();
                MessageBox.Show(Shoko.Commons.Properties.Resources.Process_Running, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_SyncVotes(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.SyncVotes();
                MessageBox.Show(Shoko.Commons.Properties.Resources.Process_Running, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_SyncMALUp(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SyncMALUpload();
                MessageBox.Show(Shoko.Commons.Properties.Resources.Process_Queued, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_SyncMALDown(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SyncMALDownload();
                MessageBox.Show(Shoko.Commons.Properties.Resources.Process_Queued, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RevokeVote(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Parameter as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                VM_ShokoServer.Instance.RevokeVote(ser.AniDB_ID);

                // refresh the data
                //ser.RefreshBase();
                //ser.AniDB_Anime.Detail.RefreshBase();

                VM_MainListHelper.Instance.UpdateHeirarchy(ser);

                //SetDetailBinding(null);
                //SetDetailBinding(ser);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;
            EnableDisableGroupControls(false);
            toggleStatusWorker.RunWorkerAsync(obj);
        }


        private void CommandBinding_BreadCrumbSelect(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // switching back to the top view (show all filters)
                if (e.Parameter == null)
                {
                    VM_MainListHelper.Instance.ShowChildWrappers(null);
                }
                if (e.Parameter is IListWrapper)
                {
                    VM_MainListHelper.Instance.ShowChildWrappers(e.Parameter as IListWrapper);
                }
                HighlightMainListItem();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ToggleFave(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            if (obj.GetType() == typeof(VM_AnimeGroup_User))
            {
                VM_AnimeGroup_User grp = (VM_AnimeGroup_User)obj;
                grp.IsFave = grp.IsFave == 1 ? 0 : 1;

                // the user can toggle the fave without going into edit mode
                if (grp.IsReadOnly)
                    grp.Save();


                //BindingExpression be = ccDetail.GetBindingExpression(ContentControl.ContentProperty);
                //be.UpdateSource();
            }

            if (obj.GetType() == typeof(VM_AnimeSeries_User))
            {
                VM_AnimeSeries_User ser = (VM_AnimeSeries_User)obj;
                VM_AnimeGroup_User grp = ser.TopLevelAnimeGroup;
                if (grp == null) return;

                grp.IsFave = grp.IsFave == 1 ? 0 : 1;
                grp.Save();

                ser.PopulateIsFave();

                VM_MainListHelper.Instance.UpdateHeirarchy(ser);
            }
        }

        private void CommandBinding_ToggleExpandTags(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.TagsExpanded = !VM_UserSettings.Instance.TagsExpanded;
        }

        private void CommandBinding_ToggleExpandTitles(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.TitlesExpanded = !VM_UserSettings.Instance.TitlesExpanded;
        }

        private void SetWindowFullscreen()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            //this.Topmost = true;
            WindowState = WindowState.Maximized;
        }

        private void CommandBinding_WindowFullScreen(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.WindowFullScreen = true;
            VM_UserSettings.Instance.WindowNormal = false;
            SetWindowFullscreen();
        }

        private void SetWindowNormal()
        {
            WindowState = AppSettings.DefaultWindowState;
            WindowStyle = WindowStyle.SingleBorderWindow;
        }

        private void CommandBinding_WindowNormal(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.WindowFullScreen = false;
            VM_UserSettings.Instance.WindowNormal = true;
            SetWindowNormal();
        }

        private void CommandBinding_WindowClose(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandBinding_WindowMinimize(object sender, ExecutedRoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        #region Server Queue Actions

        private void CommandBinding_HasherQueuePause(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetCommandProcessorHasherPaused(true);
                VM_ShokoServer.Instance.UpdateServerStatus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_HasherQueueResume(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetCommandProcessorHasherPaused(false);
                VM_ShokoServer.Instance.UpdateServerStatus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_GeneralQueuePause(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetCommandProcessorGeneralPaused(true);
                VM_ShokoServer.Instance.UpdateServerStatus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_GeneralQueueResume(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetCommandProcessorGeneralPaused(false);
                VM_ShokoServer.Instance.UpdateServerStatus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ServerImageQueuePause(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetCommandProcessorImagesPaused(true);
                VM_ShokoServer.Instance.UpdateServerStatus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ServerImageQueueResume(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetCommandProcessorImagesPaused(false);
                VM_ShokoServer.Instance.UpdateServerStatus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        #endregion

        #endregion






        public bool GroupFilter_GroupSearch(object obj)
        {
            VM_AnimeGroup_User grpvm = obj as VM_AnimeGroup_User;
            if (grpvm == null) return false;
            return GroupSearchFilterHelper.EvaluateGroupFilter(groupFilterVM, grpvm);
        }

        void lbPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetDetailBindingPlaylist(null);

                ListBox lb = (ListBox)sender;

                object obj = lb.SelectedItem;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_Playlist))
                {
                    Cursor = Cursors.Wait;
                    VM_Playlist pl = obj as VM_Playlist;
                    pl.PopulatePlaylistObjects();
                    //series.RefreshBase();
                    //VM_MainListHelper.Instance.LastAnimeSeriesID = series.AnimeSeriesID.Value;
                    //VM_MainListHelper.Instance.CurrentSeries = series;
                }


                SetDetailBindingPlaylist(obj);
                Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void lbGroupsSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //epListMain.DataContext = null;

                ListBox lb = (ListBox)sender;

                object obj = lb.SelectedItem;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {

                    VM_AnimeSeries_User series = obj as VM_AnimeSeries_User;
                    //series.RefreshBase();
                    //VM_MainListHelper.Instance.LastAnimeSeriesID = series.AnimeSeriesID.Value;
                    VM_MainListHelper.Instance.CurrentSeries = series;
                }

                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = obj as VM_AnimeGroup_User;
                    //VM_MainListHelper.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;
                }

                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = obj as VM_GroupFilter;
                    //VM_MainListHelper.Instance.LastGroupFilterID = gf.GroupFilterID.Value;

                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }

                if (!string.IsNullOrEmpty(VM_MainListHelper.Instance.CurrentOpenGroupFilter) && lbGroupsSeries.SelectedItem != null)
                    VM_MainListHelper.Instance.LastGroupForGF[VM_MainListHelper.Instance.CurrentOpenGroupFilter] = lbGroupsSeries.SelectedIndex;

                //SetDetailBinding(VM_MainListHelper.Instance.AllGroups[0]);
                SetDetailBinding(obj);

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {
                    VM_AnimeSeries_User series = obj as VM_AnimeSeries_User;
                    //epListMain.DataContext = series;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowChildrenForCurrentGroup(VM_AnimeSeries_User ser)
        {
            if (lbGroupsSeries.SelectedItem == null) return;

            if (lbGroupsSeries.SelectedItem is IListWrapper)
            {
                // this is the last supported drill down
                if (lbGroupsSeries.SelectedItem.GetType() == typeof(VM_AnimeSeries_User)) return;

                //VM_MainListHelper.Instance.LastAnimeSeriesID = ser.AnimeSeriesID.Value;

                EnableDisableGroupControls(false);
                showChildWrappersWorker.RunWorkerAsync(lbGroupsSeries.SelectedItem);
            }
        }

        void lbGroupsSeries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            var source = e.Source as FrameworkElement;

            if (Equals(originalSource.DataContext, source.DataContext)) //both null, or both what ever the scrollbar and the listview/treeview
                return;

            try
            {
                var selItem = lbGroupsSeries.SelectedItem;
                if (selItem == null) return;

                var animeGroup = selItem as VM_AnimeGroup_User;
                if (animeGroup != null)
                    VM_MainListHelper.Instance.CurrentOpenGroupFilter = "VM_AnimeGroup_User|" + animeGroup.AnimeGroupID;

                var groupFilter = selItem as VM_GroupFilter;
                if (groupFilter != null)
                    VM_MainListHelper.Instance.CurrentOpenGroupFilter = "GroupFilterVM|" + groupFilter.GroupFilterID;

                if (selItem is IListWrapper)
                {
                    //SetDetailBinding(null);
                    // this is the last supported drill down
                    if (selItem.GetType() == typeof(VM_AnimeSeries_User)) return;

                    EnableDisableGroupControls(false);
                    showChildWrappersWorker.RunWorkerAsync(lbGroupsSeries.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex.ToString());
            }
        }


        private void HighlightMainListItem()
        {
            try
            {
                if (VM_MainListHelper.Instance.LastGroupForGF.ContainsKey(VM_MainListHelper.Instance.CurrentOpenGroupFilter))
                {
                    int lastSelIndex = VM_MainListHelper.Instance.LastGroupForGF[VM_MainListHelper.Instance.CurrentOpenGroupFilter];
                    if (lastSelIndex < lbGroupsSeries.Items.Count)
                    {
                        lbGroupsSeries.SelectedItem = lbGroupsSeries.Items[lastSelIndex];
                        lbGroupsSeries.Focus();
                        lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[lastSelIndex]);
                        //SetDetailBinding(lbGroupsSeries.SelectedItem);

                        return;
                    }
                    else
                    {
                        // move to the previous item
                        if (lastSelIndex - 1 <= lbGroupsSeries.Items.Count)
                        {
                            if (lastSelIndex > 0)
                            {
                                lbGroupsSeries.SelectedItem = lbGroupsSeries.Items[lastSelIndex - 1];
                                lbGroupsSeries.Focus();
                                lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[lastSelIndex - 1]);
                                //SetDetailBinding(lbGroupsSeries.SelectedItem);

                                return;
                            }
                        }
                    }
                }

                if (lbGroupsSeries.Items != null && lbGroupsSeries.Items.Count > 0)
                {
                    lbGroupsSeries.SelectedIndex = 0;
                    lbGroupsSeries.Focus();
                    lbGroupsSeries.ScrollIntoView(lbGroupsSeries.SelectedItem);
                    //SetDetailBinding(lbGroupsSeries.SelectedItem);
                }

                return;                
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }




        private void EnableDisableGroupControls(bool val)
        {
            lbGroupsSeries.IsEnabled = val;
            txtGroupSearch.IsEnabled = val;
            tbSeriesEpisodes.IsEnabled = val;
            //epListMain.IsEnabled = val;
            //ccDetail.IsEnabled = val;
        }


        private void SetDetailBinding(object objToBind)
        {
            try
            {
                //BindingOperations.ClearBinding(ccDetail, ContentControl.ContentProperty);

                if (objToBind != null && objToBind.GetType().Equals(typeof(VM_AnimeSeries_User)))
                {
                    VM_AnimeSeries_User ser = objToBind as VM_AnimeSeries_User;
                    if (AppSettings.DisplaySeriesSimple)
                    {
                        AnimeSeriesSimplifiedControl ctrl = new AnimeSeriesSimplifiedControl();
                        ctrl.DataContext = ser;

                        AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl();
                        cont.IsMetroDash = false;
                        cont.DataContext = ctrl;

                        objToBind = cont;
                    }
                    else
                    {
                        AnimeSeries ctrl = new AnimeSeries();
                        ctrl.DataContext = ser;

                        AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl();
                        cont.IsMetroDash = false;
                        cont.DataContext = ctrl;

                        objToBind = cont;
                    }
                }

                Binding b = new Binding();
                b.Source = objToBind;
                b.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                ccDetail.SetBinding(ContentProperty, b);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void SetDetailBindingPlaylist(object objToBind)
        {
            try
            {
                //BindingOperations.ClearBinding(ccDetail, ContentControl.ContentProperty);
                Binding b = new Binding();
                b.Source = objToBind;
                b.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                ccPlaylist.SetBinding(ContentProperty, b);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }




        void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        void grdMain_LayoutUpdated(object sender, EventArgs e)
        {
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
