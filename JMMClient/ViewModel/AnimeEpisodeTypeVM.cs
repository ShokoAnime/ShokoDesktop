using System;
using System.Collections.Generic;

namespace JMMClient.ViewModel
{
    public class AnimeEpisodeTypeVM : MainListWrapper, IComparable<AnimeEpisodeTypeVM>
    {
        public enum SortMethod { EpisodeTypeDescription = 0, EpisodeType };
        public static SortMethod SortType { get; set; }

        public EpisodeType EpisodeType { get; set; }
        public string EpisodeTypeDescription { get; set; }
        AnimeSeriesVM AnimeSeries { get; set; }

        public AnimeEpisodeTypeVM()
        {

        }

        public string EpisodeSummary
        {
            get
            {
                return string.Format("{0} Episodes", GetDirectChildren().Count);
            }
        }

        public AnimeEpisodeTypeVM(AnimeSeriesVM series, AnimeEpisodeVM ep)
        {
            AnimeSeries = series;
            EpisodeType = (EpisodeType)ep.EpisodeType;
            EpisodeTypeDescription = AnimeEpisodeTypeVM.EpisodeTypeTranslated(EpisodeType);
        }

        public int CompareTo(AnimeEpisodeTypeVM obj)
        {
            switch (SortType)
            {
                case SortMethod.EpisodeTypeDescription:
                    return EpisodeTypeDescription.CompareTo(obj.EpisodeTypeDescription);

                case SortMethod.EpisodeType:
                    return EpisodeType.CompareTo(obj.EpisodeType);
            }

            return 0;
        }

        public override List<MainListWrapper> GetDirectChildren()
        {
            List<MainListWrapper> eps = new List<MainListWrapper>();
            List<AnimeEpisodeVM> allEps = new List<AnimeEpisodeVM>();
            try
            {

                foreach (AnimeEpisodeVM epvm in AnimeSeries.AllEpisodes)
                {
                    if (epvm.EpisodeType == (int)this.EpisodeType)
                        allEps.Add(epvm);
                }

                // check settings to see if we need to hide episodes

                AnimeEpisodeVM.SortType = AnimeEpisodeVM.SortMethod.EpisodeNumber;
                allEps.Sort();
                eps.AddRange(allEps);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            return eps;
        }

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
    }
}
