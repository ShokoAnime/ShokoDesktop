using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AnimeTagVM : IComparable<AnimeTagVM>
	{
		public int TagID { get; set; }
		public int Spoiler { get; set; }
		public int LocalSpoiler { get; set; }
		public int GlobalSpoiler { get; set; }
		public string TagName { get; set; }
		public int TagCount { get; set; }
		public string TagDescription { get; set; }
		public int Approval { get; set; }

		public AnimeTagVM()
		{
		}

		public AnimeTagVM(JMMServerBinary.Contract_AnimeTag contract)
		{
			this.Approval = contract.Approval;
			this.GlobalSpoiler = contract.GlobalSpoiler;
			this.LocalSpoiler = contract.LocalSpoiler;
			this.Spoiler = contract.Spoiler;
			this.TagCount = contract.TagCount;
			this.TagDescription = contract.TagDescription;
			this.TagID = contract.TagID;
			this.TagName = contract.TagName;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}) - {2}", TagName, Approval, TagDescription);
		}

		public int CompareTo(AnimeTagVM obj)
		{
			if (Approval > obj.Approval) return -1;
			if (Approval < obj.Approval) return 1;

			return 0;
		}
	}
}
