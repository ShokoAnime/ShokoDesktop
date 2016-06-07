using NLog;
using System;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
    public class AnimeSearchVM : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public int? AnimeSeriesID { get; set; }
        public string AnimeSeriesName { get; set; }
        public int? AnimeGroupID { get; set; }
        public string AnimeGroupName { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", AnimeID, MainTitle, Titles);
        }


        private int animeID = 0;
        public int AnimeID
        {
            get { return animeID; }
            set
            {
                animeID = value;
                NotifyPropertyChanged("AnimeID");
            }
        }

        private string mainTitle = "";
        public string MainTitle
        {
            get { return mainTitle; }
            set
            {
                mainTitle = value;
                NotifyPropertyChanged("MainTitle");
            }
        }

        private string titles = "";
        public string Titles
        {
            get { return titles; }
            set
            {
                titles = value;
                NotifyPropertyChanged("Titles");
            }
        }

        private bool seriesExists = false;
        public bool SeriesExists
        {
            get { return seriesExists; }
            set
            {
                seriesExists = value;
                NotifyPropertyChanged("SeriesExists");
            }
        }

        private bool seriesNotExists = true;
        public bool SeriesNotExists
        {
            get { return seriesNotExists; }
            set
            {
                seriesNotExists = value;
                NotifyPropertyChanged("SeriesNotExists");
            }
        }

        public string AnimeID_Friendly
        {
            get
            {
                return string.Format("AniDB: {0}", AnimeID);
            }
        }

        public string AniDB_SiteURL
        {
            get
            {
                return string.Format(Constants.URLS.AniDB_Series, AnimeID);

            }
        }

        public AnimeSearchVM()
        {
        }

        public AnimeSearchVM(JMMServerBinary.Contract_AnimeSearch contract)
        {
            Populate(contract);
        }

        public void Populate(JMMServerBinary.Contract_AnimeSearch contract)
        {
            this.AnimeGroupID = contract.AnimeGroupID;
            this.AnimeGroupName = contract.AnimeGroupName;
            this.AnimeID = contract.AnimeID;
            this.AnimeSeriesID = contract.AnimeSeriesID;
            this.AnimeSeriesName = contract.AnimeSeriesName;
            this.MainTitle = contract.MainTitle;
            this.SeriesExists = contract.SeriesExists;
            this.SeriesNotExists = !contract.SeriesExists;
            this.Titles = contract.Titles;
        }
    }
}
