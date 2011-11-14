using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient
{
	public class UserSettingsVM : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private static UserSettingsVM _instance;
		public static UserSettingsVM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new UserSettingsVM();
				}
				return _instance;
			}
		}


		public bool CategoriesExpanded
		{
			get { return AppSettings.CategoriesExpanded; }
			set
			{
				AppSettings.CategoriesExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("CategoriesExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("CategoriesCollapsed"));
			}
		}

		public bool CategoriesCollapsed
		{
			get { return !AppSettings.CategoriesExpanded; }
		}

		public bool TagsExpanded
		{
			get { return AppSettings.TagsExpanded; }
			set
			{
				AppSettings.TagsExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("TagsExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("TagsCollapsed"));
			}
		}

		public bool TagsCollapsed
		{
			get { return !AppSettings.TagsExpanded; }
		}

		public bool TitlesExpanded
		{
			get { return AppSettings.TitlesExpanded; }
			set
			{
				AppSettings.TitlesExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("TitlesExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("TitlesCollapsed"));
			}
		}

		public bool TitlesCollapsed
		{
			get { return !AppSettings.TitlesExpanded; }
		}

		public bool SeriesTvDBLinksExpanded
		{
			get { return AppSettings.SeriesTvDBLinksExpanded; }
			set
			{
				AppSettings.SeriesTvDBLinksExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesTvDBLinksExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesTvDBLinksCollapsed"));
			}
		}

		public bool SeriesTvDBLinksCollapsed
		{
			get { return !AppSettings.SeriesTvDBLinksExpanded; }
		}

		public bool SeriesNextEpisodeExpanded
		{
			get { return AppSettings.SeriesNextEpisodeExpanded; }
			set
			{
				AppSettings.SeriesNextEpisodeExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesNextEpisodeExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesNextEpisodeCollapsed"));
			}
		}

		public bool SeriesNextEpisodeCollapsed
		{
			get { return !AppSettings.SeriesNextEpisodeExpanded; }
		}

		public bool SeriesFileSummaryExpanded
		{
			get { return AppSettings.SeriesFileSummaryExpanded; }
			set
			{
				AppSettings.SeriesFileSummaryExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesFileSummaryExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesFileSummaryCollapsed"));
			}
		}

		public bool SeriesFileSummaryCollapsed
		{
			get { return !AppSettings.SeriesFileSummaryExpanded; }
		}

		public bool SeriesGroupExpanded
		{
			get { return AppSettings.SeriesGroupExpanded; }
			set
			{
				AppSettings.SeriesGroupExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesGroupExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("SeriesGroupCollapsed"));
			}
		}

		public bool SeriesGroupCollapsed
		{
			get { return !AppSettings.SeriesGroupExpanded; }
		}

		public bool DashWatchNextEpExpanded
		{
			get { return AppSettings.DashWatchNextEpExpanded; }
			set
			{
				AppSettings.DashWatchNextEpExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DashWatchNextEpExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("DashWatchNextEpCollapsed"));
			}
		}

		public bool DashWatchNextEpCollapsed
		{
			get { return !AppSettings.DashWatchNextEpExpanded; }
		}

		public bool DashSeriesMissingEpisodesExpanded
		{
			get { return AppSettings.DashSeriesMissingEpisodesExpanded; }
			set
			{
				AppSettings.DashSeriesMissingEpisodesExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DashSeriesMissingEpisodesExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("DashSeriesMissingEpisodesCollapsed"));
			}
		}

		public bool DashSeriesMissingEpisodesCollapsed
		{
			get { return !AppSettings.DashSeriesMissingEpisodesExpanded; }
		}

		public bool DashMiniCalendarExpanded
		{
			get { return AppSettings.DashMiniCalendarExpanded; }
			set
			{
				AppSettings.DashMiniCalendarExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DashMiniCalendarExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("DashMiniCalendarCollapsed"));
			}
		}

		public bool DashRecommendationsWatchCollapsed
		{
			get { return !AppSettings.DashRecommendationsWatchExpanded; }
		}

		public bool DashRecommendationsWatchExpanded
		{
			get { return AppSettings.DashRecommendationsWatchExpanded; }
			set
			{
				AppSettings.DashRecommendationsWatchExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DashRecommendationsWatchExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("DashRecommendationsWatchCollapsed"));
			}
		}

		public bool DashRecommendationsDownloadCollapsed
		{
			get { return !AppSettings.DashRecommendationsDownloadExpanded; }
		}

		public bool DashRecommendationsDownloadExpanded
		{
			get { return AppSettings.DashRecommendationsDownloadExpanded; }
			set
			{
				AppSettings.DashRecommendationsDownloadExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DashRecommendationsDownloadExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("DashRecommendationsDownloadCollapsed"));
			}
		}


		public bool DashTraktFriendsCollapsed
		{
			get { return !AppSettings.DashTraktFriendsExpanded; }
		}

		public bool DashTraktFriendsExpanded
		{
			get { return AppSettings.DashTraktFriendsExpanded; }
			set
			{
				AppSettings.DashTraktFriendsExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DashTraktFriendsExpanded"));
				OnPropertyChanged(new PropertyChangedEventArgs("DashTraktFriendsCollapsed"));
			}
		}

		public bool DashMiniCalendarCollapsed
		{
			get { return !AppSettings.DashMiniCalendarExpanded; }
		}

		public int DisplayHeight_GroupList
		{
			get { return AppSettings.DisplayHeight_GroupList; }
			set
			{
				AppSettings.DisplayHeight_GroupList = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DisplayHeight_GroupList"));	
			}
		}

		public int DisplayHeight_SeriesInfo
		{
			get { return AppSettings.DisplayHeight_SeriesInfo; }
			set
			{
				AppSettings.DisplayHeight_SeriesInfo = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DisplayHeight_SeriesInfo"));
			}
		}

		public int DisplayWidth_EpisodeImage
		{
			get { return AppSettings.DisplayWidth_EpisodeImage; }
			set
			{
				AppSettings.DisplayWidth_EpisodeImage = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DisplayWidth_EpisodeImage"));
			}
		}

		public int DisplayStyle_GroupList
		{
			get { return AppSettings.DisplayStyle_GroupList; }
			set
			{
				AppSettings.DisplayStyle_GroupList = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DisplayStyle_GroupList"));
			}
		}

		public int DisplayHeight_DashImage
		{
			get { return AppSettings.DisplayHeight_DashImage; }
			set
			{
				AppSettings.DisplayHeight_DashImage = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DisplayHeight_DashImage"));
			}
		}

		public int Dash_WatchNext_Items
		{
			get { return AppSettings.Dash_WatchNext_Items; }
			set
			{
				AppSettings.Dash_WatchNext_Items = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_WatchNext_Items"));
			}
		}

		public int Dash_WatchNext_Height
		{
			get { return AppSettings.Dash_WatchNext_Height; }
			set
			{
				AppSettings.Dash_WatchNext_Height = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_WatchNext_Height"));
			}
		}

		public DashWatchNextStyle Dash_WatchNext_Style
		{
			get { return AppSettings.Dash_WatchNext_Style; }
			set
			{
				AppSettings.Dash_WatchNext_Style = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_WatchNext_Style"));
			}
		}

		public int Dash_MissingEps_Items
		{
			get { return AppSettings.Dash_MissingEps_Items; }
			set
			{
				AppSettings.Dash_MissingEps_Items = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_MissingEps_Items"));
			}
		}

		public int Dash_MissingEps_Height
		{
			get { return AppSettings.Dash_MissingEps_Height; }
			set
			{
				AppSettings.Dash_MissingEps_Height = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_MissingEps_Height"));
			}
		}

		public int Dash_MiniCalendarDays
		{
			get { return AppSettings.Dash_MiniCalendarDays; }
			set
			{
				AppSettings.Dash_MiniCalendarDays = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_MiniCalendarDays"));
			}
		}

		public int Dash_MiniCalendar_Height
		{
			get { return AppSettings.Dash_MiniCalendar_Height; }
			set
			{
				AppSettings.Dash_MiniCalendar_Height = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_MiniCalendar_Height"));
			}
		}

		public int Dash_RecWatch_Height
		{
			get { return AppSettings.Dash_RecWatch_Height; }
			set
			{
				AppSettings.Dash_RecWatch_Height = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_RecWatch_Height"));
			}
		}

		public int Dash_RecWatch_Items
		{
			get { return AppSettings.Dash_RecWatch_Items; }
			set
			{
				AppSettings.Dash_RecWatch_Items = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_RecWatch_Items"));
			}
		}

		public int Dash_RecDownload_Height
		{
			get { return AppSettings.Dash_RecDownload_Height; }
			set
			{
				AppSettings.Dash_RecDownload_Height = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_RecDownload_Height"));
			}
		}

		public int Dash_RecDownload_Items
		{
			get { return AppSettings.Dash_RecDownload_Items; }
			set
			{
				AppSettings.Dash_RecDownload_Items = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_RecDownload_Items"));
			}
		}

		public int Dash_TraktFriends_Height
		{
			get { return AppSettings.Dash_TraktFriends_Height; }
			set
			{
				AppSettings.Dash_TraktFriends_Height = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Dash_TraktFriends_Height"));
			}
		}

		public int EpisodeImageOverviewStyle
		{
			get { return AppSettings.EpisodeImageOverviewStyle; }
			set
			{
				AppSettings.EpisodeImageOverviewStyle = value;
				OnPropertyChanged(new PropertyChangedEventArgs("EpisodeImageOverviewStyle"));
			}
		}

		public bool HideEpisodeImageWhenUnwatched
		{
			get { return AppSettings.HideEpisodeImageWhenUnwatched; }
			set
			{
				AppSettings.HideEpisodeImageWhenUnwatched = value;
				OnPropertyChanged(new PropertyChangedEventArgs("HideEpisodeImageWhenUnwatched"));
			}
		}

		public bool HideEpisodeOverviewWhenUnwatched
		{
			get { return AppSettings.HideEpisodeOverviewWhenUnwatched; }
			set
			{
				AppSettings.HideEpisodeOverviewWhenUnwatched = value;
				OnPropertyChanged(new PropertyChangedEventArgs("HideEpisodeOverviewWhenUnwatched"));
			}
		}

		public int GetSeriesWidgetPosition(SeriesWidgets swid)
		{
			// read the series widgets order
			string[] widgets = AppSettings.SeriesWidgetsOrder.Split(';');

			int i = 1;
			foreach (string widget in widgets)
			{
				SeriesWidgets thisswid = (SeriesWidgets)int.Parse(widget);

				if (thisswid == swid)
					return i;
				else
					i++;
			}

			return 1;
		}

		public int MoveUpSeriesWidget(SeriesWidgets swid)
		{
			// read the series widgets order
			string[] widgets = AppSettings.SeriesWidgetsOrder.Split(';');

			string moveWidget = ((int)swid).ToString();

			// find the position of the language to be moved
			int pos = -1;
			for (int i = 0; i < widgets.Length; i++)
			{
				if (widgets[i].Trim().ToUpper() == moveWidget.Trim().ToUpper()) pos = i;
			}

			if (pos == -1) return -1; // not found
			if (pos == 0) return -1; // already at top

			string wid1 = widgets[pos - 1];
			widgets[pos - 1] = moveWidget;
			widgets[pos] = wid1;

			string newWidgetOrder = string.Empty;
			foreach (string wid in widgets)
			{
				if (!string.IsNullOrEmpty(newWidgetOrder))
					newWidgetOrder += ";";

				newWidgetOrder += wid;
			}

			AppSettings.SeriesWidgetsOrder = newWidgetOrder;

			return pos - 1;
		}

		public int MoveDownSeriesWidget(SeriesWidgets swid)
		{
			// read the series widgets order
			string[] widgets = AppSettings.SeriesWidgetsOrder.Split(';');
			string moveWidget = ((int)swid).ToString();

			// find the position of the language to be moved
			int pos = -1;
			for (int i = 0; i < widgets.Length; i++)
			{
				if (widgets[i].Trim().ToUpper() == moveWidget.Trim().ToUpper()) pos = i;
			}

			if (pos == -1) return -1; // not found
			if (pos == widgets.Length - 1) return -1; // already at bottom

			string lan1 = widgets[pos + 1];
			widgets[pos + 1] = moveWidget;
			widgets[pos] = lan1;

			string newWidgetOrder = string.Empty;
			foreach (string wid in widgets)
			{
				if (!string.IsNullOrEmpty(newWidgetOrder))
					newWidgetOrder += ";";

				newWidgetOrder += wid;
			}

			AppSettings.SeriesWidgetsOrder = newWidgetOrder;

			return pos + 1;
		}

		public int GetDashboardWidgetPosition(DashboardWidgets swid)
		{
			// read the series widgets order
			string[] widgets = AppSettings.DashboardWidgetsOrder.Split(';');

			int i = 1;
			foreach (string widget in widgets)
			{
				DashboardWidgets thisswid = (DashboardWidgets)int.Parse(widget);

				if (thisswid == swid)
					return i;
				else
					i++;
			}

			return 1;
		}

		public int MoveUpDashboardWidget(DashboardWidgets swid)
		{
			// read the series widgets order
			string[] widgets = AppSettings.DashboardWidgetsOrder.Split(';');

			string moveWidget = ((int)swid).ToString();

			// find the position of the language to be moved
			int pos = -1;
			for (int i = 0; i < widgets.Length; i++)
			{
				if (widgets[i].Trim().ToUpper() == moveWidget.Trim().ToUpper()) pos = i;
			}

			if (pos == -1) return -1; // not found
			if (pos == 0) return -1; // already at top

			string wid1 = widgets[pos - 1];
			widgets[pos - 1] = moveWidget;
			widgets[pos] = wid1;

			string newWidgetOrder = string.Empty;
			foreach (string wid in widgets)
			{
				if (!string.IsNullOrEmpty(newWidgetOrder))
					newWidgetOrder += ";";

				newWidgetOrder += wid;
			}

			AppSettings.DashboardWidgetsOrder = newWidgetOrder;

			return pos - 1;
		}

		public int MoveDownDashboardWidget(DashboardWidgets swid)
		{
			// read the series widgets order
			string[] widgets = AppSettings.DashboardWidgetsOrder.Split(';');
			string moveWidget = ((int)swid).ToString();

			// find the position of the language to be moved
			int pos = -1;
			for (int i = 0; i < widgets.Length; i++)
			{
				if (widgets[i].Trim().ToUpper() == moveWidget.Trim().ToUpper()) pos = i;
			}

			if (pos == -1) return -1; // not found
			if (pos == widgets.Length - 1) return -1; // already at bottom

			string lan1 = widgets[pos + 1];
			widgets[pos + 1] = moveWidget;
			widgets[pos] = lan1;

			string newWidgetOrder = string.Empty;
			foreach (string wid in widgets)
			{
				if (!string.IsNullOrEmpty(newWidgetOrder))
					newWidgetOrder += ";";

				newWidgetOrder += wid;
			}

			AppSettings.DashboardWidgetsOrder = newWidgetOrder;

			return pos + 1;
		}
	}
}
