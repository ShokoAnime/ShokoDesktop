using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AniDB_VoteVM
	{
		public int EntityID { get; set; }
		public decimal VoteValue { get; set; }
		public int VoteType { get; set; }

		public AniDB_VoteVM()
		{
		}

		public AniDB_VoteVM(JMMServerBinary.Contract_AniDBVote contract)
		{
			this.EntityID = contract.EntityID;
			this.VoteValue = contract.VoteValue;
			this.VoteType = contract.VoteType;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", EntityID, VoteValue);
		}
	}
}
