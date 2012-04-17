using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JMMClient.ViewModel;
using NLog;

namespace JMMClient.Downloads
{
	public class DownloadHelper
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static string FixNyaaTorrentLink(string url)
		{
			// on some trackers the user will post the torrent page instead of the 
			// direct torrent link
			return url.Replace("page=torrentinfo", "page=download");
		}

		public static List<TorrentLinkVM> SearchTorrents(DownloadSearchCriteria search)
		{
			List<string> parms = search.GetParms();
			List<TorrentLinkVM> links = new List<TorrentLinkVM>();




			TorrentsAnimeSuki suki = new TorrentsAnimeSuki();

			List<string> episodeGroupParms = new List<string>();

			// lets do something special for episodes
			/*if (UserSettingsVM.Instance.TorrentSearchPreferOwnGroups && search.SearchType == DownloadSearchType.Episode)
			{
				AnimeEpisodeVM ep = search.SearchParameter as AnimeEpisodeVM;

				AnimeSeriesVM series = MainListHelperVM.Instance.GetSeries(ep.AnimeSeriesID);
				if (series != null && series.AniDB_Anime != null)
				{
					List<GroupVideoQualityVM> videoQualityRecords = new List<GroupVideoQualityVM>();
					List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(series.AniDB_ID);
					foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
					{
						GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
						videoQualityRecords.Add(vidQual);
					}

					// apply sorting
					if (videoQualityRecords.Count > 0)
					{
						List<SortPropOrFieldAndDirection> sortlist = new List<SortPropOrFieldAndDirection>();
						sortlist.Add(new SortPropOrFieldAndDirection("FileCountNormal", true, SortType.eInteger));
						videoQualityRecords = Sorting.MultiSort<GroupVideoQualityVM>(videoQualityRecords, sortlist);
					}

					//only use the first 2
					int i = 0;
					foreach (GroupVideoQualityVM gvq in videoQualityRecords)
					{
						if (i == 2) break;
						if (!episodeGroupParms.Contains(gvq.GroupNameShort))
						{
							episodeGroupParms.Add(gvq.GroupNameShort);
							i++;
						}
					}
				}
			}*/

			foreach (TorrentSourceVM src in UserSettingsVM.Instance.SelectedTorrentSources)
			{
				if (src.TorrentSource == TorrentSourceType.Nyaa)
				{
					TorrentsNyaa nyaa = new TorrentsNyaa();
					List<TorrentLinkVM> ttLinks = null;
					Dictionary<string, TorrentLinkVM> dictLinks = new Dictionary<string, TorrentLinkVM>();

					foreach (string grp in episodeGroupParms)
					{
						List<string> tempParms = new List<string>();
						foreach (string parmTemp in parms)
							tempParms.Add(parmTemp);
						tempParms.Insert(0, grp);
						ttLinks = nyaa.GetTorrents(tempParms);

						logger.Trace("Searching for: " + search.ToString() + "(" + grp + ")");

						// only use the first 10
						int x = 0;
						foreach (TorrentLinkVM link in ttLinks)
						{
							if (x == 10) break;
							dictLinks[link.TorrentDownloadLink] = link;
							logger.Trace("Adding link: " + link.ToString());
						}
					}

					logger.Trace("Searching for: " + search.ToString());
					ttLinks = nyaa.GetTorrents(parms);
					foreach (TorrentLinkVM link in ttLinks)
					{
						dictLinks[link.TorrentDownloadLink] = link;
						//logger.Trace("Adding link: " + link.ToString());
					}

					links.AddRange(dictLinks.Values);
				}

				if (src.TorrentSource == TorrentSourceType.AnimeSuki)
				{
					List<TorrentLinkVM> sukiLinks = suki.GetTorrents(parms);
					links.AddRange(sukiLinks);
				}

				if (src.TorrentSource == TorrentSourceType.TokyoToshokanAll || src.TorrentSource == TorrentSourceType.TokyoToshokanAnime)
				{
					TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan(src.TorrentSource);
					List<TorrentLinkVM> ttLinks = null;
					Dictionary<string, TorrentLinkVM> dictLinks = new Dictionary<string, TorrentLinkVM>();

					foreach (string grp in episodeGroupParms)
					{
						List<string> tempParms = new List<string>();
						foreach (string parmTemp in parms)
							tempParms.Add(parmTemp);
						tempParms.Insert(0, grp);
						ttLinks = tt.GetTorrents(tempParms);

						logger.Trace("Searching for: " + search.ToString() + "(" + grp + ")");

						// only use the first 10
						int x = 0;
						foreach (TorrentLinkVM link in ttLinks)
						{
							if (x == 0) break;
							dictLinks[link.TorrentDownloadLink] = link;
							//logger.Trace("Adding link: " + link.ToString());
						}
					}

					logger.Trace("Searching for: " + search.ToString());
					ttLinks = tt.GetTorrents(parms);
					foreach (TorrentLinkVM link in ttLinks)
					{
						dictLinks[link.TorrentDownloadLink] = link;
						//logger.Trace("Adding link: " + link.ToString());
					}

					links.AddRange(dictLinks.Values);
				}

				
			}



			return links;
		}
	}
}
