using System;
using System.Collections.Generic;
using System.Linq;

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
		public HashSet<string> HideTags { get; set; }
		public int? CanEditServerSettings { get; set; }
        public HashSet<string> PlexUsers { get; set; }
		public bool IsAdminUser
		{
			get { return IsAdmin == 1; }
		}

        public bool IsAniDBUserBool
        {
            get { return IsAniDBUser == 1; }
        }

        public bool CanEditSettings
        {
            get { return CanEditServerSettings.HasValue ? CanEditServerSettings.Value == 1 : false; }
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
			this.HideTags = new HashSet<string>(contract.HideCategories);
			this.CanEditServerSettings = contract.CanEditServerSettings;
		    this.PlexUsers = new HashSet<string>(contract.PlexUsers);
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
			contract.HideCategories = this.HideTags.ToList();
			contract.CanEditServerSettings = this.CanEditServerSettings;
		    contract.PlexUsers = this.PlexUsers.ToList();
			return contract;
		}

        public override string ToString()
        {
            return string.Format("{0} - {1} ({2}) - {3}", Username, IsAdmin, IsAniDBUser, HideTags);
        }

		private bool EvaluateTags(HashSet<string> allcats)
		{
		    return !allcats.Overlaps(HideTags);
		}

        public bool EvaluateGroup(AnimeGroupVM grp)
        {
            if (grp.AnimeGroupID.Value == 215)
                Console.WriteLine("");

			return EvaluateTags(grp.Stat_AllTags);
		}

		public bool EvaluateSeries(AnimeSeriesVM ser)
		{
			// make sure the user has not filtered this out
			return EvaluateTags(ser.AllTags);
		}

		public bool EvaluateAnime(AniDB_AnimeVM anime)
		{
			// make sure the user has not filtered this out
			return EvaluateTags(anime.AllTags);
		}
	}
}
