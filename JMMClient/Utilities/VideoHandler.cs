using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using NLog;
using JMMClient.ViewModel;

namespace JMMClient.Utilities
{
	public class VideoHandler
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private Dictionary<int, VideoDetailedVM> recentlyPlayedFiles = null;
		private string lastFileNameProcessed = string.Empty;
		private string lastPositionProcessed = string.Empty;
		private System.Timers.Timer handleTimer = null;
		private string iniPath = string.Empty;

		private List<FileSystemWatcher> watcherVids = null;
		Dictionary<string, string> previousFilePositions = new Dictionary<string, string>();

		public delegate void VideoWatchedEventHandler(VideoWatchedEventArgs ev);
		public event VideoWatchedEventHandler VideoWatchedEvent;
		protected void OnVideoWatchedEvent(VideoWatchedEventArgs ev)
		{
			if (VideoWatchedEvent != null)
			{
				VideoWatchedEvent(ev);
			}
		}

		public void PlayVideo(VideoDetailedVM vid)
		{
			try
			{
				recentlyPlayedFiles[vid.VideoLocalID] = vid;
				Process.Start(new ProcessStartInfo(vid.FullPath));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void PlayVideo(VideoLocalVM vid)
		{
			try
			{
				Process.Start(new ProcessStartInfo(vid.FullPath));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void Init()
		{
			try
			{
				recentlyPlayedFiles = new Dictionary<int, VideoDetailedVM>();
				previousFilePositions.Clear();

				if (!AppSettings.VideoAutoSetWatched) return;

				StopWatchingFiles();
				watcherVids = new List<FileSystemWatcher>();

				if (!string.IsNullOrEmpty(AppSettings.MPCFolder) && Directory.Exists(AppSettings.MPCFolder))
				{
					FileSystemWatcher fsw = new FileSystemWatcher(AppSettings.MPCFolder, "*.ini");
					fsw.IncludeSubdirectories = false;
					fsw.Changed += new FileSystemEventHandler(fsw_Changed);
					fsw.EnableRaisingEvents = true;
				}

				if (!string.IsNullOrEmpty(AppSettings.PotPlayerFolder) && Directory.Exists(AppSettings.PotPlayerFolder))
				{
					FileSystemWatcher fsw = new FileSystemWatcher(AppSettings.PotPlayerFolder, "*.ini");
					fsw.IncludeSubdirectories = false;
					fsw.Changed += new FileSystemEventHandler(fsw_Changed);
					fsw.EnableRaisingEvents = true;
				}

			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		public void StopWatchingFiles()
		{
			if (watcherVids == null) return;

			foreach (FileSystemWatcher fsw in watcherVids)
			{
				fsw.EnableRaisingEvents = false;
			}
		}

		private void fsw_Changed(object sender, FileSystemEventArgs e)
		{
			
			// delay by 200ms since MPC will update the file multiple times in quick succession
			// and also the delay allows us access to the file
			iniPath = e.FullPath;

			if (handleTimer != null)
				handleTimer.Stop();

			handleTimer = new System.Timers.Timer();
			handleTimer.AutoReset = false;
			handleTimer.Interval = 200; // 200 ms
			handleTimer.Elapsed += new System.Timers.ElapsedEventHandler(handleTimer_Elapsed);
			handleTimer.Enabled = true;
		}

		void handleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
			{
				FileInfo fi = new FileInfo(iniPath);
				if (fi.DirectoryName.Equals(AppSettings.MPCFolder, StringComparison.InvariantCultureIgnoreCase))
					HandleFileChangeMPC(iniPath);

				if (fi.DirectoryName.Equals(AppSettings.PotPlayerFolder, StringComparison.InvariantCultureIgnoreCase))
					HandleFileChangePotPlayer(iniPath);
			});
		}

		public void HandleFileChangePotPlayer(string filePath)
		{
			try
			{
				if (!AppSettings.VideoAutoSetWatched) return;

				List<int> allFiles = new List<int>();

				string[] lines = File.ReadAllLines(filePath);

				bool foundSectionStart = false;
				bool foundSectionEnd = false;

				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i];

					if (line.ToLower().Contains("[rememberfiles]"))
						foundSectionStart = true;

					if (foundSectionStart && line.Trim().ToLower().StartsWith("[") && !line.ToLower().Contains("[rememberfiles]"))
						foundSectionEnd = true;

					if (foundSectionStart && !foundSectionEnd)
					{
						if (!line.ToLower().Contains("[rememberfiles]") && !string.IsNullOrEmpty(line)) allFiles.Add(i);
					}

				}

				if (allFiles.Count == 0) return;

				Dictionary<string, string> filePositions = new Dictionary<string, string>();
				foreach (int lineNumber in allFiles)
				{
					// find the last file played
					string fileNameLine = lines[lineNumber];

					int iPos1 = fileNameLine.IndexOf("=");
					int iPos2 = fileNameLine.IndexOf("=", iPos1 + 1);

					if (iPos1 <= 0 || iPos2 <= 0) continue;

					string position = fileNameLine.Substring(iPos1 + 1, iPos2 - iPos1 - 1);
					string fileName = fileNameLine.Substring(iPos2 + 1, fileNameLine.Length - iPos2 - 1);


					filePositions[fileName] = position;
				}

				// find all the files which have changed
				Dictionary<string, string> changedFilePositions = new Dictionary<string, string>();

				foreach (KeyValuePair<string, string> kvp in filePositions)
				{
					changedFilePositions[kvp.Key] = kvp.Value;
				}

				// update the changed positions
				foreach (KeyValuePair<string, string> kvp in changedFilePositions)
				{
					previousFilePositions[kvp.Key] = kvp.Value;
				}

				foreach (KeyValuePair<string, string> kvp in changedFilePositions)
				{
					lastFileNameProcessed = kvp.Key;
					lastPositionProcessed = kvp.Value;

					long mpcPos = 0;
					if (!long.TryParse(kvp.Value, out mpcPos)) continue;

					// if mpcPos == 0, it means that file has finished played completely

					// MPC position is in micro-seconds
					// convert to milli-seconds
					//double mpcPosMS = (double)mpcPos / (double)10000;
					double mpcPosMS = (double)mpcPos;

					foreach (KeyValuePair<int, VideoDetailedVM> kvpVid in recentlyPlayedFiles)
					{
						if (kvpVid.Value.FullPath.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase))
						{
							// we don't care about files that are already watched
							if (kvpVid.Value.Watched) continue;

							logger.Info(string.Format("Video position for {0} has changed to {1}", kvp.Key, kvp.Value));

							// now check if this file is considered watched
							double fileDurationMS = (double)kvpVid.Value.VideoInfo_Duration;

							double progress = mpcPosMS / fileDurationMS * (double)100;
							if (progress > (double)AppSettings.VideoWatchedPct || mpcPos == 0)
							{
								VideoDetailedVM vid = kvpVid.Value;

								logger.Info(string.Format("Updating to watched from MPC: {0}", kvp.Key));

								JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, true, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
								MainListHelperVM.Instance.UpdateHeirarchy(vid);
								MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);

								//kvp.Value.VideoLocal_IsWatched = 1;
								OnVideoWatchedEvent(new VideoWatchedEventArgs(vid.VideoLocalID, vid));

								Debug.WriteLine("complete");
							}

						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		public void HandleFileChangeMPC(string filePath)
		{
			try
			{
				if (!AppSettings.VideoAutoSetWatched) return;

				List<int> allFiles = new List<int>();

				string[] lines = File.ReadAllLines(filePath);

				// really we only need to check the first file, but will do this just in case
				string prefix = string.Format("File Name ");
				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i];

					if (line.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)) allFiles.Add(i);

					if (allFiles.Count == 20) break;
				}

				if (allFiles.Count == 0) return;

				Dictionary<string, string> filePositions = new Dictionary<string, string>();
				foreach (int lineNumber in allFiles)
				{
					// find the last file played
					string fileNameLine = lines[lineNumber];
					string filePosLine = lines[lineNumber + 1];

					int iPos1 = fileNameLine.IndexOf("=");
					int iPos2 = filePosLine.IndexOf("=");

					string fileName = fileNameLine.Substring(iPos1 + 1, fileNameLine.Length - iPos1 - 1);
					string position = filePosLine.Substring(iPos2 + 1, filePosLine.Length - iPos2 - 1);

					filePositions[fileName] = position;
				}

				// find all the files which have changed
				Dictionary<string, string> changedFilePositions = new Dictionary<string, string>();
				/*foreach (KeyValuePair<string, string> kvp in filePositions)
				{
					if (previousFilePositions.ContainsKey(kvp.Key))
					{
						if (!previousFilePositions[kvp.Key].Equals(kvp.Value))
							changedFilePositions[kvp.Key] = kvp.Value;
					}
					else
						changedFilePositions[kvp.Key] = kvp.Value;
				}*/

				foreach (KeyValuePair<string, string> kvp in filePositions)
				{
					changedFilePositions[kvp.Key] = kvp.Value;
				}

				// update the changed positions
				foreach (KeyValuePair<string, string> kvp in changedFilePositions)
				{
					previousFilePositions[kvp.Key] = kvp.Value;
				}

				foreach (KeyValuePair<string, string> kvp in changedFilePositions)
				{
					lastFileNameProcessed = kvp.Key;
					lastPositionProcessed = kvp.Value;

					long mpcPos = 0;
					if (!long.TryParse(kvp.Value, out mpcPos)) continue;

					// if mpcPos == 0, it means that file has finished played completely

					// MPC position is in micro-seconds
					// convert to milli-seconds
					double mpcPosMS = (double)mpcPos / (double)10000;

					foreach (KeyValuePair<int, VideoDetailedVM> kvpVid in recentlyPlayedFiles)
					{
						if (kvpVid.Value.FullPath.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase))
						{
							// we don't care about files that are already watched
							if (kvpVid.Value.Watched) continue;

							logger.Info(string.Format("Video position for {0} has changed to {1}", kvp.Key, kvp.Value));

							// now check if this file is considered watched
							double fileDurationMS = (double)kvpVid.Value.VideoInfo_Duration;

							double progress = mpcPosMS / fileDurationMS * (double)100;
							if (progress > (double)AppSettings.VideoWatchedPct || mpcPos == 0)
							{
								VideoDetailedVM vid = kvpVid.Value;

								logger.Info(string.Format("Updating to watched from MPC: {0}", kvp.Key));

								JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, true, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
								MainListHelperVM.Instance.UpdateHeirarchy(vid);
								MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);

								//kvp.Value.VideoLocal_IsWatched = 1;
								OnVideoWatchedEvent(new VideoWatchedEventArgs(vid.VideoLocalID, vid));

								Debug.WriteLine("complete");
							}

						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		public void PlayAllUnwatchedEpisodes(int animeSeriesID)
		{
			try
			{
				List<JMMServerBinary.Contract_AnimeEpisode> rawEps = JMMServerVM.Instance.clientBinaryHTTP.GetAllUnwatchedEpisodes(animeSeriesID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				List<AnimeEpisodeVM> episodes = new List<AnimeEpisodeVM>();
				foreach (JMMServerBinary.Contract_AnimeEpisode raw in rawEps)
					episodes.Add(new AnimeEpisodeVM(raw));


				string plsPath = GenerateTemporaryPlayList(episodes);
				if (!string.IsNullOrEmpty(plsPath))
					Process.Start(plsPath);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex.Message, ex);
			}
		}

		public void PlayEpisodes(List<AnimeEpisodeVM> episodes)
		{
			try
			{
				string plsPath = GenerateTemporaryPlayList(episodes);
				if (!string.IsNullOrEmpty(plsPath))
					Process.Start(plsPath);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex.Message, ex);
			}
		}

		public string GenerateTemporaryPlayList(List<AnimeEpisodeVM> episodes)
		{
			try
			{
				List<VideoDetailedVM> vids = new List<VideoDetailedVM>();
				foreach (AnimeEpisodeVM ep in episodes)
				{
					if (ep.FilesForEpisode.Count > 0)
					{
						VideoDetailedVM vid = GetAutoFileForEpisode(ep);
						if (vid != null)
						{
							vids.Add(vid);
							recentlyPlayedFiles[vid.VideoLocalID] = vid;
						}
					}
				}
				return GenerateTemporaryPlayList(vids);
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}

			return string.Empty;
		}

		public string GenerateTemporaryPlayList(List<VideoDetailedVM> vids)
		{
			try
			{
				// get a temporary file
				string filePath = Utils.GetTempFilePathWithExtension(".pls");

				string plsContent = "";

				plsContent += @"[playlist]" + Environment.NewLine;

				for (int i=1; i<=vids.Count; i++)
					plsContent += string.Format(@"File{0}={1}", i, vids[i-1].FullPath) + Environment.NewLine;

				plsContent += @"NumberOfEntries=" + vids.Count.ToString() + Environment.NewLine;
				plsContent += @"Version=2" + Environment.NewLine;

				File.WriteAllText(filePath, plsContent);

				return filePath;
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}

			return string.Empty;
		}

		public VideoDetailedVM GetAutoFileForEpisode(AnimeEpisodeVM ep)
		{
			try
			{
				if (ep.FilesForEpisode == null) return null;
				if (ep.FilesForEpisode.Count == 1) return ep.FilesForEpisode[0];

				// find the previous episode
				JMMServerBinary.Contract_AnimeEpisode raw = JMMServerVM.Instance.clientBinaryHTTP.GetPreviousEpisodeForUnwatched(ep.AnimeSeriesID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				
				if (raw == null)
				{
					List<VideoDetailedVM> vids = ep.FilesForEpisode;
					// just use the best quality file
					List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
					sortCriteria.Add(new SortPropOrFieldAndDirection("OverallVideoSourceRanking", true, SortType.eInteger));
					vids = Sorting.MultiSort<VideoDetailedVM>(vids, sortCriteria);

					return vids[0];
				}
				else
				{
					List<VideoDetailedVM> vids = ep.FilesForEpisode;
					
					// sort by quality
					List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
					sortCriteria.Add(new SortPropOrFieldAndDirection("OverallVideoSourceRanking", true, SortType.eInteger));
					vids = Sorting.MultiSort<VideoDetailedVM>(vids, sortCriteria);

					if (AppSettings.AutoFileSubsequent == (int)AutoFileSubsequentType.BestQuality)
					{
						// just use the best quality file
						return vids[0];
					}
					else
					{
						// otherwise look at which groups files they watched previously
						AnimeEpisodeVM previousEp = new AnimeEpisodeVM(raw);
						List<VideoDetailedVM> vidsPrevious = previousEp.FilesForEpisode;

						foreach (VideoDetailedVM vidPrev in vidsPrevious)
						{
							if (vidPrev.Watched)
							{
								foreach (VideoDetailedVM vid in vids)
								{
									if (vid.AniDB_Anime_GroupName.Equals(vidPrev.AniDB_Anime_GroupName, StringComparison.InvariantCultureIgnoreCase))
									{
										return vid;
									}
								}
							}
						}

						// if none played??? use the best quality
						return vids[0];
					}
				}
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}

			return null;
		}
	}

	public class VideoWatchedEventArgs : EventArgs
	{
		public readonly int VideoLocalID = 0;
		public readonly VideoDetailedVM VideoLocal = null;

		public VideoWatchedEventArgs(int videoLocalID, VideoDetailedVM vid)
		{
			this.VideoLocalID = videoLocalID;
			this.VideoLocal = vid;
		}
	}
}
