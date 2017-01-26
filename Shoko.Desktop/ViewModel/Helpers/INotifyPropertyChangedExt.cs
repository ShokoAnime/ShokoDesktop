using System.ComponentModel;

namespace Shoko.Desktop.ViewModel.Helpers
{
    public interface INotifyPropertyChangedExt : INotifyPropertyChanged
    {
        void NotifyPropertyChanged(string propname);
    }
}