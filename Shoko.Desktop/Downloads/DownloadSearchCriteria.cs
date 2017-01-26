using System.Collections.Generic;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Downloads
{
    public class DownloadSearchCriteria
    {
        private DownloadSearchType searchType = DownloadSearchType.Manual;
        public DownloadSearchType SearchType
        {
            get { return searchType; }
            set { searchType = value; }
        }

        private object searchParameter = null;
        public object SearchParameter
        {
            get { return searchParameter; }
            set { searchParameter = value; }
        }

        public DownloadSearchCriteria(DownloadSearchType sType, object parm)
        {
            searchType = sType;
            searchParameter = parm;
        }

        public List<string> GetParms()
        {
            List<string> parms = new List<string>();

            if (searchType == DownloadSearchType.Episode)
            {
                VM_AnimeEpisode_User ep = searchParameter as VM_AnimeEpisode_User;

                if (ep.AniDB_Anime == null) ep.RefreshAnime();

                VM_AniDB_Anime anime = ep.AniDB_Anime;
                if (anime == null) return parms;

                // only use the first 2 words of the anime's title
                string[] titles = anime.MainTitle.Split(' ');
                int i = 0;
                foreach (string s in titles)
                {
                    i++;
                    parms.Add(s.Trim());
                    if (i == 2) break;
                }

                parms.Add(ep.EpisodeNumber.ToString().PadLeft(2, '0'));
            }

            if (searchType == DownloadSearchType.Series)
            {
                VM_AniDB_Anime anime = searchParameter as VM_AniDB_Anime;

                // only use the first 2 words of the anime's title
                string[] titles = anime.MainTitle.Split(' ');
                int i = 0;
                foreach (string s in titles)
                {
                    i++;
                    parms.Add(s.Trim());
                    if (i == 2) break;
                }
            }

            if (searchType == DownloadSearchType.Manual)
            {
                string[] titles = searchParameter.ToString().Split(' ');
                foreach (string s in titles)
                {
                    parms.Add(s.Trim());
                }
            }

            return parms;
        }

        public override string ToString()
        {
            string ret = "";

            switch (searchType)
            {
                case DownloadSearchType.Episode: ret = "Episode"; break;
                case DownloadSearchType.Manual: ret = "Manual"; break;
                case DownloadSearchType.Series: ret = "Anime"; break;
            }

            ret += ": ";

            int i = 0;
            List<string> parms = GetParms();
            foreach (string parm in parms)
            {
                i++;
                ret += parm;
                if (i < parms.Count) ret += " + ";
            }

            return ret;
        }
    }
}
