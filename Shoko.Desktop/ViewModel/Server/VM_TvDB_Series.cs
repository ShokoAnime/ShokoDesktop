using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
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
                this.SetField(()=>base.SeriesName,(r)=> base.SeriesName = r, value);
            }
        }

        [JsonIgnore, XmlIgnore]
        public string SeriesURL => string.Format(Models.Constants.URLS.TvDB_Series, SeriesID);

        public new int SeriesID
        {
            get { return base.SeriesID; }
            set
            {
                this.SetField(()=>base.SeriesID,(r)=> base.SeriesID = r, value,()=>SeriesID, ()=>SeriesURL);
            }
        }

    }
}
        