using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class ZoomPlayerVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {
        private bool _monitoringPositions;
        private int _tcpControlPort = 4769;
        private bool _tcpControlServerEnabled = false;

        public VideoPlayer Player => VideoPlayer.ZoomPlayer;

        public override void Init()
        {
            try
            {
                string installDir =
                    (string)
                        Registry.GetValue(@"HKEY_CURRENT_USER\Software\VirtuaMedia\ZoomPlayer", "InstallDirectory", null);
                Int32 tcpControlServerEnabled =
                    (Int32) Registry.GetValue(@"HKEY_CURRENT_USER\Software\VirtuaMedia\ZoomPlayer", "OPTCPEnable", null);
                Int32 tcpControlPort =
                    (Int32) Registry.GetValue(@"HKEY_CURRENT_USER\Software\VirtuaMedia\ZoomPlayer", "OPTCPPort", null);

                if (installDir != null)
                {
                    PlayerPath = string.Format("{0}\\zplayer.exe", installDir);

                    if (File.Exists(PlayerPath))
                    {
                        // Check if TCP control server is enabled
                        switch (tcpControlServerEnabled)
                        {
                            case 0:
                                _tcpControlServerEnabled = false;
                                break;
                            case 1:
                                _tcpControlServerEnabled = true;
                                break;
                        }

                        // Retrieve TCP control port and fallback to default in case of error
                        _tcpControlPort = tcpControlPort;

                        //Debug.WriteLine("Zoom player - path: " + PlayerPath);
                        //Debug.WriteLine("Zoom player - control server enabled: " + _tcpControlServerEnabled);
                        //Debug.WriteLine("Zoom player - control server port: " + _tcpControlPort);

                        Active = true;
                        return;
                    }
                }

                Active = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Zoom player - Error during Init: " + e.Message);
                Active = false;
            }
        }

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
                    {
                        // Convert to seconds and timespan format
                        Debug.WriteLine($"Zoom player - Server resume position in ms: {video.ResumePosition}");
                        string resumeTimeSpan = TimeSpan.FromSeconds(video.ResumePosition / 1000).ToString(@"hh\:mm\:ss");
                        Debug.WriteLine($"Zoom player - Resume position in timespan format: {resumeTimeSpan}");
                        init += " /Seek:" + resumeTimeSpan;
                        Debug.WriteLine(video.Uri);
                        Debug.WriteLine(init);
                    }

                    // Only subtitle track is supported via command line
                    /*
                    if (video.SubtitlePaths != null && video.SubtitlePaths.Count > 0)
                    {
                        foreach (string s in video.SubtitlePaths)
                        {
                            init += " /sub:\"" + s + "\"";
                        }
                    }*/

                    process = Process.Start(PlayerPath, init);
                }
                if (process != null)
                {
                    IsPlaying = true;

                    if (_tcpControlServerEnabled)
                    {
                        Thread t = new Thread(() => StartPlayPositionRetrieval(video.Uri, true));
                        t.IsBackground = true;
                        t.Start();
                    }

                    process.WaitForExit();
                    IsPlaying = false;
                }
            });
        }

        internal override void FileChangeEvent(string filePath)
        {
            // Not used
        }

        // Process Zoom player TCP control commands
        private void StartPlayPositionRetrieval(string fileName, bool firstStart = false)
        {
            double postitionTimeCodeMs = 0;

            try
            {
                if (firstStart)
                {
                    // Wait for process to start and skip short playbacks
                    var startupDelay = TimeSpan.FromSeconds(5);
                    Thread.Sleep(startupDelay);

                    // Check if still running
                    if (!IsPlaying)
                    {
                        Debug.WriteLine("Zoom player - After waiting 5 seconds ZP was no longer running, stopping playback record.");
                        return;
                    }
                }

                var tc = new ZPConnection("localhost", _tcpControlPort);
                var playerInactive = false;
                char[] charSeparator = {' '};
                var videoDuration = "";

                // Disable live reporting
                if (tc.IsConnected)
                {
                    tc.Write("1100 0" + Environment.NewLine);
                }
                else
                {
                    Debug.WriteLine("Zoom player - No connection could be made to zoom player API using port 4769 to store time code!");
                    return;
                }

                while (tc.IsConnected && IsPlaying)
                {
                    try
                    {
                        // Check if playing
                        tc.Write("1000" + Environment.NewLine);

                        var playing = tc.Read();

                        if (playing.StartsWith("1000 "))
                        {
                            if (playing.Contains(" "))
                            {
                                var playingSplit = playing.Split(charSeparator);
                                playing = playingSplit[1].Trim();

                                if (playing != "3")
                                {
                                    playerInactive = true;
                                    //Debug.WriteLine("Zoom player is paused");
                                }
                                else
                                {
                                    playerInactive = false;
                                    //Debug.WriteLine("Zoom player is running");
                                }
                            }
                        }
                        Thread.Sleep(100);

                        if (!playerInactive)
                        {
                            tc.Write("1110" + Environment.NewLine);
                            videoDuration = tc.Read();
                            if (string.IsNullOrEmpty(videoDuration.Trim()) || videoDuration.ToLower().Contains("seek") ||
                                videoDuration.ToLower().StartsWith("2200") ||
                                videoDuration.ToLower().Contains("derived"))
                            {
                                continue;
                            }
                            Thread.Sleep(100);

                            tc.Write("1120" + Environment.NewLine);

                            var timecode = tc.Read();

                            if (timecode.StartsWith("1120 "))
                            {
                                try
                                {
                                    if (videoDuration.Contains(" "))
                                    {
                                        var videoDurationSplit = videoDuration.Split(charSeparator);
                                        var timeCodeSplit = timecode.Split(charSeparator);
                                        videoDuration = videoDurationSplit[1].Trim();
                                        timecode = timeCodeSplit[1].Trim();

                                        double videoDurationMS = 0;
                                        var successfullyParsedVideoDuration = double.TryParse(videoDuration,
                                            out videoDurationMS);
                                        var successfullyParsedTimeCode = double.TryParse(timecode,
                                            out postitionTimeCodeMs);

                                        if (successfullyParsedVideoDuration && successfullyParsedTimeCode)
                                        {
                                            tc.Write("1800" + Environment.NewLine);
                                            var videoFilenameRetrieved = tc.Read();

                                            if (videoFilenameRetrieved.StartsWith("1800 "))
                                            {
                                                var videoFilenameReadOut =
                                                    videoFilenameRetrieved.Replace("1800 ", string.Empty).Trim();

                                                if (!string.IsNullOrEmpty(videoFilenameReadOut) &&
                                                    videoFilenameReadOut.ToLower() != fileName.ToLower())
                                                {
                                                    Debug.WriteLine("Zoom player - Video filename didn't match original: " +
                                                                    videoFilenameReadOut);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(1000);

                        if (!IsPlaying)
                        {
                            Thread.Sleep(250);
                            Debug.WriteLine("[Position retrieval] Zoom player - no longer running");
                            UpdateFilePosition(fileName, postitionTimeCodeMs);
                        }
                        else
                        {
                            Thread.Sleep(250);
                            Debug.WriteLine("[Position retrieval] Zoom player - error occured");
                            Debug.WriteLine(e.Message);

                            StartPlayPositionRetrieval(fileName);
                        }
                    }
                }
                UpdateFilePosition(fileName, postitionTimeCodeMs);
            }
            catch (Exception)
            {
                Thread.Sleep(1000);

                if (!IsPlaying)
                {
                    UpdateFilePosition(fileName, postitionTimeCodeMs);
                }
                else
                {
                    StartPlayPositionRetrieval(fileName);
                }
            }
        }

        private void UpdateFilePosition(string fileName, double position)
        {
            Debug.WriteLine("Zoom player - updating timecode (ms): " + position);

            Dictionary<string, long> filePositions = new Dictionary<string, long>();
            filePositions.Add(fileName, (long)position);
            OnPositionChangeEvent(filePositions);
        }

        internal enum Verbs
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }

        internal enum Options
        {
            SGA = 3
        }

        internal class ZPConnection
        {
            private readonly TcpClient tcpSocket;

            private int TimeOutMs = 100;

            public ZPConnection(string Hostname, int Port)
            {
                try
                {
                    tcpSocket = new TcpClient(Hostname, Port);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Zoom player - No connection could be made to tcp control server");
                }
            }

            public bool IsConnected
            {
                get { return tcpSocket.Connected; }
            }

            public void WriteLine(string cmd)
            {
                Write(cmd + "\n");
            }

            public void Write(string cmd)
            {
                if (!tcpSocket.Connected) return;
                var buf = Encoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
                tcpSocket.GetStream().Write(buf, 0, buf.Length);
            }

            public string Read()
            {
                if (!tcpSocket.Connected) return null;
                var sb = new StringBuilder();
                do
                {
                    ParseCommmand(sb);
                    Thread.Sleep(TimeOutMs);
                } while (tcpSocket.Available > 0);
                return sb.ToString();
            }

            private void ParseCommmand(StringBuilder sb)
            {
                while (tcpSocket.Available > 0)
                {
                    var input = tcpSocket.GetStream().ReadByte();
                    switch (input)
                    {
                        case -1:
                            break;
                        case (int)Verbs.IAC:
                            // interpret as command
                            var inputverb = tcpSocket.GetStream().ReadByte();
                            if (inputverb == -1) break;
                            switch (inputverb)
                            {
                                case (int)Verbs.IAC:
                                    //literal IAC = 255 escaped, so append char 255 to string
                                    sb.Append(inputverb);
                                    break;
                                case (int)Verbs.DO:
                                case (int)Verbs.DONT:
                                case (int)Verbs.WILL:
                                case (int)Verbs.WONT:
                                    // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                    var inputoption = tcpSocket.GetStream().ReadByte();
                                    if (inputoption == -1) break;
                                    tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                    if (inputoption == (int)Options.SGA)
                                        tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                    else
                                        tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                    tcpSocket.GetStream().WriteByte((byte)inputoption);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            sb.Append((char)input);
                            break;
                    }
                }
            }
        }
    }
}
