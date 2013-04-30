using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class TvDBSummary
	{
		public int AnimeID { get; set; }

		// TvDB ID
		public Dictionary<int, TvDBDetails> TvDetails = new Dictionary<int, TvDBDetails>();

		// All the TvDB cross refs for this anime
		private List<CrossRef_AniDB_TvDBVMV2> crossRefTvDBV2 = null;
		public List<CrossRef_AniDB_TvDBVMV2> CrossRefTvDBV2
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
				List<JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2> contract = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefV2(this.AnimeID);
				if (contract != null)
				{
					crossRefTvDBV2 = new List<CrossRef_AniDB_TvDBVMV2>();
					foreach (JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2 x in contract)
						crossRefTvDBV2.Add(new CrossRef_AniDB_TvDBVMV2(x));
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		// All the episode overrides for this anime
		private List<CrossRef_AniDB_TvDBEpisodeVM> crossRefTvDBEpisodes = null;
		public List<CrossRef_AniDB_TvDBEpisodeVM> CrossRefTvDBEpisodes
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

		private Dictionary<int, int> dictTvDBCrossRefEpisodes = null;
		public Dictionary<int, int> DictTvDBCrossRefEpisodes
		{
			get
			{
				if (dictTvDBCrossRefEpisodes == null)
				{
					dictTvDBCrossRefEpisodes = new Dictionary<int, int>();
					foreach (CrossRef_AniDB_TvDBEpisodeVM xrefEp in CrossRefTvDBEpisodes)
						dictTvDBCrossRefEpisodes[xrefEp.AniDBEpisodeID] = xrefEp.TvDBEpisodeID;
				}
				return dictTvDBCrossRefEpisodes;
			}
		}

		// All the episodes regardless of which cross ref they come from 
		private Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = null;
		public Dictionary<int, TvDB_EpisodeVM> DictTvDBEpisodes
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
					dictTvDBEpisodes = new Dictionary<int, TvDB_EpisodeVM>();
					foreach (TvDBDetails det in TvDetails.Values)
					{
						if (det != null)
						{
							
							// create a dictionary of absolute episode numbers for tvdb episodes
							// sort by season and episode number
							// ignore season 0, which is used for specials
							List<TvDB_EpisodeVM> eps = det.TvDBEpisodes;

							int i = 1;
							foreach (TvDB_EpisodeVM ep in eps)
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
				crossRefTvDBEpisodes = new List<CrossRef_AniDB_TvDBEpisodeVM>();
				List<JMMServerBinary.Contract_CrossRef_AniDB_TvDB_Episode> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefEpisode(this.AnimeID);
				if (contracts != null)
				{
					foreach (JMMServerBinary.Contract_CrossRef_AniDB_TvDB_Episode contract in contracts)
						crossRefTvDBEpisodes.Add(new CrossRef_AniDB_TvDBEpisodeVM(contract));
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

			foreach (CrossRef_AniDB_TvDBVMV2 xref in CrossRefTvDBV2)
			{
				TvDBDetails det = new TvDBDetails(xref.TvDBID);
				TvDetails[xref.TvDBID] = det;
			}
		}

	}
}
