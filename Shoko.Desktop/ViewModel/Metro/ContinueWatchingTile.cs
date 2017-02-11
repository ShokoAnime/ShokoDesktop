using System;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.ViewModel.Metro
{
    public class ContinueWatchingTile
    {
        public string AnimeName { get; set; }
        public string EpisodeDetails { get; set; }
        public String Picture { get; set; }
        public long Height { get; set; }
        public String TileSize { get; set; }
        public VM_AnimeSeries_User AnimeSeries { get; set; }
        public int UnwatchedEpisodes { get; set; }

    }


}
