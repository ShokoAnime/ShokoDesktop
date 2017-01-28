using System.ComponentModel;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Azure;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_TvDBV2 : Azure_CrossRef_AniDB_TvDB, INotifyPropertyChangedExt
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
        public string IsAdminApprovedImage => IsAdminApproved == 1 ? @"/Images/16_tick.png" : @"/Images/placeholder.png";

        public new int IsAdminApproved
        {
            get { return base.IsAdminApproved; }
            set { base.IsAdminApproved = this.SetField(base.IsAdminApproved, value, ()=>IsAdminApproved, ()=>IsAdminApprovedBool,()=> IsAdminApprovedImage); }
        }

        public bool IsAdminApprovedBool => IsAdminApproved==1;
        public string SeriesURL => this.GetSeriesURL();
        public string AniDBURL => this.GetAniDBURL();
        public string AniDBStartEpisodeTypeString => this.GetAniDBStartEpisodeTypeString();
        public string AniDBStartEpisodeNumberString => this.GetAniDBStartEpisodeNumberString();
        public string TvDBSeasonNumberString => this.GetTvDBSeasonNumberString();
        public string TvDBStartEpisodeNumberString => this.GetTvDBStartEpisodeNumberString();

    }
}
