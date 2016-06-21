using JMMClient.ImageDownload;
using System;
using System.ComponentModel;
using System.IO;

namespace JMMClient.ViewModel
{
    public class TvDB_ImageWideBannerVM : INotifyPropertyChanged
    {
        public int TvDB_ImageWideBannerID { get; set; }
        public int Id { get; set; }
        public int SeriesID { get; set; }
        public string BannerPath { get; set; }
        public string BannerType { get; set; }
        public string BannerType2 { get; set; }
        public string Language { get; set; }
        public int Enabled { get; set; }
        public int? SeasonNumber { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public string FullImagePathPlain
        {
            get
            {
                string fname = BannerPath;
                fname = BannerPath.Replace("/", @"\");
                string filename = Path.Combine(Utils.GetTvDBImagePath(), fname);

                return filename;
            }
        }

        public string FullImagePath
        {
            get
            {
                if (!File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Banner, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }

        private bool isImageEnabled = false;
        public bool IsImageEnabled
        {
            get { return isImageEnabled; }
            set
            {
                isImageEnabled = value;
                NotifyPropertyChanged("IsImageEnabled");
            }
        }

        private bool isImageDisabled = false;
        public bool IsImageDisabled
        {
            get { return isImageDisabled; }
            set
            {
                isImageDisabled = value;
                NotifyPropertyChanged("IsImageDisabled");
            }
        }

        private bool isImageDefault = false;
        public bool IsImageDefault
        {
            get { return isImageDefault; }
            set
            {
                isImageDefault = value;
                NotifyPropertyChanged("IsImageDefault");
            }
        }

        private bool isImageNotDefault = false;
        public bool IsImageNotDefault
        {
            get { return isImageNotDefault; }
            set
            {
                isImageNotDefault = value;
                NotifyPropertyChanged("IsImageNotDefault");
            }
        }

        public TvDB_ImageWideBannerVM(JMMServerBinary.Contract_TvDB_ImageWideBanner contract)
        {
            this.TvDB_ImageWideBannerID = contract.TvDB_ImageWideBannerID;
            this.Id = contract.Id;
            this.SeriesID = contract.SeriesID;
            this.BannerPath = contract.BannerPath;
            this.BannerType = contract.BannerType;
            this.BannerType2 = contract.BannerType2;
            this.Language = contract.Language;
            this.Enabled = contract.Enabled;
            this.SeasonNumber = contract.SeasonNumber;

            IsImageEnabled = Enabled == 1;
            IsImageDisabled = Enabled != 1;
        }
    }
}
