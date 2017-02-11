using System;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.ViewModel.Metro
{
    public class RandomSeriesTile
    {
        public string AnimeName { get; set; }
        public string Details { get; set; }
        public String Picture { get; set; }
        public long Height { get; set; }
        public String TileSize { get; set; }
        public VM_AnimeSeries_User AnimeSeries { get; set; }

    }
}
