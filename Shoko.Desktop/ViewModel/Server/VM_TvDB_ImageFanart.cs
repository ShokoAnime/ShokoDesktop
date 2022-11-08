using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
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

        [JsonIgnore, XmlIgnore]
        public string FullImagePathPlain => string.Intern(Path.Combine(Utils.GetTvDBImagePath(), BannerPath.Replace("/", @"\")));

        [JsonIgnore, XmlIgnore]
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

        [JsonIgnore, XmlIgnore]
        public bool IsImageEnabled
        {
            get { return base.Enabled == 1; }
            set { base.Enabled = value ? 1 : 0; }
        }


        public new int Enabled
        {
            get { return base.Enabled; }
            set { this.SetField(()=>base.Enabled,(r)=> base.Enabled = r, value, () => Enabled, () => IsImageEnabled); }
        }
        private bool isImageDefault;
        [JsonIgnore, XmlIgnore]
        public bool IsImageDefault
        {
            get { return isImageDefault; }
            set { this.SetField(()=>isImageDefault,value); }
        }

        public new string BannerPath
        {
            get => base.BannerPath == null ? null : string.Intern(base.BannerPath);
            set => base.BannerPath = value == null ? null : string.Intern(value);
        }

        public new string BannerType
        {
            get => base.BannerType == null ? null : string.Intern(base.BannerType);
            set => base.BannerType = value == null ? null : string.Intern(value);
        }

        public new string BannerType2
        {
            get => base.BannerType2 == null ? null : string.Intern(base.BannerType2);
            set => base.BannerType2 = value == null ? null : string.Intern(value);
        }

        public new string Colors
        {
            get => base.Colors == null ? null : string.Intern(base.Colors);
            set => base.Colors = value == null ? null : string.Intern(value);
        }

        public new string Language
        {
            get => base.Language == null ? null : string.Intern(base.Language);
            set => base.Language = value == null ? null : string.Intern(value);
        }

        public new string VignettePath
        {
            get => base.VignettePath == null ? null : string.Intern(base.VignettePath);
            set => base.VignettePath = value == null ? null : string.Intern(value);
        }
    }
}
