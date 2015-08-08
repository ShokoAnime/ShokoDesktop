using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JMMClient.ImageDownload;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
    public class Trakt_ShoutUserVM : BindableObject
	{
		public int AnimeID { get; set; }

		// user details
		public Trakt_UserVM User { get; set; }
		// shout details
		public Trakt_ShoutVM Shout { get; set; }

		public Trakt_ShoutUserVM(JMMServerBinary.Contract_Trakt_ShoutUser contract)
		{
			this.User = new Trakt_UserVM(contract.User);
			this.Shout = new Trakt_ShoutVM(contract.Shout);
		}


        private bool isShoutCollapsed = false;
        public bool IsShoutCollapsed
        {
            get { return isShoutCollapsed; }
            set
            {
                isShoutCollapsed = value;
                base.RaisePropertyChanged("IsShoutCollapsed");
            }
        }

        private bool isShoutExpanded = false;
        public bool IsShoutExpanded
        {
            get { return isShoutExpanded; }
            set
            {
                isShoutExpanded = value;
                base.RaisePropertyChanged("IsShoutExpanded");
            }
        }

        public string CommentTruncated
        {
            get
            {
                if (Shout.Text.Length > 250)
                    return Shout.Text.Substring(0, 250) + ".......";
                else
                    return Shout.Text;
            }
        }

        public string Comment
        {
            get
            {
                return Shout.Text;
            }
        }
	}
}
