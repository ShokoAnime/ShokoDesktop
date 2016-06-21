using JMMClient.Forms;
using JMMClient.ViewModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
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
            cboDashWatchNextStyle.Items.Add(Properties.Resources.DashWatchNextStyle_Simple);
            cboDashWatchNextStyle.Items.Add(Properties.Resources.DashWatchNextStyle_Detailed);

            if (UserSettingsVM.Instance.Dash_WatchNext_Style == DashWatchNextStyle.Simple)
                cboDashWatchNextStyle.SelectedIndex = 0;
            else
                cboDashWatchNextStyle.SelectedIndex = 1;

            cboDashWatchNextStyle.SelectionChanged += new SelectionChangedEventHandler(cboDashWatchNextStyle_SelectionChanged);

            cboDashRecentAdditionsType.Items.Clear();
            cboDashRecentAdditionsType.Items.Add(JMMClient.Properties.Resources.Episodes);
            cboDashRecentAdditionsType.Items.Add(JMMClient.Properties.Resources.Series);
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

            MainWindow.videoHandler.VideoWatchedEvent += new Utilities.VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

            btnToggleDash.Click += new RoutedEventHandler(btnToggleDash_Click);
        }





        void btnToggleDash_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
            mainwdw.ShowDashMetroView(MetroViews.MainMetro);
        }

        void videoHandler_VideoWatchedEvent(Utilities.VideoWatchedEventArgs ev)
        {
            try
            {
                MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

                if (MainWindow.CurrentMainTabIndex == MainWindow.TAB_MAIN_Dashboard && mainwdw.dash.Visibility == System.Windows.Visibility.Visible)
                    RefreshData(true, false, false);
            }
            catch { }
        }


        void chkCalUpcomingOnly_Click(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void cboDashWatchNextStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cboDashWatchNextStyle.SelectedItem.ToString() == Properties.Resources.DashWatchNextStyle_Simple)
                UserSettingsVM.Instance.Dash_WatchNext_Style = DashWatchNextStyle.Simple;
            else
                UserSettingsVM.Instance.Dash_WatchNext_Style = DashWatchNextStyle.Detailed;
        }

        void cboDashRecentAdditionsType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserSettingsVM.Instance.DashRecentAdditionsType = cboDashRecentAdditionsType.SelectedIndex;
            RefreshData(false, true, false);
        }

        void udItemsRecentAdditions_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_RecentAdditions_Items = udItemsRecentAdditions.Value.Value;
        }

        void udItemsMissingEps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_MissingEps_Items = udItemsMissingEps.Value.Value;
        }

        void udDaysMiniCalendar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_MiniCalendarDays = udDaysMiniCalendar.Value.Value;
        }

        void udItemsWatchNext_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_WatchNext_Items = udItemsWatchNext.Value.Value;
        }



        void udItemsRecentEpisodes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_RecentlyWatchedEp_Items = udItemsRecentEpisodes.Value.Value;
        }



        void udItemsRecWatch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_RecWatch_Items = udItemsRecWatch.Value.Value;
        }

        void udItemsRecDownload_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.Dash_RecDownload_Items = udItemsRecDownload.Value.Value;
        }

        void btnWatchNextReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_WatchNext_Height = UserSettingsVM.Instance.Dash_WatchNext_Height - 10;
        }

        void btnWatchNextIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_WatchNext_Height = UserSettingsVM.Instance.Dash_WatchNext_Height + 10;
        }

        void btnRecentAdditionsReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecentAdditions_Height = UserSettingsVM.Instance.Dash_RecentAdditions_Height - 10;
        }

        void btnRecentAdditionsIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecentAdditions_Height = UserSettingsVM.Instance.Dash_RecentAdditions_Height + 10;
        }

        void btnRecentEpisodesReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecentlyWatchedEp_Height = UserSettingsVM.Instance.Dash_RecentlyWatchedEp_Height - 10;
        }

        void btnRecentEpisodesIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecentlyWatchedEp_Height = UserSettingsVM.Instance.Dash_RecentlyWatchedEp_Height + 10;
        }




        void btnMissingEpsReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_MissingEps_Height = UserSettingsVM.Instance.Dash_MissingEps_Height - 10;
        }

        void btnMissingEpsIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_MissingEps_Height = UserSettingsVM.Instance.Dash_MissingEps_Height + 10;
        }

        void btnMiniCalendarReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_MiniCalendar_Height = UserSettingsVM.Instance.Dash_MiniCalendar_Height - 10;
        }

        void btnMiniCalendarIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_MiniCalendar_Height = UserSettingsVM.Instance.Dash_MiniCalendar_Height + 10;
        }

        void btnRecWatchReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecWatch_Height = UserSettingsVM.Instance.Dash_RecWatch_Height - 10;
        }

        void btnRecWatchIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecWatch_Height = UserSettingsVM.Instance.Dash_RecWatch_Height + 10;
        }

        void btnRecDownloadReduce_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecDownload_Height = UserSettingsVM.Instance.Dash_RecDownload_Height - 10;
        }

        void btnRecDownloadIncrease_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.Dash_RecDownload_Height = UserSettingsVM.Instance.Dash_RecDownload_Height + 10;
        }

        void btnEditDashboardFinish_Click(object sender, RoutedEventArgs e)
        {
            DashboardVM.Instance.IsBeingEdited = !DashboardVM.Instance.IsBeingEdited;
            DashboardVM.Instance.IsReadOnly = !DashboardVM.Instance.IsReadOnly;
        }

        void btnEditDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardVM.Instance.IsBeingEdited = !DashboardVM.Instance.IsBeingEdited;
            DashboardVM.Instance.IsReadOnly = !DashboardVM.Instance.IsReadOnly;
        }

        void btnExpandDashSeriesMissingEpisodes_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettingsVM.Instance.DashSeriesMissingEpisodesCollapsed && DashboardVM.Instance.SeriesMissingEps.Count == 0)
                DashboardVM.Instance.RefreshSeriesMissingEps();

            UserSettingsVM.Instance.DashSeriesMissingEpisodesExpanded = !UserSettingsVM.Instance.DashSeriesMissingEpisodesExpanded;
        }

        void btnExpandDashMiniCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettingsVM.Instance.DashMiniCalendarCollapsed && DashboardVM.Instance.MiniCalendar.Count == 0)
                DashboardVM.Instance.RefreshMiniCalendar();

            UserSettingsVM.Instance.DashMiniCalendarExpanded = !UserSettingsVM.Instance.DashMiniCalendarExpanded;
        }

        void btnExpandRecWatch_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettingsVM.Instance.DashRecommendationsWatchCollapsed && DashboardVM.Instance.RecommendationsWatch.Count == 0)
                DashboardVM.Instance.RefreshRecommendationsWatch();

            UserSettingsVM.Instance.DashRecommendationsWatchExpanded = !UserSettingsVM.Instance.DashRecommendationsWatchExpanded;
        }

        void btnExpandRecDownload_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettingsVM.Instance.DashRecommendationsDownloadCollapsed && DashboardVM.Instance.RecommendationsDownload.Count == 0)
                DashboardVM.Instance.RefreshRecommendationsDownload();

            UserSettingsVM.Instance.DashRecommendationsDownloadExpanded = !UserSettingsVM.Instance.DashRecommendationsDownloadExpanded;
        }

        void btnExpandDashWatchNext_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettingsVM.Instance.DashWatchNextEpCollapsed && DashboardVM.Instance.EpsWatchNext_Recent.Count == 0)
                DashboardVM.Instance.RefreshEpsWatchNext_Recent();

            UserSettingsVM.Instance.DashWatchNextEpExpanded = !UserSettingsVM.Instance.DashWatchNextEpExpanded;
        }

        void btnExpandDashRecentEpisodes_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettingsVM.Instance.DashRecentlyWatchEpsCollapsed && DashboardVM.Instance.EpsWatchedRecently.Count == 0)
                DashboardVM.Instance.RefreshRecentlyWatchedEps();

            UserSettingsVM.Instance.DashRecentlyWatchEpsExpanded = !UserSettingsVM.Instance.DashRecentlyWatchEpsExpanded;
        }

        void btnExpandDashRecentAdditions_Click(object sender, RoutedEventArgs e)
        {
            RecentAdditionsType addType = RecentAdditionsType.Episode;
            if (cboDashRecentAdditionsType.SelectedIndex == 0) addType = RecentAdditionsType.Episode;
            if (cboDashRecentAdditionsType.SelectedIndex == 1) addType = RecentAdditionsType.Series;

            if (UserSettingsVM.Instance.DashRecentAdditionsCollapsed && DashboardVM.Instance.RecentAdditions.Count == 0)
                DashboardVM.Instance.RefreshRecentAdditions(addType);

            UserSettingsVM.Instance.DashRecentAdditionsExpanded = !UserSettingsVM.Instance.DashRecentAdditionsExpanded;
        }

        void refreshDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Arrow;
            this.IsEnabled = true;
            IsLoadingData = false;
        }

        void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                RefreshOptions opt = e.Argument as RefreshOptions;

                DashboardVM.Instance.RefreshData(opt.RefreshContinueWatching, opt.RefreshRecentAdditions,
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
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                Window parentWindow = Window.GetWindow(this);

                IsLoadingData = true;
                this.IsEnabled = false;
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
            this.IsEnabled = true;
            IsLoadingData = false;
        }

        void getMissingDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DashboardVM.Instance.GetMissingRecommendationsDownload();
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
            this.IsEnabled = false;
            parentWindow.Cursor = Cursors.Wait;
            getMissingDataWorker.RunWorkerAsync();
        }

        void btnForceCalendarRefresh_Click(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.clientBinaryHTTP.UpdateCalendarData();

            MessageBox.Show(JMMClient.Properties.Resources.JMMServer_ProcessQueued, JMMClient.Properties.Resources.JMMServer_Running, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            this.Cursor = Cursors.Wait;

            try
            {
                Window parentWindow = Window.GetWindow(this);
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
                        MessageBox.Show(response.ErrorMessage, JMMClient.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

                    ser = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
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
                this.Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_IgnoreAnimeWatch(object sender, ExecutedRoutedEventArgs e)
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

                    JMMServerVM.Instance.clientBinaryHTTP.IgnoreAnime(rec.RecommendedAnimeID, (int)RecommendationType.Watch,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                    DashboardVM.Instance.RefreshRecommendationsWatch();
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
                if (obj.GetType() == typeof(RecommendationVM))
                {
                    RecommendationVM rec = obj as RecommendationVM;
                    if (rec == null) return;

                    JMMServerVM.Instance.clientBinaryHTTP.IgnoreAnime(rec.RecommendedAnimeID, (int)RecommendationType.Download,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                    DashboardVM.Instance.RefreshRecommendationsDownload();
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
                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = obj as AnimeEpisodeVM;

                    if (ep.FilesForEpisode.Count == 1)
                        MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0]);
                    else if (ep.FilesForEpisode.Count > 1)
                    {
                        if (AppSettings.AutoFileSingleEpisode)
                        {
                            VideoDetailedVM vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
                            if (vid != null)
                                MainWindow.videoHandler.PlayVideo(vid);
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
                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = obj as AnimeEpisodeVM;

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

                    JMMServerVM.Instance.SyncVotes();
                    MessageBox.Show(JMMClient.Properties.Resources.JMMServer_ProcessRunning, JMMClient.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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

            UserSettingsVM.Instance.MoveUpDashboardWidget(swid);
            SetWidgetOrder();
        }

        private void CommandBinding_MoveDownWidget(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            DashboardWidgets swid = (DashboardWidgets)int.Parse(obj.ToString());

            UserSettingsVM.Instance.MoveDownDashboardWidget(swid);
            SetWidgetOrder();
        }

        private void SetWidgetOrder()
        {

            DashPos_WatchNextEpisode = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.WatchNextEpisode);
            DashPos_SeriesMissingEpisodes = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.SeriesMissingEpisodes);
            DashPos_MiniCalendar = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.MiniCalendar);
            DashPos_RecWatch = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecommendationsWatch);
            DashPos_RecDownload = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecommendationsDownload);
            DashPos_RecentlyWatchedEpisode = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecentlyWatchedEpisode);
            DashPos_RecentAdditions = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecentAdditions);

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
