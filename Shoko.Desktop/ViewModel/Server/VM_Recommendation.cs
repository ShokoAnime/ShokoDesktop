using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_Recommendation : CL_Recommendation, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        public new VM_AniDB_Anime Recommended_AniDB_Anime
        {
            get
            {
                return (VM_AniDB_Anime)base.Recommended_AniDB_Anime;
            }
            set
            {
                base.Recommended_AniDB_Anime = value;
                if (value == null)
                {
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
                    Recommended_DisplayName = Shoko.Commons.Properties.Resources.Recommendation_Missing;
                    Recommended_AnimeInfoExists = false;
                    Recommended_PosterPath = $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
                    Recommended_Description = Shoko.Commons.Properties.Resources.Recommendation_Overview;
                }
                else
                {
                    Recommended_DisplayName = value.FormattedTitle;
                    Recommended_AnimeInfoExists = true;
                    Recommended_PosterPath = value.PosterPath ?? $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
                    Recommended_Description = value.Description ?? Shoko.Commons.Properties.Resources.Recommendation_Overview;
                }
                Recommended_ShowCreateSeriesButton = (!Recommended_LocalSeriesExists && Recommended_AnimeInfoExists);
            }
        }

        public new VM_AnimeSeries_User Recommended_AnimeSeries
        {
            get { return (VM_AnimeSeries_User) base.Recommended_AnimeSeries; }
            set
            {
                base.Recommended_AnimeSeries = value;
                Recommended_LocalSeriesExists = value != null;
                Recommended_ShowCreateSeriesButton = (!Recommended_LocalSeriesExists && Recommended_AnimeInfoExists);
            }
        }

        public new VM_AniDB_Anime BasedOn_AniDB_Anime
        {
            get
            {
                return (VM_AniDB_Anime)base.BasedOn_AniDB_Anime;
            }
            set
            {
                base.BasedOn_AniDB_Anime = value;
                if (value != null)
                {
                    BasedOn_DisplayName = BasedOn_AniDB_Anime.FormattedTitle;
                    BasedOn_PosterPath = BasedOn_AniDB_Anime.PosterPath;
                }
            }
        }

        public new VM_AnimeSeries_User BasedOn_AnimeSeries
        {
            get { return (VM_AnimeSeries_User)base.BasedOn_AnimeSeries; }
            set { base.BasedOn_AnimeSeries = value; }
        }

        private string recommended_DisplayName = "";
        [JsonIgnore, XmlIgnore]
        public string Recommended_DisplayName
        {
            get { return recommended_DisplayName; }
            set
            {
                this.SetField(()=>recommended_DisplayName,value);
            }
        }

        private string basedOn_DisplayName = "";
        [JsonIgnore, XmlIgnore]
        public string BasedOn_DisplayName
        {
            get { return basedOn_DisplayName; }
            set
            {
                this.SetField(()=>basedOn_DisplayName,(r)=> basedOn_DisplayName = r, value);
            }
        }

        private string basedOn_Description = "";
        [JsonIgnore, XmlIgnore]
        public string BasedOn_Description
        {
            get
            {
                if (basedOn_Description == "")
                {
                    basedOn_Description = BasedOn_AniDB_Anime?.Description ??
                                          Shoko.Commons.Properties.Resources.Recommendation_Overview;
                }
                return basedOn_Description;
            }
            set
            {
                this.SetField(()=>basedOn_Description,value);
            }
        }

        private string recommended_Description = "";
        [JsonIgnore, XmlIgnore]
        public string Recommended_Description
        {
            get
            {
                if (recommended_Description == "")
                {
                    recommended_Description = Recommended_AniDB_Anime?.Description ??
                                          Shoko.Commons.Properties.Resources.Recommendation_Overview;
                }
                return recommended_Description;
            }
            set
            {
                this.SetField(()=>recommended_Description,value);
            }
        }


        private string recommended_AniDB_SiteURL = "";
        [JsonIgnore, XmlIgnore]
        public string Recommended_AniDB_SiteURL
        {
            get
            {
                if (recommended_AniDB_SiteURL == "")
                {
                    recommended_AniDB_SiteURL = Recommended_AniDB_Anime?.AniDB_SiteURL ?? "";
                }
                return recommended_AniDB_SiteURL;
            }
            set
            {
                this.SetField(()=>recommended_AniDB_SiteURL,value);
            }
        }

        private string basedOn_AniDB_SiteURL = "";
        [JsonIgnore, XmlIgnore]
        public string BasedOn_AniDB_SiteURL
        {
            get
            {
                if (basedOn_AniDB_SiteURL == "")
                {
                    basedOn_AniDB_SiteURL = BasedOn_AniDB_Anime?.AniDB_SiteURL ?? "";
                }
                return basedOn_AniDB_SiteURL;
            }
            set
            {
                this.SetField(()=>BasedOn_AniDB_SiteURL,(r)=> BasedOn_AniDB_SiteURL = r, value);
            }
        }

        private bool recommended_LocalSeriesExists;
        [JsonIgnore, XmlIgnore]
        public bool Recommended_LocalSeriesExists
        {
            get { return recommended_LocalSeriesExists; }
            set
            {
                this.SetField(()=>recommended_LocalSeriesExists,value);
            }
        }

        private bool recommended_AnimeInfoExists;
        [JsonIgnore, XmlIgnore]
        public bool Recommended_AnimeInfoExists
        {
            get { return recommended_AnimeInfoExists; }
            set
            {
                this.SetField(()=>recommended_AnimeInfoExists,value);
            }
        }



        private bool recommended_ShowCreateSeriesButton;
        [JsonIgnore, XmlIgnore]
        public bool Recommended_ShowCreateSeriesButton
        {
            get { return recommended_ShowCreateSeriesButton; }
            set
            {
                this.SetField(()=>recommended_ShowCreateSeriesButton,value);
            }
        }

        private string recommended_PosterPath = "";
        [JsonIgnore, XmlIgnore]
        public string Recommended_PosterPath
        {
            get { return recommended_PosterPath; }
            set
            {
                this.SetField(()=>recommended_PosterPath,value);
            }
        }

        private string basedOn_PosterPath = "";
        [JsonIgnore, XmlIgnore]
        public string BasedOn_PosterPath
        {
            get { return basedOn_PosterPath; }
            set
            {
                this.SetField(()=>basedOn_PosterPath,(r)=> basedOn_PosterPath = r, value);
            }
        }

        [JsonIgnore, XmlIgnore]
        public string Recommended_ApprovalRating
        {
            get
            {
                return $"{Math.Round(RecommendedApproval)}%";
            }
        }

        [JsonIgnore, XmlIgnore]
        public string BasedOnVoteValueFormatted
        {
            get { return (BasedOnVoteValue / 100D).ToString(CultureInfo.CurrentCulture); }
        }
    }
}
