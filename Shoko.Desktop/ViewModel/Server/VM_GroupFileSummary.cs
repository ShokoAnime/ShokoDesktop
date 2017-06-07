using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupFileSummary : CL_GroupFileSummary
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasAnySpecials => this.HasAnySpecials();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string TotalFileSizeFormatted => this.GetTotalFileSizeFormatted();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AverageFileSizeFormatted => this.GetAverageFileSizeFormatted();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string PrettyDescription => this.GetPrettyDescription();
    }
}
