using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Anime_Relation : CL_AniDB_Anime_Relation, INotifyPropertyChanged, INotifyPropertyChangedExt
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
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                this.SetField(()=>displayName,value);
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

        public new int RelatedAnimeID
        {
            get { return base.RelatedAnimeID; }
            set { this.SetField(()=>base.RelatedAnimeID,(r)=> base.RelatedAnimeID = r, value, () => AniDB_SiteURL); }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Series, RelatedAnimeID);

        private bool localSeriesExists;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool LocalSeriesExists
        {
            get { return localSeriesExists; }
            set
            {
                this.SetField(()=>localSeriesExists,value);
            }
        }

        private bool animeInfoExists;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool AnimeInfoExists
        {
            get { return animeInfoExists; }
            set
            {
                this.SetField(()=>animeInfoExists,value);
            }
        }

        private bool showCreateSeriesButton;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool ShowCreateSeriesButton
        {
            get { return showCreateSeriesButton; }
            set
            {
                this.SetField(()=>showCreateSeriesButton,value);
            }
        }

        private string posterPath = "";
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string PosterPath
        {
            get { return posterPath; }
            set
            {
                this.SetField(()=>posterPath,value);
            }
        }

        private int sortPriority = int.MaxValue;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int SortPriority
        {
            get { return sortPriority; }
            set
            {
                this.SetField(()=>sortPriority,value);
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
