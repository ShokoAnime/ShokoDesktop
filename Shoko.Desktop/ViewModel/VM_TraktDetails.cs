using System;
using System.Collections.Generic;
using System.Linq;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_TraktDetails
    {
        public string TraktID { get; set; }

        public VM_TraktDetails(string traktID)
        {
            TraktID = traktID;

            PopulateTraktEpisodes();
        }

        private Dictionary<int, Trakt_Episode> dictTraktEpisodes;
        public Dictionary<int, Trakt_Episode> DictTraktEpisodes
        {
            get
            {
                if (dictTraktEpisodes == null)
                {
                    try
                    {
                        if (TraktEpisodes != null)
                        {
      

                            dictTraktEpisodes = new Dictionary<int, Trakt_Episode>();
                            // create a dictionary of absolute episode numbers for Trakt episodes
                            // sort by season and episode number
                            // ignore season 0, which is used for specials
                            List<Trakt_Episode> eps = TraktEpisodes;


                            int i = 1;
                            foreach (Trakt_Episode ep in eps)
                            {
                                dictTraktEpisodes[i] = ep;
                                i++;

                            }
            
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

        private Dictionary<int, int> dictTraktSeasons;
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
                          

                            dictTraktSeasons = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<Trakt_Episode> eps = TraktEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (Trakt_Episode ep in eps)
                            {
                                if (ep.Season != lastSeason)
                                    dictTraktSeasons[ep.Season] = i;

                                lastSeason = ep.Season;
                                i++;

                            }
                       
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

        private Dictionary<int, int> dictTraktSeasonsSpecials;
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
                

                            dictTraktSeasonsSpecials = new Dictionary<int, int>();
                            // create a dictionary of season numbers and the first episode for that season

                            List<Trakt_Episode> eps = TraktEpisodes;
                            int i = 1;
                            int lastSeason = -999;
                            foreach (Trakt_Episode ep in eps)
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

                traktEpisodes = VM_ShokoServer.Instance.ShokoServices.GetAllTraktEpisodesByTraktID(TraktID).OrderBy(a => a.Season).ThenBy(a => a.EpisodeNumber).ToList();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private List<Trakt_Episode> traktEpisodes;
        public List<Trakt_Episode> TraktEpisodes
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
