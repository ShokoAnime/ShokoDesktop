using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class VideoLocalVM : IComparable<VideoLocalVM>
	{
		public int VideoLocalID { get; set; }
		public string FilePath { get; set; }
		public int ImportFolderID { get; set; }
		public string Hash { get; set; }
		public string CRC32 { get; set; }
		public string MD5 { get; set; }
		public string SHA1 { get; set; }
		public int HashSource { get; set; }
		public long FileSize { get; set; }
		public int IsWatched { get; set; }
		public int IsIgnored { get; set; }
		public DateTime? WatchedDate { get; set; }
		public DateTime DateTimeUpdated { get; set; }

		public ImportFolderVM ImportFolder { get; set; }

		public string FullPath
		{
			get
			{
				if (AppSettings.ImportFolderMappings.ContainsKey(ImportFolderID))
					return Path.Combine(AppSettings.ImportFolderMappings[ImportFolderID], FilePath);
				else
					return Path.Combine(ImportFolder.ImportFolderLocation, FilePath);
			}
		}

		public string FileName
		{
			get
			{
				return Path.GetFileName(FullPath);
			}
		}

		public string FileDirectory
		{
			get
			{
				return Path.GetDirectoryName(FullPath);
			}
		}

		public string FormattedFileSize
		{
			get
			{
				return Utils.FormatFileSize(FileSize);
			}
		}

		public VideoLocalVM()
		{
		}

		public string ClosestAnimeMatchString
		{
			get
			{
				// if this file is manually linked to a series already
				// then use that main title
				List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(
					this.VideoLocalID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				if (eps.Count > 0)
				{
					AnimeEpisodeVM ep = new AnimeEpisodeVM(eps[0]);
					ep.RefreshAnime();
					return ep.AniDB_Anime.MainTitle;
				}

				string match = Path.GetFileNameWithoutExtension(this.FileName);

				//remove any group names or CRC's
				while (true)
				{
					int pos = match.IndexOf('[');
					if (pos >= 0)
					{
						int endPos = match.IndexOf(']', pos);
						if (endPos >= 0)
						{
							string rubbish = match.Substring(pos, endPos - pos + 1);
							match = match.Replace(rubbish, "");
						}
						else break;
					}
					else break;
				}

				//remove any video information
				while (true)
				{
					int pos = match.IndexOf('(');
					if (pos >= 0)
					{
						int endPos = match.IndexOf(')', pos);
						if (endPos >= 0)
						{
							string rubbish = match.Substring(pos, endPos - pos + 1);
							match = match.Replace(rubbish, "");
						}
						else break;
					}
					else break;
				}

				//if (match.Length < 16)
				//	return match;
				//else
				//	return match.Substring(0, 16);

				return match;
			}
		}

		public VideoLocalVM(JMMServerBinary.Contract_VideoLocal contract)
		{
			this.CRC32 = contract.CRC32;
			this.DateTimeUpdated = contract.DateTimeUpdated;
			this.FilePath = contract.FilePath;
			this.FileSize = contract.FileSize;
			this.Hash = contract.Hash;
			this.HashSource = contract.HashSource;
			this.ImportFolderID = contract.ImportFolderID;
			this.IsWatched = contract.IsWatched;
			this.IsIgnored = contract.IsIgnored;
			this.MD5 = contract.MD5;
			this.SHA1 = contract.SHA1;
			this.VideoLocalID = contract.VideoLocalID;
			this.WatchedDate = contract.WatchedDate;

			ImportFolder = new ImportFolderVM(contract.ImportFolder);
		}

		public int CompareTo(VideoLocalVM obj)
		{
			return FullPath.CompareTo(obj.FullPath);
		}

		public List<AnimeEpisodeVM> GetEpisodes()
		{
			List<AnimeEpisodeVM> eps = new List<AnimeEpisodeVM>();
			
			try
			{
				List<JMMServerBinary.Contract_AnimeEpisode> epContracts = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(this.VideoLocalID, 
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in epContracts)
				{
					AnimeEpisodeVM ep = new AnimeEpisodeVM(epcontract);
					eps.Add(ep);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			return eps;
		}
	}
}
