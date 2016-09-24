using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class MPCVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {

        private System.Timers.Timer playerWebUiTimer = null;
        private long currentPosition;

        public override void Play(VideoInfo video)
        {
            if (IsPlaying)
                return;
            Task.Factory.StartNew(() =>
            {
                Process process;
                if (video.IsPlaylist)
                    process = Process.Start(PlayerPath, '"' + video.Uri + '"');
                else
                {
                    string init = '"' + video.Uri + '"';
                    if (video.ResumePosition > 0)
                        init += " /start " + video.ResumePosition;
                    if (video.SubtitlePaths != null && video.SubtitlePaths.Count > 0)
                    {
                        foreach (string s in video.SubtitlePaths)
                        {
                            init += " /sub \"" + s + "\"";
                        }
                    }
                    process = Process.Start(PlayerPath, init);
                }
                if (process != null)
                {
                    IsPlaying = true;
                    StartWatcher(AppSettings.MPCFolder);
                    process.WaitForExit();
                    StopWatcher();
                    IsPlaying = false;
                    if (video != null)
                        BaseVideoPlayer.PlaybackStopped(video, (long)currentPosition);
                }
            });

        }


        public override void Init()
        {
            string[] playersexenames =
            {
                "mpc-hc64.exe",
                "mpc-hc64_nvo.exe",
                "mpc-64.exe",
                "mpc-hc.exe",
                "mpc-hc_nvo.exe",
                "mpc.exe",
                "mpc-be64.exe", 
                "mpc-be.exe"
            }; //Prefer 64 Bit nvo is nvidia optimus

            string[] registryplaces = 
            {
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Combined Community Codec Pack 64bit_is1",
                "InstallLocation",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Combined Community Codec Pack_is1",
                "InstallLocation",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\KLiteCodecPack_is1",
                "InstallLocation",
            };
            string[] installregplaces =
            {
                @"HKEY_CURRENT_USER\SOFTWARE\MPC-HC\MPC-HC",
                @"HKEY_CURRENT_USER\SOFTWARE\MPC-HC64\MPC-HC",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\MPC-HC",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\MPC-BE",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\MPC-BE",

            };
            string[] subdirs=new string[]
            {
                "",
                "MPC",
                "MPC-HC64",
                "MPC-HC",
                "MPC-BE64",
                "MPC-BE",
            };
            PlayerPath = Utils.CheckSysPath(playersexenames);
            if (string.IsNullOrEmpty(PlayerPath))
            {
                for (int x = 0; x < registryplaces.Length; x += 2)
                {
                    string path= (string)Registry.GetValue(registryplaces[x],registryplaces[x+1], null);
                    if (!string.IsNullOrEmpty(path))
                    {
                        foreach (string subdir in subdirs)
                        {
                            string subdirpath = (!string.IsNullOrEmpty(subdir)) ? Path.Combine(path, subdir) : path;
                            foreach (string pname in playersexenames)
                            {
                                string npath = Path.Combine(subdirpath, pname);
                                if (File.Exists(npath))
                                {
                                    PlayerPath = npath;
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(PlayerPath))
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(PlayerPath))
                        break;
                }
            }
            if (string.IsNullOrEmpty(PlayerPath))
            {
            	foreach(string r in installregplaces)
                {
                    PlayerPath = (string)Registry.GetValue(r, "ExePath", null);
                    if (!string.IsNullOrEmpty(PlayerPath))
                        break;
                }
            }
            if (string.IsNullOrEmpty(PlayerPath))
            {
                Active = false;
                return;
            }
            Active = true;

        }
        internal override void StartWatcher(string path)
        {


            StopWatcher();
            if (AppSettings.MPCIniIntegration)
                base.StartWatcher(path);
            if (AppSettings.MPCWebUiIntegration)
            {
                playerWebUiTimer = new System.Timers.Timer();
                playerWebUiTimer.Elapsed += HandleWebUIRequest;
                playerWebUiTimer.Interval = 1000;
                playerWebUiTimer.Enabled = true;
            }
        }

        internal override void StopWatcher()
        {
            if (playerWebUiTimer != null)
            {
                playerWebUiTimer.Stop();
                playerWebUiTimer.Dispose();
                playerWebUiTimer = null;
            }
            base.StopWatcher();
        }

        public VideoPlayer Player => VideoPlayer.MPC;

        internal override void FileChangeEvent(string filePath)
        {
            try
            {
                if (!AppSettings.VideoAutoSetWatched) return;

                List<int> allFilesHeaders = new List<int>();
                List<int> allFilesPositions = new List<int>();

                string[] lines = File.ReadAllLines(filePath);

                // really we only need to check the first file, but will do this just in case

                // MPC 1.3 and lower looks like this
                // File Name 0=M:\[ Anime to Watch - New ]\Arata Kangatari\[HorribleSubs] Arata Kangatari - 05 [720p].mkv
                // File Position 0=14251233493
                // File Name 1=M:\[ Anime to Watch - New ]\Hentai Ouji to Warawanai Neko\[gg]_Hentai_Ouji_to_Warawanai_Neko_-_04_[62E1DBF8].mkv
                // File Position 1=13688612500

                // MPC 1.6 and lower looks like this
                // File Name 0=M:\[ Anime to Watch - New ]\Arata Kangatari\[HorribleSubs] Arata Kangatari - 05 [720p].mkv
                // File Name 1=M:\[ Anime to Watch - New ]\Hentai Ouji to Warawanai Neko\[gg]_Hentai_Ouji_to_Warawanai_Neko_-_04_[62E1DBF8].mkv
                // File Position 0=14251233493
                // File Position 1=13688612500

                // get file name header lines
                string prefixHeader = string.Format("File Name ");
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.StartsWith(prefixHeader, StringComparison.InvariantCultureIgnoreCase)) allFilesHeaders.Add(i);

                    if (allFilesHeaders.Count == 20) break;
                }

                if (allFilesHeaders.Count == 0) return;

                string prefixPos = string.Format("File Position ");
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.StartsWith(prefixPos, StringComparison.InvariantCultureIgnoreCase)) allFilesPositions.Add(i);

                    if (allFilesPositions.Count == 20) break;
                }

                Dictionary<string, long> filePositions = new Dictionary<string, long>();
                foreach (int lineNumber in allFilesHeaders)
                {
                    // find the last file played
                    string fileNameLine = lines[lineNumber];

                    // find the number of this file
                    string temp = fileNameLine.Trim().Replace(prefixHeader, "");
                    int iPosTemp = temp.IndexOf("=");
                    if (iPosTemp < 0) continue;

                    string fileNumber = temp.Substring(0, iPosTemp);

                    // now find the match play position line
                    string properPosLine = string.Empty;
                    foreach (int lineNumberPos in allFilesPositions)
                    {
                        string filePosLineTemp = lines[lineNumberPos];

                        // find the number of this file
                        string temp2 = filePosLineTemp.Trim().Replace(prefixPos, "");
                        int iPosTemp2 = temp2.IndexOf("=");
                        if (iPosTemp2 < 0) continue;

                        string fileNumber2 = temp2.Substring(0, iPosTemp2);
                        if (fileNumber.Equals(fileNumber2, StringComparison.InvariantCultureIgnoreCase))
                        {
                            properPosLine = filePosLineTemp;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(properPosLine)) continue;

                    // extract the file name and position
                    int iPos1 = fileNameLine.IndexOf("=");
                    if (iPos1 < 0) continue;

                    int iPos2 = properPosLine.IndexOf("=");

                    string fileName = fileNameLine.Substring(iPos1 + 1, fileNameLine.Length - iPos1 - 1);
                    string position = properPosLine.Substring(iPos2 + 1, properPosLine.Length - iPos2 - 1);

                    long mpcPos = 0;
                    long.TryParse(position, out mpcPos);

                    // if mpcPos == 0, it means that file either finished playing or have been stopped or re/started
                    // please note that mpc does not trigger *.ini file update at all times or at least periodically
                    // manual change of file position via clicks modify the file as well as starting playback and swiching file
                    // using arrows to navigate forward and backwards however do not as well regular playback

                    // MPC position is in 10^-7 s		                      
                    // convert to milli-seconds (10^-3 s)

                    // dont worry about remainder, as we're checking against 1 s precision later anyway
                    filePositions[fileName] = mpcPos / 10000;
                    currentPosition = mpcPos / 10000;
                }
                OnPositionChangeEvent(filePositions);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }

        // Make and handle MPC-HC Web UI request
        private async void HandleWebUIRequest(object source, System.Timers.ElapsedEventArgs e)
        {
            // Stop timer for the time request is processed
            playerWebUiTimer.Stop();
            // Request
            string mpcUIFullUrl = "http://" + AppSettings.MPCWebUIUrl + ":" + AppSettings.MPCWebUIPort + "/variables.html";
            // Helper variables
            string responseString = "";
            string nowPlayingFile = "";
            string nowPlayingFilePosition = "";
            string nowPlayingFileDuration = "";
            // Regex for extracting relevant information
            Regex fileRegex = new Regex("<p id=\"filepath\">(.*?)</p>");
            Regex filePositionRegex = new Regex("<p id=\"position\">(.*?)</p>");
            Regex fileDurationRegex = new Regex("<p id=\"duration\">(.*?)</p>");

            try
            {
                // Make HTTP request to Web UI
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(mpcUIFullUrl))
                using (HttpContent content = response.Content)
                {
                    // Check if request was ok
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Read the string
                        responseString = await content.ReadAsStringAsync();
                        // Parse result
                        if (responseString != null)
                        {
                            // extract currently playing video informations
                            nowPlayingFile = fileRegex.Match(responseString).Groups[1].ToString();
                            nowPlayingFilePosition = filePositionRegex.Match(responseString).Groups[1].ToString();
                            nowPlayingFileDuration = fileDurationRegex.Match(responseString).Groups[1].ToString();
                            // Parse number values for future aritmetics
                            double filePosition;
                            double fileDuration;
                            Double.TryParse(nowPlayingFilePosition, out filePosition);

                            Dictionary<string, long> pos=new Dictionary<string, long>();
                            pos.Add(nowPlayingFile,(long)filePosition);
                            OnPositionChangeEvent(pos);
                            currentPosition = (long)filePosition;
                        }
                    }
                    // Start timer again
                    playerWebUiTimer?.Start();
                }
            }
            catch (Exception exception)
            {
                logger.ErrorException(exception.ToString(), exception);
                playerWebUiTimer?.Start();
            }
        }
    }
}
