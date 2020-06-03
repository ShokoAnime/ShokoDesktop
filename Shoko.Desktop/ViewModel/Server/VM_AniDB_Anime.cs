﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

using System.Xml.Serialization;
using Newtonsoft.Json;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models;
using Shoko.Models.Client;
using Formatting = Shoko.Commons.Utils.Formatting;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Anime : CL_AniDB_Anime, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Random fanartRandom = new Random();
        private static readonly Random posterRandom = new Random();

        [JsonIgnore, XmlIgnore]
        public string DescriptionFormatted
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                    return Shoko.Commons.Properties.Resources.Recommendation_NoOverview;
                return string.Intern(Description);
            }
        }


        [JsonIgnore, XmlIgnore]
        public new VM_AniDB_Anime_DefaultImage DefaultImagePoster
        {
            get {  return (VM_AniDB_Anime_DefaultImage)base.DefaultImagePoster;}
            set
            {
                base.DefaultImagePoster = value;
                this.OnPropertyChanged(()=>IsImageDefault);
            }
        }

        [JsonIgnore, XmlIgnore]
        public new VM_AniDB_Anime_DefaultImage DefaultImageFanart
        {
            get { return (VM_AniDB_Anime_DefaultImage)base.DefaultImageFanart; }
            set { base.DefaultImageFanart = value; }
        }

        public new VM_AniDB_Anime_DefaultImage DefaultImageWideBanner
        {
            get { return (VM_AniDB_Anime_DefaultImage)base.DefaultImageWideBanner; }
            set { base.DefaultImageWideBanner = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        public void UpdateDisableExternalLinksFlag()
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.UpdateAnimeDisableExternalLinksFlag(AnimeID, DisableExternalLinksFlag);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        [JsonIgnore, XmlIgnore]
        public string EpisodeCountFormatted
        {
            get
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                // Multiple Episodes
                if (EpisodeCountNormal > 1)
                {
                    // Multiple Episodes, Multiple Specials
                    if (EpisodeCountSpecial > 1)
                    {
                        return $"{EpisodeCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({EpisodeCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                    }
                    // Multiple Episodes, No Specials
                    if (EpisodeCountSpecial <= 0)
                    {
                        return $"{EpisodeCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({EpisodeCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                    }
                    return $"{EpisodeCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episodes} ({EpisodeCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Special})";
                }
                // Single Episode, Multiple Specials
                if (EpisodeCountSpecial > 1)
                {
                    return $"{EpisodeCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episode} ({EpisodeCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                }
                // Single Episodes, No Specials
                if (EpisodeCountSpecial <= 0)
                {
                    return $"{EpisodeCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episode} ({EpisodeCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Specials})";
                }
                return $"{EpisodeCountNormal} {Shoko.Commons.Properties.Resources.Anime_Episode} ({EpisodeCountSpecial} {Shoko.Commons.Properties.Resources.Anime_Special})";
            }
        }

        [JsonIgnore, XmlIgnore]
        private VM_AniDB_AnimeCrossRefs aniDB_AnimeCrossRefs;
        public VM_AniDB_AnimeCrossRefs AniDB_AnimeCrossRefs
        {
            get
            {
                if (aniDB_AnimeCrossRefs != null) return aniDB_AnimeCrossRefs;
                RefreshAnimeCrossRefs();
                return aniDB_AnimeCrossRefs;
            }
        }

        public void RefreshAnimeCrossRefs()
        {
            try
            {
                aniDB_AnimeCrossRefs = (VM_AniDB_AnimeCrossRefs)VM_ShokoServer.Instance.ShokoServices.GetCrossRefDetails(AnimeID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        [JsonIgnore, XmlIgnore]
        public new int ImageEnabled
        {
            get { return base.ImageEnabled; }
            set { this.SetField(()=>base.ImageEnabled,(r)=> base.ImageEnabled = r, value, () => IsImageEnabled); }
        }


        [JsonIgnore, XmlIgnore]
        public bool IsImageEnabled
        {
            get { return ImageEnabled==1; }
            set { this.SetField(()=>ImageEnabled, value ? 1 : 0); }
        }


        private bool? isImageDefault;
        [JsonIgnore, XmlIgnore]
        public bool IsImageDefault
        {
            get
            {
                if (!isImageDefault.HasValue)
                    isImageDefault = (DefaultImagePoster != null && DefaultImagePoster.ImageParentType == (int) ImageEntityType.AniDB_Cover);
                return isImageDefault.Value;
            }
            set
            {
                this.SetField(()=>isImageDefault, value);
            }
        }

        public new int DisableExternalLinksFlag
        {
            get { return base.DisableExternalLinksFlag; }
            set
            {
                this.SetField(()=>base.DisableExternalLinksFlag,(r)=> base.DisableExternalLinksFlag = r, value, ()=> IsTvDBLinkEnabled, ()=> IsTraktLinkEnabled, ()=> IsMALLinkEnabled, ()=>IsMovieDBLinkEnabled);                
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsTvDBLinkEnabled
        {
            get { return (DisableExternalLinksFlag & Constants.LinkFlags.FlagLinkTvDB) == 0; }
            set
            {
                if (value)
                    DisableExternalLinksFlag &= Constants.LinkFlags.FlagLinkTvDB;
                else
                    DisableExternalLinksFlag |= Constants.LinkFlags.FlagLinkTvDB;
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsTraktLinkEnabled
        {
            get { return (DisableExternalLinksFlag & Constants.LinkFlags.FlagLinkTrakt) == 0; }
            set
            {
                if (value)
                    DisableExternalLinksFlag &= Constants.LinkFlags.FlagLinkTrakt;
                else
                    DisableExternalLinksFlag |= Constants.LinkFlags.FlagLinkTrakt;
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsMALLinkEnabled
        {
            get { return (DisableExternalLinksFlag & Constants.LinkFlags.FlagLinkMAL) == 0; }
            set
            {
                if (value)
                    DisableExternalLinksFlag &= Constants.LinkFlags.FlagLinkMAL;
                else
                    DisableExternalLinksFlag |= Constants.LinkFlags.FlagLinkMAL;
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsMovieDBLinkEnabled
        {
            get { return (DisableExternalLinksFlag & Constants.LinkFlags.FlagLinkMovieDB) == 0; }
            set
            {
                if (value)
                    DisableExternalLinksFlag &= Constants.LinkFlags.FlagLinkMovieDB;
                else
                    DisableExternalLinksFlag |= Constants.LinkFlags.FlagLinkMovieDB;
            }
        }

        #region Posters

        [JsonIgnore, XmlIgnore]
        public string PosterPathNoDefaultPlain
        {
            get
            {
                string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

                return fileName;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string PosterPathNoDefault
        {
            get
            {
                string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

                if (!File.Exists(fileName))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                }

                return fileName;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string PosterPath
        {
            get
            {
                string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

                if (!File.Exists(fileName))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    return fileName;
                }
                return fileName;
            }
        }

        private string posterPathWithRandoms = string.Empty;
        [JsonIgnore, XmlIgnore]
        public string PosterPathWithRandoms
        {
            get
            {
                if (VM_UserSettings.Instance.AlwaysUseAniDBPoster) return PosterPath;

                if (DefaultImagePoster == null)
                {
                    if (!string.IsNullOrEmpty(posterPathWithRandoms)) return posterPathWithRandoms;
                    VM_PosterContainer poster = GetRandomPoster();
                    if (poster != null)
                    {
                        posterPathWithRandoms = poster.FullImagePath;
                        return posterPathWithRandoms;
                    }
                    return PosterPath;
                }
                return PosterPath;
            }
        }

        public VM_PosterContainer GetRandomPoster()
        {
            List<VM_PosterContainer> enabledPosters = new List<VM_PosterContainer>();
            foreach (VM_PosterContainer poster in AniDB_AnimeCrossRefs.AllPosters)
            {
                if (poster.IsImageEnabled && File.Exists(poster.FullImagePath)) enabledPosters.Add(poster);
            }

            if (enabledPosters.Count > 0)
                return enabledPosters[posterRandom.Next(0, enabledPosters.Count)];
            return null;
        }

        [JsonIgnore, XmlIgnore]
        public string FullImagePath => PosterPath;

        [JsonIgnore, XmlIgnore]
        public string DefaultPosterPath
        {
            get
            {
                if (VM_UserSettings.Instance.AlwaysUseAniDBPoster) return PosterPath;

                if (DefaultImagePoster == null)
                    return PosterPath;
                ImageEntityType imageType = (ImageEntityType)DefaultImagePoster.ImageParentType;

                switch (imageType)
                {
                    case ImageEntityType.AniDB_Cover:
                        return PosterPath;

                    case ImageEntityType.TvDB_Cover:
                        if (DefaultImagePoster.TVPoster != null)
                            return DefaultImagePoster.TVPoster.FullImagePath;
                        return PosterPath;

                    case ImageEntityType.MovieDB_Poster:
                        if (DefaultImagePoster.MoviePoster != null)
                            return DefaultImagePoster.MoviePoster.FullImagePath;
                        return PosterPath;
                }

                return PosterPath;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string DefaultPosterPathNoBlanks
        {
            get
            {
                if (DefaultImagePoster == null)
                    return PosterPathNoDefault;
                ImageEntityType imageType = (ImageEntityType)DefaultImagePoster.ImageParentType;

                switch (imageType)
                {
                    case ImageEntityType.AniDB_Cover:
                        return PosterPath;

                    case ImageEntityType.TvDB_Cover:
                        if (DefaultImagePoster.TVPoster != null)
                            return DefaultImagePoster.TVPoster.FullImagePath;
                        return PosterPath;

                    case ImageEntityType.MovieDB_Poster:
                        if (DefaultImagePoster.MoviePoster != null)
                            return DefaultImagePoster.MoviePoster.FullImagePath;
                        return PosterPath;
                }

                return PosterPath;
            }
        }

        private List<string> GetFanartFilenames()
        {
            List<string> allFanart = new List<string>();

            // check if user has specied a fanart to always be used
            if (!string.IsNullOrEmpty(DefaultImageFanart?.FullImagePathOnlyExisting) && File.Exists(DefaultImageFanart.FullImagePathOnlyExisting))
            {
                allFanart.Add(DefaultImageFanart.FullImagePathOnlyExisting);
                return allFanart;
            }

            //if (anime.AniDB_AnimeCrossRefs != nul
            foreach (VM_FanartContainer fanart in AniDB_AnimeCrossRefs.AllFanarts)
            {
                if (!fanart.IsImageEnabled) continue;
                if (!File.Exists(fanart.FullImagePath)) continue;

                allFanart.Add(fanart.FullImagePath);
            }


            return allFanart;
        }

        private List<string> GetFanartFilenamesPreferThumb()
        {
            List<string> allFanart = new List<string>();

            // check if user has specied a fanart to always be used
            if (DefaultImageFanart != null)
            {
                allFanart.Add(DefaultImageFanart.FullImagePath);
                return allFanart;
            }

            //if (anime.AniDB_AnimeCrossRefs != nul
            foreach (VM_FanartContainer fanart in AniDB_AnimeCrossRefs.AllFanarts)
            {
                if (!fanart.IsImageEnabled) continue;

                allFanart.Add(fanart.FullThumbnailPath);
            }


            return allFanart;
        }

        [JsonIgnore, XmlIgnore]
        public bool UseFanartOnSeries
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return false;
                if (string.IsNullOrEmpty(FanartPath)) return false;

                return true;

            }
        }

        [JsonIgnore, XmlIgnore]
        public bool UsePosterOnSeries
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return true;
                if (string.IsNullOrEmpty(FanartPath)) return true;

                return false;

            }
        }

        [JsonIgnore, XmlIgnore]
        public bool UseFanartOnPlaylistHeader
        {
            get
            {
                if (!AppSettings.UseFanartOnPlaylistHeader) return false;
                if (string.IsNullOrEmpty(FanartPath)) return false;

                return true;

            }
        }

        [JsonIgnore, XmlIgnore]
        public bool UsePosterOnPlaylistHeader
        {
            get
            {
                if (!AppSettings.UseFanartOnPlaylistHeader) return true;
                if (string.IsNullOrEmpty(FanartPath)) return true;

                return false;

            }
        }

        [JsonIgnore, XmlIgnore]
        public bool UseFanartOnPlaylistItems
        {
            get
            {
                if (!AppSettings.UseFanartOnPlaylistItems) return false;
                if (string.IsNullOrEmpty(FanartPath)) return false;

                return true;

            }
        }

        [JsonIgnore, XmlIgnore]
        public bool UsePosterOnPlaylistItems
        {
            get
            {
                if (!AppSettings.UseFanartOnPlaylistItems) return true;
                if (string.IsNullOrEmpty(FanartPath)) return true;

                return false;

            }
        }

        [JsonIgnore, XmlIgnore]
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

        [JsonIgnore, XmlIgnore]
        public string FanartPathPreferThumb
        {
            get
            {
                List<string> allFanarts = GetFanartFilenamesPreferThumb();
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

        [JsonIgnore, XmlIgnore]
        public string FanartPathThenPosterPath
        {
            get
            {
                if (!AppSettings.UseFanartOnSeries) return DefaultPosterPath;

                if (string.IsNullOrEmpty(FanartPath))
                    return DefaultPosterPath;

                return FanartPath;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string FanartPathThenPosterPathForPlaylistHeader
        {
            get
            {
                if (!AppSettings.UseFanartOnPlaylistHeader) return PosterPathWithRandoms;

                if (string.IsNullOrEmpty(FanartPath))
                    return PosterPathWithRandoms;

                return FanartPath;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string FanartPathThenPosterPathForPlaylistItems
        {
            get
            {
                if (!AppSettings.UseFanartOnPlaylistItems) return PosterPathWithRandoms;

                if (string.IsNullOrEmpty(FanartPath))
                    return PosterPathWithRandoms;

                return FanartPath;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string FanartPathFallbackPosterPath
        {
            get
            {
                if (string.IsNullOrEmpty(FanartPath))
                    return PosterPathWithRandoms;

                return FanartPath;
            }
        }

        #endregion

        [JsonIgnore, XmlIgnore]
        public string AirDateAsString
        {
            get
            {
                if (AirDate.HasValue)
                    return AirDate.Value.ToString("dd MMM yyyy", Commons.Culture.Global);
                return "";
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool FinishedAiring
        {
            get
            {
                if (!EndDate.HasValue) return false; // ongoing

                // all series have finished airing and the user has all the episodes
                if (EndDate.Value < DateTime.Now) return true;

                return false;
            }
        }

        [JsonIgnore, XmlIgnore]
        public List<VM_VideoLocal> AllVideoLocals
        {
            get
            {
                try
                {
                    DateTime start = DateTime.Now;
                    List<VM_VideoLocal> vids  = VM_ShokoServer.Instance.ShokoServices.GetVideoLocalsForAnime(AnimeID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoLocal>();
                    TimeSpan ts = DateTime.Now - start;
                    logger.Trace("Got vids for anime from service: {0} in {1} ms", AnimeID, ts.TotalMilliseconds);
                    return vids;
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
                return new List<VM_VideoLocal>();
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool FanartExists => AniDB_AnimeCrossRefs?.AllFanarts.Count > 0;


        /*public string FanartPath
        {
            get
            {
                string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                // this should be randomised or use the default 
                if (DefaultFanart != null)
                    return DefaultFanart.FullImagePath;

                if (AniDB_AnimeCrossRefs == null)
                    return packUriBlank;

                if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
                    return packUriBlank;

                if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath))
                    return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

                return packUriBlank;
            }
        }*/

        [JsonIgnore, XmlIgnore]
        public string FanartPathOnlyExisting
        {
            get
            {
                // this should be randomised or use the default 
                if (!string.IsNullOrEmpty(DefaultImageFanart?.FullImagePathOnlyExisting))
                    return DefaultImageFanart.FullImagePathOnlyExisting;

                if (AniDB_AnimeCrossRefs == null)
                    return "";

                if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
                    return "";

                if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath))
                    return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

                return "";
            }
        }

        [JsonIgnore, XmlIgnore]
        public string FanartThumbnailPath
        {
            get
            {
                string packUriBlank = $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankfanart.png";

                // this should be randomised or use the default 
                if (DefaultImageFanart != null)
                    return DefaultImageFanart.FullImagePath;

                if (AniDB_AnimeCrossRefs == null)
                    return packUriBlank;

                if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
                    return packUriBlank;

                if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullThumbnailPath))
                    return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

                return packUriBlank;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string EndDateAsString => EndDate?.ToString("dd MMM yyyy", Commons.Culture.Global) ?? Shoko.Commons.Properties.Resources.Ongoing;

        [JsonIgnore, XmlIgnore]
        public string EndYearAsString => EndYear > 0 ? EndYear.ToString() : Shoko.Commons.Properties.Resources.Ongoing;

        [JsonIgnore, XmlIgnore]
        public string AirDateAndEndDate
        {
            get
            {
                if (AirDateAsString.Equals(EndDateAsString, StringComparison.InvariantCultureIgnoreCase))
                    return AirDateAsString;
                return $"{AirDateAsString}  {Shoko.Commons.Properties.Resources.To}  {EndDateAsString}";
            }
        }

        [JsonIgnore, XmlIgnore]
        public string BeginYearAndEndYear => BeginYear == EndYear ? BeginYear.ToString() : $"{BeginYear} - {EndYearAsString}";

        [JsonIgnore, XmlIgnore]
        public string AniDB_SiteURL => string.Format(Constants.URLS.AniDB_Series, AnimeID);

        [JsonIgnore, XmlIgnore]
        public string AniDB_SiteURLDiscussion => string.Format(Constants.URLS.AniDB_SeriesDiscussion, AnimeID);

        [JsonIgnore, XmlIgnore]
        public string AnimeID_Friendly => $"AniDB: {AnimeID}";

        public AnimeType AnimeTypeEnum => AnimeType > 5 ? Models.Enums.AnimeType.Other : (AnimeType) AnimeType;

        [JsonIgnore, XmlIgnore]
        public string AnimeTypeDescription
        {
            get
            {
                switch (AnimeTypeEnum)
                {
                    case Models.Enums.AnimeType.Movie: return Shoko.Commons.Properties.Resources.AnimeType_Movie;
                    case Models.Enums.AnimeType.Other: return Shoko.Commons.Properties.Resources.AnimeType_Other;
                    case Models.Enums.AnimeType.OVA: return Shoko.Commons.Properties.Resources.AnimeType_OVA;
                    case Models.Enums.AnimeType.TVSeries: return Shoko.Commons.Properties.Resources.AnimeType_TVSeries;
                    case Models.Enums.AnimeType.TVSpecial: return Shoko.Commons.Properties.Resources.AnimeType_TVSpecial;
                    case Models.Enums.AnimeType.Web: return Shoko.Commons.Properties.Resources.AnimeType_Web;
                    default: return Shoko.Commons.Properties.Resources.AnimeType_Other;

                }
            }
        }

        [JsonIgnore, XmlIgnore]
        public decimal AniDBTotalRating
        {
            get
            {
                try
                {
                    decimal totalRating = 0;
                    totalRating += ((decimal)Rating * VoteCount);
                    totalRating += ((decimal)TempRating * TempVoteCount);

                    return totalRating;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [JsonIgnore, XmlIgnore]
        public int AniDBTotalVotes
        {
            get
            {
                try
                {
                    return TempVoteCount + VoteCount;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [JsonIgnore, XmlIgnore]
        public decimal AniDBRating
        {
            get
            {
                try
                {
                    if (AniDBTotalVotes == 0)
                        return 0;
                    return AniDBTotalRating / AniDBTotalVotes / 100;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [JsonIgnore, XmlIgnore]
        public string AniDBRatingFormatted => $"{Formatting.FormatAniDBRating((double) AniDBRating)} ({AniDBTotalVotes} {Shoko.Commons.Properties.Resources.Votes})";

        #region OldTvDBCode

        /*private List<CrossRef_AniDB_TvDBVMV2> crossRefTvDBV2 = null;
        public List<CrossRef_AniDB_TvDBVMV2> CrossRefTvDBV2
        {
            get
            {
                if (crossRefTvDBV2 == null)
                {
                    try
                    {
                        //TODO
                        List<JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2> contract = VM_ShokoServer.Instance.clientBinaryHTTP.GetTVDBCrossRefV2(this.AnimeID);
                        if (contract != null)
                        {
                            crossRefTvDBV2 = new List<CrossRef_AniDB_TvDBVMV2>();
                            foreach (JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2 x in contract)
                                crossRefTvDBV2.Add(new CrossRef_AniDB_TvDBVMV2(x));
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return crossRefTvDBV2;
            }
        }

        private List<CrossRef_AniDB_TvDBEpisodeVM> crossRefTvDBEpisodes = null;
        public List<CrossRef_AniDB_TvDBEpisodeVM> CrossRefTvDBEpisodes
        {
            get
            {
                if (crossRefTvDBEpisodes == null)
                {
                    try
                    {
                        crossRefTvDBEpisodes = new List<CrossRef_AniDB_TvDBEpisodeVM>();
                        List<JMMServerBinary.Contract_CrossRef_AniDB_TvDB_Episode> contracts = VM_ShokoServer.Instance.clientBinaryHTTP.GetTVDBCrossRefEpisode(this.AnimeID);
                        if (contracts != null)
                        {
                            foreach (JMMServerBinary.Contract_CrossRef_AniDB_TvDB_Episode contract in contracts)
                                crossRefTvDBEpisodes.Add(new CrossRef_AniDB_TvDBEpisodeVM(contract));
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return crossRefTvDBEpisodes;
            }
        }

        private Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = null;
        public Dictionary<int, TvDB_EpisodeVM> DictTvDBEpisodes
        {
            get
            {
                if (dictTvDBEpisodes == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTvDBEpisodes = new Dictionary<int,TvDB_EpisodeVM>();
                            // create a dictionary of absolute episode numbers for tvdb episodes
                            // sort by season and episode number
                            // ignore season 0, which is used for specials
                            List<TvDB_EpisodeVM> eps = TvDBEpisodes;
                            

                            int i = 1;
                            foreach (TvDB_EpisodeVM ep in eps)
                            {
                                //if (ep.SeasonNumber > 0)
                                //{
                                    dictTvDBEpisodes[i] = ep;
                                    i++;
                                //}

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got TvDB Episodes in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTvDBEpisodes;
            }
        }

        private Dictionary<int, int> dictTvDBSeasons = null;
        public Dictionary<int, int> DictTvDBSeasons
        {
            get
            {
                if (dictTvDBSeasons == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTvDBSeasons = new Dictionary<int,int>();
                            // create a dictionary of season numbers and the first episode for that season
                            
                            List<TvDB_EpisodeVM> eps = TvDBEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (TvDB_EpisodeVM ep in eps)
                            {
                                if (ep.SeasonNumber != lastSeason)
                                    dictTvDBSeasons[ep.SeasonNumber] = i;

                                lastSeason = ep.SeasonNumber;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got TvDB Seasons in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTvDBSeasons;
            }
        }

        private Dictionary<int, int> dictTvDBSeasonsSpecials = null;
        public Dictionary<int, int> DictTvDBSeasonsSpecials
        {
            get
            {
                if (dictTvDBSeasonsSpecials == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTvDBSeasonsSpecials = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<TvDB_EpisodeVM> eps = TvDBEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (TvDB_EpisodeVM ep in eps)
                            {
                                if (ep.SeasonNumber > 0) continue;

                                int thisSeason = 0;
                                if (ep.AirsBeforeSeason.HasValue) thisSeason = ep.AirsBeforeSeason.Value;
                                if (ep.AirsAfterSeason.HasValue) thisSeason = ep.AirsAfterSeason.Value;

                                if (thisSeason != lastSeason)
                                    dictTvDBSeasonsSpecials[thisSeason] = i;

                                lastSeason = thisSeason;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got TvDB Seasons in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTvDBSeasonsSpecials;
            }
        }

        private List<TvDB_EpisodeVM> tvDBEpisodes = null;
        public List<TvDB_EpisodeVM> TvDBEpisodes
        {
            get
            {
                if (tvDBEpisodes == null)
                {
                    try
                    {
                        if (CrossRefTvDBV2 != null)
                        {
                            tvDBEpisodes = new List<TvDB_EpisodeVM>();
                            foreach (CrossRef_AniDB_TvDBVMV2 xref in CrossRefTvDBV2)
                            {
                                List<JMMServerBinary.Contract_TvDB_Episode> eps = VM_ShokoServer.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(xref.TvDBID);
                                
                                foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
                                    tvDBEpisodes.Add(new TvDB_EpisodeVM(episode));
                            }

                            if (tvDBEpisodes.Count > 0)
                            {
                                List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
                                sortCriteria.Add(new SortPropOrFieldAndDirection("SeasonNumber", false, SortType.eInteger));
                                sortCriteria.Add(new SortPropOrFieldAndDirection("EpisodeNumber", false, SortType.eInteger));
                                tvDBEpisodes = Sorting.MultiSort<TvDB_EpisodeVM>(tvDBEpisodes, sortCriteria);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return tvDBEpisodes;
            }
        }
        */

        #endregion

        public void ClearTvDBData()
        {
            tvSummary = null;
        }

        private VM_TvDBSummary tvSummary;
        [JsonIgnore, XmlIgnore]
        public VM_TvDBSummary TvSummary
        {
            get
            {
                if (tvSummary == null)
                {
                    try
                    {
                        tvSummary = new VM_TvDBSummary();
                        tvSummary.Populate(AnimeID);
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return tvSummary;
            }
        }

        public void ClearTraktData()
        {
            _traktSummary = null;
        }

        private VM_TraktSummary _traktSummary;
        [JsonIgnore, XmlIgnore]
        public VM_TraktSummary traktSummary
        {
            get
            {
                if (_traktSummary == null)
                {
                    try
                    {
                        _traktSummary = new VM_TraktSummary();
                        _traktSummary.Populate(AnimeID);
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return _traktSummary;
            }
        }

        [JsonIgnore, XmlIgnore]
        public List<VM_ExternalSiteLink> ExternalSiteLinks
        {
            get
            {
                List<VM_ExternalSiteLink> links = new List<VM_ExternalSiteLink>();

                VM_ExternalSiteLink anidb = new VM_ExternalSiteLink
                {
                    SiteName = "AniDB",
                    SiteLogo = @"/Images/32_anidb.png",
                    SiteURL = AniDB_SiteURL,
                    SiteURLDiscussion = AniDB_SiteURLDiscussion
                };
                links.Add(anidb);

                //RefreshAnimeCrossRefs();

                //TODO
                if (AniDB_AnimeCrossRefs != null && AniDB_AnimeCrossRefs.TvDBCrossRefExists && AniDB_AnimeCrossRefs.TvDBSeries != null && AniDB_AnimeCrossRefs.TvDBSeries.Count > 0)
                {
                    VM_ExternalSiteLink tvdb = new VM_ExternalSiteLink
                    {
                        SiteName = "TvDB",
                        SiteLogo = @"/Images/32_tvdb.png",
                        SiteURL = AniDB_AnimeCrossRefs.TvDBSeries[0].SeriesURL,
                        SiteURLDiscussion = AniDB_AnimeCrossRefs.TvDBSeries[0].SeriesURL
                    };
                    links.Add(tvdb);
                }

                if (AniDB_AnimeCrossRefs != null && AniDB_AnimeCrossRefs.TraktCrossRefExists && AniDB_AnimeCrossRefs.TraktShows != null && AniDB_AnimeCrossRefs.TraktShows.Count > 0)
                {
                    VM_ExternalSiteLink trakt = new VM_ExternalSiteLink
                    {
                        SiteName = "Trakt",
                        SiteLogo = @"/Images/32_trakt.png",
                        SiteURL = AniDB_AnimeCrossRefs.TraktShows[0].URL,
                        SiteURLDiscussion = AniDB_AnimeCrossRefs.TraktShows[0].URL
                    };
                    links.Add(trakt);
                }

                if (AniDB_AnimeCrossRefs != null && AniDB_AnimeCrossRefs.MALCrossRefExists && AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_MAL != null &&
                    AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_MAL.Count > 0)
                {
                    VM_ExternalSiteLink mal = new VM_ExternalSiteLink
                    {
                        SiteName = "MAL",
                        SiteLogo = @"/Images/32_mal.png",
                        SiteURL = AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_MAL[0].GetSiteURL(),
                        SiteURLDiscussion = AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_MAL[0].GetSiteURL()
                    };
                    links.Add(mal);
                }


                return links;
            }
        }

        public int LowestLevenshteinDistance(string input)
        {
            int lowestLD = int.MaxValue;

            foreach (string nm in this.GetAllTitles())
            {
                int ld = Utils.LevenshteinDistance(input, nm);
                if (ld < lowestLD) lowestLD = ld;
            }

            return lowestLD;
        }

        /// <summary>
        /// Gets all anime records, but puts the best X matches at the top
        /// with the rest sorted by the main title
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public static List<VM_AniDB_Anime> BestLevenshteinDistanceMatches(string input, int maxResults)
        {
            // List<AniDB_AnimeVM> AllAnime = new List<AniDB_AnimeVM>(VM_MainListHelper.Instance.AllAnimeDictionary.Values);
            List<VM_AniDB_Anime> animes = VM_ShokoServer.Instance.ShokoServices.GetAllAnime().Cast<VM_AniDB_Anime>().OrderBy(a => a.MainTitle).ToList();
            List<VM_AniDB_Anime> ret = animes.Select(a => new Tuple<int, VM_AniDB_Anime>(a.LowestLevenshteinDistance(input), a)).OrderBy(a => a.Item1).Take(maxResults).Select(a => a.Item2).ToList();
            ret.AddRange(animes);
            return ret;
        }

        public static List<VM_AniDB_Anime> BestLevenshteinDistanceMatchesCache(string input, int maxResults)
        {
            List<VM_AniDB_Anime> animes = new List<VM_AniDB_Anime>(VM_MainListHelper.Instance.AllAnimeDictionary.Values);
            List<VM_AniDB_Anime> newanimes = VM_ShokoServer.Instance.ShokoServices.GetAllAnime().Cast<VM_AniDB_Anime>().OrderBy(a => a.MainTitle).ToList();
            animes.AddRange(newanimes);
            List<VM_AniDB_Anime> ret = animes.Select(a => new Tuple<int, VM_AniDB_Anime>(a.LowestLevenshteinDistance(input), a)).OrderBy(a => a.Item1).Take(maxResults).Select(a => a.Item2).ToList();
            ret.AddRange(animes);
            return ret;

        }

        public List<string> ToSearchParameters()
        {
            List<string> parms=new List<string>();
            // only use the first 2 words of the anime's title
            string[] titles = MainTitle.Split(' ');
            int i = 0;
            foreach (string s in titles)
            {
                i++;
                parms.Add(s.Trim());
                if (i == 2) break;
            }
            return parms;
        }

    }

}
