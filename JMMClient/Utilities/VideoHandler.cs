using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using NLog;

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

		FileSystemWatcher fsw = null;
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

		public void Init()
		{
			try
			{
				recentlyPlayedFiles = new Dictionary<int, VideoDetailedVM>();
				previousFilePositions.Clear();

				if (!AppSettings.VideoAutoSetWatched) return;
				if (string.IsNullOrEmpty(AppSettings.MPCFolder)) return;
				if (!Directory.Exists(AppSettings.MPCFolder)) return;


				if (fsw != null)
					fsw.Dispose();

				fsw = new FileSystemWatcher(AppSettings.MPCFolder, "*.ini");
				fsw.IncludeSubdirectories = false;
				fsw.Changed += new FileSystemEventHandler(fsw_Changed);
				fsw.EnableRaisingEvents = true;
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
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
				HandleFileChange(iniPath);
			});
		}

		public void HandleFileChange(string filePath)
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
				foreach (KeyValuePair<string, string> kvp in filePositions)
				{
					if (previousFilePositions.ContainsKey(kvp.Key))
					{
						if (!previousFilePositions[kvp.Key].Equals(kvp.Value))
							changedFilePositions[kvp.Key] = kvp.Value;
					}
					else
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
					long.TryParse(kvp.Value, out mpcPos);
					if (mpcPos <= 0) continue;

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
							if (progress > (double)AppSettings.VideoWatchedPct)
							{
								VideoDetailedVM vid = kvpVid.Value;

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
