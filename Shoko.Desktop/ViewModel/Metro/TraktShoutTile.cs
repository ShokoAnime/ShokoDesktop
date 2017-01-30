using System;
using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Metro
{
    public class TraktShoutTile :INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private String showName;
        public String ShowName
        {
            get { return showName; }
            set
            {
                this.SetField(()=>showName,value);
            }
        }

        private String friendName;
        public String FriendName
        {
            get { return friendName; }
            set
            {
                this.SetField(()=>friendName,value);
            }
        }

        private String details;
        public String Details
        {
            get { return details; }
            set
            {
                this.SetField(()=>details,value);
            }
        }

        private String shoutText;
        public String ShoutText
        {
            get { return shoutText; }
            set
            {
                this.SetField(()=>shoutText,value);
            }
        }

        private string shoutDateString;
        public String ShoutDateString
        {
            get { return shoutDateString; }
            set
            {
                this.SetField(()=>shoutDateString,value);
            }
        }

        private String showPicture;
        public String ShowPicture
        {
            get { return showPicture; }
            set
            {
                this.SetField(()=>showPicture,value);
            }
        }

        private String uRL;
        public String URL
        {
            get { return uRL; }
            set
            {
                this.SetField(()=>uRL,value);
            }
        }

        public long Height { get; set; }
        public String TileSize { get; set; }

        private String friendPicture;
        public String FriendPicture
        {
            get { return friendPicture; }
            set
            {
                this.SetField(()=>friendPicture,value);
            }
        }

        private String onlineShowPicture;
        public String OnlineShowPicture
        {
            get { return onlineShowPicture; }
            set
            {
                this.SetField(()=>onlineShowPicture,value);
            }
        }

        private String onlineFriendPicture;
        public String OnlineFriendPicture
        {
            get { return onlineFriendPicture; }
            set
            {
                this.SetField(()=>OnlineFriendPicture,value);
            }
        }
    }
}
