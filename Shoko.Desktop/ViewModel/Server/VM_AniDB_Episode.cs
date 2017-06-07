using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Episode : AniDB_Episode
    {
        [ScriptIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string EpisodeName => this.GetEpisodeName();
    }
}
