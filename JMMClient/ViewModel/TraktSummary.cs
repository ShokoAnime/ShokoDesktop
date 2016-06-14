using System;
using System.Collections.Generic;

namespace JMMClient.ViewModel
{
    public class TraktSummary
    {
        public int AnimeID { get; set; }

        // Trakt ID
        public Dictionary<string, TraktDetails> traktDetails = new Dictionary<string, TraktDetails>();

        // All the Trakt cross refs for this anime
        private List<CrossRef_AniDB_TraktVMV2> crossRefTraktV2 = null;
        public List<CrossRef_AniDB_TraktVMV2> CrossRefTraktV2
        {
            get
            {
                if (crossRefTraktV2 == null)
                {
                    PopulateCrossRefs();
                }
                return crossRefTraktV2;
            }
        }

        private void PopulateCrossRefs()
        {
            try
            {
                List<JMMServerBinary.Contract_CrossRef_AniDB_TraktV2> contract = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefV2(this.AnimeID);
                if (contract != null)
                {
                    crossRefTraktV2 = new List<CrossRef_AniDB_TraktVMV2>();
                    foreach (JMMServerBinary.Contract_CrossRef_AniDB_TraktV2 xref in contract)
                        crossRefTraktV2.Add(new CrossRef_AniDB_TraktVMV2(xref));
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        // All the episode overrides for this anime
        private List<CrossRef_AniDB_TraktEpisodeVM> crossRefTraktEpisodes = null;
        public List<CrossRef_AniDB_TraktEpisodeVM> CrossRefTraktEpisodes
        {
            get
            {
                if (crossRefTraktEpisodes == null)
                {
                    PopulateCrossRefsEpisodes();
                }
                return crossRefTraktEpisodes;
            }
        }

        private Dictionary<int, string> dictTraktCrossRefEpisodes = null;
        public Dictionary<int, string> DictTraktCrossRefEpisodes
        {
            get
            {
                if (dictTraktCrossRefEpisodes == null)
                {
                    dictTraktCrossRefEpisodes = new Dictionary<int, string>();
                    foreach (CrossRef_AniDB_TraktEpisodeVM xrefEp in CrossRefTraktEpisodes)
                        dictTraktCrossRefEpisodes[xrefEp.AniDBEpisodeID] = xrefEp.TraktID;
                }
                return dictTraktCrossRefEpisodes;
            }
        }

        // All the episodes regardless of which cross ref they come from 
        private Dictionary<int, Trakt_EpisodeVM> dictTraktEpisodes = null;
        public Dictionary<int, Trakt_EpisodeVM> DictTraktEpisodes
        {
            get
            {
                if (dictTraktEpisodes == null)
                {
                    PopulateDictTraktEpisodes();
                }
                return dictTraktEpisodes;
            }
        }

        private void PopulateDictTraktEpisodes()
        {
            try
            {
                dictTraktEpisodes = new Dictionary<int, Trakt_EpisodeVM>();
                foreach (TraktDetails det in traktDetails.Values)
                {
                    if (det != null)
                    {

                        // create a dictionary of absolute episode numbers for Trakt episodes
                        // sort by season and episode number
                        // ignore season 0, which is used for specials
                        List<Trakt_EpisodeVM> eps = det.TraktEpisodes;

                        int i = 1;
                        foreach (Trakt_EpisodeVM ep in eps)
                        {
                            dictTraktEpisodes[i] = ep;
                            i++;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void PopulateCrossRefsEpisodes()
        {
            try
            {
                crossRefTraktEpisodes = new List<CrossRef_AniDB_TraktEpisodeVM>();
                List<JMMServerBinary.Contract_CrossRef_AniDB_Trakt_Episode> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefEpisode(this.AnimeID);
                if (contracts != null)
                {
                    foreach (JMMServerBinary.Contract_CrossRef_AniDB_Trakt_Episode contract in contracts)
                        crossRefTraktEpisodes.Add(new CrossRef_AniDB_TraktEpisodeVM(contract));
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void Populate(int animeID)
        {
            AnimeID = animeID;

            try
            {
                PopulateCrossRefs();
                PopulateCrossRefsEpisodes();
                PopulateTraktDetails();
                PopulateDictTraktEpisodes();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void PopulateTraktDetails()
        {
            if (CrossRefTraktV2 == null) return;

            // don't use the same show twice
            List<string> ids = new List<string>();

            foreach (CrossRef_AniDB_TraktVMV2 xref in CrossRefTraktV2)
            {
                if (!ids.Contains(xref.TraktID))
                {
                    ids.Add(xref.TraktID);
                    TraktDetails det = new TraktDetails(xref.TraktID);
                    traktDetails[xref.TraktID] = det;
                }
            }
        }
    }
}
