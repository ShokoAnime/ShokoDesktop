using System;
using System.Collections.Generic;

namespace JMMClient.ViewModel
{
    public class TvDBDetails
    {
        public int TvDBID { get; set; }

        public TvDBDetails(int tvDBID)
        {
            TvDBID = tvDBID;

            PopulateTvDBEpisodes();
        }

        private Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = null;
        public Dictionary<int, TvDB_EpisodeVM> DictTvDBEpisodes
        {
            get
            {
                if (dictTvDBEpisodes == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTvDBEpisodes = new Dictionary<int, TvDB_EpisodeVM>();
                            // create a dictionary of absolute episode numbers for tvdb episodes
                            // sort by season and episode number
                            // ignore season 0, which is used for specials
                            List<TvDB_EpisodeVM> eps = TvDBEpisodes;


                            int i = 1;
                            foreach (TvDB_EpisodeVM ep in eps)
                            {
                                dictTvDBEpisodes[i] = ep;
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
                return dictTvDBEpisodes;
            }
        }

        private Dictionary<int, int> dictTvDBSeasons = null;
        public Dictionary<int, int> DictTvDBSeasons
        {
            get
            {
                if (dictTvDBSeasons == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTvDBSeasons = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<TvDB_EpisodeVM> eps = TvDBEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (TvDB_EpisodeVM ep in eps)
                            {
                                if (ep.SeasonNumber != lastSeason)
                                    dictTvDBSeasons[ep.SeasonNumber] = i;

                                lastSeason = ep.SeasonNumber;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got TvDB Seasons in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTvDBSeasons;
            }
        }

        private Dictionary<int, int> dictTvDBSeasonsSpecials = null;
        public Dictionary<int, int> DictTvDBSeasonsSpecials
        {
            get
            {
                if (dictTvDBSeasonsSpecials == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {
                            DateTime start = DateTime.Now;

                            dictTvDBSeasonsSpecials = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<TvDB_EpisodeVM> eps = TvDBEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (TvDB_EpisodeVM ep in eps)
                            {
                                if (ep.SeasonNumber > 0) continue;

                                int thisSeason = 0;
                                if (ep.AirsBeforeSeason.HasValue) thisSeason = ep.AirsBeforeSeason.Value;
                                if (ep.AirsAfterSeason.HasValue) thisSeason = ep.AirsAfterSeason.Value;

                                if (thisSeason != lastSeason)
                                    dictTvDBSeasonsSpecials[thisSeason] = i;

                                lastSeason = thisSeason;
                                i++;

                            }
                            TimeSpan ts = DateTime.Now - start;
                            //logger.Trace("Got TvDB Seasons in {0} ms", ts.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowErrorMessage(ex);
                    }
                }
                return dictTvDBSeasonsSpecials;
            }
        }

        private void PopulateTvDBEpisodes()
        {
            try
            {

                tvDBEpisodes = new List<TvDB_EpisodeVM>();
                List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(TvDBID);

                foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
                    tvDBEpisodes.Add(new TvDB_EpisodeVM(episode));

                if (tvDBEpisodes.Count > 0)
                {
                    List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
                    sortCriteria.Add(new SortPropOrFieldAndDirection("SeasonNumber", false, SortType.eInteger));
                    sortCriteria.Add(new SortPropOrFieldAndDirection("EpisodeNumber", false, SortType.eInteger));
                    tvDBEpisodes = Sorting.MultiSort<TvDB_EpisodeVM>(tvDBEpisodes, sortCriteria);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private List<TvDB_EpisodeVM> tvDBEpisodes = null;
        public List<TvDB_EpisodeVM> TvDBEpisodes
        {
            get
            {
                if (tvDBEpisodes == null)
                {
                    PopulateTvDBEpisodes();
                }
                return tvDBEpisodes;
            }
        }
    }
}
