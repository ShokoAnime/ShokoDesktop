using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupVideoQuality : CL_GroupVideoQuality
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool HasAnySpecials => this.GetHasAnySpecials();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string TotalFileSizeFormatted => this.GetTotalFileSizeFormatted();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string AverageFileSizeFormatted => this.GetAverageFileSizeFormatted();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string PrettyDescription => this.GetPrettyDescription();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsBluRay => this.IsBluRay();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsDVD => this.IsDVD();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsHD => this.IsHD();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsFullHD => this.IsFullHD();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsHi08P => this.IsHi08P();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsHi10P => this.IsHi10P();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsHi12P => this.IsHi12P();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsDualAudio => this.IsDualAudio();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsMultiAudio => this.IsMultiAudio();
    }
}
