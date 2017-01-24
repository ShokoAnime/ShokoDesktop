﻿using Infralution.Localization.Wpf;
using JMMClient.Downloads;
using JMMClient.Forms;
using JMMClient.ImageDownload;
using JMMClient.UserControls;
using JMMClient.Utilities;
using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using JMMClient.AutoUpdates;
using JMMClient.VideoPlayers;

namespace JMMClient
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

        public static GroupFilterVM groupFilterVM = null;
        public static List<UserCulture> userLanguages = new List<UserCulture>();
        public static ImageDownloader imageHelper = null;

        private AnimeGroupVM groupBeforeChanges = null;
        private GroupFilterVM groupFilterBeforeChanges = null;



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
                this.grdMain.LayoutUpdated += new EventHandler(grdMain_LayoutUpdated);
                this.LayoutUpdated += new EventHandler(MainWindow_LayoutUpdated);

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

                MainListHelperVM.Instance.ViewGroups.Filter = GroupSearchFilter;
                cboLanguages.SelectionChanged += new SelectionChangedEventHandler(cboLanguages_SelectionChanged);

                if (MainListHelperVM.Instance.SeriesSearchTextBox == null) MainListHelperVM.Instance.SeriesSearchTextBox = seriesSearch.txtSeriesSearch;

                //grdSplitEps.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler(grdSplitEps_DragCompleted);


                imageHelper = new ImageDownloader();
                imageHelper.Init();

                videoHandler.Init();
                //videoHandler.HandleFileChange(AppSettings.MPCFolder + "\\mpc-hc.ini");

                InitCulture();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                imageHelper.QueueUpdateEvent += new ImageDownloader.QueueUpdateEventHandler(imageHelper_QueueUpdateEvent);

                cboGroupSort.Items.Clear();
                foreach (string sType in GroupFilterHelper.GetAllSortTypes())
                    cboGroupSort.Items.Add(sType);
                cboGroupSort.SelectedIndex = 0;
                btnToolbarSort.Click += new RoutedEventHandler(btnToolbarSort_Click);

                tabControl1.SelectionChanged += new SelectionChangedEventHandler(tabControl1_SelectionChanged);
                tabFileManager.SelectionChanged += new SelectionChangedEventHandler(tabFileManager_SelectionChanged);
                tabSettingsChild.SelectionChanged += new SelectionChangedEventHandler(tabSettingsChild_SelectionChanged);

                this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
                this.StateChanged += new EventHandler(MainWindow_StateChanged);

                // Have commented this out because it is no good when Desktop and Server are sharing
                // the same base image path
                //DeleteAvatarImages();

                this.AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(this.CloseTab));

                btnUpdateMediaInfo.Click += new RoutedEventHandler(btnUpdateMediaInfo_Click);
                btnFeed.Click += new RoutedEventHandler(btnFeed_Click);
                btnDiscord.Click += new RoutedEventHandler(btnDiscord_Click);
                btnAbout.Click += new RoutedEventHandler(btnAbout_Click);
                btnClearHasherQueue.Click += new RoutedEventHandler(btnClearHasherQueue_Click);
                btnClearGeneralQueue.Click += new RoutedEventHandler(btnClearGeneralQueue_Click);
                btnClearServerImageQueue.Click += new RoutedEventHandler(btnClearServerImageQueue_Click);
                btnAdminMessages.Click += new RoutedEventHandler(btnAdminMessages_Click);

                JMMServerVM.Instance.BaseImagePath = Utils.GetBaseImagesPath();

                // timer for automatic updates
                postStartTimer = new System.Timers.Timer();
                postStartTimer.AutoReset = false;
                postStartTimer.Interval = 5 * 1000; // 15 seconds
                postStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(postStartTimer_Elapsed);

                btnSwitchUser.Click += new RoutedEventHandler(btnSwitchUser_Click);

                //videoHandler.HandleFileChange(@"C:\Program Files (x86)\Combined Community Codec Pack\MPC\mpc-hc.ini");

                MainWindow.videoHandler.VideoWatchedEvent += new VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

                if (AppSettings.DashboardType == DashboardType.Normal)
                    dash.Visibility = System.Windows.Visibility.Visible;
                else
                    dashMetro.Visibility = System.Windows.Visibility.Visible;

                UserSettingsVM.Instance.SetDashMetro_Image_Width();
                MainListHelperVM.Instance.Refreshed += Instance_Refreshed;
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

            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
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
          if (JMMServerVM.Instance.ServerOnline)
          {
             if (JMMServerVM.Instance.AuthenticateUser())
             {
                MainListHelperVM.Instance.ClearData();
                MainListHelperVM.Instance.ShowChildWrappers(null);

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
                this.Cursor = Cursors.Wait;
                JMMServerVM.Instance.clientBinaryHTTP.ClearImagesQueue();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            this.Cursor = Cursors.Arrow;
        }

        void btnClearGeneralQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                JMMServerVM.Instance.clientBinaryHTTP.ClearGeneralQueue();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            this.Cursor = Cursors.Arrow;
        }

        void btnClearHasherQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                JMMServerVM.Instance.clientBinaryHTTP.ClearHasherQueue();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            this.Cursor = Cursors.Arrow;
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
                System.Diagnostics.Process.Start("https://discord.gg/0XKJW7TObKLajoKc");
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
                JMMServerVM.Instance.clientBinaryHTTP.RefreshAllMediaInfo();
                MessageBox.Show(Properties.Resources.Main_ProcessRunning, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
            System.Windows.Controls.TabItem tabItem = args.Source as System.Windows.Controls.TabItem;
            if (tabItem != null)
            {
                System.Windows.Controls.TabControl tabControl = tabItem.Parent as System.Windows.Controls.TabControl;
                if (tabControl != null)
                    tabControl.Items.Remove(tabItem);
            }
        }


        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            //if (this.WindowState == System.Windows.WindowState.Minimized) this.Hide();

            if (this.WindowState == System.Windows.WindowState.Normal || this.WindowState == System.Windows.WindowState.Maximized)
                AppSettings.DefaultWindowState = this.WindowState;
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
            JMMServerVM.Instance.Test();

            bool loggedIn = false;
            if (JMMServerVM.Instance.ServerOnline)
                loggedIn = JMMServerVM.Instance.LoginAsLastUser();

            if (!loggedIn)
            {
                // authenticate user
                if (JMMServerVM.Instance.ServerOnline && !JMMServerVM.Instance.AuthenticateUser())
                {
                    this.Close();
                    return;
                }
            }

            if (JMMServerVM.Instance.ServerOnline)
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
                JMMServerVM.Instance.ApplicationVersion = Utils.GetApplicationVersion(a);
            }

            UTorrentHelperVM.Instance.ValidateCredentials();

            postStartTimer.Start();

            CheckForUpdatesNew(false);

            var collView = CollectionViewSource.GetDefaultView(tabControl1.Items);
            collView.CurrentChanging += CollView_CurrentChanging;
        }

        void postStartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            postStartTimer.Stop();

            UTorrentHelperVM.Instance.Init();

            if (JMMServerVM.Instance.ServerOnline)
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
                    JMMAutoUpdatesHelper.ConvertToAbsoluteVersion(
                        JMMAutoUpdatesHelper.GetLatestVersionNumber(AppSettings.UpdateChannel));

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
                    MainListHelperVM.Instance.MainScrollerWidth = tempWidth;
                }

                tempWidth = tabControl1.ActualWidth - 20;
                //tempWidth = tabControl1.ActualWidth - 300;
                if (tempWidth > 0)
                    MainListHelperVM.Instance.FullScrollerWidth = tempWidth;

                tempHeight = tabControl1.ActualHeight - 50;
                if (tempHeight > 0)
                    MainListHelperVM.Instance.FullScrollerHeight = tempHeight;

                tempWidth = ccPlaylist.ActualWidth - 8;
                if (tempWidth > 0)
                    MainListHelperVM.Instance.PlaylistWidth = tempWidth;

                tempWidth = tabcDownloads.ActualWidth - 130;
                if (tempWidth > 0)
                    MainListHelperVM.Instance.DownloadRecScrollerWidth = tempWidth;
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
                if (e.Source is System.Windows.Controls.TabControl)
                {
                    System.Windows.Controls.TabControl tab = e.Source as System.Windows.Controls.TabControl;
                    this.Cursor = Cursors.Wait;
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
                    this.Cursor = Cursors.Arrow;
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
            
            DashboardVM.Instance.RefreshData(opt.RefreshContinueWatching, opt.RefreshRecentAdditions,
                opt.RefreshOtherWidgets, opt.RecentAdditionType);
        }

        void showDashboardWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
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
                        if (dash.Visibility == System.Windows.Visibility.Visible)
                        {
                            if (DashboardVM.Instance.EpsWatchNext_Recent.Count == 0 &&
                                DashboardVM.Instance.SeriesMissingEps.Count == 0
                                && DashboardVM.Instance.MiniCalendar.Count == 0 &&
                                DashboardVM.Instance.RecommendationsWatch.Count == 0
                                && DashboardVM.Instance.RecommendationsDownload.Count == 0)
                            {
                                tabControl1.IsEnabled = false;
                                this.Cursor = Cursors.Wait;

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
                            if (DashboardMetroVM.Instance.ContinueWatching.Count == 0)
                                dashMetro.RefreshAllData();
                        }

                        if (JMMServerVM.Instance.AllCustomTags.Count == 0) JMMServerVM.Instance.RefreshAllCustomTags();
                    }
                }

                if (tabIndex == TAB_MAIN_Collection)
                {
                    lock (lockCollectionsTab)
                    {
                        if (MainListHelperVM.Instance.AllGroupsDictionary.Count == 0)
                        {
                            MainListHelperVM.Instance.RefreshGroupsSeriesData();
                        }

                        if (MainListHelperVM.Instance.CurrentWrapper == null && lbGroupsSeries.Items.Count == 0)
                        {
                            MainListHelperVM.Instance.SearchTextBox = txtGroupSearch;
                            MainListHelperVM.Instance.CurrentGroupFilter = MainListHelperVM.Instance.AllGroupFilter;
                            MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
                            lbGroupsSeries.SelectedIndex = 0;
                        }
                    }
                    if (JMMServerVM.Instance.AllCustomTags.Count == 0) JMMServerVM.Instance.RefreshAllCustomTags();
                }


                if (tabIndex == TAB_MAIN_FileManger)
                {
                    if (unRecVids.UnrecognisedFiles.Count == 0) unRecVids.RefreshUnrecognisedFiles();
                    if (JMMServerVM.Instance.AllCustomTags.Count == 0) JMMServerVM.Instance.RefreshAllCustomTags();
                }

                if (tabIndex == TAB_MAIN_Playlists)
                {
                    lock (lockPlaylistsTab)
                    {
                        if (PlaylistHelperVM.Instance.Playlists == null ||
                            PlaylistHelperVM.Instance.Playlists.Count == 0) PlaylistHelperVM.Instance.RefreshData();
                        if (lbPlaylists.Items.Count > 0 && lbPlaylists.SelectedIndex < 0)
                            lbPlaylists.SelectedIndex = 0;
                    }

                }

                if (tabIndex == TAB_MAIN_Bookmarks)
                {
                    lock (lockBookmarksTab)
                    {
                        if (MainListHelperVM.Instance.BookmarkedAnime == null ||
                            MainListHelperVM.Instance.BookmarkedAnime.Count == 0)
                            MainListHelperVM.Instance.RefreshBookmarkedAnime();

                        if (ucBookmarks.lbBookmarks.Items.Count > 0)
                            ucBookmarks.lbBookmarks.SelectedIndex = 0;
                    }

                }

                if (tabIndex == TAB_MAIN_Search)
                {
                    lock (lockSearchTab)
                    {
                        if (MainListHelperVM.Instance.AllSeriesDictionary == null ||
                            MainListHelperVM.Instance.AllSeriesDictionary.Count == 0)
                            MainListHelperVM.Instance.RefreshGroupsSeriesData();
                    }
                }

                if (tabIndex == TAB_MAIN_Server)
                {
                    lock (lockServerTab)
                    {
                        if (JMMServerVM.Instance.FolderProviders.Count == 0)
                            JMMServerVM.Instance.RefreshCloudAccounts();
                        if (JMMServerVM.Instance.ImportFolders.Count == 0) JMMServerVM.Instance.RefreshImportFolders();
                    }
                }

                if (tabIndex == TAB_MAIN_Settings)
                {
                    lock (lockSettingsTab)
                    {
                        if (JMMServerVM.Instance.FolderProviders.Count == 0)
                            JMMServerVM.Instance.RefreshCloudAccounts();
                        if (JMMServerVM.Instance.ImportFolders.Count == 0) JMMServerVM.Instance.RefreshImportFolders();
                        if (JMMServerVM.Instance.SelectedLanguages.Count == 0)
                            JMMServerVM.Instance.RefreshNamingLanguages();
                        if (JMMServerVM.Instance.AllUsers.Count == 0) JMMServerVM.Instance.RefreshAllUsers();
                        if (JMMServerVM.Instance.AllTags.Count == 0) JMMServerVM.Instance.RefreshAllTags();
                        if (JMMServerVM.Instance.AllCustomTags.Count == 0) JMMServerVM.Instance.RefreshAllCustomTags();
                    }
                }

                if (tabIndex == TAB_MAIN_Pinned)
                {
                    lock (lockPinnedTab)
                    {
                        if (JMMServerVM.Instance.AllCustomTags.Count == 0) JMMServerVM.Instance.RefreshAllCustomTags();
                    }
                }

                if (tabIndex == TAB_MAIN_Downloads)
                {
                    lock (lockDownloadsTab)
                    {
                        if (UserSettingsVM.Instance.SelectedTorrentSources.Count == 0 ||
                            UserSettingsVM.Instance.UnselectedTorrentSources.Count == 0)
                            UserSettingsVM.Instance.RefreshTorrentSources();
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

                //if (!this.IsLoaded || !JMMServerVM.Instance.UserAuthenticated) return;
                if (!JMMServerVM.Instance.UserAuthenticated) return;


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
                if (e.Source is System.Windows.Controls.TabControl)
                {
                    System.Windows.Controls.TabControl tab = e.Source as System.Windows.Controls.TabControl;
                    if (tab.SelectedIndex == TAB_Settings_Display)
                    {
                        if (JMMServerVM.Instance.SelectedLanguages.Count == 0) JMMServerVM.Instance.RefreshNamingLanguages();
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
            MainListHelperVM.Instance.ViewGroups.SortDescriptions.Clear();
            GroupFilterSorting sortType = GroupFilterHelper.GetEnumForText_Sorting(cboGroupSort.SelectedItem.ToString());
            MainListHelperVM.Instance.ViewGroups.SortDescriptions.Add(GroupFilterHelper.GetSortDescription(sortType, GroupFilterSortDirection.Asc));
        }



        void imageHelper_QueueUpdateEvent(QueueUpdateEventArgs ev)
        {
            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
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
                    result = System.Windows.Forms.MessageBox.Show(JMMClient.Properties.Resources.Language_Info, JMMClient.Properties.Resources.Language_Switch, System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        System.Windows.Forms.Application.Restart();
                        System.Windows.Application.Current.Shutdown();
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
                if (MainListHelperVM.Instance.CurrentWrapper == null)
                    MainListHelperVM.Instance.ShowAllGroups();


                MainListHelperVM.Instance.ViewGroups.Refresh();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private bool GroupSearchFilter(object obj)
        {
            MainListWrapper grp = obj as MainListWrapper;
            if (grp == null) return true;

            if (obj.GetType() != typeof(AnimeGroupVM) && obj.GetType() != typeof(AnimeSeriesVM))
                return true;

            // first evaluate the group filter
            // if the group doesn't match the group filter we won't continue
            if (obj.GetType() == typeof(AnimeGroupVM))
            {
                AnimeGroupVM grpvm = obj as AnimeGroupVM;
                //if (!GroupSearchFilterHelper.EvaluateGroupFilter(MainListHelperVM.Instance.CurrentGroupFilter, grpvm)) return false;

                return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
            }

            if (obj.GetType() == typeof(AnimeSeriesVM))
            {
                AnimeSeriesVM ser = obj as AnimeSeriesVM;
                //if (!GroupSearchFilterHelper.EvaluateGroupFilter(MainListHelperVM.Instance.CurrentGroupFilter, ser)) return false;

                return GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, txtGroupSearch.Text);
            }

            return true;
        }

        void refreshGroupsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            showChildWrappersWorker.RunWorkerAsync(MainListHelperVM.Instance.CurrentWrapper);
        }

        void refreshGroupsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MainListHelperVM.Instance.RefreshGroupsSeriesData();
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
                MainListHelperVM.Instance.ShowChildWrappers(e.Argument as MainListWrapper);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void toggleStatusWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableDisableGroupControls(true);
            this.Cursor = Cursors.Arrow;
        }


        void toggleStatusWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object obj = e.Argument;
                AnimeSeriesVM ser = null;
                bool newStatus = false;

                if (obj.GetType() == typeof(VideoDetailedVM))
                {
                    VideoDetailedVM vid = obj as VideoDetailedVM;
                    newStatus = !vid.Watched;
                    JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                    MainListHelperVM.Instance.UpdateHeirarchy(vid);

                    ser = MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);
                }

                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
                    newStatus = !ep.Watched;
                    JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

                    ser = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
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
            MainListHelperVM.Instance.RefreshGroupsSeriesData();
            MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
            this.Cursor = Cursors.Arrow;

            MainListHelperVM.Instance.ViewGroups.Refresh();
            showChildWrappersWorker.RunWorkerAsync(MainListHelperVM.Instance.CurrentWrapper);
        }

        void moveSeriesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object obj = e.Argument;
                if (obj.GetType() != typeof(MoveSeriesDetails)) return;

                MoveSeriesDetails request = obj as MoveSeriesDetails;
                DateTime start = DateTime.Now;

                //request.UpdatedSeries.Save();
                JMMServerBinary.Contract_AnimeSeries_SaveResponse response =
                    JMMServerVM.Instance.clientBinaryHTTP.MoveSeries(request.UpdatedSeries.AnimeSeriesID.Value, request.UpdatedSeries.AnimeGroupID,
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal, (Action)
                            delegate()
                            {
                                this.Cursor = Cursors.Arrow;
                                MessageBox.Show(response.ErrorMessage);
                            });
                    return;
                }
                else
                {
                    request.UpdatedSeries.Populate(response.AnimeSeries);
                }




                // update all the attached groups

                /*Dictionary<int, JMMServerBinary.Contract_AnimeGroup> grpsDict = new Dictionary<int, JMMServerBinary.Contract_AnimeGroup>();
				List<JMMServerBinary.Contract_AnimeGroup> grps = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveGroupInclusive(request.UpdatedSeries.AnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				List<JMMServerBinary.Contract_AnimeGroup> grpsOld = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveGroupInclusive(request.OldAnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_AnimeGroup tempGrp in grps)
					grpsDict[tempGrp.AnimeGroupID] = tempGrp;

				foreach (JMMServerBinary.Contract_AnimeGroup tempGrp in grpsOld)
					grpsDict[tempGrp.AnimeGroupID] = tempGrp;
				
				foreach (AnimeGroupVM grp in MainListHelperVM.Instance.AllGroups)
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
            List<JMMServerBinary.Contract_AniDBAnime> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();

            int i = 0;
            foreach (JMMServerBinary.Contract_AniDBAnime anime in contracts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadAniDBCover(new AniDB_AnimeVM(anime), false);
                i++;

                //if (i == 80) break;
            }

            // 2. Download posters from TvDB
            List<JMMServerBinary.Contract_TvDB_ImagePoster> posters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBPosters(null);
            foreach (JMMServerBinary.Contract_TvDB_ImagePoster poster in posters)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBPoster(new TvDB_ImagePosterVM(poster), false);
            }

            // 2a. Download posters from MovieDB
            List<JMMServerBinary.Contract_MovieDB_Poster> moviePosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBPosters(null);
            foreach (JMMServerBinary.Contract_MovieDB_Poster poster in moviePosters)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBPoster(new MovieDB_PosterVM(poster), false);
            }

            // 3. Download wide banners from TvDB
            List<JMMServerBinary.Contract_TvDB_ImageWideBanner> banners = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBWideBanners(null);
            foreach (JMMServerBinary.Contract_TvDB_ImageWideBanner banner in banners)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBWideBanner(new TvDB_ImageWideBannerVM(banner), false);
            }

            // 4. Download fanart from TvDB
            List<JMMServerBinary.Contract_TvDB_ImageFanart> fanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBFanart(null);
            foreach (JMMServerBinary.Contract_TvDB_ImageFanart fanart in fanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBFanart(new TvDB_ImageFanartVM(fanart), false);
            }

            // 4a. Download fanart from MovieDB
            List<JMMServerBinary.Contract_MovieDB_Fanart> movieFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBFanart(null);
            foreach (JMMServerBinary.Contract_MovieDB_Fanart fanart in movieFanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBFanart(new MovieDB_FanartVM(fanart), false);
            }

            // 5. Download episode images from TvDB
            List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(null);
            foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBEpisode(new TvDB_EpisodeVM(episode), false);
            }

            // 6. Download posters from Trakt
            List<JMMServerBinary.Contract_Trakt_ImagePoster> traktPosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktPosters(null);
            foreach (JMMServerBinary.Contract_Trakt_ImagePoster traktposter in traktPosters)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktposter.ImageURL)) continue;
                imageHelper.DownloadTraktPoster(new Trakt_ImagePosterVM(traktposter), false);
            }

            // 7. Download fanart from Trakt
            List<JMMServerBinary.Contract_Trakt_ImageFanart> traktFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktFanart(null);
            foreach (JMMServerBinary.Contract_Trakt_ImageFanart traktFanart in traktFanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktFanart.ImageURL)) continue;
                imageHelper.DownloadTraktFanart(new Trakt_ImageFanartVM(traktFanart), false);
            }

            // 8. Download episode images from Trakt
            List<JMMServerBinary.Contract_Trakt_Episode> traktEpisodes = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktEpisodes(null);
            foreach (JMMServerBinary.Contract_Trakt_Episode traktEp in traktEpisodes)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktEp.EpisodeImage)) continue;

                // special case for trak episodes
                // Trakt will return the fanart image when no episode image exists, but we don't want this
                int pos = traktEp.EpisodeImage.IndexOf(@"episodes/");
                if (pos <= 0) continue;

                //logger.Trace("Episode image: {0} - {1}/{2}", traktEp.Trakt_ShowID, traktEp.Season, traktEp.EpisodeNumber);

                imageHelper.DownloadTraktEpisode(new Trakt_EpisodeVM(traktEp), false);
            }
        }

        private void RefreshView()
        {
            if (!JMMServerVM.Instance.ServerOnline) return;

            EnableDisableGroupControls(false);

            try
            {
                this.Cursor = Cursors.Wait;

                // we are look at all the group filters
                if (MainListHelperVM.Instance.CurrentWrapper == null)
                {
                    MainListHelperVM.Instance.SearchTextBox = txtGroupSearch;
                    MainListHelperVM.Instance.CurrentGroupFilter = MainListHelperVM.Instance.AllGroupFilter;

                    //refreshGroupsWorker.RunWorkerAsync(null);

                    MainListHelperVM.Instance.RefreshGroupsSeriesData();
                    DownloadAllImages();

                    MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)HighlightMainListItem);
                }

                // we are inside one of the group filters, groups or series
                if (MainListHelperVM.Instance.CurrentWrapper != null)
                {
                    // refresh the groups and series data
                    refreshGroupsWorker.RunWorkerAsync(null);

                    // refresh the episodes
                    if (lbGroupsSeries.SelectedItem is AnimeSeriesVM)
                    {
                        AnimeSeriesVM ser = lbGroupsSeries.SelectedItem as AnimeSeriesVM;
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
                this.Cursor = Cursors.Arrow;
                EnableDisableGroupControls(true);
            }
        }



        #region Command Bindings

        private void CommandBinding_EditTraktCredentials(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl1.SelectedIndex = TAB_MAIN_Settings;
            tabSettingsChild.SelectedIndex = TAB_Settings_TvDB;
        }

        public void ShowPinnedFileAvDump(VideoLocalVM vid)
        {
            try
            {
                foreach (AVDumpVM dumpTemp in MainListHelperVM.Instance.AVDumpFiles)
                {
                    if (dumpTemp.FullPath == vid.LocalFileSystemFullPath) return;
                }

                AVDumpVM dump = new AVDumpVM(vid);
                MainListHelperVM.Instance.AVDumpFiles.Add(dump);

                tabControl1.SelectedIndex = TAB_MAIN_FileManger;
                tabFileManager.SelectedIndex = TAB_FileManger_Avdump;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowPinnedSeriesOld(AnimeSeriesVM series)
        {
            this.Cursor = Cursors.Wait;

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

                AnimeSeriesVM ser = ctrl.DataContext as AnimeSeriesVM;
                if (ser == null) continue;

                if (ser.AnimeSeriesID == series.AnimeSeriesID)
                {
                    tabControl1.SelectedIndex = TAB_MAIN_Pinned;
                    tabPinned.SelectedIndex = curTab;
                    this.Cursor = Cursors.Arrow;
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

            this.Cursor = Cursors.Arrow;
        }

        public void ShowPinnedSeries(AnimeSeriesVM series, bool isMetroDash = false)
        {
            this.Cursor = Cursors.Wait;

            CloseableTabItem cti = new CloseableTabItem();
            //TabItem cti = new TabItem();

            // if the pinned tab already has this, don't open it again.
            int curTab = -1;
            foreach (object obj in tabPinned.Items)
            {
                curTab++;
                CloseableTabItem ctiTemp = obj as CloseableTabItem;
                if (ctiTemp == null) continue;

                AnimeSeriesVM ser = null;
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

                ser = ctrl.DataContext as AnimeSeriesVM;
                if (ser == null) continue;

                if (ser.AnimeSeriesID == series.AnimeSeriesID)
                {
                    tabControl1.SelectedIndex = TAB_MAIN_Pinned;
                    tabPinned.SelectedIndex = curTab;
                    this.Cursor = Cursors.Arrow;
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

            this.Cursor = Cursors.Arrow;
        }


        public void RefreshPinnedSeries()
        {
            this.Cursor = Cursors.Wait;

            foreach (object obj in tabPinned.Items)
            {
                CloseableTabItem ctiTemp = obj as CloseableTabItem;
                if (ctiTemp == null) continue;

                AnimeSeriesVM ser = null;
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

                ser = ctrl.DataContext as AnimeSeriesVM;
                if (ser == null) continue;

                if (ser.AnimeSeriesID.HasValue)
                    if (MainListHelperVM.Instance.AllSeriesDictionary.TryGetValue(ser.AnimeSeriesID.Value, out ser))
                        ctrl.DataContext = ser;
            }

            this.Cursor = Cursors.Arrow;
        }
        public void RefreshPlayList()
        {
            this.Cursor = Cursors.Wait;

            foreach (object obj in lbPlaylists.Items)
            {
                var playListVM = obj as PlaylistVM;
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
                            if (!itemSer.AnimeSeriesID.HasValue) continue;

                            var itemSeriesId = itemSer.AnimeSeriesID.Value;
                            if (MainListHelperVM.Instance.AllSeriesDictionary.TryGetValue(itemSeriesId, out itemSer))
                                item.Series = itemSer;
                        }
                    }
                }

                var ser = playListVM.Series;
                if (ser == null) continue;
                if (!playListVM.Series.AnimeSeriesID.HasValue) continue;

                var seriesId = playListVM.Series.AnimeSeriesID.Value;
                if (MainListHelperVM.Instance.AllSeriesDictionary.TryGetValue(seriesId, out ser))
                    playListVM.Series = ser;

                //if (playListVM.ite)
            }

            this.Cursor = Cursors.Arrow;
        }

        private void SetColours()
        {
            if (tabControl1.SelectedIndex == TAB_MAIN_Dashboard)
            {
                if (dash.Visibility == System.Windows.Visibility.Visible)
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
            tileContinueWatching.Visibility = System.Windows.Visibility.Collapsed;
            dash.Visibility = System.Windows.Visibility.Collapsed;
            dashMetro.Visibility = System.Windows.Visibility.Collapsed;

            switch (viewType)
            {
                case MetroViews.MainNormal:
                    dash.Visibility = System.Windows.Visibility.Visible;
                    DisplayMainTab(TAB_MAIN_Dashboard);
                    AppSettings.DashboardType = DashboardType.Normal;
                    break;
                case MetroViews.MainMetro:
                    dashMetro.Visibility = System.Windows.Visibility.Visible;
                    DisplayMainTab(TAB_MAIN_Dashboard);
                    AppSettings.DashboardType = DashboardType.Metro;
                    break;
                case MetroViews.ContinueWatching:
                    tileContinueWatching.Visibility = System.Windows.Visibility.Visible;
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
                if (obj.GetType() == typeof(AniDB_AnimeVM) || obj.GetType() == typeof(AniDB_Anime_SimilarVM))
                {
                    AniDB_AnimeVM anime = null;

                    if (obj.GetType() == typeof(AniDB_AnimeVM))
                        anime = (AniDB_AnimeVM)obj;

                    if (obj.GetType() == typeof(AniDB_Anime_SimilarVM))
                        anime = ((AniDB_Anime_SimilarVM)obj).AniDB_Anime;

                    // check if a series already exists
                    bool seriesExists = JMMServerVM.Instance.clientBinaryHTTP.GetSeriesExistingForAnime(anime.AnimeID);
                    if (seriesExists)
                    {
                        MessageBox.Show(Properties.Resources.ERROR_SeriesExists, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VideoLocalVM))
                {
                    VideoLocalVM vid = obj as VideoLocalVM;

                    foreach (AVDumpVM dumpTemp in MainListHelperVM.Instance.AVDumpFiles)
                    {
                        if (dumpTemp.FullPath == vid.LocalFileSystemFullPath) return;
                    }

                    AVDumpVM dump = new AVDumpVM(vid);
                    MainListHelperVM.Instance.AVDumpFiles.Add(dump);

                }

                if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    foreach (VideoLocalVM vid in mv.VideoLocals)
                    {
                        bool alreadyExists = false;
                        foreach (AVDumpVM dumpTemp in MainListHelperVM.Instance.AVDumpFiles)
                        {
                            if (dumpTemp.FullPath == vid.LocalFileSystemFullPath)
                            {
                                alreadyExists = true;
                                break;
                            }

                        }

                        if (alreadyExists) continue;

                        AVDumpVM dump = new AVDumpVM(vid);
                        MainListHelperVM.Instance.AVDumpFiles.Add(dump);
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
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(RecommendationVM))
                {
                    RecommendationVM rec = obj as RecommendationVM;
                    if (rec == null) return;

                    BookmarkedAnimeVM bookmark = new BookmarkedAnimeVM();
                    bookmark.AnimeID = rec.RecommendedAnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        MainListHelperVM.Instance.RefreshBookmarkedAnime();

                }

                if (obj.GetType() == typeof(MissingEpisodeVM))
                {
                    MissingEpisodeVM rec = obj as MissingEpisodeVM;
                    if (rec == null) return;

                    BookmarkedAnimeVM bookmark = new BookmarkedAnimeVM();
                    bookmark.AnimeID = rec.AnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        MainListHelperVM.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(AniDB_AnimeVM))
                {
                    AniDB_AnimeVM rec = obj as AniDB_AnimeVM;
                    if (rec == null) return;

                    BookmarkedAnimeVM bookmark = new BookmarkedAnimeVM();
                    bookmark.AnimeID = rec.AnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        MainListHelperVM.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(AniDB_Anime_SimilarVM))
                {
                    AniDB_Anime_SimilarVM sim = (AniDB_Anime_SimilarVM)obj;

                    BookmarkedAnimeVM bookmark = new BookmarkedAnimeVM();
                    bookmark.AnimeID = sim.SimilarAnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        MainListHelperVM.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(AniDB_Anime_RelationVM))
                {
                    AniDB_Anime_RelationVM rel = (AniDB_Anime_RelationVM)obj;

                    BookmarkedAnimeVM bookmark = new BookmarkedAnimeVM();
                    bookmark.AnimeID = rel.RelatedAnimeID;
                    bookmark.Downloading = 0;
                    bookmark.Notes = "";
                    bookmark.Priority = 1;
                    if (bookmark.Save())
                        MainListHelperVM.Instance.RefreshBookmarkedAnime();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RefreshBookmarks(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            try
            {
                MainListHelperVM.Instance.RefreshBookmarkedAnime();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowTorrentSearch(DownloadSearchCriteria crit)
        {
            this.Cursor = Cursors.Wait;

            tabControl1.SelectedIndex = TAB_MAIN_Downloads;
            tabcDownloads.SelectedIndex = 1;
            ucTorrentSearch.PerformSearch(crit);

            this.Cursor = Cursors.Arrow;
        }

        private void CommandBinding_ShowTorrentSearch(object sender, ExecutedRoutedEventArgs e)
        {
            //object obj = lbGroupsSeries.SelectedItem;
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = (AnimeEpisodeVM)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Episode, ep);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(MissingEpisodeVM))
                {
                    MissingEpisodeVM rec = obj as MissingEpisodeVM;
                    if (rec == null) return;

                    JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodeByAniDBEpisodeID(rec.EpisodeID,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                    if (contract != null)
                    {
                        DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Episode, new AnimeEpisodeVM(contract));
                        ShowTorrentSearch(crit);
                    }
                }

                /*if (obj.GetType() == typeof(MissingFileVM))
				{
					MissingFileVM mis = (MissingFileVM)obj;
					ShowPinnedSeries(mis.AnimeSeries);
				}*/


                if (obj.GetType() == typeof(RecommendationVM))
                {
                    RecommendationVM rec = obj as RecommendationVM;
                    if (rec == null) return;

                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, rec.Recommended_AniDB_Anime);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(AniDB_AnimeVM))
                {
                    AniDB_AnimeVM anime = (AniDB_AnimeVM)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, anime);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(AnimeSeriesVM))
                {
                    AnimeSeriesVM ser = (AnimeSeriesVM)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, ser.AniDB_Anime);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(AniDB_Anime_SimilarVM))
                {
                    AniDB_Anime_SimilarVM sim = (AniDB_Anime_SimilarVM)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, sim.AniDB_Anime);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(AniDB_Anime_RelationVM))
                {
                    AniDB_Anime_RelationVM rel = (AniDB_Anime_RelationVM)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, rel.AniDB_Anime);
                    ShowTorrentSearch(crit);
                }

                if (obj.GetType() == typeof(BookmarkedAnimeVM))
                {
                    BookmarkedAnimeVM ba = (BookmarkedAnimeVM)obj;
                    DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Series, ba.AniDB_Anime);
                    ShowTorrentSearch(crit);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowWebCacheAdmin(AniDB_AnimeVM anime)
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

                if (obj.GetType() == typeof(AniDB_AnimeVM))
                {
                    AniDB_AnimeVM anime = (AniDB_AnimeVM)obj;
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

                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = (AnimeEpisodeVM)obj;
                    objID = ep.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(AnimeSeriesVM))
                {
                    AnimeSeriesVM ser = (AnimeSeriesVM)obj;
                    objID = ser.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(AniDB_Anime_SimilarVM))
                {
                    AniDB_Anime_SimilarVM sim = (AniDB_Anime_SimilarVM)obj;
                    objID = sim.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(AniDB_Anime_RelationVM))
                {
                    AniDB_Anime_RelationVM rel = (AniDB_Anime_RelationVM)obj;
                    objID = rel.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(RecommendationVM))
                {
                    RecommendationVM rec = (RecommendationVM)obj;
                    objID = rec.Recommended_AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(MissingFileVM))
                {
                    MissingFileVM mis = (MissingFileVM)obj;
                    objID = mis.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(MissingEpisodeVM))
                {
                    MissingEpisodeVM misEp = (MissingEpisodeVM)obj;
                    objID = misEp.AnimeSeries.AnimeSeriesID;
                }

                if (obj.GetType() == typeof(PlaylistItemVM))
                {
                    PlaylistItemVM pli = (PlaylistItemVM)obj;
                    if (pli.ItemType == PlaylistItemType.AnimeSeries)
                    {
                        var ser = (AnimeSeriesVM)pli.PlaylistItem;
                        objID = ser.AnimeSeriesID;
                    }
                    else if (pli.ItemType == PlaylistItemType.Episode)
                    {
                        AnimeEpisodeVM ep = pli.PlaylistItem as AnimeEpisodeVM;
                        objID = ep.AnimeSeriesID;
                    }
                }

                if (obj.GetType() == typeof(AnimeSearchVM))
                {
                    AnimeSearchVM search = (AnimeSearchVM)obj;
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

                    AnimeSeriesVM ser;
                    if (MainListHelperVM.Instance.AllSeriesDictionary.TryGetValue(valObjID, out ser) == false)
                    {
                        // get the series
                        JMMServerBinary.Contract_AnimeSeries serContract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(valObjID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                        if (serContract != null)
                        {
                            ser = new AnimeSeriesVM(serContract);
                            MainListHelperVM.Instance.AllSeriesDictionary[valObjID] = ser;
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
                MainListHelperVM.Instance.ShowAllGroups();

                if (e.Parameter is AnimeTagVM)
                {
                    AnimeTagVM obj = e.Parameter as AnimeTagVM;
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
                MainListHelperVM.Instance.MoveBackUpHeirarchy();
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
                if (obj.GetType() == typeof(AnimeGroupVM))
                {
                    AnimeGroupVM grp = (AnimeGroupVM)obj;

                    if (grp.AnimeGroupID.HasValue)
                    {
                        groupBeforeChanges = new AnimeGroupVM();
                        Cloner.Clone(grp, groupBeforeChanges);
                    }

                    grp.IsReadOnly = false;
                    grp.IsBeingEdited = true;

                }

                if (obj.GetType() == typeof(GroupFilterVM))
                {
                    GroupFilterVM gf = (GroupFilterVM)obj;

                    if (gf.GroupFilterID.HasValue && gf.GroupFilterID.Value!=0)
                    {
                        groupFilterBeforeChanges = new GroupFilterVM();
                        groupFilterBeforeChanges.FilterName = gf.FilterName;
                        groupFilterBeforeChanges.BaseCondition = gf.BaseCondition;
                        groupFilterBeforeChanges.ApplyToSeries = gf.ApplyToSeries;
                        groupFilterBeforeChanges.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();
                        groupFilterBeforeChanges.SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();

                        foreach (GroupFilterConditionVM gfc_cur in gf.FilterConditions)
                        {
                            GroupFilterConditionVM gfc = new GroupFilterConditionVM();
                            gfc.ConditionOperator = gfc_cur.ConditionOperator;
                            gfc.ConditionParameter = gfc_cur.ConditionParameter;
                            gfc.ConditionType = gfc_cur.ConditionType;
                            gfc.GroupFilterConditionID = gfc_cur.GroupFilterConditionID;
                            gfc.GroupFilterID = gfc_cur.GroupFilterID;
                            groupFilterBeforeChanges.FilterConditions.Add(gfc);
                        }

                        foreach (GroupFilterSortingCriteria gfcs_cur in gf.SortCriteriaList)
                        {
                            GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
                            gfsc.GroupFilterID = gfcs_cur.GroupFilterID;
                            gfsc.SortDirection = gfcs_cur.SortDirection;
                            gfsc.SortType = gfcs_cur.SortType;
                            groupFilterBeforeChanges.SortCriteriaList.Add(gfsc);
                        }
                        //Cloner.Clone(gf, groupFilterBeforeChanges);
                    }

                    gf.IsLocked = false;
                    gf.IsBeingEdited = true;

                    groupFilterVM = gf;
                    MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
                }

                if (obj.GetType() == typeof(AnimeSeriesVM))
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
                if (obj.GetType() == typeof(AnimeGroupVM))
                {
                    AnimeGroupVM grp = (AnimeGroupVM)obj;
                    bool isnew = !grp.AnimeGroupID.HasValue;
                    if (grp.Validate())
                    {
                        grp.IsReadOnly = true;
                        grp.IsBeingEdited = false;
                        if (grp.Save() && isnew)
                        {
                            MainListHelperVM.Instance.RefreshGroupsSeriesData();
                            MainListHelperVM.Instance.ViewGroups.Refresh();
                            //MainListHelperVM.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;

                            if (!grp.AnimeGroupParentID.HasValue)
                            {
                                // move to all groups
                                // only if it is a top level group
                                MainListHelperVM.Instance.ShowAllGroups();
                                HighlightMainListItem();
                            }
                            else
                            {
                                AnimeGroupVM parentGroup = grp.ParentGroup;
                                if (parentGroup != null)
                                    showChildWrappersWorker.RunWorkerAsync(parentGroup);
                            }
                        }

                    }
                    //BindingExpression be = ccDetail.GetBindingExpression(ContentControl.ContentProperty);
                    //be.UpdateSource();
                }

                if (obj.GetType() == typeof(GroupFilterVM))
                {
                    GroupFilterVM gf = (GroupFilterVM)obj;


					bool isnew = !(gf.GroupFilterID.HasValue && gf.GroupFilterID.Value!=0);
					if (gf.Validate())
					{
						gf.IsLocked = false;
						gf.IsBeingEdited = false;
						if (gf.Save() && isnew)
						{
						    MainListHelperVM.Instance.AllGroupFiltersDictionary.Remove(0);
						    MainListHelperVM.Instance.AllGroupFiltersDictionary.Add(gf.GroupFilterID.Value, gf);
                            gf.GetDirectChildren();
							//MainListHelperVM.Instance.LastGroupFilterID = gf.GroupFilterID.Value;
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
                if (obj.GetType() == typeof(ImportFolderVM))
                {
                    ImportFolderVM fldr = (ImportFolderVM)obj;

                    JMMServerVM.Instance.clientBinaryHTTP.ScanFolder(fldr.ImportFolderID.Value);
                    MessageBox.Show(JMMClient.Properties.Resources.Import_Running, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (obj.GetType() == typeof(GroupFilterVM))
                {
                    GroupFilterVM gf = (GroupFilterVM)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(JMMClient.Properties.Resources.Filter_DeleteGroup, gf.FilterName),
                    JMMClient.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                        foreach (MainListWrapper wrapper in MainListHelperVM.Instance.CurrentWrapperList)
                        {
                            if (wrapper is GroupFilterVM)
                            {
                                GroupFilterVM gfTemp = wrapper as GroupFilterVM;
                                if (gfTemp.GroupFilterID.HasValue && gf.GroupFilterID.Value == gfTemp.GroupFilterID.Value)
                                {
                                    pos = i;
                                    break;
                                }
                            }
                            i++;
                        }

						// remove from group filter list
                        if (gf.GroupFilterID.HasValue && MainListHelperVM.Instance.AllGroupFiltersDictionary.ContainsKey(gf.GroupFilterID.Value))
                            MainListHelperVM.Instance.AllGroupFiltersDictionary.Remove(gf.GroupFilterID.Value);
                        // remove from current wrapper list
                        if (pos >= 0)
						{
							MainListHelperVM.Instance.CurrentWrapperList.RemoveAt(pos);
							//MainListHelperVM.Instance.ViewGroups.Refresh();
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
                if (obj.GetType() == typeof(AnimeGroupVM))
                {
                    AnimeGroupVM grp = (AnimeGroupVM)obj;
                    grp.IsReadOnly = true;
                    grp.IsBeingEdited = false;

                    // copy all editable properties
                    if (grp.AnimeGroupID.HasValue) // an existing group
                    {
                        grp.GroupName = groupBeforeChanges.GroupName;
                        grp.IsFave = groupBeforeChanges.IsFave;

                        //grp.AnimeGroupParentID = groupBeforeChanges.AnimeGroupParentID;
                        grp.Description = groupBeforeChanges.Description;
                        grp.SortName = groupBeforeChanges.SortName;

                        MainListHelperVM.Instance.ViewGroups.Refresh();
                        EnableDisableGroupControls(true);
                        //MainListHelperVM.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;
                        HighlightMainListItem();
                    }
                    else
                    {
                        HighlightMainListItem();
                        SetDetailBinding(null);
                    }


                }

                if (obj.GetType() == typeof(GroupFilterVM))
                {
                    GroupFilterVM gf = (GroupFilterVM)obj;
                    gf.IsLocked = true;
                    gf.IsBeingEdited = false;

                    // copy all editable properties
                    if (gf.GroupFilterID.HasValue && gf.GroupFilterID.Value!=0) // an existing group filter
                    {
                        gf.FilterName = groupFilterBeforeChanges.FilterName;
                        gf.ApplyToSeries = groupFilterBeforeChanges.ApplyToSeries;
                        gf.BaseCondition = groupFilterBeforeChanges.BaseCondition;
                        gf.FilterConditions.Clear();
                        gf.SortCriteriaList.Clear();

                        foreach (GroupFilterConditionVM gfc_old in groupFilterBeforeChanges.FilterConditions)
                        {
                            GroupFilterConditionVM gfc = new GroupFilterConditionVM();
                            gfc.ConditionOperator = gfc_old.ConditionOperator;
                            gfc.ConditionParameter = gfc_old.ConditionParameter;
                            gfc.ConditionType = gfc_old.ConditionType;
                            gfc.GroupFilterConditionID = gfc_old.GroupFilterConditionID;
                            gfc.GroupFilterID = gfc_old.GroupFilterID;
                            gf.FilterConditions.Add(gfc);
                        }

                        foreach (GroupFilterSortingCriteria gfsc_old in groupFilterBeforeChanges.SortCriteriaList)
                        {
                            GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
                            gfsc.GroupFilterID = gfsc_old.GroupFilterID;
                            gfsc.SortDirection = gfsc_old.SortDirection;
                            gfsc.SortType = gfsc_old.SortType;
                            gf.SortCriteriaList.Add(gfsc);
                        }

                        //MainListHelperVM.Instance.LastGroupFilterID = gf.GroupFilterID.Value;
                    }
                    else
                    {
                        SetDetailBinding(null);
                    }
                    EnableDisableGroupControls(true);
                    HighlightMainListItem();
                }

                if (obj.GetType() == typeof(AnimeSeriesVM))
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
                GroupFilterVM gfNew = new GroupFilterVM();
                gfNew.AllowEditing = true;
                gfNew.IsBeingEdited = true;
                gfNew.IsLocked = false;
                gfNew.IsSystemGroupFilter = false;
                gfNew.FilterName = JMMClient.Properties.Resources.Filter_New;
                gfNew.ApplyToSeries = 0;
                gfNew.BaseCondition = (int)GroupFilterBaseCondition.Include;
                gfNew.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();

                MainListHelperVM.Instance.AllGroupFiltersDictionary[0]=gfNew;
                
                groupFilterVM = gfNew;
                MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gfNew);

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
                if (obj.GetType() == typeof(GroupFilterConditionVM))
                {
                    GroupFilterConditionVM gfc = (GroupFilterConditionVM)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(JMMClient.Properties.Resources.Filter_DeleteCondition, gfc.NiceDescription),
                    JMMClient.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        // remove from group list
                        //gfc.Delete();


                        // fund the GroupFilter

                        foreach (GroupFilterVM gf in MainListHelperVM.Instance.AllGroupFiltersDictionary.Values)
                        {
                            if (!gf.AllowEditing) continue; // all filter
                            if (gf.GroupFilterID == gfc.GroupFilterID)
                            {
                                int pos = -1;
                                for (int i = 0; i < gf.FilterConditions.Count; i++)
                                {
                                    if (gfc.ConditionOperator == gf.FilterConditions[i].ConditionOperator &&
                                        gfc.ConditionParameter == gf.FilterConditions[i].ConditionParameter &&
                                        gfc.ConditionType == gf.FilterConditions[i].ConditionType)
                                    {
                                        pos = i;
                                        break;
                                    }
                                }
                                if (pos >= 0)
                                    gf.FilterConditions.RemoveAt(pos);

                                groupFilterVM = gf;
                                MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                                MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
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

                GroupFilterVM gf = (GroupFilterVM)obj;
                GroupFilterConditionVM gfc = new GroupFilterConditionVM();

                GroupFilterConditionForm frm = new GroupFilterConditionForm();
                frm.Owner = this;
                frm.Init(gf, gfc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    gf.FilterConditions.Add(gfc);

                    groupFilterVM = gf;
                    MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
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

                GroupFilterVM gf = (GroupFilterVM)obj;
                GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();

                GroupFilterSortingForm frm = new GroupFilterSortingForm();
                frm.Owner = this;
                frm.Init(gf, gfsc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    gf.SortCriteriaList.Add(gfsc);

                    groupFilterVM = gf;
                    MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
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
                if (obj.GetType() == typeof(GroupFilterSortingCriteria))
                {
                    GroupFilterSortingCriteria gfsc = (GroupFilterSortingCriteria)obj;
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
                if (obj.GetType() == typeof(GroupFilterSortingCriteria))
                {
                    GroupFilterSortingCriteria gfsc = (GroupFilterSortingCriteria)obj;
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
        private void GroupFilterSortMoveUpDown(GroupFilterSortingCriteria gfsc, int direction)
        {
            // find the sorting condition
            foreach (GroupFilterVM gf in MainListHelperVM.Instance.AllGroupFiltersDictionary.Values)
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
                            MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                            MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
                        }
                    }
                    else
                    {
                        if (pos + 1 < gf.SortCriteriaList.Count)
                        {
                            gf.SortCriteriaList.Move(pos, pos + 1);
                            groupFilterVM = gf;
                            MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                            MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
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
                if (obj.GetType() == typeof(GroupFilterSortingCriteria))
                {
                    GroupFilterSortingCriteria gfsc = (GroupFilterSortingCriteria)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(JMMClient.Properties.Resources.Filter_DeleteSort),
                    JMMClient.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        // find the sorting condition
                        foreach (GroupFilterVM gf in MainListHelperVM.Instance.AllGroupFiltersDictionary.Values)
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
                                MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                                MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
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
                AnimeGroupVM grpNew = new AnimeGroupVM();
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
                AnimeGroupVM grp = e.Parameter as AnimeGroupVM;
                if (grp == null) return;

                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
                frm.Owner = this;
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    bool deleteFiles = frm.DeleteFiles;

                    this.Cursor = Cursors.Wait;
                    JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeGroup(grp.AnimeGroupID.Value, deleteFiles);

                    MainListHelperVM.Instance.RefreshGroupsSeriesData();
                    MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
                    SetDetailBinding(null);
                    this.Cursor = Cursors.Arrow;
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
                AnimeGroupVM grp = e.Parameter as AnimeGroupVM;
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
            AnimeGroupVM grp = e.Parameter as AnimeGroupVM;
            if (grp == null) return;

            try
            {
                AnimeGroupVM grpNew = new AnimeGroupVM();
                grpNew.IsReadOnly = false;
                grpNew.IsBeingEdited = true;
                grpNew.AnimeGroupParentID = grp.AnimeGroupID.Value;
                SetDetailBinding(grpNew);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_AddPlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            PlaylistHelperVM.CreatePlaylist(this);
        }

        private void CommandBinding_DeletePlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            PlaylistVM pl = e.Parameter as PlaylistVM;
            if (pl == null) return;



            try
            {
                MessageBoxResult res = MessageBox.Show(string.Format(JMMClient.Properties.Resources.Playlist_Delete, pl.PlaylistName),
                    JMMClient.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    this.Cursor = Cursors.Wait;

                    if (pl.PlaylistID.HasValue)
                    {
                        string msg = JMMServerVM.Instance.clientBinaryHTTP.DeletePlaylist(pl.PlaylistID.Value);
                        if (!string.IsNullOrEmpty(msg))
                            Utils.ShowErrorMessage(msg);
                    }

                    SetDetailBindingPlaylist(null);

                    // refresh data
                    PlaylistHelperVM.Instance.RefreshData();
                    if (lbPlaylists.Items.Count > 0)
                        lbPlaylists.SelectedIndex = 0;



                    this.Cursor = Cursors.Arrow;
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
                PlaylistItemVM pli = e.Parameter as PlaylistItemVM;
                if (pli == null) return;

                this.Cursor = Cursors.Wait;

                // get the playlist
                JMMServerBinary.Contract_Playlist plContract = JMMServerVM.Instance.clientBinaryHTTP.GetPlaylist(pli.PlaylistID);
                if (plContract == null)
                {
                    this.Cursor = Cursors.Arrow;
                    MessageBox.Show(JMMClient.Properties.Resources.Filter_PlaylistMissing, JMMClient.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                PlaylistVM pl = new PlaylistVM(plContract);

                if (pli.ItemType == PlaylistItemType.AnimeSeries)
                {
                    AnimeSeriesVM ser = pli.PlaylistItem as AnimeSeriesVM;
                    pl.RemoveSeries(ser.AnimeSeriesID.Value);
                }
                if (pli.ItemType == PlaylistItemType.Episode)
                {
                    AnimeEpisodeVM ep = pli.PlaylistItem as AnimeEpisodeVM;
                    pl.RemoveEpisode(ep.AnimeEpisodeID);
                }

                pl.Save();
                plContract = JMMServerVM.Instance.clientBinaryHTTP.GetPlaylist(pli.PlaylistID);

                // refresh data
                if (lbPlaylists.Items.Count > 0)
                {
                    // get the current playlist
                    PlaylistVM selPL = lbPlaylists.SelectedItem as PlaylistVM;
                    if (selPL != null && plContract != null && selPL.PlaylistID == plContract.PlaylistID)
                    {
                        selPL.Populate(plContract);
                        selPL.PopulatePlaylistObjects();
                        PlaylistHelperVM.Instance.OnPlaylistModified(new PlaylistModifiedEventArgs(plContract.PlaylistID.Value));
                    }
                    else
                    {
                        PlaylistHelperVM.Instance.RefreshData();
                    }
                }

                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RefreshPlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // refresh data
                PlaylistHelperVM.Instance.RefreshData();
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
            UserSettingsVM.Instance.SeriesGroup_Image_Height = UserSettingsVM.Instance.SeriesGroup_Image_Height + 10;
        }

        private void CommandBinding_DecrementSeriesImageSize(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettingsVM.Instance.SeriesGroup_Image_Height = UserSettingsVM.Instance.SeriesGroup_Image_Height - 10;
        }

        private void CommandBinding_NewSeries(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private int prevgf = -1;
        private void CommandBinding_DeleteSeries(object sender, ExecutedRoutedEventArgs e)
        {
            AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
            if (ser == null) return;

            try
            {
                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
                frm.Owner = this;
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    this.Cursor = Cursors.Wait;

                    JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeSeries(ser.AnimeSeriesID.Value, frm.DeleteFiles, frm.DeleteGroups);
                    MainListHelperVM.Instance.RefreshGroupsSeriesData();
                    MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
                    SetDetailBinding(null);
                    this.Cursor = Cursors.Arrow;

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }
 

        private void CommandBinding_MoveSeries(object sender, ExecutedRoutedEventArgs e)
        {
            AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
            if (ser == null) return;

            try
            {
                MoveSeries frm = new MoveSeries();
                frm.Owner = this;
                frm.Init(ser);
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    AnimeGroupVM grpSelected = frm.SelectedGroup;
                    if (grpSelected == null) return;

                    MoveSeriesDetails request = new MoveSeriesDetails();
                    request.OldAnimeGroupID = ser.AnimeGroupID;

                    ser.AnimeGroupID = grpSelected.AnimeGroupID.Value;
                    request.UpdatedSeries = ser;

                    this.Cursor = Cursors.Wait;
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
                JMMServerVM.Instance.RunImport();
                MessageBox.Show(JMMClient.Properties.Resources.Import_Running, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.Main_RunProcess),
                    Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    JMMServerVM.Instance.RemoveMissingFiles();
                    MessageBox.Show(Properties.Resources.Process_Running, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
                JMMServerVM.Instance.SyncMyList();
                MessageBox.Show(JMMClient.Properties.Resources.Process_Running, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
                JMMServerVM.Instance.SyncVotes();
                MessageBox.Show(JMMClient.Properties.Resources.Process_Running, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
                JMMServerVM.Instance.clientBinaryHTTP.SyncMALUpload();
                MessageBox.Show(JMMClient.Properties.Resources.Process_Queued, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
                JMMServerVM.Instance.clientBinaryHTTP.SyncMALDownload();
                MessageBox.Show(JMMClient.Properties.Resources.Process_Queued, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RevokeVote(object sender, ExecutedRoutedEventArgs e)
        {
            AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
            if (ser == null) return;

            try
            {
                JMMServerVM.Instance.RevokeVote(ser.AniDB_ID);

                // refresh the data
                //ser.RefreshBase();
                //ser.AniDB_Anime.Detail.RefreshBase();

                MainListHelperVM.Instance.UpdateHeirarchy(ser);

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

            this.Cursor = Cursors.Wait;
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
                    MainListHelperVM.Instance.ShowChildWrappers(null);
                }
                if (e.Parameter is MainListWrapper)
                {
                    MainListHelperVM.Instance.ShowChildWrappers(e.Parameter as MainListWrapper);
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

            if (obj.GetType() == typeof(AnimeGroupVM))
            {
                AnimeGroupVM grp = (AnimeGroupVM)obj;
                grp.IsFave = grp.IsFave == 1 ? 0 : 1;

                // the user can toggle the fave without going into edit mode
                if (grp.IsReadOnly)
                    grp.Save();


                //BindingExpression be = ccDetail.GetBindingExpression(ContentControl.ContentProperty);
                //be.UpdateSource();
            }

            if (obj.GetType() == typeof(AnimeSeriesVM))
            {
                AnimeSeriesVM ser = (AnimeSeriesVM)obj;
                AnimeGroupVM grp = ser.TopLevelAnimeGroup;
                if (grp == null) return;

                grp.IsFave = grp.IsFave == 1 ? 0 : 1;
                grp.Save();

                ser.PopulateIsFave();

                MainListHelperVM.Instance.UpdateHeirarchy(ser);
            }
        }

        private void CommandBinding_ToggleExpandTags(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettingsVM.Instance.TagsExpanded = !UserSettingsVM.Instance.TagsExpanded;
        }

        private void CommandBinding_ToggleExpandTitles(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettingsVM.Instance.TitlesExpanded = !UserSettingsVM.Instance.TitlesExpanded;
        }

        private void SetWindowFullscreen()
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            //this.Topmost = true;
            this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void CommandBinding_WindowFullScreen(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettingsVM.Instance.WindowFullScreen = true;
            UserSettingsVM.Instance.WindowNormal = false;
            SetWindowFullscreen();
        }

        private void SetWindowNormal()
        {
            this.WindowState = AppSettings.DefaultWindowState;
            this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
        }

        private void CommandBinding_WindowNormal(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettingsVM.Instance.WindowFullScreen = false;
            UserSettingsVM.Instance.WindowNormal = true;
            SetWindowNormal();
        }

        private void CommandBinding_WindowClose(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBinding_WindowMinimize(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }


        #region Server Queue Actions

        private void CommandBinding_HasherQueuePause(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorHasherPaused(true);
                JMMServerVM.Instance.UpdateServerStatus();
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
                JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorHasherPaused(false);
                JMMServerVM.Instance.UpdateServerStatus();
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
                JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorGeneralPaused(true);
                JMMServerVM.Instance.UpdateServerStatus();
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
                JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorGeneralPaused(false);
                JMMServerVM.Instance.UpdateServerStatus();
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
                JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorImagesPaused(true);
                JMMServerVM.Instance.UpdateServerStatus();
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
                JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorImagesPaused(false);
                JMMServerVM.Instance.UpdateServerStatus();
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
            AnimeGroupVM grpvm = obj as AnimeGroupVM;
            if (grpvm == null) return false;
            return GroupSearchFilterHelper.EvaluateGroupFilter(groupFilterVM, grpvm);
        }

        void lbPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetDetailBindingPlaylist(null);

                System.Windows.Controls.ListBox lb = (System.Windows.Controls.ListBox)sender;

                object obj = lb.SelectedItem;
                if (obj == null) return;

                if (obj.GetType() == typeof(PlaylistVM))
                {
                    this.Cursor = Cursors.Wait;
                    PlaylistVM pl = obj as PlaylistVM;
                    pl.PopulatePlaylistObjects();
                    //series.RefreshBase();
                    //MainListHelperVM.Instance.LastAnimeSeriesID = series.AnimeSeriesID.Value;
                    //MainListHelperVM.Instance.CurrentSeries = series;
                }


                SetDetailBindingPlaylist(obj);
                this.Cursor = Cursors.Arrow;

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

                System.Windows.Controls.ListBox lb = (System.Windows.Controls.ListBox)sender;

                object obj = lb.SelectedItem;
                if (obj == null) return;

                if (obj.GetType() == typeof(AnimeSeriesVM))
                {

                    AnimeSeriesVM series = obj as AnimeSeriesVM;
                    series.RefreshBase();
                    //MainListHelperVM.Instance.LastAnimeSeriesID = series.AnimeSeriesID.Value;
                    MainListHelperVM.Instance.CurrentSeries = series;
                }

                if (obj.GetType() == typeof(AnimeGroupVM))
                {
                    AnimeGroupVM grp = obj as AnimeGroupVM;
                    //MainListHelperVM.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;
                }

                if (obj.GetType() == typeof(GroupFilterVM))
                {
                    GroupFilterVM gf = obj as GroupFilterVM;
                    //MainListHelperVM.Instance.LastGroupFilterID = gf.GroupFilterID.Value;

                    groupFilterVM = gf;
                    MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
                }

                if (!string.IsNullOrEmpty(MainListHelperVM.Instance.CurrentOpenGroupFilter) && lbGroupsSeries.SelectedItem != null)
                    MainListHelperVM.Instance.LastGroupForGF[MainListHelperVM.Instance.CurrentOpenGroupFilter] = lbGroupsSeries.SelectedIndex;

                //SetDetailBinding(MainListHelperVM.Instance.AllGroups[0]);
                SetDetailBinding(obj);

                if (obj.GetType() == typeof(AnimeSeriesVM))
                {
                    AnimeSeriesVM series = obj as AnimeSeriesVM;
                    //epListMain.DataContext = series;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowChildrenForCurrentGroup(AnimeSeriesVM ser)
        {
            if (lbGroupsSeries.SelectedItem == null) return;

            if (lbGroupsSeries.SelectedItem is MainListWrapper)
            {
                // this is the last supported drill down
                if (lbGroupsSeries.SelectedItem.GetType() == typeof(AnimeSeriesVM)) return;

                //MainListHelperVM.Instance.LastAnimeSeriesID = ser.AnimeSeriesID.Value;

                EnableDisableGroupControls(false);
                showChildWrappersWorker.RunWorkerAsync(lbGroupsSeries.SelectedItem);
            }
        }

        void lbGroupsSeries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            var source = e.Source as FrameworkElement;

            if (object.Equals(originalSource.DataContext, source.DataContext)) //both null, or both what ever the scrollbar and the listview/treeview
                return;

            try
            {
                var selItem = lbGroupsSeries.SelectedItem;
                if (selItem == null) return;

                var animeGroup = selItem as AnimeGroupVM;
                if (animeGroup != null)
                    MainListHelperVM.Instance.CurrentOpenGroupFilter = "AnimeGroupVM|" + animeGroup.AnimeGroupID.GetValueOrDefault(int.MinValue);

                var groupFilter = selItem as GroupFilterVM;
                if (groupFilter != null)
                    MainListHelperVM.Instance.CurrentOpenGroupFilter = "GroupFilterVM|" + groupFilter.GroupFilterID.GetValueOrDefault(int.MinValue);

                if (selItem is MainListWrapper)
                {
                    //SetDetailBinding(null);
                    // this is the last supported drill down
                    if (selItem.GetType() == typeof(AnimeSeriesVM)) return;

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
                if (MainListHelperVM.Instance.LastGroupForGF.ContainsKey(MainListHelperVM.Instance.CurrentOpenGroupFilter))
                {
                    int lastSelIndex = MainListHelperVM.Instance.LastGroupForGF[MainListHelperVM.Instance.CurrentOpenGroupFilter];
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

                if (objToBind != null && objToBind.GetType().Equals(typeof(AnimeSeriesVM)))
                {
                    AnimeSeriesVM ser = objToBind as AnimeSeriesVM;
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
                ccDetail.SetBinding(ContentControl.ContentProperty, b);
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
                ccPlaylist.SetBinding(ContentControl.ContentProperty, b);
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
