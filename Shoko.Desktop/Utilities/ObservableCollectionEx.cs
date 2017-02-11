using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Shoko.Desktop.Utilities
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            foreach (var item in items)
                Items.Add(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void ReplaceRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}