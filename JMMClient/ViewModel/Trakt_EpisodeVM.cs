using JMMClient.ImageDownload;
using System.IO;

namespace JMMClient.ViewModel
{
    public class Trakt_EpisodeVM
    {
        public int Trakt_EpisodeID { get; set; }
        public int Trakt_ShowID { get; set; }
        public int Season { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string Overview { get; set; }
        public string EpisodeImage { get; set; }

        public Trakt_EpisodeVM(JMMServerBinary.Contract_Trakt_Episode contract)
        {
            this.Trakt_EpisodeID = contract.Trakt_EpisodeID;
            this.Trakt_ShowID = contract.Trakt_ShowID;
            this.Season = contract.Season;
            this.EpisodeNumber = contract.EpisodeNumber;
            this.Title = contract.Title;
            this.URL = contract.URL;
            this.Overview = contract.Overview;
            this.EpisodeImage = contract.EpisodeImage;
        }

        public string FullImagePathPlain
        {
            get
            {
                // typical EpisodeImage url
                // http://vicmackey.trakt.tv/images/episodes/3228-1-1.jpg

                // get the TraktID from the URL
                // http://trakt.tv/show/11eyes/season/1/episode/1 (11 eyes)

                if (string.IsNullOrEmpty(EpisodeImage)) return "";
                if (string.IsNullOrEmpty(URL)) return "";

                // on Trakt, if the episode doesn't have a proper screenshot, they will return the
                // fanart instead, we will ignore this
                int pos = EpisodeImage.IndexOf(@"episodes/");
                if (pos <= 0) return "";

                int posID = URL.IndexOf(@"show/");
                if (posID <= 0) return "";

                int posIDNext = URL.IndexOf(@"/", posID + 6);
                if (posIDNext <= 0) return "";

                string traktID = URL.Substring(posID + 5, posIDNext - posID - 5);
                traktID = traktID.Replace("/", @"\");

                string imageName = EpisodeImage.Substring(pos + 9, EpisodeImage.Length - pos - 9);
                imageName = imageName.Replace("/", @"\");

                string relativePath = Path.Combine("episodes", traktID);
                relativePath = Path.Combine(relativePath, imageName);

                string filename = Path.Combine(Utils.GetTraktImagePath(), relativePath);

                return filename;
            }
        }

        public string FullImagePath
        {
            get
            {


                if (!File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Episode, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }

        public string OnlineImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(EpisodeImage)) return "";
                return EpisodeImage;
            }
        }
    }
}
