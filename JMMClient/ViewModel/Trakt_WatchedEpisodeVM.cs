using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class Trakt_WatchedEpisodeVM
	{
		public int Watched { get; set; }
		public DateTime? WatchedDate { get; set; }

		public string Episode_Season { get; set; }
		public string Episode_Number { get; set; }
		public string Episode_Title { get; set; }
		public string Episode_Overview { get; set; }
		public string Episode_Url { get; set; }
		public string Episode_Screenshot { get; set; }

		public TraktTVShowResponseVM TraktShow { get; set; }

		public int? AnimeSeriesID { get; set; }
		public AniDB_AnimeVM Anime { get; set; }

		public string WatchedDateAsString
		{
			get
			{
				if (!WatchedDate.HasValue) return "";
				return WatchedDate.Value.ToString("dd MMM yyyy", Globals.Culture);
			}
		}

		public string ShowTitle
		{
			get
			{
				if (Anime != null)
					return Anime.MainTitle;

				return TraktShow.title;
			}
		}

		public Trakt_WatchedEpisodeVM(JMMServerBinary.Contract_Trakt_WatchedEpisode contract)
		{
			this.Watched = contract.Watched;
			this.WatchedDate = contract.WatchedDate;
			this.Episode_Season = contract.Episode_Season;
			this.Episode_Number = contract.Episode_Number;
			this.Episode_Title = contract.Episode_Title;
			this.Episode_Overview = contract.Episode_Overview;
			this.Episode_Url = contract.Episode_Url;
			this.Episode_Screenshot = contract.Episode_Screenshot;
			//this.Episode_Screenshot = "/Images/16_Refresh.png";
			this.AnimeSeriesID = contract.AnimeSeriesID;

			if (contract.TraktShow != null)
				this.TraktShow = new TraktTVShowResponseVM(contract.TraktShow);

			if (contract.Anime != null)
				this.Anime = new AniDB_AnimeVM(contract.Anime);
		}
	}
}
