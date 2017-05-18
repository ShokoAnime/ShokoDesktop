using NLog;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.ImageDownload
{
    public class ImageDownloader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string QUEUE_STOP = "StopQueue";
        private ConcurrentQueue<ImageDownloadRequest> imagesToDownload = new ConcurrentQueue<ImageDownloadRequest>();
        //private BlockingList<ImageDownloadRequest> imagesToDownload = new BlockingList<ImageDownloadRequest>();
        private BackgroundWorker workerImages = new BackgroundWorker();
        private static object downloadsLock = new object();

        public int QueueCount => imagesToDownload.Count;

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
            workerImages.RunWorkerAsync();
        }

        public void DownloadAniDBCover(VM_AniDB_Anime anime, bool forceDownload)
        {
            if (string.IsNullOrEmpty(anime.Picname)) return;

            try
            {
                if (anime.AnimeID == 8580)
                    Console.Write("");

                string url = string.Format(Models.Constants.URLS.AniDB_Images, anime.Picname);
                string filename = anime.PosterPathNoDefault;

                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, anime, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(anime.PosterPath))
                    {
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTvDBPoster(VM_TvDB_ImagePoster poster, bool forceDownload)
        {
            if (string.IsNullOrEmpty(poster.BannerPath)) return;

            try
            {
                string url = string.Format(Models.Constants.URLS.TvDB_Images, poster.BannerPath);
                string filename = poster.FullImagePath;

                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Cover, poster, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(poster.FullImagePath))
                    {
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTvDBWideBanner(VM_TvDB_ImageWideBanner wideBanner, bool forceDownload)
        {
            if (string.IsNullOrEmpty(wideBanner.BannerPath)) return;

            try
            {
                string url = string.Format(Models.Constants.URLS.TvDB_Images, wideBanner.BannerPath);
                string filename = wideBanner.FullImagePath;

                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Banner, wideBanner, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(wideBanner.FullImagePath))
                    {
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTvDBEpisode(VM_TvDB_Episode episode, bool forceDownload)
        {
            if (string.IsNullOrEmpty(episode.Filename)) return;

            try
            {
                string url = string.Format(Models.Constants.URLS.TvDB_Images, episode.Filename);
                string filename = episode.FullImagePath;

                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Episode, episode, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(episode.FullImagePath))
                    {
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTvDBFanart(VM_TvDB_ImageFanart fanart, bool forceDownload)
        {
            if (string.IsNullOrEmpty(fanart.BannerPath)) return;

            try
            {
                string url = string.Format(Models.Constants.URLS.TvDB_Images, fanart.BannerPath);
                string filename = fanart.FullImagePath;

                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, fanart, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(fanart.FullImagePath) || !File.Exists(fanart.FullThumbnailPath))
                    {
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadMovieDBPoster(VM_MovieDB_Poster poster, bool forceDownload)
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
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadMovieDBFanart(VM_MovieDB_Fanart fanart, bool forceDownload)
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
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTraktPoster(VM_Trakt_ImagePoster poster, bool forceDownload)
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
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTraktFanart(VM_Trakt_ImageFanart fanart, bool forceDownload)
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
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void DownloadTraktEpisode(VM_Trakt_Episode episode, bool forceDownload)
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
                        imagesToDownload.Enqueue(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Enqueue(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
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

                    VM_AniDB_Anime anime = req.ImageData as VM_AniDB_Anime;
                    return anime.PosterPathNoDefaultPlain;

                case ImageEntityType.TvDB_Cover:

                    VM_TvDB_ImagePoster poster = req.ImageData as VM_TvDB_ImagePoster;
                    return poster.FullImagePathPlain;

                case ImageEntityType.TvDB_Banner:

                    VM_TvDB_ImageWideBanner banner = req.ImageData as VM_TvDB_ImageWideBanner;
                    return banner.FullImagePathPlain;

                case ImageEntityType.TvDB_Episode:

                    VM_TvDB_Episode episode = req.ImageData as VM_TvDB_Episode;
                    return episode.FullImagePathPlain;

                case ImageEntityType.TvDB_FanArt:

                    VM_TvDB_ImageFanart fanart = req.ImageData as VM_TvDB_ImageFanart;

                    if (thumbNailOnly)
                        return fanart.FullThumbnailPathPlain;
                    else
                        return fanart.FullImagePathPlain;

                case ImageEntityType.MovieDB_Poster:

                    VM_MovieDB_Poster moviePoster = req.ImageData as VM_MovieDB_Poster;
                    return moviePoster.FullImagePathPlain;

                case ImageEntityType.MovieDB_FanArt:

                    VM_MovieDB_Fanart movieFanart = req.ImageData as VM_MovieDB_Fanart;
                    return movieFanart.FullImagePathPlain;

                case ImageEntityType.Trakt_Poster:

                    VM_Trakt_ImagePoster traktPoster = req.ImageData as VM_Trakt_ImagePoster;
                    return traktPoster.FullImagePathPlain;

                case ImageEntityType.Trakt_Fanart:

                    VM_Trakt_ImageFanart trakFanart = req.ImageData as VM_Trakt_ImageFanart;
                    return trakFanart.FullImagePathPlain;

                case ImageEntityType.Trakt_Episode:

                    VM_Trakt_Episode trakEp = req.ImageData as VM_Trakt_Episode;
                    return trakEp.FullImagePathPlain;

                case ImageEntityType.AniDB_Character:

                    VM_AniDB_Character chr = req.ImageData as VM_AniDB_Character;
                    return chr.ImagePathPlain;

                case ImageEntityType.AniDB_Creator:

                    VM_AniDB_Seiyuu cre = req.ImageData as VM_AniDB_Seiyuu;
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

                    VM_AniDB_Anime anime = req.ImageData as VM_AniDB_Anime;
                    return anime.AnimeID.ToString();

                case ImageEntityType.TvDB_Cover:

                    VM_TvDB_ImagePoster poster = req.ImageData as VM_TvDB_ImagePoster;
                    return poster.TvDB_ImagePosterID.ToString();

                case ImageEntityType.TvDB_Banner:

                    VM_TvDB_ImageWideBanner banner = req.ImageData as VM_TvDB_ImageWideBanner;
                    return banner.TvDB_ImageWideBannerID.ToString();

                case ImageEntityType.TvDB_Episode:

                    VM_TvDB_Episode episode = req.ImageData as VM_TvDB_Episode;
                    return episode.TvDB_EpisodeID.ToString();

                case ImageEntityType.TvDB_FanArt:

                    VM_TvDB_ImageFanart fanart = req.ImageData as VM_TvDB_ImageFanart;
                    return fanart.TvDB_ImageFanartID.ToString();

                case ImageEntityType.MovieDB_Poster:

                    VM_MovieDB_Poster moviePoster = req.ImageData as VM_MovieDB_Poster;
                    return moviePoster.MovieDB_PosterID.ToString();

                case ImageEntityType.MovieDB_FanArt:

                    VM_MovieDB_Fanart movieFanart = req.ImageData as VM_MovieDB_Fanart;
                    return movieFanart.MovieDB_FanartID.ToString();

                case ImageEntityType.Trakt_Poster:

                    VM_Trakt_ImagePoster traktPoster = req.ImageData as VM_Trakt_ImagePoster;
                    return traktPoster.Trakt_ImagePosterID.ToString();

                case ImageEntityType.Trakt_Fanart:

                    VM_Trakt_ImageFanart trakFanart = req.ImageData as VM_Trakt_ImageFanart;
                    return trakFanart.Trakt_ImageFanartID.ToString();

                case ImageEntityType.Trakt_CommentUser:

                    VM_Trakt_CommentUser traktShoutUser = req.ImageData as VM_Trakt_CommentUser;
                    return traktShoutUser.User.Trakt_FriendID.ToString();

                case ImageEntityType.Trakt_Episode:

                    VM_Trakt_Episode trakEp = req.ImageData as VM_Trakt_Episode;
                    return trakEp.Trakt_EpisodeID.ToString();

                case ImageEntityType.AniDB_Character:

                    VM_AniDB_Character chr = req.ImageData as VM_AniDB_Character;
                    return chr.AniDB_CharacterID.ToString();

                case ImageEntityType.AniDB_Creator:

                    VM_AniDB_Seiyuu cre = req.ImageData as VM_AniDB_Seiyuu;
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
                            using (Stream img = VM_ShokoServer.Instance.ShokoImages.GetImage(int.Parse(entityID), (int) req.ImageType, false))
                            using (Stream wstream = File.OpenWrite(tempName))
                            {
                                img.CopyTo(wstream);
                            }
                        }
                        catch
                        {
                            return;
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
                                using (Stream img = VM_ShokoServer.Instance.ShokoImages.GetImage(int.Parse(entityID), (int)req.ImageType, false))
                                using (Stream wstream = File.OpenWrite(tempName))
                                {
                                    img.CopyTo(wstream);
                                }
                            }
                            catch
                            {
                                return;
                            }


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
                logger.Error(ex, ex.ToString());
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
                            logger.Error(ex, ex.ToString());
                        }
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
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
                    logger.Error(ex, ex.ToString());
                }
            }
            */
        }
    }
}
