using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Episode : CL_AniDB_Episode
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string EpisodeName
        {
            get
            {
                if (Titles.ContainsKey("EN")) return Titles["EN"];
                if (Titles.ContainsKey("X-JAT")) return Titles["X-JAT"];
                return $"Episode {EpisodeNumber}";
            }
        }
    }
}
