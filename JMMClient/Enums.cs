using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
    public enum AzureLinkType
    {
        TvDB = 1,
        Trakt = 2,
        MAL = 3,
        MovieDB = 4,
    }

	public enum SeriesSearchType
	{
		TitleOnly = 0,
		Everything = 1
	}

    public enum CustomTagCrossRefType
    {
        Anime = 1,
        Group = 2
    }

	public enum AiringState
	{
		All = 0,
		StillAiring = 1,
		FinishedAiring = 2,
	}

	public enum AniDBRecommendationType
	{
		ForFans = 1,
		Recommended = 2,
		MustSee = 3,
	}

	public enum AniDBFileDeleteType
	{
		Delete = 0,
        DeleteLocalOnly = 1,
        MarkDeleted = 2,
        MarkExternalStorage = 3,
        MarkUnknown = 4
	}

	public enum RatingCollectionState
	{
		All = 0,
		InMyCollection = 1,
		AllEpisodesInMyCollection = 2,
		NotInMyCollection = 3
	}

	public enum RatingWatchedState
	{
		All = 0,
		AllEpisodesWatched = 1,
		NotWatched = 2
	}

	public enum RatingVotedState
	{
		All = 0,
		Voted = 1,
		NotVoted = 2
	}

	public enum ImageEntityType
	{
		AniDB_Cover = 1, // use AnimeID
		AniDB_Character = 2, // use CharID
		AniDB_Creator = 3, // use CreatorID
		TvDB_Banner = 4, // use TvDB Banner ID
		TvDB_Cover = 5, // use TvDB Cover ID
		TvDB_Episode = 6, // use TvDB Episode ID
		TvDB_FanArt = 7, // use TvDB FanArt ID
		MovieDB_FanArt = 8,
		MovieDB_Poster = 9,
		Trakt_Poster = 10,
		Trakt_Fanart = 11,
		Trakt_Episode = 12,
		Trakt_Friend = 13,
		Trakt_ActivityScrobble = 14,
		Trakt_CommentUser = 15,
		Trakt_WatchedEpisode = 16
	}

	public enum AutoFileSubsequentType
	{
		PreviousGroup = 0,
		BestQuality = 1
	}

	public enum ImageDownloadEventType
	{
		Started = 1,
		Complete = 2
	}

	public enum EpisodeType
	{
		Episode = 1,
		Credits = 2,
		Special = 3,
		Trailer = 4,
		Parody = 5,
		Other = 6
	}

	public enum enAnimeType
	{
		Movie = 0,
		OVA = 1,
		TVSeries = 2,
		TVSpecial = 3,
		Web = 4,
		Other = 5
	}

	public enum WatchedStatus
	{
		All = 1,
		Unwatched = 2,
		Watched = 3
	}

	public enum CrossrefSource
	{
		AniDB = 1,
		User = 2
	}

	public enum AniDBVoteType
	{
		Anime = 1,
		AnimeTemp = 2,
		Group = 3,
		Episode = 4
	}

	public class WatchedStatusContainer
	{
		public WatchedStatus WatchedStatus { get; set; }
		public string WatchedStatusDescription { get; set; }

		public WatchedStatusContainer()
		{
		}

		public WatchedStatusContainer(WatchedStatus status, string desc)
		{
			WatchedStatus = status;
			WatchedStatusDescription = desc;
		}

		public static List<WatchedStatusContainer> GetAll()
		{
			List<WatchedStatusContainer> statuses = new List<WatchedStatusContainer>();
			statuses.Add(new WatchedStatusContainer(WatchedStatus.All, JMMClient.Properties.Resources.Episodes_Watched_All));
			statuses.Add(new WatchedStatusContainer(WatchedStatus.Unwatched, JMMClient.Properties.Resources.Episodes_Watched_Unwatched));
			statuses.Add(new WatchedStatusContainer(WatchedStatus.Watched, JMMClient.Properties.Resources.Episodes_Watched_Watched));
			return statuses;
		}
	}

	public enum AvailableEpisodeType
	{
		All = 1,
		Available = 2,
		NoFiles = 3
	}

	public enum AnimeGroupSortMethod 
	{ 
		SortName = 0, 
		IsFave = 1 
	}

	public enum SortDirection
	{
		Ascending = 1,
		Descending = 2
	}

	public enum GroupFilterConditionType
	{
		CompletedSeries = 1,
		MissingEpisodes = 2,
		HasUnwatchedEpisodes = 3,
		AllEpisodesWatched = 4,
		UserVoted = 5,
		Tag = 6,
		AirDate = 7,
		Studio = 8,
		AssignedTvDBInfo = 9,
		ReleaseGroup = 11, 
		AnimeType = 12,
		VideoQuality = 13,
		Favourite = 14,
		AnimeGroup = 15,
		AniDBRating = 16,
		UserRating = 17,
		SeriesCreatedDate = 18,
		EpisodeAddedDate = 19,
		EpisodeWatchedDate = 20,
		FinishedAiring = 21,
		MissingEpisodesCollecting = 22,
		AudioLanguage = 23,
		SubtitleLanguage = 24,
		AssignedTvDBOrMovieDBInfo = 25,
		AssignedMovieDBInfo = 26,
		UserVotedAny = 27,
		HasWatchedEpisodes = 28,
		AssignedMALInfo = 29,
		EpisodeCount = 30,
        CustomTags = 31
	}

	public enum GroupFilterOperator
	{
		Include = 1,
		Exclude = 2,
		GreaterThan = 3,
		LessThan = 4,
		Equals = 5,
		NotEquals = 6,
		In = 7,
		NotIn = 8,
		LastXDays = 9,
		InAllEpisodes = 10,
		NotInAllEpisodes = 11
	}

	public enum GroupFilterSorting
	{
		SeriesAddedDate = 1,
		EpisodeAddedDate = 2,
		EpisodeAirDate = 3,
		EpisodeWatchedDate = 4,
		GroupName = 5,
		Year = 6,
		SeriesCount = 7,
		UnwatchedEpisodeCount = 8,
		MissingEpisodeCount = 9,
		UserRating = 10,
		AniDBRating = 11,
		SortName = 12
	}

	public enum GroupFilterSortDirection
	{
		Asc = 1,
		Desc = 2
	}

	public enum GroupFilterBaseCondition
	{
		Include = 1,
		Exclude = 2
	}

	public enum EpisodeDisplayStyle
	{
		Always = 1,
		InExpanded = 2,
		Never = 3
	}

	public class AvailableEpisodeTypeContainer
	{
		public AvailableEpisodeType AvailableEpisodeType { get; set; }
		public string AvailableEpisodeTypeDescription { get; set; }

		public AvailableEpisodeTypeContainer()
		{
		}

		public AvailableEpisodeTypeContainer(AvailableEpisodeType eptype, string desc)
		{
			AvailableEpisodeType = eptype;
			AvailableEpisodeTypeDescription = desc;
		}

		public static List<AvailableEpisodeTypeContainer> GetAll()
		{
			List<AvailableEpisodeTypeContainer> eptypes = new List<AvailableEpisodeTypeContainer>();
			eptypes.Add(new AvailableEpisodeTypeContainer(AvailableEpisodeType.All, JMMClient.Properties.Resources.Episodes_AvAll));
			eptypes.Add(new AvailableEpisodeTypeContainer(AvailableEpisodeType.Available, JMMClient.Properties.Resources.Episodes_AvOnly));
			eptypes.Add(new AvailableEpisodeTypeContainer(AvailableEpisodeType.NoFiles, JMMClient.Properties.Resources.Episodes_AvMissing));
			return eptypes;
		}
	}

	public enum CrossRefType
	{
		MovieDB = 1,
		MyAnimeList = 2,
		AnimePlanet = 3,
		BakaBT = 4,
		TraktTV = 5,
		AnimeNano = 6,
		CrunchRoll = 7,
		Konachan = 8
	}

	public enum ImageSizeType
	{
		Poster = 1,
		Fanart = 2,
		WideBanner = 3
	}

	public enum SeriesWidgets
	{
		Categories = 1,
		Titles = 2,
		FileSummary = 3,
		TvDBLinks = 4,
		PlayNextEpisode = 5,
		Tags = 6,
        CustomTags = 7
	}

	public enum DashboardWidgets
	{
		WatchNextEpisode = 1,
		SeriesMissingEpisodes = 2,
		MiniCalendar = 3,
		RecommendationsWatch = 4,
		RecommendationsDownload = 5,
		TraktFriends = 6,
		RecentlyWatchedEpisode = 7,
		RecentAdditions = 8
	}

	public enum DashWatchNextStyle
	{
		Simple = 1,
		Detailed = 2
	}

	public enum DashboardType
	{
		Normal = 1,
		Metro = 2
	}

	public enum DashboardMetroImageType
	{
		Fanart = 1,
		Posters = 2
	}

	public enum DashboardMetroProcessType
	{
		TraktActivity = 1,
		ContinueWatching = 2,
		RandomSeries = 3,
		NewEpisodes = 4
	}

	public enum MetroViews
	{
		MainNormal = 1,
		MainMetro = 2,
		ContinueWatching = 3
	}

	public enum ScheduledUpdateFrequency
	{
        Never = 1,
        HoursSix = 2,
        HoursTwelve = 3,
        Daily = 4,
        WeekOne = 5,
        MonthOne = 6
    }

	public enum RecommendationType
	{
		Watch = 1,
		Download = 2
	}

	public enum IgnoreAnimeType
	{
		RecWatch = 1,
		RecDownload = 2
	}

	public enum DataSourceType
	{
		AniDB = 1,
		TheTvDB = 2
	}

	public enum TraktActivityAction
	{
		Scrobble = 1,
		Comment = 2
	}

	public enum TraktActivityType
	{
		Episode = 1,
		Show = 2
	}

	public enum PlaylistItemType
	{
		Episode = 1,
		AnimeSeries = 2
	}

	public enum PlaylistPlayOrder
	{
		Sequential = 1,
		Random = 2
	}

	public enum RandomSeriesEpisodeLevel
	{
		All = 1,
		GroupFilter = 2,
		Group = 3,
		Series = 4
	}

	public enum RecentAdditionsType
	{
		Episode = 1,
		Series = 2
	}

	public enum TorrentFilePriority
	{
		DontDownload = 0,
		Low = 1,
		Medium = 2,
		High = 3
	}

	public enum TorrentOriginator
	{
		Manual = 0,
		Series = 1,
		Episode = 2
	}

	public enum TorrentDownloadStatus
	{
		Ongoing = 0,
		Complete = 1
	}

	public enum TorrentSourceType
	{
		TokyoToshokanAnime = 1,
		TokyoToshokanAll = 2,
		BakaBT = 3,
		Nyaa = 4,
		AnimeSuki = 5,
		AnimeBytes = 6,
        Sukebei = 7
    }

	public enum DownloadSearchType
	{
		Episode = 1,
		Series = 2,
		Manual = 3
	}

	public enum FileSearchCriteria
	{
		Name = 1,
		Size = 2,
		LastOneHundred = 3,
		ED2KHash = 4
	}

	public class EnumTranslator
	{
		public static string EpisodeTypeTranslated(EpisodeType epType)
		{
			switch (epType)
			{
				case EpisodeType.Credits:
					return JMMClient.Properties.Resources.EpisodeType_Credits;
				case EpisodeType.Episode:
					return JMMClient.Properties.Resources.EpisodeType_Normal;
				case EpisodeType.Other:
					return JMMClient.Properties.Resources.EpisodeType_Other;
				case EpisodeType.Parody:
					return JMMClient.Properties.Resources.EpisodeType_Parody;
				case EpisodeType.Special:
					return JMMClient.Properties.Resources.EpisodeType_Specials;
				case EpisodeType.Trailer:
					return JMMClient.Properties.Resources.EpisodeType_Trailer;
				default:
					return JMMClient.Properties.Resources.EpisodeType_Normal;

			}
		}

		public static EpisodeType EpisodeTypeTranslatedReverse(string epType)
		{
			if (epType == JMMClient.Properties.Resources.EpisodeType_Credits) return EpisodeType.Credits;
			if (epType == JMMClient.Properties.Resources.EpisodeType_Normal) return EpisodeType.Episode;
			if (epType == JMMClient.Properties.Resources.EpisodeType_Other) return EpisodeType.Other;
			if (epType == JMMClient.Properties.Resources.EpisodeType_Parody) return EpisodeType.Parody;
			if (epType == JMMClient.Properties.Resources.EpisodeType_Trailer) return EpisodeType.Trailer;
			if (epType == JMMClient.Properties.Resources.EpisodeType_Specials) return EpisodeType.Special;

			return EpisodeType.Episode;
		}

		public static string TorrentSourceTranslated(TorrentSourceType tsType)
		{
			switch (tsType)
			{
				case TorrentSourceType.TokyoToshokanAnime: return "Tokyo Toshokan (Anime)";
				case TorrentSourceType.TokyoToshokanAll: return "Tokyo Toshokan (All)";
				case TorrentSourceType.BakaBT: return "BakaBT";
				case TorrentSourceType.Nyaa: return "Nyaa";
                case TorrentSourceType.Sukebei: return "Sukebei Nyaa";
                case TorrentSourceType.AnimeSuki: return "Anime Suki";
				case TorrentSourceType.AnimeBytes: return "Anime Byt.es";
				default: return "Tokyo Toshokan (Anime)";
			}
		}

		public static string TorrentSourceTranslatedShort(TorrentSourceType tsType)
		{
			switch (tsType)
			{
				case TorrentSourceType.TokyoToshokanAnime: return "TT";
				case TorrentSourceType.TokyoToshokanAll: return "TT";
				case TorrentSourceType.BakaBT: return "BakaBT";
				case TorrentSourceType.Nyaa: return "Nyaa";
                case TorrentSourceType.Sukebei: return "Suke Nyaa";
                case TorrentSourceType.AnimeSuki: return "Suki";
				case TorrentSourceType.AnimeBytes: return "AByt.es";
				default: return "TT";
			}
		}

		public static TorrentSourceType TorrentSourceTranslatedReverse(string tsType)
		{
			if (tsType == "Tokyo Toshokan (Anime)") return TorrentSourceType.TokyoToshokanAnime;
			if (tsType == "Tokyo Toshokan (All)") return TorrentSourceType.TokyoToshokanAll;
			if (tsType == "BakaBT") return TorrentSourceType.BakaBT;
			if (tsType == "Nyaa") return TorrentSourceType.Nyaa;
            if (tsType == "Sukebei Nyaa") return TorrentSourceType.Sukebei;
            if (tsType == "Anime Suki") return TorrentSourceType.AnimeSuki;
			if (tsType == "Anime Byt.es") return TorrentSourceType.AnimeBytes;

			return TorrentSourceType.TokyoToshokanAnime;
		}
	}
}
