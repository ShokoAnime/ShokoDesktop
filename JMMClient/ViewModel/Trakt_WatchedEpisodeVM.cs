using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

		public bool HasAnimeSeries
		{
			get
			{
				return AnimeSeriesID.HasValue;
			}
		}

		public string ImagePathForDisplay
		{
			get
			{
				if (!string.IsNullOrEmpty(FullImagePath) && File.Exists(FullImagePath)) return FullImagePath;

				// use fanart instead
				if (Anime != null && !string.IsNullOrEmpty(Anime.FanartPathOnlyExisting))
					return Anime.FanartPathOnlyExisting;

				return @"/Images/EpisodeThumb_NotFound.png";
			}
		}

		public string OnlineImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(Episode_Screenshot)) return "";
				return Episode_Screenshot;
			}
		}

		public string FullImagePath
		{
			get
			{
				// typical EpisodeImage url
				// http://vicmackey.trakt.tv/images/episodes/3228-1-1.jpg

				// get the TraktID from the URL
				// http://trakt.tv/show/11eyes/season/1/episode/1 (11 eyes)

				if (string.IsNullOrEmpty(Episode_Screenshot)) return "";

				// on Trakt, if the episode doesn't have a proper screenshot, they will return the
				// fanart instead, we will ignore this
				int pos = Episode_Screenshot.IndexOf(@"episodes/");
				if (pos <= 0) return "";

				string traktID = TraktShow.TraktID;
				traktID = traktID.Replace("/", @"\");

				string imageName = Episode_Screenshot.Substring(pos + 9, Episode_Screenshot.Length - pos - 9);
				imageName = imageName.Replace("/", @"\");

				string relativePath = Path.Combine("episodes", traktID);
				relativePath = Path.Combine(relativePath, imageName);

				return Path.Combine(Utils.GetTraktImagePath(), relativePath);
			}
		}

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
					return Anime.FormattedTitle;

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

			Console.Write(this.FullImagePath);
			Console.Write(this.OnlineImagePath);
			Console.Write(this.ImagePathForDisplay);
		}
	}
}
