using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_MovieDBMovieSearch_Response : CL_MovieDBMovieSearch_Response
    {
        public string SiteURL => this.GetSiteURL();
    }
}
