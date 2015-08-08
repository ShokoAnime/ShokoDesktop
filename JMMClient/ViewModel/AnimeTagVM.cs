using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AnimeTagVM : IComparable<AnimeTagVM>
	{
		public int TagID { get; set; }
		public int LocalSpoiler { get; set; }
		public int GlobalSpoiler { get; set; }
		public string TagName { get; set; }
		public string TagDescription { get; set; }
        public int Weight { get; set; }

		public AnimeTagVM()
		{
		}

		public AnimeTagVM(JMMServerBinary.Contract_AnimeTag contract)
		{
			this.GlobalSpoiler = contract.GlobalSpoiler;
			this.LocalSpoiler = contract.LocalSpoiler;
			this.TagDescription = contract.TagDescription;
			this.TagID = contract.TagID;
			this.TagName = contract.TagName;
            this.Weight = contract.Weight;
		}

		public override string ToString()
		{
            return string.Format("{0} ({1}) - {2}", TagName, Weight, TagDescription);
		}

		public int CompareTo(AnimeTagVM obj)
		{
            if (Weight > obj.Weight) return 1;
            if (Weight < obj.Weight) return -1;

			return 0;
		}
	}
}
