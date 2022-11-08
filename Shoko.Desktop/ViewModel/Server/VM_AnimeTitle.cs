using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeTitle : CL_AnimeTitle, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        private string flagImage;
        [JsonIgnore, XmlIgnore]
        public string FlagImage => flagImage ?? (flagImage = string.Intern(this.GetFlagImage()));

        private string languageDescription;
        [JsonIgnore, XmlIgnore]
        public string LanguageDescription => languageDescription ?? (languageDescription = string.Intern(this.GetLanguageDescription()));

        public new string TitleType
        {
            get => base.TitleType == null ? null : string.Intern(base.TitleType);
            set => this.SetField(() => base.TitleType, (r) => base.TitleType = r == null ? null : string.Intern(r), value);
        }

        public new string Language
        {
            get => base.Language == null ? null : string.Intern(base.Language);
            set => this.SetField(() => base.Language, (r) => base.Language = r == null ? null : string.Intern(r), value);
        }

        public new string Title
        {
            get => base.Title == null ? null : string.Intern(base.Title);
            set => this.SetField(() => base.Title, (r) => base.Title = r == null ? null : string.Intern(r), value);
        }
    }
}
