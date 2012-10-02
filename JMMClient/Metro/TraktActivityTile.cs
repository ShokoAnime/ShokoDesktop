using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JMMClient.ViewModel;
using System.ComponentModel;

namespace JMMClient
{
	public class TraktActivityTile : BindableObject
	{
		public string ShowName { get; set; }
		public string FriendName { get; set; }
		public string EpisodeDetails { get; set; }
		public long Height { get; set; }
		public String TileSize { get; set; }
		public Trakt_ActivityScrobbleVM Scrobble { get; set; }

		private String uRL;
		public String URL
		{
			get { return uRL; }
			set
			{
				uRL = value;
				base.RaisePropertyChanged("URL");
			}
		}

		private String showPicture;
		public String ShowPicture
		{
			get { return showPicture; }
			set
			{
				showPicture = value;
				base.RaisePropertyChanged("ShowPicture");
			}
		}

		private String friendPicture;
		public String FriendPicture
		{
			get { return friendPicture; }
			set
			{
				friendPicture = value;
				base.RaisePropertyChanged("FriendPicture");
			}
		}
	}
}
