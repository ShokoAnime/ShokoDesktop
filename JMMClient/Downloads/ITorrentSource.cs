using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.Downloads
{
	public interface ITorrentSource
	{
		string GetSourceName();
		string GetSourceNameShort();
		List<TorrentLinkVM> GetTorrents(List<string> searchParms);
		bool SupportsSearching();
		bool SupportsBrowsing();
		bool SupportsCRCMatching();
	}
}
