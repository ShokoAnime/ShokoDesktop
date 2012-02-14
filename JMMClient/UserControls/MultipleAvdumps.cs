using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
	public class MultipleAvdumps
	{
		public int SelectedCount { get; set; }
		public List<AVDumpVM> AVDumps { get; set; }
	}
}
