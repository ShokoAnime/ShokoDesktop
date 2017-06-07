using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
using Formatting = Shoko.Commons.Utils.Formatting;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    /// <summary>
    /// This class is used for the basic details about an AnimeGroup
    /// The only details we get from the server are the ones we want to display in the main list
    /// This is to make it simpler and faster
    /// When we want to get the extended details about a group we will get AnimeGroupDetailedVM record
    /// 
    /// </summary>
    public class VM_AnimeGroup_User : CL_AnimeGroup_User, IListWrapper, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        private static readonly Random fanartRandom = new Random();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int ObjectType => 1;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsEditable => true;
       
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public HashSet<int> DirectSeries { get; set; }
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public HashSet<int> AllSeries { get; set; }


        public void PopulateSerieInfo(ObservableListDictionary<int, VM_AnimeGroup_User> groups, ObservableListDictionary<int, VM_AnimeSeries_User> series)
        {
            List<int> allgroups = RecursiveGetGroups(groups, this).Select(a => a.AnimeGroupID).ToList();
            AllSeries = new HashSet<int>(series.Values.Where(a => allgroups.Contains(a.AnimeGroupID)).Select(a => a.AnimeSeriesID));
            DirectSeries = new HashSet<int>(series.Values.Where(a => a.AnimeGroupID == AnimeGroupID).Select(a => a.AnimeSeriesID));
        }

        private List<VM_AnimeGroup_User> RecursiveGetGroups(ObservableListDictionary<int, VM_AnimeGroup_User> groups, VM_AnimeGroup_User initialgrp)
        {
            List<VM_AnimeGroup_User> ls = groups.Values.Where(a => a.AnimeGroupParentID.HasValue && a.AnimeGroupParentID.Value == initialgrp.AnimeGroupID).ToList();
            foreach (VM_AnimeGroup_User v in ls.ToList())
            {
                ls.AddRange(RecursiveGetGroups(groups, v));
            }
            ls.Add(initialgrp);
            return ls;
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public static AnimeGroupSortMethod SortMethod { get; set; } = AnimeGroupSortMethod.SortName;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public static SortDirection sortDirection = SortDirection.Ascending;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public static SortDirection SortDirection
        {
            get { return sortDirection; }
            set { sortDirection = value; }
        }





        public override string ToString()
        {
            return $"{AnimeGroupID} - {GroupName}";
        }

        #region Editable members

        public new int? AnimeGroupParentID
        {
            get { return base.AnimeGroupParentID; }
            set { this.SetField(()=>base.AnimeGroupParentID,(r)=> base.AnimeGroupParentID = r, value); }
        }


        public new int? DefaultAnimeSeriesID
        {
            get { return base.DefaultAnimeSeriesID; }
            set { this.SetField(()=>base.DefaultAnimeSeriesID,(r)=> base.DefaultAnimeSeriesID = r, value, ()=>DefaultAnimeSeriesID,()=>HasDefaultSeries); }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasDefaultSeries => base.DefaultAnimeSeriesID.HasValue;

        private bool isReadOnly = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { this.SetField(()=>isReadOnly, value); }
        }

        private bool isBeingEdited;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsBeingEdited
        {
            get { return isBeingEdited; }
            set { this.SetField(()=>isBeingEdited, value); }
        }

        public new string GroupName
        {
            get { return base.GroupName; }
            set { this.SetField(()=>base.GroupName,(r)=> base.GroupName = r, value); }
        }

        public new int IsFave
        {
            get { return base.IsFave; }
            set
            {
                this.SetField(()=>base.IsFave,(r)=> base.IsFave = r, value, ()=>IsFave, ()=>BIsFave);
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool BIsFave => IsFave == 1;

        public new decimal? Stat_UserVoteTemporary
        {
            get  { return  base. Stat_UserVoteTemporary; }
            set { this.SetField(()=>base.Stat_UserVoteTemporary,(r)=> base.Stat_UserVoteTemporary = r, value, () => Stat_UserVoteTemporary, () => UserHasVotedAny); }
        }

        public new decimal? Stat_UserVotePermanent
        {
            get { return base.Stat_UserVotePermanent; }
            set { this.SetField(()=>base.Stat_UserVotePermanent,(r)=> base.Stat_UserVotePermanent = r, value, ()=>Stat_UserVotePermanent, ()=> UserHasVoted, () => UserHasVotedAny); }
        }
        
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool UserHasVoted => Stat_UserVotePermanent.HasValue;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool UserHasVotedAny => Stat_UserVotePermanent.HasValue || Stat_UserVoteTemporary.HasValue;


        public new int WatchedEpisodeCount
        {
            get { return base.WatchedEpisodeCount; }
            set { this.SetField(()=>base.WatchedEpisodeCount,(r)=> base.WatchedEpisodeCount = r, value); }
        }

        public new int UnwatchedEpisodeCount
        {
            get { return base.UnwatchedEpisodeCount; }
            set { this.SetField(()=>base.UnwatchedEpisodeCount,(r)=> base.UnwatchedEpisodeCount = r, value); }
        }


        public new int WatchedCount
        {
            get { return base.WatchedCount; }
            set { this.SetField(()=>base.WatchedCount,(r)=> base.WatchedCount = r, value); }
        }

       
        public new DateTime? EpisodeAddedDate
        {
            get { return base.EpisodeAddedDate; }
            set { this.SetField(()=>base.EpisodeAddedDate,(r)=> base.EpisodeAddedDate = r, value); 
            }
        }

        public new DateTime? WatchedDate
        {
            get { return base.WatchedDate; }
            set { this.SetField(()=>base.WatchedDate,(r)=> base.WatchedDate = r, value); }
        }


        public new string SortName
        {
            get { return base.SortName; }
            set { this.SetField(()=>base.SortName,(r)=> base.SortName = r, value); }
        }

        public new string Description
        {
            get { return base.Description; }
            set { this.SetField(()=>base.Description,(r)=> base.Description = r, value); }
        }

        #endregion

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int AllSeriesCount => Stat_SeriesCount;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<string> TagsList
        {
            get
            {
                List<string> tagList = new List<string>();
                foreach (VM_AnimeSeries_User series in AllAnimeSeries)
                {
                    foreach (string tag in series.AllTags)
                    {
                        if (!tagList.Contains(tag)) tagList.Add(tag);
                    }
                }
                return tagList;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<string> AnimeTypesList
        {
            get
            {
                List<string> atypeList = new List<string>();
                foreach (VM_AnimeSeries_User series in AllAnimeSeries)
                {
                    string atype = series.AniDBAnime.AniDBAnime.AnimeTypeDescription;
                    if (!atypeList.Contains(atype)) atypeList.Add(atype);
                }
                return atypeList;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string Summary
        {
            get
            {
                string summ = "";
                if (AllSubGroups.Count > 0)
                    summ = $"{AllSubGroups.Count} Groups";



                if (summ.Length > 0) summ += ", ";

                if (VM_MainListHelper.Instance.CurrentGroupFilter != null && VM_MainListHelper.Instance.CurrentGroupFilter.IsApplyToSeries)
                {
                    List<VM_AnimeSeries_User> allSeries = AllAnimeSeries;
                    int serCount = 0;
                    foreach (VM_AnimeSeries_User ser in allSeries)
                    {
                        if (VM_MainListHelper.Instance.CurrentGroupFilter.EvaluateGroupFilter(ser))
                            serCount++;
                    }
                    summ = summ + $"{serCount} Series";
                }
                else
                    summ = summ + $"{AllAnimeSeries.Count} Series";

                return summ;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasUnwatchedFiles => UnwatchedEpisodeCount > 0;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool AllFilesWatched => UnwatchedEpisodeCount == 0;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool AnyFilesWatched => WatchedEpisodeCount > 0;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasMissingEpisodesAny => (MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0);

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasMissingEpisodesAllDifferentToGroups => (MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups);

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasMissingEpisodesGroups => MissingEpisodeCountGroups > 0;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasMissingEpisodes => MissingEpisodeCountGroups > 0;

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

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public VM_AnimeSeries_User DefaultSeries
        {
            get
            {
                if (!DefaultAnimeSeriesID.HasValue) return null;

                if (!VM_MainListHelper.Instance.AllSeriesDictionary.ContainsKey(DefaultAnimeSeriesID.Value)) return null;

                return VM_MainListHelper.Instance.AllSeriesDictionary[DefaultAnimeSeriesID.Value];
            }
        }

        private List<string> GetPosterFilenames()
        {
            List<string> allPosters = new List<string>();

            // check if user has specied a fanart to always be used
            if (!string.IsNullOrEmpty(DefaultSeries?.AniDBAnime.AniDBAnime.DefaultPosterPathNoBlanks) && File.Exists(DefaultSeries.AniDBAnime.AniDBAnime.DefaultPosterPathNoBlanks))
            {
                allPosters.Add(DefaultSeries.AniDBAnime.AniDBAnime.DefaultPosterPathNoBlanks);
                return allPosters;
            }

            foreach (VM_AnimeSeries_User ser in AllAnimeSeries)
            {
                if (!string.IsNullOrEmpty(ser.AniDBAnime.AniDBAnime.DefaultPosterPathNoBlanks) && File.Exists(ser.AniDBAnime.AniDBAnime.DefaultPosterPathNoBlanks))
                    allPosters.Add(ser.AniDBAnime.AniDBAnime.DefaultPosterPathNoBlanks);
            }

            return allPosters;
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string PosterPath
        {
            get
            {
                string packUriBlank = $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";

                List<string> allPosters = GetPosterFilenames();
                string posterName = "";
                if (allPosters.Count > 0)
                    posterName = allPosters[fanartRandom.Next(0, allPosters.Count)];

                if (!String.IsNullOrEmpty(posterName))
                    return posterName;

                return packUriBlank;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullImagePath => PosterPath;

        private List<string> GetFanartFilenames()
        {
            List<string> allFanart = new List<string>();

            // check if user has specied a fanart to always be used
            if (!string.IsNullOrEmpty(DefaultSeries?.AniDBAnime.AniDBAnime.DefaultImageFanart?.FullImagePathOnlyExisting) && File.Exists(DefaultSeries.AniDBAnime.AniDBAnime.DefaultImageFanart.FullImagePathOnlyExisting))
            {
                allFanart.Add(DefaultSeries.AniDBAnime.AniDBAnime.DefaultImageFanart.FullImagePathOnlyExisting);
                return allFanart;
            }


            foreach (VM_AnimeSeries_User ser in AllAnimeSeries)
            {
                foreach (VM_FanartContainer fanart in ser.AniDBAnime.AniDBAnime.AniDB_AnimeCrossRefs.AllFanarts)
                {
                    if (!fanart.IsImageEnabled) continue;
                    if (!File.Exists(fanart.FullImagePath)) continue;

                    allFanart.Add(fanart.FullImagePath);
                }
            }


            return allFanart;
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool UseFanartOnSeries
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return false;
                if (string.IsNullOrEmpty(FanartPath)) return false;

                return true;

            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool UsePosterOnSeries
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return true;
                if (string.IsNullOrEmpty(FanartPath)) return true;

                return false;

            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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


        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string LastWatchedDescription
        {
            get
            {
                if (WatchedDate.HasValue)
                {
                    DateTime today = DateTime.Now;
                    DateTime yesterday = today.AddDays(-1);

                    if (WatchedDate.Value.Day == today.Day && WatchedDate.Value.Month == today.Month && WatchedDate.Value.Year == today.Year)
                        return Shoko.Commons.Properties.Resources.Today;

                    if (WatchedDate.Value.Day == yesterday.Day && WatchedDate.Value.Month == yesterday.Month && WatchedDate.Value.Year == yesterday.Year)
                        return Shoko.Commons.Properties.Resources.Yesterday;

                    return WatchedDate.Value.ToString("dd MMM yyyy", Commons.Culture.Global);
                }
                return "";
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public decimal AniDBTotalRating
        {
            get
            {
                try
                {
                    decimal totalRating = 0;
                    foreach (VM_AnimeSeries_User series in AllAnimeSeries)
                    {
                        totalRating += ((decimal)series.AniDBAnime.AniDBAnime.Rating * series.AniDBAnime.AniDBAnime.VoteCount);
                        totalRating += ((decimal)series.AniDBAnime.AniDBAnime.TempRating * series.AniDBAnime.AniDBAnime.TempVoteCount);
                    }

                    return totalRating;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int AniDBTotalVotes
        {
            get
            {
                try
                {
                    int cnt = 0;
                    foreach (VM_AnimeSeries_User series in AllAnimeSeries)
                    {
                        cnt += series.AniDBAnime.AniDBAnime.AniDBTotalVotes;
                    }

                    return cnt;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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

                    return Stat_AniDBRating / 100;

                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDBRatingFormatted => $"{Formatting.FormatAniDBRating((double) AniDBRating)} ({AniDBTotalVotes} {Shoko.Commons.Properties.Resources.Votes})";

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeCountFormatted
        {
            get
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                int epCountNormal = 0;
                int epCountSpecial = 0;
                foreach (VM_AnimeSeries_User series in AllAnimeSeries)
                {
                    epCountNormal += series.AniDBAnime.AniDBAnime.EpisodeCountNormal;
                    epCountSpecial += series.AniDBAnime.AniDBAnime.EpisodeCountSpecial;
                }

                // Multiple Episodes
                if (epCountNormal > 1)
                {
                    // Multiple Episodes, Multiple Specials
                    if (epCountSpecial > 1)
                    {
                        return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                    }
                    // Multiple Episodes, No Specials
                    if (epCountSpecial <= 0)
                    {
                        return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                    }
                    return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Special})";
                }
                // No Episodes, No Specials 
                if (epCountNormal <= 0)
                {
                    return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                }

                // Single Episode, Multiple Specials
                if (epCountSpecial >= 1)
                {
                    return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episode} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                }
                // Single Episodes, No Specials
                if (epCountSpecial <= 0)
                {
                    return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episode} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                }
                return $"{epCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episode} ({epCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Special})";
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string GroupType
        {
            get
            {
                if (AnimeGroupParentID.HasValue)
                    return Shoko.Commons.Properties.Resources.SubGroup;
                return Shoko.Commons.Properties.Resources.Group;
            }
        }


        public VM_AnimeGroup_User()
        {
            AllSeries = new HashSet<int>();
        }

        // ReSharper disable once FunctionComplexityOverflow
        public void Populate(CL_AnimeGroup_User contract)
        {

            // readonly members
            AnimeGroupID = contract.AnimeGroupID;
            AnimeGroupParentID = contract.AnimeGroupParentID;
            DateTimeUpdated = contract.DateTimeUpdated;
            IsManuallyNamed = contract.IsManuallyNamed;
            MissingEpisodeCount = contract.MissingEpisodeCount;
            MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;
            PlayedCount = contract.PlayedCount;
            StoppedCount = contract.StoppedCount;
            UnwatchedEpisodeCount = contract.UnwatchedEpisodeCount;
            WatchedCount = contract.WatchedCount;
            EpisodeAddedDate = contract.EpisodeAddedDate;
            WatchedDate = contract.WatchedDate;
            WatchedEpisodeCount = contract.WatchedEpisodeCount;
            Stat_AirDate_Min = contract.Stat_AirDate_Min;
            Stat_AirDate_Max = contract.Stat_AirDate_Max;
            Stat_EndDate = contract.Stat_EndDate;
            Stat_SeriesCreatedDate = contract.Stat_SeriesCreatedDate;
            Stat_UserVoteOverall = contract.Stat_UserVoteOverall;
            Stat_UserVotePermanent = contract.Stat_UserVotePermanent;
            Stat_UserVoteTemporary = contract.Stat_UserVoteTemporary;
            Stat_AllTags = new HashSet<string>(contract.Stat_AllTags, StringComparer.InvariantCultureIgnoreCase);
            Stat_AllCustomTags = new HashSet<string>(contract.Stat_AllCustomTags, StringComparer.InvariantCultureIgnoreCase);
            Stat_AllTitles = new HashSet<string>(contract.Stat_AllTitles, StringComparer.InvariantCultureIgnoreCase);
            Stat_IsComplete = contract.Stat_IsComplete;
            Stat_HasFinishedAiring = contract.Stat_HasFinishedAiring;
            Stat_IsCurrentlyAiring = contract.Stat_IsCurrentlyAiring;
            Stat_AllVideoQuality = new HashSet<string>(contract.Stat_AllVideoQuality);
            Stat_AllVideoQuality_Episodes = new HashSet<string>(contract.Stat_AllVideoQuality_Episodes);
            Stat_AudioLanguages = new HashSet<string>(contract.Stat_AudioLanguages);
            Stat_SubtitleLanguages = new HashSet<string>(contract.Stat_SubtitleLanguages);
            Stat_HasTvDBLink = contract.Stat_HasTvDBLink;
            Stat_HasMALLink = contract.Stat_HasMALLink;
            Stat_HasMovieDBLink = contract.Stat_HasMovieDBLink;
            Stat_HasMovieDBOrTvDBLink = contract.Stat_HasMovieDBOrTvDBLink;
            Stat_SeriesCount = contract.Stat_SeriesCount;
            Stat_EpisodeCount = contract.Stat_EpisodeCount;
            Stat_AniDBRating = contract.Stat_AniDBRating;

            // editable members
            GroupName = contract.GroupName;
            IsFave = contract.IsFave;
            SortName = contract.SortName;
            DefaultAnimeSeriesID = contract.DefaultAnimeSeriesID;
            Description = contract.Description;

        }



        public CL_AnimeGroup_Save_Request ToRequest()
        {
            return new CL_AnimeGroup_Save_Request
            {
                AnimeGroupID = AnimeGroupID,
                AnimeGroupParentID = AnimeGroupParentID,
                IsManuallyNamed = IsManuallyNamed,
                GroupName = GroupName,
                IsFave = IsFave,
                SortName = SortName,
                Description = Description
            };
        }




        /// <summary>
        /// returns the direct child series of this group
        /// </summary>
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<VM_AnimeSeries_User> AnimeSeries
        {
            get
            {
                return DirectSeries.Select(a => VM_MainListHelper.Instance.AllSeriesDictionary.SureGet(a))
                        .Where(a => a != null)
                        .OrderBy(a => a.Stat_AirDate_Min).ToList();
                /*
                List<VM_AnimeSeries_User> series = new List<VM_AnimeSeries_User>();
                foreach (VM_AnimeSeries_User ser in VM_MainListHelper.Instance.AllSeries)
                {
                    if (ser.AnimeGroupID == this.AnimeGroupID)
                    {
                        series.Add(ser);
                    }
                }

                JMMClient.VM_AnimeSeries_User.SortType = JMMClient.VM_AnimeSeries_User.SortMethod.AirDate;
                series.Sort();

                return series;*/
            }
        }

        /// <summary>
        /// returns all the anime series under this group  and sub groups
        /// </summary>
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<VM_AnimeSeries_User> AllAnimeSeries
        {
            get
            {
	            if (AllSeries == null) PopulateSerieInfo(VM_MainListHelper.Instance.AllGroupsDictionary,
		            VM_MainListHelper.Instance.AllSeriesDictionary);
	            if (AllSeries == null) return new List<VM_AnimeSeries_User>();
				return
					AllSeries.Select(a => VM_MainListHelper.Instance.AllSeriesDictionary.SureGet(a))
						.Where(a => a != null)
						.OrderBy(a => a.Stat_AirDate_Min).ToList();
	            /*
                List<VM_AnimeSeries_User> series = new List<VM_AnimeSeries_User>();
                try
                {
                    GetAnimeSeriesRecursive(this, ref series);

                    JMMClient.VM_AnimeSeries_User.SortType = JMMClient.VM_AnimeSeries_User.SortMethod.AirDate;
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
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<VM_AnimeSeries_User> AllAnimeSeriesFiltered
        {
            get
            {
                List<VM_AnimeSeries_User> series = new List<VM_AnimeSeries_User>();
                try
                {
                    // check if the current group filter is also applied to series
                    if (VM_MainListHelper.Instance.CurrentGroupFilter != null && VM_MainListHelper.Instance.CurrentGroupFilter.IsApplyToSeries)
                    {
                        foreach (VM_AnimeSeries_User ser in AllAnimeSeries)
                        {
                            if (VM_MainListHelper.Instance.CurrentGroupFilter.EvaluateGroupFilter(ser))
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

        public List<IListWrapper> GetDirectChildren()
        {
            List<IListWrapper> children = new List<IListWrapper>();
            children.AddRange(SubGroups);

            // check if the current group filter is also applied to series
            if (VM_MainListHelper.Instance.CurrentGroupFilter != null && VM_MainListHelper.Instance.CurrentGroupFilter.IsApplyToSeries)
            {
                foreach (VM_AnimeSeries_User ser in AnimeSeries)
                {
                    if (VM_MainListHelper.Instance.CurrentGroupFilter.EvaluateGroupFilter(ser))
                        children.Add(ser);
                }
            }
            else
                children.AddRange(AnimeSeries);

            return children;
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public VM_AnimeGroup_User ParentGroup
        {
            get
            {
                if (!AnimeGroupParentID.HasValue) return null;
                if (VM_MainListHelper.Instance.AllGroupsDictionary.ContainsKey(AnimeGroupParentID.Value))
                    return VM_MainListHelper.Instance.AllGroupsDictionary[AnimeGroupParentID.Value];
                return null;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<VM_AnimeGroup_User> AllSubGroups
        {
            get
            {
                List<VM_AnimeGroup_User> grps = new List<VM_AnimeGroup_User>();
                GetAnimeGroupsRecursive(this, ref grps);
                return grps;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<VM_AnimeGroup_User> SubGroups
        {
            get
            {
                return VM_MainListHelper.Instance.AllGroupsDictionary.Values.Where(
                        a => a.AnimeGroupParentID.HasValue && a.AnimeGroupParentID.Value == AnimeGroupID).ToList();

            }
        }

        private static void GetAnimeGroupsRecursive(VM_AnimeGroup_User grp, ref List<VM_AnimeGroup_User> groupList)
        {
            // get the child groups for this group
            groupList.AddRange(grp.SubGroups);

            foreach (VM_AnimeGroup_User subGroup in grp.SubGroups)
            {
                GetAnimeGroupsRecursive(subGroup, ref groupList);
            }
        }
        /*
        private static void GetAnimeSeriesRecursive(VM_AnimeGroup_User grp, ref List<VM_AnimeSeries_User> seriesList)
        {
            // get the child groups for this group
            seriesList.AddRange(grp.AnimeSeries);

            foreach (VM_AnimeGroup_User subGroup in grp.SubGroups)
            {
                GetAnimeSeriesRecursive(subGroup, ref seriesList);
            }
        }
        */
        public bool Validate()
        {
            if (string.IsNullOrEmpty(GroupName))
            {
                MessageBox.Show(Shoko.Commons.Properties.Resources.Anime_GroupName);
                return false;
            }

            if (string.IsNullOrEmpty(SortName))
            {
                MessageBox.Show(Shoko.Commons.Properties.Resources.Anime_SortName);
                return false;
            }

            return true;
        }


        public bool Save()
        {
            try
            {
                CL_Response<CL_AnimeGroup_User> response=VM_ShokoServer.Instance.ShokoServices.SaveGroup(ToRequest(),
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    MessageBox.Show(response.ErrorMessage);
                    return false;
                }
                Populate((VM_AnimeGroup_User)response.Result);
                VM_MainListHelper.Instance.AllGroupsDictionary[AnimeGroupID] = this;
                PopulateSerieInfo(VM_MainListHelper.Instance.AllGroupsDictionary, VM_MainListHelper.Instance.AllSeriesDictionary);
                return true;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                return false;
            }
        }
    }
}
