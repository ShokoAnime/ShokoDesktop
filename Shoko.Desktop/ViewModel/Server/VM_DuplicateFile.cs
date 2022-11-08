
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_DuplicateFile : CL_DuplicateFile
    {
        [JsonIgnore, XmlIgnore]
        public string LocalFilePath1 => this.GetLocalFilePath1();
        [JsonIgnore, XmlIgnore]
        public string LocalFilePath2 => this.GetLocalFilePath2();
        [JsonIgnore, XmlIgnore]
        public string LocalFileName1 => this.GetLocalFileName1();
        [JsonIgnore, XmlIgnore]
        public string LocalFileName2 => this.GetLocalFileName2();
        [JsonIgnore, XmlIgnore]
        public string LocalFileDirectory1 => this.GetLocalFileDirectory1();
        [JsonIgnore, XmlIgnore]
        public string LocalFileDirectory2 => this.GetLocalFileDirectory2();
        [JsonIgnore, XmlIgnore]
        public string EpisodeNumberAndName => this.GetEpisodeNumberAndName();
    }
}
