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
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FlagImage => string.Intern(this.GetFlagImage());
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string LanguageDescription => string.Intern(this.GetLanguageDescription());

        public new string TitleType
        {
            get => base.TitleType == null ? null : string.Intern(base.TitleType);
            set => base.TitleType = value == null ? null : string.Intern(value);
        }

        public new string Language
        {
            get => base.Language == null ? null : string.Intern(base.Language);
            set => base.Language = value == null ? null : string.Intern(value);
        }

        public new string Title
        {
            get => base.Title == null ? null : string.Intern(base.Title);
            set => base.Title = value == null ? null : string.Intern(value);
        }
    }
}
