using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
	public class GroupSearchFilterHelper
	{
		public static bool EvaluateGroupTextSearch(AnimeGroupVM grp, string filterText)
		{
			if (String.IsNullOrEmpty(filterText) || grp == null)
				return true;

			// do this so that when viewing sub groups they don't get filtered
			if (grp.AnimeGroupParentID.HasValue) return true;

			// get all possible names for the group

			// search the group name
			int index = grp.GroupName.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;

			// search the sort name
			index = grp.SortName.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;

			// check the categories
			if (grp.Stat_AllCategories != null)
			{
				index = grp.Stat_AllCategories.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
				if (index > -1) return true;
			}

			// search the titles (romaji name, english names) etc from anidb
			if (grp.Stat_AllTitles != null)
			{
				index = grp.Stat_AllTitles.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
				if (index > -1) return true;
			}

			return false;
		}

		public static bool EvaluateSeriesTextSearch(AnimeSeriesVM series, string filterText)
		{
			if (String.IsNullOrEmpty(filterText) || series == null)
				return true;

			if (!string.IsNullOrEmpty(series.SeriesNameOverride))
			{
				int index = series.SeriesNameOverride.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
				if (index > -1) return true;
			}

			return EvaluateAnimeTextSearch(series.AniDB_Anime, filterText);
		}

		public static bool EvaluateAnimeTextSearch(AniDB_AnimeVM anime, string filterText)
		{
			if (String.IsNullOrEmpty(filterText) || anime == null)
				return true;

			// search the romaji name, english names etc from anidb
			int index = anime.AllTitles.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;

			// check the categories
			index = anime.AllCategories.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;

			// check the tags
			index = anime.AllTags.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;


			return false;
		}

		public static bool EvaluateGroupFilter(GroupFilterVM gf, AnimeGroupVM grp)
		{
			return gf.EvaluateGroupFilter(grp);
		}

		public static bool EvaluateGroupFilter(GroupFilterVM gf, AnimeSeriesVM ser)
		{
			return gf.EvaluateGroupFilter(ser);
		}
	}
}
