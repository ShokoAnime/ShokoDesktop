using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_MALResultVM
	{
		public int AnimeID { get; set; }
		public int MALID { get; set; }
		public int CrossRefSource { get; set; }
		public string MALTitle { get; set; }

		public string SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.MAL_Series, MALID);
			}
		}

		public CrossRef_AniDB_MALResultVM()
		{
		}

		public CrossRef_AniDB_MALResultVM(JMMServerBinary.Contract_CrossRef_AniDB_MALResult contract)
		{
			this.AnimeID = contract.AnimeID;
			this.MALID = contract.MALID;
			this.CrossRefSource = contract.CrossRefSource;
			this.MALTitle = contract.MALTitle;
			
		}

		public override string ToString()
		{
			return string.Format("{0} --- {1}", MALID, MALTitle);
		}
	}
}
