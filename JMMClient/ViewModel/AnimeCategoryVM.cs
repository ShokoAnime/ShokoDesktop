using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AnimeCategoryVM : IComparable<AnimeCategoryVM>
	{
		public int CategoryID { get; set; }
		public int ParentID { get; set; }
		public int IsHentai { get; set; }
		public string CategoryName { get; set; }
		public string CategoryDescription { get; set; }
		public int Weighting { get; set; }

		public AnimeCategoryVM()
		{
		}

		public AnimeCategoryVM(JMMServerBinary.Contract_AnimeCategory contract)
		{
			this.CategoryDescription = contract.CategoryDescription;
			this.CategoryID = contract.CategoryID;
			this.CategoryName = contract.CategoryName;
			this.IsHentai = contract.IsHentai;
			this.ParentID = contract.ParentID;
			this.Weighting = contract.Weighting;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}) - {2}", CategoryName, Weighting, CategoryDescription);
		}

		public int CompareTo(AnimeCategoryVM obj)
		{
			//return Weighting.CompareTo(obj.Weighting);

			if (Weighting > obj.Weighting) return -1;
			if (Weighting < obj.Weighting) return 1;

			return 0;
		}
	}
}
