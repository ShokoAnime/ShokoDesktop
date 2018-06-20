using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Formatting = Shoko.Commons.Utils.Formatting;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeEpisode_User : CL_AnimeEpisode_User, IListWrapper, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int ObjectType => 4;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsEditable => false;

        public enum SortMethod { EpisodeNumber = 0, AirDate = 1 };
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public static SortMethod SortType { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private string displayTypeLabel = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string DisplayTypeLabel
        {
            get => displayTypeLabel;
            set { this.SetField(()=>displayTypeLabel,value); }
        }

        private int episodeOrder;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int EpisodeOrder
        {
            get => episodeOrder;
            set
            {
                this.SetField(()=>episodeOrder,value);
                SetDisplayDetails();
            }
        }

        private void SetDisplayDetails()
        {
            // Episode Type
            if (EpisodeOrder == 0) DisplayTypeLabel = "Previous Episode";
            else if (EpisodeOrder == 1) DisplayTypeLabel = "Next Episode";
            else DisplayTypeLabel = "";

            // Display Options
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool MultipleUnwatchedEpsSeries => UnwatchedEpCountSeries > 1;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string WatchedDateAsString => WatchedDate?.ToString("dd MMM yyyy - HH:mm", Commons.Culture.Global) ?? "";

        #region Editable members

        private new int WatchedCount
        {
            get => base.WatchedCount;
            set
            {
                this.SetField(()=>base.WatchedCount,(r)=> base.WatchedCount = r, value, ()=>Watched, ()=>IsWatched);
                // episode image / overview in summary
                bool se1 = false;
                bool se2 = false;
                if (VM_UserSettings.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.Always)
                {
                    se1 = true;
                    se2 = true;
                }
                if (!Watched && VM_UserSettings.Instance.HideEpisodeImageWhenUnwatched) se1 = false;
                if (!Watched && VM_UserSettings.Instance.HideEpisodeOverviewWhenUnwatched) se2 = false;
                ShowEpisodeImageInSummary = se1;
                ShowEpisodeOverviewInSummary = se2;

                // episode image / overview in expanded
                se1 = false;
                se2 = false;

                if (VM_UserSettings.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.Always ||
                    VM_UserSettings.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.InExpanded)
                {
                    se1 = true;
                    se2 = true;
                }

                if (!Watched && VM_UserSettings.Instance.HideEpisodeImageWhenUnwatched) se1 = false;
                if (!Watched && VM_UserSettings.Instance.HideEpisodeOverviewWhenUnwatched) se2 = false;

                ShowEpisodeImageInExpanded = se1;
                ShowEpisodeOverviewInExpanded = se2;
                ShowEpisodeImageInDashboard = ShowEpisodeImageInExpanded;
            }
        }
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int IsWatched => WatchedCount > 0 ? 1 : 0;

        public new string AniDB_EnglishName
        {
            get => base.AniDB_EnglishName;
            set
            {
                base.AniDB_EnglishName = value;
                EpisodeName = AniDB_EnglishName.Trim().Length > 0 ? AniDB_EnglishName : AniDB_RomajiName;
            }
        }

        public new DateTime? AniDB_AirDate
        {
            get => base.AniDB_AirDate;
            set
            {
                base.AniDB_AirDate = value;
                AirDateAsString = AniDB_AirDate?.ToString("dd MMM yyyy", Commons.Culture.Global) ?? "";
            }
        }

        public new DateTime? WatchedDate
        {
            get => base.WatchedDate;
            set
            {
                this.SetField(()=>base.WatchedDate,(r)=> base.WatchedDate = r, value);
                SetLastWatchedDescription();
            }
        }

        private string episodeName = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeName
        {
            get => episodeName;
            set
            {
                this.SetField(()=>episodeName,value);
                SetEpisodeNameVariants();
            }
        }

        private string episodeNumberAndName = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeNumberAndName
        {
            get => episodeNumberAndName;
            set { this.SetField(()=>episodeNumberAndName,value); }
        }

        private string episodeNumberAndNameTruncated = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeNumberAndNameTruncated
        {
            get => episodeNumberAndNameTruncated;
            set { this.SetField(()=>episodeNumberAndNameTruncated,value); }
        }

        private string episodeNumberAndNameWithType = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeNumberAndNameWithType
        {
            get => episodeNumberAndNameWithType;
            set { this.SetField(()=>episodeNumberAndNameWithType,value); }
        }

        private string episodeNumberWithType = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeNumberWithType
        {
            get => episodeNumberWithType;
            set { this.SetField(()=>episodeNumberWithType,value); }
        }

        private string episodeNumberAndNameWithTypeTruncated = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeNumberAndNameWithTypeTruncated
        {
            get => episodeNumberAndNameWithTypeTruncated;
            set { this.SetField(()=>episodeNumberAndNameWithTypeTruncated,value); }
        }

        private string episodeTypeAndNumber = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeTypeAndNumber
        {
            get => episodeTypeAndNumber;
            set { this.SetField(()=>episodeTypeAndNumber,value); }
        }

        private string episodeTypeAndNumberAbsolute = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeTypeAndNumberAbsolute
        {
            get => episodeTypeAndNumberAbsolute;
            set { this.SetField(()=>episodeTypeAndNumberAbsolute,value); }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool Watched => WatchedCount > 0;

        private bool tvDBLinkExists;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool TvDBLinkExists
        {
            get => tvDBLinkExists;
            set { this.SetField(()=>tvDBLinkExists,value); }
        }

        private bool tvDBLinkMissing = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool TvDBLinkMissing
        {
            get => tvDBLinkMissing;
            set { this.SetField(()=>tvDBLinkMissing,value); }
        }


        private bool traktLinkExists;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool TraktLinkExists
        {
            get => traktLinkExists;
            set { this.SetField(()=>traktLinkExists,value); }
        }

        private bool traktLinkMissing = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool TraktLinkMissing
        {
            get => traktLinkMissing;
            set { this.SetField(()=>traktLinkMissing,value); }
        }

        private string traktEpisodeURL = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string TraktEpisodeURL
        {
            get => traktEpisodeURL;
            set { this.SetField(()=>traktEpisodeURL,value); }
        }

        public new int LocalFileCount
        {
            get => base.LocalFileCount;
            set { this.SetField(()=>base.LocalFileCount,(r)=> base.LocalFileCount = r, value, ()=>LocalFileCount, ()=>OneFileOnly, ()=>NoFiles, ()=>HasFiles, ()=>FileDetails, ()=>MultipleFiles); }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool OneFileOnly => base.LocalFileCount == 1;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool NoFiles => base.LocalFileCount == 0;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool MultipleFiles => LocalFileCount > 1;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasFiles => base.LocalFileCount > 0;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FileDetails
        {
            get
            {
                if (MultipleFiles)
                {
                    return string.Format("{0} " + Commons.Properties.Resources.Anime_Files, LocalFileCount);
                }
                if (NoFiles && !FutureDated)
                {
                    return string.Format("{0} " + Commons.Properties.Resources.Anime_Files, LocalFileCount);
                }
                return string.Format("{0} " + Commons.Properties.Resources.Anime_File, LocalFileCount);
            }
        }

        private string episodeOverviewLoading = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeOverviewLoading
        {
            get
            {
                if (string.IsNullOrEmpty(episodeOverviewLoading))
                {
                    EpisodeOverviewLoading = (Description?.Length ?? 0) > 0
                        ? Description
                        : Commons.Properties.Resources.AnimeEpisode_NoOverview;
                }
                return episodeOverviewLoading;
            }
            set
            {
                this.SetField(()=>episodeOverviewLoading,value);

                string trunc = EpisodeOverviewLoading;
                if (!string.IsNullOrEmpty(trunc) && EpisodeOverviewLoading.Length > 500)
                    trunc = EpisodeOverviewLoading.Substring(0, 500) + "...";

                EpisodeOverviewTruncated = trunc;
            }
        }

        private string episodeOverviewTruncated = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeOverviewTruncated
        {
            get => episodeOverviewTruncated;
            set
            {
                this.SetField(()=>episodeOverviewTruncated,value);
            }
        }

        private string episodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeImageLoading
        {
            get
            {
                if (string.IsNullOrEmpty(episodeImageLoading))
                    episodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
                return episodeImageLoading;
            }
            set
            {
                this.SetField(()=>episodeImageLoading,value);
            }
        }

        private string airDateAsString = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AirDateAsString
        {
            get => airDateAsString;
            set
            {
                this.SetField(()=>airDateAsString,value);
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDBRatingFormatted => $"{Commons.Properties.Resources.Rating}: {AniDB_Rating} ({AniDB_Votes} {Commons.Properties.Resources.Votes})";

        private bool showEpisodeImageInSummary = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool ShowEpisodeImageInSummary
        {
            get => showEpisodeImageInSummary;
            set
            {
                this.SetField(()=>showEpisodeImageInSummary,value);
            }
        }

        private bool showEpisodeOverviewInSummary = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool ShowEpisodeOverviewInSummary
        {
            get => showEpisodeOverviewInSummary;
            set
            {
                this.SetField(()=>showEpisodeOverviewInSummary,value);
            }
        }


        private bool showEpisodeImageInExpanded = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool ShowEpisodeImageInExpanded
        {
            get => showEpisodeImageInExpanded;
            set
            {
                this.SetField(()=>showEpisodeImageInExpanded,value);
            }
        }

        private bool showEpisodeOverviewInExpanded = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool ShowEpisodeOverviewInExpanded
        {
            get => showEpisodeOverviewInExpanded;
            set
            {
                this.SetField(()=>showEpisodeOverviewInExpanded,value);
            }
        }

        private bool showEpisodeImageInDashboard = true;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool ShowEpisodeImageInDashboard
        {
            get => showEpisodeImageInDashboard;
            set
            {
                this.SetField(()=>showEpisodeImageInDashboard,value);
            }
        }

        private string lastWatchedDescription = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string LastWatchedDescription
        {
            get => lastWatchedDescription;
            set
            {
                this.SetField(()=>lastWatchedDescription,value);
            }
        }

        #endregion

        public void SetLastWatchedDescription()
        {
            if (WatchedDate.HasValue)
            {
                DateTime today = DateTime.Now;
                DateTime yesterday = today.AddDays(-1);

                if (WatchedDate.Value.Day == today.Day && WatchedDate.Value.Month == today.Month && WatchedDate.Value.Year == today.Year)
                {
                    LastWatchedDescription = Commons.Properties.Resources.Today;
                    return;
                }

                if (WatchedDate.Value.Day == yesterday.Day && WatchedDate.Value.Month == yesterday.Month && WatchedDate.Value.Year == yesterday.Year)
                {
                    LastWatchedDescription = Commons.Properties.Resources.Yesterday;
                    return;
                }

                LastWatchedDescription = WatchedDate.Value.ToString("dd MMM yyyy", Commons.Culture.Global);
            }
            else
                LastWatchedDescription = "";
        }

        public void SetTvDBInfo()
        {
            //logger.Trace("SetTvDBInfo: RefreshAnime start: {0} - {1}", this.AnimeSeries.SeriesName, this.EpisodeNumberAndName);
            RefreshAnime();

            VM_TvDBSummary tvSummary = AniDBAnime.TvSummary;

            SetTvDBInfo(tvSummary);
        }


        public void SetTvDBInfo(VM_TvDBSummary tvSummary)
        {
            // TODO Titles and Overviews not stupid.

            #region episode override
            // check if this episode has a direct tvdb over-ride
            if (tvSummary.DictTvDBCrossRefEpisodes.ContainsKey(AniDB_EpisodeID))
            {
                foreach (VM_TvDB_Episode tvep in tvSummary.DictTvDBEpisodes.Values)
                {
                    if (tvSummary.DictTvDBCrossRefEpisodes[AniDB_EpisodeID] != tvep.Id) continue;
                    if(!string.IsNullOrEmpty(tvep.Overview))
                        EpisodeOverviewLoading = tvep.Overview;
                    if (string.IsNullOrEmpty(EpisodeOverviewLoading))
                        EpisodeOverviewLoading = Commons.Properties.Resources.AnimeEpisode_NoOverview;

                    if (string.IsNullOrEmpty(tvep.FullImagePathPlain) || !File.Exists(tvep.FullImagePath))
                    {
                        EpisodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
                        // if there is no proper image to show, we will hide it on the dashboard
                        ShowEpisodeImageInDashboard = false;
                    }
                    else
                        EpisodeImageLoading = tvep.FullImagePath;

                    if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
                        EpisodeName = tvep.EpisodeName;

                    TvDBLinkExists = true;
                    TvDBLinkMissing = false;

                    return;
                }
            }
            #endregion

            //logger.Trace("SetTvDBInfo: normal episodes start");

            #region normal episodes
            // now do stuff to improve performance
            if (EpisodeTypeEnum == Models.Enums.EpisodeType.Episode)
            {
                if (tvSummary.CrossRefTvDBV2 != null && tvSummary.CrossRefTvDBV2.Count > 0)
                {
                    //logger.Trace("SetTvDBInfo: sorting TvDB cross refs: {0} records", tvSummary.CrossRefTvDBV2.Count);

                    // find the xref that is right
                    // relies on the xref's being sorted by season number and then episode number (desc)
                    List<VM_CrossRef_AniDB_TvDBV2> tvDBCrossRef = tvSummary.CrossRefTvDBV2.OrderByDescending(a=>a.AniDBStartEpisodeNumber).ToList();

                    //logger.Trace("SetTvDBInfo: looking for starting points");

                    bool foundStartingPoint = false;
                    VM_CrossRef_AniDB_TvDBV2 xrefBase = null;
                    foreach (VM_CrossRef_AniDB_TvDBV2 xrefTV in tvDBCrossRef)
                    {
                        if (xrefTV.AniDBStartEpisodeType != (int)Models.Enums.EpisodeType.Episode) continue;
                        if (EpisodeNumber < xrefTV.AniDBStartEpisodeNumber) continue;
                        foundStartingPoint = true;
                        xrefBase = xrefTV;
                        break;
                    }

                    //logger.Trace("SetTvDBInfo: looking for starting points - done");

                    // we have found the starting epiosde numbder from AniDB
                    // now let's check that the TvDB Season and Episode Number exist
                    if (foundStartingPoint)
                    {

                        //logger.Trace("SetTvDBInfo: creating dictionary");

                        Dictionary<int, int> dictTvDBSeasons = null;
                        Dictionary<int, VM_TvDB_Episode> dictTvDBEpisodes = null;
                        foreach (VM_TvDBDetails det in tvSummary.TvDetails.Values)
                        {
                            if (det.TvDBID != xrefBase.TvDBID) continue;
                            dictTvDBSeasons = det.DictTvDBSeasons;
                            dictTvDBEpisodes = det.DictTvDBEpisodes;
                            break;
                        }

                        //logger.Trace("SetTvDBInfo: creating dictionary - done");

                        if (dictTvDBSeasons != null && dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber))
                        {
                            int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] + (EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
                            if (dictTvDBEpisodes.ContainsKey(episodeNumber))
                            {

                                //logger.Trace("SetTvDBInfo: loading episode overview");
                                VM_TvDB_Episode tvep = dictTvDBEpisodes[episodeNumber];
                                if(!string.IsNullOrEmpty(tvep.Overview))
                                    EpisodeOverviewLoading = tvep.Overview;
                                if (string.IsNullOrEmpty(EpisodeOverviewLoading))
                                    EpisodeOverviewLoading = Commons.Properties.Resources.AnimeEpisode_NoOverview;

                                //logger.Trace("SetTvDBInfo: loading episode overview - done");

                                if (string.IsNullOrEmpty(tvep.FullImagePathPlain) || !File.Exists(tvep.FullImagePath))
                                {
                                    EpisodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
                                    // if there is no proper image to show, we will hide it on the dashboard
                                    ShowEpisodeImageInDashboard = false;
                                }
                                else
                                    EpisodeImageLoading = tvep.FullImagePath;

                                //logger.Trace("SetTvDBInfo: episode image - done");

                                if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
                                    EpisodeName = tvep.EpisodeName;
                            }
                        }
                    }
                }
            }
            #endregion

            //logger.Trace("SetTvDBInfo: normal episodes finish");

            #region special episodes
            if (EpisodeTypeEnum == Models.Enums.EpisodeType.Special)
            {
                // find the xref that is right
                // relies on the xref's being sorted by season number and then episode number (desc)
                List<VM_CrossRef_AniDB_TvDBV2> tvDBCrossRef = tvSummary.CrossRefTvDBV2?.OrderByDescending(a=>a.AniDBStartEpisodeNumber).ToList() ?? new List<VM_CrossRef_AniDB_TvDBV2>();

                bool foundStartingPoint = false;
                VM_CrossRef_AniDB_TvDBV2 xrefBase = null;
                foreach (VM_CrossRef_AniDB_TvDBV2 xrefTV in tvDBCrossRef)
                {
                    if (xrefTV.AniDBStartEpisodeType != (int)Models.Enums.EpisodeType.Special) continue;
                    if (EpisodeNumber >= xrefTV.AniDBStartEpisodeNumber)
                    {
                        foundStartingPoint = true;
                        xrefBase = xrefTV;
                        break;
                    }
                }

                if (tvSummary.CrossRefTvDBV2 != null && tvSummary.CrossRefTvDBV2.Count > 0)
                {
                    // we have found the starting epiosde numbder from AniDB
                    // now let's check that the TvDB Season and Episode Number exist
                    if (foundStartingPoint)
                    {

                        Dictionary<int, int> dictTvDBSeasons = null;
                        Dictionary<int, VM_TvDB_Episode> dictTvDBEpisodes = null;
                        foreach (VM_TvDBDetails det in tvSummary.TvDetails.Values)
                        {
                            if (det.TvDBID == xrefBase.TvDBID)
                            {
                                dictTvDBSeasons = det.DictTvDBSeasons;
                                dictTvDBEpisodes = det.DictTvDBEpisodes;
                                break;
                            }
                        }

                        if (dictTvDBSeasons != null && dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber))
                        {
                            int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] + (EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
                            if (dictTvDBEpisodes.ContainsKey(episodeNumber))
                            {
                                VM_TvDB_Episode tvep = dictTvDBEpisodes[episodeNumber];
                                if(!string.IsNullOrEmpty(tvep.Overview))
                                    EpisodeOverviewLoading = tvep.Overview;
                                if (string.IsNullOrEmpty(EpisodeOverviewLoading))
                                    EpisodeOverviewLoading = Commons.Properties.Resources.AnimeEpisode_NoOverview;

                                if (string.IsNullOrEmpty(tvep.FullImagePathPlain) || !File.Exists(tvep.FullImagePath))
                                {
                                    EpisodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
                                    // if there is no proper image to show, we will hide it on the dashboard
                                    ShowEpisodeImageInDashboard = false;
                                }
                                else
                                    EpisodeImageLoading = tvep.FullImagePath;

                                if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
                                    EpisodeName = tvep.EpisodeName;
                            }
                        }
                    }
                }
            }
            #endregion

            TvDBLinkExists = false;
            TvDBLinkMissing = true;
            if (EpisodeImageLoading == @"/Images/EpisodeThumb_NotFound.png") ShowEpisodeImageInDashboard = false;

        }

        public int GetTvDBEpisodeID()
        {
            if (AniDBAnime == null) RefreshAnime();

            VM_TvDBSummary tvSummary = AniDBAnime.TvSummary;
            if (tvSummary.DictTvDBCrossRefEpisodes.ContainsKey(AniDB_EpisodeID))
                return tvSummary.DictTvDBCrossRefEpisodes[AniDB_EpisodeID];

            switch (EpisodeTypeEnum)
            {
                case Models.Enums.EpisodeType.Episode:
                    return GetTvDBEpisodeIDByType(tvSummary, Models.Enums.EpisodeType.Episode);
                case Models.Enums.EpisodeType.Special:
                    return GetTvDBEpisodeIDByType(tvSummary, Models.Enums.EpisodeType.Special);
            }

            return 0;
        }

        private int GetTvDBEpisodeIDByType(VM_TvDBSummary tvSummary, EpisodeType type)
        {
            if (tvSummary.CrossRefTvDBV2 == null || tvSummary.CrossRefTvDBV2.Count <= 0) return 0;

            List<VM_CrossRef_AniDB_TvDBV2> tvDBCrossRef = tvSummary.CrossRefTvDBV2
                .OrderByDescending(a => a.AniDBStartEpisodeNumber).ToList();

            bool foundStartingPoint = false;
            VM_CrossRef_AniDB_TvDBV2 xrefBase = null;
            foreach (VM_CrossRef_AniDB_TvDBV2 xrefTV in tvDBCrossRef)
            {
                if (xrefTV.AniDBStartEpisodeType != (int) type) continue;
                if (EpisodeNumber < xrefTV.AniDBStartEpisodeNumber) continue;
                foundStartingPoint = true;
                xrefBase = xrefTV;
                break;
            }

            if (!foundStartingPoint) return 0;

            Dictionary<int, int> dictTvDBSeasons = null;
            Dictionary<int, VM_TvDB_Episode> dictTvDBEpisodes = null;
            foreach (VM_TvDBDetails det in tvSummary.TvDetails.Values)
            {
                if (det.TvDBID != xrefBase.TvDBID) continue;
                dictTvDBSeasons = det.DictTvDBSeasons;
                dictTvDBEpisodes = det.DictTvDBEpisodes;
                break;
            }

            if (dictTvDBSeasons == null || !dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber)) return 0;

            int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] +
                                (EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) -
                                (xrefBase.AniDBStartEpisodeNumber - 1);
            if (!dictTvDBEpisodes.ContainsKey(episodeNumber)) return 0;

            VM_TvDB_Episode tvep = dictTvDBEpisodes[episodeNumber];
            return tvep.Id;
        }


        public void SetTraktInfo()
        {
            //this.RefreshAnime();

            VM_TraktSummary tvSummary = AniDBAnime.traktSummary;

            SetTraktInfo(tvSummary);
        }

        public void SetTraktInfo(VM_TraktSummary traktSummary)
        {
            TraktLinkExists = false;
            TraktLinkMissing = true;

            #region episode override
            // check if this episode has a direct tvdb over-ride
            /*if (traktSummary.DictTraktCrossRefEpisodes.ContainsKey(AniDB_EpisodeID))
            {
                foreach (Trakt_EpisodeVM traktEp in traktSummary.DictTraktEpisodes.Values)
                {
                    if (traktSummary.DictTraktCrossRefEpisodes[AniDB_EpisodeID] == traktEp.Id)
                    {
                        if (string.IsNullOrEmpty(traktEp.Overview))
                            this.EpisodeOverviewLoading = "Episode Overview Not Available";
                        else
                            this.EpisodeOverviewLoading = traktEp.Overview;

                        if (string.IsNullOrEmpty(traktEp.FullImagePathPlain) || !File.Exists(traktEp.FullImagePath))
                        {
                            this.EpisodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
                            // if there is no proper image to show, we will hide it on the dashboard
                            ShowEpisodeImageInDashboard = false;
                        }
                        else
                            this.EpisodeImageLoading = traktEp.FullImagePath;

                        if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(traktEp.EpisodeName))
                            EpisodeName = traktEp.EpisodeName;

                        TvDBLinkExists = true;
                        TvDBLinkMissing = false;

                        return;
                    }
                }
            }*/
            #endregion

            //logger.Trace("SetTvDBInfo: normal episodes start");

            #region normal episodes
            // now do stuff to improve performance
            if (EpisodeTypeEnum == Models.Enums.EpisodeType.Episode)
            {
                if (traktSummary?.CrossRefTraktV2 != null && traktSummary.CrossRefTraktV2.Count > 0)
                {
                    //logger.Trace("SetTvDBInfo: sorting TvDB cross refs: {0} records", tvSummary.CrossRefTvDBV2.Count);

                    // find the xref that is right
                    // relies on the xref's being sorted by season number and then episode number (desc)
                    List<VM_CrossRef_AniDB_TraktV2> traktCrossRefs = traktSummary.CrossRefTraktV2.OrderByDescending(a=>a.AniDBStartEpisodeNumber).ToList();

                    //logger.Trace("SetTvDBInfo: looking for starting points");

                    bool foundStartingPoint = false;
                    VM_CrossRef_AniDB_TraktV2 xrefBase = null;
                    foreach (VM_CrossRef_AniDB_TraktV2 xrefTV in traktCrossRefs)
                    {
                        if (xrefTV.AniDBStartEpisodeType != (int)Models.Enums.EpisodeType.Episode) continue;
                        if (EpisodeNumber >= xrefTV.AniDBStartEpisodeNumber)
                        {
                            foundStartingPoint = true;
                            xrefBase = xrefTV;
                            break;
                        }
                    }

                    //logger.Trace("SetTvDBInfo: looking for starting points - done");

                    // we have found the starting epiosde numbder from AniDB
                    // now let's check that the TvDB Season and Episode Number exist
                    if (foundStartingPoint)
                    {

                        //logger.Trace("SetTvDBInfo: creating dictionary");

                        Dictionary<int, int> dictTraktSeasons = null;
                        Dictionary<int, Trakt_Episode> dictTraktEpisodes = null;
                        foreach (VM_TraktDetails det in traktSummary.traktDetails.Values)
                        {
                            if (det.TraktID.Equals(xrefBase.TraktID, StringComparison.InvariantCultureIgnoreCase))
                            {
                                dictTraktSeasons = det.DictTraktSeasons;
                                dictTraktEpisodes = det.DictTraktEpisodes;
                                break;
                            }
                        }

                        //logger.Trace("SetTvDBInfo: creating dictionary - done");

                        if (dictTraktSeasons != null && dictTraktSeasons.ContainsKey(xrefBase.TraktSeasonNumber))
                        {
                            int episodeNumber = dictTraktSeasons[xrefBase.TraktSeasonNumber] + (EpisodeNumber + xrefBase.TraktStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
                            if (dictTraktEpisodes.ContainsKey(episodeNumber))
                            {

                                //logger.Trace("SetTvDBInfo: loading episode overview");
                                Trakt_Episode traktEp = dictTraktEpisodes[episodeNumber];
                                TraktLinkExists = true;
                                TraktLinkMissing = false;

                                TraktEpisodeURL = traktEp.URL;
                            }
                        }
                    }




                }
            }
            #endregion

            //logger.Trace("SetTvDBInfo: normal episodes finish");

            #region special episodes
            if (EpisodeTypeEnum == Models.Enums.EpisodeType.Special)
            {
                // find the xref that is right
                // relies on the xref's being sorted by season number and then episode number (desc)
                if (traktSummary != null)
                {
                    List<VM_CrossRef_AniDB_TraktV2> traktCrossRef = traktSummary.CrossRefTraktV2?.OrderByDescending(a=>a.AniDBStartEpisodeNumber).ToList() ?? new List<VM_CrossRef_AniDB_TraktV2>();

                    bool foundStartingPoint = false;
                    VM_CrossRef_AniDB_TraktV2 xrefBase = null;
                    foreach (VM_CrossRef_AniDB_TraktV2 xrefTrakt in traktCrossRef)
                    {
                        if (xrefTrakt.AniDBStartEpisodeType != (int)Models.Enums.EpisodeType.Special) continue;
                        if (EpisodeNumber >= xrefTrakt.AniDBStartEpisodeNumber)
                        {
                            foundStartingPoint = true;
                            xrefBase = xrefTrakt;
                            break;
                        }
                    }

                    if (traktSummary.CrossRefTraktV2 != null && traktSummary.CrossRefTraktV2.Count > 0)
                    {
                        // we have found the starting epiosde numbder from AniDB
                        // now let's check that the Trakt Season and Episode Number exist
                        if (foundStartingPoint)
                        {

                            Dictionary<int, int> dictTraktSeasons = null;
                            Dictionary<int, Trakt_Episode> dictTraktEpisodes = null;
                            foreach (VM_TraktDetails det in traktSummary.traktDetails.Values)
                            {
                                if (det.TraktID.Equals(xrefBase.TraktID, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    dictTraktSeasons = det.DictTraktSeasons;
                                    dictTraktEpisodes = det.DictTraktEpisodes;
                                    break;
                                }
                            }

                            if (dictTraktSeasons != null && dictTraktSeasons.ContainsKey(xrefBase.TraktSeasonNumber))
                            {
                                int episodeNumber = dictTraktSeasons[xrefBase.TraktSeasonNumber] + (EpisodeNumber + xrefBase.TraktStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
                                if (dictTraktEpisodes.ContainsKey(episodeNumber))
                                {
                                    Trakt_Episode traktEp = dictTraktEpisodes[episodeNumber];
                                    TraktLinkExists = true;
                                    TraktLinkMissing = false;

                                    TraktEpisodeURL = traktEp.URL;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool FutureDated
        {
            get
            {
                if (!AniDB_AirDate.HasValue) return true;

                return (AniDB_AirDate.Value > DateTime.Now);
            }
        }


        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Episode, AniDB_EpisodeID);

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AnimeName
        {
            get
            {
                //logger.Trace("Getting anime name for ep#: {0}", this.EpisodeNumber);

                string animeName = "";
                if (VM_MainListHelper.Instance.AllSeriesDictionary.ContainsKey(AnimeSeriesID))
                {
                    VM_AnimeSeries_User ser = VM_MainListHelper.Instance.AllSeriesDictionary[AnimeSeriesID];
                    if (ser.AniDBAnime.AniDBAnime != null)
                        animeName = ser.SeriesName;
                }
                else
                    animeName = "NOT FOUND!";

                return animeName;
            }
        }




        private VM_AniDB_Anime aniDB_Anime;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public VM_AniDB_Anime AniDBAnime => aniDB_Anime;

        public void RefreshAnime()
        {
            RefreshAnime(false);
        }

        public void RefreshAnime(bool forced)
        {
            if (VM_MainListHelper.Instance.AllSeriesDictionary.ContainsKey(AnimeSeriesID))
            {
                VM_AnimeSeries_User ser = VM_MainListHelper.Instance.AllSeriesDictionary[AnimeSeriesID];
                aniDB_Anime = ser.AniDBAnime.AniDBAnime;
            }

            if (forced && aniDB_Anime != null)
                VM_MainListHelper.Instance.UpdateAnime(aniDB_Anime.AnimeID);

            if (forced && aniDB_Anime == null)
            {
                VM_MainListHelper.Instance.UpdateAnime(AnimeSeries.AniDBAnime.AniDBAnime.AnimeID);
                if (AnimeSeries?.AniDBAnime.AniDBAnime != null)
                    aniDB_Anime = AnimeSeries.AniDBAnime.AniDBAnime;
            }
        }

        private VM_AnimeSeries_User animeSeries;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public VM_AnimeSeries_User AnimeSeries
        {
            get
            {
                if (animeSeries != null) return animeSeries;
                VM_AnimeSeries_User rawSeries;
                if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(AnimeSeriesID, out rawSeries) == false)
                {
                    // get the series
                    rawSeries = (VM_AnimeSeries_User)VM_ShokoServer.Instance.ShokoServices.GetSeries(AnimeSeriesID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (rawSeries != null)
                    {
                        VM_MainListHelper.Instance.AllSeriesDictionary[AnimeSeriesID] = rawSeries;
                    }
                }
                if (rawSeries == null) return null;
                animeSeries = rawSeries;
                return animeSeries;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string RunTime => Formatting.FormatSecondsToDisplayTime(AniDB_LengthSeconds);

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public EpisodeType EpisodeTypeEnum => (EpisodeType)EpisodeType;




        private void SetEpisodeNameVariants()
        {
            EpisodeNumberAndName = $"{EpisodeNumber} - {EpisodeName}";
            string shortType = "";
            switch (EpisodeTypeEnum)
            {
                case Models.Enums.EpisodeType.Credits: shortType = "C"; break;
                case Models.Enums.EpisodeType.Episode: shortType = ""; break;
                case Models.Enums.EpisodeType.Other: shortType = "O"; break;
                case Models.Enums.EpisodeType.Parody: shortType = "P"; break;
                case Models.Enums.EpisodeType.Special: shortType = "S"; break;
                case Models.Enums.EpisodeType.Trailer: shortType = "T"; break;
            }
            EpisodeNumberAndNameWithType = $"{shortType}{EpisodeNumber} - {EpisodeName}";
            EpisodeNumberWithType = $"{EpisodeTypeEnum.ToString()} {EpisodeNumber}";
            EpisodeTypeAndNumber = $"{shortType}{EpisodeNumber}";
            EpisodeTypeAndNumberAbsolute = $"{shortType}{EpisodeNumber.ToString().PadLeft(5, '0')}";

            EpisodeNumberAndNameTruncated = EpisodeNumberAndName;
            if (EpisodeNumberAndName.Length > 60)
                EpisodeNumberAndNameTruncated = EpisodeNumberAndName.Substring(0, 60) + "...";

            EpisodeNumberAndNameWithTypeTruncated = EpisodeNumberAndNameWithType;
            if (EpisodeNumberAndNameWithTypeTruncated.Length > 60)
                EpisodeNumberAndNameWithTypeTruncated = EpisodeNumberAndNameWithType.Substring(0, 60) + "...";
        }

        public void Populate(CL_AnimeEpisode_User contract)
        {
            try
            {
                AniDB_EpisodeID = contract.AniDB_EpisodeID;
                AnimeEpisodeID = contract.AnimeEpisodeID;
                AnimeSeriesID = contract.AnimeSeriesID;
                DateTimeUpdated = contract.DateTimeUpdated;
                EpisodeNumber = contract.EpisodeNumber;
                EpisodeType = contract.EpisodeType;
                UnwatchedEpCountSeries = contract.UnwatchedEpCountSeries;
                AniDB_LengthSeconds = contract.AniDB_LengthSeconds;
                AniDB_Rating = contract.AniDB_Rating;
                AniDB_Votes = contract.AniDB_Votes;
                AniDB_RomajiName = contract.AniDB_RomajiName;
                AniDB_EnglishName = contract.AniDB_EnglishName;
                AniDB_AirDate = contract.AniDB_AirDate;
                Description = contract.Description;
                LocalFileCount = contract.LocalFileCount;
                PlayedCount = contract.PlayedCount;
                StoppedCount = contract.StoppedCount;
                WatchedCount = contract.WatchedCount;
                WatchedDate = contract.WatchedDate;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int EpisodeStatus
        {
            get
            {
                // 1 = No files found
                // 2 = Watched
                // 3 = Unwatched
                if (LocalFileCount == 0) return 1;

                if (IsWatched == 1) return 2;

                return 3;
            }
        }



        public List<VM_VideoLocal> GetAllVideoLocals()
        {
            try
            {

                return VM_ShokoServer.Instance.ShokoServices.GetVideoLocalsForEpisode(AnimeEpisodeID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).Cast<VM_VideoLocal>().OrderBy(a=>a.FileName).ToList();

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            return new List<VM_VideoLocal>();
        }

        public void RefreshFilesForEpisode()
        {
            try
            {

                filesForEpisode=VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(AnimeEpisodeID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).Cast<VM_VideoDetailed>().OrderByDescending(a => a.GetOverallVideoSourceRanking()).ToList();

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private List<VM_VideoDetailed> filesForEpisode;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<VM_VideoDetailed> FilesForEpisode
        {
            get
            {
                if (filesForEpisode == null)
                {
                    RefreshFilesForEpisode();
                }

                return filesForEpisode;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public List<CL_AniDB_GroupStatus> ReleaseGroups
        {
            get
            {
                List<CL_AniDB_GroupStatus> relgrps = new List<CL_AniDB_GroupStatus>();

                try
                {
                    List<CL_AniDB_GroupStatus> contracts = VM_ShokoServer.Instance.ShokoServices.GetMyReleaseGroupsForAniDBEpisode(AniDB_EpisodeID);

                    foreach (CL_AniDB_GroupStatus rg in contracts)
                    {
                        relgrps.Add(rg);
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return relgrps;
            }
        }

        public List<IListWrapper> GetDirectChildren()
        {
            List<IListWrapper> childFiles = new List<IListWrapper>();
            List<VM_VideoDetailed> allFiles = FilesForEpisode;

            // check settings to see if we need to hide episodes
            childFiles.AddRange(allFiles);
            return childFiles;
        }

        public List<string> ToSearchParameters()
        {
            List<string> parms = new List<string>();
            if (AniDBAnime == null) RefreshAnime();
            VM_AniDB_Anime anime = AniDBAnime;
            if (anime == null) return parms;

            // only use the first 2 words of the anime's title
            string[] titles = anime.MainTitle.Split(' ');
            int i = 0;
            foreach (string s in titles)
            {
                i++;
                parms.Add(s.Trim());
                if (i == 2) break;
            }
            parms.Add(EpisodeNumber.ToString().PadLeft(2, '0'));
            return parms;
        }
    }
}
