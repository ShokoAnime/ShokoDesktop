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

			List<string> episodeGroupParms = new List<string>();

			// get the sources that are in both the selected sources and the default sources
			// default sources have an order
			List<TorrentSourceVM> orderedSources = new List<TorrentSourceVM>();

			// if only full torrent sites
			bool onlyFullSites = false;
			if (search.SearchType == DownloadSearchType.Series)
			{
				if (UserSettingsVM.Instance.BakaBTOnlyUseForSeriesSearches &&
				!string.IsNullOrEmpty(UserSettingsVM.Instance.BakaBTUsername) && !string.IsNullOrEmpty(UserSettingsVM.Instance.BakaBTPassword))
				{
					onlyFullSites = true;
					TorrentSourceVM src = new TorrentSourceVM(TorrentSourceType.BakaBT, true);
					orderedSources.Add(src);
				}

				if (UserSettingsVM.Instance.AnimeBytesOnlyUseForSeriesSearches &&
				!string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesUsername) && !string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesPassword))
				{
					onlyFullSites = true;
					TorrentSourceVM src = new TorrentSourceVM(TorrentSourceType.AnimeBytes, true);
					orderedSources.Add(src);
				}
			}

			if (!onlyFullSites)
			{
				foreach (TorrentSourceVM src in UserSettingsVM.Instance.SelectedTorrentSources)
				{
					foreach (TorrentSourceVM srcCur in UserSettingsVM.Instance.CurrentSearchTorrentSources)
					{
						if (srcCur.IsDisabled) continue;
						if (src.TorrentSource == srcCur.TorrentSource)
							orderedSources.Add(srcCur);
					}
				}

				// now get any sources that we missed
				foreach (TorrentSourceVM src in UserSettingsVM.Instance.CurrentSearchTorrentSources)
				{
					if (src.IsDisabled) continue;
					bool foundSource = false;
					foreach (TorrentSourceVM srcDone in orderedSources)
					{
						if (srcDone.TorrentSource == src.TorrentSource) foundSource = true;
					}
					if (!foundSource)
						orderedSources.Add(src);
				}
			}

			foreach (TorrentSourceVM src in orderedSources)
			{
				if (src.IsDisabled) continue;

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

                if (src.TorrentSource == TorrentSourceType.Sukebei)
                {
                    TorrentsSukebei sukebei = new TorrentsSukebei();
                    List<TorrentLinkVM> ttLinks = null;
                    Dictionary<string, TorrentLinkVM> dictLinks = new Dictionary<string, TorrentLinkVM>();

                    foreach (string grp in episodeGroupParms)
                    {
                        List<string> tempParms = new List<string>();
                        foreach (string parmTemp in parms)
                            tempParms.Add(parmTemp);
                        tempParms.Insert(0, grp);
                        ttLinks = sukebei.GetTorrents(tempParms);

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
                    ttLinks = sukebei.GetTorrents(parms);
                    foreach (TorrentLinkVM link in ttLinks)
                    {
                        dictLinks[link.TorrentDownloadLink] = link;
                        //logger.Trace("Adding link: " + link.ToString());
                    }

                    links.AddRange(dictLinks.Values);
                }

                if (src.TorrentSource == TorrentSourceType.AnimeSuki)
				{
					TorrentsAnimeSuki suki = new TorrentsAnimeSuki();
					List<TorrentLinkVM> sukiLinks = suki.GetTorrents(parms);
					links.AddRange(sukiLinks);
				}

				if (src.TorrentSource == TorrentSourceType.BakaBT)
				{
					TorrentsBakaBT bakaBT = new TorrentsBakaBT();
					List<TorrentLinkVM> bbLinks = bakaBT.GetTorrents(parms);
					links.AddRange(bbLinks);
				}

				if (src.TorrentSource == TorrentSourceType.AnimeBytes)
				{
					TorrentsAnimeBytes abytes = new TorrentsAnimeBytes();
					List<TorrentLinkVM> abytesLinks = abytes.GetTorrents(parms);
					links.AddRange(abytesLinks);
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
