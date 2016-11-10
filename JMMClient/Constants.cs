namespace JMMClient
{
    public static class Constants
    {
        public static readonly int FlagLinkTvDB = 1;
        public static readonly int FlagLinkTrakt = 2;
        public static readonly int FlagLinkMAL = 4;
        public static readonly int FlagLinkMovieDB = 8;

        public struct CharacterType
        {
            public static readonly string MAIN = "main character in";
        }

        public struct DBLogType
        {
            public static readonly string APIAniDBHTTP = "AniDB HTTP";
            public static readonly string APIAniDBUDP = "AniDB UDP";
            public static readonly string APIAzureHTTP = "Cache HTTP";
        }

        // http://wiki.anidb.net/w/WebAOM#Move.2Frename_system
        public struct FileRenameTag_Name
        {
            public static readonly string AnimeNameRomaji = Properties.Resources.Rename_AnimeNameRomaji;
            public static readonly string AnimeNameKanji = Properties.Resources.Rename_AnimeNameKanji;
            public static readonly string AnimeNameEnglish = Properties.Resources.Rename_AnimeNameEnglish;
            public static readonly string EpisodeNameRomaji = Properties.Resources.Rename_EpisodeNameRomaji;
            public static readonly string EpisodeNameEnglish = Properties.Resources.Rename_EpisodeNameEnglish;
            public static readonly string EpisodeNumber = Properties.Resources.Rename_EpisodeNumber;
            public static readonly string GroupShortName = Properties.Resources.Rename_GroupShortName;
            public static readonly string GroupLongName = Properties.Resources.Rename_GroupLongName;
            public static readonly string ED2KLower = Properties.Resources.Rename_ED2KLower;
            public static readonly string ED2KUpper = Properties.Resources.Rename_ED2KUpper;
            public static readonly string CRCLower = Properties.Resources.Rename_CRCLower;
            public static readonly string CRCUpper = Properties.Resources.Rename_CRCUpper;
            public static readonly string FileVersion = Properties.Resources.Rename_FileVersion;
            public static readonly string Source = Properties.Resources.Rename_Source;
            public static readonly string Resolution = Properties.Resources.Rename_Resolution;
            public static readonly string Year = Properties.Resources.Rename_Year;
            public static readonly string Episodes = Properties.Resources.Rename_Episodes; // Total number of episodes
            public static readonly string Type = Properties.Resources.Rename_Type; // Type [unknown, TV, OVA, Movie, TV Special, Other, web]
            public static readonly string FileID = Properties.Resources.Rename_FileID;
            public static readonly string AnimeID = Properties.Resources.Rename_AnimeID;
            public static readonly string EpisodeID = Properties.Resources.Rename_EpisodeID;
            public static readonly string GroupID = Properties.Resources.Rename_GroupID;
            public static readonly string DubLanguage = Properties.Resources.Rename_DubLanguage;
            public static readonly string SubLanguage = Properties.Resources.Rename_SubLanguage;
            public static readonly string VideoCodec = Properties.Resources.Rename_VideoCodec; //tracks separated with '
            public static readonly string AudioCodec = Properties.Resources.Rename_AudioCodec; //tracks separated with '
            public static readonly string VideoBitDepth = Properties.Resources.Rename_VideoBitDepth; // 8bit, 10bit
            public static readonly string OriginalFileName = Properties.Resources.Rename_OriginalFileName; // The original file name as specified by the sub group
            public static readonly string Censored = Properties.Resources.Rename_Censored;
            public static readonly string Deprecated = Properties.Resources.Rename_Deprecated;
        }

        public struct FileRenameTag_Tag
        {
            public static readonly string AnimeNameRomaji = "%ann";
            public static readonly string AnimeNameKanji = "%kan";
            public static readonly string AnimeNameEnglish = "%eng";
            public static readonly string EpisodeNameRomaji = "%epn";
            //public static readonly string EpisodeNameKanji = "%epk";
            public static readonly string EpisodeNameEnglish = "%epr";
            public static readonly string EpisodeNumber = "%enr";
            public static readonly string GroupShortName = "%grp";
            public static readonly string GroupLongName = "%grl";
            public static readonly string ED2KLower = "%ed2";
            public static readonly string ED2KUpper = "%ED2";
            public static readonly string CRCLower = "%crc";
            public static readonly string CRCUpper = "%CRC";
            public static readonly string FileVersion = "%ver";
            public static readonly string Source = "%src";
            public static readonly string Resolution = "%res";
            public static readonly string Year = "%yea";
            public static readonly string Episodes = "%eps"; // Total number of episodes
            public static readonly string Type = "%typ"; // Type [unknown, TV, OVA, Movie, TV Special, Other, web]
            public static readonly string FileID = "%fid";
            public static readonly string AnimeID = "%aid";
            public static readonly string EpisodeID = "%eid";
            public static readonly string GroupID = "%gid";
            public static readonly string DubLanguage = "%dub";
            public static readonly string SubLanguage = "%sub";
            public static readonly string VideoCodec = "%vid"; //tracks separated with '
            public static readonly string AudioCodec = "%aud"; //tracks separated with '
            public static readonly string VideoBitDepth = "%bit"; // 8bit, 10bit
            public static readonly string OriginalFileName = "%sna"; // The original file name as specified by the sub group
            public static readonly string Censored = "%cen";
            public static readonly string Deprecated = "%dep";


            /*
			%md5 / %MD5	 md5 sum (lower/upper)
			%sha / %SHA	 sha1 sum (lower/upper)
			%inv	 Invalid crc string
			 * */
        }

        public struct FileRenameTest_Name
        {
            public static readonly string AnimeID = Properties.Resources.Rename_AnimeID;
            public static readonly string GroupID = Properties.Resources.Rename_GroupID;
            public static readonly string FileVersion = Properties.Resources.Rename_FileVersion;
            public static readonly string EpisodeNumber = Properties.Resources.Rename_EpisodeNumber;
            public static readonly string EpisodeCount = Properties.Resources.Rename_EpisodeCount;
            public static readonly string RipSource = Properties.Resources.Rename_RipSource;
            public static readonly string AnimeType = Properties.Resources.Rename_AnimeType;
            public static readonly string Year = Properties.Resources.Rename_Year;
            public static readonly string DubLanguage = Properties.Resources.Rename_DubLanguage;
            public static readonly string SubLanguage = Properties.Resources.Rename_SubLanguage;
            public static readonly string Codec = Properties.Resources.Rename_Codec;
            public static readonly string Tag = Properties.Resources.Rename_Tag;
            public static readonly string VideoBitDepth = Properties.Resources.Rename_VideoBitDepth;
            public static readonly string VideoResolutionWidth = Properties.Resources.Rename_VideoResolutionWidth;
            public static readonly string VideoResolutionHeight = Properties.Resources.Rename_VideoResolutionHeight;
            public static readonly string ManuallyLinked = Properties.Resources.Rename_ManuallyLinked;
            public static readonly string HasEpisodes = Properties.Resources.Rename_HasEpisodes;
        }

		public struct FileRenameTest_Test
		{
			public static readonly string AnimeID = "A()";
			public static readonly string GroupID = "G()";
			public static readonly string FileVersion = "F()";
			public static readonly string EpisodeNumber = "E()";
			public static readonly string EpisodeCount = "X()";
			public static readonly string RipSource = "R()";
			public static readonly string AnimeType = "T()";
			public static readonly string Year = "Y()";
			public static readonly string DubLanguage = "D()";
			public static readonly string SubLanguage = "S()";
			public static readonly string Codec = "C()";
			public static readonly string Tag = "I()";
			public static readonly string VideoBitDepth = "Z()";
			public static readonly string VideoResolutionWidth = "W()";
			public static readonly string VideoResolutionHeight = "U()";
			public static readonly string ManuallyLinked = "M()";
			public static readonly string HasEpisodes = "N()";
		}
		/*
		public struct StaticGF
		{
			public static readonly int All = -999;

			public static readonly int Predefined = -900;

			public static readonly int Predefined_Years = -901;
			public static readonly int Predefined_Tags = -902;

			public static readonly int Predefined_Years_Child = -921;
			public static readonly int Predefined_Tags_Child = -922;
		}
		*/
		public struct URLS
		{
			public static readonly string MAL_Series = @"http://myanimelist.net/anime/{0}";
			public static readonly string MAL_SeriesDiscussion = @"http://myanimelist.net/anime/{0}/{1}/forum";

            public static readonly string AniDB_File = @"http://anidb.net/perl-bin/animedb.pl?show=file&fid={0}";
            public static readonly string AniDB_Episode = @"http://anidb.net/perl-bin/animedb.pl?show=ep&eid={0}";
            public static readonly string AniDB_Series = @"http://anidb.net/perl-bin/animedb.pl?show=anime&aid={0}";
            public static readonly string AniDB_SeriesDiscussion = @"http://anidb.net/perl-bin/animedb.pl?show=threads&do=anime&id={0}";
            public static readonly string AniDB_ReleaseGroup = @"http://anidb.net/perl-bin/animedb.pl?show=group&gid={0}";
            public static readonly string AniDB_Images = @"http://img7.anidb.net/pics/anime/{0}";
            public static readonly string AniDB_Series_NewRelease = @"http://anidb.net/perl-bin/animedb.pl?show=addfilem&aid={0}";

            public static readonly string TvDB_Series = @"http://thetvdb.com/?tab=series&id={0}";
            //public static readonly string tvDBEpisodeURLPrefix = @"http://anidb.net/perl-bin/animedb.pl?show=ep&eid={0}";
            public static readonly string TvDB_Images = @"http://thetvdb.com/banners/{0}";

            public static readonly string MovieDB_Series = @"http://www.themoviedb.org/movie/{0}";
            public static readonly string Trakt_Series = @"http://trakt.tv/show/{0}";

            public static readonly string Blank = @"http://blank";


        }

        public struct AnimeTitleType
        {
            public static readonly string Main = "main";
            public static readonly string Official = "official";
            public static readonly string ShortName = "short";
            public static readonly string Synonym = "synonym";
        }
    }

    public static class Globals
    {
        public static System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.CurrentCulture;

    }
}
