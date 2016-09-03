using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using JMMClient.JMMServerBinary;
using JMMClient.Utilities;
using JMMClient.ViewModel;
using Microsoft.Win32;
using NLog;
using Stream = JMMClient.JMMServerBinary.Stream;


namespace JMMClient.VideoPlayers
{
    public class VideoHandler
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, VideoInfo> recentlyPlayedFiles = null;

        private System.Timers.Timer handleTimer = null;


        private List<FileSystemWatcher> watcherVids = null;
        Dictionary<string, string> previousFilePositions = new Dictionary<string, string>();

        // Timer for MPC HC Web UI Requests
        private System.Timers.Timer playerWebUiTimer = null;


        public List<IVideoPlayer> Players=new List<IVideoPlayer>();



        public VideoHandler()
        {
            AddTempPathToSubtilePaths();
            recentlyPlayedFiles = new Dictionary<int, VideoInfo>();
            previousFilePositions.Clear();

            Players = new List<IVideoPlayer>();
            Players.Add(new MPVVideoPlayer());
            Players.Add(new MPCVideoPlayer());
            Players.Add(new PotVideoPlayer());
            Players.Add(new VLCVideoPlayer());
            Players.Add(new ExternalMPVVideoPlayer());
            foreach (IVideoPlayer v in Players)
            {
                v.PositionChange += FindChangedFiles;
            }
        }
        /*
        private bool IsLocalMachine(string url)
        {
            Uri uri = new Uri(url, UriKind.Absolute);
            List<IPAddress> addresses=new List<IPAddress>();
            IPAddress oip;
            if (!IPAddress.TryParse(uri.Host, out oip))
            {
                IPHostEntry entry = Dns.GetHostEntry(uri.Host);
                if (entry != null && entry.AddressList != null && entry.AddressList.Length > 0)
                {
                    addresses.AddRange(entry.AddressList);
                }
                else
                    return true;
            }
            else
                addresses.Add(oip);
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation uip in item.GetIPProperties().UnicastAddresses)
                    {
                        foreach (IPAddress ip in addresses)
                        {
                            if (uip.Address.Equals(ip))
                                return true;
                        }
                    }
                }
            }
            return false;
        }
        */

        public delegate void VideoWatchedEventHandler(VideoWatchedEventArgs ev);
        public event VideoWatchedEventHandler VideoWatchedEvent;
        protected void OnVideoWatchedEvent(VideoWatchedEventArgs ev)
        {
            VideoWatchedEvent?.Invoke(ev);
        }

      

        private void AddTempPathToSubtilePaths()
        {
            string path = Path.GetTempPath();
            //FFDSHow
            try
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GNU\ffdshow",true);
                if (k != null)
                {
                    string org = (string)k.GetValue("subSearchDir", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("subSearchDir", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GNU\ffdshow64",true);
                if (k != null)
                {
                    string org = (string)k.GetValue("subSearchDir", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if(!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("subSearchDir", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Gabest\VSFilter\DefTextPathes",true);
                if (k != null)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        string val = (string)k.GetValue("Path" + x, null);
                        if (val != null && val == path)
                            break;
                        if (val == null)
                        {
                            k.SetValue("Path" + x, path);
                            break;
                        }
                    }
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MPC-HC\MPC-HC\Settings",true);
                if (k != null)
                {
                    string org = (string)k.GetValue("SubtitlePaths", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("SubtitlePaths", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Daum\PotPlayerMini\CaptionFolderList",true);
                if (k != null)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        string val = (string)k.GetValue(x.ToString(), null);
                        if (val != null && val == path)
                            break;
                        if (val == null)
                        {
                            k.SetValue(x.ToString(), path);
                            break;
                        }
                    }
                }
                string vlcrcpath = Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.ApplicationData)), "vlc", "vlcrc");
                try
                {
                    if (File.Exists(vlcrcpath))
                    {
                        string[] lines = File.ReadAllLines(vlcrcpath);
                        for (int x = 0; x < lines.Length; x++)
                        {
                            string s = lines[x];
                            if (s.StartsWith("#sub-autodetect-path=") || s.StartsWith("sub-autodetect-path="))
                            {
                                if (!s.Contains(path))
                                {
                                    s += ", " + path;
                                    if (s.StartsWith("#"))
                                        s = s.Substring(1);
                                    lines[x] = s;
                                    File.WriteAllLines(vlcrcpath, lines);
                                    break;
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {

                }
            }
            catch (Exception e)
            {
                int a = 1;
            }
            

        }



        private IVideoPlayer ResolvePlayer()
        {
            IVideoPlayer p = Players.FirstOrDefault(a => a.Player == (VideoPlayer)AppSettings.DefaultPlayer_GroupList);
            if (p != null && p.Active)
                return p;
            return Players.FirstOrDefault(a => a.Active);
        }

        public bool IsActive(VideoPlayer p)
        {
            IVideoPlayer v = Players.FirstOrDefault(a => a.Player == p);
            if (v == null)
                return false;
            v.Init();
            return v.Active;
        }

        public bool Active
        {
            get { return Players.FirstOrDefault(a => a.Active) != null; }
        }

        public IVideoPlayer DefaultPlayer
        {
            get { return ResolvePlayer(); }
        }

        public void PlayVideo(VideoLocalVM vid, bool forcebegining)
        {
            try
            {
                IVideoPlayer player = ResolvePlayer();
                if (player == null)
                    throw new Exception("Please configure a Video Player");
                VideoInfo info = vid.ToVideoInfo(forcebegining);
                recentlyPlayedFiles[info.VideoLocalId]=info;
                player.Play(info);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
        public void PlayVideo(VideoDetailedVM vid,bool forcebegining)
        {
            try
            {
                IVideoPlayer player = ResolvePlayer();
                if (player == null)
                    throw new Exception("Please configure a Video Player");
                VideoInfo info = vid.ToVideoInfo(forcebegining);
                recentlyPlayedFiles[info.VideoLocalId] = info;
                player.Play(info);
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
                foreach (IVideoPlayer v in Players)
                {
                    v.Init();
                }


            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }


        private void FindChangedFiles(Dictionary<string, long> filePositions)
        {
            // find all the files which have changed
            Dictionary<string, long> changedFilePositions = new Dictionary<string, long>();

            foreach (KeyValuePair<string, long> kvp in filePositions)
            {
                changedFilePositions[kvp.Key] = kvp.Value;
            }

            // update the changed positions
            foreach (KeyValuePair<string, long> kvp in changedFilePositions)
            {
                previousFilePositions[kvp.Key] = kvp.Value.ToString();
            }

            foreach (KeyValuePair<string, long> kvp in changedFilePositions)
            {
                long mpcPosMS = kvp.Value;

                VideoInfo v = recentlyPlayedFiles.Values.FirstOrDefault(a => a.Uri == kvp.Key);
                if (v!=null)
                {
                    logger.Info(string.Format("Video position for {0} has changed to {1}", kvp.Key, kvp.Value));
                    v.ChangePosition(kvp.Value); //Set New Resume Position

                    if (!AppSettings.VideoAutoSetWatched) return;
                    // we don't care about files that are already watched
                    if (v.WasWatched) continue;


                    // now check if this file is considered watched
                    double fileDurationMS = (double)v.Duration;

                    double progress = mpcPosMS / fileDurationMS * 100.0d;

                    if (progress > (double)AppSettings.VideoWatchedPct)
                    {
                        logger.Info(string.Format("Updating to watched by media player: {0}", kvp.Key));

                        JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(v.VideoLocalId, true, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                        if (v.VideoDetailed != null)
                        {
                            MainListHelperVM.Instance.UpdateHeirarchy(v.VideoDetailed);
                            MainListHelperVM.Instance.GetSeriesForVideo(v.VideoLocalId);

                            //kvp.Value.VideoLocal_IsWatched = 1;
                            OnVideoWatchedEvent(new VideoWatchedEventArgs(v.VideoLocalId, v.VideoDetailed));
                        }
                        Debug.WriteLine("complete");

                    }
                }
            }
        }

        public void PlayAllUnwatchedEpisodes(int animeSeriesID)
        {
            try
            {
                IVideoPlayer player = ResolvePlayer();
                if (player == null)
                    throw new Exception("Please configure a Video Player");
                List<JMMServerBinary.Contract_AnimeEpisode> rawEps = JMMServerVM.Instance.clientBinaryHTTP.GetAllUnwatchedEpisodes(animeSeriesID,
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                List<AnimeEpisodeVM> episodes = new List<AnimeEpisodeVM>();
                foreach (JMMServerBinary.Contract_AnimeEpisode raw in rawEps)
                    episodes.Add(new AnimeEpisodeVM(raw));
                string plsPath = GenerateTemporaryPlayList(episodes);
                if (!string.IsNullOrEmpty(plsPath))
                    player.Play(new VideoInfo {Uri = plsPath, IsPlaylist = true});
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
                IVideoPlayer player = ResolvePlayer();
                if (player == null)
                    throw new Exception("Please configure a Video Player");
                string plsPath = GenerateTemporaryPlayList(episodes);
                if (!string.IsNullOrEmpty(plsPath))
                    player.Play(new VideoInfo { Uri = plsPath, IsPlaylist = true });
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
                            recentlyPlayedFiles[vid.VideoLocalID] = vid.ToVideoInfo(false);
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
                List<string> lines=new List<string>();

                for (int i = 1; i <= vids.Count; i++)
                {
                    if (string.IsNullOrEmpty(vids[i-1].FullPath) && vids[i - 1].Media != null && vids[i - 1].Media.Parts != null &&
                        vids[i - 1].Media.Parts.Count > 0)
                        lines.Add(string.Format(@"File{0}={1}", i, vids[i - 1].Media.Parts[0].Key) + Environment.NewLine);
                    else if (!string.IsNullOrEmpty(vids[i-1].FullPath))
                        lines.Add(string.Format(@"File{0}={1}", i, vids[i - 1].FullPath) + Environment.NewLine);
                }
                if (lines.Count == 0)
                    return string.Empty;
                plsContent += string.Join("",lines);
                plsContent += @"NumberOfEntries=" + lines.Count.ToString() + Environment.NewLine;
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
