using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class LogMessageVM
	{
		public int LogMessageID { get; set; }
		public string LogType { get; set; }
		public string LogContent { get; set; }
		public DateTime LogDate { get; set; }

		public LogMessageVM(JMMServerBinary.Contract_LogMessage contract)
		{
			this.LogMessageID = contract.LogMessageID;
			this.LogType = contract.LogType;
			this.LogContent = contract.LogContent;
			this.LogDate = contract.LogDate;
		}
	}
}
