using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
	public class RecommendationTile
	{
		public string AnimeName { get; set; }
		public string Details { get; set; }
		public String Picture { get; set; }
		public long Height { get; set; }
		public String TileSize { get; set; }
		public AnimeSeriesVM AnimeSeries { get; set; }
		public string URL { get; set; }
		public string Source { get; set; }
		public string Description { get; set; }
		public int AnimeID { get; set; }
		public int SimilarAnimeID { get; set; }
		public bool HasSeries { get; set; }
	}
}
