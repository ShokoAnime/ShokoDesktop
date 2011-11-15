using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using JMMClient.ViewModel;
using System.IO;
using System.Net;
using System.Threading;
using NLog;

namespace JMMClient.ImageDownload
{
	public class ImageDownloader
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private const string QUEUE_STOP = "StopQueue";
		private BlockingList<ImageDownloadRequest> imagesToDownload = new BlockingList<ImageDownloadRequest>();
		private BackgroundWorker workerImages = new BackgroundWorker();

		public int QueueCount
		{
			get { return imagesToDownload.Count; }
		}

		public ImageDownloader()
		{
			workerImages.WorkerReportsProgress = true;
			workerImages.WorkerSupportsCancellation = true;
			workerImages.DoWork += new DoWorkEventHandler(ProcessImages);
		}

		public delegate void QueueUpdateEventHandler(QueueUpdateEventArgs ev);
		public event QueueUpdateEventHandler QueueUpdateEvent;
		protected void OnQueueUpdateEvent(QueueUpdateEventArgs ev)
		{
			if (QueueUpdateEvent != null)
			{
				QueueUpdateEvent(ev);
			}
		}

		public delegate void ImageDownloadEventHandler(ImageDownloadEventArgs ev);
		public event ImageDownloadEventHandler ImageDownloadEvent;
		protected void OnImageDownloadEvent(ImageDownloadEventArgs ev)
		{
			if (ImageDownloadEvent != null)
			{
				ImageDownloadEvent(ev);
			}
		}

		public void Init()
		{
			this.workerImages.RunWorkerAsync();
		}

		public void DownloadAniDBCover(AniDB_AnimeVM anime, bool forceDownload)
		{
			if (string.IsNullOrEmpty(anime.Picname)) return;

			try
			{
				if (anime.AnimeID == 8580)
					Console.Write("");

				string url = string.Format(Constants.URLS.AniDB_Images, anime.Picname);
				string filename = anime.PosterPathNoDefault;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, anime, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(anime.PosterPath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTvDBPoster(TvDB_ImagePosterVM poster, bool forceDownload)
		{
			if (string.IsNullOrEmpty(poster.BannerPath)) return;

			try
			{
				string url = string.Format(Constants.URLS.TvDB_Images, poster.BannerPath);
				string filename = poster.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Cover, poster, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(poster.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTvDBWideBanner(TvDB_ImageWideBannerVM wideBanner, bool forceDownload)
		{
			if (string.IsNullOrEmpty(wideBanner.BannerPath)) return;

			try
			{
				string url = string.Format(Constants.URLS.TvDB_Images, wideBanner.BannerPath);
				string filename = wideBanner.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Banner, wideBanner, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(wideBanner.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTvDBEpisode(TvDB_EpisodeVM episode, bool forceDownload)
		{
			if (string.IsNullOrEmpty(episode.Filename)) return;

			try
			{
				string url = string.Format(Constants.URLS.TvDB_Images, episode.Filename);
				string filename = episode.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Episode, episode, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(episode.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTvDBFanart(TvDB_ImageFanartVM fanart, bool forceDownload)
		{
			if (string.IsNullOrEmpty(fanart.BannerPath)) return;

			try
			{
				string url = string.Format(Constants.URLS.TvDB_Images, fanart.BannerPath);
				string filename = fanart.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, fanart, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(fanart.FullImagePath) || !File.Exists(fanart.FullThumbnailPath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadMovieDBPoster(MovieDB_PosterVM poster, bool forceDownload)
		{
			if (string.IsNullOrEmpty(poster.URL)) return;

			try
			{
				string url = poster.URL;
				string filename = poster.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_Poster, poster, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(poster.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadMovieDBFanart(MovieDB_FanartVM fanart, bool forceDownload)
		{
			if (string.IsNullOrEmpty(fanart.URL)) return;

			try
			{
				string url = fanart.URL;
				string filename = fanart.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_FanArt, fanart, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(fanart.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTraktPoster(Trakt_ImagePosterVM poster, bool forceDownload)
		{
			if (string.IsNullOrEmpty(poster.ImageURL)) return;

			try
			{
				string url = poster.ImageURL;
				string filename = poster.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Poster, poster, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(poster.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTraktFanart(Trakt_ImageFanartVM fanart, bool forceDownload)
		{
			if (string.IsNullOrEmpty(fanart.ImageURL)) return;

			try
			{
				string url = fanart.ImageURL;
				string filename = fanart.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Fanart, fanart, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(fanart.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void DownloadTraktEpisode(Trakt_EpisodeVM episode, bool forceDownload)
		{
			if (string.IsNullOrEmpty(episode.EpisodeImage)) return;

			try
			{
				string url = episode.EpisodeImage;
				string filename = episode.FullImagePath;

				ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Episode, episode, forceDownload);

				// check if this file has already been downloaded and exists
				if (!req.ForceDownload)
				{
					// check to make sure the file actually exists
					if (!File.Exists(episode.FullImagePath))
					{
						this.imagesToDownload.Add(req);
						OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
						return;
					}

					// the file exists so don't download it again
					return;
				}

				this.imagesToDownload.Add(req);
				OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private string GetFileName(ImageDownloadRequest req, bool thumbNailOnly)
		{
			switch (req.ImageType)
			{
				case ImageEntityType.AniDB_Cover:

					AniDB_AnimeVM anime = req.ImageData as AniDB_AnimeVM;
					return anime.PosterPathNoDefault;

				case ImageEntityType.TvDB_Cover:

					TvDB_ImagePosterVM poster = req.ImageData as TvDB_ImagePosterVM;
					return poster.FullImagePath;

				case ImageEntityType.TvDB_Banner:

					TvDB_ImageWideBannerVM banner = req.ImageData as TvDB_ImageWideBannerVM;
					return banner.FullImagePath;

				case ImageEntityType.TvDB_Episode:

					TvDB_EpisodeVM episode = req.ImageData as TvDB_EpisodeVM;
					return episode.FullImagePath;

				case ImageEntityType.TvDB_FanArt:

					TvDB_ImageFanartVM fanart = req.ImageData as TvDB_ImageFanartVM;

					if (thumbNailOnly)
						return fanart.FullThumbnailPath;
					else
						return fanart.FullImagePath;

				case ImageEntityType.MovieDB_Poster:

					MovieDB_PosterVM moviePoster = req.ImageData as MovieDB_PosterVM;
					return moviePoster.FullImagePath;

				case ImageEntityType.MovieDB_FanArt:

					MovieDB_FanartVM movieFanart = req.ImageData as MovieDB_FanartVM;
					return movieFanart.FullImagePath;

				case ImageEntityType.Trakt_Poster:

					Trakt_ImagePosterVM traktPoster = req.ImageData as Trakt_ImagePosterVM;
					return traktPoster.FullImagePath;

				case ImageEntityType.Trakt_Fanart:

					Trakt_ImageFanartVM trakFanart = req.ImageData as Trakt_ImageFanartVM;
					return trakFanart.FullImagePath;

				case ImageEntityType.Trakt_Friend:

					Trakt_FriendVM trakFriend = req.ImageData as Trakt_FriendVM;
					return trakFriend.FullImagePath;

				case ImageEntityType.Trakt_Episode:

					Trakt_EpisodeVM trakEp = req.ImageData as Trakt_EpisodeVM;
					return trakEp.FullImagePath;

				default:
					return "";
			}

		}

		private string GetEntityID(ImageDownloadRequest req)
		{
			switch (req.ImageType)
			{
				case ImageEntityType.AniDB_Cover:

					AniDB_AnimeVM anime = req.ImageData as AniDB_AnimeVM;
					return anime.AnimeID.ToString();

				case ImageEntityType.TvDB_Cover:

					TvDB_ImagePosterVM poster = req.ImageData as TvDB_ImagePosterVM;
					return poster.TvDB_ImagePosterID.ToString();

				case ImageEntityType.TvDB_Banner:

					TvDB_ImageWideBannerVM banner = req.ImageData as TvDB_ImageWideBannerVM;
					return banner.TvDB_ImageWideBannerID.ToString();

				case ImageEntityType.TvDB_Episode:

					TvDB_EpisodeVM episode = req.ImageData as TvDB_EpisodeVM;
					return episode.TvDB_EpisodeID.ToString();

				case ImageEntityType.TvDB_FanArt:

					TvDB_ImageFanartVM fanart = req.ImageData as TvDB_ImageFanartVM;
					return fanart.TvDB_ImageFanartID.ToString();

				case ImageEntityType.MovieDB_Poster:

					MovieDB_PosterVM moviePoster = req.ImageData as MovieDB_PosterVM;
					return moviePoster.MovieDB_PosterID.ToString();

				case ImageEntityType.MovieDB_FanArt:

					MovieDB_FanartVM movieFanart = req.ImageData as MovieDB_FanartVM;
					return movieFanart.MovieDB_FanartID.ToString();

				case ImageEntityType.Trakt_Poster:

					Trakt_ImagePosterVM traktPoster = req.ImageData as Trakt_ImagePosterVM;
					return traktPoster.Trakt_ImagePosterID.ToString();

				case ImageEntityType.Trakt_Fanart:

					Trakt_ImageFanartVM trakFanart = req.ImageData as Trakt_ImageFanartVM;
					return trakFanart.Trakt_ImageFanartID.ToString();

				case ImageEntityType.Trakt_Friend:

					Trakt_FriendVM trakFriend = req.ImageData as Trakt_FriendVM;
					return trakFriend.Trakt_FriendID.ToString();

				case ImageEntityType.Trakt_Episode:

					Trakt_EpisodeVM trakEp = req.ImageData as Trakt_EpisodeVM;
					return trakEp.Trakt_EpisodeID.ToString();

				default:
					return "";
			}

		}

		public void DownloadImage(ImageDownloadRequest req)
		{
			string fileName = GetFileName(req, false);
			string entityID = GetEntityID(req);
			bool downloadImage = true;
			bool fileExists = File.Exists(fileName);

			if (fileExists)
			{
				if (!req.ForceDownload)
					downloadImage = false;
				else
					downloadImage = true;
			}
			else
				downloadImage = true;

			if (downloadImage)
			{
				string tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName));
				if (File.Exists(tempName)) File.Delete(tempName);


				OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Started));
				if (fileExists) File.Delete(fileName);

				byte[] imageArray = null;
				try
				{
					imageArray = JMMServerVM.Instance.imageClient.GetImage(entityID, (int)req.ImageType, false);
				}
				catch { }

				if (imageArray == null) return;

				File.WriteAllBytes(tempName, imageArray);

				// move the file to it's final location
				string fullPath = Path.GetDirectoryName(fileName);
				if (!Directory.Exists(fullPath))
					Directory.CreateDirectory(fullPath);

				// move the file to it's final location
				File.Move(tempName, fileName);



			}


			// if the file is a tvdb fanart also get the thumbnail
			if (req.ImageType == ImageEntityType.TvDB_FanArt)
			{
				fileName = GetFileName(req, true);
				entityID = GetEntityID(req);
				downloadImage = true;
				fileExists = File.Exists(fileName);

				if (fileExists)
				{
					if (!req.ForceDownload)
						downloadImage = false;
					else
						downloadImage = true;
				}
				else
					downloadImage = true;

				if (downloadImage)
				{
					string tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName));
					if (File.Exists(tempName)) File.Delete(tempName);

					OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Started));
					if (fileExists) File.Delete(fileName);

					byte[] imageArray = null;
					try
					{
						imageArray = JMMServerVM.Instance.imageClient.GetImage(entityID, (int)req.ImageType, true);
					}
					catch { }

					if (imageArray == null) return;

					File.WriteAllBytes(tempName, imageArray);

					// move the file to it's final location
					string fullPath = Path.GetDirectoryName(fileName);
					if (!Directory.Exists(fullPath))
						Directory.CreateDirectory(fullPath);

					// move the file to it's final location
					File.Move(tempName, fileName);
				}
			}

			
		}

		private void ProcessImages(object sender, DoWorkEventArgs args)
		{


			foreach (ImageDownloadRequest req in imagesToDownload)
			{
				try
				{
					DownloadImage(req);
					imagesToDownload.Remove(req);
					OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
				}
				catch (Exception ex)
				{
					imagesToDownload.Remove(req);
					OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
					logger.ErrorException(ex.ToString(), ex);
				}
			}

		}
	}
}
