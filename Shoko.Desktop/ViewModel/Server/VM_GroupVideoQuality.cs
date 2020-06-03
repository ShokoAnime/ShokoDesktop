using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupVideoQuality : CL_GroupVideoQuality
    {
        [JsonIgnore, XmlIgnore]
        public bool HasAnySpecials => this.HasAnySpecials();
        [JsonIgnore, XmlIgnore]
        public string TotalFileSizeFormatted => this.GetTotalFileSizeFormatted();
        [JsonIgnore, XmlIgnore]
        public string AverageFileSizeFormatted => this.GetAverageFileSizeFormatted();
        [JsonIgnore, XmlIgnore]
        public string PrettyDescription => this.GetPrettyDescription();
        [JsonIgnore, XmlIgnore]
        public bool IsBluRay => this.IsBluRay();
        [JsonIgnore, XmlIgnore]
        public bool IsDVD => this.IsDVD();
        [JsonIgnore, XmlIgnore]
        public bool IsHD => this.IsHD();
        [JsonIgnore, XmlIgnore]
        public bool IsFullHD => this.IsFullHD();
        [JsonIgnore, XmlIgnore]
        public bool IsHi08P => this.IsHi08P();
        [JsonIgnore, XmlIgnore]
        public bool IsHi10P => this.IsHi10P();
        [JsonIgnore, XmlIgnore]
        public bool IsHi12P => this.IsHi12P();
        [JsonIgnore, XmlIgnore]
        public bool IsDualAudio => this.IsDualAudio();
        [JsonIgnore, XmlIgnore]
        public bool IsMultiAudio => this.IsMultiAudio();
    }
}
