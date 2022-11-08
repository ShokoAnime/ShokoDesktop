using System;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_Trakt_Comment : CL_Trakt_Comment
    {
        
        public new CL_TraktTVShowResponse TraktShow { get { return base.TraktShow; } set { base.TraktShow = value; } }
        public new VM_AniDB_Anime Anime { get { return (VM_AniDB_Anime) base.Anime; } set { base.Anime = value; } }

        [JsonIgnore, XmlIgnore]
        public bool HasAnimeSeries => AnimeSeriesID.HasValue;

        [JsonIgnore, XmlIgnore]
        public string EpisodeDescription => $"{Episode_Season}x{Episode_Number} - {Episode_Title}";

        [JsonIgnore, XmlIgnore]
        public string ImagePathForDisplay => @"/Images/EpisodeThumb_NotFound.png";

        [JsonIgnore, XmlIgnore]
        public string ShowTitle => Anime != null ? Anime.FormattedTitle : TraktShow.title;

        [JsonIgnore, XmlIgnore]
        public string CommentDateString
        {
            get
            {
                if (Inserted.HasValue)
                    //return Inserted.Value.ToString("dd MMM yyyy", Globals.Culture);
                    return Inserted.Value.ToString("dd MMM yyyy", Commons.Culture.Global) + ", " + Inserted.Value.ToShortTimeString();
                else
                    return "";
            }
        }

        public new string Episode_Title {
            get
            {
                if (!string.IsNullOrEmpty(base.Episode_Title) && base.Episode_Title.Length > 30)
                    return base.Episode_Title.Substring(0, 30) + "...";
                return base.Episode_Title;
            }
            set { base.Episode_Title = value; }
        }

     
    }
}
