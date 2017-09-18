using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeTag : CL_AnimeTag, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new string TagName
        {
            get => base.TagName;
            set => this.SetField(() => base.TagName, (r) => base.TagName = r, value);
        }

        public new string TagDescription
        {
            get => base.TagDescription;
            set => this.SetField(() => base.TagDescription, (r) => base.TagDescription = r, value);
        }
    }
}