using System;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Commons.Utils;
using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Helpers
{
    public class GroupSearchFilterHelper
    {
        public static bool EvaluateGroupTextSearch(VM_AnimeGroup_User grp, string filterText)
        {
            if (string.IsNullOrEmpty(filterText) || grp == null)
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

            // search the titles (romaji name, english names) etc from anidb
            if (grp.Stat_AllTitles != null && grp.Stat_AllTitles.Count > 0)
            {
                if (grp.Stat_AllTitles.SubContains(filterText))
                    return true;

                foreach (string title in grp.Stat_AllTitles)
                {
                    if (string.IsNullOrEmpty(title)) continue;
                    if (!Misc.FuzzyMatches(title, filterText)) continue;
                    return true;
                }
            }

            // check the tags
            if (grp.Stat_AllTags != null && grp.Stat_AllTags.Count > 0)
            {

                if (grp.Stat_AllTags.Contains(filterText, StringComparer.InvariantCultureIgnoreCase))
                    return true;
            }

            // check the custom tags
            if (grp.Stat_AllCustomTags != null && grp.Stat_AllCustomTags.Count > 0)
            {
                if (grp.Stat_AllCustomTags.Contains(filterText, StringComparer.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool EvaluateSeriesTextSearch(VM_AnimeSeries_User series, string filterText, SeriesSearchType searchType)
        {
            if (string.IsNullOrEmpty(filterText) || series == null)
                return true;

            if (!string.IsNullOrEmpty(series.SeriesNameOverride))
            {
                int index = series.SeriesNameOverride.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
                if (index > -1) return true;
            }

            return EvaluateAnimeTextSearch(series.AniDBAnime, filterText, searchType);
        }

        public static bool EvaluateSeriesTextSearch(VM_AnimeSeries_User series, string filterText)
        {
            return EvaluateSeriesTextSearch(series, filterText, SeriesSearchType.Everything);
        }

        public static bool EvaluateAnimeTextSearch(CL_AniDB_AnimeDetailed anime, string filterText, SeriesSearchType searchType)
        {
            if (string.IsNullOrEmpty(filterText) || anime == null)
                return true;

            // search the romaji name, english names etc from anidb
            if (anime.AnimeTitles.Any(a =>
                (a.Language.Equals("en") || a.Language.Equals("x-jat") ||
                 a.Language.Equals(VM_ShokoServer.Instance.LanguagePreference)) && a.Title.Contains(filterText)))
                return true;

            foreach (string title in anime.AnimeTitles.Where(a =>
                a.Language.Equals("en") || a.Language.Equals("x-jat") ||
                a.Language.Equals(VM_ShokoServer.Instance.LanguagePreference)).Select(a => a.Title))
            {
                if (string.IsNullOrEmpty(title)) continue;
                if (!Misc.FuzzyMatches(title, filterText)) continue;
                return true;
            }

            if (searchType == SeriesSearchType.Everything)
            {
                // check the tags
                if (anime.Tags.Select(a => a.TagName).Contains(filterText, StringComparer.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool EvaluateAnimeTextSearch(CL_AniDB_AnimeDetailed anime, string filterText)
        {
            return EvaluateAnimeTextSearch(anime, filterText, SeriesSearchType.Everything);
        }

        public static bool EvaluateAnimeTextSearch(VM_AniDB_Anime anime, string filterText, SeriesSearchType searchType)
        {
            if (string.IsNullOrEmpty(filterText) || anime == null)
                return true;

            // search the romaji name, english names etc from anidb
            if (anime.GetAllTitles().SubContains(filterText))
                return true;

            if (anime.GetAllTitles() != null)
            {
                foreach (string title in anime.GetAllTitles())
                {
                    if (string.IsNullOrEmpty(title)) continue;
                    if (!Misc.FuzzyMatches(title, filterText)) continue;
                    return true;
                }
            }

            if (searchType == SeriesSearchType.Everything)
            {
                // check the tags
                if (anime.GetAllTags().Contains(filterText, StringComparer.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool EvaluateAnimeTextSearch(VM_AniDB_Anime anime, string filterText)
        {
            return EvaluateAnimeTextSearch(anime, filterText, SeriesSearchType.Everything);
        }

        public static bool EvaluateGroupFilter(VM_GroupFilter gf, VM_AnimeGroup_User grp)
        {
            return gf.EvaluateGroupFilter(grp);
        }

        public static bool EvaluateGroupFilter(VM_GroupFilter gf, VM_AnimeSeries_User ser)
        {
            return gf.EvaluateGroupFilter(ser);
        }
    }
}
