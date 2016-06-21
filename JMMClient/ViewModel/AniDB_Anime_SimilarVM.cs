using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace JMMClient.ViewModel
{
    public class AniDB_Anime_SimilarVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public int AniDB_Anime_SimilarID { get; set; }
        public int SimilarAnimeID { get; set; }
        public int Approval { get; set; }
        public int Total { get; set; }

        public AniDB_AnimeVM AniDB_Anime { get; set; }
        public AnimeSeriesVM AnimeSeries { get; set; }

        private string displayName = "";
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = value;
                NotifyPropertyChanged("DisplayName");
            }
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

        private string approvalRating = "";
        public string ApprovalRating
        {
            get { return approvalRating; }
            set
            {
                approvalRating = value;
                NotifyPropertyChanged("ApprovalRating");
            }
        }

        public double approvalPercentage = 0;
        public double ApprovalPercentage
        {
            get { return approvalPercentage; }
            set
            {
                approvalPercentage = value;
                NotifyPropertyChanged("ApprovalPercentage");
            }
        }

        private string aniDB_SiteURL = "";
        public string AniDB_SiteURL
        {
            get { return aniDB_SiteURL; }
            set
            {
                aniDB_SiteURL = value;
                NotifyPropertyChanged("AniDB_SiteURL");
            }
        }

        private bool localSeriesExists = false;
        public bool LocalSeriesExists
        {
            get { return localSeriesExists; }
            set
            {
                localSeriesExists = value;
                NotifyPropertyChanged("LocalSeriesExists");
            }
        }

        private bool animeInfoExists = false;
        public bool AnimeInfoExists
        {
            get { return animeInfoExists; }
            set
            {
                animeInfoExists = value;
                NotifyPropertyChanged("AnimeInfoExists");
            }
        }

        private bool animeInfoNotExists = false;
        public bool AnimeInfoNotExists
        {
            get { return animeInfoNotExists; }
            set
            {
                animeInfoNotExists = value;
                NotifyPropertyChanged("AnimeInfoNotExists");
            }
        }

        private bool showCreateSeriesButton = false;
        public bool ShowCreateSeriesButton
        {
            get { return showCreateSeriesButton; }
            set
            {
                showCreateSeriesButton = value;
                NotifyPropertyChanged("ShowCreateSeriesButton");
            }
        }

        private string posterPath = "";
        public string PosterPath
        {
            get { return posterPath; }
            set
            {
                posterPath = value;
                NotifyPropertyChanged("PosterPath");
            }
        }

        public AniDB_Anime_SimilarVM()
        {
        }

        public void PopulateAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
        {
            if (animeContract != null)
                AniDB_Anime = new AniDB_AnimeVM(animeContract);

            EvaluateProperties();
        }

        public void PopulateSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
        {
            if (seriesContract != null)
                AnimeSeries = new AnimeSeriesVM(seriesContract);

            EvaluateProperties();
        }

        public void EvaluateProperties()
        {
            if (AniDB_Anime != null)
            {
                DisplayName = AniDB_Anime.FormattedTitle;
                AnimeInfoExists = true;
                PosterPath = AniDB_Anime.PosterPath;
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                DisplayName = Properties.Resources.Recommendation_Missing;
                AnimeInfoExists = false;
                PosterPath = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);
            }

            AnimeInfoNotExists = !AnimeInfoExists;

            if (AnimeSeries != null)
                LocalSeriesExists = true;
            else
                LocalSeriesExists = false;

            if (!localSeriesExists && AnimeInfoExists) ShowCreateSeriesButton = true;
            else ShowCreateSeriesButton = false;
        }

        public void Populate(JMMServerBinary.Contract_AniDB_Anime_Similar details)
        {
            this.AniDB_Anime_SimilarID = details.AniDB_Anime_SimilarID;
            this.AnimeID = details.AnimeID;
            this.SimilarAnimeID = details.SimilarAnimeID;
            this.Approval = details.Approval;
            this.Total = details.Total;

            AniDB_SiteURL = string.Format(Constants.URLS.AniDB_Series, SimilarAnimeID);
            ApprovalPercentage = 0;
            if (this.Total > 0)
                ApprovalPercentage = (double)Approval / (double)Total * (double)100;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            ApprovalRating = string.Format("{0} ({1})" + " " + Properties.Resources.Votes, Utils.FormatPercentage(ApprovalPercentage), this.Total);

            PopulateAnime(details.AniDB_Anime);
            PopulateSeries(details.AnimeSeries);
        }
    }
}
