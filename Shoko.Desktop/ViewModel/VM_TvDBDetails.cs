using System;
using System.Collections.Generic;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_TvDBDetails
    {
        public int TvDBID { get; set; }

        public VM_TvDBDetails(int tvDBID)
        {
            TvDBID = tvDBID;

            PopulateTvDBEpisodes();
        }

        private Dictionary<int, VM_TvDB_Episode> dictTvDBEpisodes;
        public Dictionary<int, VM_TvDB_Episode> DictTvDBEpisodes
        {
            get
            {
                if (dictTvDBEpisodes == null)
                {
                    try
                    {
                        if (TvDBEpisodes != null)
                        {

                            dictTvDBEpisodes = new Dictionary<int, VM_TvDB_Episode>();
                            // create a dictionary of absolute episode numbers for tvdb episodes
                            // sort by season and episode number
                            // ignore season 0, which is used for specials

                            int i = 1;
                            foreach (VM_TvDB_Episode ep in TvDBEpisodes)
                            {
                                dictTvDBEpisodes[i] = ep;
                                i++;

                            }
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

        private Dictionary<int, int> dictTvDBSeasons;
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
          

                            dictTvDBSeasons = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            int i = 1;
                            int lastSeason = -999;
                            foreach (VM_TvDB_Episode ep in TvDBEpisodes)
                            {
                                if (ep.SeasonNumber != lastSeason)
                                    dictTvDBSeasons[ep.SeasonNumber] = i;

                                lastSeason = ep.SeasonNumber;
                                i++;

                            }

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

        private Dictionary<int, int> dictTvDBSeasonsSpecials;
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
   

                            dictTvDBSeasonsSpecials = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season
                            int i = 1;
                            int lastSeason = -999;
                            foreach (VM_TvDB_Episode ep in TvDBEpisodes)
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

                tvDBEpisodes=VM_ShokoServer.Instance.ShokoServices.GetAllTvDBEpisodes(TvDBID).CastList<VM_TvDB_Episode>().OrderBy(a=>a.SeasonNumber).ThenBy(a=>a.EpisodeNumber).ToList();

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private List<VM_TvDB_Episode> tvDBEpisodes;
        public List<VM_TvDB_Episode> TvDBEpisodes
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
