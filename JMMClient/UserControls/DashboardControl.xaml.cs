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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DashboardControl.xaml
	/// </summary>
	public partial class DashboardControl : UserControl
	{
		BackgroundWorker refreshDataWorker = new BackgroundWorker();

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

			btnExpandDashWatchNext.Click += new RoutedEventHandler(btnExpandDashWatchNext_Click);
			btnExpandDashSeriesMissingEpisodes.Click += new RoutedEventHandler(btnExpandDashSeriesMissingEpisodes_Click);
			btnExpandDashMiniCalendar.Click += new RoutedEventHandler(btnExpandDashMiniCalendar_Click);
			btnEditDashboard.Click += new RoutedEventHandler(btnEditDashboard_Click);
			btnEditDashboardFinish.Click += new RoutedEventHandler(btnEditDashboardFinish_Click);

			btnWatchNextIncrease.Click += new RoutedEventHandler(btnWatchNextIncrease_Click);
			btnWatchNextReduce.Click += new RoutedEventHandler(btnWatchNextReduce_Click);

			btnMissingEpsIncrease.Click += new RoutedEventHandler(btnMissingEpsIncrease_Click);
			btnMissingEpsReduce.Click += new RoutedEventHandler(btnMissingEpsReduce_Click);

			btnMiniCalendarIncrease.Click += new RoutedEventHandler(btnMiniCalendarIncrease_Click);
			btnMiniCalendarReduce.Click += new RoutedEventHandler(btnMiniCalendarReduce_Click);

			udItemsWatchNext.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsWatchNext_ValueChanged);
			udDaysMiniCalendar.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udDaysMiniCalendar_ValueChanged);
			udItemsMissingEps.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udItemsMissingEps_ValueChanged);

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

			DashPos_WatchNextEpisode = DashPos_WatchNextEpisode * 2;
			DashPos_SeriesMissingEpisodes = DashPos_SeriesMissingEpisodes * 2;
			DashPos_MiniCalendar = DashPos_MiniCalendar * 2;

			DashPos_WatchNextEpisode_Header = DashPos_WatchNextEpisode - 1;
			DashPos_SeriesMissingEpisodes_Header = DashPos_SeriesMissingEpisodes - 1;
			DashPos_MiniCalendar_Header = DashPos_MiniCalendar - 1;
		}
	}
}
