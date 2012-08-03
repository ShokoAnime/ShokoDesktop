using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class VideoLocalRenamedVM
	{
		public int VideoLocalID { get; set; }
		public VideoLocalVM VideoLocal { get; set; }
		public string NewFileName { get; set; }

		public VideoLocalRenamedVM(JMMServerBinary.Contract_VideoLocalRenamed contract)
		{
			this.VideoLocalID = contract.VideoLocalID;
			this.NewFileName = contract.NewFileName;
			this.VideoLocal = new VideoLocalVM(contract.VideoLocal);
		}

		public VideoLocalRenamedVM()
		{
			this.VideoLocalID = -1;
			this.NewFileName = "";
			this.VideoLocal = null;
		}
	}
}
