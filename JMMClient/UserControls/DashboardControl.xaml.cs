using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JMMClient.Forms;
using System.ComponentModel;
using JMMClient.ViewModel;

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

		// Trakt Friends Download position
		public static readonly DependencyProperty DashPos_TraktFriendsProperty = DependencyProperty.Register("DashPos_TraktFriends",
			typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

		public int DashPos_TraktFriends
		{
			get { return (int)GetValue(DashPos_TraktFriendsProperty); }
			set { SetValue(DashPos_TraktFriendsProperty, value); }
		}

		public static readonly DependencyProperty DashPos_TraktFriends_HeaderProperty = DependencyProperty.Register("DashPos_TraktFriends_Header",
			typeof(int), typeof(DashboardControl), new UIPropertyMetadata((int)1, null));

		public int DashPos_TraktFriends_Header
		{
			get { return (int)GetValue(DashPos_TraktFriends_HeaderProperty); }
			set { SetValue(DashPos_TraktFriends_HeaderProperty, value); }
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

			cboDashWatchNextStyle.Items.Clear();
			cboDashWatchNextStyle.Items.Add(Properties.Resources.DashWatchNextStyle_Simple);
			cboDashWatchNextStyle.Items.Add(Properties.Resources.DashWatchNextStyle_Detailed);

			if (UserSettingsVM.Instance.Dash_WatchNext_Style == DashWatchNextStyle.Simple)
				cboDashWatchNextStyle.SelectedIndex = 0;
			else
				cboDashWatchNextStyle.SelectedIndex = 1;

			cboDashWatchNextStyle.SelectionChanged += new SelectionChangedEventHandler(cboDashWatchNextStyle_SelectionChanged);

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
			btnExpandTraktFriends.Click += new RoutedEventHandler(btnExpandTraktFriends_Click);
			btnExpandDashRecentEpisodes.Click += new RoutedEventHandler(btnExpandDashRecentEpisodes_Click);

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

			btnTraktFriendsIncrease.Click += new RoutedEventHandler(btnTraktFriendsIncrease_Click);
			btnTraktFriendsReduce.Click += new RoutedEventHandler(btnTraktFriendsReduce_Click);

			udItemsWatchNext.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsWatchNext_ValueChanged);
			udDaysMiniCalendar.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udDaysMiniCalendar_ValueChanged);
			udItemsMissingEps.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsMissingEps_ValueChanged);
			udItemsRecWatch.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecWatch_ValueChanged);
			udItemsRecDownload.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecDownload_ValueChanged);
			udItemsTraktFriends.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsTraktFriends_ValueChanged);
			udItemsRecentEpisodes.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsRecentEpisodes_ValueChanged);

			btnGetRecDownloadMissingInfo.Click += new RoutedEventHandler(btnGetRecDownloadMissingInfo_Click);
			btnForceTraktRefresh.Click += new RoutedEventHandler(btnForceTraktRefresh_Click);

			chkTraktAnimeOnly.Click += new RoutedEventHandler(chkTraktAnimeOnly_Click);

			SetWidgetOrder();

			togTraktScrobbles.Click += new RoutedEventHandler(togTraktScrobbles_Click);
			togTraktShouts.Click += new RoutedEventHandler(togTraktShouts_Click);
		}

		void togTraktShouts_Click(object sender, RoutedEventArgs e)
		{
			//RefreshData();
			if (UserSettingsVM.Instance.DashTraktFriendsExpanded)
				DashboardVM.Instance.RefreshTraktFriends(togTraktScrobbles.IsChecked.Value, togTraktShouts.IsChecked.Value);
		}

		void togTraktScrobbles_Click(object sender, RoutedEventArgs e)
		{
			if (UserSettingsVM.Instance.DashTraktFriendsExpanded)
				DashboardVM.Instance.RefreshTraktFriends(togTraktScrobbles.IsChecked.Value, togTraktShouts.IsChecked.Value);
		}

		

		

		

		

		void chkTraktAnimeOnly_Click(object sender, RoutedEventArgs e)
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

		void udItemsTraktFriends_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			UserSettingsVM.Instance.Dash_TraktFriends_Items = udItemsTraktFriends.Value.Value;
		}
		
		void btnWatchNextReduce_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.Dash_WatchNext_Height = UserSettingsVM.Instance.Dash_WatchNext_Height - 10;
		}

		void btnWatchNextIncrease_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.Dash_WatchNext_Height = UserSettingsVM.Instance.Dash_WatchNext_Height + 10;
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



		void btnTraktFriendsReduce_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.Dash_TraktFriends_Height = UserSettingsVM.Instance.Dash_TraktFriends_Height - 10;
		}

		void btnTraktFriendsIncrease_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.Dash_TraktFriends_Height = UserSettingsVM.Instance.Dash_TraktFriends_Height + 10;
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

		void btnExpandTraktFriends_Click(object sender, RoutedEventArgs e)
		{
			if (UserSettingsVM.Instance.DashTraktFriendsCollapsed && DashboardVM.Instance.TraktActivity.Count == 0)
				DashboardVM.Instance.RefreshTraktFriends(togTraktScrobbles.IsChecked.Value, togTraktShouts.IsChecked.Value);

			UserSettingsVM.Instance.DashTraktFriendsExpanded = !UserSettingsVM.Instance.DashTraktFriendsExpanded;
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

				DashboardVM.Instance.RefreshData(opt.TraktScrobbles, opt.TraktShouts, opt.OnlyContinueWatching);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnToolbarRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshData(false);
			
		}

		private void RefreshData(bool onlyContinueWatching)
		{
			Window parentWindow = Window.GetWindow(this);

			IsLoadingData = true;
			this.IsEnabled = false;
			parentWindow.Cursor = Cursors.Wait;

			RefreshOptions opt = new RefreshOptions();
			opt.TraktScrobbles = togTraktScrobbles.IsChecked.Value;
			opt.TraktShouts = togTraktShouts.IsChecked.Value;
			opt.OnlyContinueWatching = onlyContinueWatching;
			refreshDataWorker.RunWorkerAsync(opt);
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


		void btnForceTraktRefresh_Click(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.clientBinaryHTTP.RefreshTraktFriendInfo();

			MessageBox.Show("Process is running on server, please try refreshing in a few seconds", "Running", MessageBoxButton.OK, MessageBoxImage.Information);
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
						MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

					ser = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
				}

				RefreshData(true);
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
						Utils.PlayVideo(ep.FilesForEpisode[0]);
					else if (ep.FilesForEpisode.Count > 1)
					{
						PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
						frm.Owner = parentWindow;
						frm.Init(ep);
						bool? result = frm.ShowDialog();
						if (result.Value)
						{
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_JoinTrakt(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(Trakt_SignupVM))
				{
					Trakt_SignupVM signup = obj as Trakt_SignupVM;
					if (signup == null) return;

					parentWindow.Cursor = Cursors.Wait;
					string retMessage = "";
					bool success = JMMServerVM.Instance.clientBinaryHTTP.CreateTraktAccount(signup.Username, signup.Password, signup.Email, ref retMessage);
					parentWindow.Cursor = Cursors.Arrow;

					if (success)
					{
						MessageBox.Show(retMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
						JMMServerVM.Instance.GetServerSettings();
						DashboardVM.Instance.RefreshTraktFriends(togTraktScrobbles.IsChecked.Value, togTraktShouts.IsChecked.Value);
					}
					else
						MessageBox.Show(retMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					
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
					MessageBox.Show("Process is Running on server, please try refreshing when it has finished", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_FriendRequestDeny(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(Trakt_FriendRequestVM))
				{
					Trakt_FriendRequestVM req = obj as Trakt_FriendRequestVM;
					if (req == null) return;

					TraktFriendApproveDeny(req, false);

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_FriendRequestApprove(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(Trakt_FriendRequestVM))
				{
					Trakt_FriendRequestVM req = obj as Trakt_FriendRequestVM;
					if (req == null) return;

					TraktFriendApproveDeny(req, true);

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void TraktFriendApproveDeny(Trakt_FriendRequestVM req, bool isApprove)
		{
			try
			{
				Window parentWindow = Window.GetWindow(this);
				parentWindow.Cursor = Cursors.Wait;
				string retMessage = "";

				bool success = false;
				if (isApprove)
					success = JMMServerVM.Instance.clientBinaryHTTP.TraktFriendRequestApprove(req.Username, ref retMessage);
				else
					success = JMMServerVM.Instance.clientBinaryHTTP.TraktFriendRequestDeny(req.Username, ref retMessage);
				parentWindow.Cursor = Cursors.Arrow;

				if (success)
				{
					MessageBox.Show(retMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
					DashboardVM.Instance.RefreshTraktFriends(togTraktScrobbles.IsChecked.Value, togTraktShouts.IsChecked.Value);
				}
				else
					MessageBox.Show(retMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

				
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
			DashPos_TraktFriends = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.TraktFriends);
			DashPos_RecentlyWatchedEpisode = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.RecentlyWatchedEpisode);

			DashPos_WatchNextEpisode = DashPos_WatchNextEpisode * 2;
			DashPos_SeriesMissingEpisodes = DashPos_SeriesMissingEpisodes * 2;
			DashPos_MiniCalendar = DashPos_MiniCalendar * 2;
			DashPos_RecWatch = DashPos_RecWatch * 2;
			DashPos_RecDownload = DashPos_RecDownload * 2;
			DashPos_TraktFriends = DashPos_TraktFriends * 2;
			DashPos_RecentlyWatchedEpisode = DashPos_RecentlyWatchedEpisode * 2;

			DashPos_WatchNextEpisode_Header = DashPos_WatchNextEpisode - 1;
			DashPos_SeriesMissingEpisodes_Header = DashPos_SeriesMissingEpisodes - 1;
			DashPos_MiniCalendar_Header = DashPos_MiniCalendar - 1;
			DashPos_RecWatch_Header = DashPos_RecWatch - 1;
			DashPos_RecDownload_Header = DashPos_RecDownload - 1;
			DashPos_TraktFriends_Header = DashPos_TraktFriends - 1;
			DashPos_RecentlyWatchedEpisode_Header = DashPos_RecentlyWatchedEpisode - 1;
		}
	}

	public class SyncVotesDummy
	{
	}

	public class RefreshOptions
	{
		public bool TraktScrobbles { get; set; }
		public bool TraktShouts { get; set; }
		public bool OnlyContinueWatching { get; set; }
	}
}
