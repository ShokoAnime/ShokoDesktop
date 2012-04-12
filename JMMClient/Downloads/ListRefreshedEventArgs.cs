using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.Downloads
{
	public class ListRefreshedEventArgs : EventArgs
	{
		public readonly List<Torrent> Torrents = new List<Torrent>();

		public ListRefreshedEventArgs(List<Torrent> tors)
		{
			this.Torrents = tors;
		}
	}
}
