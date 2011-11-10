using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class MissingFileVM : INotifyPropertyChanged
	{
		public int EpisodeID { get; set; }
		public int FileID { get; set; }
		public int AnimeID { get; set; }
		public string AnimeTitle { get; set; }
		public int EpisodeNumber { get; set; }

		public AnimeSeriesVM AnimeSeries { get; set; }

		public string AniDB_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_Series, AnimeID);

			}
		}

		public string Episode_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_Episode, EpisodeID);

			}
		}

		public string File_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_File, FileID);

			}
		}

		public string AnimeTitleAndID
		{
			get
			{
				return string.Format("{0} ({1})", AnimeTitle, AnimeID);
			}
		}

		public string EpisodeNumberAndID
		{
			get
			{
				return string.Format("Episode {0} ({1})", EpisodeNumber, EpisodeID);
			}
		}

		public string FileDescAndID
		{
			get
			{
				return string.Format("File {0}", FileID);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		private Boolean hasSeriesData = true;
		public Boolean HasSeriesData
		{
			get { return hasSeriesData; }
			set
			{
				hasSeriesData = value;
				NotifyPropertyChanged("HasSeriesData");
			}
		}

		public MissingFileVM(JMMServerBinary.Contract_MissingFile contract)
		{
			this.EpisodeID = contract.EpisodeID;
			this.FileID = contract.FileID;
			this.AnimeID = contract.AnimeID;
			this.AnimeTitle = contract.AnimeTitle;
			this.EpisodeNumber = contract.EpisodeNumber;

			AnimeSeries = null;
			if (contract.AnimeSeries != null) AnimeSeries = new AnimeSeriesVM(contract.AnimeSeries);

			HasSeriesData = AnimeSeries != null;
		}
	}
}
