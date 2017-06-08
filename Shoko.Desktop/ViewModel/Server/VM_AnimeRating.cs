using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeRating : CL_AnimeRating, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AnimeName => AnimeDetailed.AniDBAnime.MainTitle;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string Rating => $"{AnimeDetailed.AniDBAnime.AniDBRating:0.00}";

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int Year => AnimeDetailed.AniDBAnime.BeginYear;

        private decimal userRating = -1;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
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
                this.SetField(()=>userRating,value);
            }
        }

        public new VM_AniDB_AnimeDetailed AnimeDetailed
        {
            get { return (VM_AniDB_AnimeDetailed)base.AnimeDetailed; }
            set { this.SetField(()=>base.AnimeDetailed,(r)=> base.AnimeDetailed = r, value); }
        }

        
        public new VM_AnimeSeries_User AnimeSeries
        {
            get { return (VM_AnimeSeries_User)base.AnimeSeries; }
            set { this.SetField(()=>base.AnimeSeries,(r)=> base.AnimeSeries = r, value); }
        }

    }
}
