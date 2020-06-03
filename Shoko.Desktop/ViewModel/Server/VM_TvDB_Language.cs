using System;

using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.TvDB;

namespace Shoko.Desktop.ViewModel.Server
{
    // ReSharper disable once InconsistentNaming
    public class VM_TvDB_Language : TvDB_Language
    {
        [JsonIgnore, XmlIgnore]
        public string LanguageFlagImage => string.Intern(this.GetLanguageFlagImage());
    }
}
