using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Shoko.Commons.Notification;
using Shoko.Commons.Properties;
using Shoko.Commons.Utils;
using Shoko.Desktop.Properties;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Anime_Similar : CL_AniDB_Anime_Similar, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }



        public new VM_AniDB_Anime AniDB_Anime
        {
            get { return (VM_AniDB_Anime) base.AniDB_Anime; }
            set
            {
                base.AniDB_Anime = value;
                EvaluateProperties();
            }
        }

        public new VM_AnimeSeries_User AnimeSeries
        {
            get { return (VM_AnimeSeries_User) base.AnimeSeries; }
            set
            {
                base.AnimeSeries = value;
                EvaluateProperties();
            }
        }

        public new int Approval
        {
            get { return base.Approval; }
            set { this.SetField(()=>base.Approval,(r)=> base.Approval = r, value, () => ApprovalPercentage, () => ApprovalRating); }
        }
        public new int Total
        {
            get { return base.Total; }
            set { this.SetField(()=>base.Total,(r)=> base.Total = r, value, () => ApprovalPercentage, () => ApprovalRating);  }
        }

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

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set
            {
                this.SetField(()=>base.AnimeID,(r)=> base.AnimeID = r, value);
            }
        }

        public string ApprovalRating
        {
            get
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
                return string.Format("{0} ({1})" + " " + Resources.Votes, Formatting.FormatPercentage(ApprovalPercentage), Total);
            }

        }



        public double ApprovalPercentage
        {
            get
            {
                double app = 0;
                if (Total > 0)
                    app = Approval / (double)Total * 100;
                return app;
            }
        }


        public new int SimilarAnimeID
        {
            get { return base.SimilarAnimeID; }
            set { this.SetField(()=>base.SimilarAnimeID,(r)=> base.SimilarAnimeID = r, value, () => AniDB_SiteURL); }
            
        }
        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Series, SimilarAnimeID);

        private bool localSeriesExists;
        public bool LocalSeriesExists
        {
            get { return localSeriesExists; }
            set
            {
                this.SetField(()=>localSeriesExists, value);
            }
        }

        private bool animeInfoExists;
        public bool AnimeInfoExists
        {
            get { return animeInfoExists; }
            set
            {
                this.SetField(()=>animeInfoExists, value);
            }
        }


        private bool showCreateSeriesButton;
        public bool ShowCreateSeriesButton
        {
            get { return showCreateSeriesButton; }
            set
            {
                this.SetField(()=>showCreateSeriesButton, value);
            }
        }

        private string posterPath = "";
        public string PosterPath
        {
            get { return posterPath; }
            set
            {
                this.SetField(()=>posterPath, value);
            }
        }

        public void PopulateAnime(VM_AniDB_Anime anime)
        {
            AniDB_Anime = anime;
        }

        public void PopulateSeries(VM_AnimeSeries_User seriesContract)
        {
            AnimeSeries = seriesContract;
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

                DisplayName = Resources.Recommendation_Missing;
                AnimeInfoExists = false;
                PosterPath = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
            }

            LocalSeriesExists = AnimeSeries != null;

            if (!localSeriesExists && AnimeInfoExists) ShowCreateSeriesButton = true;
            else ShowCreateSeriesButton = false;
        }


    }
}
