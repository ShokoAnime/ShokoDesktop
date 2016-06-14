using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JMMClient.ViewModel;
using System.ComponentModel;
using NLog;
using JMMClient.ImageDownload;

namespace JMMClient
{
	public class AniDB_AnimeVM :  INotifyPropertyChanged
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static Random fanartRandom = new Random();
        private static Random posterRandom = new Random();

        public int AnimeID { get; set; }
		public int EpisodeCount { get; set; }
		public DateTime? AirDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string URL { get; set; }
		public string Picname { get; set; }
		public int BeginYear { get; set; }
		public int EndYear { get; set; }
		public int AnimeType { get; set; }
		public string MainTitle { get; set; }
		public string FormattedTitle { get; set; }
		public HashSet<string> AllTitles { get; set; }
		public HashSet<string> AllTags { get; set; }
		public string Description { get; set; }
		public string DescriptionTruncated { get; set; }
		public int EpisodeCountNormal { get; set; }
		public int EpisodeCountSpecial { get; set; }
		public int Rating { get; set; }
		public int VoteCount { get; set; }
		public int TempRating { get; set; }
		public int TempVoteCount { get; set; }
		public int AvgReviewRating { get; set; }
		public int ReviewCount { get; set; }
		public DateTime DateTimeUpdated { get; set; }
		public DateTime DateTimeDescUpdated { get; set; }
		public int ImageEnabled { get; set; }
		public string AwardList { get; set; }
		public int Restricted { get; set; }
		public int? AnimePlanetID { get; set; }
		public int? ANNID { get; set; }
		public int? AllCinemaID { get; set; }
		public int? AnimeNfo { get; set; }
		public int? LatestEpisodeNumber { get; set; }
		public int DisableExternalLinksFlag { get; set; }


		public AniDB_Anime_DefaultImageVM DefaultPoster { get; set; }
		public AniDB_Anime_DefaultImageVM DefaultFanart { get; set; }
		public AniDB_Anime_DefaultImageVM DefaultWideBanner { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public AniDB_AnimeVM()
		{
		}

		public void UpdateDisableExternalLinksFlag()
		{
			DisableExternalLinksFlag = 0;
			if (IsTvDBLinkDisabled) DisableExternalLinksFlag += Constants.FlagLinkTvDB;
			if (IsTraktLinkDisabled) DisableExternalLinksFlag += Constants.FlagLinkTrakt;
			if (IsMALLinkDisabled) DisableExternalLinksFlag += Constants.FlagLinkMAL;
			if (IsMovieDBLinkDisabled) DisableExternalLinksFlag += Constants.FlagLinkMovieDB;

			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeDisableExternalLinksFlag(this.AnimeID, DisableExternalLinksFlag);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public AniDB_AnimeVM(JMMServerBinary.Contract_AniDBAnime contract)
		{
			this.AirDate = contract.AirDate;
            this.AllCinemaID = contract.AllCinemaID;
            this.AllTags = new HashSet<string>(contract.AllTags);
			this.AllTitles = new HashSet<string>(contract.AllTitles);
			this.AnimeID = contract.AnimeID;
			this.AnimeNfo = contract.AnimeNfo;
			this.AnimePlanetID = contract.AnimePlanetID;
			this.AnimeType = contract.AnimeType;
			this.ANNID = contract.ANNID;
			this.AvgReviewRating = contract.AvgReviewRating;
			this.AwardList = contract.AwardList;
			this.BeginYear = contract.BeginYear;
			this.Description = Utils.ReparseDescription(contract.Description);

			

			//DescriptionTruncated = trunc;

			this.DateTimeDescUpdated = contract.DateTimeDescUpdated;
			this.DateTimeUpdated = contract.DateTimeUpdated;
			this.EndDate = contract.EndDate;
			this.EndYear = contract.EndYear;
			this.EpisodeCount = contract.EpisodeCount;
			this.EpisodeCountNormal = contract.EpisodeCountNormal;
			this.EpisodeCountSpecial = contract.EpisodeCountSpecial;
			this.ImageEnabled = contract.ImageEnabled;
			this.LatestEpisodeNumber = contract.LatestEpisodeNumber;
			this.MainTitle = contract.MainTitle;
			this.Picname = contract.Picname;
			this.Rating = contract.Rating;
			this.Restricted = contract.Restricted;
			this.ReviewCount = contract.ReviewCount;
			this.TempRating = contract.TempRating;
			this.TempVoteCount = contract.TempVoteCount;
			this.URL = contract.URL;
			this.VoteCount = contract.VoteCount;
			this.FormattedTitle = contract.FormattedTitle;
			this.DisableExternalLinksFlag = contract.DisableExternalLinksFlag;

			this.IsTvDBLinkDisabled = (DisableExternalLinksFlag & Constants.FlagLinkTvDB) > 0;
			this.IsTraktLinkDisabled = (DisableExternalLinksFlag & Constants.FlagLinkTrakt) > 0;
			this.IsMALLinkDisabled = (DisableExternalLinksFlag & Constants.FlagLinkMAL) > 0;
			this.IsMovieDBLinkDisabled = (DisableExternalLinksFlag & Constants.FlagLinkMovieDB) > 0;


			IsImageEnabled = ImageEnabled == 1;
			IsImageDisabled = ImageEnabled != 1;

			if (AnimeID == 8150)
			{
				Console.Write("");
			}

			if (contract.DefaultImagePoster != null)
				DefaultPoster = new AniDB_Anime_DefaultImageVM(contract.DefaultImagePoster);
			else
				DefaultPoster = null;

			if (contract.DefaultImageFanart != null)
				DefaultFanart = new AniDB_Anime_DefaultImageVM(contract.DefaultImageFanart);
			else
				DefaultFanart = null;

			if (contract.DefaultImageWideBanner != null)
				DefaultWideBanner = new AniDB_Anime_DefaultImageVM(contract.DefaultImageWideBanner);
			else
				DefaultWideBanner = null;

			bool isDefault = false;
			if (DefaultPoster != null && DefaultPoster.ImageParentType == (int)ImageEntityType.AniDB_Cover)
				isDefault = true;

			IsImageDefault = isDefault;
			IsImageNotDefault = !isDefault;
		}

		public string EpisodeCountFormatted
		{
			get
			{
				return string.Format("{0} {1} ({2} {3})", EpisodeCountNormal, JMMClient.Properties.Resources.Episodes,
					EpisodeCountSpecial, JMMClient.Properties.Resources.Specials);
			}
		}

		private AniDB_AnimeCrossRefsVM aniDB_AnimeCrossRefs = null;
		public AniDB_AnimeCrossRefsVM AniDB_AnimeCrossRefs
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
				JMMServerBinary.Contract_AniDB_AnimeCrossRefs xrefDetails = JMMServerVM.Instance.clientBinaryHTTP.GetCrossRefDetails(this.AnimeID);
				if (xrefDetails == null) return;

				aniDB_AnimeCrossRefs = new AniDB_AnimeCrossRefsVM();
				aniDB_AnimeCrossRefs.Populate(xrefDetails);
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		private bool isImageEnabled = false;
		public bool IsImageEnabled
		{
			get { return isImageEnabled; }
			set
			{
				isImageEnabled = value;
				NotifyPropertyChanged("IsImageEnabled");
			}
		}

		private bool isImageDisabled = false;
		public bool IsImageDisabled
		{
			get { return isImageDisabled; }
			set
			{
				isImageDisabled = value;
				NotifyPropertyChanged("IsImageDisabled");
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set
			{
				isImageDefault = value;
				NotifyPropertyChanged("IsImageDefault");
			}
		}

		private bool isImageNotDefault = false;
		public bool IsImageNotDefault
		{
			get { return isImageNotDefault; }
			set
			{
				isImageNotDefault = value;
				NotifyPropertyChanged("IsImageNotDefault");
			}
		}

		private bool isTvDBLinkDisabled = false;
		public bool IsTvDBLinkDisabled
		{
			get { return isTvDBLinkDisabled; }
			set
			{
				isTvDBLinkDisabled = value;
				NotifyPropertyChanged("IsTvDBLinkDisabled");
				IsTvDBLinkEnabled = !value;
			}
		}

		private bool isTvDBLinkEnabled = true;
		public bool IsTvDBLinkEnabled
		{
			get { return isTvDBLinkEnabled; }
			set
			{
				isTvDBLinkEnabled = value;
				NotifyPropertyChanged("IsTvDBLinkEnabled");
			}
		}

		private bool isTraktLinkDisabled = false;
		public bool IsTraktLinkDisabled
		{
			get { return isTraktLinkDisabled; }
			set
			{
				isTraktLinkDisabled = value;
				NotifyPropertyChanged("IsTraktLinkDisabled");
				IsTraktLinkEnabled = !value;
			}
		}

		private bool isTraktLinkEnabled = true;
		public bool IsTraktLinkEnabled
		{
			get { return isTraktLinkEnabled; }
			set
			{
				isTraktLinkEnabled = value;
				NotifyPropertyChanged("IsTraktLinkEnabled");
			}
		}

		private bool isMALLinkDisabled = false;
		public bool IsMALLinkDisabled
		{
			get { return isMALLinkDisabled; }
			set
			{
				isMALLinkDisabled = value;
				NotifyPropertyChanged("IsMALLinkDisabled");
				IsMALLinkEnabled = !value;
			}
		}

		private bool isMALLinkEnabled = true;
		public bool IsMALLinkEnabled
		{
			get { return isMALLinkEnabled; }
			set
			{
				isMALLinkEnabled = value;
				NotifyPropertyChanged("IsMALLinkEnabled");
			}
		}

		private bool isMovieDBLinkDisabled = false;
		public bool IsMovieDBLinkDisabled
		{
			get { return isMovieDBLinkDisabled; }
			set
			{
				isMovieDBLinkDisabled = value;
				NotifyPropertyChanged("IsMovieDBLinkDisabled");
				IsMovieDBLinkEnabled = !value;
			}
		}

		private bool isMovieDBLinkEnabled = true;
		public bool IsMovieDBLinkEnabled
		{
			get { return isMovieDBLinkEnabled; }
			set
			{
				isMovieDBLinkEnabled = value;
				NotifyPropertyChanged("IsMovieDBLinkEnabled");
			}
		}

		#region Posters

		public string PosterPathNoDefaultPlain
		{
			get
			{
				string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

				return fileName;
			}
		}

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

		public string PosterPath
		{
			get
            {
                string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

                if (!File.Exists(fileName))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(fileName)) return fileName;

                    string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);
                    return packUriBlank;
                }
                return fileName;
			}
		}

        private string posterPathWithRandoms = string.Empty;
        public string PosterPathWithRandoms
        {
            get
            {
                if (UserSettingsVM.Instance.AlwaysUseAniDBPoster) return PosterPath;

                if (DefaultPoster == null)
                {
                    if (!string.IsNullOrEmpty(posterPathWithRandoms)) return posterPathWithRandoms;
                    PosterContainer poster = GetRandomPoster();
                    if (poster != null)
                    {
                        posterPathWithRandoms = poster.FullImagePath;
                        return posterPathWithRandoms;
                    }
                    else
                    {
                        return PosterPath;
                    }
                }
                else
                    return PosterPath;
            }
        }

        public PosterContainer GetRandomPoster()
        {
            List<PosterContainer> enabledPosters = new List<PosterContainer>();
            foreach (PosterContainer poster in AniDB_AnimeCrossRefs.AllPosters)
            {
                if (poster.IsImageEnabled && File.Exists(poster.FullImagePath)) enabledPosters.Add(poster);
            }

            if (enabledPosters.Count > 0)
                return enabledPosters[posterRandom.Next(0, enabledPosters.Count)];
            else
                return null;
        }

		public string FullImagePath
		{
			get
			{
				return PosterPath;
			}
		}

		public string DefaultPosterPath
		{
			get
			{
                if (UserSettingsVM.Instance.AlwaysUseAniDBPoster) return PosterPath;

                if (DefaultPoster == null)
					return PosterPath;
				else
				{
					ImageEntityType imageType = (ImageEntityType)DefaultPoster.ImageParentType;

					switch (imageType)
					{
						case ImageEntityType.AniDB_Cover:
							return this.PosterPath;

						case ImageEntityType.TvDB_Cover:
							if (DefaultPoster.TVPoster != null)
								return DefaultPoster.TVPoster.FullImagePath;
							else
								return this.PosterPath;

						case ImageEntityType.Trakt_Poster:
							if (DefaultPoster.TraktPoster != null)
								return DefaultPoster.TraktPoster.FullImagePath;
							else
								return this.PosterPath;

						case ImageEntityType.MovieDB_Poster:
							if (DefaultPoster.MoviePoster != null)
								return DefaultPoster.MoviePoster.FullImagePath;
							else
								return this.PosterPath;
					}
				}

				return PosterPath;
			}
		}

		public string DefaultPosterPathNoBlanks
		{
			get
			{
				if (DefaultPoster == null)
					return PosterPathNoDefault;
				else
				{
					ImageEntityType imageType = (ImageEntityType)DefaultPoster.ImageParentType;

					switch (imageType)
					{
						case ImageEntityType.AniDB_Cover:
							return this.PosterPath;

						case ImageEntityType.TvDB_Cover:
							if (DefaultPoster.TVPoster != null)
								return DefaultPoster.TVPoster.FullImagePath;
							else
								return this.PosterPath;

						case ImageEntityType.Trakt_Poster:
							if (DefaultPoster.TraktPoster != null)
								return DefaultPoster.TraktPoster.FullImagePath;
							else
								return this.PosterPath;

						case ImageEntityType.MovieDB_Poster:
							if (DefaultPoster.MoviePoster != null)
								return DefaultPoster.MoviePoster.FullImagePath;
							else
								return this.PosterPath;
					}
				}

				return PosterPath;
			}
		}

		private List<string> GetFanartFilenames()
		{
			List<string> allFanart = new List<string>();

			// check if user has specied a fanart to always be used
			if (DefaultFanart != null)
			{
				if (!string.IsNullOrEmpty(DefaultFanart.FullImagePathOnlyExisting) && File.Exists(DefaultFanart.FullImagePathOnlyExisting))
				{
					allFanart.Add(DefaultFanart.FullImagePathOnlyExisting);
					return allFanart;
				}
			}

			//if (anime.AniDB_AnimeCrossRefs != nul
			foreach (FanartContainer fanart in AniDB_AnimeCrossRefs.AllFanarts)
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
			if (DefaultFanart != null)
			{
				allFanart.Add(DefaultFanart.FullThumbnailPath);
				return allFanart;
			}

			//if (anime.AniDB_AnimeCrossRefs != nul
			foreach (FanartContainer fanart in AniDB_AnimeCrossRefs.AllFanarts)
			{
				if (!fanart.IsImageEnabled) continue;

				allFanart.Add(fanart.FullThumbnailPath);
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

		public bool UseFanartOnPlaylistHeader
		{
			get
			{
				if (!AppSettings.UseFanartOnPlaylistHeader) return false;
				if (string.IsNullOrEmpty(FanartPath)) return false;

				return true;

			}
		}

		public bool UsePosterOnPlaylistHeader
		{
			get
			{
				if (!AppSettings.UseFanartOnPlaylistHeader) return true;
				if (string.IsNullOrEmpty(FanartPath)) return true;

				return false;

			}
		}

		public bool UseFanartOnPlaylistItems
		{
			get
			{
				if (!AppSettings.UseFanartOnPlaylistItems) return false;
				if (string.IsNullOrEmpty(FanartPath)) return false;

				return true;

			}
		}

		public bool UsePosterOnPlaylistItems
		{
			get
			{
				if (!AppSettings.UseFanartOnPlaylistItems) return true;
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

		public string AirDateAsString
		{
			get
			{
				if (AirDate.HasValue)
					return AirDate.Value.ToString("dd MMM yyyy", Globals.Culture);
				else
					return "";
			}
		}

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

        public List<VideoLocalVM> AllVideoLocals
        {
            get
            {
                List<VideoLocalVM> vids = new List<VideoLocalVM>();
                try
                {
                    DateTime start = DateTime.Now;
                    List<JMMServerBinary.Contract_VideoLocal> raws = JMMServerVM.Instance.clientBinaryHTTP.GetVideoLocalsForAnime(AnimeID,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                    TimeSpan ts = DateTime.Now - start;
                    logger.Trace("Got vids for anime from service: {0} in {1} ms", AnimeID, ts.TotalMilliseconds);

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

        public bool FanartExists
		{
			get
			{
				if (AniDB_AnimeCrossRefs == null) return false;

				if (AniDB_AnimeCrossRefs.AllFanarts.Count > 0)
					return true;
				else
					return false;

			}
		}

		public bool FanartMissing
		{
			get
			{
				return !FanartExists;
			}
		}

		/*public string FanartPath
		{
			get
			{
				string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);

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

		public string FanartPathOnlyExisting
		{
			get
			{
				// this should be randomised or use the default 
				if (DefaultFanart != null && !string.IsNullOrEmpty(DefaultFanart.FullImagePathOnlyExisting))
					return DefaultFanart.FullImagePathOnlyExisting;

				if (AniDB_AnimeCrossRefs == null)
					return "";

				if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
					return "";

				if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath))
					return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

				return "";
			}
		}

		public string FanartThumbnailPath
		{
			get
			{
				string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);

				// this should be randomised or use the default 
				if (DefaultFanart != null)
					return DefaultFanart.FullThumbnailPath;

				if (AniDB_AnimeCrossRefs == null)
					return packUriBlank;

				if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
					return packUriBlank;

				if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullThumbnailPath))
					return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

				return packUriBlank;
			}
		}

		public string EndDateAsString
		{
			get
			{
				if (EndDate.HasValue)
					return EndDate.Value.ToString("dd MMM yyyy", Globals.Culture);
				else
					return JMMClient.Properties.Resources.Ongoing;
			}
		}

		public string EndYearAsString
		{
			get
			{
				if (EndYear > 0)
					return EndYear.ToString();
				else
					return JMMClient.Properties.Resources.Ongoing;
			}
		}

		public string AirDateAndEndDate
		{
			get
			{
				return string.Format("{0}  {1}  {2}", AirDateAsString, JMMClient.Properties.Resources.To, EndDateAsString);
			}
		}

		public string BeginYearAndEndYear
		{
			get
			{
				if (BeginYear == EndYear) return BeginYear.ToString();
				else
					return string.Format("{0} - {1}", BeginYear, EndYearAsString);
			}
		}

		public string AniDB_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_Series, AnimeID);

			}
		}

		public string AniDB_SiteURLDiscussion
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_SeriesDiscussion, AnimeID);

			}
		}

		public string AnimeID_Friendly
		{
			get
			{
				return string.Format("AniDB: {0}", AnimeID);
			}
		}

		public enAnimeType AnimeTypeEnum
		{
			get
			{
				if (AnimeType > 5) return enAnimeType.Other;
				return (enAnimeType)AnimeType;
			}
		}

		public string AnimeTypeDescription
		{
			get
			{
				switch (AnimeTypeEnum)
				{
					case enAnimeType.Movie: return JMMClient.Properties.Resources.AnimeType_Movie;
					case enAnimeType.Other: return JMMClient.Properties.Resources.AnimeType_Other;
					case enAnimeType.OVA: return JMMClient.Properties.Resources.AnimeType_OVA;
					case enAnimeType.TVSeries: return JMMClient.Properties.Resources.AnimeType_TVSeries;
					case enAnimeType.TVSpecial: return JMMClient.Properties.Resources.AnimeType_TVSpecial;
					case enAnimeType.Web: return JMMClient.Properties.Resources.AnimeType_Web;
					default: return JMMClient.Properties.Resources.AnimeType_Other;
						
				}
			}
		}

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
					return TempVoteCount + VoteCount;
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
					if (AniDBTotalVotes == 0)
						return 0;
					else
						return AniDBTotalRating / (decimal)AniDBTotalVotes / (decimal)100;

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

		

		private AniDB_AnimeDetailedVM detail = null;
		public AniDB_AnimeDetailedVM Detail
		{
			get
			{
				if (detail == null)
				{
					try
					{
						JMMServerBinary.Contract_AniDB_AnimeDetailed contract = JMMServerVM.Instance.clientBinaryHTTP.GetAnimeDetailed(this.AnimeID);
						detail = new AniDB_AnimeDetailedVM();
						detail.Populate(contract, this.AnimeID);
					}
					catch (Exception ex)
					{
						Utils.ShowErrorMessage(ex);
					}
				}
				return detail;

				/*if (MainListHelperVM.Instance.AllAnimeDetailedDictionary.ContainsKey(this.AnimeID))
					return MainListHelperVM.Instance.AllAnimeDetailedDictionary[this.AnimeID];
				else
					return null;*/
			}
		}

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
						List<JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2> contract = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefV2(this.AnimeID);
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
						List<JMMServerBinary.Contract_CrossRef_AniDB_TvDB_Episode> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefEpisode(this.AnimeID);
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
								List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(xref.TvDBID);
								
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

		private TvDBSummary tvSummary = null;
		public TvDBSummary TvSummary
		{
			get
			{
				if (tvSummary == null)
				{
					try
					{
						tvSummary = new TvDBSummary();
						tvSummary.Populate(this.AnimeID);
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

        private TraktSummary _traktSummary = null;
        public TraktSummary traktSummary
        {
            get
            {
                if (_traktSummary == null)
                {
                    try
                    {
                        _traktSummary = new TraktSummary();
                        _traktSummary.Populate(this.AnimeID);
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return _traktSummary;
            }
        }

		public List<ExternalSiteLink> ExternalSiteLinks
		{
			get
			{
				List<ExternalSiteLink> links = new List<ExternalSiteLink>();

				ExternalSiteLink anidb = new ExternalSiteLink();
				anidb.SiteName = "AniDB";
				anidb.SiteLogo = @"/Images/anidb.ico";
				anidb.SiteURL = AniDB_SiteURL;
				anidb.SiteURLDiscussion = AniDB_SiteURLDiscussion;
				links.Add(anidb);

				//RefreshAnimeCrossRefs();

				//TODO
				if (AniDB_AnimeCrossRefs != null && AniDB_AnimeCrossRefs.TvDBCrossRefExists && AniDB_AnimeCrossRefs.TvDBSeriesV2 != null && AniDB_AnimeCrossRefs.TvDBSeriesV2.Count > 0)
				{
					ExternalSiteLink tvdb = new ExternalSiteLink();
					tvdb.SiteName = "TvDB";
					tvdb.SiteLogo = @"/Images/tvdb.ico";
					tvdb.SiteURL = AniDB_AnimeCrossRefs.TvDBSeriesV2[0].SeriesURL;
					tvdb.SiteURLDiscussion = AniDB_AnimeCrossRefs.TvDBSeriesV2[0].SeriesURL;
					links.Add(tvdb);
				}

                if (AniDB_AnimeCrossRefs != null && AniDB_AnimeCrossRefs.TraktCrossRefExists && AniDB_AnimeCrossRefs.TraktShowV2 != null && AniDB_AnimeCrossRefs.TraktShowV2.Count > 0)
				{
					ExternalSiteLink trakt = new ExternalSiteLink();
					trakt.SiteName = "Trakt";
					trakt.SiteLogo = @"/Images/trakttv.ico";
					trakt.SiteURL = AniDB_AnimeCrossRefs.TraktShowV2[0].ShowURL;
                    trakt.SiteURLDiscussion = AniDB_AnimeCrossRefs.TraktShowV2[0].ShowURL;
					links.Add(trakt);
				}

				if (AniDB_AnimeCrossRefs != null && AniDB_AnimeCrossRefs.MALCrossRefExists && AniDB_AnimeCrossRefs.CrossRef_AniDB_MAL != null &&
					AniDB_AnimeCrossRefs.CrossRef_AniDB_MAL.Count > 0)
				{
					ExternalSiteLink mal = new ExternalSiteLink();
					mal.SiteName = "MAL";
					mal.SiteLogo = @"/Images/myanimelist.ico";
					mal.SiteURL = AniDB_AnimeCrossRefs.CrossRef_AniDB_MAL[0].SiteURL;
					mal.SiteURLDiscussion = AniDB_AnimeCrossRefs.CrossRef_AniDB_MAL[0].SiteURL;
					links.Add(mal);
				}


				return links;
			}
		}

		public int LowestLevenshteinDistance(string input)
		{
			int lowestLD = int.MaxValue;

			foreach (string nm in this.AllTitles)
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
		public static List<AniDB_AnimeVM> BestLevenshteinDistanceMatches(string input, int maxResults)
		{
			List<LDContainer> allLDs = new List<LDContainer>();

			List<AniDB_AnimeVM> AllAnime = new List<AniDB_AnimeVM>();
			List<JMMServerBinary.Contract_AniDBAnime> animeRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();
			foreach (JMMServerBinary.Contract_AniDBAnime anime in animeRaw)
			{
				AniDB_AnimeVM animeNew = new AniDB_AnimeVM(anime);
				AllAnime.Add(animeNew);
			}

			// now sort the groups by name
			List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("MainTitle", false, SortType.eString));
			AllAnime = Sorting.MultiSort<AniDB_AnimeVM>(AllAnime, sortCriteria);

			
			foreach (AniDB_AnimeVM anime in AllAnime)
			{
				LDContainer ldc = new LDContainer();
				ldc.AnimeID = anime.AnimeID;
				ldc.LD = anime.LowestLevenshteinDistance(input);
				ldc.Anime = anime;
				allLDs.Add(ldc);
			}

			// now sort the groups by best score
			sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("LD", false, SortType.eInteger));
			allLDs = Sorting.MultiSort<LDContainer>(allLDs, sortCriteria);

			List<AniDB_AnimeVM> retAnime = new List<AniDB_AnimeVM>();
			for (int i = 0; i < allLDs.Count; i++)
			{
				AniDB_AnimeVM anime = allLDs[i].Anime;
				retAnime.Add(anime);
				if (i == maxResults - 1) break;
			}

			foreach (AniDB_AnimeVM anime in AllAnime)
			{
				retAnime.Add(anime);
			}

			return retAnime;
		}

		public static List<AniDB_AnimeVM> BestLevenshteinDistanceMatchesCache(string input, int maxResults)
		{
			List<LDContainer> allLDs = new List<LDContainer>();

			List<AniDB_AnimeVM> AllAnime = new List<AniDB_AnimeVM>(MainListHelperVM.Instance.AllAnimeDictionary.Values);
			List<JMMServerBinary.Contract_AniDBAnime> animeRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();
			foreach (JMMServerBinary.Contract_AniDBAnime anime in animeRaw)
			{
				AniDB_AnimeVM animeNew = new AniDB_AnimeVM(anime);
				AllAnime.Add(animeNew);
			}

			// now sort the groups by name
			List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("MainTitle", false, SortType.eString));
			AllAnime = Sorting.MultiSort<AniDB_AnimeVM>(AllAnime, sortCriteria);


			foreach (AniDB_AnimeVM anime in AllAnime)
			{
				LDContainer ldc = new LDContainer();
				ldc.AnimeID = anime.AnimeID;
				ldc.LD = anime.LowestLevenshteinDistance(input);
				ldc.Anime = anime;
				allLDs.Add(ldc);
			}

			// now sort the groups by best score
			sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("LD", false, SortType.eInteger));
			allLDs = Sorting.MultiSort<LDContainer>(allLDs, sortCriteria);

			List<AniDB_AnimeVM> retAnime = new List<AniDB_AnimeVM>();
			for (int i = 0; i < allLDs.Count; i++)
			{
				AniDB_AnimeVM anime = allLDs[i].Anime;
				retAnime.Add(anime);
				if (i == maxResults - 1) break;
			}

			foreach (AniDB_AnimeVM anime in AllAnime)
			{
				retAnime.Add(anime);
			}

			return retAnime;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", this.FormattedTitle, this.AnimeID);
		}


	}

	public class LDContainer
	{
		public int LD { get; set; }
		public int AnimeID { get; set; }
		public AniDB_AnimeVM Anime { get; set; }
	}
}
