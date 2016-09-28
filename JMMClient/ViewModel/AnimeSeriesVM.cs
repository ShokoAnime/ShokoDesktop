using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace JMMClient
{
    public class AnimeSeriesVM : MainListWrapper, INotifyPropertyChanged, IComparable<AnimeSeriesVM>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Readonly members

        public int? AnimeSeriesID { get; set; }
        //public int AnimeGroupID { get; set; }
        public int AniDB_ID { get; set; }
        //public int UnwatchedEpisodeCount { get; set; }
        public DateTime DateTimeUpdated { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public int WatchedEpisodeCount { get; set; }
        public string DefaultAudioLanguage { get; set; }
        public string DefaultSubtitleLanguage { get; set; }


        public int PlayedCount { get; set; }
        //public int WatchedCount { get; set; }
        public int StoppedCount { get; set; }
        public int LatestLocalEpisodeNumber { get; set; }

        #endregion

        public DateTime? Stat_EndDate { get; set; }
        public decimal? Stat_UserVotePermanent { get; set; }
        public decimal? Stat_UserVoteTemporary { get; set; }

        public string Stat_AllTitles { get; set; }
        public bool Stat_IsComplete { get; set; }
        public bool Stat_HasFinishedAiring { get; set; }
        public string Stat_AllVideoQuality { get; set; }
        public string Stat_AllVideoQualityEpisodes { get; set; }
        public string Stat_AudioLanguages { get; set; }
        public string Stat_SubtitleLanguages { get; set; }
        public bool Stat_HasTvDBLink { get; set; }
        public bool Stat_HasMovieDBLink { get; set; }

        #region Sorting properties

        // These properties are used when sorting group filters, and must match the names on the AnimeGroupVM

        public decimal AniDBRating
        {
            get
            {
                try
                {
                    return AniDB_Anime.AniDBRating;

                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public DateTime? Stat_AirDate_Min
        {
            get
            {
                try
                {
                    return AniDB_Anime.AirDate;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public DateTime? Stat_AirDate_Max
        {
            get
            {
                try
                {
                    return AniDB_Anime.AirDate;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public DateTime? EpisodeAddedDate { get; set; }
        public DateTime? WatchedDate { get; set; }

        public string SortName
        {
            get
            {
                return SeriesName;
            }
        }

        public string DateTimeCreatedAsString
        {
            get
            {
                return DateTimeCreated.ToString("dd MMM yyyy - HH:mm", Globals.Culture);
            }
        }

        private int missingEpisodeCount = 0;
        public int MissingEpisodeCount
        {
            get { return missingEpisodeCount; }
            set
            {
                missingEpisodeCount = value;
                NotifyPropertyChanged("MissingEpisodeCount");
            }
        }

        public DateTime? Stat_SeriesCreatedDate
        {
            get
            {
                return DateTimeCreated;
            }
        }

        public decimal? Stat_UserVoteOverall
        {
            get
            {
                return AniDB_Anime.Detail.UserRating;
            }
        }

        public int AllSeriesCount
        {
            get
            {
                return 1;
            }
        }

        private int unwatchedEpisodeCount = 0;
        public int UnwatchedEpisodeCount
        {
            get { return unwatchedEpisodeCount; }
            set
            {
                unwatchedEpisodeCount = value;
                NotifyPropertyChanged("UnwatchedEpisodeCount");
            }
        }

        #endregion

        public AniDB_AnimeVM AniDB_Anime { get; set; }
        public List<CrossRef_AniDB_TvDBVMV2> CrossRef_AniDB_TvDBV2 { get; set; }
        public CrossRef_AniDB_OtherVM CrossRef_AniDB_MovieDB { get; set; }
        public List<CrossRef_AniDB_MALVM> CrossRef_AniDB_MAL { get; set; }
        public List<TvDB_SeriesVM> TvDBSeriesV2 { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        #region Editable members

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

        private Boolean isSeriesNameOverridden = false;
        public Boolean IsSeriesNameOverridden
        {
            get { return isSeriesNameOverridden; }
            set
            {
                isSeriesNameOverridden = value;
                NotifyPropertyChanged("IsSeriesNameOverridden");

                SetSeriesNames();
            }
        }

        private Boolean isSeriesNameNotOverridden = false;
        public Boolean IsSeriesNameNotOverridden
        {
            get { return isSeriesNameNotOverridden; }
            set
            {
                isSeriesNameNotOverridden = value;
                NotifyPropertyChanged("IsSeriesNameNotOverridden");
            }
        }

        private string seriesNameOverride = "";
        public string SeriesNameOverride
        {
            get { return seriesNameOverride; }
            set
            {
                seriesNameOverride = value;
                NotifyPropertyChanged("SeriesNameOverride");
            }
        }

        private int animeGroupID = 0;
        public int AnimeGroupID
        {
            get { return animeGroupID; }
            set
            {
                animeGroupID = value;
                NotifyPropertyChanged("AnimeGroupID");
            }
        }

        private int watchedCount = 0;
        public int WatchedCount
        {
            get { return watchedCount; }
            set
            {
                watchedCount = value;
                NotifyPropertyChanged("WatchedCount");
            }
        }

        private Boolean isFave = false;
        public Boolean IsFave
        {
            get { return isFave; }
            set
            {
                isFave = value;
                NotifyPropertyChanged("IsFave");
            }
        }

        private Boolean isNotFave = true;
        public Boolean IsNotFave
        {
            get { return isNotFave; }
            set
            {
                isNotFave = value;
                NotifyPropertyChanged("IsNotFave");
            }
        }

        private bool hasMissingEpisodesAny = false;
        public bool HasMissingEpisodesAny
        {
            get { return hasMissingEpisodesAny; }
            set
            {
                hasMissingEpisodesAny = value;
                NotifyPropertyChanged("HasMissingEpisodesAny");
            }
        }

        private bool hasMissingEpisodesAllDifferentToGroups = false;
        public bool HasMissingEpisodesAllDifferentToGroups
        {
            get { return hasMissingEpisodesAllDifferentToGroups; }
            set
            {
                hasMissingEpisodesAllDifferentToGroups = value;
                NotifyPropertyChanged("HasMissingEpisodesAllDifferentToGroups");
            }
        }

        private bool hasMissingEpisodesGroups = false;
        public bool HasMissingEpisodesGroups
        {
            get { return hasMissingEpisodesGroups; }
            set
            {
                hasMissingEpisodesGroups = value;
                NotifyPropertyChanged("HasMissingEpisodesGroups");
            }
        }



        private int missingEpisodeCountGroups = 0;
        public int MissingEpisodeCountGroups
        {
            get { return missingEpisodeCountGroups; }
            set
            {
                missingEpisodeCountGroups = value;
                NotifyPropertyChanged("MissingEpisodeCountGroups");
            }
        }

        private string posterPath = "";
        public string PosterPath
        {
            get { return AniDB_Anime.DefaultPosterPath; }
            set
            {
                posterPath = value;
                NotifyPropertyChanged("PosterPath");
            }
        }

        private string seriesName = "";
        public string SeriesName
        {
            get { return seriesName; }
            set
            {
                seriesName = value;
                NotifyPropertyChanged("SeriesName");
            }
        }

        private string seriesNameTruncated = "";
        public string SeriesNameTruncated
        {
            get { return seriesNameTruncated; }
            set
            {
                seriesNameTruncated = value;
                NotifyPropertyChanged("SeriesNameTruncated");
            }
        }

        private string groupName = "";
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                NotifyPropertyChanged("GroupName");
            }
        }


        private string defaultFolder = "";
        public string DefaultFolder
        {
            get { return defaultFolder; }
            set
            {
                defaultFolder = value;
                NotifyPropertyChanged("DefaultFolder");
            }
        }

        public void SetSeriesNames()
        {
            if (!string.IsNullOrEmpty(SeriesNameOverride))
                SeriesName = SeriesNameOverride;
            else
            {
                if (JMMServerVM.Instance.SeriesNameSource == DataSourceType.AniDB)
                    SeriesName = AniDB_Anime.FormattedTitle;
                else
                {

                    if (TvDBSeriesV2 != null && TvDBSeriesV2.Count > 0 && !string.IsNullOrEmpty(TvDBSeriesV2[0].SeriesName) &&
                        !TvDBSeriesV2[0].SeriesName.ToUpper().Contains("**DUPLICATE"))
                        SeriesName = TvDBSeriesV2[0].SeriesName;
                    else
                        SeriesName = AniDB_Anime.FormattedTitle;
                }
            }

            string ret = SeriesName;
            if (ret.Length > 30)
                ret = ret.Substring(0, 28) + "...";

            SeriesNameTruncated = ret;
        }

        // Try to find correct group name, forked over from MyAnime3
        public void SetGroupNames(JMMServerBinary.Contract_AnimeSeries contract)
        {
            try
            {
                // Check for group name
                if (Heirarchy.Count == 0)
                {
                    AnimeGroupVM grp = MainListHelperVM.Instance.AllGroupsDictionary[contract.AnimeGroupID];
                    if (grp != null)
                    {
                        GroupName = grp.GroupName;
                    }
                    else
                    {
                        GroupName = SeriesName;
                    }
                }
                else
                {
                    GroupName = Heirarchy.FirstOrDefault(g => g != null).GroupName;
                }

                // If failed to set revert to using series name
                if (string.IsNullOrEmpty(GroupName.Trim()))
                {
                    GroupName = SeriesName;
                }
            }
            catch (Exception)
            {
                GroupName = SeriesName;
            }
        }

        #endregion

        public enum SortMethod { SortName = 0, AirDate = 1 };
        public static SortMethod SortType { get; set; }

        public bool IsComplete
        {
            get
            {
                if (!AniDB_Anime.EndDate.HasValue) return false; // ongoing

                // all series have finished airing and the user has all the episodes
                if (AniDB_Anime.EndDate.Value < DateTime.Now && !HasMissingEpisodesAny) return true;

                return false;
            }
        }

        public bool FinishedAiring
        {
            get
            {
                if (!AniDB_Anime.EndDate.HasValue) return false; // ongoing

                // all series have finished airing
                if (AniDB_Anime.EndDate.Value < DateTime.Now) return true;

                return false;
            }
        }

        public bool UserHasVotedPerm
        {
            get
            {
                if (AniDB_Anime.Detail == null || AniDB_Anime.Detail.UserVote == null) return false;

                if (AniDB_Anime.Detail.UserVote.VoteType != (int)AniDBVoteType.Anime) return false;

                return true;
            }
        }

        public bool UserHasVotedAny
        {
            get
            {
                if (AniDB_Anime.Detail == null || AniDB_Anime.Detail.UserVote == null) return false;

                return true;
            }
        }





        public HashSet<string> AllTags => AniDB_Anime.AllTags;

        public HashSet<string> CustomTags => new HashSet<string>(AniDB_Anime.Detail.CustomTags.Select(a => a.TagName));

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




        public string Description
        {
            get
            {
                if (JMMServerVM.Instance.SeriesDescriptionSource == DataSourceType.AniDB)
                    return AniDB_Anime.Description;

                if (TvDBSeriesV2 != null && TvDBSeriesV2.Count > 0 && !string.IsNullOrEmpty(TvDBSeriesV2[0].Overview))
                    return TvDBSeriesV2[0].Overview;
                else
                    return AniDB_Anime.Description;
            }
        }

        public string DescriptionTruncated
        {
            get
            {
                string trunc = Description;
                if (!string.IsNullOrEmpty(trunc) && trunc.Length > 500)
                    trunc = trunc.Substring(0, 500) + "...";

                return trunc;
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
                        return Properties.Resources.Today;

                    if (WatchedDate.Value.Day == yesterday.Day && WatchedDate.Value.Month == yesterday.Month && WatchedDate.Value.Year == yesterday.Year)
                        return Properties.Resources.Yesterday;

                    return WatchedDate.Value.ToString("dd MMM yyyy", Globals.Culture);
                }
                else
                    return "";
            }
        }


        public string EpisodeCountFormatted
        {
            get
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                // Multiple Episodes
                if (AniDB_Anime.EpisodeCountNormal > 1)
                {
                    // Multiple Episodes, Multiple Specials
                    if (AniDB_Anime.EpisodeCountSpecial > 1)
                    {
                        return string.Format("{0} {1} ({2} {3})", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episodes, AniDB_Anime.EpisodeCountSpecial, Properties.Resources.Anime_Specials);
                    }
                    else
                    {
                        // Multiple Episodes, No Specials
                        if (AniDB_Anime.EpisodeCountSpecial <= 0)
                        {
                            return string.Format("{0} {1} ({2} {3})", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episodes, AniDB_Anime.EpisodeCountSpecial, Properties.Resources.Anime_Specials);
                        }
                        else
                            return string.Format("{0} {1} ({2} {3})", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episodes, AniDB_Anime.EpisodeCountSpecial, Properties.Resources.Anime_Special);
                    }
                }
                else
                {
                    // Single Episode, Multiple Specials
                    if (AniDB_Anime.EpisodeCountSpecial > 1)
                    {
                        return string.Format("{0} {1} ({2} {3})", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episode, AniDB_Anime.EpisodeCountSpecial, Properties.Resources.Anime_Specials);
                    }
                    else
                    {
                        // Single Episodes, No Specials
                        if (AniDB_Anime.EpisodeCountSpecial <= 0)
                        {
                            return string.Format("{0} {1} ({2} {3})", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episode, AniDB_Anime.EpisodeCountSpecial, Properties.Resources.Anime_Specials);
                        }
                        else
                            return string.Format("{0} {1} ({2} {3})", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episode, AniDB_Anime.EpisodeCountSpecial, Properties.Resources.Anime_Special);
                    }
                }
            }
        }

        public string EpisodeCountFormattedShort
        {
            get
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                if (AniDB_Anime.EpisodeCountNormal > 1)
                {
                    return string.Format("{0} {1}", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episodes);
                }
                else
                {
                    return string.Format("{0} {1}", AniDB_Anime.EpisodeCountNormal, Properties.Resources.Anime_Episode);
                }

            }
        }

        public string NamesSummary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(SeriesName);

                return sb.ToString();
            }
        }



        private List<AnimeEpisodeVM> allEpisodes;
        public List<AnimeEpisodeVM> AllEpisodes
        {
            get
            {
                if (allEpisodes == null)
                {
                    RefreshEpisodes();
                }
                return allEpisodes;
            }
        }

        public List<VideoLocalVM> AllVideoLocals
        {
            get
            {
                List<VideoLocalVM> vids = new List<VideoLocalVM>();
                try
                {
                    DateTime start = DateTime.Now;
                    List<JMMServerBinary.Contract_VideoLocal> raws = JMMServerVM.Instance.clientBinaryHTTP.GetVideoLocalsForAnime(AniDB_ID,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                    TimeSpan ts = DateTime.Now - start;
                    logger.Trace("Got vids for series from service: {0} in {1} ms", AniDB_ID, ts.TotalMilliseconds);

                    foreach (JMMServerBinary.Contract_VideoLocal raw in raws)
                    {
                        VideoLocalVM vid = new VideoLocalVM(raw);
                        vids.Add(vid);
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return vids;
            }
        }


        public void RefreshEpisodes()
        {
            allEpisodes = new List<AnimeEpisodeVM>();

            try
            {
                DateTime start = DateTime.Now;
                List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForSeries(AnimeSeriesID.Value,
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                TimeSpan ts = DateTime.Now - start;
                logger.Info("Got episode data from service: {0} in {1} ms", AniDB_Anime.FormattedTitle, ts.TotalMilliseconds);

                start = DateTime.Now;

                TvDBSummary tvSummary = AniDB_Anime.TvSummary;
                // Normal episodes
                List<AnimeEpisodeVM> specials = new List<AnimeEpisodeVM>();
                foreach (JMMServerBinary.Contract_AnimeEpisode ep in eps)
                {
                    AnimeEpisodeVM epvm = new AnimeEpisodeVM(ep);
                    epvm.SetTvDBInfo(tvSummary);
                    allEpisodes.Add(epvm);
                }

                ts = DateTime.Now - start;
                logger.Info("Got episode contracts: {0} in {1} ms", AniDB_Anime.FormattedTitle, ts.TotalMilliseconds);

                start = DateTime.Now;
                allEpisodes.Sort();
                ts = DateTime.Now - start;
                logger.Info("Sorted episode contracts: {0} in {1} ms", AniDB_Anime.FormattedTitle, ts.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public int LatestRegularEpisodeNumber
        {
            get
            {
                int latestEpNo = 0;

                try
                {
                    List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForSeries(AnimeSeriesID.Value,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                    allEpisodes = new List<AnimeEpisodeVM>();

                    foreach (JMMServerBinary.Contract_AnimeEpisode ep in eps)
                    {
                        if ((EpisodeType)ep.EpisodeType == EpisodeType.Episode)
                        {
                            if (ep.EpisodeNumber > latestEpNo) latestEpNo = ep.EpisodeNumber;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return latestEpNo;
            }
        }


        public List<AnimeEpisodeTypeVM> EpisodeTypes
        {
            get
            {
                List<AnimeEpisodeTypeVM> epTypes = new List<AnimeEpisodeTypeVM>();

                try
                {
                    foreach (AnimeEpisodeVM ep in AllEpisodes)
                    {
                        AnimeEpisodeTypeVM epType = new AnimeEpisodeTypeVM(this, ep);

                        bool alreadyAdded = false;
                        foreach (AnimeEpisodeTypeVM thisEpType in epTypes)
                        {
                            if (thisEpType.EpisodeType == epType.EpisodeType)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        }
                        if (!alreadyAdded)
                            epTypes.Add(epType);
                    }

                    AnimeEpisodeTypeVM.SortType = AnimeEpisodeTypeVM.SortMethod.EpisodeType;
                    epTypes.Sort();
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return epTypes;
            }
        }

        public AnimeSeriesVM()
        {
            CrossRef_AniDB_TvDBV2 = new List<CrossRef_AniDB_TvDBVMV2>();
            TvDBSeriesV2 = new List<TvDB_SeriesVM>();
        }

        public void Populate(JMMServerBinary.Contract_AnimeSeries contract)
        {
            //TODO we can get detailed in here
            AniDB_Anime = new AniDB_AnimeVM(contract.AniDBAnime.AniDBAnime);
            NotifyPropertyChanged("AniDB_Anime");

            MainListHelperVM.Instance.AllAnimeDictionary[AniDB_Anime.AnimeID] = AniDB_Anime;

            CrossRef_AniDB_TvDBV2.Clear();
            TvDBSeriesV2.Clear();

            if (contract.CrossRefAniDBTvDBV2 != null)
            {
                foreach (JMMClient.JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2 contractTV in contract.CrossRefAniDBTvDBV2)
                    CrossRef_AniDB_TvDBV2.Add(new CrossRef_AniDB_TvDBVMV2(contractTV));
            }

            if (contract.TvDB_Series != null)
            {
                foreach (JMMClient.JMMServerBinary.Contract_TvDB_Series contractSer in contract.TvDB_Series)
                    TvDBSeriesV2.Add(new TvDB_SeriesVM(contractSer));
            }

            if (contract.CrossRefAniDBMovieDB != null)
                CrossRef_AniDB_MovieDB = new CrossRef_AniDB_OtherVM(contract.CrossRefAniDBMovieDB);
            else
                CrossRef_AniDB_MovieDB = null;

            if (contract.CrossRefAniDBMAL != null)
            {
                CrossRef_AniDB_MAL = new List<CrossRef_AniDB_MALVM>();
                foreach (JMMServerBinary.Contract_CrossRef_AniDB_MAL contractTemp in contract.CrossRefAniDBMAL)
                    CrossRef_AniDB_MAL.Add(new CrossRef_AniDB_MALVM(contractTemp));
            }
            else
                CrossRef_AniDB_MAL = null;


            this.AniDB_ID = contract.AniDB_ID;
            this.AnimeGroupID = contract.AnimeGroupID;
            this.AnimeSeriesID = contract.AnimeSeriesID;
            this.DateTimeUpdated = contract.DateTimeUpdated;
            this.DateTimeCreated = contract.DateTimeCreated;
            this.DefaultAudioLanguage = contract.DefaultAudioLanguage;
            this.DefaultSubtitleLanguage = contract.DefaultSubtitleLanguage;
            this.SeriesNameOverride = contract.SeriesNameOverride;
            this.DefaultFolder = contract.DefaultFolder;

            IsSeriesNameOverridden = !string.IsNullOrEmpty(SeriesNameOverride);
            IsSeriesNameNotOverridden = string.IsNullOrEmpty(SeriesNameOverride);

            this.LatestLocalEpisodeNumber = contract.LatestLocalEpisodeNumber;
            this.PlayedCount = contract.PlayedCount;
            this.StoppedCount = contract.StoppedCount;
            this.UnwatchedEpisodeCount = contract.UnwatchedEpisodeCount;
            this.WatchedCount = contract.WatchedCount;
            this.WatchedDate = contract.WatchedDate;
            this.EpisodeAddedDate = contract.EpisodeAddedDate;
            this.WatchedEpisodeCount = contract.WatchedEpisodeCount;

            this.MissingEpisodeCount = contract.MissingEpisodeCount;
            this.MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;

            HasMissingEpisodesAny = (MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0);
            HasMissingEpisodesAllDifferentToGroups = (MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups);
            HasMissingEpisodesGroups = MissingEpisodeCountGroups > 0;

            //PosterPath = AniDB_Anime.DefaultPosterPath;

            SetSeriesNames();
            SetGroupNames(contract);
        }

        public AnimeSeriesVM(JMMServerBinary.Contract_AnimeSeries contract)
        {
            CrossRef_AniDB_TvDBV2 = new List<CrossRef_AniDB_TvDBVMV2>();
            TvDBSeriesV2 = new List<TvDB_SeriesVM>();

            Populate(contract);
        }

        public void RefreshBase()
        {
            JMMServerBinary.Contract_AnimeSeries contract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(AnimeSeriesID.Value,
                JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
            Populate(contract);
            allEpisodes = null;
        }


        public int CompareTo(AnimeSeriesVM obj)
        {
            switch (SortType)
            {
                case SortMethod.SortName:
                    return SortName.CompareTo(obj.SortName);

                case SortMethod.AirDate:
                    if (AniDB_Anime.AirDate.HasValue && obj.AniDB_Anime.AirDate.HasValue)
                        return AniDB_Anime.AirDate.Value.CompareTo(obj.AniDB_Anime.AirDate.Value);
                    else
                        return 0;

                default:
                    return SortName.CompareTo(obj.SortName);
            }

        }

        public bool Save()
        {
            try
            {
                JMMServerBinary.Contract_AnimeSeries_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.SaveSeries(this.ToContract(),
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    MessageBox.Show(response.ErrorMessage);
                    return false;
                }
                else
                {
                    this.Populate(response.AnimeSeries);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                return false;
            }
        }

        public JMMServerBinary.Contract_AnimeSeries_Save ToContract()
        {
            JMMServerBinary.Contract_AnimeSeries_Save contract = new JMMServerBinary.Contract_AnimeSeries_Save();
            contract.AniDB_ID = this.AniDB_ID;
            contract.AnimeGroupID = this.AnimeGroupID;
            contract.AnimeSeriesID = this.AnimeSeriesID;
            contract.DefaultAudioLanguage = this.DefaultAudioLanguage;
            contract.DefaultSubtitleLanguage = this.DefaultSubtitleLanguage;
            contract.SeriesNameOverride = this.SeriesNameOverride;
            contract.DefaultFolder = this.DefaultFolder;

            return contract;
        }

        /*public override List<MainListWrapper> GetDirectChildren()
		{
			List<MainListWrapper> eps = new List<MainListWrapper>();
			List<AnimeEpisodeVM> allEps = AllEpisodes;

			// check settings to see if we need to hide episodes

			AnimeEpisodeVM.SortType = AnimeEpisodeVM.SortMethod.EpisodeNumber;
			allEps.Sort();
			eps.AddRange(allEps);
			return eps;
		}*/

        public AnimeGroupVM TopLevelAnimeGroup
        {
            get
            {
                try
                {
                    JMMServerBinary.Contract_AnimeGroup contract = JMMServerVM.Instance.clientBinaryHTTP.GetTopLevelGroupForSeries(
                    this.AnimeSeriesID.Value, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                    if (contract == null) return null;
                    AnimeGroupVM grp = new AnimeGroupVM(contract);
                    return grp;
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                    return null;
                }
            }

        }

        public void PopulateIsFave()
        {
            IsFave = false;
            IsNotFave = true;

            AnimeGroupVM grp = TopLevelAnimeGroup;
            if (grp != null)
            {
                IsFave = grp.BIsFave;
                IsNotFave = grp.BIsNotFave;
            }
        }

        public override List<MainListWrapper> GetDirectChildren()
        {
            List<MainListWrapper> eps = new List<MainListWrapper>();

            try
            {
                List<AnimeEpisodeTypeVM> allEpTypes = EpisodeTypes;

                // check settings to see if we need to hide episodes

                AnimeEpisodeTypeVM.SortType = AnimeEpisodeTypeVM.SortMethod.EpisodeType;
                allEpTypes.Sort();
                eps.AddRange(allEpTypes);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            return eps;
        }

        public List<AnimeGroupVM> Heirarchy
        {
            get
            {
                if (MainListHelperVM.Instance.AllGroupsDictionary.Count == 0)
                {
                    MainListHelperVM.Instance.UpdateAnimeGroups();
                }

                List<AnimeGroupVM> groups = new List<AnimeGroupVM>();

                if (MainListHelperVM.Instance.AllGroupsDictionary.ContainsKey(this.AnimeGroupID))
                {
                    AnimeGroupVM thisGrp = MainListHelperVM.Instance.AllGroupsDictionary[this.AnimeGroupID];
                    groups.Add(thisGrp);
                    while (thisGrp.AnimeGroupParentID.HasValue)
                    {
                        if (MainListHelperVM.Instance.AllGroupsDictionary.ContainsKey(thisGrp.AnimeGroupParentID.Value))
                        {
                            thisGrp = MainListHelperVM.Instance.AllGroupsDictionary[thisGrp.AnimeGroupParentID.Value];
                            groups.Add(thisGrp);
                        }
                        else
                            return groups;
                    }
                }

                return groups;
            }
        }
    }
}