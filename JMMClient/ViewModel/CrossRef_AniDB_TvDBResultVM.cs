using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_TvDBResultVM
	{
		public int AnimeID { get; set; }
		public int TvDBID { get; set; }
		public int TvDBSeasonNumber { get; set; }
		public int AdminApproved { get; set; }
		public string SeriesName { get; set; }

		public CrossRef_AniDB_TvDBResultVM()
		{
		}

		public CrossRef_AniDB_TvDBResultVM(JMMServerBinary.Contract_CrossRef_AniDB_TvDBResult contract)
		{
			this.AnimeID = contract.AnimeID;
			this.TvDBID = contract.TvDBID;
			this.TvDBSeasonNumber = contract.TvDBSeasonNumber;
			this.AdminApproved = contract.AdminApproved;
			this.SeriesName = contract.SeriesName;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} --- {2} Season # {3}", AnimeID, SeriesName, TvDBID, TvDBSeasonNumber);
		}
	}
}
