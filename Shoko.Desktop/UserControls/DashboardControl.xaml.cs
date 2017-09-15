using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NLog;
using Shoko.Desktop.Enums;
using Shoko.Models.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl : UserControl
    {
        BackgroundWorker refreshDataWorker = new BackgroundWorker();
        BackgroundWorker getMissingDataWorker = new BackgroundWorker();

        public static readonly DependencyProperty DashPos_WatchNextEpisodeProperty = DependencyProperty.Register("DashPos_WatchNextEpisode",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_WatchNextEpisode
        {
            get { return (int)GetValue(DashPos_WatchNextEpisodeProperty); }
            set { SetValue(DashPos_WatchNextEpisodeProperty, value); }
        }

        public static readonly DependencyProperty DashPos_WatchNextEpisode_HeaderProperty = DependencyProperty.Register("DashPos_WatchNextEpisode_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_WatchNextEpisode_Header
        {
            get { return (int)GetValue(DashPos_WatchNextEpisode_HeaderProperty); }
            set { SetValue(DashPos_WatchNextEpisode_HeaderProperty, value); }
        }


        public static readonly DependencyProperty DashPos_RecentAdditionsProperty = DependencyProperty.Register("DashPos_RecentAdditions",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecentAdditions
        {
            get { return (int)GetValue(DashPos_RecentAdditionsProperty); }
            set { SetValue(DashPos_RecentAdditionsProperty, value); }
        }

        public static readonly DependencyProperty DashPos_RecentAdditions_HeaderProperty = DependencyProperty.Register("DashPos_RecentAdditions_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecentAdditions_Header
        {
            get { return (int)GetValue(DashPos_RecentAdditions_HeaderProperty); }
            set { SetValue(DashPos_RecentAdditions_HeaderProperty, value); }
        }



        public static readonly DependencyProperty DashPos_RecentlyWatchedEpisodeProperty = DependencyProperty.Register("DashPos_RecentlyWatchedEpisode",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecentlyWatchedEpisode
        {
            get { return (int)GetValue(DashPos_RecentlyWatchedEpisodeProperty); }
            set { SetValue(DashPos_RecentlyWatchedEpisodeProperty, value); }
        }

        public static readonly DependencyProperty DashPos_RecentlyWatchedEpisode_HeaderProperty = DependencyProperty.Register("DashPos_RecentlyWatchedEpisode_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecentlyWatchedEpisode_Header
        {
            get { return (int)GetValue(DashPos_RecentlyWatchedEpisode_HeaderProperty); }
            set { SetValue(DashPos_RecentlyWatchedEpisode_HeaderProperty, value); }
        }







        public static readonly DependencyProperty DashPos_SeriesMissingEpisodesProperty = DependencyProperty.Register("DashPos_SeriesMissingEpisodes",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_SeriesMissingEpisodes
        {
            get { return (int)GetValue(DashPos_SeriesMissingEpisodesProperty); }
            set { SetValue(DashPos_SeriesMissingEpisodesProperty, value); }
        }

        public static readonly DependencyProperty DashPos_SeriesMissingEpisodes_HeaderProperty = DependencyProperty.Register("DashPos_SeriesMissingEpisodes_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_SeriesMissingEpisodes_Header
        {
            get { return (int)GetValue(DashPos_SeriesMissingEpisodes_HeaderProperty); }
            set { SetValue(DashPos_SeriesMissingEpisodes_HeaderProperty, value); }
        }

        // mini calendar position
        public static readonly DependencyProperty DashPos_MiniCalendarProperty = DependencyProperty.Register("DashPos_MiniCalendar",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_MiniCalendar
        {
            get { return (int)GetValue(DashPos_MiniCalendarProperty); }
            set { SetValue(DashPos_MiniCalendarProperty, value); }
        }

        public static readonly DependencyProperty DashPos_MiniCalendar_HeaderProperty = DependencyProperty.Register("DashPos_MiniCalendar_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_MiniCalendar_Header
        {
            get { return (int)GetValue(DashPos_MiniCalendar_HeaderProperty); }
            set { SetValue(DashPos_MiniCalendar_HeaderProperty, value); }
        }

        // recommendations watch position
        public static readonly DependencyProperty DashPos_RecWatchProperty = DependencyProperty.Register("DashPos_RecWatch",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecWatch
        {
            get { return (int)GetValue(DashPos_RecWatchProperty); }
            set { SetValue(DashPos_RecWatchProperty, value); }
        }

        public static readonly DependencyProperty DashPos_RecWatch_HeaderProperty = DependencyProperty.Register("DashPos_RecWatch_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecWatch_Header
        {
            get { return (int)GetValue(DashPos_RecWatch_HeaderProperty); }
            set { SetValue(DashPos_RecWatch_HeaderProperty, value); }
        }

        // recommendations Download position
        public static readonly DependencyProperty DashPos_RecDownloadProperty = DependencyProperty.Register("DashPos_RecDownload",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecDownload
        {
            get { return (int)GetValue(DashPos_RecDownloadProperty); }
            set { SetValue(DashPos_RecDownloadProperty, value); }
        }

        public static readonly DependencyProperty DashPos_RecDownload_HeaderProperty = DependencyProperty.Register("DashPos_RecDownload_Header",
            typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

        public int DashPos_RecDownload_Header
        {
            get { return (int)GetValue(DashPos_RecDownload_HeaderProperty); }
            set { SetValue(DashPos_RecDownload_HeaderProperty, value); }
        }


        public static readonly DependencyProperty IsLoadingDataProperty = DependencyProperty.Register("IsLoadingData",
            typeof(bool), typeof(DashboardControl), new UIPropertyMetadata(false, null));

        public bool IsLoadingData
        {
            get { return (bool)GetValue(IsLoadingDataProperty); }
            set { SetValue(IsLoadingDataProperty, value); }
        }

        public DashboardControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboDashWatchNextStyle.Items.Clear();
            cboDashWatchNextStyle.Items.Add(Shoko.Commons.Properties.Resources.DashWatchNextStyle_Simple);
            cboDashWatchNextStyle.Items.Add(Shoko.Commons.Properties.Resources.DashWatchNextStyle_Detailed);

            if (VM_UserSettings.Instance.Dash_WatchNext_Style == DashWatchNextStyle.Simple)
                cboDashWatchNextStyle.SelectedIndex = 0;
            else
                cboDashWatchNextStyle.SelectedIndex = 1;

            cboDashWatchNextStyle.SelectionChanged += new SelectionChangedEventHandler(cboDashWatchNextStyle_SelectionChanged);

            cboDashRecentAdditionsType.Items.Clear();
            cboDashRecentAdditionsType.Items.Add(Shoko.Commons.Properties.Resources.Anime_Episodes);
            cboDashRecentAdditionsType.Items.Add(Shoko.Commons.Properties.Resources.Series);
            cboDashRecentAdditionsType.SelectedIndex = AppSettings.DashRecentAdditionsType;
            cboDashRecentAdditionsType.SelectionChanged += new SelectionChangedEventHandler(cboDashRecentAdditionsType_SelectionChanged);

            btnToolbarRefresh.Click += new RoutedEventHandler(btnToolbarRefresh_Click);

            refreshDataWorker.DoWork += new DoWorkEventHandler(refreshDataWorker_DoWork);
            refreshDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshDataWorker_RunWorkerCompleted);

            getMissingDataWorker.DoWork += new DoWorkEventHandler(getMissingDataWorker_DoWork);
            getMissingDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getMissingDataWorker_RunWorkerCompleted);

            btnExpandDashWatchNext.Click += new RoutedEventHandler(btnExpandDashWatchNext_Click);
            btnExpandDashSeriesMissingEpisodes.Click += new RoutedEventHandler(btnExpandDashSeriesMissingEpisodes_Click);
            btnExpandDashMiniCalendar.Click += new RoutedEventHandler(btnExpandDashMiniCalendar_Click);
            btnExpandRecWatch.Click += new RoutedEventHandler(btnExpandRecWatch_Click);
            btnExpandRecDownload.Click += new RoutedEventHandler(btnExpandRecDownload_Click);
            btnExpandDashRecentEpisodes.Click += new RoutedEventHandler(btnExpandDashRecentEpisodes_Click);
            btnExpandDashRecentAdditions.Click += new RoutedEventHandler(btnExpandDashRecentAdditions_Click);

            btnEditDashboard.Click += new RoutedEventHandler(btnEditDashboard_Click);
            btnEditDashboardFinish.Click += new RoutedEventHandler(btnEditDashboardFinish_Click);

            btnWatchNextIncrease.Click += new RoutedEventHandler(btnWatchNextIncrease_Click);
            btnWatchNextReduce.Click += new RoutedEventHandler(btnWatchNextReduce_Click);

            btnRecentEpisodesIncrease.Click += new RoutedEventHandler(btnRecentEpisodesIncrease_Click);
            btnRecentEpisodesReduce.Click += new RoutedEventHandler(btnRecentEpisodesReduce_Click);

            btnMissingEpsIncrease.Click += new RoutedEventHandler(btnMissingEpsIncrease_Click);
            btnMissingEpsReduce.Click += new RoutedEventHandler(btnMissingEpsReduce_Click);

            btnMiniCalendarIncrease.Click += new RoutedEventHandler(btnMiniCalendarIncrease_Click);
            btnMiniCalendarReduce.Click += new RoutedEventHandler(btnMiniCalendarReduce_Click);

            btnRecWatchIncrease.Click += new RoutedEventHandler(btnRecWatchIncrease_Click);
            btnRecWatchReduce.Click += new RoutedEventHandler(btnRecWatchReduce_Click);

            btnRecDownloadIncrease.Click += new RoutedEventHandler(btnRecDownloadIncrease_Click);
            btnRecDownloadReduce.Click += new RoutedEventHandler(btnRecDownloadReduce_Click);

            btnRecentAdditionsIncrease.Click += new RoutedEventHandler(btnRecentAdditionsIncrease_Click);
            btnRecentAdditionsReduce.Click += new RoutedEventHandler(btnRecentAdditionsReduce_Click);

            udItemsWatchNext.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsWatchNext_ValueChanged);
            udDaysMiniCalendar.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udDaysMiniCalendar_ValueChanged);
            udItemsMissingEps.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsMissingEps_ValueChanged);
            udItemsRecWatch.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecWatch_ValueChanged);
            udItemsRecDownload.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecDownload_ValueChanged);
            udItemsRecentEpisodes.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecentEpisodes_ValueChanged);
            udItemsRecentAdditions.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecentAdditions_ValueChanged);

            btnGetRecDownloadMissingInfo.Click += new RoutedEventHandler(btnGetRecDownloadMissingInfo_Click);
            btnForceCalendarRefresh.Click += btnForceCalendarRefresh_Click;

            chkCalUpcomingOnly.Click += chkCalUpcomingOnly_Click;

            SetWidgetOrder();

            MainWindow.videoHandler.VideoWatchedEvent += new VideoPlayers.VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

            btnToggleDash.Click += new RoutedEventHandler(btnToggleDash_Click);
        }





        void btnToggleDash_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
            mainwdw.ShowDashMetroView(MetroViews.MainMetro);
        }

        void videoHandler_VideoWatchedEvent(VideoPlayers.VideoWatchedEventArgs ev)
        {
            try
            {
                MainWindow mainwdw = (MainWindow) Window.GetWindow(this);

                if (MainWindow.CurrentMainTabIndex == (int) MainWindow.TAB_MAIN.Dashboard &&
                    mainwdw?.dash.Visibility == Visibility.Visible)
                    RefreshData(true, false, false);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }
        }


        void chkCalUpcomingOnly_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboDashWatchNextStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cboDashWatchNextStyle.SelectedItem.ToString() == Shoko.Commons.Properties.Resources.DashWatchNextStyle_Simple)
                VM_UserSettings.Instance.Dash_WatchNext_Style = DashWatchNextStyle.Simple;
            else
                VM_UserSettings.Instance.Dash_WatchNext_Style = DashWatchNextStyle.Detailed;
        }

        void cboDashRecentAdditionsType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM_UserSettings.Instance.DashRecentAdditionsType = cboDashRecentAdditionsType.SelectedIndex;
            RefreshData(false, true, false);
        }

        void udItemsRecentAdditions_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_RecentAdditions_Items = udItemsRecentAdditions.Value.Value;
        }

        void udItemsMissingEps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_MissingEps_Items = udItemsMissingEps.Value.Value;
        }

        void udDaysMiniCalendar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_MiniCalendarDays = udDaysMiniCalendar.Value.Value;
        }

        void udItemsWatchNext_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_WatchNext_Items = udItemsWatchNext.Value.Value;
        }



        void udItemsRecentEpisodes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_RecentlyWatchedEp_Items = udItemsRecentEpisodes.Value.Value;
        }



        void udItemsRecWatch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_RecWatch_Items = udItemsRecWatch.Value.Value;
        }

        void udItemsRecDownload_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM_UserSettings.Instance.Dash_RecDownload_Items = udItemsRecDownload.Value.Value;
        }

        void btnWatchNextReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_WatchNext_Height = VM_UserSettings.Instance.Dash_WatchNext_Height - 10;
        }

        void btnWatchNextIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_WatchNext_Height = VM_UserSettings.Instance.Dash_WatchNext_Height + 10;
        }

        void btnRecentAdditionsReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecentAdditions_Height = VM_UserSettings.Instance.Dash_RecentAdditions_Height - 10;
        }

        void btnRecentAdditionsIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecentAdditions_Height = VM_UserSettings.Instance.Dash_RecentAdditions_Height + 10;
        }

        void btnRecentEpisodesReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecentlyWatchedEp_Height = VM_UserSettings.Instance.Dash_RecentlyWatchedEp_Height - 10;
        }

        void btnRecentEpisodesIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecentlyWatchedEp_Height = VM_UserSettings.Instance.Dash_RecentlyWatchedEp_Height + 10;
        }




        void btnMissingEpsReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_MissingEps_Height = VM_UserSettings.Instance.Dash_MissingEps_Height - 10;
        }

        void btnMissingEpsIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_MissingEps_Height = VM_UserSettings.Instance.Dash_MissingEps_Height + 10;
        }

        void btnMiniCalendarReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_MiniCalendar_Height = VM_UserSettings.Instance.Dash_MiniCalendar_Height - 10;
        }

        void btnMiniCalendarIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_MiniCalendar_Height = VM_UserSettings.Instance.Dash_MiniCalendar_Height + 10;
        }

        void btnRecWatchReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecWatch_Height = VM_UserSettings.Instance.Dash_RecWatch_Height - 10;
        }

        void btnRecWatchIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecWatch_Height = VM_UserSettings.Instance.Dash_RecWatch_Height + 10;
        }

        void btnRecDownloadReduce_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecDownload_Height = VM_UserSettings.Instance.Dash_RecDownload_Height - 10;
        }

        void btnRecDownloadIncrease_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.Dash_RecDownload_Height = VM_UserSettings.Instance.Dash_RecDownload_Height + 10;
        }

        void btnEditDashboardFinish_Click(object sender, RoutedEventArgs e)
        {
            VM_Dashboard.Instance.IsBeingEdited = !VM_Dashboard.Instance.IsBeingEdited;
            VM_Dashboard.Instance.IsReadOnly = !VM_Dashboard.Instance.IsReadOnly;
        }

        void btnEditDashboard_Click(object sender, RoutedEventArgs e)
        {
            VM_Dashboard.Instance.IsBeingEdited = !VM_Dashboard.Instance.IsBeingEdited;
            VM_Dashboard.Instance.IsReadOnly = !VM_Dashboard.Instance.IsReadOnly;
        }

        void btnExpandDashSeriesMissingEpisodes_Click(object sender, RoutedEventArgs e)
        {
            if (VM_UserSettings.Instance.DashSeriesMissingEpisodesCollapsed && VM_Dashboard.Instance.SeriesMissingEps.Count == 0)
                VM_Dashboard.Instance.RefreshSeriesMissingEps();

            VM_UserSettings.Instance.DashSeriesMissingEpisodesExpanded = !VM_UserSettings.Instance.DashSeriesMissingEpisodesExpanded;
        }

        void btnExpandDashMiniCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (VM_UserSettings.Instance.DashMiniCalendarCollapsed && VM_Dashboard.Instance.MiniCalendar.Count == 0)
                VM_Dashboard.Instance.RefreshMiniCalendar();

            VM_UserSettings.Instance.DashMiniCalendarExpanded = !VM_UserSettings.Instance.DashMiniCalendarExpanded;
        }

        void btnExpandRecWatch_Click(object sender, RoutedEventArgs e)
        {
            if (VM_UserSettings.Instance.DashRecommendationsWatchCollapsed && VM_Dashboard.Instance.RecommendationsWatch.Count == 0)
                VM_Dashboard.Instance.RefreshRecommendationsWatch();

            VM_UserSettings.Instance.DashRecommendationsWatchExpanded = !VM_UserSettings.Instance.DashRecommendationsWatchExpanded;
        }

        void btnExpandRecDownload_Click(object sender, RoutedEventArgs e)
        {
            if (VM_UserSettings.Instance.DashRecommendationsDownloadCollapsed && VM_Dashboard.Instance.RecommendationsDownload.Count == 0)
                VM_Dashboard.Instance.RefreshRecommendationsDownload();

            VM_UserSettings.Instance.DashRecommendationsDownloadExpanded = !VM_UserSettings.Instance.DashRecommendationsDownloadExpanded;
        }

        void btnExpandDashWatchNext_Click(object sender, RoutedEventArgs e)
        {
            if (VM_UserSettings.Instance.DashWatchNextEpCollapsed && VM_Dashboard.Instance.EpsWatchNext_Recent.Count == 0)
                VM_Dashboard.Instance.RefreshEpsWatchNext_Recent();

            VM_UserSettings.Instance.DashWatchNextEpExpanded = !VM_UserSettings.Instance.DashWatchNextEpExpanded;
        }

        void btnExpandDashRecentEpisodes_Click(object sender, RoutedEventArgs e)
        {
            if (VM_UserSettings.Instance.DashRecentlyWatchEpsCollapsed && VM_Dashboard.Instance.EpsWatchedRecently.Count == 0)
                VM_Dashboard.Instance.RefreshRecentlyWatchedEps();

            VM_UserSettings.Instance.DashRecentlyWatchEpsExpanded = !VM_UserSettings.Instance.DashRecentlyWatchEpsExpanded;
        }

        void btnExpandDashRecentAdditions_Click(object sender, RoutedEventArgs e)
        {
            RecentAdditionsType addType = RecentAdditionsType.Episode;
            if (cboDashRecentAdditionsType.SelectedIndex == 0) addType = RecentAdditionsType.Episode;
            if (cboDashRecentAdditionsType.SelectedIndex == 1) addType = RecentAdditionsType.Series;

            if (VM_UserSettings.Instance.DashRecentAdditionsCollapsed && VM_Dashboard.Instance.RecentAdditions.Count == 0)
                VM_Dashboard.Instance.RefreshRecentAdditions(addType);

            VM_UserSettings.Instance.DashRecentAdditionsExpanded = !VM_UserSettings.Instance.DashRecentAdditionsExpanded;
        }

        void refreshDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Arrow;
            IsEnabled = true;
            IsLoadingData = false;
        }

        void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                RefreshOptions opt = e.Argument as RefreshOptions;

                VM_Dashboard.Instance.RefreshData(opt.RefreshContinueWatching, opt.RefreshRecentAdditions,
                    opt.RefreshOtherWidgets, opt.RecentAdditionType);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData(true, true, true);

        }

        private void RefreshData(bool refreshContinueWatching, bool refreshRecentAdditions, bool refreshOtherWidgets)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                Window parentWindow = Window.GetWindow(this);

                IsLoadingData = true;
                IsEnabled = false;
                parentWindow.Cursor = Cursors.Wait;

                RecentAdditionsType addType = RecentAdditionsType.Episode;
                if (cboDashRecentAdditionsType.SelectedIndex == 0) addType = RecentAdditionsType.Episode;
                if (cboDashRecentAdditionsType.SelectedIndex == 1) addType = RecentAdditionsType.Series;

                RefreshOptions opt = new RefreshOptions();
                opt.RefreshContinueWatching = refreshContinueWatching;
                opt.RefreshRecentAdditions = refreshRecentAdditions;
                opt.RefreshOtherWidgets = refreshOtherWidgets;
                opt.RecentAdditionType = addType;
                refreshDataWorker.RunWorkerAsync(opt);
            });
        }


        void getMissingDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Arrow;
            IsEnabled = true;
            IsLoadingData = false;
        }

        void getMissingDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VM_Dashboard.Instance.GetMissingRecommendationsDownload();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnGetRecDownloadMissingInfo_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            IsLoadingData = true;
            IsEnabled = false;
            parentWindow.Cursor = Cursors.Wait;
            getMissingDataWorker.RunWorkerAsync();
        }

        void btnForceCalendarRefresh_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.ShokoServices.UpdateCalendarData();

            MessageBox.Show(Shoko.Commons.Properties.Resources.ShokoServer_ProcessQueued, Shoko.Commons.Properties.Resources.ShokoServer_Running, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;

            try
            {
                Window parentWindow = Window.GetWindow(this);
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

                RefreshData(true, true, false);
                if (newStatus == true && ser != null)
                {
                    Utils.PromptToRateSeries(ser, parentWindow);
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

        private void CommandBinding_IgnoreAnimeWatch(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = obj as VM_Recommendation;
                    if (rec == null) return;

                    VM_ShokoServer.Instance.ShokoServices.IgnoreAnime(rec.RecommendedAnimeID, (int)RecommendationType.Watch,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_Dashboard.Instance.RefreshRecommendationsWatch();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_IgnoreAnimeDownload(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = obj as VM_Recommendation;
                    if (rec == null) return;

                    VM_ShokoServer.Instance.ShokoServices.IgnoreAnime(rec.RecommendedAnimeID, (int)RecommendationType.Download,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_Dashboard.Instance.RefreshRecommendationsDownload();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;


                    if (ep.FilesForEpisode.Count == 1)
                    {
                        bool force = true;
                        if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                            Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                        {
                            if (ep.FilesForEpisode[0].VideoLocal_ResumePosition > 0)
                            {
                                AskResumeVideo ask = new AskResumeVideo(ep.FilesForEpisode[0].VideoLocal_ResumePosition);
                                ask.Owner = Window.GetWindow(this);
                                if (ask.ShowDialog() == true)
                                    force = false;
                            }
                        }
                        MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0], force);
                    }
                    else if (ep.FilesForEpisode.Count > 1)
                    {
                        if (AppSettings.AutoFileSingleEpisode)
                        {
                            VM_VideoDetailed vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
                            if (vid != null)
                            {
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
                                MainWindow.videoHandler.PlayVideo(vid, force);
                            }
                        }
                        else
                        {
                            PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
                            frm.Owner = parentWindow;
                            frm.Init(ep);
                            bool? result = frm.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayAllUnwatchedEpisode(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;

                    MainWindow.videoHandler.PlayAllUnwatchedEpisodes(ep.AnimeSeriesID);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_SyncVotes(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(SyncVotesDummy))
                {

                    VM_ShokoServer.Instance.SyncVotes();
                    MessageBox.Show(Shoko.Commons.Properties.Resources.ShokoServer_ProcessRunning, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_MoveUpWidget(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            DashboardWidgets swid = (DashboardWidgets)int.Parse(obj.ToString());

            VM_UserSettings.Instance.MoveUpDashboardWidget(swid);
            SetWidgetOrder();
        }

        private void CommandBinding_MoveDownWidget(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            DashboardWidgets swid = (DashboardWidgets)int.Parse(obj.ToString());

            VM_UserSettings.Instance.MoveDownDashboardWidget(swid);
            SetWidgetOrder();
        }

        private void SetWidgetOrder()
        {

            DashPos_WatchNextEpisode = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.WatchNextEpisode);
            DashPos_SeriesMissingEpisodes = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.SeriesMissingEpisodes);
            DashPos_MiniCalendar = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.MiniCalendar);
            DashPos_RecWatch = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecommendationsWatch);
            DashPos_RecDownload = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecommendationsDownload);
            DashPos_RecentlyWatchedEpisode = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecentlyWatchedEpisode);
            DashPos_RecentAdditions = VM_UserSettings.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecentAdditions);

            DashPos_WatchNextEpisode = DashPos_WatchNextEpisode * 2;
            DashPos_SeriesMissingEpisodes = DashPos_SeriesMissingEpisodes * 2;
            DashPos_MiniCalendar = DashPos_MiniCalendar * 2;
            DashPos_RecWatch = DashPos_RecWatch * 2;
            DashPos_RecDownload = DashPos_RecDownload * 2;
            DashPos_RecentlyWatchedEpisode = DashPos_RecentlyWatchedEpisode * 2;
            DashPos_RecentAdditions = DashPos_RecentAdditions * 2;

            DashPos_WatchNextEpisode_Header = DashPos_WatchNextEpisode - 1;
            DashPos_SeriesMissingEpisodes_Header = DashPos_SeriesMissingEpisodes - 1;
            DashPos_MiniCalendar_Header = DashPos_MiniCalendar - 1;
            DashPos_RecWatch_Header = DashPos_RecWatch - 1;
            DashPos_RecDownload_Header = DashPos_RecDownload - 1;
            DashPos_RecentlyWatchedEpisode_Header = DashPos_RecentlyWatchedEpisode - 1;
            DashPos_RecentAdditions_Header = DashPos_RecentAdditions - 1;
        }
    }

    public class SyncVotesDummy
    {
    }

    public class RefreshOptions
    {
        public bool RefreshContinueWatching { get; set; }
        public bool RefreshRecentAdditions { get; set; }
        public bool RefreshOtherWidgets { get; set; }
        public RecentAdditionsType RecentAdditionType { get; set; }
    }
}
