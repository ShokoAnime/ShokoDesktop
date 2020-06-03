using System.ComponentModel;

using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_Trakt_CommentUser : CL_Trakt_CommentUser, INotifyPropertyChanged, INotifyPropertyChangedExt
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        private bool isCommentExpanded;
        [JsonIgnore, XmlIgnore]
        public bool IsCommentExpanded
        {
            get { return isCommentExpanded; }
            set
            {
                this.SetField(()=>isCommentExpanded,value);
            }
        }

        [JsonIgnore, XmlIgnore]
        public string CommentTruncated
        {
            get
            {
                if (Comment.Text.Length > 250)
                    return Comment.Text.Substring(0, 250) + ".......";
                else
                    return Comment.Text;
            }
        }

        public string CommentText => Comment.Text;
    }
}
