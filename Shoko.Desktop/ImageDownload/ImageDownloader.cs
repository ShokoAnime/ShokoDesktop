using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using NLog;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Enums;

namespace Shoko.Desktop.ImageDownload
{
    public class ImageDownloader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<ImageDownloadRequest> imagesToDownload = new ConcurrentQueue<ImageDownloadRequest>();
        private readonly BackgroundWorker workerImages = new BackgroundWorker();
        private static readonly SemaphoreSlim DownloadsLock = new(8);
        public static bool Stopping = false;

        public int QueueCount => imagesToDownload.Count;

        public ImageDownloader()
        {
            workerImages.WorkerReportsProgress = true;
            workerImages.WorkerSupportsCancellation = true;
            workerImages.DoWork += ProcessImages;
        }

        public delegate void QueueUpdateEventHandler(QueueUpdateEventArgs ev);
        public event QueueUpdateEventHandler QueueUpdateEvent;
        protected void OnQueueUpdateEvent(QueueUpdateEventArgs ev)
        {
            QueueUpdateEvent?.Invoke(ev);
        }

        public delegate void ImageDownloadEventHandler(ImageDownloadEventArgs ev);
        public event ImageDownloadEventHandler ImageDownloadEvent;
        protected void OnImageDownloadEvent(ImageDownloadEventArgs ev)
        {
            ImageDownloadEvent?.Invoke(ev);
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
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, fanart, forceDownload);

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

        public void DownloadMovieDBPoster(VM_MovieDB_Poster poster, bool forceDownload)
        {
            if (string.IsNullOrEmpty(poster.URL)) return;

            try
            {
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

        private string GetFileName(ImageDownloadRequest req)
        {
            switch (req.ImageType)
            {
                case ImageEntityType.AniDB_Cover:

                    VM_AniDB_Anime anime = req.ImageData as VM_AniDB_Anime;
                    return anime?.PosterPathNoDefaultPlain;

                case ImageEntityType.TvDB_Cover:

                    VM_TvDB_ImagePoster poster = req.ImageData as VM_TvDB_ImagePoster;
                    return poster?.FullImagePathPlain;

                case ImageEntityType.TvDB_Banner:

                    VM_TvDB_ImageWideBanner banner = req.ImageData as VM_TvDB_ImageWideBanner;
                    return banner?.FullImagePathPlain;

                case ImageEntityType.TvDB_Episode:

                    VM_TvDB_Episode episode = req.ImageData as VM_TvDB_Episode;
                    return episode?.FullImagePathPlain;

                case ImageEntityType.TvDB_FanArt:

                    VM_TvDB_ImageFanart fanart = req.ImageData as VM_TvDB_ImageFanart;
                    return fanart?.FullImagePathPlain;

                case ImageEntityType.MovieDB_Poster:

                    VM_MovieDB_Poster moviePoster = req.ImageData as VM_MovieDB_Poster;
                    return moviePoster?.FullImagePathPlain;

                case ImageEntityType.MovieDB_FanArt:

                    VM_MovieDB_Fanart movieFanart = req.ImageData as VM_MovieDB_Fanart;
                    return movieFanart?.FullImagePathPlain;

                case ImageEntityType.AniDB_Character:

                    VM_AniDB_Character chr = req.ImageData as VM_AniDB_Character;
                    return chr?.ImagePathPlain;

                case ImageEntityType.AniDB_Creator:

                    VM_AniDB_Seiyuu cre = req.ImageData as VM_AniDB_Seiyuu;
                    return cre?.ImagePathPlain;

                default:
                    return string.Empty;
            }

        }

        private string GetEntityID(ImageDownloadRequest req)
        {
            switch (req.ImageType)
            {
                case ImageEntityType.AniDB_Cover:

                    VM_AniDB_Anime anime = req.ImageData as VM_AniDB_Anime;
                    return anime?.AnimeID.ToString();

                case ImageEntityType.TvDB_Cover:

                    VM_TvDB_ImagePoster poster = req.ImageData as VM_TvDB_ImagePoster;
                    return poster?.TvDB_ImagePosterID.ToString();

                case ImageEntityType.TvDB_Banner:

                    VM_TvDB_ImageWideBanner banner = req.ImageData as VM_TvDB_ImageWideBanner;
                    return banner?.TvDB_ImageWideBannerID.ToString();

                case ImageEntityType.TvDB_Episode:

                    VM_TvDB_Episode episode = req.ImageData as VM_TvDB_Episode;
                    return episode?.TvDB_EpisodeID.ToString();

                case ImageEntityType.TvDB_FanArt:

                    VM_TvDB_ImageFanart fanart = req.ImageData as VM_TvDB_ImageFanart;
                    return fanart?.TvDB_ImageFanartID.ToString();

                case ImageEntityType.MovieDB_Poster:

                    VM_MovieDB_Poster moviePoster = req.ImageData as VM_MovieDB_Poster;
                    return moviePoster?.MovieDB_PosterID.ToString();

                case ImageEntityType.MovieDB_FanArt:

                    VM_MovieDB_Fanart movieFanart = req.ImageData as VM_MovieDB_Fanart;
                    return movieFanart?.MovieDB_FanartID.ToString();

                case ImageEntityType.AniDB_Character:

                    VM_AniDB_Character chr = req.ImageData as VM_AniDB_Character;
                    return chr?.AniDB_CharacterID.ToString();

                case ImageEntityType.AniDB_Creator:

                    VM_AniDB_Seiyuu cre = req.ImageData as VM_AniDB_Seiyuu;
                    return cre?.AniDB_SeiyuuID.ToString();

                default:
                    return string.Empty;
            }

        }

        public void DownloadImage(ImageDownloadRequest req)
        {
            try
            {
                var fileName = GetFileName(req);
                var entityID = GetEntityID(req);
                var downloadImage = true;
                var fileExists = string.IsNullOrEmpty(fileName) || File.Exists(fileName);

                if (fileExists && !req.ForceDownload) downloadImage = false;

                if (downloadImage)
                {
                    var tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName));
                    if (File.Exists(tempName)) File.Delete(tempName);

                    OnImageDownloadEvent(new ImageDownloadEventArgs(string.Empty, req, ImageDownloadEventType.Started));
                    if (fileExists) File.Delete(fileName);

                    try
                    {
                        DownloadsLock.Wait();
                        using var img =
                            (Stream)VM_ShokoServer.Instance.ShokoImages.GetImage(int.Parse(entityID),
                                (int)req.ImageType, false);
                        using var wstream = File.OpenWrite(tempName);
                        img.CopyTo(wstream);

                        DownloadsLock.Release();
                    }
                    catch
                    {
                        return;
                    }

                    // move the file to it's final location
                    var fullPath = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);

                    // move the file to it's final location
                    File.Move(tempName, fileName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }

        }

        private void ProcessImages(object sender, DoWorkEventArgs args)
        {
            while (!Stopping)
            {
                while (!imagesToDownload.IsEmpty)
                {
                    if (imagesToDownload.TryDequeue(out var req))
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
        }
    }
}
