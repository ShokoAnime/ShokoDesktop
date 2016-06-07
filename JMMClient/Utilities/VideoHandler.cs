using JMMClient.ViewModel;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JMMClient.Utilities
{
    public class VideoHandler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, VideoDetailedVM> recentlyPlayedFiles = null;
        private System.Timers.Timer handleTimer = null;
        private string iniPath = string.Empty;

        private List<FileSystemWatcher> watcherVids = null;
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
                if (vid.FullPath.Contains("http:"))
                {
                    try
                    {
                        Process.Start(GetPlayerExe(), '"' + vid.FullPath.Replace(@"\", "/") + '"');
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex.ToString(), ex);
                    }
                }
                else
                {
                    try
                    {
                        string playerPath = GetPlayerExe();
                        if (string.IsNullOrEmpty(playerPath))
                            Process.Start(new ProcessStartInfo(vid.FullPath));
                        else
                            Process.Start(playerPath, '"' + vid.FullPath + '"');
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex.ToString(), ex);
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void PlayVideo(VideoLocalVM vid)
        {
            try
            {
                if (vid.FullPath.Contains("http:"))
                {
                    try
                    {
                        Process.Start(GetPlayerExe(), '"' + vid.FullPath.Replace(@"\", "/") + '"');
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex.ToString(), ex);
                    }
                }
                else
                {
                    try
                    {
                        string playerPath = GetPlayerExe();
                        if (string.IsNullOrEmpty(playerPath))
                            Process.Start(new ProcessStartInfo(vid.FullPath));
                        else
                            Process.Start(playerPath, '"' + vid.FullPath + '"');

                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex.ToString(), ex);
                    }
                }

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

                StopWatchingFiles();
                watcherVids = new List<FileSystemWatcher>();

                if (!string.IsNullOrEmpty(AppSettings.MPCFolder) && Directory.Exists(AppSettings.MPCFolder))
                {
                    FileSystemWatcher fsw = new FileSystemWatcher(AppSettings.MPCFolder, "*.ini");
                    fsw.IncludeSubdirectories = false;
                    fsw.Changed += new FileSystemEventHandler(fsw_Changed);
                    fsw.EnableRaisingEvents = true;
                }

                if (!string.IsNullOrEmpty(AppSettings.PotPlayerFolder) && Directory.Exists(AppSettings.PotPlayerFolder))
                {
                    FileSystemWatcher fsw = new FileSystemWatcher(AppSettings.PotPlayerFolder, "*.ini");
                    fsw.IncludeSubdirectories = false;
                    fsw.Changed += new FileSystemEventHandler(fsw_Changed);
                    fsw.EnableRaisingEvents = true;
                }

                if (!string.IsNullOrEmpty(AppSettings.VLCFolder) && Directory.Exists(AppSettings.VLCFolder))
                {
                    FileSystemWatcher fsw = new FileSystemWatcher(AppSettings.VLCFolder, "*.ini");
                    fsw.IncludeSubdirectories = false;
                    fsw.Changed += new FileSystemEventHandler(fsw_Changed);
                    fsw.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }

        public void StopWatchingFiles()
        {
            if (watcherVids == null) return;

            foreach (FileSystemWatcher fsw in watcherVids)
            {
                fsw.EnableRaisingEvents = false;
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
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                FileInfo fi = new FileInfo(iniPath);
                if (fi.DirectoryName.Equals(AppSettings.MPCFolder, StringComparison.InvariantCultureIgnoreCase))
                    HandleFileChangeMPC(iniPath);

                if (fi.DirectoryName.Equals(AppSettings.PotPlayerFolder, StringComparison.InvariantCultureIgnoreCase))
                    HandleFileChangePotPlayer(iniPath);

                if (fi.DirectoryName.Equals(AppSettings.VLCFolder, StringComparison.InvariantCultureIgnoreCase))
                    HandleFileChangeVLC(iniPath);
            });
        }

        public void HandleFileChangePotPlayer(string filePath)
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

                FindChangedFiles(filePositions);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }

        public void HandleFileChangeMPC(string filePath)
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
                }

                FindChangedFiles(filePositions);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }

        public void HandleFileChangeVLC(string filePath)
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

                FindChangedFiles(filePositions);
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

                foreach (KeyValuePair<int, VideoDetailedVM> kvpVid in recentlyPlayedFiles)
                {
                    if (kvpVid.Value.FullPath.Equals(Path.GetFullPath(kvp.Key), StringComparison.InvariantCultureIgnoreCase))
                    {
                        // we don't care about files that are already watched
                        if (kvpVid.Value.Watched) continue;

                        logger.Info(string.Format("Video position for {0} has changed to {1}", kvp.Key, kvp.Value));

                        // now check if this file is considered watched
                        double fileDurationMS = (double)kvpVid.Value.VideoInfo_Duration;

                        double progress = mpcPosMS / fileDurationMS * 100.0d;

                        if (progress > (double)AppSettings.VideoWatchedPct)
                        {
                            VideoDetailedVM vid = kvpVid.Value;

                            logger.Info(string.Format("Updating to watched by media player: {0}", kvp.Key));

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

        public void PlayAllUnwatchedEpisodes(int animeSeriesID)
        {
            try
            {
                List<JMMServerBinary.Contract_AnimeEpisode> rawEps = JMMServerVM.Instance.clientBinaryHTTP.GetAllUnwatchedEpisodes(animeSeriesID,
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                List<AnimeEpisodeVM> episodes = new List<AnimeEpisodeVM>();
                foreach (JMMServerBinary.Contract_AnimeEpisode raw in rawEps)
                    episodes.Add(new AnimeEpisodeVM(raw));


                string plsPath = GenerateTemporaryPlayList(episodes);
                if (!string.IsNullOrEmpty(plsPath))
                    Process.Start(plsPath);
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
                string plsPath = GenerateTemporaryPlayList(episodes);
                if (!string.IsNullOrEmpty(plsPath))
                    Process.Start(plsPath);
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
                            recentlyPlayedFiles[vid.VideoLocalID] = vid;
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

                for (int i = 1; i <= vids.Count; i++)
                    plsContent += string.Format(@"File{0}={1}", i, vids[i - 1].FullPath) + Environment.NewLine;

                plsContent += @"NumberOfEntries=" + vids.Count.ToString() + Environment.NewLine;
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



        /// <summary>
        /// Finds the path to the users set video player's executable. Checkes the sys path first, then for 64bit versions, followed lastly by 32 bit.
        /// </summary>
        /// <returns>A string of the full path to the default player otherwise returns a null if cant be found.</returns>
        public string GetPlayerExe()
        {
            //TODO check 32bit registry logic

            string playerPath = null;

            switch (UserSettingsVM.Instance.DefaultPlayer_GroupList)
            {
                case (int)DefaultVideoPlayer.MPC:

                    //Check if Media Player Classic is available from PATH.
                    playerPath = Utils.CheckSysPath(new string[] { "mpc-hc64.exe", "mpc-hc.exe" });
                    if (!string.IsNullOrEmpty(playerPath))
                        return playerPath;

                    //Look for 64bit
                    playerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Combined Community Codec Pack 64bit_is1", "InstallLocation", null);
                    if (!string.IsNullOrEmpty(playerPath))
                        playerPath = Path.Combine(playerPath, @"MPC\mpc-hc64.exe");
                    else // could not find 64, look for 32
                    {
                        playerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Combined Community Codec Pack_is1", "InstallLocation", null);
                        if (!string.IsNullOrEmpty(playerPath))
                            playerPath = Path.Combine(playerPath, @"MPC\mpc-hc.exe");
                    }
                    return playerPath;
                case (int)DefaultVideoPlayer.PotPlayer:
                    //Check if PotPlayer is available from PATH.
                    playerPath = Utils.CheckSysPath(new string[] { "PotPlayerMini64.exe", "PotPlayerMini.exe" });
                    if (!string.IsNullOrEmpty(playerPath))
                        return playerPath;

                    playerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PotPlayer64", "DisplayIcon", null);
                    if (!string.IsNullOrEmpty(playerPath))
                        return playerPath;

                    playerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\PotPlayer", "DisplayIcon", null);
                    if (!string.IsNullOrEmpty(playerPath))
                        return playerPath;
                    return playerPath;
                case (int)DefaultVideoPlayer.VLC:
                    //Check if VLC is available from PATH.
                    playerPath = Utils.CheckSysPath(new string[] { "vlc.exe" });
                    if (!string.IsNullOrEmpty(playerPath))
                        return playerPath;

                    playerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VLC media player", "DisplayIcon", null);
                    return playerPath;
                default:
                    return playerPath;
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
