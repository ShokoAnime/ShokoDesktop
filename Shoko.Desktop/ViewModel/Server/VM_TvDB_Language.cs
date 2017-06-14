using System;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.TvDB;

namespace Shoko.Desktop.ViewModel.Server
{
    // ReSharper disable once InconsistentNaming
    public class VM_TvDB_Language : TvDB_Language
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string LanguageFlagImage => string.Intern(this.GetLanguageFlagImage());
    }
}
