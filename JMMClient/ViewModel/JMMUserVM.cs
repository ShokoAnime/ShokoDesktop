using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class JMMUserVM
	{
		public int? JMMUserID { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int IsAdmin { get; set; }
		public int IsAniDBUser { get; set; }
		public int IsTraktUser { get; set; }
		public string HideCategories { get; set; }

		public bool IsAdminUser
		{
			get { return IsAdmin == 1; }
		}

		public bool IsAniDBUserBool
		{
			get { return IsAniDBUser == 1; }
		}

		public bool IsTraktUserBool
		{
			get { return IsTraktUser == 1; }
		}

		public JMMUserVM()
		{
		}

		public JMMUserVM(JMMServerBinary.Contract_JMMUser contract)
		{
			this.JMMUserID = contract.JMMUserID;
			this.Username = contract.Username;
			this.Password = contract.Password;
			this.IsAdmin = contract.IsAdmin;
			this.IsAniDBUser = contract.IsAniDBUser;
			this.IsTraktUser = contract.IsTraktUser;
			this.HideCategories = contract.HideCategories;
		}

		public JMMServerBinary.Contract_JMMUser ToContract()
		{
			JMMServerBinary.Contract_JMMUser contract = new JMMServerBinary.Contract_JMMUser();
			contract.JMMUserID = this.JMMUserID;
			contract.Username = this.Username;
			contract.Password = this.Password;
			contract.IsAdmin = this.IsAdmin;
			contract.IsAniDBUser = this.IsAniDBUser;
			contract.IsTraktUser = this.IsTraktUser;
			contract.HideCategories = this.HideCategories;
			return contract;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} ({2}) - {3}", Username, IsAdmin, IsAniDBUser, HideCategories);
		}

		private bool EvaluateCategoryString(string allcats)
		{
			string filterParm = HideCategories.Trim();

			string[] cats = filterParm.Trim().Split(',');
			int index = 0;
			foreach (string cat in cats)
			{
				if (cat.Trim().Length == 0) continue;
				if (cat.Trim() == ",") continue;

				index = allcats.IndexOf(cat, 0, StringComparison.InvariantCultureIgnoreCase);
				if (index > -1) return false;
			}

			return true;
		}

		public bool EvaluateGroup(AnimeGroupVM grp)
		{
			// make sure the user has not filtered this out
			if (!string.IsNullOrEmpty(JMMServerVM.Instance.CurrentUser.HideCategories))
			{
				return EvaluateCategoryString(grp.Stat_AllCategories);
			}

			return true;
		}

		public bool EvaluateSeries(AnimeSeriesVM ser)
		{
			// make sure the user has not filtered this out
			if (!string.IsNullOrEmpty(JMMServerVM.Instance.CurrentUser.HideCategories))
			{
				return EvaluateCategoryString(ser.CategoriesString);
			}

			return true;
		}

		public bool EvaluateAnime(AniDB_AnimeVM anime)
		{
			// make sure the user has not filtered this out
			if (!string.IsNullOrEmpty(JMMServerVM.Instance.CurrentUser.HideCategories))
			{
				return EvaluateCategoryString(anime.AllCategories);
			}

			return true;
		}
	}
}
