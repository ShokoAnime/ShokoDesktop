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
            if (string.IsNullOrEmpty(filterText)) return true;

            if (grp == null)
                return false;

            // do this so that when viewing sub groups they don't get filtered
            if (grp.AnimeGroupParentID.HasValue) return true;

            // get all possible names for the group

            // search the group name
            if (grp.GroupName.FuzzyMatches(filterText)) return true;

            // search the sort name
            if (grp.SortName.FuzzyMatches(filterText)) return true;

            // search the titles (romaji name, english names) etc from anidb
            if (grp.AllAnimeSeries.Any(a => EvaluateSeriesTextSearch(a, filterText)))
                return true;

            return false;
        }

        public static bool EvaluateSeriesTextSearch(VM_AnimeSeries_User series, string filterText, SeriesSearchType searchType = SeriesSearchType.Everything)
        {
            if (string.IsNullOrEmpty(filterText)) return true;
            
            if (series == null) return false;

            if (!string.IsNullOrEmpty(series.SeriesNameOverride) && series.SeriesNameOverride.FuzzyMatches(filterText))
                return true;

            return EvaluateAnimeTextSearch(series.AniDBAnime, filterText, searchType);
        }

        public static bool EvaluateAnimeTextSearch(CL_AniDB_AnimeDetailed anime, string filterText, SeriesSearchType searchType)
        {
            if (string.IsNullOrEmpty(filterText)) return true;

            if (anime == null) return false;

            // search the romaji name, english names etc from anidb
            if (anime.AnimeTitles.Any(a =>
                (a.Language.Equals("en") || a.Language.Equals("x-jat") ||
                 a.Language.Equals(VM_ShokoServer.Instance.LanguagePreference)) && a.Title.FuzzyMatches(filterText)))
                return true;

            // check the tags
            if (searchType == SeriesSearchType.Everything && anime.Tags.Select(a => a.TagName).Any(a => a.FuzzyMatches(filterText)))
            {
                return true;
            }

            return false;
        }

        public static bool EvaluateAnimeTextSearch(CL_AniDB_AnimeDetailed anime, string filterText)
        {
            return EvaluateAnimeTextSearch(anime, filterText, SeriesSearchType.Everything);
        }

        public static bool EvaluateAnimeTextSearch(VM_AniDB_Anime anime, string filterText, SeriesSearchType searchType = SeriesSearchType.Everything)
        {
            if (string.IsNullOrEmpty(filterText)) return true;
            if (anime == null) return false;

            // search the romaji name, english names etc from anidb
            if (anime.GetAllTitles() != null)
            {
                foreach (string title in anime.GetAllTitles())
                {
                    if (title.FuzzyMatches(filterText)) return true;
                }
            }

            // check the tags
            if (searchType == SeriesSearchType.Everything && anime.GetAllTags().Contains(filterText, StringComparer.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
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
