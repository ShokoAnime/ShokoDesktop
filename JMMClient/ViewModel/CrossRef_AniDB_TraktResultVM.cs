using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_TraktResultVM
	{
		public int AnimeID { get; set; }
		public string TraktID { get; set; }
		public int TraktSeasonNumber { get; set; }
		public int AdminApproved { get; set; }
		public string ShowName { get; set; }

		public CrossRef_AniDB_TraktResultVM()
		{
		}

		public CrossRef_AniDB_TraktResultVM(JMMServerBinary.Contract_CrossRef_AniDB_TraktResult contract)
		{
			this.AnimeID = contract.AnimeID;
			this.TraktID = contract.TraktID;
			this.TraktSeasonNumber = contract.TraktSeasonNumber;
			this.AdminApproved = contract.AdminApproved;
			this.ShowName = contract.ShowName;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} Season # {2}", AnimeID, TraktID, TraktSeasonNumber);
		}
	}
}
