using Shoko.Commons.Extensions;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_MALAnime_Response : CL_MALAnime_Response
    {
        public string SiteURL => this.GetSiteURL();
        public string Overview => this.GetOverview();
        public string PosterPath => this.GetPosterPath();

    }
}
