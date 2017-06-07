using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.TvDB;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_TVDB_Series_Search_Response : TVDB_Series_Search_Response
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string BannerURL => this.GetBannerURL();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string SeriesURL => this.GetSeriesURL();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string LanguageFlagImage => this.GetLanguageFlagImage();
    }
}
