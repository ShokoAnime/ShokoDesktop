using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_AnimeCrossRefs : CL_AniDB_AnimeCrossRefs, INotifyPropertyChanged, INotifyPropertyChangedExt
    {

        private VM_AniDB_Anime anime;
        public new int AnimeID
        {
            get { return base.AnimeID; }
            set
            {
                base.AnimeID = value;
                if (VM_MainListHelper.Instance.AllAnimeDictionary.ContainsKey(value))
                    anime = VM_MainListHelper.Instance.AllAnimeDictionary[value];
                if (anime!=null)
                    AllPosters.Insert(0, new VM_PosterContainer(ImageEntityType.AniDB_Cover, anime));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }



        #region TvDB

        public bool TvDBCrossRefExists => !(Obs_CrossRef_AniDB_TvDB.Count == 0);

        public bool TvDBCrossRefMissing => (Obs_CrossRef_AniDB_TvDB.Count == 0);

        public new List<VM_TvDB_Series> TvDBSeries
        {
            get { return base.TvDBSeries.CastList<VM_TvDB_Series>(); }
            set
            {
                this.SetField(base.TvDBSeries, value.CastList<TvDB_Series>(), () => TvDBSeries, () => TvDBCrossRefExists, () => TvDBCrossRefMissing);
            }
        }



        private ObservableCollectionEx<VM_CrossRef_AniDB_TvDBV2> crossRef_AniDB_TvDB;

        public ObservableCollectionEx<VM_CrossRef_AniDB_TvDBV2> Obs_CrossRef_AniDB_TvDB
        {
            get
            {
                if (crossRef_AniDB_TvDB == null)
                {
                    crossRef_AniDB_TvDB = new ObservableCollectionEx<VM_CrossRef_AniDB_TvDBV2>();
                    if (base.CrossRef_AniDB_TvDB.Count > 0)
                        crossRef_AniDB_TvDB.ReplaceRange(base.CrossRef_AniDB_TvDB.Cast<VM_CrossRef_AniDB_TvDBV2>());
                }
                return crossRef_AniDB_TvDB;
            }
        }
        
        // ReSharper disable once UnusedMember.Local
        private new List<CrossRef_AniDB_TvDBV2> CrossRef_AniDB_TvDB
        {
            get { return Obs_CrossRef_AniDB_TvDB.CastList<CrossRef_AniDB_TvDBV2>(); }
            set
            {
                crossRef_AniDB_TvDB.ReplaceRange(value.CastList<VM_CrossRef_AniDB_TvDBV2>());
                this.OnPropertyChanged(() => Obs_CrossRef_AniDB_TvDB);
                this.OnPropertyChanged(() => TvDBCrossRefExists);
                this.OnPropertyChanged(() => TvDBCrossRefMissing);

            }

        }



        public new List<VM_TvDB_Episode> TvDBEpisodes
        {
            get { return base.TvDBEpisodes.CastList<VM_TvDB_Episode>(); }
            set { base.TvDBEpisodes= this.SetField(base.TvDBEpisodes, value.CastList<TvDB_Episode>()); }
        }
        public new List<VM_TvDB_ImageFanart> TvDBImageFanarts
        {
            get { return base.TvDBImageFanarts.CastList<VM_TvDB_ImageFanart>(); }
            set
            {
                foreach (VM_TvDB_ImageFanart contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int)ImageEntityType.TvDB_FanArt && anime.DefaultImageFanart.ImageParentID == contract.TvDB_ImageFanartID;
                    contract.IsImageDefault = isDefault;
                    AllFanarts.Add(new VM_FanartContainer(ImageEntityType.TvDB_FanArt, contract));
                }
                base.TvDBImageFanarts = this.SetField(base.TvDBImageFanarts, value.CastList<TvDB_ImageFanart>());
            }
        }
        public new List<VM_TvDB_ImagePoster> TvDBImagePosters
        {
            get { return base.TvDBImagePosters.CastList<VM_TvDB_ImagePoster>(); }
            set
            {
                foreach (VM_TvDB_ImagePoster contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int)ImageEntityType.TvDB_Cover && anime.DefaultImageFanart.ImageParentID == contract.TvDB_ImagePosterID;
                    contract.IsImageDefault = isDefault;
                    AllPosters.Add(new VM_PosterContainer(ImageEntityType.TvDB_Cover, contract));
                }
                base.TvDBImagePosters = this.SetField(base.TvDBImagePosters, value.CastList<TvDB_ImagePoster>());
            }
        }
        public new List<VM_TvDB_ImageWideBanner> TvDBImageWideBanners
        {
            get { return base.TvDBImageWideBanners.CastList<VM_TvDB_ImageWideBanner>(); }
            set
            {
                foreach (VM_TvDB_ImageWideBanner contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int)ImageEntityType.TvDB_Banner && anime.DefaultImageFanart.ImageParentID == contract.TvDB_ImageWideBannerID;
                    contract.IsImageDefault = isDefault;
                }
                base.TvDBImageWideBanners = this.SetField(base.TvDBImageWideBanners, value.CastList<TvDB_ImageWideBanner>());
            }
        }


    #endregion

        #region MovieDB

        public bool MovieDBCrossRefExists => !(CrossRef_AniDB_MovieDB == null || MovieDBMovie == null);

        public bool MovieDBCrossRefMissing => (CrossRef_AniDB_MovieDB == null || MovieDBMovie == null);

        public new VM_MovieDB_Movie MovieDBMovie
        {
            get { return (VM_MovieDB_Movie)base.MovieDBMovie; }
            set
            {
                base.MovieDBMovie=this.SetField(base.MovieDBMovie, value, ()=>MovieDBMovie, () => MovieDBCrossRefExists, () => MovieDBCrossRefMissing);
            }
        }

        public new CrossRef_AniDB_Other CrossRef_AniDB_MovieDB
        {
            get { return base.CrossRef_AniDB_MovieDB; }
            set
            {
                base.CrossRef_AniDB_MovieDB = this.SetField(base.CrossRef_AniDB_MovieDB, value, () => CrossRef_AniDB_MovieDB, () => MovieDBCrossRefExists, ()=>MovieDBCrossRefMissing);
            }
        }

        public new List<VM_MovieDB_Fanart> MovieDBFanarts
        {
            get { return base.MovieDBFanarts.CastList<VM_MovieDB_Fanart>(); }
            set
            {
                foreach (VM_MovieDB_Fanart contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int)ImageEntityType.MovieDB_FanArt && anime.DefaultImageFanart.ImageParentID == contract.MovieDB_FanartID;
                    contract.IsImageDefault = isDefault;
                    AllFanarts.Add(new VM_FanartContainer(ImageEntityType.MovieDB_FanArt, contract));
                }
                base.MovieDBFanarts = this.SetField(base.MovieDBFanarts, value.CastList<MovieDB_Fanart>());
            }
        }
        public new List<VM_MovieDB_Poster> MovieDBPosters
        {
            get { return base.MovieDBPosters.CastList<VM_MovieDB_Poster>(); }
            set
            {
                foreach (VM_MovieDB_Poster contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int)ImageEntityType.MovieDB_Poster && anime.DefaultImageFanart.ImageParentID == contract.MovieDB_PosterID;
                    contract.IsImageDefault = isDefault;
                    AllPosters.Add(new VM_PosterContainer(ImageEntityType.MovieDB_Poster, contract));
                }
                base.MovieDBPosters = this.SetField(base.MovieDBPosters, value.CastList<MovieDB_Poster>());
            }
        }



    #endregion

        private List<VM_PosterContainer> allPosters;
        public List<VM_PosterContainer> AllPosters
        {
            get { return allPosters; }
            set
            {
                allPosters = this.SetField(allPosters, value);
            }
        }

        private List<VM_FanartContainer> allFanarts;
        public List<VM_FanartContainer> AllFanarts
        {
            get { return allFanarts; }
            set
            {
                allFanarts = this.SetField(allFanarts, value);
            }
        }

        #region Trakt





        public new List<VM_Trakt_Show> TraktShows
        {
            get { return base.TraktShows.CastList<VM_Trakt_Show>(); }
            set
            {
                this.SetField(base.TraktShows,value.CastList<CL_Trakt_Show>(), ()=>TraktShows, () => TraktCrossRefExists, () => TraktCrossRefMissing);
            }
        }

        public new List<VM_Trakt_ImageFanart> TraktImageFanarts
        {
            get { return base.TraktImageFanarts.CastList<VM_Trakt_ImageFanart>(); }
            set
            {
                foreach (VM_Trakt_ImageFanart contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int) ImageEntityType.Trakt_Fanart && anime.DefaultImageFanart.ImageParentID == contract.Trakt_ImageFanartID;
                    contract.IsImageDefault = isDefault;
                    AllFanarts.Add(new VM_FanartContainer(ImageEntityType.Trakt_Fanart, contract));
                }
                base.TraktImageFanarts = this.SetField(base.TraktImageFanarts, value.CastList<Trakt_ImageFanart>());
            }
        }

        public new List<VM_Trakt_ImagePoster> TraktImagePosters
        {
            get { return base.TraktImagePosters.CastList<VM_Trakt_ImagePoster>(); }
            set
            {
                foreach (VM_Trakt_ImagePoster contract in value)
                {
                    bool isDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int)ImageEntityType.Trakt_Poster && anime.DefaultImageFanart.ImageParentID == contract.Trakt_ImagePosterID;
                    contract.IsImageDefault = isDefault;
                    AllPosters.Add(new VM_PosterContainer(ImageEntityType.Trakt_Poster, contract));
                }
                base.TraktImagePosters = this.SetField(base.TraktImagePosters, value.CastList<Trakt_ImagePoster>());
            }
        }

        private new List<CrossRef_AniDB_TraktV2> CrossRef_AniDB_Trakt
        {
            get { return Obs_CrossRef_AniDB_Trakt.CastList<CrossRef_AniDB_TraktV2>(); }
            set
            {
                crossRef_AniDB_Trakt.ReplaceRange(value.CastList<VM_CrossRef_AniDB_TraktV2>());
                this.OnPropertyChanged(() => Obs_CrossRef_AniDB_Trakt);
                this.OnPropertyChanged(() => TraktCrossRefExists);
                this.OnPropertyChanged(() => TraktCrossRefMissing);
            }
        }

        private ObservableCollectionEx<VM_CrossRef_AniDB_TraktV2> crossRef_AniDB_Trakt;

        public ObservableCollectionEx<VM_CrossRef_AniDB_TraktV2> Obs_CrossRef_AniDB_Trakt
        {
            get
            {
                if (crossRef_AniDB_Trakt == null)
                {
                    crossRef_AniDB_Trakt = new ObservableCollectionEx<VM_CrossRef_AniDB_TraktV2>();
                    if (base.CrossRef_AniDB_Trakt.Count > 0)
                        crossRef_AniDB_Trakt.ReplaceRange(base.CrossRef_AniDB_Trakt.Cast<VM_CrossRef_AniDB_TraktV2>());
                }
                return crossRef_AniDB_Trakt;
            }
        }



        public bool TraktCrossRefExists => !(CrossRef_AniDB_Trakt == null || CrossRef_AniDB_Trakt.Count == 0 || TraktShows == null || TraktShows.Count == 0);
        public bool TraktCrossRefMissing => (CrossRef_AniDB_Trakt == null || CrossRef_AniDB_Trakt.Count == 0 || TraktShows == null || TraktShows.Count == 0);

        #endregion

        #region MAL

        private new List<CrossRef_AniDB_MAL> CrossRef_AniDB_MAL
        {
            get { return Obs_CrossRef_AniDB_MAL.ToList(); }
            set
            {
                crossRef_AniDB_MAL.ReplaceRange(value);
                this.OnPropertyChanged(() => Obs_CrossRef_AniDB_Trakt);
                this.OnPropertyChanged(()=> MALCrossRefExists);
                this.OnPropertyChanged(() => MalCrossRefMissing);
            }
        }

        private ObservableCollectionEx<CrossRef_AniDB_MAL> crossRef_AniDB_MAL;
        public ObservableCollectionEx<CrossRef_AniDB_MAL> Obs_CrossRef_AniDB_MAL
        {
            get
            {
                if (crossRef_AniDB_MAL == null)
                {
                    crossRef_AniDB_MAL = new ObservableCollectionEx<CrossRef_AniDB_MAL>();
                    if (base.CrossRef_AniDB_MAL.Count > 0)
                        crossRef_AniDB_MAL.ReplaceRange(base.CrossRef_AniDB_MAL);
                }
                return crossRef_AniDB_MAL;
            }
        }

        public bool MALCrossRefExists => !(Obs_CrossRef_AniDB_MAL == null || Obs_CrossRef_AniDB_MAL.Count == 0);

        public bool MalCrossRefMissing => (Obs_CrossRef_AniDB_MAL == null || Obs_CrossRef_AniDB_MAL.Count == 0);

        #endregion

        public VM_AniDB_AnimeCrossRefs()
        {
            AllPosters = new List<VM_PosterContainer>();
            AllFanarts = new List<VM_FanartContainer>();
        }
        public void Populate(VM_AniDB_AnimeCrossRefs details)
        {
            AnimeID = details.AnimeID;
            CrossRef_AniDB_MAL = details.CrossRef_AniDB_MAL;
            CrossRef_AniDB_Trakt = details.CrossRef_AniDB_Trakt;
            TraktShows = details.TraktShows;
            TraktImageFanarts = details.TraktImageFanarts;
            TraktImagePosters = details.TraktImagePosters;
            CrossRef_AniDB_Trakt = details.CrossRef_AniDB_Trakt; 
            TvDBSeries = details.TvDBSeries;
            TvDBEpisodes = details.TvDBEpisodes;
            TvDBImageFanarts = details.TvDBImageFanarts;
            TvDBImagePosters = details.TvDBImagePosters;
            TvDBImageWideBanners = details.TvDBImageWideBanners;
            CrossRef_AniDB_MovieDB = details.CrossRef_AniDB_MovieDB;
            MovieDBMovie = details.MovieDBMovie;
            MovieDBFanarts = details.MovieDBFanarts;
            MovieDBPosters = details.MovieDBPosters;
        }
    }
}
