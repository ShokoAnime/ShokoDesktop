
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Server
{
    // ReSharper disable once InconsistentNaming
    public class VM_VideoLocal_Renamed : CL_VideoLocal_Renamed
    {
        [JsonIgnore, XmlIgnore]
        public string StatusImage => this.GetStatusImage();
    }
}
