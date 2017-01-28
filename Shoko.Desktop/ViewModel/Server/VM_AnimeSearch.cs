using System.Collections.Generic;
using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeSearch : CL_AnimeSearch, INotifyPropertyChangedExt
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set { base.AnimeID = this.SetField(base.AnimeID, value); }
        }

        public new string MainTitle
        {
            get { return base.MainTitle; }
            set { base.MainTitle = this.SetField(base.MainTitle,value); }
        }

		public new HashSet<string> Titles
		{
			get { return base.Titles; }
			set { base.Titles = this.SetField(base.Titles, value); }
		}

        public new bool SeriesExists
        {
            get { return base.SeriesExists; }
            set { base.SeriesExists = this.SetField(base.SeriesExists, value); }
        }



        public string AnimeID_Friendly => $"AniDB: {AnimeID}";

        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Series, AnimeID);


    
	}
}
