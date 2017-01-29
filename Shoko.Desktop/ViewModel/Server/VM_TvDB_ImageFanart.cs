using System.ComponentModel;
using System.IO;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_TvDB_ImageFanart : TvDB_ImageFanart, INotifyPropertyChanged, INotifyPropertyChangedExt
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public string FullImagePathPlain => Path.Combine(Utils.GetTvDBImagePath(), BannerPath.Replace("/", @"\"));

        public string FullImagePath
        {
            get
            {
                if (!File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }

        public string FullThumbnailPathPlain => Path.Combine(Utils.GetTvDBImagePath(), ThumbnailPath.Replace("/", @"\"));

        public string FullThumbnailPath
        {
            get
            {
                if (!File.Exists(FullThumbnailPathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullThumbnailPathPlain)) return FullThumbnailPathPlain;
                }

                return FullThumbnailPathPlain;
            }
        }

        public bool IsImageEnabled
        {
            get { return base.Enabled == 1; }
            set { base.Enabled = value ? 1 : 0; }
        }


        public new int Enabled
        {
            get { return base.Enabled; }
            set { base.Enabled = this.SetField(base.Enabled, value, () => Enabled, () => IsImageEnabled); }
        }
        private bool isImageDefault;
        public bool IsImageDefault
        {
            get { return isImageDefault; }
            set { isImageDefault = this.SetField(isImageDefault, value); }
        }


    }
}
