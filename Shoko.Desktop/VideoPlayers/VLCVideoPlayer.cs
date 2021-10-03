using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Win32;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Utilities;

namespace Shoko.Desktop.VideoPlayers
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
            {
                AddFileToQueue(video);
                return;
            }
            Task.Factory.StartNew(() =>
            {
                Process process;
                // Parameters for web ui
                string webUIParams = $" --http-host=localhost --http-port={webUIPort} --http-password={webUIPassword} ";

                if (video.IsPlaylist)
                    process = Process.Start(PlayerPath, $"\"{video.Uri}\" {webUIParams}");
                else
                {
                    string init = $" {webUIParams}";
                    process = Process.Start(PlayerPath, init);
                    if (video.ResumePosition > 0)
                    {
                        SeekTo(video);
                    }
                }
                if (process != null)
                {
                    IsPlaying = true;
                    AddFileToQueue(video);
                    Thread.Sleep(1000);
                    PlayVideo();
                    if (video.ResumePosition > 0) SeekTo(video);
                    StartWatcher("");
                    process.WaitForExit();
                    StopWatcher();
                    IsPlaying = false;
                    if (video != null)
                        PlaybackStopped(video, (long) currentPosition);
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

        private async void AddFileToQueue(VideoInfo video)
        {
            string videoPath = video.Uri;
            int startTime = (int) video.ResumePosition;
            string vlcEnqueueUrl =
                $"http://localhost:{webUIPort}/requests/status.xml?command=in_enqueue&input=file:///{videoPath}";
            try
            {
                // Make HTTP request to Web UI
                HttpClient client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes(":" + webUIPassword);
                var header = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Authorization = header;
                await client.GetAsync(vlcEnqueueUrl);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        private async void PlayVideo()
        {

            string vlcEnqueueUrl =
                $"http://localhost:{webUIPort}/requests/status.xml?command=pl_play";
            try
            {
                // Make HTTP request to Web UI
                HttpClient client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes(":" + webUIPassword);
                var header = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Authorization = header;
                await client.GetAsync(vlcEnqueueUrl);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        private async void SeekTo(VideoInfo video)
        {
            int startTime = (int) video.ResumePosition;
            startTime = startTime / 1000;
            string vlcEnqueueUrl =
                $"http://localhost:{webUIPort}/requests/status.xml?command=seek&val={startTime}";
            try
            {
                // Make HTTP request to Web UI
                HttpClient client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes(":" + webUIPassword);
                var header = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Authorization = header;
                HttpResponseMessage response = await client.GetAsync(vlcEnqueueUrl);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        // Make and handle VLC web UI request
        private async void HandleWebUIRequest(object source, System.Timers.ElapsedEventArgs e)
        {
            // Stop timer for the time request is processed
            playerWebUiTimer.Stop();

            // Request
            string VLCUIStatusUrl = $"http://localhost:{webUIPort}/requests/status.xml";

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
                                    filePosition = webPosition * 1000;
                                    if (isDoubleDuration)
                                        fileDuration = webDuration * 1000;

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
                logger.Error(ex, ex.ToString());
                playerWebUiTimer?.Start();
            }
        }

        private async Task<string> GetNowPlayingFile()
        {
            string filename = "";
            string VLCUIPlaylistUrl = $"http://localhost:{webUIPort}/requests/playlist.xml";

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
                            filename = filename.Replace("file:///", string.Empty).Replace("/", "\\").Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }

            return filename;
        }
    }
}