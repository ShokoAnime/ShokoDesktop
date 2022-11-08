using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.ImageDownload;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Character : CL_AniDB_Character, INotifyPropertyChanged, INotifyPropertyChangedExt
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


        [JsonIgnore, XmlIgnore]
        public string ImagePathPlain => string.Intern(Path.Combine(Utils.GetAniDBCharacterImagePath(CharID), PicName));

        [JsonIgnore, XmlIgnore]
        public string CharNameShort => CharName.Length <= 25 ? CharName : string.Intern(CharName.Substring(0, 24) + "...");

        [JsonIgnore, XmlIgnore]
        public string ImagePath
        {
            get
            {
                string fileName = string.Intern(Path.Combine(Utils.GetAniDBCharacterImagePath(CharID), PicName));

                if (!File.Exists(fileName))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Character, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(fileName)) return fileName;

                    string packUriBlank = string.Intern($"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png");
                    return packUriBlank;
                }
                return fileName;
            }
        }

        public new string PicName
        {
            get => base.PicName == null ? null : string.Intern(base.PicName);
            set => base.PicName = value == null ? null : string.Intern(value);
        }

        public new string CreatorListRaw
        {
            get => base.CreatorListRaw == null ? null : string.Intern(base.CreatorListRaw);
            set => base.CreatorListRaw = value == null ? null : string.Intern(value);
        }

        public new string CharName
        {
            get => base.CharName == null ? null : string.Intern(base.CharName);
            set => base.CharName = value == null ? null : string.Intern(value);
        }

        public new string CharKanjiName
        {
            get => base.CharKanjiName == null ? null : string.Intern(base.CharKanjiName);
            set => base.CharKanjiName = value == null ? null : string.Intern(value);
        }

        public new string CharDescription
        {
            get => base.CharDescription == null ? null : string.Intern(base.CharDescription);
            set => base.CharDescription = value == null ? null : string.Intern(value);
        }

        public new string CharType
        {
            get => base.CharType == null ? null : string.Intern(base.CharType);
            set => base.CharType = value == null ? null : string.Intern(value);
        }
    }
}