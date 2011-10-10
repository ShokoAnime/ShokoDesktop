using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_TraktVM
	{
		public int CrossRef_AniDB_TraktID { get; set; }
		public int AnimeID { get; set; }
		public string TraktID { get; set; }
		public int TraktSeasonNumber { get; set; }
		public int CrossRefSource { get; set; }

		public CrossRef_AniDB_TraktVM()
		{
		}

		public CrossRef_AniDB_TraktVM(JMMServerBinary.Contract_CrossRef_AniDB_Trakt contract)
		{
			this.CrossRef_AniDB_TraktID = contract.CrossRef_AniDB_TraktID;
			this.AnimeID = contract.AnimeID;
			this.TraktID = contract.TraktID;
			this.TraktSeasonNumber = contract.TraktSeasonNumber;
			this.CrossRefSource = contract.CrossRefSource;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} Season # {2}", AnimeID, TraktID, TraktSeasonNumber);
		}
	}
}
