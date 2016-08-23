using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class PotVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {
        public override void Init()
        {
            PlayerPath = Utils.CheckSysPath(new string[] { "PotPlayerMini64.exe", "PotPlayerMini.exe" });
            if (string.IsNullOrEmpty(PlayerPath))
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PotPlayer64", "DisplayIcon", null);
            if (string.IsNullOrEmpty(PlayerPath))
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\PotPlayer", "DisplayIcon", null);
            if (string.IsNullOrEmpty(PlayerPath))
            {
                Active = false;
                return;
            }
            Active = true;
        }

        public VideoPlayer Player => VideoPlayer.PotPlayer;


        public override void Play(VideoInfo video)
        {
            Process process = Process.Start(PlayerPath, '"' + video.Uri + '"');
            if (process != null)
            {
                StartWatcher(AppSettings.PotPlayerFolder);
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

                    if (foundSectionStart
                        && line.Trim().ToLower().StartsWith("[")
                        && !line.ToLower().Contains("[rememberfiles]"))
                        foundSectionEnd = true;

                    if (foundSectionStart
                        && !foundSectionEnd
                        && !line.ToLower().Contains("[rememberfiles]")
                        && !string.IsNullOrEmpty(line))
                        allFiles.Add(i);
                }

                if (allFiles.Count == 0) return;

                Dictionary<string, long> filePositions = new Dictionary<string, long>();
                foreach (int lineNumber in allFiles)
                {
                    // find the last file played
                    string fileNameLine = lines[lineNumber];

                    int iPos1 = fileNameLine.IndexOf("=");
                    int iPos2 = fileNameLine.IndexOf("=", iPos1 + 1);

                    if (iPos1 <= 0 || iPos2 <= 0) continue;

                    string position = fileNameLine.Substring(iPos1 + 1, iPos2 - iPos1 - 1);
                    string fileName = fileNameLine.Substring(iPos2 + 1, fileNameLine.Length - iPos2 - 1);

                    long mpcPos = 0;
                    long.TryParse(position, out mpcPos);

                    // handle the case of PotPlayer having a psoition of 0, which means 100% watched
                    if (mpcPos == 0)
                        mpcPos = (long)100;

                    filePositions[fileName] = mpcPos;
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
