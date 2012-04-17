using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Web;

namespace JMMClient.Downloads
{
	public class TorrentsNyaa : ITorrentSource
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private TorrentSourceType SourceType = TorrentSourceType.Nyaa;

		#region ITorrentSource Members

		public string TorrentSourceName
		{
			get
			{
				return EnumTranslator.TorrentSourceTranslated(SourceType);
			}
		}

		public string TorrentSourceNameShort
		{
			get
			{
				return EnumTranslator.TorrentSourceTranslatedShort(SourceType);
			}
		}

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


		private List<TorrentLinkVM> ParseSource(string output)
		{
			List<TorrentLinkVM> torLinks = new List<TorrentLinkVM>();

			char q = (char)34;
			string quote = q.ToString();

			string startBlock = @"http://www.nyaa.eu/?page=torrentinfo";

			string nameStart = ">";
			string nameEnd = "</a>";

			string torStart = "href=" + quote;
			string torEnd = quote;



			string sizeStart = "tlistsize" + quote + ">";
			string sizeEnd = "</td>";

			string seedStart = "tlistsn" + quote + ">";
			string seedEnd = "</td>";

			string leechStart = "tlistln" + quote + ">";
			string leechEnd = "</td>";

			int pos = output.IndexOf(startBlock, 0);
			while (pos > 0)
			{

				if (pos <= 0) break;

				int posNameStart = output.IndexOf(nameStart, pos + 1);
				int posNameEnd = output.IndexOf(nameEnd, posNameStart + nameStart.Length + 1);

				string torName = output.Substring(posNameStart + nameStart.Length, posNameEnd - posNameStart - nameStart.Length);

				int posTorStart = output.IndexOf(torStart, posNameEnd);
				int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

				string torLink = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
				torLink = DownloadHelper.FixNyaaTorrentLink(torLink);

				// remove html codes
				torLink = HttpUtility.HtmlDecode(torLink);

				string torSize = "";
				int posSizeStart = output.IndexOf(sizeStart, posNameEnd);
				int posSizeEnd = 0;
				if (posSizeStart > 0)
				{
					posSizeEnd = output.IndexOf(sizeEnd, posSizeStart + sizeStart.Length + 1);

					torSize = output.Substring(posSizeStart + sizeStart.Length, posSizeEnd - posSizeStart - sizeStart.Length);
				}

				string torSeed = "";
				int posSeedStart = output.IndexOf(seedStart, posSizeEnd);
				int posSeedEnd = 0;
				if (posSeedStart > 0)
				{
					posSeedEnd = output.IndexOf(seedEnd, posSeedStart + seedStart.Length + 1);

					torSeed = output.Substring(posSeedStart + seedStart.Length, posSeedEnd - posSeedStart - seedStart.Length);
				}

				string torLeech = "";
				int posLeechStart = output.IndexOf(leechStart, posSeedStart + 3);
				int posLeechEnd = 0;
				if (posLeechStart > 0)
				{
					posLeechEnd = output.IndexOf(leechEnd, posLeechStart + leechStart.Length + 1);

					torLeech = output.Substring(posLeechStart + leechStart.Length, posLeechEnd - posLeechStart - leechStart.Length);
				}

				TorrentLinkVM torrentLink = new TorrentLinkVM(SourceType);
				torrentLink.TorrentDownloadLink = torLink;
				torrentLink.TorrentName = torName;
				torrentLink.Size = torSize.Trim();
				torrentLink.Seeders = torSeed.Trim();
				torrentLink.Leechers = torLeech.Trim();
				torLinks.Add(torrentLink);

				pos = output.IndexOf(startBlock, pos + 1);

			}
			//Console.ReadLine();

			return torLinks;
		}

		public List<TorrentLinkVM> GetTorrents(List<string> searchParms)
		{
			string urlBase = "http://www.nyaa.eu/?page=search&cats=1_37&filter=0&term={0}";

			string searchCriteria = "";
			foreach (string parm in searchParms)
			{
				if (searchCriteria.Length > 0) searchCriteria += "+";
				searchCriteria += parm.Trim();
			}

			string url = string.Format(urlBase, searchCriteria);
			string output = Utils.DownloadWebPage(url);

			logger.Trace("GetTorrents Search: " + url);
			//logger.Trace("GetTorrents Results: " + output);

			return ParseSource(output);
		}

		public List<TorrentLinkVM> BrowseTorrents()
		{
			string url = "http://www.nyaa.eu/?page=torrents&cats=1_37";
			string output = Utils.DownloadWebPage(url);

			return ParseSource(output);
		}

		#endregion
	}
}
