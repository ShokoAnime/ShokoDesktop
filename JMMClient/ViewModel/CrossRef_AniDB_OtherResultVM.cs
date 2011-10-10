using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_OtherResultVM
	{
		public int AnimeID { get; set; }
		public string CrossRefID { get; set; }

		public CrossRef_AniDB_OtherResultVM()
		{
		}

		public CrossRef_AniDB_OtherResultVM(JMMServerBinary.Contract_CrossRef_AniDB_OtherResult contract)
		{
			this.AnimeID = contract.AnimeID;
			this.CrossRefID = contract.CrossRefID;
			
		}

		public override string ToString()
		{
			return string.Format("{0} = {1}", AnimeID, CrossRefID);
		}
	}
}
