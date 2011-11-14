using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class Trakt_FriendVM
	{
		public string Username { get; set; }
		public string Full_name { get; set; }
		public string Gender { get; set; }
		public object Age { get; set; }
		public string Location { get; set; }
		public string About { get; set; }
		public int Joined { get; set; }
		public DateTime? JoinedDate { get; set; }
		public string Avatar { get; set; }
		public string Url { get; set; }
		public DateTime? LastEpisodeWatchedDate { get; set; }

		public List<Trakt_WatchedEpisodeVM> WatchedEpisodes { get; set; }
		public Trakt_WatchedEpisodeVM LastEpisodeWatched { get; set; }

		public Trakt_FriendVM(JMMServerBinary.Contract_Trakt_Friend contract)
		{
			WatchedEpisodes = new List<Trakt_WatchedEpisodeVM>();

			this.Username = contract.Username;
			this.Full_name = contract.Full_name;
			this.Gender = contract.Gender;
			this.Age = contract.Age;
			this.Location = contract.Location;
			this.About = contract.About;
			this.Joined = contract.Joined;
			this.JoinedDate = contract.JoinedDate;
			this.Avatar = contract.Avatar;
			//this.Avatar = "/Images/16_Refresh.png";
			this.Url = contract.Url;

			

			foreach (JMMServerBinary.Contract_Trakt_WatchedEpisode ep in contract.WatchedEpisodes)
				WatchedEpisodes.Add(new Trakt_WatchedEpisodeVM(ep));

			LastEpisodeWatched = null;
			LastEpisodeWatchedDate = null;
			if (WatchedEpisodes.Count > 0)
			{
				LastEpisodeWatched = WatchedEpisodes[0];
				LastEpisodeWatchedDate = LastEpisodeWatched.WatchedDate;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", Username, Avatar);
		}
	}
}
