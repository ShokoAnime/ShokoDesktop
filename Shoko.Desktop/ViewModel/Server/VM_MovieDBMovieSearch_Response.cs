using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_MovieDBMovieSearch_Response : CL_MovieDBMovieSearch_Response
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string SiteURL => this.GetSiteURL();
    }
}
