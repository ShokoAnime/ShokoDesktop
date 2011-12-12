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

			btnEditDashboard.Click += new RoutedEventHandler(btnEditDashboard_Click);
			btnEditDashboardFinish.Click += new RoutedEventHandler(btnEditDashboardFinish_Click);

			btnWatchNextIncrease.Click += new RoutedEventHandler(btnWatchNextIncrease_Click);
			btnWatchNextReduce.Click += new RoutedEventHandler(btnWatchNextReduce_Click);

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

			btnGetRecDownloadMissingInfo.Click += new RoutedEventHandler(btnGetRecDownloadMissingInfo_Click);

			SetWidgetOrder();

			
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
				DashboardVM.Instance.RefreshTraktFriends();

			UserSettingsVM.Instance.DashTraktFriendsExpanded = !UserSettingsVM.Instance.DashTraktFriendsExpanded;
		}

		void btnExpandDashWatchNext_Click(object sender, RoutedEventArgs e)
		{
			if (UserSettingsVM.Instance.DashWatchNextEpCollapsed && DashboardVM.Instance.EpsWatchNext_Recent.Count == 0)
				DashboardVM.Instance.RefreshEpsWatchNext_Recent();

			UserSettingsVM.Instance.DashWatchNextEpExpanded = !UserSettingsVM.Instance.DashWatchNextEpExpanded;
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
				DashboardVM.Instance.RefreshData();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnToolbarRefresh_Click(object sender, RoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			IsLoadingData = true;
			this.IsEnabled = false;
			parentWindow.Cursor = Cursors.Wait;
			refreshDataWorker.RunWorkerAsync();
			
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


		private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			this.Cursor = Cursors.Wait;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					bool newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					MainListHelperVM.Instance.UpdateHeirarchy(vid);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					bool newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);
				}

				DashboardVM.Instance.RefreshData();
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
			DashPos_TraktFriends = UserSettingsVM.Instance.GetDashboardWidgetPosition(DashboardWidgets.TraktFriends);

			DashPos_WatchNextEpisode = DashPos_WatchNextEpisode * 2;
			DashPos_SeriesMissingEpisodes = DashPos_SeriesMissingEpisodes * 2;
			DashPos_MiniCalendar = DashPos_MiniCalendar * 2;
			DashPos_RecWatch = DashPos_RecWatch * 2;
			DashPos_RecDownload = DashPos_RecDownload * 2;
			DashPos_TraktFriends = DashPos_TraktFriends * 2;

			DashPos_WatchNextEpisode_Header = DashPos_WatchNextEpisode - 1;
			DashPos_SeriesMissingEpisodes_Header = DashPos_SeriesMissingEpisodes - 1;
			DashPos_MiniCalendar_Header = DashPos_MiniCalendar - 1;
			DashPos_RecWatch_Header = DashPos_RecWatch - 1;
			DashPos_RecDownload_Header = DashPos_RecDownload - 1;
			DashPos_TraktFriends_Header = DashPos_TraktFriends - 1;
		}
	}
}
