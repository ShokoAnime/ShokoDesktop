using System.Collections.Generic;

namespace Shoko.Desktop.Downloads
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
