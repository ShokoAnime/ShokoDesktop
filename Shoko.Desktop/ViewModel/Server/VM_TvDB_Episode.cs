using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_TvDB_Episode : TvDB_Episode
    {
        public new string EpisodeName
        {
            get => base.EpisodeName == null ? null : string.Intern(base.EpisodeName);
            set => base.EpisodeName = value == null ? null : string.Intern(value);
        }

        public new string Overview
        {
            get => base.Overview == null ? null : string.Intern(base.Overview);
            set => base.Overview = value == null ? null : string.Intern(value);
        }

        public new string Filename
        {
            get => base.Filename == null ? null : string.Intern(base.Filename);
            set => base.Filename = value == null ? null : string.Intern(value);
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string ImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(Filename)) return string.Intern("/Images/EpisodeThumb_NotFound.png");

                if (File.Exists(FullImagePath)) return FullImagePath;

                return OnlineImagePath;
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullImagePathPlain => string.IsNullOrEmpty(Filename) ? string.Intern("") : string.Intern(Path.Combine(Utils.GetTvDBImagePath(), Filename.Replace("/", @"\")));

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string OnlineImagePath => string.IsNullOrEmpty(Filename) ? string.Intern("") : string.Intern(string.Format(Models.Constants.URLS.TvDB_Images, Filename));
    }
}
