using System.IO;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_TvDB_Episode : TvDB_Episode
    {
        public string ImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(Filename)) return @"/Images/EpisodeThumb_NotFound.png";

                if (File.Exists(FullImagePath)) return FullImagePath;

                return OnlineImagePath;
            }
        }

        public string FullImagePathPlain => string.IsNullOrEmpty(Filename) ? "" : Path.Combine(Utils.GetTvDBImagePath(), Filename.Replace("/", @"\"));

        public string FullImagePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FullImagePathPlain) && !File.Exists(FullImagePathPlain))
                {
                    //logger.Trace("TvDB_EpisodeVM: downloading image\n - {0}\n", FullImagePathPlain);
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Episode, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    //logger.Trace("TvDB_EpisodeVM: downloading image done");
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }

        public string OnlineImagePath => string.IsNullOrEmpty(Filename) ? "" : string.Format(Models.Constants.URLS.TvDB_Images, Filename);
    }
}
