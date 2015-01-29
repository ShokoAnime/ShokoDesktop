using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class CrossRef_AniDB_TvDBVMV2 : INotifyPropertyChanged
	{
		public int CrossRef_AniDB_TvDBV2ID { get; set; }
		public int AnimeID { get; set; }
		public int AniDBStartEpisodeType { get; set; }
		public int AniDBStartEpisodeNumber { get; set; }
		public int TvDBID { get; set; }
		public int TvDBSeasonNumber { get; set; }
		public int TvDBStartEpisodeNumber { get; set; }
		public string TvDBTitle { get; set; }
		public int CrossRefSource { get; set; }

        public string Username { get; set; }

        

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

        private string isAdminApprovedImage = @"/Images/placeholder.png";
        public string IsAdminApprovedImage
        {
            get { return isAdminApprovedImage; }
            set
            {
                isAdminApprovedImage = value;
                NotifyPropertyChanged("IsAdminApprovedImage");
            }
        }

        private void SetAdminApprovedImage()
        {
            if (IsAdminApproved == 1) 
                IsAdminApprovedImage = @"/Images/16_tick.png";
            else
                IsAdminApprovedImage = @"/Images/placeholder.png";
        }

        private int isAdminApproved = 0;
        public int IsAdminApproved
        {
            get { return isAdminApproved; }
            set
            {
                isAdminApproved = value;
                NotifyPropertyChanged("IsAdminApproved");

                IsAdminApprovedBool = isAdminApproved == 1;
                IsNotAdminApprovedBool = !IsAdminApprovedBool;

                SetAdminApprovedImage();
            }
        }

        private bool isAdminApprovedBool = false;
        public bool IsAdminApprovedBool
        {
            get { return isAdminApprovedBool; }
            set
            {
                isAdminApprovedBool = value;
                NotifyPropertyChanged("IsAdminApprovedBool");
            }
        }

        private bool isNotAdminApprovedBool = true;
        public bool IsNotAdminApprovedBool
        {
            get { return isNotAdminApprovedBool; }
            set
            {
                isNotAdminApprovedBool = value;
                NotifyPropertyChanged("IsNotAdminApprovedBool");
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

        private string anidbURL = string.Empty;
        public string AniDBURL
        {
            get { return anidbURL; }
            set
            {
                anidbURL = value;
                NotifyPropertyChanged("AniDBURL");
            }
        }

		public string AniDBStartEpisodeTypeString
		{
			get
			{
				return EnumTranslator.EpisodeTypeTranslated((EpisodeType)AniDBStartEpisodeType);
			}
		}

		public string AniDBStartEpisodeNumberString
		{
			get
			{
				return string.Format("# {0}", AniDBStartEpisodeNumber);
			}
		}

		public string TvDBSeasonNumberString
		{
			get
			{
				return string.Format("S{0}", TvDBSeasonNumber);
			}
		}

		public string TvDBStartEpisodeNumberString
		{
			get
			{
				return string.Format("EP# {0}", TvDBStartEpisodeNumber);
			}
		}

		public CrossRef_AniDB_TvDBVMV2()
		{
		}

		public CrossRef_AniDB_TvDBVMV2(JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2 contract)
		{
			this.AnimeID = contract.AnimeID;
			this.AniDBStartEpisodeType = contract.AniDBStartEpisodeType;
			this.AniDBStartEpisodeNumber = contract.AniDBStartEpisodeNumber;
			this.TvDBID = contract.TvDBID;
			this.CrossRef_AniDB_TvDBV2ID = contract.CrossRef_AniDB_TvDBV2ID;
			this.TvDBSeasonNumber = contract.TvDBSeasonNumber;
			this.TvDBStartEpisodeNumber = contract.TvDBStartEpisodeNumber;
			this.CrossRefSource = contract.CrossRefSource;
			this.TvDBTitle = contract.TvDBTitle;

			SeriesURL = string.Format(Constants.URLS.TvDB_Series, TvDBID);
            AniDBURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
		}

		public CrossRef_AniDB_TvDBVMV2(JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB contract)
		{
			this.AnimeID = contract.AnimeID;
			this.AniDBStartEpisodeType = contract.AniDBStartEpisodeType;
			this.AniDBStartEpisodeNumber = contract.AniDBStartEpisodeNumber;
			this.TvDBID = contract.TvDBID;
			this.CrossRef_AniDB_TvDBV2ID = contract.CrossRef_AniDB_TvDBId.Value;
			this.TvDBSeasonNumber = contract.TvDBSeasonNumber;
			this.TvDBStartEpisodeNumber = contract.TvDBStartEpisodeNumber;
			this.CrossRefSource = contract.CrossRefSource;
			this.TvDBTitle = contract.TvDBTitle;
            this.Username = contract.Username;
            this.IsAdminApproved = contract.IsAdminApproved;

			SeriesURL = string.Format(Constants.URLS.TvDB_Series, TvDBID);
            AniDBURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
		}

		public override string ToString()
		{
			return string.Format("{0} = {1}   AniDB # {2}:{3}   TvDB {4}:{5}", AnimeID, TvDBID, AniDBStartEpisodeType, AniDBStartEpisodeNumber, TvDBSeasonNumber, TvDBStartEpisodeNumber);
		}
	}
}
