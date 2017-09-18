using System;
using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.ViewModel.Metro
{
    public class RandomSeriesTile : INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private string animeName = null;
        public string AnimeName { get => animeName; set => this.SetField(()=> animeName, value); }

        private string details = null;
        public string Details { get => details; set => this.SetField(()=> details, value); }

        private string picture = null;
        public string Picture { get => picture; set => this.SetField(()=> picture, value); }

        private long height = 0L;
        public long Height { get => height; set => this.SetField(()=> height, value); }

        private string tileSize = null;
        public string TileSize { get => tileSize; set => this.SetField(()=> tileSize, value); }

        private VM_AnimeSeries_User animeSeries = null;
        public VM_AnimeSeries_User AnimeSeries { get => animeSeries; set => this.SetField(()=> animeSeries, value); }

    }
}
