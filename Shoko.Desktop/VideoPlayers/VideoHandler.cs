using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Models.Enums;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;


namespace Shoko.Desktop.VideoPlayers
{
    public class VideoHandler
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private TraktHelper traktHelper = new TraktHelper();
        private Dictionary<int, VideoInfo> recentlyPlayedFiles = null;

        private System.Timers.Timer handleTimer = null;
        private bool scrobbleLock;
        public static long scrobblePosition;

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
            //Players.Add(new MPVVideoPlayer());
            Players.Add(new MPCVideoPlayer());
            Players.Add(new VLCVideoPlayer());
            Players.Add(new ZoomPlayerVideoPlayer());
            Players.Add(new ExternalMPVVideoPlayer());
            Players.Add(new PotVideoPlayer());
            Players.Add(new WindowsDefaultVideoPlayer());

            foreach (IVideoPlayer v in Players)
            {
                v.FilePositionsChange += FindChangedFiles;
                v.VideoInfoChange += CheckWatchStatus;
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
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
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

        public IVideoPlayer DefaultPlayer => ResolvePlayer();

        public void PlayVideo(VM_VideoLocal vid, bool forcebegining)
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
        public void PlayVideo(VM_VideoDetailed vid,bool forcebegining)
        {
            try
            {
                IVideoPlayer player = ResolvePlayer();
                if (player == null)
                    throw new Exception("Please configure a Video Player");
                VideoInfo info = vid.ToVideoInfo(forcebegining);

                if(player.Player == VideoPlayer.WindowsDefault && info.IsUrl)
                    throw new Exception("Streaming is not supported from Windows default player, please select one of the supported ones from settings");

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
                logger.Error(ex, ex.ToString());
            }
        }

        private void CheckWatchStatus(VideoInfo v, long position)
        {
            MayUpdateWatchStatus(v, position);
        }
        internal bool MayUpdateWatchStatus(VideoInfo v, long position)
        {

            if (v != null)
            {
                if (!AppSettings.VideoAutoSetWatched) return false;
                // we don't care about files that are already watched
                if (v.WasWatched) return true;
                // now check if this file is considered watched
                double fileDurationMS = (double)v.Duration;

                double progress = position / fileDurationMS * 100.0d;

                if (progress > (double)AppSettings.VideoWatchedPct)
                {
                    logger.Info($"Updating to watched by media player: {v.Uri}");

                    VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnVideo(v.VideoLocalId, true, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (v.VideoDetailed != null)
                    {
                        VM_MainListHelper.Instance.UpdateHeirarchy(v.VideoDetailed);
                        VM_MainListHelper.Instance.GetSeriesForVideo(v.VideoLocalId);

                        //kvp.Value.VideoLocal_IsWatched = 1;
                        v.WasWatched = true;
                        OnVideoWatchedEvent(new VideoWatchedEventArgs(v.VideoLocalId, v.VideoDetailed));
                    }
                    Debug.WriteLine("complete");

                }
            }
            return true;
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
                VideoInfo v = recentlyPlayedFiles.Values.FirstOrDefault(a => a.Uri == kvp.Key);
                if (v != null)
                {
                    logger.Info($"Video position for {v.Uri} has changed to {kvp.Value}");
                    v.ChangePosition(kvp.Value); //Set New Resume Position

                    try
                    {
                        if (VM_ShokoServer.Instance.Trakt_IsEnabled &&
                            !string.IsNullOrEmpty(VM_ShokoServer.Instance.Trakt_AuthToken)
                            && !scrobbleLock)
                        {
                            scrobblePosition = kvp.Value;
                            scrobbleLock = true;
                            traktHelper.TraktScrobble(TraktHelper.ScrobblePlayingStatus.Start, v, (int) kvp.Value,
                                (int) v.Duration);
                            scrobbleLock = false;
                        }
                    }
                    catch (Exception)
                    {
                        scrobbleLock = false;
                    }

                    if (!MayUpdateWatchStatus(v, kvp.Value))
                        return;
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
                List<VM_AnimeEpisode_User> episodes = VM_ShokoServer.Instance.ShokoServices.GetAllUnwatchedEpisodes(animeSeriesID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();

                string plsPath = GenerateTemporaryPlayList(episodes);
                if (!string.IsNullOrEmpty(plsPath))
                    player.Play(new VideoInfo {Uri = plsPath, IsPlaylist = true});
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex.Message, ex);
            }
        }

        public void PlayEpisodes(List<VM_AnimeEpisode_User> episodes)
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

        public string GenerateTemporaryPlayList(List<VM_AnimeEpisode_User> episodes)
        {
            try
            {
                List<VM_VideoDetailed> vids = new List<VM_VideoDetailed>();
                foreach (VM_AnimeEpisode_User ep in episodes)
                {
                    if (ep.FilesForEpisode.Count > 0)
                    {
                        VM_VideoDetailed vid = GetAutoFileForEpisode(ep);
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
                logger.Error(ex, ex.ToString());
            }

            return string.Empty;
        }

        public string GenerateTemporaryPlayList(List<VM_VideoDetailed> vids)
        {
            try
            {
                // get a temporary file
                string filePath = Utils.GetTempFilePathWithExtension(".pls");

                string plsContent = "";

                plsContent += @"[playlist]" + Environment.NewLine;
                List<string> lines=new List<string>();

                string url = $"http://{AppSettings.JMMServer_Address}:{AppSettings.JMMServer_Port}/Stream/%s/%s/false/null";

                for (int i = 1; i <= vids.Count; i++)
                {
                    var vid = vids[i - 1];
                    if (string.IsNullOrEmpty(vid.GetFullPath()))
                        lines.Add($@"File{i}={string.Format(url, vid.VideoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID)}" + Environment.NewLine);
                    else if (!string.IsNullOrEmpty(vids[i-1].GetFullPath()))
                        lines.Add($@"File{i}={vids[i - 1].GetFullPath()}" + Environment.NewLine);
                }
                if (lines.Count == 0)
                    return string.Empty;
                plsContent += string.Join("",lines);
                plsContent += @"NumberOfEntries=" + lines.Count + Environment.NewLine;
                plsContent += @"Version=2" + Environment.NewLine;

                File.WriteAllText(filePath, plsContent);

                return filePath;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }

            return string.Empty;
        }

        public VM_VideoDetailed GetAutoFileForEpisode(VM_AnimeEpisode_User ep)
        {
            try
            {
                if (ep.FilesForEpisode == null) return null;
                if (ep.FilesForEpisode.Count == 1) return ep.FilesForEpisode[0];

                // find the previous episode
                VM_AnimeEpisode_User raw = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetPreviousEpisodeForUnwatched(ep.AnimeSeriesID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);


                if (raw == null)
                {
                    return ep.FilesForEpisode.OrderByDescending(a => a.GetOverallVideoSourceRanking()).FirstOrDefault();
                }
                else
                {
                    List<VM_VideoDetailed> vids = ep.FilesForEpisode.OrderByDescending(a=>a.GetOverallVideoSourceRanking()).ToList();

                    // sort by quality

                    if (AppSettings.AutoFileSubsequent == (int)AutoFileSubsequentType.BestQuality)
                    {
                        // just use the best quality file
                        return vids[0];
                    }
                    else
                    {
                        // otherwise look at which groups files they watched previously
                        VM_AnimeEpisode_User previousEp = raw;
                        List<VM_VideoDetailed> vidsPrevious = previousEp.FilesForEpisode;

                        foreach (VM_VideoDetailed vidPrev in vidsPrevious)
                        {
                            if (vidPrev.Watched)
                            {
                                foreach (VM_VideoDetailed vid in vids)
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
                logger.Error(ex, ex.ToString());
            }

            return null;
        }
    }

    public class VideoWatchedEventArgs : EventArgs
    {
        public readonly int VideoLocalID = 0;
        public readonly VM_VideoDetailed VideoLocal = null;

        public VideoWatchedEventArgs(int videoLocalID, VM_VideoDetailed vid)
        {
            VideoLocalID = videoLocalID;
            VideoLocal = vid;
        }
    }
}
