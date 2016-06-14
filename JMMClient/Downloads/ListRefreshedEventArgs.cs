using System;
using System.Collections.Generic;

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
