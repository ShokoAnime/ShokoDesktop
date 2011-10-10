using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class TvDB_SeriesVM : INotifyPropertyChanged
	{
		public int TvDB_SeriesID { get; set; }
		public int SeriesID { get; set; }
		public string Overview { get; set; }
		public string Status { get; set; }
		public string Banner { get; set; }
		public string Fanart { get; set; }
		public string Lastupdated { get; set; }
		public string Poster { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		private string seriesName = string.Empty;
		public string SeriesName
		{
			get { return seriesName; }
			set
			{
				seriesName = value;
				NotifyPropertyChanged("SeriesName");
			}
		}

		private string seriesURL = string.Empty;
		public string SeriesURL
		{
			get { return seriesURL; }
			set
			{
				seriesURL = value;
				NotifyPropertyChanged("SeriesURL");
			}
		}

		public TvDB_SeriesVM(JMMServerBinary.Contract_TvDB_Series contract)
		{
			this.TvDB_SeriesID = contract.TvDB_SeriesID;
			this.SeriesID = contract.SeriesID;
			this.Overview = contract.Overview;
			this.SeriesName = contract.SeriesName;
			this.Status = contract.Status;
			this.Banner = contract.Banner;
			this.Fanart = contract.Fanart;
			this.Lastupdated = contract.Lastupdated;
			this.Poster = contract.Poster;

			SeriesURL = string.Format(Constants.URLS.TvDB_Series, SeriesID);
		}
	}
}
