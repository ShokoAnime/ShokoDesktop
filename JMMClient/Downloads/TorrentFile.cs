using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace JMMClient.Downloads
{
	public class TorrentFile
	{
		private string fileName;
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		private long fileSize = 0;
		public long FileSize
		{
			get { return fileSize; }
			set { fileSize = value; }
		}

		private long downloaded = 0;
		public long Downloaded
		{
			get { return downloaded; }
			set { downloaded = value; }
		}

		private long priority = 0;
		public long Priority
		{
			get { return priority; }
			set { priority = value; }
		}

		public TorrentFile()
		{
		}

		public TorrentFile(DataRow row)
		{
			this.fileName = row[0].ToString();
			this.fileSize = long.Parse(row[1].ToString());
			this.downloaded = long.Parse(row[2].ToString());
			this.priority = long.Parse(row[3].ToString());
		}

		public override string ToString()
		{
			return string.Format("Torrent File: {0} - {1}", fileName, FileSizeFormatted);
		}

		public string PriorityFormatted
		{
			get
			{
				switch (Priority)
				{
					case 0: return "Skip";
					case 1: return "Low";
					case 2: return "Normal";
					case 3: return "High";
				}

				return "";
			}
		}

		public string FileSizeFormatted
		{
			get
			{
				return Utils.FormatByteSize(fileSize);
			}
		}

		public string DownloadedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)downloaded);
			}
		}
	}
}
