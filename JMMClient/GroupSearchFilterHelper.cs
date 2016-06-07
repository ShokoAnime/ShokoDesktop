using System;

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

            // check the tags
            if (grp.Stat_AllTags != null)
            {
                index = grp.Stat_AllTags.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
                if (index > -1) return true;
            }

            // check the custom tags
            if (grp.Stat_AllCustomTags != null)
            {
                index = grp.Stat_AllCustomTags.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
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

        public static bool EvaluateSeriesTextSearch(AnimeSeriesVM series, string filterText, SeriesSearchType searchType)
        {
            if (String.IsNullOrEmpty(filterText) || series == null)
                return true;

            if (!string.IsNullOrEmpty(series.SeriesNameOverride))
            {
                int index = series.SeriesNameOverride.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
                if (index > -1) return true;
            }

            return EvaluateAnimeTextSearch(series.AniDB_Anime, filterText, searchType);
        }

        public static bool EvaluateSeriesTextSearch(AnimeSeriesVM series, string filterText)
        {
            return EvaluateSeriesTextSearch(series, filterText, SeriesSearchType.Everything);
        }

        public static bool EvaluateAnimeTextSearch(AniDB_AnimeVM anime, string filterText, SeriesSearchType searchType)
        {
            if (String.IsNullOrEmpty(filterText) || anime == null)
                return true;

            // search the romaji name, english names etc from anidb
            int index = anime.AllTitles.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
            if (index > -1) return true;

            if (searchType == SeriesSearchType.Everything)
            {
                // check the tags
                index = anime.AllTags.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
                if (index > -1) return true;
            }

            if (searchType == SeriesSearchType.Everything)
            {
                // check the tags
                index = anime.AllTags.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
                if (index > -1) return true;
            }


            return false;
        }

        public static bool EvaluateAnimeTextSearch(AniDB_AnimeVM anime, string filterText)
        {
            return EvaluateAnimeTextSearch(anime, filterText, SeriesSearchType.Everything);
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
