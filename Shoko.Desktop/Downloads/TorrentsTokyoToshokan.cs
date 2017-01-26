using NLog;
using System.Collections.Generic;
using System.Web;
using Shoko.Commons.Languages;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;

namespace Shoko.Desktop.Downloads
{
    public class TorrentsTokyoToshokan : ITorrentSource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private TorrentSourceType SourceType = TorrentSourceType.TokyoToshokanAnime;

        #region ITorrentSource Members

        public string TorrentSourceName => EnumTranslator.TorrentSourceTranslated(SourceType);

        public string TorrentSourceNameShort => EnumTranslator.TorrentSourceTranslatedShort(SourceType);

        public string GetSourceName()
        {
            return TorrentSourceName;
        }

        public string GetSourceNameShort()
        {
            return TorrentSourceNameShort;
        }

        public bool SupportsSearching()
        {
            return true;
        }

        public bool SupportsBrowsing()
        {
            return true;
        }

        public bool SupportsCRCMatching()
        {
            return true;
        }

        public TorrentsTokyoToshokan()
        {
        }

        public TorrentsTokyoToshokan(TorrentSourceType tsType)
        {
            SourceType = tsType;
        }

        private List<TorrentLinkVM> ParseSource(string output)
        {
            List<TorrentLinkVM> torLinks = new List<TorrentLinkVM>();

            char q = (char)34;
            string quote = q.ToString();

            string startBlock = "<a rel=" + quote + "nofollow" + quote + " type=" + quote + "application/x-bittorrent" + quote;

            string torStart = "href=" + quote;
            string torEnd = quote;

            string nameStart = ">";
            string nameEnd = "</a>";

            string sizeStart = "Size:";
            string sizeEnd = "|";

            int pos = output.IndexOf(startBlock, 0);
            while (pos > 0)
            {

                if (pos <= 0) break;

                int posTorStart = output.IndexOf(torStart, pos + 1);
                int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

                //Console.WriteLine("{0} - {1}", posTorStart, posTorEnd);

                string torLink = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
                torLink = DownloadHelper.FixNyaaTorrentLink(torLink);

                // remove html codes
                //torLink = torLink.Replace("amp;", "");
                torLink = HttpUtility.HtmlDecode(torLink);

                int posNameStart = output.IndexOf(nameStart, posTorEnd);
                int posNameEnd = output.IndexOf(nameEnd, posNameStart + nameStart.Length + 1);


                string torName = output.Substring(posNameStart + nameStart.Length, posNameEnd - posNameStart - nameStart.Length);

                string torSize = "";
                int posSizeStart = output.IndexOf(sizeStart, posNameEnd);
                if (posSizeStart > 0)
                {
                    int posSizeEnd = output.IndexOf(sizeEnd, posSizeStart + sizeStart.Length + 1);

                    torSize = output.Substring(posSizeStart + sizeStart.Length, posSizeEnd - posSizeStart - sizeStart.Length);
                }

                TorrentLinkVM torrentLink = new TorrentLinkVM(SourceType);
                torrentLink.TorrentDownloadLink = torLink;
                torrentLink.TorrentName = torName;
                torrentLink.Size = torSize.Trim();
                torLinks.Add(torrentLink);

                pos = output.IndexOf(startBlock, pos + 1);

            }

            return torLinks;
        }

        public List<TorrentLinkVM> GetTorrents(List<string> searchParms)
        {
            string urlBase = "http://www.tokyotosho.info/search.php?terms={0}&type=1";
            if (SourceType == TorrentSourceType.TokyoToshokanAll)
                urlBase = "http://www.tokyotosho.info/search.php?terms={0}";

            string searchCriteria = "";
            foreach (string parm in searchParms)
            {
                if (searchCriteria.Length > 0) searchCriteria += "+";
                searchCriteria += parm.Trim();
            }

            string url = string.Format(urlBase, searchCriteria);
            string output = Utils.DownloadWebPage(url);

            logger.Trace("GetTorrents Search: " + url);

            return ParseSource(output);
        }

        public List<TorrentLinkVM> BrowseTorrents()
        {
            string url = "http://www.tokyotosho.info/?cat=1";
            if (SourceType == TorrentSourceType.TokyoToshokanAll)
                url = "http://www.tokyotosho.info/";
            string output = Utils.DownloadWebPage(url);

            return ParseSource(output);
        }

        #endregion
    }
}
