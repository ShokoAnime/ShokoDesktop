using System.ComponentModel;

using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Azure;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_TvDBV2 : Azure_CrossRef_AniDB_TvDB, INotifyPropertyChanged, INotifyPropertyChangedExt
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
        [JsonIgnore, XmlIgnore]
        public string IsAdminApprovedImage => IsAdminApproved == 1 ? @"/Images/16_tick.png" : @"/Images/placeholder.png";

        public new int IsAdminApproved
        {
            get { return base.IsAdminApproved; }
            set { this.SetField(()=>base.IsAdminApproved,(r)=> base.IsAdminApproved = r, value, ()=>IsAdminApproved, ()=>IsAdminApprovedBool,()=> IsAdminApprovedImage); }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsAdminApprovedBool => IsAdminApproved==1;
        [JsonIgnore, XmlIgnore]
        public string SeriesURL => this.GetSeriesURL();
        [JsonIgnore, XmlIgnore]
        public string AniDBURL => this.GetAniDBURL();
        [JsonIgnore, XmlIgnore]
        public string AniDBStartEpisodeTypeString => this.GetAniDBStartEpisodeTypeString();
        [JsonIgnore, XmlIgnore]
        public string AniDBStartEpisodeNumberString => this.GetAniDBStartEpisodeNumberString();
        [JsonIgnore, XmlIgnore]
        public string TvDBSeasonNumberString => this.GetTvDBSeasonNumberString();
        [JsonIgnore, XmlIgnore]
        public string TvDBStartEpisodeNumberString => this.GetTvDBStartEpisodeNumberString();

    }
}
