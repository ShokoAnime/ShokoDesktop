using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
	public static class Constants
	{
		public static readonly string AssemblyName = @"JMMDesktop";

		public struct URLS
		{
			public static readonly string MAL_Series = @"http://myanimelist.net/anime/{0}";

			public static readonly string AniDB_File = @"http://anidb.net/perl-bin/animedb.pl?show=file&fid={0}";
			public static readonly string AniDB_Episode = @"http://anidb.net/perl-bin/animedb.pl?show=ep&eid={0}";
			public static readonly string AniDB_Series = @"http://anidb.net/perl-bin/animedb.pl?show=anime&aid={0}";
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
