using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.Downloads
{
	public class TorrentSourceVM
	{
		public TorrentSourceType TorrentSource { get; set; }

		public string TorrentSourceName 
		{
			get
			{
				return EnumTranslator.TorrentSourceTranslated(TorrentSource);
			}
		}

		public string TorrentSourceNameShort
		{
			get
			{
				return EnumTranslator.TorrentSourceTranslatedShort(TorrentSource);
			}
		}

		public TorrentSourceVM()
		{
		}

		public TorrentSourceVM(TorrentSourceType tsType)
		{
			TorrentSource = tsType;
		}

		public List<TorrentLinkVM> BrowseTorrents()
		{
			List<TorrentLinkVM> links = new List<TorrentLinkVM>();

			if (TorrentSource == TorrentSourceType.Nyaa)
			{
				TorrentsNyaa nyaa = new TorrentsNyaa();
				List<TorrentLinkVM> ttLinks = nyaa.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (TorrentSource == TorrentSourceType.TokyoToshokanAnime)
			{
				TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan(TorrentSourceType.TokyoToshokanAnime);
				List<TorrentLinkVM> ttLinks = tt.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (TorrentSource == TorrentSourceType.TokyoToshokanAll)
			{
				TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan(TorrentSourceType.TokyoToshokanAll);
				List<TorrentLinkVM> ttLinks = tt.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (TorrentSource == TorrentSourceType.AnimeSuki)
			{
				TorrentsAnimeSuki suki = new TorrentsAnimeSuki();
				List<TorrentLinkVM> sukiLinks = suki.BrowseTorrents();
				links.AddRange(sukiLinks);
			}

			

			/*if (TorrentSource == TorrentSource.BakaUpdates)
			{
				TorrentsBakaUpdates bakau = new TorrentsBakaUpdates();
				List<TorrentLink> bakauLinks = bakau.BrowseTorrents();
				links.AddRange(bakauLinks);
			}*/

			foreach (TorrentLinkVM torLink in links)
				NLog.LogManager.GetCurrentClassLogger().Trace(torLink.ToStringMatch());

			return links;
		}
	}
}
