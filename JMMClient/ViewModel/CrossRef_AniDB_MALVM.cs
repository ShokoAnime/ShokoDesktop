using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_MALVM
	{
		public int CrossRef_AniDB_MALID { get; set; }
		public int AnimeID { get; set; }
		public int MALID { get; set; }
		public string MALTitle { get; set; }
		public int CrossRefSource { get; set; }

		public CrossRef_AniDB_MALVM()
		{
		}

		public CrossRef_AniDB_MALVM(JMMServerBinary.Contract_CrossRef_AniDB_MAL contract)
		{
			this.CrossRef_AniDB_MALID = contract.CrossRef_AniDB_MALID;
			this.AnimeID = contract.AnimeID;
			this.MALID = contract.MALID;
			this.MALTitle = contract.MALTitle;
			this.CrossRefSource = contract.CrossRefSource;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} - {2}", AnimeID, MALID, MALTitle);
		}
	}
}
