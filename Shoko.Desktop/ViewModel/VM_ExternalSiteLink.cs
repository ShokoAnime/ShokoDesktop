using System.ComponentModel;
using Shoko.Commons.Notification;

namespace Shoko.Desktop.ViewModel
{
    public class VM_ExternalSiteLink : INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private string siteName;
        public string SiteName { get => siteName; set => this.SetField(()=> siteName, value); }

        private string siteLogo;
        public string SiteLogo { get => siteLogo; set => this.SetField(()=> siteLogo, value); }

        private string siteURL;
        public string SiteURL { get => siteURL; set => this.SetField(()=> siteURL, value); }

        private string siteURLDiscussion;
        public string SiteURLDiscussion { get => siteURLDiscussion; set => this.SetField(()=> siteURLDiscussion, value); }
    }
}