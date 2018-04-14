using System;
using System.Collections.Generic;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_TvDBSummary
    {
        public int AnimeID { get; set; }

        // TvDB ID
        public Dictionary<int, VM_TvDBDetails> TvDetails = new Dictionary<int, VM_TvDBDetails>();

        // All the TvDB cross refs for this anime
        private List<VM_CrossRef_AniDB_TvDBV2> crossRefTvDBV2;
        public List<VM_CrossRef_AniDB_TvDBV2> CrossRefTvDBV2
        {
            get
            {
                if (crossRefTvDBV2 == null)
                {
                    PopulateCrossRefs();
                }
                return crossRefTvDBV2;
            }
        }

        private void PopulateCrossRefs()
        {
            try
            {
                crossRefTvDBV2 = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefV2(AnimeID).CastList<VM_CrossRef_AniDB_TvDBV2>();

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        // All the episode overrides for this anime
        private List<CrossRef_AniDB_TvDB_Episode_Override> crossRefTvDBEpisodes;
        public List<CrossRef_AniDB_TvDB_Episode_Override> CrossRefTvDBEpisodes
        {
            get
            {
                if (crossRefTvDBEpisodes == null)
                {
                    PopulateCrossRefsEpisodes();
                }
                return crossRefTvDBEpisodes;
            }
        }

        private Dictionary<int, int> dictTvDBCrossRefEpisodes;
        public Dictionary<int, int> DictTvDBCrossRefEpisodes
        {
            get
            {
                if (dictTvDBCrossRefEpisodes == null)
                {
                    dictTvDBCrossRefEpisodes = new Dictionary<int, int>();
                    foreach (CrossRef_AniDB_TvDB_Episode_Override xrefEp in CrossRefTvDBEpisodes)
                        dictTvDBCrossRefEpisodes[xrefEp.AniDBEpisodeID] = xrefEp.TvDBEpisodeID;
                }
                return dictTvDBCrossRefEpisodes;
            }
        }

        // All the episodes regardless of which cross ref they come from 
        private Dictionary<int, VM_TvDB_Episode> dictTvDBEpisodes;
        public Dictionary<int, VM_TvDB_Episode> DictTvDBEpisodes
        {
            get
            {
                if (dictTvDBEpisodes == null)
                {
                    PopulateDictTvDBEpisodes();
                }
                return dictTvDBEpisodes;
            }
        }

        private void PopulateDictTvDBEpisodes()
        {
            try
            {
                dictTvDBEpisodes = new Dictionary<int, VM_TvDB_Episode>();
                foreach (VM_TvDBDetails det in TvDetails.Values)
                {
                    if (det != null)
                    {

                        // create a dictionary of absolute episode numbers for tvdb episodes
                        // sort by season and episode number
                        // ignore season 0, which is used for specials
                        int i = 1;
                        foreach (VM_TvDB_Episode ep in det.TvDBEpisodes)
                        {
                            dictTvDBEpisodes[i] = ep;
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
                crossRefTvDBEpisodes = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefEpisode(AnimeID);
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
                PopulateTvDBDetails();
                PopulateDictTvDBEpisodes();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void PopulateTvDBDetails()
        {
            if (CrossRefTvDBV2 == null) return;

            foreach (VM_CrossRef_AniDB_TvDBV2 xref in CrossRefTvDBV2)
            {
                VM_TvDBDetails det = new VM_TvDBDetails(xref.TvDBID);
                TvDetails[xref.TvDBID] = det;
            }
        }

    }
}
