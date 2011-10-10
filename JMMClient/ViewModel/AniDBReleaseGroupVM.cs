using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AniDBReleaseGroupVM
	{
		public int GroupID { get; set; }
		public string GroupName { get; set; }

		public AniDBReleaseGroupVM()
		{
		}

		public AniDBReleaseGroupVM(JMMServerBinary.Contract_AniDBReleaseGroup contract)
		{
			this.GroupID = contract.GroupID;
			this.GroupName = contract.GroupName;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", GroupID, GroupName);
		}
	}
}
