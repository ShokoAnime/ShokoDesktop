using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class VLCVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {
        private System.Timers.Timer playerWebUiTimer = null;
        private long currentPosition;

        private static string webUIPort = "9001";
        private static string webUIPassword = "AnimePlayer";

        public override void Init()
        {
            PlayerPath = Utils.CheckSysPath(new string[] {"vlc.exe"});
            if (string.IsNullOrEmpty(PlayerPath) && !File.Exists(PlayerPath))
                PlayerPath =
                    (string)
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VLC media player",
                            "DisplayIcon", null);
            if (string.IsNullOrEmpty(PlayerPath) && !File.Exists(PlayerPath))
                PlayerPath =
                    (string)
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VLC media player",
                            "DisplayIcon", null);

            if (string.IsNullOrEmpty(PlayerPath) && File.Exists(PlayerPath))
            {
                Active = false;
                return;
            }
            Active = true;
        }

        public VideoPlayer Player => VideoPlayer.VLC;

        public override void Play(VideoInfo video)
        {
            if (IsPlaying)
                return;
            Task.Factory.StartNew(() =>
            {
                Process process;
                // Parameters for web ui
                string webUIParams = $" --http-host=localhost --http-port={webUIPort} --http-password={webUIPassword} ";

                if (video.IsPlaylist)
                    process = Process.Start(PlayerPath, $"\"{video.Uri}\" {webUIParams}");
                else
                {
                    string init = $"\"{video.Uri}\" {webUIParams}";
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
                    process = Process.Start(PlayerPath, init);
                }
                if (process != null)
                {
                    IsPlaying = true;
                    StartWatcher("");
                    process.WaitForExit();
                    StopWatcher();
                    IsPlaying = false;
                    if (video != null)
                        BaseVideoPlayer.PlaybackStopped(video, (long) currentPosition);
                }
            });
        }

        internal override void StartWatcher(string dummy)
        {
            StopWatcher();
            playerWebUiTimer = new System.Timers.Timer();
            playerWebUiTimer.Elapsed += HandleWebUIRequest;
            playerWebUiTimer.Interval = 2500;
            playerWebUiTimer.Enabled = true;

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

        internal override void FileChangeEvent(string filePath)
        {
            // No longer used
        }

        // Make and handle VLC web UI request
        private async void HandleWebUIRequest(object source, System.Timers.ElapsedEventArgs e)
        {
            // Stop timer for the time request is processed
            playerWebUiTimer.Stop();

            // Request
            string VLCUIStatusUrl = string.Format("http://localhost:{0}/requests/status.xml", webUIPort);

            // Helper variables
            string responseString = "";
            string nowPlayingFile;
            string nowPlayingFilePosition = "";
            string nowPlayingFileDuration = "";

            // Regex for extracting relevant information
            // Could do it properly with XML reader but this keeps it consistent with other players
            Regex fileRegex = new Regex("info name=\"filename\">(.*?)</info>");
            Regex filePositionRegex = new Regex("time>(.*?)</time>");
            Regex fileDurationRegex = new Regex("length>(.*?)</length>");

            try
            {
                // Make HTTP request to Web UI
                HttpClient client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes(":" + webUIPassword);
                var header = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Authorization = header;

                using (HttpResponseMessage response = await client.GetAsync(VLCUIStatusUrl))
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
                            // Extract currently playing video informations
                            nowPlayingFile = await GetNowPlayingFile();
                            if (!string.IsNullOrEmpty(nowPlayingFile))
                            {
                                nowPlayingFilePosition = filePositionRegex.Match(responseString).Groups[1].ToString();
                                nowPlayingFileDuration = fileDurationRegex.Match(responseString).Groups[1].ToString();
                                // Parse number values for future aritmetics

                                double webPosition;
                                double webDuration;
                                bool isDoublePosition = double.TryParse(nowPlayingFilePosition, out webPosition);
                                bool isDoubleDuration = double.TryParse(nowPlayingFilePosition, out webDuration);

                                double filePosition;
                                double fileDuration;
                                if (isDoublePosition)
                                {
                                    // Convert miliseconds to seconds
                                    filePosition = webPosition*1000;
                                    if (isDoubleDuration)
                                        fileDuration = webDuration*1000;

                                    Dictionary<string, long> pos = new Dictionary<string, long>();
                                    pos.Add(nowPlayingFile, (long) filePosition);
                                    OnPositionChangeEvent(pos);
                                    currentPosition = (long) filePosition;
                                }
                            }
                        }
                    }
                    // Start timer again
                    playerWebUiTimer?.Start();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
                playerWebUiTimer?.Start();
            }
        }

        private async Task<string> GetNowPlayingFile()
        {
            string filename = "";
            string VLCUIPlaylistUrl = string.Format("http://localhost:{0}/requests/playlist.xml", webUIPort);

            // Regex for extracting relevant information
            // Could do it properly with XML reader but this keeps it consistent with other players
            Regex fileRegex = new Regex("<leaf*.*uri=\"(.*?)\" current=\"current\"\\/>");

            try
            {
                // Make HTTP request to Web UI
                HttpClient client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes(":" + webUIPassword);
                var header = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Authorization = header;

                using (HttpResponseMessage response = await client.GetAsync(VLCUIPlaylistUrl))
                using (HttpContent content = response.Content)
                {
                    // Check if request was ok
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Read the string
                        string responseString = await content.ReadAsStringAsync();
                        // Parse result
                        if (responseString != null)
                        {
                            // Extract filename and url decode so it matches
                            filename = HttpUtility.UrlDecode(fileRegex.Match(responseString).Groups[1].ToString());
                            filename = filename.Replace("file:", string.Empty).Replace("/", "\\").Trim();
                        }
                    }
                }
            }
            catch (Exception ex){
                logger.ErrorException(ex.ToString(), ex);
            }

            return filename;
        }
    }
}
