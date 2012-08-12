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
			return;

			recentlyPlayedFiles = new Dictionary<int, VideoDetailedVM>();

			FileSystemWatcher fsw = new FileSystemWatcher(@"C:\Program Files (x86)\Combined Community Codec Pack\MPC", "*.ini");
			fsw.IncludeSubdirectories = false;
			fsw.Changed += new FileSystemEventHandler(fsw_Changed);
			fsw.EnableRaisingEvents = true;
		}

		private void fsw_Changed(object sender, FileSystemEventArgs e)
		{
			Debug.WriteLine(e.FullPath);
			HandleFileChange(e.FullPath);
		}

		public void HandleFileChange(string filePath)
		{
			try
			{
				List<int> firstFiveFiles = new List<int>();

				string[] lines = File.ReadAllLines(filePath);

				// really we only need to check the first file, but will do this just in case

				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i];

					if (line.StartsWith("File Name 0=", StringComparison.InvariantCultureIgnoreCase)) firstFiveFiles.Add(i);
					if (line.StartsWith("File Name 1=", StringComparison.InvariantCultureIgnoreCase)) firstFiveFiles.Add(i);
					if (line.StartsWith("File Name 2=", StringComparison.InvariantCultureIgnoreCase)) firstFiveFiles.Add(i);
					if (line.StartsWith("File Name 3=", StringComparison.InvariantCultureIgnoreCase)) firstFiveFiles.Add(i);
					if (line.StartsWith("File Name 4=", StringComparison.InvariantCultureIgnoreCase)) firstFiveFiles.Add(i);

					if (firstFiveFiles.Count == 5) break;
				}

				if (firstFiveFiles.Count == 0) return;

				// find the last file played
				string fileNameLine = lines[firstFiveFiles[0]];
				string filePosLine = lines[firstFiveFiles[0] + 1];

				string nameStart = "File Name 0=";
				string posStart = "File Position 0=";

				string fileName = fileNameLine.Substring(nameStart.Length, fileNameLine.Length - nameStart.Length);
				string position = filePosLine.Substring(posStart.Length, filePosLine.Length - posStart.Length);

				if (lastFileNameProcessed.Equals(fileName, StringComparison.InvariantCultureIgnoreCase) &&
					lastPositionProcessed.Equals(position, StringComparison.InvariantCultureIgnoreCase))
				{
					// return if the last file we looked at hasn't changed
					return;
				}

				lastFileNameProcessed = fileName;
				lastPositionProcessed = position;

				long mpcPos = 0;
				long.TryParse(position, out mpcPos);
				if (mpcPos <= 0) return;

				// MPC position is in micro-seconds
				// convert to milli-seconds
				double mpcPosMS = (double)mpcPos / (double)10000;

				foreach (KeyValuePair<int, VideoDetailedVM> kvp in recentlyPlayedFiles)
				{
					if (kvp.Value.FullPath.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						// we don't care about files that are already watched
						if (kvp.Value.Watched) continue;

						// now check if this file is considered watched
						double fileDurationMS = (double)kvp.Value.VideoInfo_Duration;

						double progress = mpcPosMS / fileDurationMS * (double)100;
						if (progress > 85)
						{
							VideoDetailedVM vid = kvp.Value;

							JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, true, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
							MainListHelperVM.Instance.UpdateHeirarchy(vid);

							//kvp.Value.VideoLocal_IsWatched = 1;
							OnVideoWatchedEvent(new VideoWatchedEventArgs(vid.VideoLocalID, vid));
							
							Debug.WriteLine("complete");
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
