using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace JMMClient.ImageDownload
{
    public class ImageDownloader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string QUEUE_STOP = "StopQueue";
        private ConcurrentQueue<ImageDownloadRequest> imagesToDownload = new ConcurrentQueue<ImageDownloadRequest>();
        //private BlockingList<ImageDownloadRequest> imagesToDownload = new BlockingList<ImageDownloadRequest>();
        private BackgroundWorker workerImages = new BackgroundWorker();
        private static object downloadsLock = new object();

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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                        this.imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                this.imagesToDownload.Enqueue(req);
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
                    return anime.PosterPathNoDefaultPlain;

                case ImageEntityType.TvDB_Cover:

                    TvDB_ImagePosterVM poster = req.ImageData as TvDB_ImagePosterVM;
                    return poster.FullImagePathPlain;

                case ImageEntityType.TvDB_Banner:

                    TvDB_ImageWideBannerVM banner = req.ImageData as TvDB_ImageWideBannerVM;
                    return banner.FullImagePathPlain;

                case ImageEntityType.TvDB_Episode:

                    TvDB_EpisodeVM episode = req.ImageData as TvDB_EpisodeVM;
                    return episode.FullImagePathPlain;

                case ImageEntityType.TvDB_FanArt:

                    TvDB_ImageFanartVM fanart = req.ImageData as TvDB_ImageFanartVM;

                    if (thumbNailOnly)
                        return fanart.FullThumbnailPathPlain;
                    else
                        return fanart.FullImagePathPlain;

                case ImageEntityType.MovieDB_Poster:

                    MovieDB_PosterVM moviePoster = req.ImageData as MovieDB_PosterVM;
                    return moviePoster.FullImagePathPlain;

                case ImageEntityType.MovieDB_FanArt:

                    MovieDB_FanartVM movieFanart = req.ImageData as MovieDB_FanartVM;
                    return movieFanart.FullImagePathPlain;

                case ImageEntityType.Trakt_Poster:

                    Trakt_ImagePosterVM traktPoster = req.ImageData as Trakt_ImagePosterVM;
                    return traktPoster.FullImagePathPlain;

                case ImageEntityType.Trakt_Fanart:

                    Trakt_ImageFanartVM trakFanart = req.ImageData as Trakt_ImageFanartVM;
                    return trakFanart.FullImagePathPlain;

                case ImageEntityType.Trakt_Episode:

                    Trakt_EpisodeVM trakEp = req.ImageData as Trakt_EpisodeVM;
                    return trakEp.FullImagePathPlain;

                case ImageEntityType.AniDB_Character:

                    AniDB_CharacterVM chr = req.ImageData as AniDB_CharacterVM;
                    return chr.ImagePathPlain;

                case ImageEntityType.AniDB_Creator:

                    AniDB_SeiyuuVM cre = req.ImageData as AniDB_SeiyuuVM;
                    return cre.ImagePathPlain;

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

                case ImageEntityType.Trakt_CommentUser:

                    Trakt_CommentUserVM traktShoutUser = req.ImageData as Trakt_CommentUserVM;
                    return traktShoutUser.User.Trakt_FriendID.ToString();

                case ImageEntityType.Trakt_Episode:

                    Trakt_EpisodeVM trakEp = req.ImageData as Trakt_EpisodeVM;
                    return trakEp.Trakt_EpisodeID.ToString();

                case ImageEntityType.AniDB_Character:

                    AniDB_CharacterVM chr = req.ImageData as AniDB_CharacterVM;
                    return chr.AniDB_CharacterID.ToString();

                case ImageEntityType.AniDB_Creator:

                    AniDB_SeiyuuVM cre = req.ImageData as AniDB_SeiyuuVM;
                    return cre.AniDB_SeiyuuID.ToString();

                default:
                    return "";
            }

        }

        public void DownloadImage(ImageDownloadRequest req)
        {
            try
            {
                lock (downloadsLock)
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

                        try
                        {
                            Stream ImageStream = JMMServerVM.GetImage(entityID, (int)req.ImageType, "0");
                            if (ImageStream == null) return;
                            using (var fileStream = File.Create(tempName))
                            {
                                ImageStream.Seek(0, SeekOrigin.Begin);
                                ImageStream.CopyTo(fileStream);
                            }

                        }

                        catch (Exception ex)
                        {
                            string x = ex.Message;
                        }  

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

                            try
                            {
                                Stream ImageStream = JMMServerVM.GetImage(entityID, (int)req.ImageType, "0");
                                if (ImageStream == null) return;
                                using (var fileStream = File.Create(tempName))
                                {
                                    ImageStream.Seek(0, SeekOrigin.Begin);
                                    ImageStream.CopyTo(fileStream);
                                }

                            }

                            catch (Exception ex)
                            {
                                string x = ex.Message;
                            }

                            //File.WriteAllBytes(tempName, imageArray);

                            // move the file to it's final location
                            string fullPath = Path.GetDirectoryName(fileName);
                            if (!Directory.Exists(fullPath))
                                Directory.CreateDirectory(fullPath);

                            // move the file to it's final location
                            File.Move(tempName, fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }

        }

        private void ProcessImages(object sender, DoWorkEventArgs args)
        {
            while (true)
            {
                while (!imagesToDownload.IsEmpty)
                {
                    ImageDownloadRequest req;
                    if (imagesToDownload.TryDequeue(out req))
                    {
                        try
                        {
                            DownloadImage(req);                        
                        }
                        catch (Exception ex)
                        {
                            logger.ErrorException(ex.ToString(), ex);
                        }
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
                    }
                }
                Thread.Sleep(100);
            }
            /*
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
            */
        }
    }
}
