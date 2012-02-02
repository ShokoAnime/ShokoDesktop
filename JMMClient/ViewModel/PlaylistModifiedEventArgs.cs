using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class PlaylistModifiedEventArgs : EventArgs
	{
		public readonly int? PlaylistID;

		public PlaylistModifiedEventArgs(int? playlistID)
		{
			PlaylistID = playlistID;
		}
	}
}
