using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Models.Client;
using Shoko.Models.Server;

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CustomTag : CustomTag, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new string TagName
        {
            get => base.TagName == null ? null : string.Intern(base.TagName);
            set => this.SetField(() => base.TagName, (r) => base.TagName = r == null ? null : string.Intern(r), value);
        }

        public new string TagDescription
        {
            get => base.TagDescription == null ? null : string.Intern(base.TagDescription);
            set => this.SetField(() => base.TagDescription, (r) => base.TagDescription = r == null ? null : string.Intern(r), value);
        }
    }
}