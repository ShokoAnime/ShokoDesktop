using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Models.Enums;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Anime_DefaultImage : CL_AniDB_Anime_DefaultImage
    {
        public new VM_MovieDB_Poster MoviePoster
        {
            get { return (VM_MovieDB_Poster) base.MoviePoster; }
            set { base.MoviePoster = value; }
        }

        public new VM_MovieDB_Fanart MovieFanart
        {
            get { return (VM_MovieDB_Fanart)base.MovieFanart; }
            set { base.MovieFanart = value; }
        }

        public new VM_TvDB_ImagePoster TVPoster
        {
            get { return (VM_TvDB_ImagePoster)base.TVPoster; }
            set { base.TVPoster = value; }
        }

        public new VM_TvDB_ImageFanart TVFanart
        {
            get { return (VM_TvDB_ImageFanart)base.TVFanart; }
            set { base.TVFanart = value; }
        }

        public new VM_TvDB_ImageWideBanner TVWideBanner
        {
            get { return (VM_TvDB_ImageWideBanner)base.TVWideBanner; }
            set { base.TVWideBanner = value; }
        }

        public new VM_Trakt_ImagePoster TraktPoster
        {
            get { return (VM_Trakt_ImagePoster)base.TraktPoster; }
            set { base.TraktPoster = value; }
        }
        public new VM_Trakt_ImageFanart TraktFanart
        {
            get { return (VM_Trakt_ImageFanart)base.TraktFanart; }
            set { base.TraktFanart = value; }
        }


        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullImagePath
        {
            get
            {
                ImageEntityType itype = (ImageEntityType)ImageParentType;
                string fileName = "";

                switch (itype)
                {
                    case ImageEntityType.AniDB_Cover:
                        if (VM_MainListHelper.Instance.AllAnimeDictionary.ContainsKey(AnimeID))
                        {
                            fileName = VM_MainListHelper.Instance.AllAnimeDictionary[AnimeID].PosterPath;
                        }
                        break;

                    case ImageEntityType.TvDB_Cover:
                        fileName = TVPoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        fileName = MoviePoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_FanArt:
                        fileName = MovieFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_FanArt:
                        fileName = TVFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_Banner:
                        fileName = TVWideBanner.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Poster:
                        fileName = TraktPoster.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Fanart:
                        fileName = TraktFanart.FullImagePath;
                        break;
                }

                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    return fileName;
                return $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullImagePathOnlyExisting
        {
            get
            {
                ImageEntityType itype = (ImageEntityType)ImageParentType;
                string fileName = "";

                switch (itype)
                {
                    case ImageEntityType.AniDB_Cover:
                        if (VM_MainListHelper.Instance.AllAnimeDictionary.ContainsKey(AnimeID))
                        {
                            fileName = VM_MainListHelper.Instance.AllAnimeDictionary[AnimeID].PosterPath;
                        }
                        break;

                    case ImageEntityType.TvDB_Cover:
                        fileName = TVPoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        fileName = MoviePoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_FanArt:
                        fileName = MovieFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_FanArt:
                        fileName = TVFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_Banner:
                        fileName = TVWideBanner.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Poster:
                        fileName = TraktPoster.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Fanart:
                        fileName = TraktFanart.FullImagePath;
                        break;
                }

                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    return fileName;
                else
                    return "";

            }
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullThumbnailPath
        {
            get
            {
                ImageEntityType itype = (ImageEntityType)ImageParentType;
                string fileName = "";

                switch (itype)
                {
                    case ImageEntityType.AniDB_Cover:
                        if (VM_MainListHelper.Instance.AllAnimeDictionary.ContainsKey(AnimeID))
                        {
                            fileName = VM_MainListHelper.Instance.AllAnimeDictionary[AnimeID].PosterPath;
                        }
                        break;

                    case ImageEntityType.TvDB_Cover:
                        fileName = TVPoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        fileName = MoviePoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_FanArt:
                        fileName = MovieFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_FanArt:
                        fileName = TVFanart.FullThumbnailPath;
                        break;

                    case ImageEntityType.TvDB_Banner:
                        fileName = TVWideBanner.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Poster:
                        fileName = TraktPoster.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Fanart:
                        fileName = TraktFanart.FullImagePath;
                        break;
                }

                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    return fileName;
                return $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Images/blankposter.png";
            }
        }

    }
}
