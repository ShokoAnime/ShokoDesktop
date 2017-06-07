using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeTitle : CL_AnimeTitle
    {
        [ScriptIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string FlagImage => this.GetFlagImage();
        [ScriptIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string LanguageDescription => this.GetLanguageDescription();
    }
}
