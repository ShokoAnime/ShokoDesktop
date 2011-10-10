using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using JMMClient.ViewModel;
using System.ComponentModel;
using NLog;
using System.IO;

namespace JMMClient
{
	public class AnimeEpisodeVM : MainListWrapper, INotifyPropertyChanged, IComparable<AnimeEpisodeVM>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public enum SortMethod { EpisodeNumber = 0, AirDate = 1 };
		public static SortMethod SortType { get; set; }

		public int AnimeEpisodeID { get; set; }
		public int EpisodeNumber { get; set; }
		public int EpisodeType { get; set; }
		public int AnimeSeriesID { get; set; }
		public int AniDB_EpisodeID { get; set; }
		public string Description { get; set; }
		public DateTime DateTimeUpdated { get; set; }
		//public int IsWatched { get; set; }
		public DateTime? WatchedDate { get; set; }
		public int PlayedCount { get; set; }
		public int WatchedCount { get; set; }
		public int StoppedCount { get; set; }
		//public int LocalFileCount { get; set; }

		

		public int AniDB_LengthSeconds { get; set; }
		public string AniDB_Rating { get; set; }
		public string AniDB_Votes { get; set; }
		public string AniDB_RomajiName { get; set; }
		public string AniDB_EnglishName { get; set; }
		public DateTime? AniDB_AirDate { get; set; }

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

		private int isWatched = 0;
		public int IsWatched
		{
			get { return isWatched; }
			set
			{
				isWatched = value;
				NotifyPropertyChanged("IsWatched");
				Watched = IsWatched == 1;
				Unwatched = IsWatched == 0;
			}
		}

		private string episodeNumberAndName = "";
		public string EpisodeNumberAndName
		{
			get { return episodeNumberAndName; }
			set
			{
				episodeNumberAndName = value;
				NotifyPropertyChanged("EpisodeNumberAndName");
			}
		}

		private string episodeNumberAndNameWithType = "";
		public string EpisodeNumberAndNameWithType
		{
			get { return episodeNumberAndNameWithType; }
			set
			{
				episodeNumberAndNameWithType = value;
				NotifyPropertyChanged("EpisodeNumberAndNameWithType");
			}
		}

		private string episodeTypeAndNumber = "";
		public string EpisodeTypeAndNumber
		{
			get { return episodeTypeAndNumber; }
			set
			{
				episodeTypeAndNumber = value;
				NotifyPropertyChanged("EpisodeTypeAndNumber");
			}
		}

		private string episodeTypeAndNumberAbsolute = "";
		public string EpisodeTypeAndNumberAbsolute
		{
			get { return episodeTypeAndNumberAbsolute; }
			set
			{
				episodeTypeAndNumberAbsolute = value;
				NotifyPropertyChanged("EpisodeTypeAndNumberAbsolute");
			}
		}

		private bool watched = false;
		public bool Watched
		{
			get { return watched; }
			set
			{
				watched = value;
				NotifyPropertyChanged("Watched");
			}
		}

		private bool unwatched = false;
		public bool Unwatched
		{
			get { return unwatched; }
			set
			{
				unwatched = value;
				NotifyPropertyChanged("Unwatched");
			}
		}

		private int localFileCount = 0;
		public int LocalFileCount
		{
			get { return localFileCount; }
			set
			{
				localFileCount = value;
				NotifyPropertyChanged("LocalFileCount");
				OneFileOnly = localFileCount == 1;
				NoFiles = localFileCount == 0;
				MultipleFiles = LocalFileCount > 1;
				HasFiles = localFileCount > 0;
				FileDetails = string.Format("{0} Files", LocalFileCount);
			}
		}

		private bool oneFileOnly = false;
		public bool OneFileOnly
		{
			get { return oneFileOnly; }
			set
			{
				oneFileOnly = value;
				NotifyPropertyChanged("OneFileOnly");
			}
		}

		private bool noFiles = false;
		public bool NoFiles
		{
			get { return noFiles; }
			set
			{
				noFiles = value;
				NotifyPropertyChanged("NoFiles");
			}
		}

		private bool multipleFiles = false;
		public bool MultipleFiles
		{
			get { return multipleFiles; }
			set
			{
				multipleFiles = value;
				NotifyPropertyChanged("MultipleFiles");
			}
		}

		private bool hasFiles = false;
		public bool HasFiles
		{
			get { return hasFiles; }
			set
			{
				hasFiles = value;
				NotifyPropertyChanged("HasFiles");
			}
		}

		private string fileDetails = "";
		public string FileDetails
		{
			get { return fileDetails; }
			set
			{
				fileDetails = value;
				NotifyPropertyChanged("FileDetails");
			}
		}

		private string episodeOverviewLoading = "";
		public string EpisodeOverviewLoading
		{
			get { return episodeOverviewLoading; }
			set
			{
				episodeOverviewLoading = value;
				NotifyPropertyChanged("EpisodeOverviewLoading");
			}
		}

		private string episodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
		public string EpisodeImageLoading
		{
			get { return episodeImageLoading; }
			set
			{
				episodeImageLoading = value;
				NotifyPropertyChanged("EpisodeImageLoading");
			}
		}

		private string airDateAsString = "";
		public string AirDateAsString
		{
			get { return airDateAsString; }
			set
			{
				airDateAsString = value;
				NotifyPropertyChanged("AirDateAsString");
			}
		}

		private string aniDBRatingFormatted = "";
		public string AniDBRatingFormatted
		{
			get { return aniDBRatingFormatted; }
			set
			{
				aniDBRatingFormatted = value;
				NotifyPropertyChanged("AniDBRatingFormatted");
			}
		}

		private bool showEpisodeImageInSummary = true;
		public bool ShowEpisodeImageInSummary
		{
			get { return showEpisodeImageInSummary; }
			set
			{
				showEpisodeImageInSummary = value;
				NotifyPropertyChanged("ShowEpisodeImageInSummary");
			}
		}

		private bool showEpisodeOverviewInSummary = true;
		public bool ShowEpisodeOverviewInSummary
		{
			get { return showEpisodeOverviewInSummary; }
			set
			{
				showEpisodeOverviewInSummary = value;
				NotifyPropertyChanged("ShowEpisodeOverviewInSummary");
			}
		}


		private bool showEpisodeImageInExpanded = true;
		public bool ShowEpisodeImageInExpanded
		{
			get { return showEpisodeImageInExpanded; }
			set
			{
				showEpisodeImageInExpanded = value;
				NotifyPropertyChanged("ShowEpisodeImageInExpanded");
			}
		}

		private bool showEpisodeOverviewInExpanded = true;
		public bool ShowEpisodeOverviewInExpanded
		{
			get { return showEpisodeOverviewInExpanded; }
			set
			{
				showEpisodeOverviewInExpanded = value;
				NotifyPropertyChanged("ShowEpisodeOverviewInExpanded");
			}
		}

		private bool showEpisodeImageInDashboard = true;
		public bool ShowEpisodeImageInDashboard
		{
			get { return showEpisodeImageInDashboard; }
			set
			{
				showEpisodeImageInDashboard = value;
				NotifyPropertyChanged("ShowEpisodeImageInDashboard");
			}
		}

		#endregion

		public void SetTvDBImageAndOverview()
		{
			this.RefreshAnime();

			Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = AniDB_Anime.DictTvDBEpisodes;
			Dictionary<int, int> dictTvDBSeasons = AniDB_Anime.DictTvDBSeasons;
			Dictionary<int, int> dictTvDBSeasonsSpecials = AniDB_Anime.DictTvDBSeasonsSpecials;
			CrossRef_AniDB_TvDBVM tvDBCrossRef = AniDB_Anime.CrossRefTvDB;

			SetTvDBImageAndOverview(dictTvDBEpisodes, dictTvDBSeasons, dictTvDBSeasonsSpecials, tvDBCrossRef);
		}

		public void SetTvDBImageAndOverview(Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes, Dictionary<int, int> dictTvDBSeasons, 
			Dictionary<int, int> dictTvDBSeasonsSpecials, CrossRef_AniDB_TvDBVM tvDBCrossRef)
		{
			// now do stuff to improve performance
			if (this.EpisodeTypeEnum == JMMClient.EpisodeType.Episode)
			{
				if (dictTvDBEpisodes != null && dictTvDBSeasons != null && tvDBCrossRef != null)
				{
					if (dictTvDBSeasons.ContainsKey(tvDBCrossRef.TvDBSeasonNumber))
					{
						int episodeNumber = dictTvDBSeasons[tvDBCrossRef.TvDBSeasonNumber] + this.EpisodeNumber - 1;
						if (dictTvDBEpisodes.ContainsKey(episodeNumber))
						{

							TvDB_EpisodeVM tvep = dictTvDBEpisodes[episodeNumber];
							if (string.IsNullOrEmpty(tvep.Overview))
								this.EpisodeOverviewLoading = "Episode Overview Not Available";
							else
								this.EpisodeOverviewLoading = tvep.Overview;

							if (string.IsNullOrEmpty(tvep.FullImagePath) || !File.Exists(tvep.FullImagePath))
							{
								if (string.IsNullOrEmpty(tvep.OnlineImagePath))
								{
									this.EpisodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
									// if there is no proper image to show, we will hide it on the dashboard
									ShowEpisodeImageInDashboard = false;
								}
								else
									this.EpisodeImageLoading = tvep.OnlineImagePath;
							}
							else
								this.EpisodeImageLoading = tvep.FullImagePath;
						}
					}
				}
			}

			if (this.EpisodeTypeEnum == JMMClient.EpisodeType.Special)
			{
				if (dictTvDBEpisodes != null && dictTvDBSeasonsSpecials != null && tvDBCrossRef != null)
				{
					if (dictTvDBSeasonsSpecials.ContainsKey(tvDBCrossRef.TvDBSeasonNumber))
					{
						int episodeNumber = dictTvDBSeasonsSpecials[tvDBCrossRef.TvDBSeasonNumber] + this.EpisodeNumber - 1;
						if (dictTvDBEpisodes.ContainsKey(episodeNumber))
						{
							TvDB_EpisodeVM tvep = dictTvDBEpisodes[episodeNumber];
							this.EpisodeOverviewLoading = tvep.Overview;

							if (string.IsNullOrEmpty(tvep.FullImagePath) || !File.Exists(tvep.FullImagePath))
							{
								if (string.IsNullOrEmpty(tvep.OnlineImagePath))
								{
									this.EpisodeImageLoading = @"/Images/EpisodeThumb_NotFound.png";
									// if there is no proper image to show, we will hide it on the dashboard
									ShowEpisodeImageInDashboard = false;
								}
								else
									this.EpisodeImageLoading = tvep.OnlineImagePath;
							}
							else
								this.EpisodeImageLoading = tvep.FullImagePath;
						}
					}
				}
			}
		}

		public bool FutureDated
		{
			get
			{
				if (!AniDB_AirDate.HasValue) return true;

				return (AniDB_AirDate.Value > DateTime.Now);
			}
		}

		

		public string AniDB_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_Episode, AniDB_EpisodeID);
			}
		}

		public string AnimeName
		{
			get
			{
				logger.Trace("Getting anime name for ep#: {0}", this.EpisodeNumber);

				string animeName = "";
				if (MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(this.AnimeSeriesID))
				{
					AnimeSeriesVM ser = MainListHelperVM.Instance.AllSeriesDictionary[this.AnimeSeriesID];
					if (ser.AniDB_Anime != null && ser.AniDB_Anime.Detail != null)
						animeName = ser.SeriesName;
				}
				else
					animeName = "NOT FOUND!";

				return animeName;
			}
		}

		public TvDB_EpisodeVM TvDBEpisode
		{
			get
			{
				logger.Trace("Getting tvdb episode for ep#: {0}", this.EpisodeNumber);

				if (MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(this.AnimeSeriesID))
				{
					AnimeSeriesVM ser = MainListHelperVM.Instance.AllSeriesDictionary[this.AnimeSeriesID];
					if (ser.AniDB_Anime == null) return null;
					if (ser.AniDB_Anime.CrossRefTvDB == null) return null;

					/*foreach (TvDB_EpisodeVM ep in ser.AniDB_Anime.TvDBEpisodes)
					{
						if (ep.EpisodeNumber == this.EpisodeNumber)
							return ep;
					}*/

					if (ser.AniDB_Anime.DictTvDBEpisodes.ContainsKey(this.EpisodeNumber))
						return ser.AniDB_Anime.DictTvDBEpisodes[this.EpisodeNumber];

					return null;
				}
				else
					return null;
			}
		}


		private AniDB_AnimeVM aniDB_Anime = null;
		public AniDB_AnimeVM AniDB_Anime
		{
			get
			{
				return aniDB_Anime;
			}
		}

		public void RefreshAnime()
		{
			aniDB_Anime = null;

			if (MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(this.AnimeSeriesID))
			{
				AnimeSeriesVM ser = MainListHelperVM.Instance.AllSeriesDictionary[this.AnimeSeriesID];
				aniDB_Anime = ser.AniDB_Anime;
			}
		}


		

		public string RunTime
		{
			get
			{
				return Utils.FormatSecondsToDisplayTime(AniDB_LengthSeconds);
			}
		}

		public string EpisodeName
		{
			get
			{
				if (AniDB_EnglishName.Trim().Length > 0)
					return AniDB_EnglishName;
				else
					return AniDB_RomajiName;
			}
		}

		public EpisodeType EpisodeTypeEnum
		{
			get
			{
				return (EpisodeType)EpisodeType;
			}
		}

		public AnimeEpisodeVM()
		{
		}

		public AnimeEpisodeVM(JMMServerBinary.Contract_AnimeEpisode contract)
		{
			Populate(contract);
		}

		public void Populate(JMMServerBinary.Contract_AnimeEpisode contract)
		{
			try
			{
				//Cloner.Clone(contract, this);
				this.AniDB_EpisodeID = contract.AniDB_EpisodeID;
				this.AnimeEpisodeID = contract.AnimeEpisodeID;
				this.AnimeSeriesID = contract.AnimeSeriesID;
				this.DateTimeUpdated = contract.DateTimeUpdated;
				this.Description = "";
				this.EpisodeNumber = contract.EpisodeNumber;
				this.EpisodeType = contract.EpisodeType;
				this.IsWatched = contract.IsWatched;
				this.LocalFileCount = contract.LocalFileCount;
				this.PlayedCount = contract.PlayedCount;
				this.StoppedCount = contract.StoppedCount;
				this.WatchedCount = contract.WatchedCount;
				this.WatchedDate = contract.WatchedDate;

				this.AniDB_LengthSeconds = contract.AniDB_LengthSeconds;
				this.AniDB_Rating = contract.AniDB_Rating;
				this.AniDB_Votes = contract.AniDB_Votes;
				this.AniDB_RomajiName = contract.AniDB_RomajiName;
				this.AniDB_EnglishName = contract.AniDB_EnglishName;
				this.AniDB_AirDate = contract.AniDB_AirDate;

				EpisodeNumberAndName = string.Format("{0} - {1}", EpisodeNumber, EpisodeName);
				string shortType = "";
				switch (EpisodeTypeEnum)
				{
					case JMMClient.EpisodeType.Credits: shortType = "C"; break;
					case JMMClient.EpisodeType.Episode: shortType = ""; break;
					case JMMClient.EpisodeType.Other: shortType = "O"; break;
					case JMMClient.EpisodeType.Parody: shortType = "P"; break;
					case JMMClient.EpisodeType.Special: shortType = "S"; break;
					case JMMClient.EpisodeType.Trailer: shortType = "T"; break;
				}
				EpisodeNumberAndNameWithType = string.Format("{0}{1} - {2}", shortType, EpisodeNumber, EpisodeName);
				EpisodeTypeAndNumber = string.Format("{0}{1}", shortType, EpisodeNumber);
				EpisodeTypeAndNumberAbsolute = string.Format("{0}{1}", shortType, EpisodeNumber.ToString().PadLeft(5, '0'));

				

				if (AniDB_AirDate.HasValue)
					AirDateAsString = AniDB_AirDate.Value.ToString("dd MMM yyyy", Globals.Culture);
				else
					AirDateAsString = "";

				//logger.Trace("Getting AniDBRatingFormatted for ep#: {0}", this.EpisodeNumber);

				AniDBRatingFormatted = string.Format("{0}: {1} ({2} {3})", JMMClient.Properties.Resources.Rating, AniDB_Rating, AniDB_Votes, JMMClient.Properties.Resources.Votes);


				// episode image / overview in summary
				ShowEpisodeImageInSummary = false;
				ShowEpisodeOverviewInSummary = false;
				if (UserSettingsVM.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.Never)
				{
					ShowEpisodeImageInSummary = false;
					ShowEpisodeOverviewInSummary = false;
				}
				else
				{
					if (UserSettingsVM.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.Always)
					{
						ShowEpisodeImageInSummary = true;
						ShowEpisodeOverviewInSummary = true;
					}

					if (!Watched && UserSettingsVM.Instance.HideEpisodeImageWhenUnwatched) ShowEpisodeImageInSummary = false;
					if (!Watched && UserSettingsVM.Instance.HideEpisodeOverviewWhenUnwatched) ShowEpisodeOverviewInSummary = false;
				}

				// episode image / overview in expanded
				ShowEpisodeImageInExpanded = false;
				ShowEpisodeOverviewInExpanded = false;
				if (UserSettingsVM.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.Never)
				{
					ShowEpisodeImageInExpanded = false;
					ShowEpisodeOverviewInExpanded = false;
				}
				else
				{
					if (UserSettingsVM.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.Always ||
						UserSettingsVM.Instance.EpisodeImageOverviewStyle == (int)EpisodeDisplayStyle.InExpanded)
					{
						ShowEpisodeImageInExpanded = true;
						ShowEpisodeOverviewInExpanded = true;
					}

					if (!Watched && UserSettingsVM.Instance.HideEpisodeImageWhenUnwatched) ShowEpisodeImageInExpanded = false;
					if (!Watched && UserSettingsVM.Instance.HideEpisodeOverviewWhenUnwatched) ShowEpisodeOverviewInExpanded = false;
				}

				ShowEpisodeImageInDashboard = ShowEpisodeImageInExpanded;


			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

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

		public int CompareTo(AnimeEpisodeVM obj)
		{
			switch (SortType)
			{
				case SortMethod.EpisodeNumber:
					return EpisodeNumber.CompareTo(obj.EpisodeNumber);

				case SortMethod.AirDate:
					if (AniDB_AirDate.HasValue && obj.AniDB_AirDate.HasValue)
						return AniDB_AirDate.Value.CompareTo(obj.AniDB_AirDate.Value);
					else
						return 0;

				default:
					return EpisodeNumber.CompareTo(obj.EpisodeNumber);
			}
		}


		public void RefreshFilesForEpisode()
		{
			try
			{
				filesForEpisode = new List<VideoDetailedVM>();
				List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(AnimeEpisodeID, 
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_VideoDetailed fi in contracts)
				{
					filesForEpisode.Add(new VideoDetailedVM(fi));
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private List<VideoDetailedVM> filesForEpisode = null;
		public List<VideoDetailedVM> FilesForEpisode
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

		public List<AniDBReleaseGroupVM> ReleaseGroups
		{
			get
			{
				List<AniDBReleaseGroupVM> relgrps = new List<AniDBReleaseGroupVM>();

				try
				{
					List<JMMServerBinary.Contract_AniDBReleaseGroup> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetMyReleaseGroupsForAniDBEpisode(this.AniDB_EpisodeID);

					foreach (JMMServerBinary.Contract_AniDBReleaseGroup rg in contracts)
					{
						relgrps.Add(new AniDBReleaseGroupVM(rg));
					}
				}
				catch (Exception ex)
				{
					Utils.ShowErrorMessage(ex);
				}
				return relgrps;
			}
		}

		public override List<MainListWrapper> GetDirectChildren()
		{
			List<MainListWrapper> childFiles = new List<MainListWrapper>();
			List<VideoDetailedVM> allFiles = FilesForEpisode;

			// check settings to see if we need to hide episodes
			childFiles.AddRange(allFiles);
			return childFiles;
		}
	}
}
