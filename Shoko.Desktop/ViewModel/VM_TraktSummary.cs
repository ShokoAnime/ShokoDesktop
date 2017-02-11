using System;
using System.Collections.Generic;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_TraktSummary
    {
        public int AnimeID { get; set; }

        // Trakt ID
        public Dictionary<string, VM_TraktDetails> traktDetails = new Dictionary<string, VM_TraktDetails>();

        // All the Trakt cross refs for this anime
        private List<VM_CrossRef_AniDB_TraktV2> crossRefTraktV2;
        public List<VM_CrossRef_AniDB_TraktV2> CrossRefTraktV2
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
                crossRefTraktV2 = VM_ShokoServer.Instance.ShokoServices.GetTraktCrossRefV2(AnimeID).CastList<VM_CrossRef_AniDB_TraktV2>();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        // All the episode overrides for this anime
        private List<CrossRef_AniDB_Trakt_Episode> crossRefTraktEpisodes;
        public List<CrossRef_AniDB_Trakt_Episode> CrossRefTraktEpisodes
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

        private Dictionary<int, string> dictTraktCrossRefEpisodes;
        public Dictionary<int, string> DictTraktCrossRefEpisodes
        {
            get
            {
                if (dictTraktCrossRefEpisodes == null)
                {
                    dictTraktCrossRefEpisodes = new Dictionary<int, string>();
                    foreach (CrossRef_AniDB_Trakt_Episode xrefEp in CrossRefTraktEpisodes)
                        dictTraktCrossRefEpisodes[xrefEp.AniDBEpisodeID] = xrefEp.TraktID;
                }
                return dictTraktCrossRefEpisodes;
            }
        }

        // All the episodes regardless of which cross ref they come from 
        private Dictionary<int, VM_Trakt_Episode> dictTraktEpisodes;
        public Dictionary<int, VM_Trakt_Episode> DictTraktEpisodes
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
                dictTraktEpisodes = new Dictionary<int, VM_Trakt_Episode>();
                foreach (VM_TraktDetails det in traktDetails.Values)
                {
                    if (det != null)
                    {

                        // create a dictionary of absolute episode numbers for Trakt episodes
                        // sort by season and episode number
                        // ignore season 0, which is used for specials
                        List<VM_Trakt_Episode> eps = det.TraktEpisodes;

                        int i = 1;
                        foreach (VM_Trakt_Episode ep in eps)
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
                crossRefTraktEpisodes = VM_ShokoServer.Instance.ShokoServices.GetTraktCrossRefEpisode(AnimeID);
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

            foreach (VM_CrossRef_AniDB_TraktV2 xref in CrossRefTraktV2)
            {
                if (!ids.Contains(xref.TraktID))
                {
                    ids.Add(xref.TraktID);
                    VM_TraktDetails det = new VM_TraktDetails(xref.TraktID);
                    traktDetails[xref.TraktID] = det;
                }
            }
        }
    }
}
