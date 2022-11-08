using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Recommendation : AniDB_Recommendation, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new int UserID
        {
            get => base.UserID;
            set => this.SetField(() => base.UserID, (r) => base.UserID = r, value);
        }

        [JsonIgnore, XmlIgnore]
        public string CommentTruncated => this.GetCommentTruncated();
        [JsonIgnore, XmlIgnore]
        public string Comment => this.GetComment();
        [JsonIgnore, XmlIgnore]
        public AniDBRecommendationType RecommendationTypeEnum => this.GetRecommendationTypeEnum();

        [JsonIgnore, XmlIgnore]
        public string RecommendationTypeText => this.GetRecommendationTypeText();
    }
}
