using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class MovieDB_MovieVM : INotifyPropertyChanged
	{
		public int MovieDB_MovieID { get; set; }
		public int MovieId { get; set; }
		public string MovieName { get; set; }
		public string OriginalName { get; set; }
		public string Overview { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		private string siteURL = string.Empty;
		public string SiteURL
		{
			get { return siteURL; }
			set
			{
				siteURL = value;
				NotifyPropertyChanged("SiteURL");
			}
		}

		public MovieDB_MovieVM(JMMServerBinary.Contract_MovieDB_Movie contract)
		{
			this.MovieDB_MovieID = contract.MovieDB_MovieID;
			this.MovieId = contract.MovieId;
			this.MovieName = contract.MovieName;
			this.OriginalName = contract.OriginalName;
			this.Overview = contract.Overview;

			SiteURL = string.Format(Constants.URLS.MovieDB_Series, MovieId);
		}
	}
}
