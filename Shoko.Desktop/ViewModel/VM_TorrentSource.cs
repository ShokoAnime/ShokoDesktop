using System.Collections.Generic;
using System.ComponentModel;
using Shoko.Commons.Languages;
using Shoko.Commons.Notification;
using Shoko.Commons.Downloads;
using Shoko.Models.Enums;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_TorrentSource : TorrentSource, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }



        public VM_TorrentSource(TorrentSourceType tsType, bool isEnabled) : base(tsType,isEnabled)
        {
            
        }

        public new bool IsEnabled
        {
            get { return base.IsEnabled; }
            set
            {
                this.SetField(()=>base.IsEnabled, (r)=>base.IsEnabled=r,value);
            }
        }

        public static TorrentSource Create(TorrentSourceType tsType, bool isEnabled)
        {
            return new VM_TorrentSource(tsType, isEnabled);
        }
    }
}
