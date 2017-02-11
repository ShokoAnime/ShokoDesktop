using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_MAL_Response : CL_CrossRef_AniDB_MAL_Response
    {
        public string SiteURL => this.GetSiteURL();

        public string StartEpisodeTypeString => this.GetStartEpisodeTypeString();

        public bool AdminApproved => this.GetAdminApproved();
    }
}
