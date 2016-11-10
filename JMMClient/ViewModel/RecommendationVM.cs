using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace JMMClient.ViewModel
{
    public class RecommendationVM : INotifyPropertyChanged
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

        public int RecommendedAnimeID { get; set; }
        public int BasedOnAnimeID { get; set; }
        public double Score { get; set; }
        public int BasedOnVoteValue { get; set; }
        public double RecommendedApproval { get; set; }

        public AniDB_AnimeVM Recommended_AniDB_Anime { get; set; }
        public AnimeSeriesVM Recommended_AnimeSeries { get; set; }

        public AniDB_AnimeVM BasedOn_AniDB_Anime { get; set; }
        public AnimeSeriesVM BasedOn_AnimeSeries { get; set; }

        private string recommended_DisplayName = "";
        public string Recommended_DisplayName
        {
            get { return recommended_DisplayName; }
            set
            {
                recommended_DisplayName = value;
                NotifyPropertyChanged("Recommended_DisplayName");
            }
        }

        private string basedOn_DisplayName = "";
        public string BasedOn_DisplayName
        {
            get { return basedOn_DisplayName; }
            set
            {
                basedOn_DisplayName = value;
                NotifyPropertyChanged("BasedOn_DisplayName");
            }
        }

        private string recommended_Description = "";
        public string Recommended_Description
        {
            get { return recommended_Description; }
            set
            {
                recommended_Description = value;
                NotifyPropertyChanged("Recommended_Description");
            }
        }


        private string recommended_AniDB_SiteURL = "";
        public string Recommended_AniDB_SiteURL
        {
            get { return recommended_AniDB_SiteURL; }
            set
            {
                recommended_AniDB_SiteURL = value;
                NotifyPropertyChanged("Recommended_AniDB_SiteURL");
            }
        }

        private string basedOn_AniDB_SiteURL = "";
        public string BasedOn_AniDB_SiteURL
        {
            get { return basedOn_AniDB_SiteURL; }
            set
            {
                basedOn_AniDB_SiteURL = value;
                NotifyPropertyChanged("BasedOn_AniDB_SiteURL");
            }
        }

        private bool recommended_LocalSeriesExists = false;
        public bool Recommended_LocalSeriesExists
        {
            get { return recommended_LocalSeriesExists; }
            set
            {
                recommended_LocalSeriesExists = value;
                NotifyPropertyChanged("Recommended_LocalSeriesExists");
            }
        }

        private bool recommended_AnimeInfoExists = false;
        public bool Recommended_AnimeInfoExists
        {
            get { return recommended_AnimeInfoExists; }
            set
            {
                recommended_AnimeInfoExists = value;
                NotifyPropertyChanged("Recommended_AnimeInfoExists");
            }
        }

        private bool recommended_AnimeInfoNotExists = false;
        public bool Recommended_AnimeInfoNotExists
        {
            get { return recommended_AnimeInfoNotExists; }
            set
            {
                recommended_AnimeInfoNotExists = value;
                NotifyPropertyChanged("Recommended_AnimeInfoNotExists");
            }
        }

        private bool recommended_ShowCreateSeriesButton = false;
        public bool Recommended_ShowCreateSeriesButton
        {
            get { return recommended_ShowCreateSeriesButton; }
            set
            {
                recommended_ShowCreateSeriesButton = value;
                NotifyPropertyChanged("Recommended_ShowCreateSeriesButton");
            }
        }

        private string recommended_PosterPath = "";
        public string Recommended_PosterPath
        {
            get { return recommended_PosterPath; }
            set
            {
                recommended_PosterPath = value;
                NotifyPropertyChanged("Recommended_PosterPath");
            }
        }

        private string basedOn_PosterPath = "";
        public string BasedOn_PosterPath
        {
            get { return basedOn_PosterPath; }
            set
            {
                basedOn_PosterPath = value;
                NotifyPropertyChanged("BasedOn_PosterPath");
            }
        }

        private string recommended_ApprovalRating = "";
        public string Recommended_ApprovalRating
        {
            get { return recommended_ApprovalRating; }
            set
            {
                recommended_ApprovalRating = value;
                NotifyPropertyChanged("Recommended_ApprovalRating");
            }
        }

        private string basedOnVoteValueFormatted = "";
        public string BasedOnVoteValueFormatted
        {
            get { return basedOnVoteValueFormatted; }
            set
            {
                basedOnVoteValueFormatted = value;
                NotifyPropertyChanged("BasedOnVoteValueFormatted");
            }
        }

        public void Populate(JMMServerBinary.Contract_Recommendation details)
        {
            this.RecommendedAnimeID = details.RecommendedAnimeID;
            this.BasedOnAnimeID = details.BasedOnAnimeID;
            this.Score = details.Score;
            this.BasedOnVoteValue = details.BasedOnVoteValue;
            this.RecommendedApproval = details.RecommendedApproval;

            Recommended_ApprovalRating = string.Format("{0}", Utils.FormatPercentage(RecommendedApproval));
            BasedOnVoteValueFormatted = String.Format("{0:0.0}", (double)BasedOnVoteValue / (double)100);

            Recommended_AniDB_SiteURL = string.Format(Constants.URLS.AniDB_Series, RecommendedAnimeID);
            BasedOn_AniDB_SiteURL = string.Format(Constants.URLS.AniDB_Series, BasedOnAnimeID);

            PopulateRecommendedAnime(details.Recommended_AniDB_Anime);
            PopulateRecommendedSeries(details.Recommended_AnimeSeries);

            PopulateBasedOnAnime(details.BasedOn_AniDB_Anime);
            PopulateBasedOnSeries(details.BasedOn_AnimeSeries);
        }

        public void PopulateRecommendedAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
        {
            if (animeContract != null)
                Recommended_AniDB_Anime = new AniDB_AnimeVM(animeContract);

            EvaluateProperties();
        }

        public void PopulateBasedOnAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
        {
            if (animeContract != null)
                BasedOn_AniDB_Anime = new AniDB_AnimeVM(animeContract);

            EvaluateProperties();
        }

        public void PopulateRecommendedSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
        {
            if (seriesContract != null)
                Recommended_AnimeSeries = new AnimeSeriesVM(seriesContract);

            EvaluateProperties();
        }

        public void PopulateBasedOnSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
        {
            if (seriesContract != null)
                BasedOn_AnimeSeries = new AnimeSeriesVM(seriesContract);

            EvaluateProperties();
        }

        public void EvaluateProperties()
        {
            if (Recommended_AniDB_Anime != null)
            {
                Recommended_DisplayName = Recommended_AniDB_Anime.FormattedTitle;
                Recommended_AnimeInfoExists = true;
                Recommended_PosterPath = Recommended_AniDB_Anime.PosterPath;
                Recommended_Description = Recommended_AniDB_Anime.Description;
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                Recommended_DisplayName = Properties.Resources.Recommendation_Missing;
                Recommended_AnimeInfoExists = false;
                Recommended_PosterPath = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                Recommended_Description = Properties.Resources.Recommendation_Overview;
            }

            Recommended_AnimeInfoNotExists = !Recommended_AnimeInfoExists;

            if (BasedOn_AniDB_Anime != null)
            {
                BasedOn_DisplayName = BasedOn_AniDB_Anime.FormattedTitle;
                BasedOn_PosterPath = BasedOn_AniDB_Anime.PosterPath;
            }

            if (Recommended_AnimeSeries != null)
                Recommended_LocalSeriesExists = true;
            else
                Recommended_LocalSeriesExists = false;

            if (!Recommended_LocalSeriesExists && Recommended_AnimeInfoExists) Recommended_ShowCreateSeriesButton = true;
            else Recommended_ShowCreateSeriesButton = false;
        }
    }
}
