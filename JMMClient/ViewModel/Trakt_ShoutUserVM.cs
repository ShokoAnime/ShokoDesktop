using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JMMClient.ImageDownload;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
    public class Trakt_CommentUserVM : BindableObject
	{
		public int AnimeID { get; set; }

		// user details
		public Trakt_UserVM User { get; set; }
		// comment details
		public Trakt_CommentVM Comment { get; set; }

		public Trakt_CommentUserVM(JMMServerBinary.Contract_Trakt_CommentUser contract)
		{
			this.User = new Trakt_UserVM(contract.User);
			this.Comment = new Trakt_CommentVM(contract.Comment);
		}


        private bool isCommentCollapsed = false;
        public bool IsCommentCollapsed
        {
            get { return isCommentCollapsed; }
            set
            {
                isCommentCollapsed = value;
                base.RaisePropertyChanged("IsCommentCollapsed");
            }
        }

        private bool isCommentExpanded = false;
        public bool IsCommentExpanded
        {
            get { return isCommentExpanded; }
            set
            {
                isCommentExpanded = value;
                base.RaisePropertyChanged("IsCommentExpanded");
            }
        }

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

        public string CommentText
        {
            get
            {
                return Comment.Text;
            }
        }
	}
}
