using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Azure;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_TraktV2 : Azure_CrossRef_AniDB_Trakt, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string IsAdminApprovedImage => IsAdminApproved == 1 ? @"/Images/16_tick.png" : @"/Images/placeholder.png";

        public new int IsAdminApproved
        {
            get { return base.IsAdminApproved; }
            set { this.SetField(()=>base.IsAdminApproved,(r)=> base.IsAdminApproved = r, value, () => IsAdminApproved, () => IsAdminApprovedBool, () => IsAdminApprovedImage); }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsAdminApprovedBool => IsAdminApproved == 1;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string ShowURL => this.GetShowURL();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDBURL => this.GetAniDBURL();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDBStartEpisodeTypeString => this.GetAniDBStartEpisodeTypeString();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDBStartEpisodeNumberString => this.GetAniDBStartEpisodeNumberString();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string TraktSeasonNumberString => this.GetTraktSeasonNumberString();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string TraktStartEpisodeNumberString => this.GetTraktStartEpisodeNumberString();
    }
}
