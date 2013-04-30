using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JMMClient.ImageDownload;

namespace JMMClient.ViewModel
{
	public class TvDB_EpisodeVM
	{
		public int TvDB_EpisodeID { get; set; }
		public int Id { get; set; }
		public int SeriesID { get; set; }
		public int SeasonID { get; set; }
		public int SeasonNumber { get; set; }
		public int EpisodeNumber { get; set; }
		public string EpisodeName { get; set; }
		public string Overview { get; set; }
		public string Filename { get; set; }
		public int EpImgFlag { get; set; }
		public int? AbsoluteNumber { get; set; }
		public int? AirsAfterSeason { get; set; }
		public int? AirsBeforeEpisode { get; set; }
		public int? AirsBeforeSeason { get; set; }

		public string ImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(Filename)) return @"/Images/EpisodeThumb_NotFound.png";

				if (File.Exists(FullImagePath)) return FullImagePath;

				return OnlineImagePath;
			}
		}

		public string FullImagePathPlain
		{
			get
			{
				if (string.IsNullOrEmpty(Filename)) return "";

				string fname = Filename;
				fname = Filename.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
			}
		}

		public string FullImagePath
		{
			get
			{
				if (!File.Exists(FullImagePathPlain))
				{
					ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Episode, this, false);
					MainWindow.imageHelper.DownloadImage(req);
					if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
				}

				return FullImagePathPlain;
			}
		}

		public string OnlineImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(Filename)) return "";
				return string.Format(Constants.URLS.TvDB_Images, Filename);
			}
		}

		public override string ToString()
		{
			return string.Format("Season#: {0}, Episode {1}-{2}", SeasonNumber, EpisodeNumber, EpisodeName);
		}

		public TvDB_EpisodeVM(JMMServerBinary.Contract_TvDB_Episode contract)
		{
			this.TvDB_EpisodeID = contract.TvDB_EpisodeID;
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.SeasonID = contract.SeasonID;
			this.SeasonNumber = contract.SeasonNumber;
			this.EpisodeNumber = contract.EpisodeNumber;
			this.EpisodeName = contract.EpisodeName;
			this.Overview = contract.Overview;
			this.Filename = contract.Filename;
			this.EpImgFlag = contract.EpImgFlag;
			this.AbsoluteNumber = contract.AbsoluteNumber;

			this.AirsAfterSeason = contract.AirsAfterSeason;
			this.AirsBeforeEpisode = contract.AirsBeforeEpisode;
			this.AirsBeforeSeason = contract.AirsBeforeSeason;
		}
	}
}
