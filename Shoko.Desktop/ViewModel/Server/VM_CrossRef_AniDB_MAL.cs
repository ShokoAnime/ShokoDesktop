using Shoko.Commons.Extensions;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_MAL : CrossRef_AniDB_MAL
    {
        public string SiteURL => this.GetSiteURL();
        public string StartEpisodeTypeString => this.GetStartEpisodeTypeString();


    }
}
