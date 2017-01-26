using System.ComponentModel;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_FanartContainer : INotifyPropertyChangedExt
    {
        public ImageEntityType ImageType { get; set; }
        public object FanartObject { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public VM_FanartContainer(ImageEntityType imageType, object poster)
        {
            ImageType = imageType;
            FanartObject = poster;

            switch (ImageType)
            {
                case ImageEntityType.TvDB_FanArt:
                    VM_TvDB_ImageFanart tvFanart = (VM_TvDB_ImageFanart) FanartObject;
                    IsImageEnabled = tvFanart.IsImageEnabled;
                    IsImageDefault = tvFanart.IsImageDefault;
                    FanartSource = "TvDB";
                    break;

                case ImageEntityType.MovieDB_FanArt:
                    VM_MovieDB_Fanart movieFanart = (VM_MovieDB_Fanart) FanartObject;
                    IsImageEnabled = movieFanart.IsImageEnabled;
                    IsImageDefault = movieFanart.IsImageDefault;
                    FanartSource = "MovieDB";
                    break;

                case ImageEntityType.Trakt_Fanart:
                    VM_Trakt_ImageFanart traktFanart = (VM_Trakt_ImageFanart) FanartObject;
                    IsImageEnabled = traktFanart.IsImageEnabled;
                    IsImageDefault = traktFanart.IsImageDefault;
                    FanartSource = "Trakt";
                    break;
            }


        }

        public string FullImagePath
        {
            get
            {
                switch (ImageType)
                {

                    case ImageEntityType.TvDB_FanArt:
                        VM_TvDB_ImageFanart tvFanart = (VM_TvDB_ImageFanart) FanartObject;
                        return tvFanart.FullImagePath;

                    case ImageEntityType.MovieDB_FanArt:
                        VM_MovieDB_Fanart movieFanart = (VM_MovieDB_Fanart) FanartObject;
                        return movieFanart.FullImagePath;

                    case ImageEntityType.Trakt_Fanart:
                        VM_Trakt_ImageFanart traktFanart = (VM_Trakt_ImageFanart) FanartObject;
                        return traktFanart.FullImagePath;
                }

                return "";
            }
        }

        public string FullThumbnailPath
        {
            get
            {
                switch (ImageType)
                {

                    case ImageEntityType.TvDB_FanArt:
                        VM_TvDB_ImageFanart tvFanart = (VM_TvDB_ImageFanart) FanartObject;
                        return tvFanart.FullThumbnailPath;

                    case ImageEntityType.MovieDB_FanArt:
                        VM_MovieDB_Fanart movieFanart = (VM_MovieDB_Fanart) FanartObject;
                        return movieFanart.FullImagePath;

                    case ImageEntityType.Trakt_Fanart:
                        VM_Trakt_ImageFanart traktFanart = (VM_Trakt_ImageFanart) FanartObject;
                        return traktFanart.FullImagePath;
                }

                return "";
            }
        }

        private bool isImageEnabled;
        public bool IsImageEnabled
        {
            get { return isImageEnabled; }
            set
            {
                isImageEnabled = this.SetField(isImageEnabled, value);
            }
        }



        private bool isImageDefault;
        public bool IsImageDefault
        {
            get { return isImageDefault; }
            set
            {
                isImageDefault = this.SetField(isImageDefault, value);
            }
        }



        private string fanartSource = "";
        public string FanartSource
        {
            get { return fanartSource; }
            set
            {
                fanartSource = this.SetField(fanartSource, value);
            }
        }
    }
}