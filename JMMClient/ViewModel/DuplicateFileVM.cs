using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class DuplicateFileVM : IComparable<DuplicateFileVM>
	{
		public int DuplicateFileID { get; set; }
		public string FilePathFile1 { get; set; }
		public string FilePathFile2 { get; set; }
		public string Hash { get; set; }
		public int ImportFolderIDFile1 { get; set; }
		public int ImportFolderIDFile2 { get; set; }
		public DateTime DateTimeUpdated { get; set; }

		// data from other entities
		public int? AnimeID { get; set; }
		public string AnimeName { get; set; }
		public int? EpisodeType { get; set; }
		public int? EpisodeNumber { get; set; }
		public string EpisodeName { get; set; }

		public ImportFolderVM ImportFolder1 { get; set; }
		public ImportFolderVM ImportFolder2 { get; set; }

		public string FullPath1
		{
			get
			{
				if (AppSettings.ImportFolderMappings.ContainsKey(ImportFolderIDFile1))
					return Path.Combine(AppSettings.ImportFolderMappings[ImportFolderIDFile1], FilePathFile1);
				else
					return Path.Combine(ImportFolder1.ImportFolderLocation, FilePathFile1);
			}
		}

		public string FileName1
		{
			get
			{
				return Path.GetFileName(FullPath1);
			}
		}

		public string FileDirectory1
		{
			get
			{
				return Path.GetDirectoryName(FullPath1);
			}
		}

		public string FullPath2
		{
			get
			{
				if (AppSettings.ImportFolderMappings.ContainsKey(ImportFolderIDFile2))
					return Path.Combine(AppSettings.ImportFolderMappings[ImportFolderIDFile2], FilePathFile2);
				else
					return Path.Combine(ImportFolder2.ImportFolderLocation, FilePathFile2);
			}
		}

		public string FileName2
		{
			get
			{
				return Path.GetFileName(FullPath2);
			}
		}

		public string FileDirectory2
		{
			get
			{
				return Path.GetDirectoryName(FullPath2);
			}
		}

		public string EpisodeNumberAndName
		{
			get
			{
				string shortType = "";
				if (EpisodeType.HasValue)
				{
					EpisodeType epType = (EpisodeType)EpisodeType.Value;
					switch (epType)
					{
						case JMMClient.EpisodeType.Credits: shortType = "C"; break;
						case JMMClient.EpisodeType.Episode: shortType = ""; break;
						case JMMClient.EpisodeType.Other: shortType = "O"; break;
						case JMMClient.EpisodeType.Parody: shortType = "P"; break;
						case JMMClient.EpisodeType.Special: shortType = "S"; break;
						case JMMClient.EpisodeType.Trailer: shortType = "T"; break;
					}
					return string.Format("{0}{1} - {2}", shortType, EpisodeNumber.Value, EpisodeName);
				}
				return FilePathFile1;
			}
		}

		public DuplicateFileVM(JMMServerBinary.Contract_DuplicateFile contract)
		{
			this.DateTimeUpdated = contract.DateTimeUpdated;
			this.DuplicateFileID = contract.DuplicateFileID;
			this.FilePathFile1 = contract.FilePathFile1;
			this.FilePathFile2 = contract.FilePathFile2;
			this.Hash = contract.Hash;
			this.ImportFolderIDFile1 = contract.ImportFolderIDFile1;
			this.ImportFolderIDFile2 = contract.ImportFolderIDFile2;

			this.AnimeID = contract.AnimeID;
			this.AnimeName = contract.AnimeName;
			this.EpisodeName = contract.EpisodeName;
			this.EpisodeNumber = contract.EpisodeNumber;
			this.EpisodeType = contract.EpisodeType;

			ImportFolder1 = new ImportFolderVM(contract.ImportFolder1);
			ImportFolder2 = new ImportFolderVM(contract.ImportFolder2);
		}

		public int CompareTo(DuplicateFileVM obj)
		{
			return AnimeName.CompareTo(obj.AnimeName);
		}
	}
}
