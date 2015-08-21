using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class Trakt_CommentPost
	{
		public int AnimeID { get; set; }
        public string TraktID { get; set; }
		public string CommentText { get; set; }
		public bool Spoiler { get; set; }
	}
}
