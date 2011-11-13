using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class MissingEpisodeVM
	{
		public int EpisodeID { get; set; }
		public int AnimeID { get; set; }
		public string AnimeTitle { get; set; }
		public int EpisodeNumber { get; set; }
		public int EpisodeType { get; set; }
		public string GroupFileSummary { get; set; }

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
				return string.Format("Episode {0} ({1})", EpisodeTypeAndNumber, EpisodeID);
			}
		}

		

		public EpisodeType EpisodeTypeEnum
		{
			get
			{
				return (EpisodeType)EpisodeType;
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

		private string episodeTypeAndNumber = "";
		public string EpisodeTypeAndNumber
		{
			get { return episodeTypeAndNumber; }
			set
			{
				episodeTypeAndNumber = value;
				NotifyPropertyChanged("EpisodeTypeAndNumber");
			}
		}

		public MissingEpisodeVM(JMMServerBinary.Contract_MissingEpisode contract)
		{
			this.EpisodeID = contract.EpisodeID;
			this.AnimeID = contract.AnimeID;
			this.AnimeTitle = contract.AnimeTitle;
			this.EpisodeNumber = contract.EpisodeNumber;
			this.EpisodeType = contract.EpisodeType;
			this.GroupFileSummary = contract.GroupFileSummary;

			AnimeSeries = null;
			if (contract.AnimeSeries != null) AnimeSeries = new AnimeSeriesVM(contract.AnimeSeries);

			HasSeriesData = AnimeSeries != null;

			string shortType = "";
			switch (EpisodeTypeEnum)
			{
				case JMMClient.EpisodeType.Credits: shortType = "C"; break;
				case JMMClient.EpisodeType.Episode: shortType = ""; break;
				case JMMClient.EpisodeType.Other: shortType = "O"; break;
				case JMMClient.EpisodeType.Parody: shortType = "P"; break;
				case JMMClient.EpisodeType.Special: shortType = "S"; break;
				case JMMClient.EpisodeType.Trailer: shortType = "T"; break;
			}
			EpisodeTypeAndNumber = string.Format("{0}{1}", shortType, EpisodeNumber);
		}
	}
}
