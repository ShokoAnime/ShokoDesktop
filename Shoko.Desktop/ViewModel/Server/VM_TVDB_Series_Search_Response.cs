using Shoko.Commons.Extensions;
using Shoko.Models.TvDB;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_TVDB_Series_Search_Response : TVDB_Series_Search_Response
    {
        public string BannerURL => this.GetBannerURL();
        public string SeriesURL => this.GetSeriesURL();
        public string LanguageFlagImage => this.GetLanguageFlagImage();
    }
}
