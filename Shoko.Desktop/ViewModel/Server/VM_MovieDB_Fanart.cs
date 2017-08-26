using System.ComponentModel;
using System.IO;
using System.Web.Script.Serialization;
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
    public class VM_MovieDB_Fanart : MovieDB_Fanart, INotifyPropertyChanged, INotifyPropertyChangedExt
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullImagePathPlain
        {
            get
            {
                //strip out the base URL
                int pos = URL?.IndexOf('/', 0) ?? -1;
                if (pos == -1) return null;
                string fname = URL.Substring(pos + 1, URL.Length - pos - 1);
                fname = fname.Replace("/", @"\");
                string filename = Path.Combine(Utils.GetMovieDBImagePath(), fname);

                return filename;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullImagePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FullImagePathPlain) && !File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_FanArt, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsImageEnabled
        {
            get => base.Enabled == 1;
            set => base.Enabled = value ? 1 : 0;
        }


        public new int Enabled
        {
            get => base.Enabled;
            set => this.SetField(()=>base.Enabled,(r)=> base.Enabled = r, value, () => Enabled, () => IsImageEnabled);
        }
        private bool isImageDefault;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsImageDefault
        {
            get => isImageDefault;
            set => this.SetField(()=>isImageDefault,value);
        }
    }
}
