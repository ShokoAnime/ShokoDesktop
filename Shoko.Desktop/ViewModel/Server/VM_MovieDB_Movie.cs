using Shoko.Commons.Extensions;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_MovieDB_Movie : MovieDB_Movie
    {
        public string SiteUrl => this.GetSiteURL();
    }
}
