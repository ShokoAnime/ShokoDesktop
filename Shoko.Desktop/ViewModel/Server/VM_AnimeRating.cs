using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeRating : CL_AnimeRating, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public string AnimeName => AnimeDetailed.AniDBAnime.MainTitle;
        public string Rating => $"{AnimeDetailed.AniDBAnime.AniDBRating:0.00}";

        public int Year => AnimeDetailed.AniDBAnime.BeginYear;

        private decimal userRating = -1;
        public decimal UserRating
        {
            get
            {
                if (userRating == -1)
                    UserRating = AnimeDetailed.UserRating;
                return userRating;
            }
            set
            {
                userRating = this.SetField(userRating, value);
            }
        }

        public new VM_AniDB_AnimeDetailed AnimeDetailed
        {
            get { return (VM_AniDB_AnimeDetailed)base.AnimeDetailed; }
            set { base.AnimeDetailed = this.SetField(base.AnimeDetailed, value); }
        }

        
        public new VM_AnimeSeries_User AnimeSeries
        {
            get { return (VM_AnimeSeries_User)base.AnimeSeries; }
            set { base.AnimeSeries = this.SetField(base.AnimeSeries, value); }
        }

    }
}
