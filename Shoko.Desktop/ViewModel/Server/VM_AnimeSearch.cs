using System.Collections.Generic;
using System.ComponentModel;

using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeSearch : CL_AnimeSearch, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set { this.SetField(()=>base.AnimeID,(r)=> base.AnimeID = r, value); }
        }

        public new string MainTitle
        {
            get { return base.MainTitle; }
            set { this.SetField(()=>base.MainTitle,(r)=> base.MainTitle = r, value); }
        }

		public new HashSet<string> Titles
		{
			get { return base.Titles; }
			set { this.SetField(()=>base.Titles,(r)=> base.Titles = r, value); }
		}

        public new bool SeriesExists
        {
            get { return base.SeriesExists; }
            set { this.SetField(()=>base.SeriesExists,(r)=> base.SeriesExists = r, value); }
        }


        [JsonIgnore, XmlIgnore]
        public string AnimeID_Friendly => $"AniDB: {AnimeID}";

        [JsonIgnore, XmlIgnore]
        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Series, AnimeID);


    
	}
}
