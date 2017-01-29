using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_TvDB_Series : TvDB_Series, INotifyPropertyChanged, INotifyPropertyChangedExt
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new string SeriesName
        {
            get { return base.SeriesName; }
            set
            {
                base.SeriesName = this.SetField(base.SeriesName, value);
            }
        }

        public string SeriesURL => string.Format(Models.Constants.URLS.TvDB_Series, SeriesID);

        public new int SeriesID
        {
            get { return base.SeriesID; }
            set
            {
                base.SeriesID = this.SetField(base.SeriesID, value,()=>SeriesID, ()=>SeriesURL);
            }
        }

    }
}
        