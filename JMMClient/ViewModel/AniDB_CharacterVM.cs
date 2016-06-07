using JMMClient.ImageDownload;
using System;
using System.ComponentModel;
using System.IO;

namespace JMMClient.ViewModel
{
    public class AniDB_CharacterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public int AniDB_CharacterID { get; set; }
        public string CharDescription { get; set; }
        public string CharName { get; set; }
        public int CharID { get; set; }
        public string CharKanjiName { get; set; }
        public string CharType { get; set; }

        public string PicName { get; set; }


        public AniDB_SeiyuuVM Seiyuu { get; set; }

        public override string ToString()
        {
            return string.Format("CHAR: {0} - {1} ({2})", CharID, CharName, ImagePath);
        }

        public string ImagePathPlain
        {
            get
            {
                string fileName = Path.Combine(Utils.GetAniDBCharacterImagePath(CharID), PicName);

                return fileName;
            }
        }

        public string CharNameShort
        {
            get
            {
                if (CharName.Length <= 25) return CharName;
                return CharName.Substring(0, 24) + "...";
            }
        }

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

                    string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);
                    return packUriBlank;
                }
                return fileName;
            }
        }

        public AniDB_CharacterVM()
        {
        }

        public void Populate(JMMServerBinary.Contract_AniDB_Character contract)
        {
            this.AniDB_CharacterID = contract.AniDB_CharacterID;
            this.CharID = contract.CharID;
            this.PicName = contract.PicName;
            this.CharName = contract.CharName;
            this.CharKanjiName = contract.CharKanjiName;
            this.CharDescription = contract.CharDescription;

            this.CharType = contract.CharType;

            if (contract.Seiyuu != null)
                this.Seiyuu = new AniDB_SeiyuuVM(contract.Seiyuu);

        }

        public AniDB_CharacterVM(JMMServerBinary.Contract_AniDB_Character contract)
        {
            Populate(contract);
        }
    }
}
