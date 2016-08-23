using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class VLCVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {
        public override void Init()
        {
            PlayerPath = Utils.CheckSysPath(new string[] { "vlc.exe" });
            if (string.IsNullOrEmpty(PlayerPath))
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VLC media player", "DisplayIcon", null);
            if (string.IsNullOrEmpty(PlayerPath))
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VLC media player", "DisplayIcon", null);
            if (string.IsNullOrEmpty(PlayerPath))
            {
                Active = false;
                return;
            }
            Active = true;
        }
        public VideoPlayer Player => VideoPlayer.VLC;
      
        public override void Play(VideoInfo video)
        {
            Process process;
            if (video.IsPlaylist)
                process=Process.Start(PlayerPath, '"' + video.Uri + '"');
            else
            {
                string init = '"' + video.Uri + '"';
                if (video.ResumePosition > 0)
                {
                    double n = video.ResumePosition;
                    n /= 1000;
                    init += " --start-time=\"" + n.ToString(CultureInfo.InvariantCulture) + "\"";
                }
                if (video.SubtitlePaths != null && video.SubtitlePaths.Count > 0)
                {
                    foreach (string s in video.SubtitlePaths)
                    {
                        init += " --sub-file=\"" + s + "\"";
                    }
                }
                process=Process.Start(PlayerPath, init);
            }
            if (process != null)
            {
                StartWatcher(AppSettings.VLCFolder);
                process.Exited += (a, b) =>
                {
                    StopWatcher();
                };
            }
        }

        internal override void FileChangeEvent(string filePath)
        {
            try
            {
                if (!AppSettings.VideoAutoSetWatched)
                    return;

                List<string> filePaths = new List<string>();
                List<long> positions = new List<long>();

                // Vlc ini file looks like
                //
                // ...
                // [RecentsMRL]
                // list=file://NAS/Anime/Gate%20Jieitai%20Kanochi%20nite%2C%20Kaku%20Tatakaeri/%5BHorribleSubs%5D%20GATE%20-%2007%20%5B720p%5D.mkv, file://NAS/Anime/Overlord/%5BHorribleSubs%5D%20OverLord%20-%2006%20%5B720p%5D.mkv
                // times=0, 0
                // ...
                //
                // and stores -1 for newly opened file, 0 for completed file, and a millisecond value for partial watched.
                // it appears to update on file open, playlist item change, or vlc close

                string[] lines = File.ReadAllLines(filePath);

                bool foundSectionStart = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.Equals("[RecentsMRL]", StringComparison.InvariantCultureIgnoreCase))
                        foundSectionStart = true;

                    if (foundSectionStart
                        && line.Trim().ToLower().StartsWith("list="))
                    {
                        filePaths.AddRange(
                            line.Remove(0, 5)
                                .Split(',')
                                .Select(
                                    f =>
                                    {
                                        Uri tmp = null;
                                        if (Uri.TryCreate(f.Trim(), UriKind.Absolute, out tmp))
                                            return tmp.LocalPath;
                                        else
                                            throw new UriFormatException("Unable to parse URI in VLC ini file");
                                    }
                                )
                            );
                    }

                    if (foundSectionStart
                        && line.Trim().ToLower().StartsWith("times="))
                    {
                        positions.AddRange(
                            line.Remove(0, 6)
                                .Split(',')
                                .Select(
                                    p =>
                                    {
                                        long posMs = 0;
                                        if (long.TryParse(p.Trim(), out posMs))
                                            return posMs;
                                        else
                                            throw new FormatException("Unable to parse times in VLC ini file");
                                    }
                                )
                            );

                        break;
                    }
                }

                if (filePaths.Count == 0)
                    return;

                if (filePaths.Count != positions.Count)
                    throw new Exception("The count of Recent Files and Timestamps in the VLC ini file differ");

                Dictionary<string, long> filePositions = new Dictionary<string, long>();

                for (int i = 0; i < filePaths.Count; i++)
                {
                    filePositions[filePaths[i]] = positions[i];
                }
                OnPositionChangeEvent(filePositions);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }
    }
}
