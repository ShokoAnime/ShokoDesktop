using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Seiyuu : AniDB_Seiyuu
    {

        [JsonIgnore, XmlIgnore]
        public string ImagePathPlain => string.IsNullOrEmpty(PicName) ? "" : Path.Combine(Utils.GetAniDBCreatorImagePath(SeiyuuID), PicName);

        [JsonIgnore, XmlIgnore]
        public string ImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePathPlain)) return ImagePathPlain;

                if (!File.Exists(ImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Creator, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(ImagePathPlain)) return ImagePathPlain;

                    string packUriBlank = $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
                    return packUriBlank;
                }

                return ImagePathPlain;
            }
        }


    }
}
