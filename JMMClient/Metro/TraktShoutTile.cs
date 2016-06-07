using System;

namespace JMMClient
{
    public class TraktShoutTile : BindableObject
    {
        private String showName;
        public String ShowName
        {
            get { return showName; }
            set
            {
                showName = value;
                base.RaisePropertyChanged("ShowName");
            }
        }

        private String friendName;
        public String FriendName
        {
            get { return friendName; }
            set
            {
                friendName = value;
                base.RaisePropertyChanged("FriendName");
            }
        }

        private String details;
        public String Details
        {
            get { return details; }
            set
            {
                details = value;
                base.RaisePropertyChanged("Details");
            }
        }

        private String shoutText;
        public String ShoutText
        {
            get { return shoutText; }
            set
            {
                shoutText = value;
                base.RaisePropertyChanged("ShoutText");
            }
        }

        private string shoutDateString;
        public String ShoutDateString
        {
            get { return shoutDateString; }
            set
            {
                shoutDateString = value;
                base.RaisePropertyChanged("ShoutDateString");
            }
        }

        private String showPicture;
        public String ShowPicture
        {
            get { return showPicture; }
            set
            {
                showPicture = value;
                base.RaisePropertyChanged("ShowPicture");
            }
        }

        private String uRL;
        public String URL
        {
            get { return uRL; }
            set
            {
                uRL = value;
                base.RaisePropertyChanged("URL");
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
                friendPicture = value;
                base.RaisePropertyChanged("FriendPicture");
            }
        }

        private String onlineShowPicture;
        public String OnlineShowPicture
        {
            get { return onlineShowPicture; }
            set
            {
                onlineShowPicture = value;
                base.RaisePropertyChanged("OnlineShowPicture");
            }
        }

        private String onlineFriendPicture;
        public String OnlineFriendPicture
        {
            get { return onlineFriendPicture; }
            set
            {
                onlineFriendPicture = value;
                base.RaisePropertyChanged("OnlineFriendPicture");
            }
        }
    }
}
