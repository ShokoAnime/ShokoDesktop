using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Shoko.Commons.Notification;
using Shoko.Desktop.Downloads;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Metro;
using Shoko.Models.Enums;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_UserSettings : INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private static VM_UserSettings _instance;
        public static VM_UserSettings Instance => _instance ?? (_instance = new VM_UserSettings());

        public ObservableCollection<TorrentSourceVM> UnselectedTorrentSources { get; set; }
        public ObservableCollection<TorrentSourceVM> SelectedTorrentSources { get; set; }
        public ObservableCollection<TorrentSourceVM> AllTorrentSources { get; set; }
        public ObservableCollection<TorrentSourceVM> CurrentSearchTorrentSources { get; set; }

        public VM_UserSettings()
        {
            UnselectedTorrentSources = new ObservableCollection<TorrentSourceVM>();
            SelectedTorrentSources = new ObservableCollection<TorrentSourceVM>();
            AllTorrentSources = new ObservableCollection<TorrentSourceVM>(GetAllTorrentSources());
            CurrentSearchTorrentSources = new ObservableCollection<TorrentSourceVM>();
        }

        public List<TorrentSourceVM> GetAllTorrentSources()
        {
            List<TorrentSourceVM> sources = new List<TorrentSourceVM> {new TorrentSourceVM(TorrentSourceType.TokyoToshokanAnime, true), new TorrentSourceVM(TorrentSourceType.TokyoToshokanAll, true), new TorrentSourceVM(TorrentSourceType.Nyaa, true), new TorrentSourceVM(TorrentSourceType.Sukebei, true), new TorrentSourceVM(TorrentSourceType.BakaBT, true), new TorrentSourceVM(TorrentSourceType.AnimeBytes, true)};


            return sources;
        }

        public void RefreshTorrentSources()
        {
            UnselectedTorrentSources.Clear();
            SelectedTorrentSources.Clear();

            try
            {

                string[] sources = AppSettings.TorrentSources.Split(';');

                foreach (string src in sources)
                {
                    if (string.IsNullOrEmpty(src)) continue;
                    int iSrc;
                    int.TryParse(src, out iSrc);


                    TorrentSourceVM selSource = new TorrentSourceVM((TorrentSourceType)iSrc, true);
                    if ((TorrentSourceType)iSrc != TorrentSourceType.AnimeSuki)
                        SelectedTorrentSources.Add(selSource);
                }

                foreach (TorrentSourceVM src in GetAllTorrentSources())
                {
                    bool inSelected = false;
                    foreach (TorrentSourceVM selSource in SelectedTorrentSources)
                    {
                        if (src.TorrentSource == selSource.TorrentSource)
                        {
                            inSelected = true;
                            break;
                        }
                    }
                    if (!inSelected)
                        UnselectedTorrentSources.Add(src);
                }


                CurrentSearchTorrentSources.Clear();
                foreach (TorrentSourceVM src in GetAllTorrentSources())
                {
                    bool inSelected = false;
                    foreach (TorrentSourceVM selSource in SelectedTorrentSources)
                    {
                        if (src.TorrentSource == selSource.TorrentSource)
                        {
                            inSelected = true;
                            break;
                        }
                    }

                    TorrentSourceVM newSource = new TorrentSourceVM(src.TorrentSource, inSelected);
                    CurrentSearchTorrentSources.Add(newSource);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void RemoveTorrentSource(TorrentSourceType tsType)
        {
            string[] sources = AppSettings.TorrentSources.Split(';');

            string newSetting = string.Empty;


            string sType = ((int)tsType).ToString();

            foreach (string src in sources)
            {
                if (string.IsNullOrEmpty(src)) continue;
                if (src.Trim() == sType) continue;

                if (!string.IsNullOrEmpty(newSetting))
                    newSetting += ";";

                newSetting += src;
            }

            AppSettings.TorrentSources = newSetting;
            RefreshTorrentSources();
        }

        public int MoveUpTorrentSource(TorrentSourceType tsType)
        {
            string[] sources = AppSettings.TorrentSources.Split(';');
            string sType = ((int)tsType).ToString();

            List<string> sourcesList = new List<string>();

            // get a list of valid sources
            foreach (string src in sources)
            {
                if (string.IsNullOrEmpty(src)) continue;
                sourcesList.Add(src);
            }

            // find the position of the source to be moved
            int pos = -1;
            for (int i = 0; i < sourcesList.Count; i++)
            {
                if (sourcesList[i].Trim() == sType) pos = i;
            }

            if (pos == -1) return -1; // not found
            if (pos == 0) return -1; // already at top

            string lan1 = sourcesList[pos - 1];
            sourcesList[pos - 1] = sType;
            sourcesList[pos] = lan1;

            string newSetting = string.Empty;

            foreach (string src in sourcesList)
            {
                if (!string.IsNullOrEmpty(newSetting))
                    newSetting += ";";

                newSetting += src;
            }

            AppSettings.TorrentSources = newSetting;
            RefreshTorrentSources();

            return pos - 1;
        }

        public int MoveDownTorrentSource(TorrentSourceType tsType)
        {
            string[] sources = AppSettings.TorrentSources.Split(';');
            string sType = ((int)tsType).ToString();

            List<string> sourcesList = new List<string>();

            // get a list of valid sources
            foreach (string src in sources)
            {
                if (string.IsNullOrEmpty(src)) continue;
                sourcesList.Add(src);
            }

            // find the position of the source to be moved
            int pos = -1;
            for (int i = 0; i < sourcesList.Count; i++)
            {
                if (sourcesList[i].Trim() == sType) pos = i;
            }

            if (pos == -1) return -1; // not found
            if (pos == sourcesList.Count - 1) return -1; // already at bottom

            string lan1 = sourcesList[pos + 1];
            sourcesList[pos + 1] = sType;
            sourcesList[pos] = lan1;

            string newSetting = string.Empty;

            foreach (string lan in sourcesList)
            {
                if (!string.IsNullOrEmpty(newSetting))
                    newSetting += ";";

                newSetting += lan;
            }

            AppSettings.TorrentSources = newSetting;
            RefreshTorrentSources();

            return pos + 1;
        }

        public void AddTorrentSource(TorrentSourceType tsType)
        {
            string sType = ((int)tsType).ToString();

            string newSetting = AppSettings.TorrentSources;

            if (!string.IsNullOrEmpty(newSetting))
                newSetting += ";";

            newSetting += sType;
            AppSettings.TorrentSources = newSetting;

            RefreshTorrentSources();
        }

        public bool TagsExpanded
        {
            get { return AppSettings.TagsExpanded; }
            set
            {
                AppSettings.TagsExpanded = this.SetField(AppSettings.TagsExpanded, value,()=> TagsExpanded, ()=>TagsCollapsed);
            }
        }

        public bool TagsCollapsed => !AppSettings.TagsExpanded;

        public bool CustomTagsExpanded
        {
            get { return AppSettings.CustomTagsExpanded; }
            set
            {
                AppSettings.CustomTagsExpanded = this.SetField(AppSettings.CustomTagsExpanded, value, ()=>CustomTagsCollapsed, ()=>CustomTagsExpanded);
            }
        }

        public bool CustomTagsCollapsed => !AppSettings.CustomTagsExpanded;

        public bool TitlesExpanded
        {
            get { return AppSettings.TitlesExpanded; }
            set
            {
                AppSettings.TitlesExpanded = this.SetField(AppSettings.TitlesExpanded, value,()=>TitlesExpanded,()=>TitlesCollapsed);
            }
        }

        public bool TitlesCollapsed => !AppSettings.TitlesExpanded;

        public bool SeriesTvDBLinksExpanded
        {
            get { return AppSettings.SeriesTvDBLinksExpanded; }
            set
            {
                AppSettings.SeriesTvDBLinksExpanded = this.SetField(AppSettings.SeriesTvDBLinksExpanded, value,()=>SeriesTvDBLinksExpanded, ()=>SeriesTvDBLinksCollapsed);
            }
        }

        public bool SeriesTvDBLinksCollapsed => !AppSettings.SeriesTvDBLinksExpanded;

        public bool SeriesNextEpisodeExpanded
        {
            get { return AppSettings.SeriesNextEpisodeExpanded; }
            set
            {
                AppSettings.SeriesNextEpisodeExpanded  = this.SetField(AppSettings.SeriesNextEpisodeExpanded , value,()=>SeriesNextEpisodeExpanded, ()=>SeriesNextEpisodeCollapsed);
            }
        }

        public bool SeriesNextEpisodeCollapsed => !AppSettings.SeriesNextEpisodeExpanded;

        public bool SeriesGroupExpanded
        {
            get { return AppSettings.SeriesGroupExpanded; }
            set
            {
                AppSettings.SeriesGroupExpanded  = this.SetField(AppSettings.SeriesGroupExpanded , value, ()=>SeriesGroupExpanded, ()=>SeriesGroupCollapsed);
            }
        }

        public bool SeriesGroupCollapsed => !AppSettings.SeriesGroupExpanded;

        public bool DashWatchNextEpExpanded
        {
            get { return AppSettings.DashWatchNextEpExpanded; }
            set
            {
                AppSettings.DashWatchNextEpExpanded  = this.SetField(AppSettings.DashWatchNextEpExpanded , value, ()=>DashWatchNextEpExpanded, ()=>DashWatchNextEpCollapsed);
            }
        }

        public bool DashWatchNextEpCollapsed => !AppSettings.DashWatchNextEpExpanded;


        public bool DashRecentlyWatchEpsExpanded
        {
            get { return AppSettings.DashRecentlyWatchEpsExpanded; }
            set
            {
                AppSettings.DashRecentlyWatchEpsExpanded  = this.SetField(AppSettings.DashRecentlyWatchEpsExpanded , value, ()=>DashRecentAdditionsExpanded, ()=>DashRecentAdditionsCollapsed);
            }
        }

        public bool DashRecentlyWatchEpsCollapsed => !AppSettings.DashRecentlyWatchEpsExpanded;


        public bool DashSeriesMissingEpisodesExpanded
        {
            get { return AppSettings.DashSeriesMissingEpisodesExpanded; }
            set
            {
                AppSettings.DashSeriesMissingEpisodesExpanded  = this.SetField(AppSettings.DashSeriesMissingEpisodesExpanded , value, ()=>DashSeriesMissingEpisodesExpanded, ()=>DashSeriesMissingEpisodesCollapsed);
            }
        }

        public bool DashSeriesMissingEpisodesCollapsed => !AppSettings.DashSeriesMissingEpisodesExpanded;

        public bool DashMiniCalendarExpanded
        {
            get { return AppSettings.DashMiniCalendarExpanded; }
            set
            {
                AppSettings.DashMiniCalendarExpanded  = this.SetField(AppSettings.DashMiniCalendarExpanded , value,()=>DashMiniCalendarExpanded, ()=>DashMiniCalendarCollapsed);
            }
        }

        public bool DashRecommendationsWatchCollapsed => !AppSettings.DashRecommendationsWatchExpanded;

        public bool DashRecommendationsWatchExpanded
        {
            get { return AppSettings.DashRecommendationsWatchExpanded; }
            set
            {
                AppSettings.DashRecommendationsWatchExpanded  = this.SetField(AppSettings.DashRecommendationsWatchExpanded , value, ()=>DashRecommendationsWatchExpanded, ()=>DashRecommendationsWatchCollapsed);
            }
        }

        public bool DashRecommendationsDownloadCollapsed => !AppSettings.DashRecommendationsDownloadExpanded;

        public bool DashRecommendationsDownloadExpanded
        {
            get { return AppSettings.DashRecommendationsDownloadExpanded; }
            set
            {
                AppSettings.DashRecommendationsDownloadExpanded  = this.SetField(AppSettings.DashRecommendationsDownloadExpanded , value, ()=>DashRecommendationsDownloadExpanded, ()=>DashRecommendationsDownloadCollapsed);
            }
        }

        public bool DashRecentAdditionsCollapsed => !AppSettings.DashRecentAdditionsExpanded;

        public bool DashRecentAdditionsExpanded
        {
            get { return AppSettings.DashRecentAdditionsExpanded; }
            set
            {
                AppSettings.DashRecentAdditionsExpanded  = this.SetField(AppSettings.DashRecentAdditionsExpanded , value, ()=>DashRecentAdditionsExpanded, ()=>DashRecentAdditionsCollapsed);
            }
        }

        public int DashRecentAdditionsType
        {
            get { return AppSettings.DashRecentAdditionsType; }
            set
            {
                AppSettings.DashRecentAdditionsType  = this.SetField(AppSettings.DashRecentAdditionsType , value);
            }
        }



        public bool DashMiniCalendarCollapsed => !AppSettings.DashMiniCalendarExpanded;

        public int DisplayHeight_GroupList
        {
            get { return AppSettings.DisplayHeight_GroupList; }
            set
            {
                AppSettings.DisplayHeight_GroupList  = this.SetField(AppSettings.DisplayHeight_GroupList , value);
            }
        }



        public int DisplayHeight_SeriesInfo
        {
            get { return AppSettings.DisplayHeight_SeriesInfo; }
            set
            {
                AppSettings.DisplayHeight_SeriesInfo  = this.SetField(AppSettings.DisplayHeight_SeriesInfo , value);
            }
        }

        public int DisplayWidth_EpisodeImage
        {
            get { return AppSettings.DisplayWidth_EpisodeImage; }
            set
            {
                AppSettings.DisplayWidth_EpisodeImage  = this.SetField(AppSettings.DisplayWidth_EpisodeImage , value);
            }
        }

        public int DisplayStyle_GroupList
        {
            get { return AppSettings.DisplayStyle_GroupList; }
            set
            {
                AppSettings.DisplayStyle_GroupList  = this.SetField(AppSettings.DisplayStyle_GroupList , value);
            }
        }

        public int DisplayHeight_DashImage
        {
            get { return AppSettings.DisplayHeight_DashImage; }
            set
            {
                AppSettings.DisplayHeight_DashImage  = this.SetField(AppSettings.DisplayHeight_DashImage , value);
            }
        }

        public int Dash_WatchNext_Items
        {
            get { return AppSettings.Dash_WatchNext_Items; }
            set
            {
                AppSettings.Dash_WatchNext_Items  = this.SetField(AppSettings.Dash_WatchNext_Items , value);
            }
        }

        public int Dash_RecentAdditions_Items
        {
            get { return AppSettings.Dash_RecentAdditions_Items; }
            set
            {
                AppSettings.Dash_RecentAdditions_Items  = this.SetField(AppSettings.Dash_RecentAdditions_Items , value);
            }
        }

        public int Dash_WatchNext_Height
        {
            get { return AppSettings.Dash_WatchNext_Height; }
            set
            {
                AppSettings.Dash_WatchNext_Height  = this.SetField(AppSettings.Dash_WatchNext_Height , value);
            }
        }




        public int DashMetro_WatchNext_Items
        {
            get { return AppSettings.DashMetro_WatchNext_Items; }
            set
            {
                AppSettings.DashMetro_WatchNext_Items  = this.SetField(AppSettings.DashMetro_WatchNext_Items , value);
            }
        }

        public int DashMetro_RandomSeries_Items
        {
            get { return AppSettings.DashMetro_RandomSeries_Items; }
            set
            {
                AppSettings.DashMetro_RandomSeries_Items  = this.SetField(AppSettings.DashMetro_RandomSeries_Items , value);
            }
        }


        public int DashMetro_NewEpisodes_Items
        {
            get { return AppSettings.DashMetro_NewEpisodes_Items; }
            set
            {
                AppSettings.DashMetro_NewEpisodes_Items  = this.SetField(AppSettings.DashMetro_NewEpisodes_Items , value);
            }
        }

        public int DashMetro_Image_Height
        {
            get { return AppSettings.DashMetro_Image_Height; }
            set
            {
                AppSettings.DashMetro_Image_Height  = this.SetField(AppSettings.DashMetro_Image_Height , value);
                SetDashMetro_Image_Width();
            }
        }

        public bool UseStreaming
        {
            get { return AppSettings.UseStreaming;  }
            set
            {
                AppSettings.UseStreaming  = this.SetField(AppSettings.UseStreaming , value);

            }
        }
        public void SetDashMetro_Image_Width()
        {
            if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
            {
                DashMetro_Image_Width = (int)(DashMetro_Image_Height * 1.777777777777778);
            }
            else
            {
                DashMetro_Image_Width = (int)(DashMetro_Image_Height * 0.68);
            }
        }

        public int DashMetro_Image_Width { get; set; } = 200;


        public int Dash_RecentAdditions_Height
        {
            get { return AppSettings.Dash_RecentAdditions_Height; }
            set
            {
                AppSettings.Dash_RecentAdditions_Height  = this.SetField(AppSettings.Dash_RecentAdditions_Height , value);
            }
        }


        public int SeriesGroup_Image_Height
        {
            get { return AppSettings.SeriesGroup_Image_Height; }
            set
            {
                AppSettings.SeriesGroup_Image_Height  = this.SetField(AppSettings.SeriesGroup_Image_Height , value);
                int width = (int)(SeriesGroup_Image_Height * 1.77777777);
                SeriesGroup_Image_Width = width;
            }
        }

        public int SeriesGroup_Image_Width
        {
            get
            {
                int width = (int)(SeriesGroup_Image_Height * 1.77777777);
                return width;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                this.OnPropertyChanged(()=>SeriesGroup_Image_Width);
            }
        }

        public int PlaylistHeader_Image_Height
        {
            get { return AppSettings.PlaylistHeader_Image_Height; }
            set
            {
                AppSettings.PlaylistHeader_Image_Height  = this.SetField(AppSettings.PlaylistHeader_Image_Height , value);
                int width = (int)(PlaylistHeader_Image_Height * 1.77777777);
                PlaylistHeader_Image_Width = width;
            }
        }

        public int PlaylistHeader_Image_Width
        {
            get
            {
                int width = (int)(PlaylistHeader_Image_Height * 1.77777777);
                return width;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                this.OnPropertyChanged(()=> PlaylistHeader_Image_Width);
            }
        }

        public int PlaylistItems_Image_Height
        {
            get { return AppSettings.PlaylistItems_Image_Height; }
            set
            {
                AppSettings.PlaylistItems_Image_Height  = this.SetField(AppSettings.PlaylistItems_Image_Height , value);
            }
        }

        public int PlaylistEpisode_Image_Width
        {
            get { return AppSettings.PlaylistEpisode_Image_Width; }
            set
            {
                AppSettings.PlaylistEpisode_Image_Width  = this.SetField(AppSettings.PlaylistEpisode_Image_Width , value);
            }
        }

        public bool PlaylistItems_ShowDetails
        {
            get { return AppSettings.PlaylistItems_ShowDetails; }
            set
            {
                AppSettings.PlaylistItems_ShowDetails  = this.SetField(AppSettings.PlaylistItems_ShowDetails , value);
            }
        }


        public int Dash_RecentlyWatchedEp_Items
        {
            get { return AppSettings.Dash_RecentlyWatchedEp_Items; }
            set
            {
                AppSettings.Dash_RecentlyWatchedEp_Items  = this.SetField(AppSettings.Dash_RecentlyWatchedEp_Items , value);
            }
        }

        public int Dash_RecentlyWatchedEp_Height
        {
            get { return AppSettings.Dash_RecentlyWatchedEp_Height; }
            set
            {
                AppSettings.Dash_RecentlyWatchedEp_Height  = this.SetField(AppSettings.Dash_RecentlyWatchedEp_Height , value);
            }
        }







        public DashWatchNextStyle Dash_WatchNext_Style
        {
            get { return AppSettings.Dash_WatchNext_Style; }
            set
            {
                AppSettings.Dash_WatchNext_Style  = this.SetField(AppSettings.Dash_WatchNext_Style , value);
            }
        }

        public int Dash_MissingEps_Items
        {
            get { return AppSettings.Dash_MissingEps_Items; }
            set
            {
                AppSettings.Dash_MissingEps_Items  = this.SetField(AppSettings.Dash_MissingEps_Items , value);
            }
        }

        public int Dash_MissingEps_Height
        {
            get { return AppSettings.Dash_MissingEps_Height; }
            set
            {
                AppSettings.Dash_MissingEps_Height  = this.SetField(AppSettings.Dash_MissingEps_Height , value);
            }
        }

        public int Dash_MiniCalendarDays
        {
            get { return AppSettings.Dash_MiniCalendarDays; }
            set
            {
                AppSettings.Dash_MiniCalendarDays  = this.SetField(AppSettings.Dash_MiniCalendarDays , value);
            }
        }

        public bool Dash_MiniCalendarUpcomingOnly
        {
            get { return AppSettings.Dash_MiniCalendarUpcomingOnly; }
            set
            {
                AppSettings.Dash_MiniCalendarUpcomingOnly  = this.SetField(AppSettings.Dash_MiniCalendarUpcomingOnly , value);
            }
        }

        public int Dash_MiniCalendar_Height
        {
            get { return AppSettings.Dash_MiniCalendar_Height; }
            set
            {
                AppSettings.Dash_MiniCalendar_Height  = this.SetField(AppSettings.Dash_MiniCalendar_Height , value);
            }
        }

        public int Dash_RecWatch_Height
        {
            get { return AppSettings.Dash_RecWatch_Height; }
            set
            {
                AppSettings.Dash_RecWatch_Height  = this.SetField(AppSettings.Dash_RecWatch_Height , value);
            }
        }

        public int Dash_RecWatch_Items
        {
            get { return AppSettings.Dash_RecWatch_Items; }
            set
            {
                AppSettings.Dash_RecWatch_Items  = this.SetField(AppSettings.Dash_RecWatch_Items , value);
            }
        }

        public int Dash_RecDownload_Height
        {
            get { return AppSettings.Dash_RecDownload_Height; }
            set
            {
                AppSettings.Dash_RecDownload_Height  = this.SetField(AppSettings.Dash_RecDownload_Height , value);
            }
        }

        public int Dash_RecDownload_Items
        {
            get { return AppSettings.Dash_RecDownload_Items; }
            set
            {
                AppSettings.Dash_RecDownload_Items  = this.SetField(AppSettings.Dash_RecDownload_Items , value);
            }
        }

        public int EpisodeImageOverviewStyle
        {
            get { return AppSettings.EpisodeImageOverviewStyle; }
            set
            {
                AppSettings.EpisodeImageOverviewStyle  = this.SetField(AppSettings.EpisodeImageOverviewStyle , value);
            }
        }

        public bool HideEpisodeImageWhenUnwatched
        {
            get { return AppSettings.HideEpisodeImageWhenUnwatched; }
            set
            {
                AppSettings.HideEpisodeImageWhenUnwatched  = this.SetField(AppSettings.HideEpisodeImageWhenUnwatched , value);
            }
        }

        public bool HideEpisodeOverviewWhenUnwatched
        {
            get { return AppSettings.HideEpisodeOverviewWhenUnwatched; }
            set
            {
                AppSettings.HideEpisodeOverviewWhenUnwatched  = this.SetField(AppSettings.HideEpisodeOverviewWhenUnwatched , value);
            }
        }

        public bool ShowDownloadButtonWhenFilesExist => !AppSettings.HideDownloadButtonWhenFilesExist;

        public bool HideDownloadButtonWhenFilesExist
        {
            get { return AppSettings.HideDownloadButtonWhenFilesExist; }
            set
            {
                AppSettings.HideDownloadButtonWhenFilesExist  = this.SetField(AppSettings.HideDownloadButtonWhenFilesExist , value,()=>HideDownloadButtonWhenFilesExist, ()=>ShowDownloadButtonWhenFilesExist);
            }
        }

        public bool DisplayRatingDialogOnCompletion
        {
            get { return AppSettings.DisplayRatingDialogOnCompletion; }
            set
            {
                AppSettings.DisplayRatingDialogOnCompletion  = this.SetField(AppSettings.DisplayRatingDialogOnCompletion , value);
            }
        }

        public bool UseFanartOnSeries
        {
            get { return AppSettings.UseFanartOnSeries; }
            set
            {
                AppSettings.UseFanartOnSeries  = this.SetField(AppSettings.UseFanartOnSeries , value);
            }
        }

        public bool AlwaysUseAniDBPoster
        {
            get { return AppSettings.AlwaysUseAniDBPoster; }
            set
            {
                AppSettings.AlwaysUseAniDBPoster  = this.SetField(AppSettings.AlwaysUseAniDBPoster , value);
            }
        }

        public bool UseFanartOnPlaylistHeader
        {
            get { return AppSettings.UseFanartOnPlaylistHeader; }
            set
            {
                AppSettings.UseFanartOnPlaylistHeader  = this.SetField(AppSettings.UseFanartOnPlaylistHeader , value);
            }
        }

        public bool UseFanartOnPlaylistItems
        {
            get { return AppSettings.UseFanartOnPlaylistItems; }
            set
            {
                AppSettings.UseFanartOnPlaylistItems  = this.SetField(AppSettings.UseFanartOnPlaylistItems , value);
            }
        }

        public bool TorrentBlackhole
        {
            get { return AppSettings.TorrentBlackhole; }
            set
            {
                AppSettings.TorrentBlackhole  = this.SetField(AppSettings.TorrentBlackhole , value);
            }
        }

        public string TorrentBlackholeFolder
        {
            get { return AppSettings.TorrentBlackholeFolder; }
            set
            {
                AppSettings.TorrentBlackholeFolder  = this.SetField(AppSettings.TorrentBlackholeFolder , value);
            }
        }


        public string UTorrentAddress
        {
            get { return AppSettings.UTorrentAddress; }
            set
            {
                AppSettings.UTorrentAddress  = this.SetField(AppSettings.UTorrentAddress , value);
            }
        }

        public string UTorrentPort
        {
            get { return AppSettings.UTorrentPort; }
            set
            {
                AppSettings.UTorrentPort  = this.SetField(AppSettings.UTorrentPort , value);
            }
        }

        public string UTorrentUsername
        {
            get { return AppSettings.UTorrentUsername; }
            set
            {
                AppSettings.UTorrentUsername  = this.SetField(AppSettings.UTorrentUsername , value);
            }
        }

        public string UTorrentPassword
        {
            get { return AppSettings.UTorrentPassword; }
            set
            {
                AppSettings.UTorrentPassword  = this.SetField(AppSettings.UTorrentPassword , value);
            }
        }

        public int UTorrentRefreshInterval
        {
            get { return AppSettings.UTorrentRefreshInterval; }
            set
            {
                AppSettings.UTorrentRefreshInterval  = this.SetField(AppSettings.UTorrentRefreshInterval , value);
            }
        }

        public bool UTorrentAutoRefresh
        {
            get { return AppSettings.UTorrentAutoRefresh; }
            set
            {
                AppSettings.UTorrentAutoRefresh  = this.SetField(AppSettings.UTorrentAutoRefresh , value);
            }
        }

        public bool TorrentSearchPreferOwnGroups
        {
            get { return AppSettings.TorrentSearchPreferOwnGroups; }
            set
            {
                AppSettings.TorrentSearchPreferOwnGroups  = this.SetField(AppSettings.TorrentSearchPreferOwnGroups , value);
            }
        }

        public string BakaBTUsername
        {
            get { return AppSettings.BakaBTUsername; }
            set
            {
                AppSettings.BakaBTUsername  = this.SetField(AppSettings.BakaBTUsername , value);
            }
        }

        public string BakaBTPassword
        {
            get { return AppSettings.BakaBTPassword; }
            set
            {
                AppSettings.BakaBTPassword  = this.SetField(AppSettings.BakaBTPassword , value);
            }
        }

        public bool BakaBTOnlyUseForSeriesSearches
        {
            get { return AppSettings.BakaBTOnlyUseForSeriesSearches; }
            set
            {
                AppSettings.BakaBTOnlyUseForSeriesSearches  = this.SetField(AppSettings.BakaBTOnlyUseForSeriesSearches , value);
            }
        }

        private string bakaBTCookieHeader = "";
        public string BakaBTCookieHeader
        {
            get { return bakaBTCookieHeader; }
            set
            {
                bakaBTCookieHeader = this.SetField(bakaBTCookieHeader, value);
                BakaBTCookieHeaderSet = !string.IsNullOrEmpty(value);
            }
        }

        private bool bakaBTCookieHeaderSet;
        public bool BakaBTCookieHeaderSet
        {
            get { return bakaBTCookieHeaderSet; }
            set
            {
                bakaBTCookieHeaderSet = this.SetField(bakaBTCookieHeaderSet, value);
            }
        }






        public string AnimeBytesUsername
        {
            get { return AppSettings.AnimeBytesUsername; }
            set
            {
                AppSettings.AnimeBytesUsername  = this.SetField(AppSettings.AnimeBytesUsername , value);
            }
        }

        public string AnimeBytesPassword
        {
            get { return AppSettings.AnimeBytesPassword; }
            set
            {
                AppSettings.AnimeBytesPassword  = this.SetField(AppSettings.AnimeBytesPassword , value);
            }
        }

        public bool AnimeBytesOnlyUseForSeriesSearches
        {
            get { return AppSettings.AnimeBytesOnlyUseForSeriesSearches; }
            set
            {
                AppSettings.AnimeBytesOnlyUseForSeriesSearches  = this.SetField(AppSettings.AnimeBytesOnlyUseForSeriesSearches , value);
            }
        }




        private string animeBytesCookieHeader = "";
        public string AnimeBytesCookieHeader
        {
            get { return animeBytesCookieHeader; }
            set
            {
                animeBytesCookieHeader = this.SetField(animeBytesCookieHeader, value);
                AnimeBytesCookieHeaderSet = !string.IsNullOrEmpty(value);
            }
        }

        private bool animeBytesCookieHeaderSet;
        public bool AnimeBytesCookieHeaderSet
        {
            get { return animeBytesCookieHeaderSet; }
            set
            {
                animeBytesCookieHeaderSet = this.SetField(animeBytesCookieHeaderSet, value);
            }
        }

        public bool MPCIniIntegration
        {
            get
            {
                return AppSettings.MPCIniIntegration;
            }
            set
            {
                AppSettings.MPCIniIntegration  = this.SetField(AppSettings.MPCIniIntegration , value);
            }
        }

        public bool MPCWebUiIntegration
        {
            get
            {
                return AppSettings.MPCWebUiIntegration;
            }
            set
            {
                AppSettings.MPCWebUiIntegration  = this.SetField(AppSettings.MPCWebUiIntegration , value);
            }
        }
        public string MPCFolder
		{
			get { return AppSettings.MPCFolder; }
			set
			{
				AppSettings.MPCFolder  = this.SetField(AppSettings.MPCFolder , value);
			}
		}

        public string MPCWebUIPort {
            get { return AppSettings.MPCWebUIPort; }
            set
            {
                AppSettings.MPCWebUIPort  = this.SetField(AppSettings.MPCWebUIPort , value);
            }
        }
        public string PotPlayerFolder
        {
            get { return AppSettings.PotPlayerFolder; }
            set
            {
                AppSettings.PotPlayerFolder  = this.SetField(AppSettings.PotPlayerFolder , value);
            }
        }
        public int VideoWatchedPct
        {
            get { return AppSettings.VideoWatchedPct; }
            set
            {
                AppSettings.VideoWatchedPct  = this.SetField(AppSettings.VideoWatchedPct , value);
            }
        }

        public bool VideoAutoSetWatched
        {
            get { return AppSettings.VideoAutoSetWatched; }
            set
            {
                AppSettings.VideoAutoSetWatched  = this.SetField(AppSettings.VideoAutoSetWatched , value);
            }
        }

        public int DownloadsRecItems
        {
            get { return AppSettings.DownloadsRecItems; }
            set
            {
                AppSettings.DownloadsRecItems  = this.SetField(AppSettings.DownloadsRecItems , value);
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




        public bool WindowFullScreen
        {
            get { return AppSettings.WindowFullScreen; }
            set
            {
                AppSettings.WindowFullScreen  = this.SetField(AppSettings.WindowFullScreen , value);
            }
        }

        public bool WindowNormal
        {
            get { return AppSettings.WindowNormal; }
            set
            {
                AppSettings.WindowNormal  = this.SetField(AppSettings.WindowNormal , value);
            }
        }

        public int DefaultPlayer_GroupList
        {
            get { return AppSettings.DefaultPlayer_GroupList; }
            set
            {
                AppSettings.DefaultPlayer_GroupList  = this.SetField(AppSettings.DefaultPlayer_GroupList , value);
            }
        }

        public void GetDashboardMetroSectionPosition(DashboardMetroProcessType swid, ref int pos, ref Visibility vis)
        {
            // read the series sections order
            string[] sections = AppSettings.DashboardMetroSectionOrder.Split(';');

            int i = 1;
            foreach (string section in sections)
            {
                string[] vals = section.Split(':');
                DashboardMetroProcessType thisswid = (DashboardMetroProcessType)int.Parse(vals[0]);

                if (thisswid == swid)
                {
                    bool v = bool.Parse(vals[1]);
                    pos = i;
                    vis = v ? Visibility.Visible : Visibility.Collapsed;
                    return;
                }
                else
                    i++;
            }
        }

        public List<MetroDashSection> GetMetroDashSections()
        {
            List<MetroDashSection> sectionsRet = new List<MetroDashSection>();

            string[] sections = AppSettings.DashboardMetroSectionOrder.Split(';');

            foreach (string section in sections)
            {
                string[] vals = section.Split(':');
                bool enabled = bool.Parse(vals[1]);

                // skip Trakt as this has been deprecated
                DashboardMetroProcessType sectionType = (DashboardMetroProcessType)int.Parse(vals[0]);
                if (sectionType == DashboardMetroProcessType.TraktActivity) continue;

                MetroDashSection dashSect = new MetroDashSection()
                {
                    SectionType = sectionType,
                    Enabled = enabled,
                    WinVisibility = enabled ? Visibility.Visible : Visibility.Collapsed
                };

                sectionsRet.Add(dashSect);
            }

            return sectionsRet;
        }

        public int MoveUpDashboardMetroSection(DashboardMetroProcessType swid)
        {
            // read the series sections order
            string[] sections = AppSettings.DashboardMetroSectionOrder.Split(';');

            string moveSectionType = ((int)swid).ToString();
            string moveSection = "";

            // find the position of the language to be moved
            int pos = -1;
            for (int i = 0; i < sections.Length; i++)
            {
                string[] vals = sections[i].Split(':');
                if (vals[0].Trim().ToUpper() == moveSectionType.Trim().ToUpper())
                {
                    pos = i;
                    moveSection = sections[i];
                }
            }

            if (pos == -1) return -1; // not found
            if (pos == 0) return -1; // already at top

            string wid1 = sections[pos - 1];
            sections[pos - 1] = moveSection;
            sections[pos] = wid1;

            string newSectionOrder = string.Empty;
            foreach (string wid in sections)
            {
                if (!string.IsNullOrEmpty(newSectionOrder))
                    newSectionOrder += ";";

                newSectionOrder += wid;
            }

            AppSettings.DashboardMetroSectionOrder = newSectionOrder;

            return pos - 1;
        }

        public int MoveDownDashboardMetroSection(DashboardMetroProcessType swid)
        {
            // read the series sections order
            string[] sections = AppSettings.DashboardMetroSectionOrder.Split(';');
            string moveSectionType = ((int)swid).ToString();
            string moveSection = "";

            // find the position of the language to be moved
            int pos = -1;
            for (int i = 0; i < sections.Length; i++)
            {
                string[] vals = sections[i].Split(':');
                if (vals[0].Trim().ToUpper() == moveSectionType.Trim().ToUpper())
                {
                    pos = i;
                    moveSection = sections[i];
                }
            }

            if (pos == -1) return -1; // not found
            if (pos == sections.Length - 1) return -1; // already at bottom

            string lan1 = sections[pos + 1];
            sections[pos + 1] = moveSection;
            sections[pos] = lan1;

            string newSectionOrder = string.Empty;
            foreach (string wid in sections)
            {
                if (!string.IsNullOrEmpty(newSectionOrder))
                    newSectionOrder += ";";

                newSectionOrder += wid;
            }

            AppSettings.DashboardMetroSectionOrder = newSectionOrder;

            return pos + 1;
        }

        public Visibility IsMPCInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.MPC) ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsMPCNotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.MPC) ? Visibility.Hidden : Visibility.Visible;
        public Visibility IsMPVInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.ExternalMPV) ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsMPVNotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.ExternalMPV) ? Visibility.Hidden : Visibility.Visible;
        public Visibility IsVLCInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.VLC) ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsVLCNotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.VLC) ? Visibility.Hidden : Visibility.Visible;
        public Visibility IsPotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.PotPlayer) ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsPotNotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.PotPlayer) ? Visibility.Hidden : Visibility.Visible;
        public Visibility IsZoomPlayerInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.ZoomPlayer) ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsZoomPlayerNotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.ZoomPlayer) ? Visibility.Hidden : Visibility.Visible;
        public Visibility IsWindowsDefaultInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.WindowsDefault) ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsWindowsDefaultNotInstalled => MainWindow.videoHandler.IsActive(VideoPlayer.WindowsDefault) ? Visibility.Hidden : Visibility.Visible;

        public void EnableDisableDashboardMetroSection(DashboardMetroProcessType swid, bool enabled)
        {
            // read the series sections order
            string[] sections = AppSettings.DashboardMetroSectionOrder.Split(';');
            string moveSectionType = ((int)swid).ToString();

            string newSectionOrder = string.Empty;
            foreach (string sect in sections)
            {
                string thisSect = sect;
                string[] vals = sect.Split(':');
                if (vals[0].Trim().Equals(moveSectionType.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    thisSect = $"{moveSectionType}:{enabled.ToString()}";
                }

                if (!string.IsNullOrEmpty(newSectionOrder))
                    newSectionOrder += ";";

                newSectionOrder += thisSect;
            }

            AppSettings.DashboardMetroSectionOrder = newSectionOrder;
        }
    }
}
