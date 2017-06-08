using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_MAL : CrossRef_AniDB_MAL
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string SiteURL => this.GetSiteURL();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string StartEpisodeTypeString => this.GetStartEpisodeTypeString();


    }
}
