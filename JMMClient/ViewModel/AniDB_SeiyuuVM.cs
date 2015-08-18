using JMMClient.ImageDownload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
    public class AniDB_SeiyuuVM
    {
        public int AniDB_SeiyuuID { get; set; }
        public int SeiyuuID { get; set; }
        public string SeiyuuName { get; set; }
        public string PicName { get; set; }


        public string ImagePathPlain
        {
            get
            {
                if (string.IsNullOrEmpty(PicName)) return "";

                return Path.Combine(Utils.GetAniDBCreatorImagePath(SeiyuuID), PicName);
            }
        }

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

                    string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);
                    return packUriBlank;
                }

                return ImagePathPlain;
            }
        }

        public AniDB_SeiyuuVM(JMMServerBinary.Contract_AniDB_Seiyuu details)
        {
            if (details == null) return;

            this.AniDB_SeiyuuID = details.AniDB_SeiyuuID;
            this.SeiyuuID = details.SeiyuuID;
            this.SeiyuuName = details.SeiyuuName;
            this.PicName = details.PicName;
        }
    }
}
