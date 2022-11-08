
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Episode : CL_AniDB_Episode
    {
        [JsonIgnore, XmlIgnore]
        public string EpisodeName
        {
            get
            {
                if (Titles.ContainsKey("en")) return Titles["en"];
                if (Titles.ContainsKey("x-jat")) return Titles["x-jat"];
                return $"Episode {EpisodeNumber}";
            }
        }
    }
}
