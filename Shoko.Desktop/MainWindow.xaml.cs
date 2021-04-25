using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Infralution.Localization.Wpf;
using NLog;
using Shoko.Commons;
using Shoko.Commons.Extensions;
using Shoko.Desktop.AutoUpdates;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.UserControls;
using Shoko.Desktop.UserControls.Community;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.VideoPlayers;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Application = System.Windows.Forms.Application;
using Binding = System.Windows.Data.Binding;
using Cursors = System.Windows.Input.Cursors;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using PlaylistItemType = Shoko.Models.Enums.PlaylistItemType;
using TabControl = System.Windows.Controls.TabControl;
using Timer = System.Timers.Timer;

namespace Shoko.Desktop
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public enum TAB_MAIN
        {
            Dashboard = 0,
            Collection,
            Playlists,
            Bookmarks,
            Server,
            FileManger,
            Settings,
            Pinned,
            Search,
            Help,
            Community
        }

        public static int CurrentMainTabIndex = (int) TAB_MAIN.Dashboard;

        private enum TAB_UTILITY
        {
            Unrecognised = 0,
            Ignored,
            ManuallyLinked,
            DuplicateFiles,
            MultipleFiles,
            MissingMyList,
            SeriesNoFiles,
            MissingEps,
            Recommendations,
            IgnoredAnime,
            Avdump,
            FileSearch,
            Rename,
            UpdateData,
            Rankings
        }

        public enum TAB_Settings
        {
            Essential = 0,
            AniDB,
            TvDB,
            WebCache,
            Display
        }

        private static Timer postStartTimer;

        public static VM_GroupFilter groupFilterVM;
        public static List<UserCulture> userLanguages = new List<UserCulture>();
        public static ImageDownloader imageHelper;

        private VM_AnimeGroup_User groupBeforeChanges;
        private VM_GroupFilter groupFilterBeforeChanges;

#pragma warning disable 414
        private int lastFileManagerTab = (int) TAB_UTILITY.Unrecognised;
#pragma warning restore 414

        private readonly BackgroundWorker showChildWrappersWorker = new BackgroundWorker();
        private readonly BackgroundWorker refreshGroupsWorker = new BackgroundWorker();
        private readonly BackgroundWorker downloadImagesWorker = new BackgroundWorker();
        private readonly BackgroundWorker toggleStatusWorker = new BackgroundWorker();
        private readonly BackgroundWorker moveSeriesWorker = new BackgroundWorker();

        private readonly BackgroundWorker showDashboardWorker = new BackgroundWorker();

        // Locks
        private readonly object lockDashBoardTab = new object();
        private readonly object lockCollectionsTab = new object();
        private readonly object lockPlaylistsTab = new object();
        private readonly object lockBookmarksTab = new object();
        private readonly object lockServerTab = new object();
        private readonly object lockUtilitiesTab = new object();
        private readonly object lockSettingsTab = new object();
        private readonly object lockPinnedTab = new object();
        private readonly object lockDownloadsTab = new object();
        private readonly object lockSearchTab = new object();

        public static VideoHandler videoHandler = new VideoHandler();
        private bool _blockTabControlChanged;

        public MainWindow()
        {
            try
            {
                AppSettings.LoadSettings();
                logger.Info("App startup - Loaded settings");

                InitializeComponent();
                
                FolderMappings.Instance.SetLoadAndSaveCallback(AppSettings.GetMappings,AppSettings.SetMappings);
                // Set application startup culture based on config settings
                logger.Info("App startup - Culture set up");
                try
                {
                    string culture = AppSettings.Culture;
                    CultureInfo ci = new CultureInfo(culture);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }
                catch (Exception cultEx)
                {
                    logger.Error($"Error settings application culture: {AppSettings.Culture}, {cultEx}");
                }
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

                Closing += (o, args) => ImageDownloader.Stopping = true;

                lbGroupsSeries.MouseDoubleClick += lbGroupsSeries_MouseDoubleClick;
                lbGroupsSeries.SelectionChanged += lbGroupsSeries_SelectionChanged;
                grdMain.LayoutUpdated += grdMain_LayoutUpdated;
                LayoutUpdated += MainWindow_LayoutUpdated;

                lbPlaylists.SelectionChanged += lbPlaylists_SelectionChanged;



                showChildWrappersWorker.DoWork += showChildWrappersWorker_DoWork;
                showChildWrappersWorker.RunWorkerCompleted += showChildWrappersWorker_RunWorkerCompleted;

                downloadImagesWorker.DoWork += downloadImagesWorker_DoWork;

                refreshGroupsWorker.DoWork += refreshGroupsWorker_DoWork;
                refreshGroupsWorker.RunWorkerCompleted += refreshGroupsWorker_RunWorkerCompleted;

                toggleStatusWorker.DoWork += toggleStatusWorker_DoWork;
                toggleStatusWorker.RunWorkerCompleted += toggleStatusWorker_RunWorkerCompleted;

                moveSeriesWorker.DoWork += moveSeriesWorker_DoWork;
                moveSeriesWorker.RunWorkerCompleted += moveSeriesWorker_RunWorkerCompleted;

                txtGroupSearch.TextChanged += txtGroupSearch_TextChanged;

                showDashboardWorker.DoWork += showDashboardWorker_DoWork;
                showDashboardWorker.RunWorkerCompleted += showDashboardWorker_RunWorkerCompleted;
                showDashboardWorker.WorkerSupportsCancellation = true;

                VM_MainListHelper.Instance.ViewGroups.Filter = GroupSearchFilter;
                cboLanguages.SelectionChanged += cboLanguages_SelectionChanged;

                if (VM_MainListHelper.Instance.SeriesSearchTextBox == null) VM_MainListHelper.Instance.SeriesSearchTextBox = seriesSearch.txtSeriesSearch;

                imageHelper = new ImageDownloader();
                imageHelper.Init();

                videoHandler.Init();

                InitCulture();

                imageHelper.QueueUpdateEvent += imageHelper_QueueUpdateEvent;

                cboGroupSort.Items.Clear();
                foreach (string sType in Commons.Extensions.Models.GetAllSortTypes())
                    cboGroupSort.Items.Add(sType);
                cboGroupSort.SelectedIndex = 0;
                btnToolbarSort.Click += btnToolbarSort_Click;

                tabControl1.SelectionChanged += tabControl1_SelectionChanged;
                tabFileManager.SelectionChanged += tabFileManager_SelectionChanged;
                tabSettingsChild.SelectionChanged += tabSettingsChild_SelectionChanged;

                Loaded += MainWindow_Loaded;
                StateChanged += MainWindow_StateChanged;

                AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(CloseTab));

                btnUpdateMediaInfo.Click += btnUpdateMediaInfo_Click;
                btnFeed.Click += btnFeed_Click;
                btnDiscord.Click += btnDiscord_Click;
                btnAbout.Click += btnAbout_Click;
                btnClearHasherQueue.Click += btnClearHasherQueue_Click;
                btnClearGeneralQueue.Click += btnClearGeneralQueue_Click;
                btnClearServerImageQueue.Click += btnClearServerImageQueue_Click;
                btnAdminMessages.Click += btnAdminMessages_Click;

                // timer for automatic updates
                postStartTimer = new Timer();
                postStartTimer.AutoReset = false;
                postStartTimer.Interval = 5 * 1000; // 15 seconds
                postStartTimer.Elapsed += postStartTimer_Elapsed;

                btnSwitchUser.Click += btnSwitchUser_Click;

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
            if (!_blockTabControlChanged) return;
            int previousIndex = tabControl1.Items.IndexOf(tabControl1.SelectedContent);
            tabControl1.SelectedIndex = previousIndex;

            e.Cancel = true;
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

       private void btnSwitchUser_Click(object sender, RoutedEventArgs e)
       {
           // authenticate user
           if (!VM_ShokoServer.Instance.ServerOnline) return;
           if (!VM_ShokoServer.Instance.AuthenticateUser()) return;
           VM_MainListHelper.Instance.ClearData();
           VM_MainListHelper.Instance.ShowChildWrappers(null);

           RecentAdditionsType addType = RecentAdditionsType.Episode;
           switch (dash.cboDashRecentAdditionsType.SelectedIndex)
           {
               case 0:
                   addType = RecentAdditionsType.Episode;
                   break;
               case 1:
                   addType = RecentAdditionsType.Series;
                   break;
           }

           RefreshOptions opt = new RefreshOptions
           {
               RecentAdditionType = addType,
               RefreshRecentAdditions = true,
               RefreshContinueWatching = true,
               RefreshOtherWidgets = true
           };

           // Check if worker is busy and cancel if needed
           if (showDashboardWorker.IsBusy)
               showDashboardWorker.CancelAsync();

           if (!showDashboardWorker.IsBusy)
               showDashboardWorker.RunWorkerAsync(opt);
           else
               logger.Error("Failed to start showDashboardWorker for btnSwitchUser");

           tabControl1.SelectedIndex = (int) TAB_MAIN.Dashboard;
       }

        private void btnClearServerImageQueue_Click(object sender, RoutedEventArgs e)
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

        private void btnClearGeneralQueue_Click(object sender, RoutedEventArgs e)
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

        private void btnClearHasherQueue_Click(object sender, RoutedEventArgs e)
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

        private void btnFeed_Click(object sender, RoutedEventArgs e)
        {
            FeedForm frm = new FeedForm();
            frm.Owner = this;
            frm.ShowDialog();
        }

        private void btnDiscord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://discord.gg/0XKJW7TObKLajoKc");
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage($"Unable to open link: {ex.Message}", ex);
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutForm frm = new AboutForm {Owner = this};
            frm.ShowDialog();
        }

        private void btnAdminMessages_Click(object sender, RoutedEventArgs e)
        {
            AdminMessagesForm frm = new AdminMessagesForm {Owner = this};
            frm.ShowDialog();
        }

        private void btnUpdateMediaInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.RefreshAllMediaInfo();
                MessageBox.Show(Commons.Properties.Resources.Main_ProcessRunning, Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CloseTab(object source, RoutedEventArgs args)
        {
            if (!(args.Source is TabItem tabItem)) return;
            if (tabItem.Parent is TabControl tabControl)
                tabControl.Items.Remove(tabItem);
        }


        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
                AppSettings.DefaultWindowState = WindowState;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppSettings.WindowFullScreen)
                SetWindowFullscreen();
            else
                SetWindowNormal();

            Utils.ClearAutoUpdateCache();

            // validate settings
            VM_ShokoServer.Instance.Test();
            VM_ShokoServer.Instance.BaseImagePath = Utils.GetBaseImagesPath();

            // Make the queue tooltip align left
            QueueTooltip.HorizontalOffset = QueueTooltip.Width - QueuePanel.Width;

            bool loggedIn = false;
            if (VM_ShokoServer.Instance.ServerOnline)
                loggedIn = VM_ShokoServer.Instance.LoginAsLastUser();

            if (!loggedIn)
                if (VM_ShokoServer.Instance.ServerOnline && !VM_ShokoServer.Instance.AuthenticateUser())
                {
                    Close();
                    return;
                }

            if (VM_ShokoServer.Instance.ServerOnline)
            {
                tabControl1.SelectedIndex = (int) TAB_MAIN.Dashboard;
                DisplayMainTab((int) TAB_MAIN.Dashboard);
                DownloadAllImages();
            }
            else
                tabControl1.SelectedIndex = (int) TAB_MAIN.Settings;


            Assembly a = Assembly.GetExecutingAssembly();
            VM_ShokoServer.Instance.ApplicationVersion = Utils.GetApplicationVersion(a);

            postStartTimer.Start();

            CheckForUpdates(false);

            var collView = CollectionViewSource.GetDefaultView(tabControl1.Items);
            collView.CurrentChanging += CollView_CurrentChanging;
        }

        private void postStartTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            postStartTimer.Stop();

            if (VM_ShokoServer.Instance.ServerOnline)
                DownloadAllImages();
        }

        public void CheckForUpdates(bool forceShowForm)
        {
            try
            {
                // get the user's version
                Assembly a = Assembly.GetExecutingAssembly();
                AssemblyName an = a.GetName();

                var verNew = ShokoAutoUpdatesHelper.ConvertToAbsoluteVersion(
                    ShokoAutoUpdatesHelper.GetLatestVersionNumber(AppSettings.UpdateChannel));

                //verNew = verInfo.versions.DesktopVersionAbs;
                long verCurrent = (an.Version.Revision * 100) +
                                  (an.Version.Build * 100 * 100) +
                                  (an.Version.Minor * 100 * 100 * 100) +
                                  (an.Version.Major * 100 * 100 * 100 * 100);

                if (forceShowForm || verNew > verCurrent)
                {
                    UpdateForm frm = new UpdateForm {Owner = this};
                    frm.ShowDialog();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }

        }


        private void MainWindow_LayoutUpdated(object sender, EventArgs e)
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
                if (tempWidth > 0)
                    VM_MainListHelper.Instance.MainScrollerWidth = tempWidth;

                tempWidth = tabControl1.ActualWidth - 20;
                //tempWidth = tabControl1.ActualWidth - 300;
                if (tempWidth > 0)
                    VM_MainListHelper.Instance.FullScrollerWidth = tempWidth;

                var tempHeight = tabControl1.ActualHeight - 50;
                if (tempHeight > 0)
                    VM_MainListHelper.Instance.FullScrollerHeight = tempHeight;

                tempWidth = ccPlaylist.ActualWidth - 8;
                if (tempWidth > 0)
                    VM_MainListHelper.Instance.PlaylistWidth = tempWidth;

                if (tempWidth > 0)
                    VM_MainListHelper.Instance.DownloadRecScrollerWidth = tempWidth;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        private void tabFileManager_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!(e.Source is TabControl)) return;
                TabControl tab = e.Source as TabControl;
                Cursor = Cursors.Wait;
                switch (tab.SelectedIndex)
                {
                    case (int) TAB_UTILITY.Unrecognised:
                        if (unRecVids.UnrecognisedFiles.Count == 0)
                            unRecVids.RefreshUnrecognisedFiles();
                        if (0==unRecVids.AllSeries.Count && string.IsNullOrEmpty(unRecVids.txtSeriesSearch.Text))
                            unRecVids.RefreshSeries(true); //inital all load
                        lastFileManagerTab = (int) TAB_UTILITY.Unrecognised;
                        break;
                    case (int) TAB_UTILITY.Ignored:
                        if (ignoredFiles.IgnoredFilesCollection.Count == 0) ignoredFiles.RefreshIgnoredFiles();
                        lastFileManagerTab = (int) TAB_UTILITY.Ignored;
                        break;
                    case (int) TAB_UTILITY.ManuallyLinked:
                        if (linkedFiles.ManuallyLinkedFiles.Count == 0) linkedFiles.RefreshLinkedFiles();
                        lastFileManagerTab = (int) TAB_UTILITY.ManuallyLinked;
                        break;
                    case (int) TAB_UTILITY.DuplicateFiles:
                        if (duplicateFiles.DuplicateFilesCollection.Count == 0) duplicateFiles.RefreshDuplicateFiles();
                        lastFileManagerTab = (int) TAB_UTILITY.DuplicateFiles;
                        break;
                    case (int) TAB_UTILITY.MultipleFiles:
                        lastFileManagerTab = (int) TAB_UTILITY.MultipleFiles;
                        break;
                    case (int) TAB_UTILITY.Rename:
                        if (fileRenaming.RenameScripts.Count == 0) fileRenaming.RefreshScripts();
                        lastFileManagerTab = (int) TAB_UTILITY.Rename;
                        break;
                    case (int) TAB_UTILITY.Rankings:
                        if (rankings.AllAnime.Count == 0) rankings.Init();
                        lastFileManagerTab = (int) TAB_UTILITY.Rankings;
                        break;
                }

                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void showDashboardWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshOptions opt = e.Argument as RefreshOptions;

            VM_Dashboard.Instance.RefreshData(opt.RefreshContinueWatching, opt.RefreshRecentAdditions,
                opt.RefreshOtherWidgets, opt.RecentAdditionType);
        }

        private void showDashboardWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Arrow;
            tabControl1.IsEnabled = true;
        }

        private void DisplayMainTab(int tabIndex)
        {
            try
            {
                CurrentMainTabIndex = tabIndex;

                switch ((TAB_MAIN) tabIndex)
                {
                    case TAB_MAIN.Dashboard:
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
                                    switch (dash.cboDashRecentAdditionsType.SelectedIndex)
                                    {
                                        case 0:
                                            addType = RecentAdditionsType.Episode;
                                            break;
                                        case 1:
                                            addType = RecentAdditionsType.Series;
                                            break;
                                    }

                                    RefreshOptions opt = new RefreshOptions
                                    {
                                        RecentAdditionType = addType,
                                        RefreshRecentAdditions = true,
                                        RefreshContinueWatching = true,
                                        RefreshOtherWidgets = true
                                    };

                                    // Check if worker is busy and cancel if needed
                                    if (showDashboardWorker.IsBusy)
                                        showDashboardWorker.CancelAsync();

                                    if (!showDashboardWorker.IsBusy)
                                        showDashboardWorker.RunWorkerAsync(opt);
                                    else
                                        logger.Error("Failed to start showDashboardWorker for TAB_MAIN.Dashboard");
                                }
                            }
                            else
                            {
                                if (VM_DashboardMetro.Instance.ContinueWatching.Count == 0)
                                    dashMetro.RefreshAllData();
                            }

                            if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                        }
                        break;
                    case TAB_MAIN.Collection:
                        lock (lockCollectionsTab)
                        {
                            if (VM_MainListHelper.Instance.AllGroupsDictionary.Count == 0)
                                VM_MainListHelper.Instance.RefreshGroupsSeriesData();

                            if (VM_MainListHelper.Instance.CurrentWrapper == null && lbGroupsSeries.Items.Count == 0)
                            {
                                VM_MainListHelper.Instance.SearchTextBox = txtGroupSearch;
                                VM_MainListHelper.Instance.CurrentGroupFilter = VM_MainListHelper.Instance.AllGroupFilter;
                                VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
                                lbGroupsSeries.SelectedIndex = 0;
                            }
                        }
                        if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                        break;
                    case TAB_MAIN.FileManger:
                        if (unRecVids.UnrecognisedFiles.Count == 0) unRecVids.RefreshUnrecognisedFiles();
                        if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                        break;
                    case TAB_MAIN.Playlists:
                        lock (lockPlaylistsTab)
                        {
                            if (VM_PlaylistHelper.Instance.Playlists == null ||
                                VM_PlaylistHelper.Instance.Playlists.Count == 0) VM_PlaylistHelper.Instance.RefreshData();
                            if (lbPlaylists.Items.Count > 0 && lbPlaylists.SelectedIndex < 0)
                                lbPlaylists.SelectedIndex = 0;
                        }
                        break;
                    case TAB_MAIN.Bookmarks:
                        lock (lockBookmarksTab)
                        {
                            if (VM_MainListHelper.Instance.BookmarkedAnime == null ||
                                VM_MainListHelper.Instance.BookmarkedAnime.Count == 0)
                                VM_MainListHelper.Instance.RefreshBookmarkedAnime();

                            if (ucBookmarks.lbBookmarks.Items.Count > 0)
                                ucBookmarks.lbBookmarks.SelectedIndex = 0;
                        }
                        break;
                    case TAB_MAIN.Search:
                        lock (lockSearchTab)
                        {
                            if (VM_MainListHelper.Instance.AllSeriesDictionary == null ||
                                VM_MainListHelper.Instance.AllSeriesDictionary.Count == 0)
                                VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                        }
                        break;
                    case TAB_MAIN.Server:
                        lock (lockServerTab)
                        {
                            if (VM_ShokoServer.Instance.ImportFolders.Count == 0) VM_ShokoServer.Instance.RefreshImportFolders();
                        }
                        break;
                    case TAB_MAIN.Settings:
                        lock (lockSettingsTab)
                        {
                            if (VM_ShokoServer.Instance.ImportFolders.Count == 0) VM_ShokoServer.Instance.RefreshImportFolders();
                            if (VM_ShokoServer.Instance.SelectedLanguages.Count == 0)
                                VM_ShokoServer.Instance.RefreshNamingLanguages();
                            if (VM_ShokoServer.Instance.AllUsers.Count == 0) VM_ShokoServer.Instance.RefreshAllUsers();
                            if (VM_ShokoServer.Instance.AllTags.Count == 0) VM_ShokoServer.Instance.RefreshAllTags();
                            if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                        }
                        break;
                    case TAB_MAIN.Pinned:
                        lock (lockPinnedTab)
                        {
                            if (VM_ShokoServer.Instance.AllCustomTags.Count == 0) VM_ShokoServer.Instance.RefreshAllCustomTags();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                tabControl1.IsEnabled = true;
            }
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetColours();

                //if (!this.IsLoaded || !VM_ShokoServer.Instance.UserAuthenticated) return;
                if (!VM_ShokoServer.Instance.UserAuthenticated) return;


                if (!(e.Source is TabControl tab)) return;

                if (!tab.Name.Equals("tabControl1", StringComparison.InvariantCultureIgnoreCase)) return;

                DisplayMainTab(tabControl1.SelectedIndex);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                tabControl1.IsEnabled = true;
            }
        }

        private void tabSettingsChild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TabControl tab = e.Source as TabControl;
                if (tab?.SelectedIndex != (int) TAB_Settings.Display) return;
                if (VM_ShokoServer.Instance.SelectedLanguages.Count == 0) VM_ShokoServer.Instance.RefreshNamingLanguages();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void btnToolbarSort_Click(object sender, RoutedEventArgs e)
        {
            VM_MainListHelper.Instance.ViewGroups.SortDescriptions.Clear();
            GroupFilterSorting sortType = cboGroupSort.SelectedItem.ToString().GetEnumForText_Sorting();
            VM_MainListHelper.Instance.ViewGroups.SortDescriptions.Add(sortType.GetSortDescription(GroupFilterSortDirection.Asc));
        }



        private void imageHelper_QueueUpdateEvent(QueueUpdateEventArgs ev)
        {
            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MethodInvoker(delegate
                {
                    tbImageDownloadQueueStatus.Text = ev.queueCount.ToString();
                }));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        private void cboLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    if (ul?.Culture.Trim().ToUpper() != currentCulture.Trim().ToUpper()) continue;
                    cboLanguages.SelectedIndex = i;
                    break;
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

            try
            {
                CultureInfo ci = new CultureInfo(ul.Culture);
                CultureInfo.DefaultThreadCurrentUICulture = ci;
                CultureManager.UICulture = ci;
                AppSettings.Culture = ul.Culture;
                ConfigurationManager.RefreshSection("appSettings");

                if (isLanguageChanged)
                {
                    var result = FlexibleMessageBox.Show(Commons.Properties.Resources.Language_Info, Commons.Properties.Resources.Language_Switch, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        Application.Restart();
                        System.Windows.Application.Current.Shutdown();
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
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
            if (!(obj is IListWrapper grp)) return true;

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

        private void refreshGroupsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            showChildWrappersWorker.RunWorkerAsync(VM_MainListHelper.Instance.CurrentWrapper);
        }

        private void refreshGroupsWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void showChildWrappersWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                EnableDisableGroupControls(true);
                if (lbGroupsSeries.Items.Count > 0)
                    HighlightMainListItem();
                else
                    SetDetailBinding(null);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void showChildWrappersWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void toggleStatusWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableDisableGroupControls(true);
            Cursor = Cursors.Arrow;
        }


        private void toggleStatusWorker_DoWork(object sender, DoWorkEventArgs e)
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
                        MessageBox.Show(response.ErrorMessage, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    ser = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                if (newStatus && ser != null)
                    Utils.PromptToRateSeries(ser, this);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void moveSeriesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableDisableGroupControls(true);
            SetDetailBinding(null);
            VM_MainListHelper.Instance.RefreshGroupsSeriesData();
            VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
            Cursor = Cursors.Arrow;

            VM_MainListHelper.Instance.ViewGroups.Refresh();
            showChildWrappersWorker.RunWorkerAsync(VM_MainListHelper.Instance.CurrentWrapper);
        }

        private void moveSeriesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object obj = e.Argument;
                if (obj.GetType() != typeof(VM_MoveSeriesDetails)) return;

                VM_MoveSeriesDetails request = obj as VM_MoveSeriesDetails;

                //request.UpdatedSeries.Save();
                CL_Response<CL_AnimeSeries_User> response =
                    VM_ShokoServer.Instance.ShokoServices.MoveSeries(request.UpdatedSeries.AnimeSeriesID, request.UpdatedSeries.AnimeGroupID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal, (Action)
                        delegate
                        {
                            Cursor = Cursors.Arrow;
                            FlexibleMessageBox.Show(response.ErrorMessage);
                        });
                else
                    request.UpdatedSeries.Populate((VM_AnimeSeries_User)response.Result);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void DownloadAllImages()
        {
            //if (!downloadImagesWorker.IsBusy)
            //  downloadImagesWorker.RunWorkerAsync();
        }

        private void downloadImagesWorker_DoWork(object sender, DoWorkEventArgs e)
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
            //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBPoster(poster, false);

            // 2a. Download posters from MovieDB
            List<VM_MovieDB_Poster> moviePosters = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBPosters(null).CastList<VM_MovieDB_Poster>();
            foreach (VM_MovieDB_Poster poster in moviePosters)
            //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBPoster(poster, false);

            // 3. Download wide banners from TvDB
            List<VM_TvDB_ImageWideBanner> banners = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBWideBanners(null).CastList<VM_TvDB_ImageWideBanner>();
            foreach (VM_TvDB_ImageWideBanner banner in banners)
            //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBWideBanner(banner, false);

            // 4. Download fanart from TvDB
            List<VM_TvDB_ImageFanart> fanarts = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBFanart(null).CastList<VM_TvDB_ImageFanart>();
            foreach (VM_TvDB_ImageFanart fanart in fanarts)
            //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBFanart(fanart, false);

            // 4a. Download fanart from MovieDB
            List<VM_MovieDB_Fanart> movieFanarts = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBFanart(null).CastList<VM_MovieDB_Fanart>();
            foreach (VM_MovieDB_Fanart fanart in movieFanarts)
            //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBFanart(fanart, false);

            // 5. Download episode images from TvDB
            List<VM_TvDB_Episode> eps = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBEpisodes(null).CastList<VM_TvDB_Episode>();
            foreach (VM_TvDB_Episode episode in eps)
            //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBEpisode(episode, false);
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
                if (VM_MainListHelper.Instance.CurrentWrapper == null) return;
                // refresh the groups and series data
                refreshGroupsWorker.RunWorkerAsync(null);

                // refresh the episodes
                VM_AnimeSeries_User ser = lbGroupsSeries.SelectedItem as VM_AnimeSeries_User;
                ser?.RefreshEpisodes();
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

        public void CommandBinding_EditTraktCredentials(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl1.SelectedIndex = (int) TAB_MAIN.Settings;
            tabSettingsChild.SelectedIndex = (int) TAB_Settings.TvDB;
        }

        public void ShowPinnedFileAvDump(VM_VideoLocal vid)
        {
            try
            {
                if (VM_MainListHelper.Instance.AVDumpFiles.Any(dumpTemp => dumpTemp.FullPath == vid.ServerPath))
                    return;

                VM_AVDump dump = new VM_AVDump(vid);
                VM_MainListHelper.Instance.AVDumpFiles.Add(dump);

                tabControl1.SelectedIndex = (int) TAB_MAIN.FileManger;
                tabFileManager.SelectedIndex = (int) TAB_UTILITY.Avdump;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
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

                ContentControl ctrl = ctiTemp?.Content as AnimeSeriesContainerControl;
                if (ctrl == null) continue;

                ContentControl subControl = (ContentControl) (ctrl.DataContext as AnimeSeriesSimplifiedControl) ?? ctrl.DataContext as AnimeSeries;

                if (subControl != null)
                    ctrl = subControl;

                var ser = ctrl.DataContext as VM_AnimeSeries_User;

                if (ser?.AnimeSeriesID != series.AnimeSeriesID) continue;

                tabControl1.SelectedIndex = (int) TAB_MAIN.Pinned;
                tabPinned.SelectedIndex = curTab;
                Cursor = Cursors.Arrow;
                return;
            }

            string tabHeader = series.SeriesName;
            if (tabHeader.Length > 30)
                tabHeader = tabHeader.Substring(0, 30) + "...";
            cti.Header = tabHeader;

            if (AppSettings.DisplaySeriesSimple)
            {
                AnimeSeriesSimplifiedControl ctrl = new AnimeSeriesSimplifiedControl {DataContext = series};

                AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl
                {
                    IsMetroDash = false,
                    DataContext = ctrl
                };

                cti.Content = cont;

                tabPinned.Items.Add(cti);
            }
            else
            {
                AnimeSeries seriesControl = new AnimeSeries {DataContext = series};

                AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl
                {
                    IsMetroDash = false,
                    DataContext = seriesControl
                };

                cti.Content = cont;

                tabPinned.Items.Add(cti);
            }

            tabControl1.SelectedIndex = (int) TAB_MAIN.Pinned;
            tabPinned.SelectedIndex = tabPinned.Items.Count - 1;

            Cursor = Cursors.Arrow;
        }


        public void RefreshPinnedSeries()
        {
            Cursor = Cursors.Wait;

            foreach (object obj in tabPinned.Items)
            {
                CloseableTabItem ctiTemp = obj as CloseableTabItem;

                ContentControl ctrl = ctiTemp?.Content as AnimeSeriesContainerControl;
                if (ctrl == null) continue;

                ContentControl subControl = (ContentControl) (ctrl.DataContext as AnimeSeriesSimplifiedControl) ?? ctrl.DataContext as AnimeSeries;

                if (subControl != null)
                    ctrl = subControl;

                var ser = ctrl.DataContext as VM_AnimeSeries_User;

                if (ser?.AnimeSeriesID == 0) continue;

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

                var playListObjects = playListVM?.PlaylistObjects;
                if (playListObjects != null)
                    foreach (var item in playListObjects)
                        if (item.ItemType == PlaylistItemType.AnimeSeries)
                        {
                            var itemSer = item.Series;
                            if (itemSer == null) continue;
                            if (itemSer.AnimeSeriesID==0) continue;

                            var itemSeriesId = itemSer.AnimeSeriesID;
                            if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(itemSeriesId, out itemSer))
                                item.Series = itemSer;
                        }

                var ser = playListVM?.Series;
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
            if (tabControl1.SelectedIndex == (int) TAB_MAIN.Dashboard)
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
                    DisplayMainTab((int) TAB_MAIN.Dashboard);
                    AppSettings.DashboardType = DashboardType.Normal;
                    break;
                case MetroViews.MainMetro:
                    dashMetro.Visibility = Visibility.Visible;
                    DisplayMainTab((int) TAB_MAIN.Dashboard);
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

        public void CommandBinding_CreateSeriesFromAnime(object sender, ExecutedRoutedEventArgs e)
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
                    if (anime == null) return;

                    // check if a series already exists
                    bool seriesExists = VM_ShokoServer.Instance.ShokoServices.GetSeriesExistingForAnime(anime.AnimeID);
                    if (seriesExists)
                    {
                        MessageBox.Show(Commons.Properties.Resources.ERROR_SeriesExists, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    NewSeries frmNewSeries = new NewSeries {Owner = this};
                    frmNewSeries.Init(anime, anime.FormattedTitle);

                    frmNewSeries.ShowDialog();
                }
                else
                {
                    NewSeries frm = new NewSeries();
                    frm.Owner = this;
                    frm.Init(0, "");
                    frm.ShowDialog();
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_AvdumpFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;
                    if (vid == null) return;
                    if (VM_MainListHelper.Instance.AVDumpFiles.Any(dumpTemp =>
                        dumpTemp.FullPath == vid.ServerPath))
                        return;

                    VM_AVDump dump = new VM_AVDump(vid);
                    VM_MainListHelper.Instance.AVDumpFiles.Add(dump);

                }

                if (obj.GetType() == typeof(MultipleVideos))
                {
                    if (obj is MultipleVideos mv)
                        foreach (VM_VideoLocal vid in mv.VideoLocals)
                        {
                            if (vid == null) continue;
                            bool alreadyExists = VM_MainListHelper.Instance.AVDumpFiles.Any(dumpTemp =>
                                dumpTemp.FullPath == vid.ServerPath);

                            if (alreadyExists) continue;

                            VM_AVDump dump = new VM_AVDump(vid);
                            VM_MainListHelper.Instance.AVDumpFiles.Add(dump);
                        }
                }

                tabControl1.SelectedIndex = (int) TAB_MAIN.FileManger;
                tabFileManager.SelectedIndex = (int) TAB_UTILITY.Avdump;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void CommandBinding_BookmarkAnime(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    if (!(obj is VM_Recommendation rec)) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime
                    {
                        AnimeID = rec.RecommendedAnimeID,
                        Downloading = 0,
                        Notes = "",
                        Priority = 1
                    };
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();

                }

                if (obj.GetType() == typeof(VM_MissingEpisode))
                {
                    if (!(obj is VM_MissingEpisode rec)) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime
                    {
                        AnimeID = rec.AnimeID,
                        Downloading = 0,
                        Notes = "",
                        Priority = 1
                    };
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime))
                {
                    if (!(obj is VM_AniDB_Anime rec)) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime
                    {
                        AnimeID = rec.AnimeID,
                        Downloading = 0,
                        Notes = "",
                        Priority = 1
                    };
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                {
                    if (!(obj is VM_AniDB_Anime_Similar sim)) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime
                    {
                        AnimeID = sim.SimilarAnimeID,
                        Downloading = 0,
                        Notes = "",
                        Priority = 1
                    };
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }

                if (obj.GetType() == typeof(VM_AniDB_Anime_Relation))
                {
                    if (!(obj is VM_AniDB_Anime_Relation rel)) return;

                    VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime
                    {
                        AnimeID = rel.RelatedAnimeID,
                        Downloading = 0,
                        Notes = "",
                        Priority = 1
                    };
                    if (bookmark.Save())
                        VM_MainListHelper.Instance.RefreshBookmarkedAnime();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_RefreshBookmarks(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_MainListHelper.Instance.RefreshBookmarkedAnime();
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
                var crit = new SearchCriteria
                {
                    ExtraInfo = string.Empty,
                    AnimeID = anime.AnimeID
                };

                tabControl1.SelectedIndex = (int) TAB_MAIN.Community;
                tabcCommunity.SelectedIndex = 0;

                ucComLinks.PerformTvDBSearch(crit);
                ucComLinks.PerformTraktSearch(crit);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_ShowWebCacheAdmin(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                SearchCriteria crit = null;

                if (obj.GetType() == typeof(VM_AniDB_Anime))
                {
                    VM_AniDB_Anime anime = (VM_AniDB_Anime)obj;
                    crit = new SearchCriteria
                    {
                        ExtraInfo = string.Empty,
                        AnimeID = anime.AnimeID
                    };
                }

                if (crit == null) return;
                tabControl1.SelectedIndex = (int) TAB_MAIN.Community;
                tabcCommunity.SelectedIndex = 0;

                ucComLinks.PerformTvDBSearch(crit);
                ucComLinks.PerformTraktSearch(crit);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_ShowPinnedSeries(object sender, ExecutedRoutedEventArgs e)
        {
            //object obj = lbGroupsSeries.SelectedItem;
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                int? objID = null;

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) obj;
                    objID = ep.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {
                    VM_AnimeSeries_User series = (VM_AnimeSeries_User) obj;
                    objID = series.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_AniDB_Anime_Similar))
                {
                    VM_AniDB_Anime_Similar sim = (VM_AniDB_Anime_Similar) obj;
                    objID = sim.AnimeSeries.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_AniDB_Anime_Relation))
                {
                    VM_AniDB_Anime_Relation rel = (VM_AniDB_Anime_Relation) obj;
                    objID = rel.AnimeSeries.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = (VM_Recommendation) obj;
                    objID = rec.Recommended_AnimeSeries.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_MissingFile))
                {
                    VM_MissingFile mis = (VM_MissingFile) obj;
                    objID = mis.AnimeSeries.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_MissingEpisode))
                {
                    VM_MissingEpisode misEp = (VM_MissingEpisode) obj;
                    objID = misEp.AnimeSeries.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(VM_PlaylistItem))
                {
                    VM_PlaylistItem pli = (VM_PlaylistItem) obj;
                    switch (pli.ItemType)
                    {
                        case PlaylistItemType.AnimeSeries:
                            var series = (VM_AnimeSeries_User) pli.PlaylistItem;
                            objID = series.AnimeSeriesID;
                            break;
                        case PlaylistItemType.Episode:
                            VM_AnimeEpisode_User ep = pli.PlaylistItem as VM_AnimeEpisode_User;
                            objID = ep?.AnimeSeriesID;
                            break;
                    }
                }
                else if (obj.GetType() == typeof(VM_AnimeSearch))
                {
                    VM_AnimeSearch search = (VM_AnimeSearch) obj;
                    objID = search.AnimeSeriesID;
                }
                else if (obj.GetType() == typeof(TraktSeriesData))
                {
                    TraktSeriesData trakt = (TraktSeriesData) obj;
                    objID = trakt.AnimeSeriesID;
                }

                if (objID == null) return;
                var valObjID = objID.Value;

                if (!VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(valObjID, out var ser))
                {
                    // get the series
                    ser = (VM_AnimeSeries_User) VM_ShokoServer.Instance.ShokoServices.GetSeries(valObjID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (ser != null)
                        VM_MainListHelper.Instance.AllSeriesDictionary[valObjID] = ser;
                }

                if (ser != null)
                    ShowPinnedSeries(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_Refresh(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshView();
        }

        public void CommandBinding_Search(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // move to all groups
                VM_MainListHelper.Instance.ShowAllGroups();

                if (e.Parameter is VM_AnimeTag obj)
                    txtGroupSearch.Text = obj.TagName;

                tabControl1.SelectedIndex = (int) TAB_MAIN.Collection;
                HighlightMainListItem();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_Back(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_Edit(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = (VM_AnimeGroup_User)obj;

                    if (grp.AnimeGroupID!=0)
                    {
                        groupBeforeChanges = new VM_AnimeGroup_User
                        {
                            GroupName = grp.GroupName,
                            IsFave = grp.IsFave,
                            Description = grp.Description,
                            SortName = grp.SortName
                        };
                    }

                    grp.IsReadOnly = false;
                    grp.IsBeingEdited = true;

                }

                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = (VM_GroupFilter)obj;

                    if (gf.GroupFilterID!=0)
                    {
                        groupFilterBeforeChanges = new VM_GroupFilter
                        {
                            GroupFilterName = gf.GroupFilterName,
                            BaseCondition = gf.BaseCondition,
                            ApplyToSeries = gf.ApplyToSeries
                        };

                        foreach (VM_GroupFilterCondition gfc_cur in gf.Obs_FilterConditions)
                        {
                            VM_GroupFilterCondition gfc =
                                new VM_GroupFilterCondition
                                {
                                    ConditionOperator = gfc_cur.ConditionOperator,
                                    ConditionParameter = gfc_cur.ConditionParameter,
                                    ConditionType = gfc_cur.ConditionType,
                                    GroupFilterConditionID = gfc_cur.GroupFilterConditionID,
                                    GroupFilterID = gfc_cur.GroupFilterID
                                };
                            groupFilterBeforeChanges.Obs_FilterConditions.Add(gfc);
                        }

                        foreach (VM_GroupFilterSortingCriteria gfcs_cur in gf.SortCriteriaList)
                        {
                            VM_GroupFilterSortingCriteria gfsc =
                                new VM_GroupFilterSortingCriteria
                                {
                                    GroupFilterID = gfcs_cur.GroupFilterID,
                                    SortDirection = gfcs_cur.SortDirection,
                                    SortType = gfcs_cur.SortType
                                };
                            groupFilterBeforeChanges.SortCriteriaList.Add(gfsc);
                        }
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

        public void CommandBinding_Save(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = (VM_AnimeGroup_User)obj;
                    grp.SortName = grp.SortName ?? grp.GroupName;
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

        public void CommandBinding_ScanFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_ImportFolder))
                {
                    VM_ImportFolder fldr = (VM_ImportFolder)obj;

                    VM_ShokoServer.Instance.ShokoServices.ScanFolder(fldr.ImportFolderID);
                    MessageBox.Show(Commons.Properties.Resources.Import_Running, Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void CommandBinding_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            EnableDisableGroupControls(false);

            try
            {
                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = (VM_GroupFilter)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(Commons.Properties.Resources.Filter_DeleteGroup, gf.GroupFilterName),
                    Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                                idx = idx - 1;
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
                            VM_MainListHelper.Instance.CurrentWrapperList.RemoveAt(pos);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableGroupControls(true);
        }

        public void CommandBinding_Cancel(object sender, ExecutedRoutedEventArgs e)
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
                            VM_GroupFilterCondition gfc =
                                new VM_GroupFilterCondition
                                {
                                    ConditionOperator = gfc_old.ConditionOperator,
                                    ConditionParameter = gfc_old.ConditionParameter,
                                    ConditionType = gfc_old.ConditionType,
                                    GroupFilterConditionID = gfc_old.GroupFilterConditionID,
                                    GroupFilterID = gfc_old.GroupFilterID
                                };
                            gf.Obs_FilterConditions.Add(gfc);
                        }

                        foreach (VM_GroupFilterSortingCriteria gfsc_old in groupFilterBeforeChanges.SortCriteriaList)
                        {
                            VM_GroupFilterSortingCriteria gfsc =
                                new VM_GroupFilterSortingCriteria
                                {
                                    GroupFilterID = gfsc_old.GroupFilterID,
                                    SortDirection = gfsc_old.SortDirection,
                                    SortType = gfsc_old.SortType
                                };
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

        public void CommandBinding_NewGroupFilter(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_GroupFilter gfNew = new VM_GroupFilter
                {
                    Locked = 0,
                    IsBeingEdited = true,
                    GroupFilterName = Commons.Properties.Resources.Filter_New,
                    ApplyToSeries = 0,
                    BaseCondition = (int) GroupFilterBaseCondition.Include
                };


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

        public void CommandBinding_DeleteFilterCondition(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_GroupFilterCondition)) return;
                VM_GroupFilterCondition gfc = (VM_GroupFilterCondition)obj;

                MessageBoxResult res = MessageBox.Show(string.Format(Commons.Properties.Resources.Filter_DeleteCondition, gfc.NiceDescription),
                    Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                foreach (VM_GroupFilter gf in VM_MainListHelper.Instance.AllGroupFiltersDictionary.Values)
                {
                    if (!gf.AllowEditing) continue; // all filter
                    if (gf.GroupFilterID != gfc.GroupFilterID) continue;
                    int pos = -1;
                    for (int i = 0; i < gf.Obs_FilterConditions.Count; i++)
                        if (gfc.ConditionOperator == gf.Obs_FilterConditions[i].ConditionOperator &&
                            gfc.ConditionParameter.Equals(gf.Obs_FilterConditions[i].ConditionParameter) &&
                            gfc.ConditionType == gf.Obs_FilterConditions[i].ConditionType)
                        {
                            pos = i;
                            break;
                        }
                    if (pos >= 0)
                        gf.Obs_FilterConditions.RemoveAt(pos);

                    VM_GroupFilter tempGF = (VM_GroupFilter)VM_ShokoServer.Instance.ShokoServices.EvaluateGroupFilter(gf);
                    gf.Populate(tempGF);
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

        public void CommandBinding_NewFilterCondition(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                object obj = e.Parameter;
                if (obj == null) return;

                VM_GroupFilter gf = (VM_GroupFilter)obj;
                VM_GroupFilterCondition gfc = new VM_GroupFilterCondition();

                GroupFilterConditionForm frm = new GroupFilterConditionForm {Owner = this};
                frm.Init(gf, gfc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    gf.Obs_FilterConditions.Add(gfc);

                    VM_GroupFilter tempGF = (VM_GroupFilter)VM_ShokoServer.Instance.ShokoServices.EvaluateGroupFilter(gf);
                    gf.Populate(tempGF);
                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                    SetDetailBinding(gf);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_NewFilterSorting(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                object obj = e.Parameter;
                if (obj == null) return;

                VM_GroupFilter gf = (VM_GroupFilter)obj;
                VM_GroupFilterSortingCriteria gfsc = new VM_GroupFilterSortingCriteria();

                GroupFilterSortingForm frm = new GroupFilterSortingForm {Owner = this};
                frm.Init(gf, gfsc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    gf.SortCriteriaList.Add(gfsc);

                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                    SetDetailBinding(gf);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_MoveUpFilterSort(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_GroupFilterSortingCriteria)) return;
                VM_GroupFilterSortingCriteria gfsc = (VM_GroupFilterSortingCriteria)obj;
                GroupFilterSortMoveUpDown(gfsc, 1);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_MoveDownFilterSort(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_GroupFilterSortingCriteria)) return;
                VM_GroupFilterSortingCriteria gfsc = (VM_GroupFilterSortingCriteria)obj;
                GroupFilterSortMoveUpDown(gfsc, 2);
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
                if (gf.GroupFilterID != gfsc.GroupFilterID) continue;
                int pos = -1;
                for (int i = 0; i < gf.SortCriteriaList.Count; i++)
                    if (gfsc.SortType == gf.SortCriteriaList[i].SortType)
                    {
                        pos = i;
                        break;
                    }

                if (direction == 1) // up
                {
                    if (pos <= 0) continue;
                    gf.SortCriteriaList.Move(pos, pos - 1);
                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }
                else
                {
                    if (pos + 1 >= gf.SortCriteriaList.Count) continue;
                    gf.SortCriteriaList.Move(pos, pos + 1);
                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }
            }
        }



        public void CommandBinding_DeleteFilterSort(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_GroupFilterSortingCriteria)) return;
                VM_GroupFilterSortingCriteria gfsc = (VM_GroupFilterSortingCriteria)obj;

                MessageBoxResult res = MessageBox.Show(string.Format(Commons.Properties.Resources.Filter_DeleteSort),
                    Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                foreach (VM_GroupFilter gf in VM_MainListHelper.Instance.AllGroupFiltersDictionary.Values)
                {
                    if (!gf.AllowEditing) continue; // all filter
                    if (gf.GroupFilterID != gfsc.GroupFilterID) continue;
                    int pos = -1;
                    for (int i = 0; i < gf.SortCriteriaList.Count; i++)
                        if (gfsc.SortType == gf.SortCriteriaList[i].SortType)
                        {
                            pos = i;
                            break;
                        }
                    if (pos >= 0)
                        gf.SortCriteriaList.RemoveAt(pos);

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

        public void CommandBinding_NewGroup(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_AnimeGroup_User grpNew = new VM_AnimeGroup_User
                {
                    IsReadOnly = false,
                    IsBeingEdited = true
                };
                SetDetailBinding(grpNew);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_DeleteGroup(object sender, ExecutedRoutedEventArgs e)
        {
            EnableDisableGroupControls(false);

            try
            {
                if (!(e.Parameter is VM_AnimeGroup_User grp)) return;

                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm {Owner = this};
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value)
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

        public void CommandBinding_ViewGroup(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Parameter is VM_AnimeGroup_User grp)) return;

                SetDetailBinding(grp);

                //DisplayMainTab((int) TAB_MAIN.Collection);
                tabControl1.SelectedIndex = (int) TAB_MAIN.Collection;


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_AddSubGroup(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is VM_AnimeGroup_User grp)) return;

            try
            {
                VM_AnimeGroup_User grpNew = new VM_AnimeGroup_User
                {
                    IsReadOnly = false,
                    IsBeingEdited = true,
                    AnimeGroupParentID = grp.AnimeGroupID
                };
                SetDetailBinding(grpNew);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_AddPlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            VM_PlaylistHelper.CreatePlaylist(this);
        }

        public void CommandBinding_DeletePlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is VM_Playlist pl)) return;

            try
            {
                MessageBoxResult res = MessageBox.Show(string.Format(Commons.Properties.Resources.Playlist_Delete, pl.PlaylistName),
                    Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
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
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_DeletePlaylistItem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Parameter is VM_PlaylistItem pli)) return;

                Cursor = Cursors.Wait;

                // get the playlist
                VM_Playlist pl = (VM_Playlist)VM_ShokoServer.Instance.ShokoServices.GetPlaylist(pli.PlaylistID);
                if (pl == null)
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show(Commons.Properties.Resources.Filter_PlaylistMissing,
                        Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                switch (pli.ItemType)
                {
                    case PlaylistItemType.AnimeSeries:
                        if (pli.PlaylistItem is VM_AnimeSeries_User ser) pl.RemoveSeries(ser.AnimeSeriesID);
                        break;
                    case PlaylistItemType.Episode:
                        if (pli.PlaylistItem is VM_AnimeEpisode_User ep) pl.RemoveEpisode(ep.AnimeEpisodeID);
                        break;
                }

                pl.Save();
                pl = (VM_Playlist)VM_ShokoServer.Instance.ShokoServices.GetPlaylist(pli.PlaylistID);

                // refresh data
                if (lbPlaylists.Items.Count > 0)
                {
                    // get the current playlist
                    if (lbPlaylists.SelectedItem is VM_Playlist selPL && pl != null &&
                        selPL.PlaylistID == pl.PlaylistID)
                    {
                        selPL.Populate(pl);
                        selPL.PopulatePlaylistObjects();
                        VM_PlaylistHelper.Instance.OnPlaylistModified(new PlaylistModifiedEventArgs(pl.PlaylistID));
                    }
                    else
                        VM_PlaylistHelper.Instance.RefreshData();
                }

                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_RefreshPlaylist(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_IncrementSeriesImageSize(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.SeriesGroup_Image_Height = VM_UserSettings.Instance.SeriesGroup_Image_Height + 10;
        }

        public void CommandBinding_DecrementSeriesImageSize(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.SeriesGroup_Image_Height = VM_UserSettings.Instance.SeriesGroup_Image_Height - 10;
        }

        public void CommandBinding_NewSeries(object sender, ExecutedRoutedEventArgs e)
        {

        }

        public void CommandBinding_DeleteSeries(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is VM_AnimeSeries_User ser)) return;

            try
            {
                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm {Owner = this};
                bool? result = frm.ShowDialog();

                if (!result.HasValue || !result.Value) return;
                Cursor = Cursors.Wait;

                VM_ShokoServer.Instance.ShokoServices.DeleteAnimeSeries(ser.AnimeSeriesID, frm.DeleteFiles, frm.DeleteGroups);
                VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);
                SetDetailBinding(null);
                Cursor = Cursors.Arrow;
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


        public void CommandBinding_MoveSeries(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is VM_AnimeSeries_User ser)) return;

            try
            {
                MoveSeries frm = new MoveSeries {Owner = this};
                frm.Init(ser);
                bool? result = frm.ShowDialog();

                if (!result.HasValue || !result.Value) return;
                VM_AnimeGroup_User grpSelected = frm.SelectedGroup;
                if (grpSelected == null) return;

                VM_MoveSeriesDetails request = new VM_MoveSeriesDetails {OldAnimeGroupID = ser.AnimeGroupID};

                ser.AnimeGroupID = grpSelected.AnimeGroupID;
                request.UpdatedSeries = ser;

                Cursor = Cursors.Wait;
                EnableDisableGroupControls(false);
                moveSeriesWorker.RunWorkerAsync(request);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_ClearSearch(object sender, ExecutedRoutedEventArgs e)
        {
            txtGroupSearch.Text = "";
            HighlightMainListItem();
        }

        public void CommandBinding_RunImport(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.RunImport();
                MessageBox.Show(Commons.Properties.Resources.Import_Running, Commons.Properties.Resources.Success,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void CommandBinding_RemoveMissingFiles(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MessageBoxResult res = MessageBox.Show(string.Format(Commons.Properties.Resources.Main_RunProcess),
                    Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                VM_ShokoServer.Instance.RemoveMissingFiles();
                MessageBox.Show(Commons.Properties.Resources.Process_Running, Commons.Properties.Resources.Success,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_SyncMyList(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.SyncMyList();
                MessageBox.Show(Commons.Properties.Resources.Process_Running, Commons.Properties.Resources.Success,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_SyncVotes(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_ShokoServer.Instance.SyncVotes();
                MessageBox.Show(Commons.Properties.Resources.Process_Running, Commons.Properties.Resources.Success,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_RevokeVote(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is VM_AnimeSeries_User ser)) return;

            try
            {
                VM_ShokoServer.Instance.RevokeVote(ser.AniDB_ID);

                // refresh the data
                //ser.RefreshBase();
                //ser.AniDBAnime.Detail.RefreshBase();

                VM_MainListHelper.Instance.UpdateHeirarchy(ser);

                //SetDetailBinding(null);
                //SetDetailBinding(ser);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        public void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;
            EnableDisableGroupControls(false);
            toggleStatusWorker.RunWorkerAsync(obj);
        }


        public void CommandBinding_BreadCrumbSelect(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // switching back to the top view (show all filters)
                switch (e.Parameter)
                {
                    case null:
                        VM_MainListHelper.Instance.ShowChildWrappers(null);
                        break;
                    case IListWrapper list:
                        VM_MainListHelper.Instance.ShowChildWrappers(list);
                        break;
                }
                HighlightMainListItem();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void CommandBinding_ToggleFave(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_ToggleExpandTags(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.TagsExpanded = !VM_UserSettings.Instance.TagsExpanded;
        }

        public void CommandBinding_ToggleExpandTitles(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.TitlesExpanded = !VM_UserSettings.Instance.TitlesExpanded;
        }

        private void SetWindowFullscreen()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }

        public void CommandBinding_WindowFullScreen(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_WindowNormal(object sender, ExecutedRoutedEventArgs e)
        {
            VM_UserSettings.Instance.WindowFullScreen = false;
            VM_UserSettings.Instance.WindowNormal = true;
            SetWindowNormal();
        }

        public void CommandBinding_WindowClose(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        public void CommandBinding_WindowMinimize(object sender, ExecutedRoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        #region Server Queue Actions

        public void CommandBinding_HasherQueuePause(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_HasherQueueResume(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_GeneralQueuePause(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_GeneralQueueResume(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_ServerImageQueuePause(object sender, ExecutedRoutedEventArgs e)
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

        public void CommandBinding_ServerImageQueueResume(object sender, ExecutedRoutedEventArgs e)
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
            if (!(obj is VM_AnimeGroup_User grpvm)) return false;
            return GroupSearchFilterHelper.EvaluateGroupFilter(groupFilterVM, grpvm);
        }

        private void lbPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void lbGroupsSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox lb = (ListBox)sender;

                object obj = lb.SelectedItem;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {

                    VM_AnimeSeries_User series = obj as VM_AnimeSeries_User;
                    VM_MainListHelper.Instance.CurrentSeries = series;
                }

                if (obj.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = obj as VM_AnimeGroup_User;
                }

                if (obj.GetType() == typeof(VM_GroupFilter))
                {
                    VM_GroupFilter gf = obj as VM_GroupFilter;

                    groupFilterVM = gf;
                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }

                if (!string.IsNullOrEmpty(VM_MainListHelper.Instance.CurrentOpenGroupFilter) && lbGroupsSeries.SelectedItem != null)
                    VM_MainListHelper.Instance.LastGroupForGF[VM_MainListHelper.Instance.CurrentOpenGroupFilter] = lbGroupsSeries.SelectedIndex;

                SetDetailBinding(obj);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void ShowChildrenForCurrentGroup(VM_AnimeSeries_User ser)
        {
            if (!(lbGroupsSeries.SelectedItem is IListWrapper)) return;
            // this is the last supported drill down
            if (lbGroupsSeries.SelectedItem.GetType() == typeof(VM_AnimeSeries_User)) return;

            //VM_MainListHelper.Instance.LastAnimeSeriesID = ser.AnimeSeriesID.Value;

            EnableDisableGroupControls(false);
            showChildWrappersWorker.RunWorkerAsync(lbGroupsSeries.SelectedItem);
        }

        public void lbGroupsSeries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            var source = e.Source as FrameworkElement;

            if (Equals(originalSource?.DataContext, source?.DataContext)) //both null, or both what ever the scrollbar and the listview/treeview
                return;

            try
            {
                var selItem = lbGroupsSeries.SelectedItem;
                switch (selItem)
                {
                    case null:
                        return;
                    case VM_AnimeGroup_User animeGroup:
                        VM_MainListHelper.Instance.CurrentOpenGroupFilter = "VM_AnimeGroup_User|" + animeGroup.AnimeGroupID;
                        break;
                    case VM_GroupFilter groupFilter:
                        VM_MainListHelper.Instance.CurrentOpenGroupFilter = "GroupFilterVM|" + groupFilter.GroupFilterID;
                        break;
                }

                if (!(selItem is IListWrapper)) return;
                if (selItem.GetType() == typeof(VM_AnimeSeries_User)) return;

                EnableDisableGroupControls(false);
                showChildWrappersWorker.RunWorkerAsync(lbGroupsSeries.SelectedItem);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex.ToString());
            }
        }


        public void HighlightMainListItem()
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
                    // move to the previous item
                    if (lastSelIndex - 1 <= lbGroupsSeries.Items.Count)
                        if (lastSelIndex > 0)
                        {
                            lbGroupsSeries.SelectedItem = lbGroupsSeries.Items[lastSelIndex - 1];
                            lbGroupsSeries.Focus();
                            lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[lastSelIndex - 1]);
                            //SetDetailBinding(lbGroupsSeries.SelectedItem);

                            return;
                        }
                }

                if (lbGroupsSeries.Items.Count <= 0) return;
                lbGroupsSeries.SelectedIndex = 0;
                lbGroupsSeries.Focus();
                lbGroupsSeries.ScrollIntoView(lbGroupsSeries.SelectedItem);
                //SetDetailBinding(lbGroupsSeries.SelectedItem);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void EnableDisableGroupControls(bool val)
        {
            lbGroupsSeries.IsEnabled = val;
            txtGroupSearch.IsEnabled = val;
            tbSeriesEpisodes.IsEnabled = val;
        }

        public void SetDetailBinding(object objToBind)
        {
            try
            {
                if (objToBind != null && objToBind.GetType() == typeof(VM_AnimeSeries_User))
                {
                    VM_AnimeSeries_User ser = objToBind as VM_AnimeSeries_User;
                    if (AppSettings.DisplaySeriesSimple)
                    {
                        AnimeSeriesSimplifiedControl ctrl = new AnimeSeriesSimplifiedControl {DataContext = ser};

                        AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl
                        {
                            IsMetroDash = false,
                            DataContext = ctrl
                        };

                        objToBind = cont;
                    }
                    else
                    {
                        AnimeSeries ctrl = new AnimeSeries {DataContext = ser};

                        AnimeSeriesContainerControl cont = new AnimeSeriesContainerControl
                        {
                            IsMetroDash = false,
                            DataContext = ctrl
                        };

                        objToBind = cont;
                    }
                }

                Binding b = new Binding
                {
                    Source = objToBind,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                };
                ccDetail.SetBinding(ContentProperty, b);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void SetDetailBindingPlaylist(object objToBind)
        {
            try
            {
                Binding b = new Binding
                {
                    Source = objToBind,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                };
                ccPlaylist.SetBinding(ContentProperty, b);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void grdMain_LayoutUpdated(object sender, EventArgs e)
        {
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
