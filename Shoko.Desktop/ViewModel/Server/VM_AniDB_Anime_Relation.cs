using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Anime_Relation : CL_AniDB_Anime_Relation, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }



        public new string RelationType
        {
            get { return base.RelationType; }
            set
            {
                base.RelationType = value;
                if (RelationType.Equals("Prequel", StringComparison.InvariantCultureIgnoreCase)) SortPriority = 1;
                if (RelationType.Equals("Sequel", StringComparison.InvariantCultureIgnoreCase)) SortPriority = 2;
            }
        }

        public new VM_AniDB_Anime AniDB_Anime
        {
            get { return (VM_AniDB_Anime)base.AniDB_Anime; }
            set
            {
                base.AniDB_Anime = value;
                EvaluateProperties();
            }
        }

        public new VM_AnimeSeries_User AnimeSeries
        {
            get { return (VM_AnimeSeries_User)base.AnimeSeries; }
            set
            {
                base.AnimeSeries = value;
                EvaluateProperties();
            }
        }

        private string displayName = "";
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = this.SetField(displayName, value);
            }
        }

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set
            {
                base.AnimeID = this.SetField(base.AnimeID, value);
            }
        }

        public new int RelatedAnimeID
        {
            get { return base.RelatedAnimeID; }
            set { base.RelatedAnimeID = this.SetField(base.RelatedAnimeID, value, () => AniDB_SiteURL); }
        }

        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Series, RelatedAnimeID);

        private bool localSeriesExists;
        public bool LocalSeriesExists
        {
            get { return localSeriesExists; }
            set
            {
                localSeriesExists = this.SetField(localSeriesExists, value);
            }
        }

        private bool animeInfoExists;
        public bool AnimeInfoExists
        {
            get { return animeInfoExists; }
            set
            {
                animeInfoExists = this.SetField(animeInfoExists, value);
            }
        }

        private bool showCreateSeriesButton;
        public bool ShowCreateSeriesButton
        {
            get { return showCreateSeriesButton; }
            set
            {
                showCreateSeriesButton = this.SetField(showCreateSeriesButton, value);
            }
        }

        private string posterPath = "";
        public string PosterPath
        {
            get { return posterPath; }
            set
            {
                posterPath = this.SetField(posterPath, value);
            }
        }

        private int sortPriority = int.MaxValue;
        public int SortPriority
        {
            get { return sortPriority; }
            set
            {
                sortPriority = this.SetField(sortPriority, value);
            }
        }


        public void PopulateAnime(VM_AniDB_Anime animeContract)
        {
            AniDB_Anime = animeContract;
        }

        public void PopulateSeries(VM_AnimeSeries_User seriesContract)
        {
            if (seriesContract!=null)
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

                DisplayName = Shoko.Commons.Properties.Resources.AniDB_DataMissing;
                AnimeInfoExists = false;
                PosterPath = $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
            }



            LocalSeriesExists = AnimeSeries != null;

            if (!localSeriesExists && AnimeInfoExists) ShowCreateSeriesButton = true;
            else ShowCreateSeriesButton = false;
        }


    }
}
