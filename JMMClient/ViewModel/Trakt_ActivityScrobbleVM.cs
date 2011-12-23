using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class Trakt_ActivityScrobbleVM
	{
		// user details
		public int Trakt_FriendID { get; private set; }
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

		// activity details
		public int ActivityAction { get; set; } // scrobble, shout
		public int ActivityType { get; set; } // episode, show
		public DateTime? ActivityDate { get; set; }
		public string Elapsed { get; set; }
		public string ElapsedShort { get; set; }

		// if episode
		public Trakt_WatchedEpisodeVM Episode { get; set; }

		public string UserImagePathForDisplay
		{
			get
			{
				if (!string.IsNullOrEmpty(UserFullImagePath) && File.Exists(UserFullImagePath)) return UserFullImagePath;

				return @"/Images/EpisodeThumb_NotFound.png";
			}
		}

		public string UserOnlineImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(Avatar)) return "";
				return Avatar;
			}
		}

		public string UserFullImagePath
		{
			get
			{
				// typical url
				// http://vicmackey.trakt.tv/images/avatars/837.jpg
				// http://gravatar.com/avatar/f894a4cbd5e8bcbb1a79010699af1183.jpg?s=140&r=pg&d=http%3A%2F%2Fvicmackey.trakt.tv%2Fimages%2Favatar-large.jpg

				if (string.IsNullOrEmpty(Avatar)) return "";

				string path = Utils.GetTraktImagePath_Avatars();
				return Path.Combine(path, string.Format("{0}.jpg", Username));
			}
		}

		public string AnimeImagePathForDisplay
		{
			get
			{
				if (!string.IsNullOrEmpty(AnimeFullImagePath) && File.Exists(AnimeFullImagePath)) return AnimeFullImagePath;

				return @"/Images/blankposter.png";
			}
		}

		public string AnimeFullImagePath
		{
			get
			{
				if (Episode == null) return "";
				if (Episode.Anime == null) return "";
				if (string.IsNullOrEmpty(Episode.Anime.DefaultPosterPath)) return "";

				return Episode.Anime.DefaultPosterPath;
			}
		}

		public Trakt_ActivityScrobbleVM(JMMServerBinary.Contract_Trakt_FriendActivity contract)
		{
			this.Trakt_FriendID = contract.Trakt_FriendID;
			this.Username = contract.Username;
			this.Full_name = contract.Full_name;
			this.Gender = contract.Gender;
			this.Age = contract.Age;
			this.Location = contract.Location;
			this.About = contract.About;
			this.Joined = contract.Joined;
			this.JoinedDate = contract.JoinedDate;
			this.Avatar = contract.Avatar;
			this.Url = contract.Url;

			this.ActivityAction = contract.ActivityAction;
			this.ActivityType = contract.ActivityType;
			this.ActivityDate = contract.ActivityDate;
			this.Elapsed = contract.Elapsed;
			this.ElapsedShort = contract.ElapsedShort;

			if (contract.Episode != null)
				this.Episode = new Trakt_WatchedEpisodeVM(contract.Episode);
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", Username, Avatar);
		}
	}
}
