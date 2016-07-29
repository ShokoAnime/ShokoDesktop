using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace JMMClient
{
    /// <summary>
    /// This class is used for the basic details about an AnimeGroup
    /// The only details we get from the server are the ones we want to display in the main list
    /// This is to make it simpler and faster
    /// When we want to get the extended details about a group we will get AnimeGroupDetailedVM record
    /// 
    /// </summary>
    public class AnimeGroupVM : MainListWrapper, INotifyPropertyChanged, IComparable<AnimeGroupVM>
    {
        private static Random fanartRandom = new Random();
        private static Random posterRandom = new Random();

        #region Readonly members
        public int? AnimeGroupID { get; set; }
        public DateTime DateTimeUpdated { get; set; }
        public int PlayedCount { get; set; }
        public int StoppedCount { get; set; }
        public int OverrideDescription { get; set; }
        public int IsManuallyNamed { get; set; }

        public int MissingEpisodeCount { get; set; }
        public int MissingEpisodeCountGroups { get; set; }

		public DateTime? Stat_AirDate_Min { get; set; }
		public DateTime? Stat_AirDate_Max { get; set; }
		public DateTime? Stat_EndDate { get; set; }
		public DateTime? Stat_SeriesCreatedDate { get; set; }
		public decimal? Stat_UserVotePermanent { get; set; }
		public decimal? Stat_UserVoteTemporary { get; set; }
		public decimal? Stat_UserVoteOverall { get; set; }
		public HashSet<string> Stat_AllTags { get; set; }
        public HashSet<string> Stat_AllCustomTags { get; set; }
		public HashSet<string> Stat_AllTitles { get; set; }
		public bool Stat_IsComplete { get; set; }
		public bool Stat_HasFinishedAiring { get; set; }
		public bool Stat_IsCurrentlyAiring { get; set; }
		public HashSet<string> Stat_AllVideoQuality { get; set; }
		public HashSet<string> Stat_AllVideoQualityEpisodes { get; set; }
		public HashSet<string> Stat_AudioLanguages { get; set; }
		public HashSet<string> Stat_SubtitleLanguages { get; set; }
		public bool Stat_HasTvDBLink { get; set; }
		public bool Stat_HasMALLink { get; set; }
		public bool Stat_HasMovieDBLink { get; set; }
		public bool Stat_HasMovieDBOrTvDBLink { get; set; }
		public int Stat_SeriesCount { get; set; }
		public int Stat_EpisodeCount { get; set; }
		public decimal Stat_AniDBRating { get; set; }

        public HashSet<int> DirectSeries { get; set; }
        public HashSet<int> AllSeries { get; set; }


        public void PopulateSerieInfo(Dictionary<int, AnimeGroupVM> groups, Dictionary<int,AnimeSeriesVM>  series)
        {
            List<int> allgroups = RecursiveGetGroups(groups, this).Select(a=>a.AnimeGroupID.Value).ToList();
            AllSeries = new HashSet<int>(series.Values.Where(a => allgroups.Contains(a.AnimeGroupID)).Select(a => a.AnimeSeriesID.Value));
            DirectSeries = new HashSet<int>(series.Values.Where(a=>a.AnimeGroupID==this.AnimeGroupID.Value).Select(a=>a.AnimeSeriesID.Value));
        }

        private List<AnimeGroupVM> RecursiveGetGroups(Dictionary<int, AnimeGroupVM> groups, AnimeGroupVM initialgrp)
        {
            List<AnimeGroupVM> ls = groups.Values.Where(a => a.AnimeGroupParentID.HasValue && a.AnimeGroupParentID.Value == initialgrp.AnimeGroupID.Value).ToList();
            foreach (AnimeGroupVM v in ls.ToList())
            {
                ls.AddRange(RecursiveGetGroups(groups,v));
            }
            ls.Add(initialgrp);
            return ls;
        }
        private static AnimeGroupSortMethod sortMethod = AnimeGroupSortMethod.SortName;
        public static AnimeGroupSortMethod SortMethod
        {
            get { return sortMethod; }
            set { sortMethod = value; }
        }

        public static SortDirection sortDirection = JMMClient.SortDirection.Ascending;
        public static SortDirection SortDirection
        {
            get { return sortDirection; }
            set { sortDirection = value; }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} - {1}", AnimeGroupID, GroupName);
        }

        #region Editable members

        private int? animeGroupParentID;
        public int? AnimeGroupParentID
        {
            get { return animeGroupParentID; }
            set
            {
                animeGroupParentID = value;
                NotifyPropertyChanged("AnimeGroupParentID");
            }
        }

        private int? defaultAnimeSeriesID;
        public int? DefaultAnimeSeriesID
        {
            get { return defaultAnimeSeriesID; }
            set
            {
                defaultAnimeSeriesID = value;
                NotifyPropertyChanged("DefaultAnimeSeriesID");
                HasDefaultSeries = defaultAnimeSeriesID.HasValue;
            }
        }

        private Boolean hasDefaultSeries = false;
        public Boolean HasDefaultSeries
        {
            get { return hasDefaultSeries; }
            set
            {
                hasDefaultSeries = value;
                NotifyPropertyChanged("HasDefaultSeries");
            }
        }

        private Boolean isReadOnly = true;
        public Boolean IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                NotifyPropertyChanged("IsReadOnly");
            }
        }

        private Boolean isBeingEdited = false;
        public Boolean IsBeingEdited
        {
            get { return isBeingEdited; }
            set
            {
                isBeingEdited = value;
                NotifyPropertyChanged("IsBeingEdited");
            }
        }

        private String groupName;
        public String GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
                NotifyPropertyChanged("GroupName");
            }
        }

        private int isFave;
        public int IsFave
        {
            get { return isFave; }
            set
            {
                isFave = value;
                NotifyPropertyChanged("IsFave");
                BIsFave = isFave == 1;
                BIsNotFave = isFave != 1;
            }
        }

        private Boolean bIsFave = false;
        public Boolean BIsFave
        {
            get { return bIsFave; }
            set
            {
                bIsFave = value;
                NotifyPropertyChanged("BIsFave");
            }
        }

        private Boolean bIsNotFave = false;
        public Boolean BIsNotFave
        {
            get { return bIsNotFave; }
            set
            {
                bIsNotFave = value;
                NotifyPropertyChanged("BIsNotFave");
            }
        }

        private Boolean userHasVoted;
        public Boolean UserHasVoted
        {
            get { return userHasVoted; }
            set
            {
                userHasVoted = value;
                NotifyPropertyChanged("UserHasVoted");
            }
        }

        private Boolean userHasVotedAny;
        public Boolean UserHasVotedAny
        {
            get { return userHasVotedAny; }
            set
            {
                userHasVotedAny = value;
                NotifyPropertyChanged("UserHasVotedAny");
            }
        }

        private int watchedEpisodeCount;
        public int WatchedEpisodeCount
        {
            get { return watchedEpisodeCount; }
            set
            {
                watchedEpisodeCount = value;
                NotifyPropertyChanged("WatchedEpisodeCount");
            }
        }

        private int unwatchedEpisodeCount;
        public int UnwatchedEpisodeCount
        {
            get { return unwatchedEpisodeCount; }
            set
            {
                unwatchedEpisodeCount = value;
                NotifyPropertyChanged("UnwatchedEpisodeCount");
            }
        }

        private int watchedCount;
        public int WatchedCount
        {
            get { return watchedCount; }
            set
            {
                watchedCount = value;
                NotifyPropertyChanged("WatchedCount");
            }
        }

        private DateTime? episodeAddedDate;
        public DateTime? EpisodeAddedDate
        {
            get { return episodeAddedDate; }
            set
            {
                episodeAddedDate = value;
                NotifyPropertyChanged("EpisodeAddedDate");
            }
        }

        private DateTime? watchedDate;
        public DateTime? WatchedDate
        {
            get { return watchedDate; }
            set
            {
                watchedDate = value;
                NotifyPropertyChanged("WatchedDate");
            }
        }


        private String sortName;
        public String SortName
        {
            get { return sortName; }
            set
            {
                sortName = value;
                NotifyPropertyChanged("SortName");
            }
        }

        private String description;
        public String Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }

        #endregion

        public int AllSeriesCount
        {
            get
            {
                return Stat_SeriesCount;
            }
        }

        public string TagsString
        {
            get
            {
                string tagsString = "";
                foreach (string cat in TagsList)
                {
                    if (!string.IsNullOrEmpty(tagsString))
                        tagsString += ", ";
                    tagsString += cat;
                }
                return tagsString;
            }
        }

		public List<string> TagsList
		{
			get
			{
				List<string> tagList = new List<string>();
				foreach (AnimeSeriesVM series in AllAnimeSeries)
				{
					foreach (string tag in series.AllTags)
					{
						if (!tagList.Contains(tag)) tagList.Add(tag);
					}
				}
				return tagList;
			}
		}

        public List<string> AnimeTypesList
        {
            get
            {
                List<string> atypeList = new List<string>();
                foreach (AnimeSeriesVM series in AllAnimeSeries)
                {
                    string atype = series.AniDB_Anime.AnimeTypeDescription;
                    if (!atypeList.Contains(atype)) atypeList.Add(atype);
                }
                return atypeList;
            }
        }

        public string AnimeTypesString
        {
            get
            {
                string atypesString = "";
                foreach (string atype in AnimeTypesList)
                {
                    if (!string.IsNullOrEmpty(atypesString))
                        atypesString += ", ";
                    atypesString += atype;
                }
                return atypesString;
            }
        }

        public string Summary
        {
            get
            {
                string summ = "";
                if (this.AllSubGroups.Count > 0)
                    summ = string.Format("{0} Groups", this.AllSubGroups.Count);



                if (summ.Length > 0) summ += ", ";

                if (MainListHelperVM.Instance.CurrentGroupFilter != null && MainListHelperVM.Instance.CurrentGroupFilter.IsApplyToSeries)
                {
                    List<AnimeSeriesVM> allSeries = AllAnimeSeries;
                    int serCount = 0;
                    foreach (AnimeSeriesVM ser in allSeries)
                    {
                        if (MainListHelperVM.Instance.CurrentGroupFilter.EvaluateGroupFilter(ser))
                            serCount++;
                    }
                    summ = summ + string.Format("{0} Series", serCount);
                }
                else
                    summ = summ + string.Format("{0} Series", AllAnimeSeries.Count);

                return summ;
            }
        }

        public bool HasUnwatchedFiles
        {
            get
            {
                return UnwatchedEpisodeCount > 0;
            }
        }

        public bool AllFilesWatched
        {
            get
            {
                return UnwatchedEpisodeCount == 0;
            }
        }

        public bool AnyFilesWatched
        {
            get
            {
                return WatchedEpisodeCount > 0;
            }
        }

        public bool HasMissingEpisodesAny
        {
            get
            {
                return (MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0);
            }
        }

        public bool HasMissingEpisodesAllDifferentToGroups
        {
            get
            {
                return (MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups);
            }
        }

        public bool HasMissingEpisodesGroups
        {
            get
            {
                return MissingEpisodeCountGroups > 0;
            }
        }

        public bool HasMissingEpisodes
        {
            get
            {
                return MissingEpisodeCountGroups > 0;
            }
        }

        /*public bool IsComplete
		{
			get
			{
				if (!Stat_EndDate.HasValue) return false; // ongoing

				// all series have finished airing and the user has all the episodes
				if (Stat_EndDate.Value < DateTime.Now && !HasMissingEpisodesAny) return true;

				return false;
			}
		}*/

        /*public bool FinishedAiring
		{
			get
			{
				if (!Stat_EndDate.HasValue) return false; // ongoing

				// all series have finished airing
				if (Stat_EndDate.Value < DateTime.Now) return true;

				return false;
			}
		}*/

        public AnimeSeriesVM DefaultSeries
        {
            get
            {
                if (!HasDefaultSeries) return null;

                if (!MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(DefaultAnimeSeriesID.Value)) return null;

                return MainListHelperVM.Instance.AllSeriesDictionary[DefaultAnimeSeriesID.Value];
            }
        }

        private List<string> GetPosterFilenames()
        {
            List<string> allPosters = new List<string>();

            // check if user has specied a fanart to always be used
            if (DefaultSeries != null)
            {
                if (!string.IsNullOrEmpty(DefaultSeries.AniDB_Anime.DefaultPosterPathNoBlanks) && File.Exists(DefaultSeries.AniDB_Anime.DefaultPosterPathNoBlanks))
                {
                    allPosters.Add(DefaultSeries.AniDB_Anime.DefaultPosterPathNoBlanks);
                    return allPosters;
                }
            }

            foreach (AnimeSeriesVM ser in AllAnimeSeries)
            {
                if (!string.IsNullOrEmpty(ser.AniDB_Anime.DefaultPosterPathNoBlanks) && File.Exists(ser.AniDB_Anime.DefaultPosterPathNoBlanks))
                    allPosters.Add(ser.AniDB_Anime.DefaultPosterPathNoBlanks);
            }

            return allPosters;
        }

        public string PosterPath
        {
            get
            {
                string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);

                List<string> allPosters = GetPosterFilenames();
                string posterName = "";
                if (allPosters.Count > 0)
                    posterName = allPosters[fanartRandom.Next(0, allPosters.Count)];

                if (!String.IsNullOrEmpty(posterName))
                    return posterName;

                return packUriBlank;
            }
        }

        public string FullImagePath
        {
            get
            {
                return PosterPath;
            }
        }

        private List<string> GetFanartFilenames()
        {
            List<string> allFanart = new List<string>();

            // check if user has specied a fanart to always be used
            if (DefaultSeries != null)
            {
                if (DefaultSeries.AniDB_Anime.DefaultFanart != null && !string.IsNullOrEmpty(DefaultSeries.AniDB_Anime.DefaultFanart.FullImagePathOnlyExisting)
                    && File.Exists(DefaultSeries.AniDB_Anime.DefaultFanart.FullImagePathOnlyExisting))
                {
                    allFanart.Add(DefaultSeries.AniDB_Anime.DefaultFanart.FullImagePathOnlyExisting);
                    return allFanart;
                }
            }


            foreach (AnimeSeriesVM ser in AllAnimeSeries)
            {
                foreach (FanartContainer fanart in ser.AniDB_Anime.AniDB_AnimeCrossRefs.AllFanarts)
                {
                    if (!fanart.IsImageEnabled) continue;
                    if (!File.Exists(fanart.FullImagePath)) continue;

                    allFanart.Add(fanart.FullImagePath);
                }
            }


            return allFanart;
        }

        public bool UseFanartOnSeries
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return false;
                if (string.IsNullOrEmpty(FanartPath)) return false;

                return true;

            }
        }

        public bool UsePosterOnSeries
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return true;
                if (string.IsNullOrEmpty(FanartPath)) return true;

                return false;

            }
        }

        public string FanartPath
        {
            get
            {
                List<string> allFanarts = GetFanartFilenames();
                string fanartName = "";
                if (allFanarts.Count > 0)
                {
                    fanartName = allFanarts[fanartRandom.Next(0, allFanarts.Count)];
                }

                if (!String.IsNullOrEmpty(fanartName))
                    return fanartName;


                return "";
            }
        }

        public string FanartPathThenPosterPath
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return PosterPath;

                if (string.IsNullOrEmpty(FanartPath))
                    return PosterPath;

                return FanartPath;
            }
        }



        public string LastWatchedDescription
        {
            get
            {
                if (WatchedDate.HasValue)
                {
                    DateTime today = DateTime.Now;
                    DateTime yesterday = today.AddDays(-1);

                    if (WatchedDate.Value.Day == today.Day && WatchedDate.Value.Month == today.Month && WatchedDate.Value.Year == today.Year)
                        return JMMClient.Properties.Resources.Today;

                    if (WatchedDate.Value.Day == yesterday.Day && WatchedDate.Value.Month == yesterday.Month && WatchedDate.Value.Year == yesterday.Year)
                        return JMMClient.Properties.Resources.Yesterday;

                    return WatchedDate.Value.ToString("dd MMM yyyy", Globals.Culture);
                }
                else
                    return "";
            }
        }


        public decimal AniDBTotalRating
        {
            get
            {
                try
                {
                    decimal totalRating = 0;
                    foreach (AnimeSeriesVM series in AllAnimeSeries)
                    {
                        totalRating += ((decimal)series.AniDB_Anime.Rating * series.AniDB_Anime.VoteCount);
                        totalRating += ((decimal)series.AniDB_Anime.TempRating * series.AniDB_Anime.TempVoteCount);
                    }

                    return totalRating;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public int AniDBTotalVotes
        {
            get
            {
                try
                {
                    int cnt = 0;
                    foreach (AnimeSeriesVM series in AllAnimeSeries)
                    {
                        cnt += series.AniDB_Anime.AniDBTotalVotes;
                    }

                    return cnt;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public decimal AniDBRating
        {
            get
            {
                try
                {
                    /*if (AniDBTotalVotes == 0)
						return 0;
					else
						return AniDBTotalRating / (decimal)AniDBTotalVotes / (decimal)100;*/

                    return Stat_AniDBRating / (decimal)100;

                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public string AniDBRatingFormatted
        {
            get
            {
                return string.Format("{0} ({1} {2})", Utils.FormatAniDBRating((double)AniDBRating),
                    AniDBTotalVotes, JMMClient.Properties.Resources.Votes);
            }
        }


        public string EpisodeCountFormatted
        {
            get
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                int epCountNormal = 0;
                int epCountSpecial = 0;
                foreach (AnimeSeriesVM series in AllAnimeSeries)
                {
                    epCountNormal += series.AniDB_Anime.EpisodeCountNormal;
                    epCountSpecial += series.AniDB_Anime.EpisodeCountSpecial;
                }

                return string.Format("{0} {1} ({2} {3})", epCountNormal, JMMClient.Properties.Resources.Episodes,
                    epCountSpecial, JMMClient.Properties.Resources.Specials);
            }
        }

        public string GroupType
        {
            get
            {
                if (this.AnimeGroupParentID.HasValue)
                    return JMMClient.Properties.Resources.SubGroup;
                else
                    return JMMClient.Properties.Resources.Group;
            }
        }


        public AnimeGroupVM()
        {
        }

        public void Populate(JMMServerBinary.Contract_AnimeGroup contract)
        {
            if (contract.AnimeGroupID == 189)
            {
                Debug.Print("");
            }

            // readonly members
            this.AnimeGroupID = contract.AnimeGroupID;
            this.AnimeGroupParentID = contract.AnimeGroupParentID;
            this.DateTimeUpdated = contract.DateTimeUpdated;
            this.IsManuallyNamed = contract.IsManuallyNamed;
            this.MissingEpisodeCount = contract.MissingEpisodeCount;
            this.MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;
            this.PlayedCount = contract.PlayedCount;
            this.StoppedCount = contract.StoppedCount;
            this.UnwatchedEpisodeCount = contract.UnwatchedEpisodeCount;
            this.WatchedCount = contract.WatchedCount;
            this.EpisodeAddedDate = contract.EpisodeAddedDate;
            this.WatchedDate = contract.WatchedDate;
            this.WatchedEpisodeCount = contract.WatchedEpisodeCount;



			this.Stat_AirDate_Min = contract.Stat_AirDate_Min;
			this.Stat_AirDate_Max = contract.Stat_AirDate_Max;
			this.Stat_EndDate = contract.Stat_EndDate;
			this.Stat_SeriesCreatedDate = contract.Stat_SeriesCreatedDate;
			this.Stat_UserVoteOverall = contract.Stat_UserVoteOverall;
			this.Stat_UserVotePermanent = contract.Stat_UserVotePermanent;
			this.Stat_UserVoteTemporary = contract.Stat_UserVoteTemporary;
            this.Stat_AllTags = new HashSet<string>(contract.Stat_AllTags,StringComparer.InvariantCultureIgnoreCase);
            this.Stat_AllCustomTags = new HashSet<string>(contract.Stat_AllCustomTags,StringComparer.InvariantCultureIgnoreCase);
			this.Stat_AllTitles = new HashSet<string>(contract.Stat_AllTitles,StringComparer.InvariantCultureIgnoreCase);
			this.Stat_IsComplete = contract.Stat_IsComplete;
			this.Stat_HasFinishedAiring = contract.Stat_HasFinishedAiring;
			this.Stat_IsCurrentlyAiring = contract.Stat_IsCurrentlyAiring;
			this.Stat_AllVideoQuality = new HashSet<string>(contract.Stat_AllVideoQuality);
			this.Stat_AllVideoQualityEpisodes = new HashSet<string>(contract.Stat_AllVideoQuality_Episodes);
			this.Stat_AudioLanguages = new HashSet<string>(contract.Stat_AudioLanguages);
			this.Stat_SubtitleLanguages = new HashSet<string>(contract.Stat_SubtitleLanguages);
			this.Stat_HasTvDBLink = contract.Stat_HasTvDBLink;
			this.Stat_HasMALLink = contract.Stat_HasMALLink;
			this.Stat_HasMovieDBLink = contract.Stat_HasMovieDBLink;
			this.Stat_HasMovieDBOrTvDBLink = contract.Stat_HasMovieDBOrTvDBLink;
			this.Stat_SeriesCount = contract.Stat_SeriesCount;
			this.Stat_EpisodeCount = contract.Stat_EpisodeCount;
			this.Stat_AniDBRating = contract.Stat_AniDBRating;

            // editable members
            this.GroupName = contract.GroupName;
            this.IsFave = contract.IsFave;
            this.SortName = contract.SortName;
            this.DefaultAnimeSeriesID = contract.DefaultAnimeSeriesID;
            if (contract.DefaultAnimeSeriesID.HasValue)
                this.HasDefaultSeries = contract.DefaultAnimeSeriesID.HasValue;
            this.Description = contract.Description;
            this.UserHasVoted = this.Stat_UserVotePermanent.HasValue;
            this.UserHasVotedAny = this.Stat_UserVotePermanent.HasValue || this.Stat_UserVoteTemporary.HasValue;
        }

        public AnimeGroupVM(JMMServerBinary.Contract_AnimeGroup contract)
        {
            Populate(contract);
        }

        public JMMServerBinary.Contract_AnimeGroup_Save ToContract()
        {
            JMMServerBinary.Contract_AnimeGroup_Save contract = new JMMServerBinary.Contract_AnimeGroup_Save();
            contract.AnimeGroupID = this.AnimeGroupID;
            contract.AnimeGroupParentID = this.AnimeGroupParentID;
            contract.IsManuallyNamed = this.IsManuallyNamed;
            //contract.DateTimeUpdated = this.DateTimeUpdated;
            //contract.MissingEpisodesCount = this.MissingEpisodesCount;
            //contract.PlayedCount = this.PlayedCount;
            //contract.StoppedCount = this.StoppedCount;
            //contract.UnwatchedEpisodeCount = this.UnwatchedEpisodeCount;
            //contract.WatchedCount = this.WatchedCount;
            //contract.WatchedDate = this.WatchedDate;
            //contract.WatchedEpisodeCount = this.WatchedEpisodeCount;

            // editable members
            contract.GroupName = this.GroupName;
            contract.IsFave = this.IsFave;
            contract.SortName = this.SortName;
            contract.Description = this.Description;

            return contract;
        }




        /// <summary>
        /// returns the direct child series of this group
        /// </summary>
        public List<AnimeSeriesVM> AnimeSeries
        {
            get
            {
                return DirectSeries.Select(a => MainListHelperVM.Instance.AllSeriesDictionary.SureGet(a))
                        .Where(a => a != null)
                        .OrderBy(a => a.Stat_AirDate_Min).ToList();
                /*
                List<AnimeSeriesVM> series = new List<AnimeSeriesVM>();
                foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
                {
                    if (ser.AnimeGroupID == this.AnimeGroupID)
                    {
                        series.Add(ser);
                    }
                }

                JMMClient.AnimeSeriesVM.SortType = JMMClient.AnimeSeriesVM.SortMethod.AirDate;
                series.Sort();

                return series;*/
            }
        }

        /// <summary>
        /// returns all the anime series under this group  and sub groups
        /// </summary>
        public List<AnimeSeriesVM> AllAnimeSeries
        {
            get
            {
                return
                    AllSeries.Select(a => MainListHelperVM.Instance.AllSeriesDictionary.SureGet(a))
                        .Where(a => a != null)
                        .OrderBy(a=>a.Stat_AirDate_Min).ToList();
                /*
                List<AnimeSeriesVM> series = new List<AnimeSeriesVM>();
                try
                {
                    GetAnimeSeriesRecursive(this, ref series);

                    JMMClient.AnimeSeriesVM.SortType = JMMClient.AnimeSeriesVM.SortMethod.AirDate;
                    series.Sort();
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return series;
                */
            }
        }

        /// <summary>
        /// returns all the anime series under this group and sub groups, which also pass the gurrent GroupFilter conditions
        /// </summary>
        public List<AnimeSeriesVM> AllAnimeSeriesFiltered
        {
            get
            {
                List<AnimeSeriesVM> series = new List<AnimeSeriesVM>();
                try
                {
                    // check if the current group filter is also applied to series
                    if (MainListHelperVM.Instance.CurrentGroupFilter != null && MainListHelperVM.Instance.CurrentGroupFilter.IsApplyToSeries)
                    {
                        foreach (AnimeSeriesVM ser in AllAnimeSeries)
                        {
                            if (MainListHelperVM.Instance.CurrentGroupFilter.EvaluateGroupFilter(ser))
                                series.Add(ser);
                        }
                    }
                    else
                        series = AllAnimeSeries;
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return series;
            }
        }

        public override List<MainListWrapper> GetDirectChildren()
        {
            List<MainListWrapper> children = new List<MainListWrapper>();
            children.AddRange(SubGroups);

            // check if the current group filter is also applied to series
            if (MainListHelperVM.Instance.CurrentGroupFilter != null && MainListHelperVM.Instance.CurrentGroupFilter.IsApplyToSeries)
            {
                foreach (AnimeSeriesVM ser in AnimeSeries)
                {
                    if (MainListHelperVM.Instance.CurrentGroupFilter.EvaluateGroupFilter(ser))
                        children.Add(ser);
                }
            }
            else
                children.AddRange(AnimeSeries);

            return children;
        }

        public AnimeGroupVM ParentGroup
        {
            get
            {
                if (!AnimeGroupParentID.HasValue) return null;
                if (MainListHelperVM.Instance.AllGroupsDictionary.ContainsKey(AnimeGroupParentID.Value))
                    return MainListHelperVM.Instance.AllGroupsDictionary[AnimeGroupParentID.Value];
                return null;
            }
        }

        public List<AnimeGroupVM> AllSubGroups
        {
            get
            {
                List<AnimeGroupVM> grps = new List<AnimeGroupVM>();
                GetAnimeGroupsRecursive(this, ref grps);
                return grps;
            }
        }

        public List<AnimeGroupVM> SubGroups
        {
            get
            {
                return MainListHelperVM.Instance.AllGroupsDictionary.Values.Where(
                        a => a.AnimeGroupParentID.HasValue && a.AnimeGroupParentID.Value == AnimeGroupID).ToList();

            }
        }

        private static void GetAnimeGroupsRecursive(AnimeGroupVM grp, ref List<AnimeGroupVM> groupList)
        {
            // get the child groups for this group
            groupList.AddRange(grp.SubGroups);

            foreach (AnimeGroupVM subGroup in grp.SubGroups)
            {
                GetAnimeGroupsRecursive(subGroup, ref groupList);
            }
        }

        private static void GetAnimeSeriesRecursive(AnimeGroupVM grp, ref List<AnimeSeriesVM> seriesList)
        {
            // get the child groups for this group
            seriesList.AddRange(grp.AnimeSeries);

            foreach (AnimeGroupVM subGroup in grp.SubGroups)
            {
                GetAnimeSeriesRecursive(subGroup, ref seriesList);
            }
        }

        public int CompareTo(AnimeGroupVM obj)
        {
            switch (SortMethod)
            {
                case AnimeGroupSortMethod.SortName:
                    return SortName.CompareTo(obj.SortName);

                case AnimeGroupSortMethod.IsFave:
                    return IsFave.CompareTo(obj.IsFave);

                default:
                    return SortName.CompareTo(obj.SortName);
            }

        }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(this.GroupName))
            {
                MessageBox.Show("Group name must be populated");
                return false;
            }

            if (string.IsNullOrEmpty(this.SortName))
            {
                MessageBox.Show("Sort name must be populated");
                return false;
            }

            return true;
        }


        public bool Save()
        {
            try
            {
                JMMServerBinary.Contract_AnimeGroup_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.SaveGroup(this.ToContract(),
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    MessageBox.Show(response.ErrorMessage);
                    return false;
                }
                else
                {
                    this.Populate(response.AnimeGroup);
                    MainListHelperVM.Instance.AllGroupsDictionary[this.AnimeGroupID.Value] = this;
                    PopulateSerieInfo(MainListHelperVM.Instance.AllGroupsDictionary,MainListHelperVM.Instance.AllSeriesDictionary);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                return false;
            }
        }
    }
}
