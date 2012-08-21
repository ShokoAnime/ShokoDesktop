using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace JMMClient.Downloads
{
	public class TorrentsAnimeBytes : ITorrentSource
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private TorrentSourceType SourceType = TorrentSourceType.AnimeBytes;

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


		public string Login(string username, string password)
		{
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return "";

			try
			{
				CookieContainer container = new CookieContainer();
				string formUrl = "http://animebyt.es/login.php"; // NOTE: This is the URL the form POSTs to, not the URL of the form (you can find this in the "action" attribute of the HTML's form tag
				string formParams = string.Format("username={0}&password={1}", username, password);

				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(formUrl);
				req.ContentType = "application/x-www-form-urlencoded";
				req.Method = "POST";
				req.CookieContainer = container;
				req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
				req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
				req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				byte[] bytes = Encoding.ASCII.GetBytes(formParams);
				req.ContentLength = bytes.Length;
				using (Stream os = req.GetRequestStream())
				{
					os.Write(bytes, 0, bytes.Length);
				}


				HttpWebResponse WebResponse = (HttpWebResponse)req.GetResponse();

				Stream responseStream = WebResponse.GetResponseStream();
				String enco = WebResponse.CharacterSet;
				Encoding encoding = null;
				if (!String.IsNullOrEmpty(enco))
					encoding = Encoding.GetEncoding(WebResponse.CharacterSet);
				if (encoding == null)
					encoding = Encoding.Default;
				StreamReader Reader = new StreamReader(responseStream, encoding);

				string output = Reader.ReadToEnd();

				logger.Trace(ShowAllCookies(container));

				if (container.Count < 3)
					return "";



				//Grab the cookie we just got back for this specifc page
				return container.GetCookieHeader(new Uri("http://animebyt.es/index.php"));
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
				return "";
			}
		}

		private List<TorrentLinkVM> ParseSource(string output)
		{
			List<TorrentLinkVM> torLinks = new List<TorrentLinkVM>();

			char q = (char)34;
			string quote = q.ToString();

			string nameStart = "<a href=" + quote + "series.php?id=";
			string nameStart2 = quote + ">";
			string nameEnd = "</a>";

			// title="View Torrent">
			string typeStart = "title=" + quote + "View Torrent" + quote + ">";
			string typeEnd = "</a>";

			// <tr class="edition_info"><td class=" " colspan="5"><strong>
			//string torrentSectionStart = "<tr class=" + quote + "edition_info";
			// <table class="torrent_group">
			string torrentSectionStart = "<table class=" + quote + "torrent_group";
			// </table>
			string torrentSectionEnd = "</table>";

			// <a href="torrents.php?action=download
			string torDownloadStart = "<a href=" + quote + "torrents.php?action=download";
			string torDownloadEnd = quote;

			// <a href="torrents.php?id=
			string torInfoPreStart = "<a href=" + quote + "torrents.php?id=";
			string torInfoStart = ">";
			string torInfoEnd = "</a>";

			// <td class="torrent_size"><span>
			string torSizeStart = "<td class=" + quote + "torrent_size" + quote + "><span>";
			string torSizeEnd = "</span>";

			// <td class="torrent_seeders" title="Seeders"><span>
			string torSeedStart = "<td class=" + quote + "torrent_seeders" + quote + " title=" + quote + "Seeders" + quote + "><span>";
			string torSeedEnd = "</span>";

			// <td class="torrent_leechers" title="Leechers"><span>
			string torLeechStart = "<td class=" + quote + "torrent_leechers" + quote + " title=" + quote + "Leechers" + quote + "><span>";
			string torLeechEnd = "</span>";

			// <img src="static/common/hentaic.png" alt="Hentai" title="This torrent is of censored hentai (18+) material!" />
			// <img src="static/common/flicon.png" alt="Freeleech!" title="This torrent is freeleech. Remember to seed!" />
			string hentaiTag = "alt=" + quote + "Hentai";
			string leechTag = "alt=" + quote + "Freeleech";

			int pos = output.IndexOf(nameStart, 0);
			while (pos > 0)
			{

				if (pos <= 0) break;

				//int posnameStart = output.IndexOf(nameStart, pos + 1);
				int posnameStart2 = output.IndexOf(nameStart2, pos + nameStart.Length);
				int posnameEnd = output.IndexOf(nameEnd, posnameStart2 + nameStart2.Length + 1);

				string torName = output.Substring(posnameStart2 + nameStart2.Length, posnameEnd - posnameStart2 - nameStart2.Length);

				// remove html codes
				torName = HttpUtility.HtmlDecode(torName);

				int posTypeStart = output.IndexOf(typeStart, posnameEnd + 1);
				int posTypeEnd = output.IndexOf(typeEnd, posTypeStart + 1);

				string torType = output.Substring(posTypeStart + typeStart.Length, posTypeEnd - posTypeStart - typeStart.Length);

				// get all the torrents

				// find the section start and end
				int posTorSectionStart = output.IndexOf(torrentSectionStart, posTypeEnd + 1);
				int posTorSectionEnd = output.IndexOf(torrentSectionEnd, posTorSectionStart + 1);

				// find all the torrents
				int posTorDownloadStart = output.IndexOf(torDownloadStart, posTorSectionStart + 1);

				while (posTorDownloadStart < posTorSectionEnd && posTorDownloadStart > 0)
				{
					int posTorDownloadEnd = output.IndexOf(torDownloadEnd, posTorDownloadStart + 9);

					string torDownloadLink = output.Substring(posTorDownloadStart + 9, posTorDownloadEnd - posTorDownloadStart - 9);
					torDownloadLink = HttpUtility.HtmlDecode(torDownloadLink);

					int posTorInfoPreStart = output.IndexOf(torInfoPreStart, posTorDownloadEnd + 1);
					int posTorInfoStart = output.IndexOf(torInfoStart, posTorInfoPreStart + 1);
					int posTorInfoEnd = output.IndexOf(torInfoEnd, posTorInfoStart + 1);

					string torInfo = output.Substring(posTorInfoStart + 1, posTorInfoEnd - posTorInfoStart - 1);

					// TODO - extract out <img> imfomation
					// <img src="static/common/hentaic.png" alt="Hentai" title="This torrent is of censored hentai (18+) material!" />
					// <img src="static/common/flicon.png" alt="Freeleech!" title="This torrent is freeleech. Remember to seed!" />
					int posImgStart = torInfo.IndexOf("<img src=", 0);
					if (posImgStart >= 0)
					{
						bool hentai = torInfo.ToUpper().Contains(hentaiTag.ToUpper());
						bool freeLeech = torInfo.ToUpper().Contains(leechTag.ToUpper());

						// remove the img alts
						torInfo = torInfo.Substring(0, posImgStart - 2);

						if (hentai) torInfo = torInfo + " [hentai]";
						if (freeLeech) torInfo = torInfo + " [FREE Leech]";
					}


					int posTorSizeStart = output.IndexOf(torSizeStart, posTorInfoEnd + 1);
					int posTorSizeEnd = output.IndexOf(torSizeEnd, posTorSizeStart + 1);

					string torSize = output.Substring(posTorSizeStart + torSizeStart.Length, posTorSizeEnd - posTorSizeStart - torSizeStart.Length);


					int posTorSeedStart = output.IndexOf(torSeedStart, posTorSizeEnd + 1);
					int posTorSeedEnd = output.IndexOf(torSeedEnd, posTorSeedStart + 1);

					string torSeed = output.Substring(posTorSeedStart + torSeedStart.Length, posTorSeedEnd - posTorSeedStart - torSeedStart.Length);

					int posTorLeechStart = output.IndexOf(torLeechStart, posTorSeedEnd + 1);
					int posTorLeechEnd = output.IndexOf(torLeechEnd, posTorLeechStart + 1);

					string torLeech = output.Substring(posTorLeechStart + torLeechStart.Length, posTorLeechEnd - posTorLeechStart - torLeechStart.Length);

					TorrentLinkVM torrentLink = new TorrentLinkVM(TorrentSourceType.AnimeBytes);
					torrentLink.TorrentDownloadLink = string.Format(@"http://animebyt.es/{0}", torDownloadLink);
					torrentLink.TorrentLink = "";
					torrentLink.AnimeType = torType;
					torrentLink.TorrentName = torName + " - " + torInfo;
					torrentLink.Size = torSize.Trim();
					torrentLink.Seeders = torSeed.Trim();
					torrentLink.Leechers = torLeech.Trim();
					torLinks.Add(torrentLink);

					// find the next download link
					posTorDownloadStart = output.IndexOf(torDownloadStart, posTorLeechEnd + 1);
				}

				// find the next torrent group
				pos = output.IndexOf(nameStart, pos + 3);
			}

			return torLinks;
		}

		public List<TorrentLinkVM> GetTorrents(List<string> searchParms)
		{
			try
			{
				if (string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesUsername) || string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesPassword))
					return new List<TorrentLinkVM>();

				if (string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesCookieHeader))
				{
					string cookie = Login(UserSettingsVM.Instance.AnimeBytesUsername, UserSettingsVM.Instance.AnimeBytesPassword);
					UserSettingsVM.Instance.AnimeBytesCookieHeader = cookie;
				}

				if (string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesCookieHeader))
					return new List<TorrentLinkVM>();

				string urlBase = "http://animebyt.es/torrents.php?filter_cat%5B1%5D=1&searchstr={0}&action=advanced&search_type=title&year=&year2=&tags=&tags_type=0&sort=time_added&way=desc&hentai=2&releasegroup=&epcount=&epcount2=&artbooktitle=";

				string searchCriteria = "";
				foreach (string parm in searchParms)
				{
					if (searchCriteria.Length > 0) searchCriteria += "+";
					searchCriteria += parm.Trim();
				}

				string url = string.Format(urlBase, searchCriteria);
				string output = Utils.DownloadWebPage(url, UserSettingsVM.Instance.AnimeBytesCookieHeader, true);

				return ParseSource(output);
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
				return new List<TorrentLinkVM>();
			}
		}

		public List<TorrentLinkVM> BrowseTorrents()
		{
			try
			{
				if (string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesUsername) || string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesPassword))
					return new List<TorrentLinkVM>();

				if (string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesCookieHeader))
				{
					string cookie = Login(UserSettingsVM.Instance.AnimeBytesUsername, UserSettingsVM.Instance.AnimeBytesPassword);
					UserSettingsVM.Instance.AnimeBytesCookieHeader = cookie;
				}

				if (string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesCookieHeader))
					return new List<TorrentLinkVM>();

				string url = "http://animebyt.es/torrents.php?filter_cat[1]=1&sort=time_added&way=desc";
				string output = Utils.DownloadWebPage(url, UserSettingsVM.Instance.AnimeBytesCookieHeader, true);

				return ParseSource(output);
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
				return new List<TorrentLinkVM>();
			}

			/*string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			string fileName = "source.txt";
			string fileNameWithPath = Path.Combine(appPath, fileName);

			StreamReader re = File.OpenText(fileNameWithPath);
			string rawText = re.ReadToEnd();
			re.Close();


			return ParseSource(rawText);*/
		}

		private List<Cookie> GetAllCookies(CookieContainer cc)
		{
			List<Cookie> lstCookies = new List<Cookie>();

			Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

			foreach (var pathList in table.Values)
			{
				SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
				foreach (CookieCollection colCookies in lstCookieCol.Values)
					foreach (Cookie c in colCookies) lstCookies.Add(c);
			}

			return lstCookies;
		}

		private string ShowAllCookies(CookieContainer cc)
		{
			StringBuilder sb = new StringBuilder();
			List<Cookie> lstCookies = GetAllCookies(cc);
			sb.AppendLine("=========================================================== ");
			sb.AppendLine(lstCookies.Count + " cookies found.");
			sb.AppendLine("=========================================================== ");
			int cpt = 1;
			foreach (Cookie c in lstCookies)
				sb.AppendLine("#" + cpt++ + "> Name: " + c.Name + "\tValue: " + c.Value + "\tDomain: " + c.Domain + "\tPath: " + c.Path + "\tExp: " + c.Expires.ToString());

			return sb.ToString();
		}

		#endregion
	}
}
