using System;
using System.IO;
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

        public bool HasAnimeSeries => AnimeSeriesID.HasValue;

        public string EpisodeDescription => $"{Episode_Season}x{Episode_Number} - {Episode_Title}";

        public string ImagePathForDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(FullImagePath) && File.Exists(FullImagePath)) return FullImagePath;

                // use fanart instead
                if (!string.IsNullOrEmpty(Anime?.FanartPathOnlyExisting))
                    return Anime.FanartPathOnlyExisting;

                return @"/Images/EpisodeThumb_NotFound.png";
            }
        }

        public string OnlineImagePath => string.IsNullOrEmpty(Episode_Screenshot) ? "" : Episode_Screenshot;

        public string FullImagePath
        {
            get
            {
                // typical EpisodeImage url
                // http://vicmackey.trakt.tv/images/episodes/3228-1-1.jpg

                // get the TraktID from the URL
                // http://trakt.tv/show/11eyes/season/1/episode/1 (11 eyes)

                if (string.IsNullOrEmpty(Episode_Screenshot)) return "";

                // on Trakt, if the episode doesn't have a proper screenshot, they will return the
                // fanart instead, we will ignore this
                int pos = Episode_Screenshot.IndexOf(@"episodes/", StringComparison.Ordinal);
                if (pos <= 0) return "";

                string traktID = TraktShow.GetTraktID();
                traktID = traktID.Replace("/", @"\");

                string imageName = Episode_Screenshot.Substring(pos + 9, Episode_Screenshot.Length - pos - 9);
                imageName = imageName.Replace("/", @"\");

                string relativePath = Path.Combine("episodes", traktID);
                relativePath = Path.Combine(relativePath, imageName);

                return Path.Combine(Utils.GetTraktImagePath(), relativePath);
            }
        }

        public string ShowTitle => Anime != null ? Anime.FormattedTitle : TraktShow.title;

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
