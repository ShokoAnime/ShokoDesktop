using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class AniDB_AnimeCrossRefsVM : INotifyPropertyChanged
	{
		public int AnimeID { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public AniDB_AnimeCrossRefsVM()
		{
		}

		private bool tvDBCrossRefExists = false;
		public bool TvDBCrossRefExists
		{
			get { return tvDBCrossRefExists; }
			set
			{
				tvDBCrossRefExists = value;
				NotifyPropertyChanged("TvDBCrossRefExists");
			}
		}

		private bool tvDBCrossRefMissing = false;
		public bool TvDBCrossRefMissing
		{
			get { return tvDBCrossRefMissing; }
			set
			{
				tvDBCrossRefMissing = value;
				NotifyPropertyChanged("TvDBCrossRefMissing");
			}
		}

		private TvDB_SeriesVM tvDBSeries = null;
		public TvDB_SeriesVM TvDBSeries
		{
			get { return tvDBSeries; }
			set
			{
				tvDBSeries = value;
				NotifyPropertyChanged("TvDBSeries");
			}
		}

		private CrossRef_AniDB_TvDBVM crossRef_AniDB_TvDB = null;
		public CrossRef_AniDB_TvDBVM CrossRef_AniDB_TvDB
		{
			get { return crossRef_AniDB_TvDB; }
			set
			{
				crossRef_AniDB_TvDB = value;
				NotifyPropertyChanged("CrossRef_AniDB_TvDB");
			}
		}

		private List<TvDB_EpisodeVM> tvDBEpisodes = null;
		public List<TvDB_EpisodeVM> TvDBEpisodes
		{
			get { return tvDBEpisodes; }
			set
			{
				tvDBEpisodes = value;
				NotifyPropertyChanged("TvDBEpisodes");
			}
		}

		private List<TvDB_ImageFanartVM> tvDBImageFanarts = null;
		public List<TvDB_ImageFanartVM> TvDBImageFanarts
		{
			get { return tvDBImageFanarts; }
			set
			{
				tvDBImageFanarts = value;
				NotifyPropertyChanged("TvDBImageFanarts");
			}
		}

		private List<TvDB_ImagePosterVM> tvDBImagePosters = null;
		public List<TvDB_ImagePosterVM> TvDBImagePosters
		{
			get { return tvDBImagePosters; }
			set
			{
				tvDBImagePosters = value;
				NotifyPropertyChanged("TvDBImagePosters");
			}
		}

		private List<TvDB_ImageWideBannerVM> tvDBImageWideBanners = null;
		public List<TvDB_ImageWideBannerVM> TvDBImageWideBanners
		{
			get { return tvDBImageWideBanners; }
			set
			{
				tvDBImageWideBanners = value;
				NotifyPropertyChanged("TvDBImageWideBanners");
			}
		}


		private bool movieDBCrossRefExists = false;
		public bool MovieDBCrossRefExists
		{
			get { return movieDBCrossRefExists; }
			set
			{
				movieDBCrossRefExists = value;
				NotifyPropertyChanged("MovieDBCrossRefExists");
			}
		}

		private bool movieDBCrossRefMissing = false;
		public bool MovieDBCrossRefMissing
		{
			get { return movieDBCrossRefMissing; }
			set
			{
				movieDBCrossRefMissing = value;
				NotifyPropertyChanged("MovieDBCrossRefMissing");
			}
		}

		private MovieDB_MovieVM movieDB_Movie = null;
		public MovieDB_MovieVM MovieDB_Movie
		{
			get { return movieDB_Movie; }
			set
			{
				movieDB_Movie = value;
				NotifyPropertyChanged("MovieDB_Movie");
			}
		}

		private CrossRef_AniDB_OtherVM crossRef_AniDB_MovieDB = null;
		public CrossRef_AniDB_OtherVM CrossRef_AniDB_MovieDB
		{
			get { return crossRef_AniDB_MovieDB; }
			set
			{
				crossRef_AniDB_MovieDB = value;
				NotifyPropertyChanged("CrossRef_AniDB_MovieDB");
			}
		}

		private List<MovieDB_FanartVM> movieDBFanarts = null;
		public List<MovieDB_FanartVM> MovieDBFanarts
		{
			get { return movieDBFanarts; }
			set
			{
				movieDBFanarts = value;
				NotifyPropertyChanged("MovieDBFanarts");
			}
		}

		private List<MovieDB_PosterVM> movieDBPosters = null;
		public List<MovieDB_PosterVM> MovieDBPosters
		{
			get { return movieDBPosters; }
			set
			{
				movieDBPosters = value;
				NotifyPropertyChanged("MovieDBPosters");
			}
		}


		private List<PosterContainer> allPosters = null;
		public List<PosterContainer> AllPosters
		{
			get { return allPosters; }
			set
			{
				allPosters = value;
				NotifyPropertyChanged("AllPosters");
			}
		}

		private List<FanartContainer> allFanarts = null;
		public List<FanartContainer> AllFanarts
		{
			get { return allFanarts; }
			set
			{
				allFanarts = value;
				NotifyPropertyChanged("AllFanarts");
			}
		}

		private bool traktCrossRefExists = false;
		public bool TraktCrossRefExists
		{
			get { return traktCrossRefExists; }
			set
			{
				traktCrossRefExists = value;
				NotifyPropertyChanged("TraktCrossRefExists");
			}
		}

		private bool traktCrossRefMissing = false;
		public bool TraktCrossRefMissing
		{
			get { return traktCrossRefMissing; }
			set
			{
				traktCrossRefMissing = value;
				NotifyPropertyChanged("TraktCrossRefMissing");
			}
		}

		private CrossRef_AniDB_TraktVM crossRef_AniDB_Trakt = null;
		public CrossRef_AniDB_TraktVM CrossRef_AniDB_Trakt
		{
			get { return crossRef_AniDB_Trakt; }
			set
			{
				crossRef_AniDB_Trakt = value;
				NotifyPropertyChanged("CrossRef_AniDB_Trakt");
			}
		}

		private Trakt_ShowVM traktShow = null;
		public Trakt_ShowVM TraktShow
		{
			get { return traktShow; }
			set
			{
				traktShow = value;
				NotifyPropertyChanged("TraktShow");
			}
		}

		private Trakt_ImageFanartVM traktImageFanart = null;
		public Trakt_ImageFanartVM TraktImageFanart
		{
			get { return traktImageFanart; }
			set
			{
				traktImageFanart = value;
				NotifyPropertyChanged("TraktImageFanart");
			}
		}

		private Trakt_ImagePosterVM traktImagePoster = null;
		public Trakt_ImagePosterVM TraktImagePoster
		{
			get { return traktImagePoster; }
			set
			{
				traktImagePoster = value;
				NotifyPropertyChanged("TraktImagePoster");
			}
		}

		private CrossRef_AniDB_MALVM crossRef_AniDB_MAL = null;
		public CrossRef_AniDB_MALVM CrossRef_AniDB_MAL
		{
			get { return crossRef_AniDB_MAL; }
			set
			{
				crossRef_AniDB_MAL = value;
				NotifyPropertyChanged("CrossRef_AniDB_MAL");
			}
		}

		private bool mALCrossRefExists = false;
		public bool MALCrossRefExists
		{
			get { return mALCrossRefExists; }
			set
			{
				mALCrossRefExists = value;
				NotifyPropertyChanged("MALCrossRefExists");
			}
		}

		private bool mALCrossRefMissing = false;
		public bool MALCrossRefMissing
		{
			get { return mALCrossRefMissing; }
			set
			{
				mALCrossRefMissing = value;
				NotifyPropertyChanged("MALCrossRefMissing");
			}
		}

		public void Populate(JMMServerBinary.Contract_AniDB_AnimeCrossRefs details)
		{
			AnimeID = details.AnimeID;

			AniDB_AnimeVM anime = null;
			if (MainListHelperVM.Instance.AllAnimeDictionary.ContainsKey(AnimeID))
				anime = MainListHelperVM.Instance.AllAnimeDictionary[AnimeID];

			CrossRef_AniDB_TvDB = null;
			TvDBSeries = null;
			TvDBEpisodes = new List<TvDB_EpisodeVM>();
			TvDBImageFanarts = new List<TvDB_ImageFanartVM>();
			TvDBImagePosters = new List<TvDB_ImagePosterVM>();
			TvDBImageWideBanners = new List<TvDB_ImageWideBannerVM>();

			CrossRef_AniDB_MovieDB = null;
			MovieDB_Movie = null;
			MovieDBPosters = new List<MovieDB_PosterVM>();
			MovieDBFanarts = new List<MovieDB_FanartVM>();

			CrossRef_AniDB_Trakt = null;
			TraktShow = null;
			TraktImageFanart = null;
			TraktImagePoster = null;

			CrossRef_AniDB_MAL = null;

			AllPosters = new List<PosterContainer>();
			AllFanarts = new List<FanartContainer>();

			// MAL
			if (details.CrossRef_AniDB_MAL != null)
				CrossRef_AniDB_MAL = new CrossRef_AniDB_MALVM(details.CrossRef_AniDB_MAL);

			if (CrossRef_AniDB_MAL == null)
			{
				MALCrossRefExists = false;
				MALCrossRefMissing = true;
			}
			else
			{
				MALCrossRefExists = true;
				MALCrossRefMissing = false;
			}

			// Trakt
			if (details.CrossRef_AniDB_Trakt != null)
				CrossRef_AniDB_Trakt = new CrossRef_AniDB_TraktVM(details.CrossRef_AniDB_Trakt);

			if (details.TraktShow != null)
				TraktShow = new Trakt_ShowVM(details.TraktShow);

			if (details.TraktImageFanart != null)
			{
				TraktImageFanart = new Trakt_ImageFanartVM(details.TraktImageFanart);

				bool isDefault = false;
				if (anime != null && anime.DefaultFanart != null && anime.DefaultFanart.ImageParentType == (int)ImageEntityType.Trakt_Fanart
					&& anime.DefaultFanart.ImageParentID == TraktImageFanart.Trakt_ImageFanartID)
				{
					isDefault = true;
				}

				TraktImageFanart.IsImageDefault = isDefault;
				TraktImageFanart.IsImageNotDefault = !isDefault;

				AllFanarts.Add(new FanartContainer(ImageEntityType.Trakt_Fanart, TraktImageFanart));
			}

			if (details.TraktImagePoster != null)
			{
				TraktImagePoster = new Trakt_ImagePosterVM(details.TraktImagePoster);

				bool isDefault = false;
				if (anime != null && anime.DefaultPoster != null && anime.DefaultPoster.ImageParentType == (int)ImageEntityType.Trakt_Poster
					&& anime.DefaultPoster.ImageParentID == TraktImagePoster.Trakt_ImagePosterID)
				{
					isDefault = true;
				}

				TraktImagePoster.IsImageDefault = isDefault;
				TraktImagePoster.IsImageNotDefault = !isDefault;

				AllPosters.Add(new PosterContainer(ImageEntityType.Trakt_Poster, TraktImagePoster));
			}

			if (CrossRef_AniDB_Trakt == null || TraktShow == null)
			{
				TraktCrossRefExists = false;
				TraktCrossRefMissing = true;
			}
			else
			{
				TraktCrossRefExists = true;
				TraktCrossRefMissing = false;
			}

			// TvDB
			if (details.CrossRef_AniDB_TvDB != null)
				CrossRef_AniDB_TvDB = new CrossRef_AniDB_TvDBVM(details.CrossRef_AniDB_TvDB);

			if (details.TvDBSeries != null)
				TvDBSeries = new TvDB_SeriesVM(details.TvDBSeries);

			foreach (JMMServerBinary.Contract_TvDB_Episode contract in details.TvDBEpisodes)
				TvDBEpisodes.Add(new TvDB_EpisodeVM(contract));

			foreach (JMMServerBinary.Contract_TvDB_ImageFanart contract in details.TvDBImageFanarts)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultFanart != null && anime.DefaultFanart.ImageParentType == (int)ImageEntityType.TvDB_FanArt
					&& anime.DefaultFanart.ImageParentID == contract.TvDB_ImageFanartID)
				{
					isDefault = true;
				}

				TvDB_ImageFanartVM tvFanart = new TvDB_ImageFanartVM(contract);
				tvFanart.IsImageDefault = isDefault;
				tvFanart.IsImageNotDefault = !isDefault;
				TvDBImageFanarts.Add(tvFanart);

				AllFanarts.Add(new FanartContainer(ImageEntityType.TvDB_FanArt, tvFanart));
			}

			foreach (JMMServerBinary.Contract_TvDB_ImagePoster contract in details.TvDBImagePosters)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultPoster != null && anime.DefaultPoster.ImageParentType == (int)ImageEntityType.TvDB_Cover
					&& anime.DefaultPoster.ImageParentID == contract.TvDB_ImagePosterID)
				{
					isDefault = true;
				}

				TvDB_ImagePosterVM tvPoster = new TvDB_ImagePosterVM(contract);
				tvPoster.IsImageDefault = isDefault;
				tvPoster.IsImageNotDefault = !isDefault;
				TvDBImagePosters.Add(tvPoster);
				AllPosters.Add(new PosterContainer(ImageEntityType.TvDB_Cover, tvPoster));
			}

			foreach (JMMServerBinary.Contract_TvDB_ImageWideBanner contract in details.TvDBImageWideBanners)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultWideBanner != null && anime.DefaultWideBanner.ImageParentType == (int)ImageEntityType.TvDB_Banner
					&& anime.DefaultWideBanner.ImageParentID == contract.TvDB_ImageWideBannerID)
				{
					isDefault = true;
				}

				TvDB_ImageWideBannerVM tvBanner = new TvDB_ImageWideBannerVM(contract);
				tvBanner.IsImageDefault = isDefault;
				tvBanner.IsImageNotDefault = !isDefault;
				TvDBImageWideBanners.Add(tvBanner);
			}

			if (CrossRef_AniDB_TvDB == null || TvDBSeries == null)
			{
				TvDBCrossRefExists = false;
				TvDBCrossRefMissing = true;
			}
			else
			{
				TvDBCrossRefExists = true;
				TvDBCrossRefMissing = false;
			}

			// MovieDB
			if (details.CrossRef_AniDB_MovieDB != null)
				CrossRef_AniDB_MovieDB = new CrossRef_AniDB_OtherVM(details.CrossRef_AniDB_MovieDB);

			if (details.MovieDBMovie != null)
				MovieDB_Movie = new MovieDB_MovieVM(details.MovieDBMovie);

			foreach (JMMServerBinary.Contract_MovieDB_Fanart contract in details.MovieDBFanarts)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultFanart != null && anime.DefaultFanart.ImageParentType == (int)ImageEntityType.MovieDB_FanArt
					&& anime.DefaultFanart.ImageParentID == contract.MovieDB_FanartID)
				{
					isDefault = true;
				}

				MovieDB_FanartVM movieFanart = new MovieDB_FanartVM(contract);
				movieFanart.IsImageDefault = isDefault;
				movieFanart.IsImageNotDefault = !isDefault;
				MovieDBFanarts.Add(movieFanart);
				AllFanarts.Add(new FanartContainer(ImageEntityType.MovieDB_FanArt, movieFanart));
			}

			foreach (JMMServerBinary.Contract_MovieDB_Poster contract in details.MovieDBPosters)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultPoster != null && anime.DefaultPoster.ImageParentType == (int)ImageEntityType.MovieDB_Poster
					&& anime.DefaultPoster.ImageParentID == contract.MovieDB_PosterID)
				{
					isDefault = true;
				}

				MovieDB_PosterVM moviePoster = new MovieDB_PosterVM(contract);
				moviePoster.IsImageDefault = isDefault;
				moviePoster.IsImageNotDefault = !isDefault;
				MovieDBPosters.Add(moviePoster);
				AllPosters.Add(new PosterContainer(ImageEntityType.MovieDB_Poster, moviePoster));
			}

			if (CrossRef_AniDB_MovieDB == null || MovieDB_Movie == null)
			{
				MovieDBCrossRefExists = false;
				MovieDBCrossRefMissing = true;
			}
			else
			{
				MovieDBCrossRefExists = true;
				MovieDBCrossRefMissing = false;
			}

		}
	}

	public class PosterContainer : INotifyPropertyChanged
	{
		public ImageEntityType ImageType { get; set; }
		public object PosterObject { get; set; }

		public PosterContainer(ImageEntityType imageType, object poster)
		{
			ImageType = imageType;
			PosterObject = poster;

			switch (ImageType)
			{
				case ImageEntityType.AniDB_Cover:
					AniDB_AnimeVM anime = PosterObject as AniDB_AnimeVM;
					IsImageEnabled = anime.IsImageEnabled;
					IsImageDisabled = anime.IsImageDisabled;
					IsImageDefault = anime.IsImageDefault;
					IsImageNotDefault = anime.IsImageNotDefault;
					PosterSource = "AniDB";
					break;

				case ImageEntityType.TvDB_Cover:
					TvDB_ImagePosterVM tvPoster = PosterObject as TvDB_ImagePosterVM;
					IsImageEnabled = tvPoster.IsImageEnabled;
					IsImageDisabled = tvPoster.IsImageDisabled;
					IsImageDefault = tvPoster.IsImageDefault;
					IsImageNotDefault = tvPoster.IsImageNotDefault;
					PosterSource = "TvDB";
					break;

				case ImageEntityType.MovieDB_Poster:
					MovieDB_PosterVM moviePoster = PosterObject as MovieDB_PosterVM;
					IsImageEnabled = moviePoster.IsImageEnabled;
					IsImageDisabled = moviePoster.IsImageDisabled;
					IsImageDefault = moviePoster.IsImageDefault;
					IsImageNotDefault = moviePoster.IsImageNotDefault;
					PosterSource = "MovieDB";
					break;

				case ImageEntityType.Trakt_Poster:
					Trakt_ImagePosterVM traktPoster = PosterObject as Trakt_ImagePosterVM;
					IsImageEnabled = traktPoster.IsImageEnabled;
					IsImageDisabled = traktPoster.IsImageDisabled;
					IsImageDefault = traktPoster.IsImageDefault;
					IsImageNotDefault = traktPoster.IsImageNotDefault;
					PosterSource = "Trakt";
					break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public string FullImagePath
		{
			get
			{
				switch (ImageType)
				{
					case ImageEntityType.AniDB_Cover:
						AniDB_AnimeVM anime = PosterObject as AniDB_AnimeVM;
						return anime.PosterPath;

					case ImageEntityType.TvDB_Cover:
						TvDB_ImagePosterVM tvPoster = PosterObject as TvDB_ImagePosterVM;
						return tvPoster.FullImagePath;

					case ImageEntityType.MovieDB_Poster:
						MovieDB_PosterVM moviePoster = PosterObject as MovieDB_PosterVM;
						return moviePoster.FullImagePath;

					case ImageEntityType.Trakt_Poster:
						Trakt_ImagePosterVM traktPoster = PosterObject as Trakt_ImagePosterVM;
						return traktPoster.FullImagePath;
				}

				return "";
			}
		}

		private bool isImageEnabled = false;
		public bool IsImageEnabled
		{
			get { return isImageEnabled; }
			set
			{
				isImageEnabled = value;
				NotifyPropertyChanged("IsImageEnabled");
			}
		}

		private bool isImageDisabled = false;
		public bool IsImageDisabled
		{
			get { return isImageDisabled; }
			set
			{
				isImageDisabled = value;
				NotifyPropertyChanged("IsImageDisabled");
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set
			{
				isImageDefault = value;
				NotifyPropertyChanged("IsImageDefault");
			}
		}

		private bool isImageNotDefault = false;
		public bool IsImageNotDefault
		{
			get { return isImageNotDefault; }
			set
			{
				isImageNotDefault = value;
				NotifyPropertyChanged("IsImageNotDefault");
			}
		}

		private string posterSource = "";
		public string PosterSource
		{
			get { return posterSource; }
			set
			{
				posterSource = value;
				NotifyPropertyChanged("PosterType");
			}
		}
	}

	public class FanartContainer : INotifyPropertyChanged
	{
		public ImageEntityType ImageType { get; set; }
		public object FanartObject { get; set; }

		public FanartContainer(ImageEntityType imageType, object poster)
		{
			ImageType = imageType;
			FanartObject = poster;

			switch (ImageType)
			{
				case ImageEntityType.TvDB_FanArt:
					TvDB_ImageFanartVM tvFanart = FanartObject as TvDB_ImageFanartVM;
					IsImageEnabled = tvFanart.IsImageEnabled;
					IsImageDisabled = tvFanart.IsImageDisabled;
					IsImageDefault = tvFanart.IsImageDefault;
					IsImageNotDefault = tvFanart.IsImageNotDefault;
					FanartSource = "TvDB";
					break;

				case ImageEntityType.MovieDB_FanArt:
					MovieDB_FanartVM movieFanart = FanartObject as MovieDB_FanartVM;
					IsImageEnabled = movieFanart.IsImageEnabled;
					IsImageDisabled = movieFanart.IsImageDisabled;
					IsImageDefault = movieFanart.IsImageDefault;
					IsImageNotDefault = movieFanart.IsImageNotDefault;
					FanartSource = "MovieDB";
					break;

				case ImageEntityType.Trakt_Fanart:
					Trakt_ImageFanartVM traktFanart = FanartObject as Trakt_ImageFanartVM;
					IsImageEnabled = traktFanart.IsImageEnabled;
					IsImageDisabled = traktFanart.IsImageDisabled;
					IsImageDefault = traktFanart.IsImageDefault;
					IsImageNotDefault = traktFanart.IsImageNotDefault;
					FanartSource = "Trakt";
					break;
			}

			
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public string FullImagePath
		{
			get
			{
				switch (ImageType)
				{

					case ImageEntityType.TvDB_FanArt:
						TvDB_ImageFanartVM tvFanart = FanartObject as TvDB_ImageFanartVM;
						return tvFanart.FullImagePath;

					case ImageEntityType.MovieDB_FanArt:
						MovieDB_FanartVM movieFanart = FanartObject as MovieDB_FanartVM;
						return movieFanart.FullImagePath;

					case ImageEntityType.Trakt_Fanart:
						Trakt_ImageFanartVM traktFanart = FanartObject as Trakt_ImageFanartVM;
						return traktFanart.FullImagePath;
				}

				return "";
			}
		}

		public string FullThumbnailPath
		{
			get
			{
				switch (ImageType)
				{

					case ImageEntityType.TvDB_FanArt:
						TvDB_ImageFanartVM tvFanart = FanartObject as TvDB_ImageFanartVM;
						return tvFanart.FullThumbnailPath;

					case ImageEntityType.MovieDB_FanArt:
						MovieDB_FanartVM movieFanart = FanartObject as MovieDB_FanartVM;
						return movieFanart.FullImagePath;

					case ImageEntityType.Trakt_Fanart:
						Trakt_ImageFanartVM traktFanart = FanartObject as Trakt_ImageFanartVM;
						return traktFanart.FullImagePath;
				}

				return "";
			}
		}

		private bool isImageEnabled = false;
		public bool IsImageEnabled
		{
			get { return isImageEnabled; }
			set
			{
				isImageEnabled = value;
				NotifyPropertyChanged("IsImageEnabled");
			}
		}

		private bool isImageDisabled = false;
		public bool IsImageDisabled
		{
			get { return isImageDisabled; }
			set
			{
				isImageDisabled = value;
				NotifyPropertyChanged("IsImageDisabled");
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set
			{
				isImageDefault = value;
				NotifyPropertyChanged("IsImageDefault");
			}
		}

		private bool isImageNotDefault = false;
		public bool IsImageNotDefault
		{
			get { return isImageNotDefault; }
			set
			{
				isImageNotDefault = value;
				NotifyPropertyChanged("IsImageNotDefault");
			}
		}

		private string fanartSource = "";
		public string FanartSource
		{
			get { return fanartSource; }
			set
			{
				fanartSource = value;
				NotifyPropertyChanged("FanartSource");
			}
		}
	}
}
