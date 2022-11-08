using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
using Shoko.Models.Enums;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_MissingFile : CL_MissingFile, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public new VM_AnimeSeries_User AnimeSeries
        {
            get { return (VM_AnimeSeries_User) base.AnimeSeries; }
            set { this.SetField(()=>base.AnimeSeries,(r)=> base.AnimeSeries = r, value, ()=>HasSeriesData); }
        }

        [JsonIgnore, XmlIgnore]
        public string AniDB_SiteURL => string.Format(Models.Constants.URLS.AniDB_Series, AnimeID);

        [JsonIgnore, XmlIgnore]
        public string Episode_SiteURL => string.Format(Models.Constants.URLS.AniDB_Episode, EpisodeID);

        [JsonIgnore, XmlIgnore]
        public string File_SiteURL => string.Format(Models.Constants.URLS.AniDB_File, FileID);

        [JsonIgnore, XmlIgnore]
        public string AnimeTitleAndID => $"{AnimeTitle} ({AnimeID})";

        [JsonIgnore, XmlIgnore]
        public string EpisodeNumberAndID => $"Episode {EpisodeTypeAndNumber} ({EpisodeID})";

        [JsonIgnore, XmlIgnore]
        public string FileDescAndID => $"File {FileID}";

        [JsonIgnore, XmlIgnore]
        public EpisodeType EpisodeTypeEnum => (EpisodeType)EpisodeType;

        public new int EpisodeType
        {
            get { return base.EpisodeType; }
            set { this.SetField(()=>base.EpisodeType,(r)=> base.EpisodeType = r, value, ()=> EpisodeType, ()=>EpisodeTypeAndNumber); }
        }

        public new int EpisodeNumber
        {
            get { return base.EpisodeNumber; }
            set { this.SetField(()=>base.EpisodeNumber,(r)=> base.EpisodeNumber = r, value, () => EpisodeNumber, ()=>EpisodeTypeAndNumber); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        [JsonIgnore, XmlIgnore]
        public bool HasSeriesData => AnimeSeries!=null;

        [JsonIgnore, XmlIgnore]
        public string EpisodeTypeAndNumber
        {
            get
            {
                string shortType = "";
                switch (EpisodeTypeEnum)
                {
                    case Models.Enums.EpisodeType.Credits: shortType = "C"; break;
                    case Models.Enums.EpisodeType.Episode: shortType = ""; break;
                    case Models.Enums.EpisodeType.Other: shortType = "O"; break;
                    case Models.Enums.EpisodeType.Parody: shortType = "P"; break;
                    case Models.Enums.EpisodeType.Special: shortType = "S"; break;
                    case Models.Enums.EpisodeType.Trailer: shortType = "T"; break;
                }
                return $"{shortType}{EpisodeNumber}";
            }

        }
    }
}
