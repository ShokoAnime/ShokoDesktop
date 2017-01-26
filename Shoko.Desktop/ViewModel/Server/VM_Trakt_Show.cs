using System.ComponentModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_Trakt_Show : CL_Trakt_Show, INotifyPropertyChangedExt
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
                base.Title = this.SetField(base.Title, value);
            }
        }

        public new string URL
        {
            get { return base.URL; }
            set
            {
                base.URL = this.SetField(base.URL, value);
            }
        }

    }
}
