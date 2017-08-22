using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_PosterContainer : INotifyPropertyChangedExt
    {
        public ImageEntityType ImageType { get; set; }
        public object PosterObject { get; set; }

        public VM_PosterContainer(ImageEntityType imageType, object poster)
        {
            ImageType = imageType;
            PosterObject = poster;

            switch (ImageType)
            {
                case ImageEntityType.AniDB_Cover:
                    VM_AniDB_Anime anime = (VM_AniDB_Anime) PosterObject;
                    IsImageEnabled = anime.IsImageEnabled;
                    IsImageDefault = anime.IsImageDefault;
                    PosterSource = "AniDB";
                    break;

                case ImageEntityType.TvDB_Cover:
                    VM_TvDB_ImagePoster tvPoster = (VM_TvDB_ImagePoster) PosterObject;
                    IsImageEnabled = tvPoster.IsImageEnabled;
                    IsImageDefault = tvPoster.IsImageDefault;
                    PosterSource = "TvDB";
                    break;

                case ImageEntityType.MovieDB_Poster:
                    VM_MovieDB_Poster moviePoster = (VM_MovieDB_Poster) PosterObject;
                    IsImageEnabled = moviePoster.IsImageEnabled;
                    IsImageDefault = moviePoster.IsImageDefault;
                    PosterSource = "MovieDB";
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public string FullImagePath
        {
            get
            {
                switch (ImageType)
                {
                    case ImageEntityType.AniDB_Cover:
                        VM_AniDB_Anime anime = (VM_AniDB_Anime) PosterObject;
                        return anime.PosterPath;

                    case ImageEntityType.TvDB_Cover:
                        VM_TvDB_ImagePoster tvPoster = (VM_TvDB_ImagePoster) PosterObject;
                        return tvPoster.FullImagePath;

                    case ImageEntityType.MovieDB_Poster:
                        VM_MovieDB_Poster moviePoster = (VM_MovieDB_Poster) PosterObject;
                        return moviePoster.FullImagePath;
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
                this.SetField(()=>isImageEnabled,value);
            }
        }

      

        private bool isImageDefault;
        public bool IsImageDefault
        {
            get { return isImageDefault; }
            set
            {
                this.SetField(()=>isImageDefault,value);
            }
        }



        private string posterSource = "";
        public string PosterSource
        {
            get { return posterSource; }
            set
            {
                this.SetField(()=>posterSource,value);
            }
        }
    }
}