using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace JMMClient.Downloads
{
	public class TorrentLinkVM
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public TorrentSourceVM Source { get; set; }

		private string torrentName;
		public string TorrentName
		{
			get
			{
				try
				{
					string dQuote = ((char)34).ToString();
					string rubish1 = "<span class=" + dQuote + "s" + dQuote + "> </span>";

					string match = torrentName;
					match = match.Replace("&#40;", "(");
					match = match.Replace("&#41;", ")");
					match = match.Replace("&#39;", "'");
					match = match.Replace("&#96;", "`");
					match = match.Replace(rubish1, "");

					return match;
				}
				catch (Exception ex)
				{
					logger.ErrorException(ex.ToString(), ex);
					return "ERROR";
				}
			}
			set {
				RawTorrentName = value;
				torrentName = value; 
			}
		}

		public string TorrentDownloadLink { get; set; }
		public string TorrentLink { get; set; }
		public string AnimeType { get; set; }
		public string Size { get; set; }
		public string Seeders { get; set; }
		public string Leechers { get; set; }
		public string RawTorrentName { get; set; }

		public TorrentLinkVM()
		{
		}

		public TorrentLinkVM(TorrentSourceType tsType)
		{
			Source = new TorrentSourceVM(tsType, true);
		}

		public override string ToString()
		{
			return string.Format("Torrent:   ({0}) {1}({2}) - {3} ", Source.TorrentSourceName, TorrentName, RawTorrentName, TorrentDownloadLink);
		}

		public string ToStringMatch()
		{
			return string.Format("Torrent Match:   {0} - {1} ", RawTorrentName, ClosestAnimeMatchString);
		}

		public string ExtraInfo
		{
			get
			{
				return string.Format("{0} ", AnimeType);
			}
		}

		public string TorrentLinkFull
		{
			get
			{
				switch (Source.TorrentSource)
				{
					case TorrentSourceType.BakaBT: return string.Format(@"http://bakabt.me{0} ", TorrentLink);
					case TorrentSourceType.AnimeBytes: return string.Format(@"http://animebyt.es/{0}", TorrentLink);
				}
				return TorrentLink;
			}
		}


		public string ClosestAnimeMatchString
		{
			get
			{

				try
				{
					string match = TorrentName;

					try { match = Path.GetFileNameWithoutExtension(TorrentName); }
					catch { }

					//match = match.Replace("&#40;", "(");
					//match = match.Replace("&#41;", ")");
					//match = match.Replace("&#39;", "'");
					//match = match.Replace("&#96;", "`");

					//remove any group names or CRC's
					while (true)
					{
						int pos = match.IndexOf('[');
						if (pos >= 0)
						{
							int endPos = match.IndexOf(']', pos);
							if (endPos >= 0)
							{
								string rubbish = match.Substring(pos, endPos - pos + 1);
								match = match.Replace(rubbish, "");
							}
							else break;
						}
						else break;
					}

					//remove any video information
					while (true)
					{
						int pos = match.IndexOf('(');
						if (pos >= 0)
						{
							int endPos = match.IndexOf(')', pos);
							if (endPos >= 0)
							{
								string rubbish = match.Substring(pos, endPos - pos + 1);
								match = match.Replace(rubbish, "");
							}
							else break;
						}
						else break;
					}

					match = match.Replace("_", " ");

					int pos2 = match.IndexOf('-');
					if (pos2 >= 1)
					{
						match = match.Substring(0, pos2).Trim();
					}

					return match;
				}
				catch (Exception ex)
				{
					logger.ErrorException(ex.ToString(), ex);
					return "";
				}
			}
		}
	}
}
