using System.ComponentModel;
using System.IO;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Character : CL_AniDB_Character, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }



        public new VM_AniDB_Seiyuu Seiyuu
        {
            get { return (VM_AniDB_Seiyuu) base.Seiyuu; }
            set { base.Seiyuu = value; }
        }



        public string ImagePathPlain => Path.Combine(Utils.GetAniDBCharacterImagePath(CharID), PicName);

        public string CharNameShort => CharName.Length <= 25 ? CharName : CharName.Substring(0, 24) + "...";

        public string ImagePath
        {
            get
            {
                string fileName = Path.Combine(Utils.GetAniDBCharacterImagePath(CharID), PicName);

                if (!File.Exists(fileName))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Character, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(fileName)) return fileName;

                    string packUriBlank = $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
                    return packUriBlank;
                }
                return fileName;
            }
        }


    }
}
