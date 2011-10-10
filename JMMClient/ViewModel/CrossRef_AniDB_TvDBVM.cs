using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_TvDBVM
	{
		public int CrossRef_AniDB_TvDBID { get; set; }
		public int AnimeID { get; set; }
		public int TvDBID { get; set; }
		public int TvDBSeasonNumber { get; set; }
		public int CrossRefSource { get; set; }

		public CrossRef_AniDB_TvDBVM()
		{
		}

		public CrossRef_AniDB_TvDBVM(JMMServerBinary.Contract_CrossRef_AniDB_TvDB contract)
		{
			this.AnimeID = contract.AnimeID;
			this.TvDBID = contract.TvDBID;
			this.CrossRef_AniDB_TvDBID = contract.CrossRef_AniDB_TvDBID;
			this.TvDBSeasonNumber = contract.TvDBSeasonNumber;
			this.CrossRefSource = contract.CrossRefSource;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} Season # {2}", AnimeID, TvDBID, TvDBSeasonNumber);
		}
	}
}
