
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupFileSummary : CL_GroupFileSummary
    {
        [JsonIgnore, XmlIgnore]
        public bool HasAnySpecials => this.HasAnySpecials();
        [JsonIgnore, XmlIgnore]
        public string TotalFileSizeFormatted => this.GetTotalFileSizeFormatted();
        [JsonIgnore, XmlIgnore]
        public string AverageFileSizeFormatted => this.GetAverageFileSizeFormatted();
        [JsonIgnore, XmlIgnore]
        public string PrettyDescription => this.GetPrettyDescription();
    }
}
