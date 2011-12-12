using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class Trakt_FriendRequestVM
	{
		public string Username { get; set; }
		public string Full_name { get; set; }
		public string Gender { get; set; }
		public object Age { get; set; }
		public string Location { get; set; }
		public string About { get; set; }
		public int Joined { get; set; }
		public DateTime? JoinedDate { get; set; }
		public int Requested { get; set; }
		public DateTime? RequestedDate { get; set; }
		public string Avatar { get; set; }
		public string Url { get; set; }

		public string ImagePathForDisplay
		{
			get
			{
				if (!string.IsNullOrEmpty(FullImagePath) && File.Exists(FullImagePath)) 
					return FullImagePath;

				if (!string.IsNullOrEmpty(OnlineImagePath))
					return OnlineImagePath;

				return @"/Images/EpisodeThumb_NotFound.png";
			}
		}

		public string OnlineImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(Avatar)) return "";
				return Avatar;
			}
		}


		public string FullImagePath
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

		public Trakt_FriendRequestVM(JMMServerBinary.Contract_Trakt_FriendFrequest contract)
		{
			this.Username = contract.Username;
			this.Full_name = contract.Full_name;
			this.Gender = contract.Gender;
			this.Age = contract.Age;
			this.Location = contract.Location;
			this.About = contract.About;
			this.Joined = contract.Joined;
			this.JoinedDate = contract.JoinedDate;
			this.Requested = contract.Requested;
			this.RequestedDate = contract.RequestedDate;
			this.Avatar = contract.Avatar;
			this.Url = contract.Url;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", Username, Avatar);
		}
	}
}
