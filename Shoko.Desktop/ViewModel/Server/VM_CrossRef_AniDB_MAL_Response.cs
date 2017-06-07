using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_MAL_Response : CL_CrossRef_AniDB_MAL_Response
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string SiteURL => this.GetSiteURL();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string StartEpisodeTypeString => this.GetStartEpisodeTypeString();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool AdminApproved => this.GetAdminApproved();
    }
}
