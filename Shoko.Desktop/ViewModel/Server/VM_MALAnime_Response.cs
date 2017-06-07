using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_MALAnime_Response : CL_MALAnime_Response
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string SiteURL => this.GetSiteURL();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string Overview => this.GetOverview();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string PosterPath => this.GetPosterPath();

    }
}
