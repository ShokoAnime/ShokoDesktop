using System;
using System.Collections.Generic;

namespace JMMClient.ViewModel
{
    public class TraktDetails
    {
        public string TraktID { get; set; }

        public TraktDetails(string traktID)
        {
            TraktID = traktID;

            PopulateTraktEpisodes();
        }

        private Dictionary<int, Trakt_EpisodeVM> dictTraktEpisodes = null;
        public Dictionary<int, Trakt_EpisodeVM> DictTraktEpisodes
        {
            get
            {
                if (dictTraktEpisodes == null)
                {
                    try
                    {
                        if (TraktEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTraktEpisodes = new Dictionary<int, Trakt_EpisodeVM>();
                            // create a dictionary of absolute episode numbers for Trakt episodes
                            // sort by season and episode number
                            // ignore season 0, which is used for specials
                            List<Trakt_EpisodeVM> eps = TraktEpisodes;


                            int i = 1;
                            foreach (Trakt_EpisodeVM ep in eps)
                            {
                                dictTraktEpisodes[i] = ep;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTraktEpisodes;
            }
        }

        private Dictionary<int, int> dictTraktSeasons = null;
        public Dictionary<int, int> DictTraktSeasons
        {
            get
            {
                if (dictTraktSeasons == null)
                {
                    try
                    {
                        if (TraktEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTraktSeasons = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<Trakt_EpisodeVM> eps = TraktEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (Trakt_EpisodeVM ep in eps)
                            {
                                if (ep.Season != lastSeason)
                                    dictTraktSeasons[ep.Season] = i;

                                lastSeason = ep.Season;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got Trakt Seasons in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTraktSeasons;
            }
        }

        private Dictionary<int, int> dictTraktSeasonsSpecials = null;
        public Dictionary<int, int> DictTraktSeasonsSpecials
        {
            get
            {
                if (dictTraktSeasonsSpecials == null)
                {
                    try
                    {
                        if (TraktEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTraktSeasonsSpecials = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<Trakt_EpisodeVM> eps = TraktEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (Trakt_EpisodeVM ep in eps)
                            {
                                if (ep.Season > 0) continue;

                                // TODO check if the Trakt API supports 'AirsBeforeSeason' for specials

                                int thisSeason = 0;
                                /*
								if (ep.AirsBeforeSeason.HasValue) thisSeason = ep.AirsBeforeSeason.Value;
								if (ep.AirsAfterSeason.HasValue) thisSeason = ep.AirsAfterSeason.Value;
                                */

                                if (thisSeason != lastSeason)
                                    dictTraktSeasonsSpecials[thisSeason] = i;

                                lastSeason = thisSeason;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got Trakt Seasons in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTraktSeasonsSpecials;
            }
        }

        private void PopulateTraktEpisodes()
        {
            try
            {

                traktEpisodes = new List<Trakt_EpisodeVM>();
                List<JMMServerBinary.Contract_Trakt_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktEpisodesByTraktID(TraktID);

                foreach (JMMServerBinary.Contract_Trakt_Episode episode in eps)
                    traktEpisodes.Add(new Trakt_EpisodeVM(episode));

                if (traktEpisodes.Count > 0)
                {
                    List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
                    sortCriteria.Add(new SortPropOrFieldAndDirection("Season", false, SortType.eInteger));
                    sortCriteria.Add(new SortPropOrFieldAndDirection("EpisodeNumber", false, SortType.eInteger));
                    traktEpisodes = Sorting.MultiSort<Trakt_EpisodeVM>(traktEpisodes, sortCriteria);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private List<Trakt_EpisodeVM> traktEpisodes = null;
        public List<Trakt_EpisodeVM> TraktEpisodes
        {
            get
            {
                if (traktEpisodes == null)
                {
                    PopulateTraktEpisodes();
                }
                return traktEpisodes;
            }
        }
    }
}
