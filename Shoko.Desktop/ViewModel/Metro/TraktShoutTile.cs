using System;
using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Metro
{
    public class TraktShoutTile : INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private string showName;
        public string ShowName { get => showName; set => this.SetField(()=>showName,value); }

        private string friendName;
        public string FriendName { get => friendName; set => this.SetField(()=>friendName,value); }

        private string details;
        public string Details { get => details; set => this.SetField(()=>details,value); }

        private string shoutText;
        public string ShoutText { get => shoutText; set => this.SetField(()=>shoutText,value); }

        private string shoutDateString;
        public string ShoutDateString { get => shoutDateString; set => this.SetField(()=>shoutDateString,value); }

        private string showPicture;
        public string ShowPicture { get => showPicture; set => this.SetField(()=>showPicture,value); }

        private string url;
        public string URL { get => url; set => this.SetField(()=>url,value); }

        private long height;
        public long Height { get => height; set => this.SetField(()=> height, value); }

        private string tileSize;
        public string TileSize { get => tileSize; set => this.SetField(()=> tileSize, value); }

        private string friendPicture;
        public string FriendPicture { get => friendPicture; set => this.SetField(()=>friendPicture,value); }

        private string onlineShowPicture;
        public string OnlineShowPicture { get => onlineShowPicture; set => this.SetField(()=>onlineShowPicture,value); }

        private string onlineFriendPicture;
        public string OnlineFriendPicture { get => onlineFriendPicture; set => this.SetField(()=>OnlineFriendPicture,value); }
    }
}
