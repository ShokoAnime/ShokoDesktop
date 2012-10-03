using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
	public class NewEpisodeTile
	{
		public string AnimeName { get; set; }
		public string EpisodeDetails { get; set; }
		public String Picture { get; set; }
		public long Height { get; set; }
		public String TileSize { get; set; }
		public AnimeSeriesVM AnimeSeries { get; set; }
	}
}
