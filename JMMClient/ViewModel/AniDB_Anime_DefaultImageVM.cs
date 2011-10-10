using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class AniDB_Anime_DefaultImageVM
	{
		public int AniDB_Anime_DefaultImageID { get; set; }
		public int AnimeID { get; set; }
		public int ImageParentID { get; set; }
		public int ImageParentType { get; set; }
		public int ImageType { get; set; }

		public MovieDB_PosterVM MoviePoster { get; set; }
		public MovieDB_FanartVM MovieFanart { get; set; }

		public TvDB_ImagePosterVM TVPoster { get; set; }
		public TvDB_ImageFanartVM TVFanart { get; set; }
		public TvDB_ImageWideBannerVM TVWideBanner { get; set; }

		public Trakt_ImagePosterVM TraktPoster { get; set; }
		public Trakt_ImageFanartVM TraktFanart { get; set; }

		public AniDB_Anime_DefaultImageVM()
		{
		}

		public AniDB_Anime_DefaultImageVM(JMMServerBinary.Contract_AniDB_Anime_DefaultImage contract)
		{
			this.AniDB_Anime_DefaultImageID = contract.AniDB_Anime_DefaultImageID;
			this.AnimeID = contract.AnimeID;
			this.ImageParentID = contract.ImageParentID;
			this.ImageParentType = contract.ImageParentType;
			this.ImageType = contract.ImageType;

			MoviePoster = null;
			MovieFanart = null;
			TVPoster = null;
			TVFanart = null;
			TVWideBanner = null;
			TraktPoster = null;
			TraktFanart = null;

			if (contract.MovieFanart != null)
				MovieFanart = new MovieDB_FanartVM(contract.MovieFanart);

			if (contract.MoviePoster != null)
				MoviePoster = new MovieDB_PosterVM(contract.MoviePoster);

			if (contract.TVPoster != null)
				TVPoster = new TvDB_ImagePosterVM(contract.TVPoster);

			if (contract.TVFanart != null)
				TVFanart = new TvDB_ImageFanartVM(contract.TVFanart);

			if (contract.TVWideBanner != null)
				TVWideBanner = new TvDB_ImageWideBannerVM(contract.TVWideBanner);

			if (contract.TraktPoster != null)
				TraktPoster = new Trakt_ImagePosterVM(contract.TraktPoster);

			if (contract.TraktFanart != null)
				TraktFanart = new Trakt_ImageFanartVM(contract.TraktFanart);
		}

		public string FullImagePath
		{
			get
			{
				ImageEntityType itype = (ImageEntityType)ImageParentType;
				string fileName = "";

				switch (itype)
				{
					case ImageEntityType.AniDB_Cover:
						if (MainListHelperVM.Instance.AllAnimeDictionary.ContainsKey(AnimeID))
						{
							fileName = MainListHelperVM.Instance.AllAnimeDictionary[AnimeID].PosterPath;
						}
						break;

					case ImageEntityType.TvDB_Cover:
						fileName = TVPoster.FullImagePath;
						break;

					case ImageEntityType.MovieDB_Poster:
						fileName = MoviePoster.FullImagePath;
						break;

					case ImageEntityType.MovieDB_FanArt:
						fileName =  MovieFanart.FullImagePath;
						break;

					case ImageEntityType.TvDB_FanArt:
						fileName =  TVFanart.FullImagePath;
						break;

					case ImageEntityType.TvDB_Banner:
						fileName =  TVWideBanner.FullImagePath;
						break;

					case ImageEntityType.Trakt_Poster:
						fileName = TraktPoster.FullImagePath;
						break;

					case ImageEntityType.Trakt_Fanart:
						fileName = TraktFanart.FullImagePath;
						break;
				}

				if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
					return fileName;
				else
					return string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);

			}
		}

		public string FullThumbnailPath
		{
			get
			{
				ImageEntityType itype = (ImageEntityType)ImageParentType;
				string fileName = "";

				switch (itype)
				{
					case ImageEntityType.AniDB_Cover:
						if (MainListHelperVM.Instance.AllAnimeDictionary.ContainsKey(AnimeID))
						{
							fileName = MainListHelperVM.Instance.AllAnimeDictionary[AnimeID].PosterPath;
						}
						break;

					case ImageEntityType.TvDB_Cover:
						fileName = TVPoster.FullImagePath;
						break;

					case ImageEntityType.MovieDB_Poster:
						fileName = MoviePoster.FullImagePath;
						break;

					case ImageEntityType.MovieDB_FanArt:
						fileName = MovieFanart.FullImagePath;
						break;

					case ImageEntityType.TvDB_FanArt:
						fileName = TVFanart.FullThumbnailPath;
						break;

					case ImageEntityType.TvDB_Banner:
						fileName = TVWideBanner.FullImagePath;
						break;

					case ImageEntityType.Trakt_Poster:
						fileName = TraktPoster.FullImagePath;
						break;

					case ImageEntityType.Trakt_Fanart:
						fileName = TraktFanart.FullImagePath;
						break;
				}

				if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
					return fileName;
				else
					return string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);

			}
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", AnimeID, ImageParentID);
		}
	}
}
