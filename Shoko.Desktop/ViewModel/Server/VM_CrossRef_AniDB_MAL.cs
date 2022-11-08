
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CrossRef_AniDB_MAL : CrossRef_AniDB_MAL
    {
        [JsonIgnore, XmlIgnore]
        public string SiteURL => this.GetSiteURL();
        [JsonIgnore, XmlIgnore]
        public string StartEpisodeTypeString => this.GetStartEpisodeTypeString();

        [JsonIgnore, XmlIgnore]
        public new string MALTitle => string.IsNullOrWhiteSpace(base.MALTitle) ? $"({MALID})" : base.MALTitle;
    }
}
