using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_Trakt_Show : CL_Trakt_Show, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new string Title
        {
            get { return base.Title ?? ""; }
            set
            {
                this.SetField(()=>base.Title,(r)=> base.Title = r, value);
            }
        }

        public new string URL
        {
            get { return base.URL; }
            set
            {
                this.SetField(()=>base.URL,(r)=> base.URL = r, value);
            }
        }

    }
}
