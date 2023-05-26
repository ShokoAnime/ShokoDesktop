using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.PlexAndKodi;

namespace Shoko.Desktop.VideoPlayers
{
    public static class Extensions
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public static VideoInfo ToVideoInfo(this VM_VideoDetailed vid, bool forcebegining)
        {
            if (string.IsNullOrEmpty(vid.GetFullPath()))
            {
                Tuple<string, List<string>> t = GetInfo(vid.VideoLocalID, vid.FileName, vid.Media);
                return new VideoInfo
                {
                    Uri = t.Item1,
                    SubtitlePaths = t.Item2,
                    IsUrl = true,
                    Duration = vid.VideoInfo_Duration,
                    ResumePosition = forcebegining ? 0 : vid.VideoLocal_ResumePosition,
                    VideoLocalId = vid.VideoLocalID,
                    WasWatched = vid.VideoLocal_WatchedDate.HasValue,
                    VideoDetailed = vid,
                };
            }
            return new VideoInfo
            {
                Uri = vid.GetFullPath(),
                IsUrl = false,
                Duration = vid.VideoInfo_Duration,
                ResumePosition = forcebegining ? 0 : vid.VideoLocal_ResumePosition,
                VideoLocalId = vid.VideoLocalID,
                WasWatched = vid.VideoLocal_WatchedDate.HasValue,
                VideoDetailed = vid
            };
        }
        public static VideoInfo ToVideoInfo(this VM_VideoLocal vid, bool forcebegining)
        { 
            if (string.IsNullOrEmpty(vid.GetLocalFileSystemFullPath()))
            {
                Tuple<string, List<string>> t = GetInfo(vid.VideoLocalID, vid.FileName, vid.Media);
                return new VideoInfo
                {
                    Uri = t.Item1,
                    SubtitlePaths = t.Item2,
                    IsUrl = true,
                    Duration = vid.Duration,
                    ResumePosition = forcebegining ? 0 : vid.ResumePosition,
                    VideoLocalId = vid.VideoLocalID,
                    VideoLocal = vid,
                    WasWatched = vid.WatchedDate.HasValue
                };
            }
            return new VideoInfo
            {
                Uri = vid.GetLocalFileSystemFullPath(),
                IsUrl = false,
                Duration = vid.Duration,
                ResumePosition = forcebegining ? 0 : vid.ResumePosition,
                VideoLocalId = vid.VideoLocalID,
                VideoLocal =  vid,
                WasWatched =  vid.WatchedDate.HasValue
            };
        }
        
        public static string Base64EncodeUrl(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes).Replace("+", "-").Replace("/", "_").Replace("=", ",");
        }
        
        private static readonly Regex UrlSafe = new Regex("[ \\$^`:<>\\[\\]\\{\\}\"“\\+%@/;=\\?\\\\\\^\\|~‘,]",
            RegexOptions.Compiled);

        private static readonly Regex UrlSafe2 = new Regex("[^0-9a-zA-Z_\\.\\s]", RegexOptions.Compiled);

        static Tuple<string, List<string>> GetInfo(int vlID, string path, Media m)
        {
            List<string> subs = new List<string>();
            //Only Support one part for now
            string name = UrlSafe.Replace(Path.GetFileName(path), " ").CompactWhitespaces().Trim();
            name = UrlSafe2.Replace(name, string.Empty).Trim().Replace(" ", "_").CompactCharacters('.', '_')
                .Replace("_.", ".").TrimStart('_').TrimStart('.');
            name = WebUtility.UrlEncode(name);

            string uri =
                $"http://{AppSettings.JMMServer_Address}:{AppSettings.JMMServer_Port}/Stream/{vlID}/{VM_ShokoServer.Instance.CurrentUser.JMMUserID}/false/{name}";
            string fname = Path.GetFileNameWithoutExtension(path);
            var p = m?.Parts?.FirstOrDefault();
            if (p?.Streams == null) return new Tuple<string, List<string>>(uri, subs);
            WebClient wc = new WebClient();
            foreach (Models.PlexAndKodi.Stream s in p.Streams.Where(a => a.File != null && a.StreamType == 3))
            {
                string extension = Path.GetExtension(s.File);
                string filePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(Path.GetDirectoryName(path)));
                
                try
                {
                    var url = $"http://{AppSettings.JMMServer_Address}:{AppSettings.JMMServer_Port}/Stream/Filename/{Base64EncodeUrl(s.File)}/{VM_ShokoServer.Instance.CurrentUser.JMMUserID}/false";
                    string subtitle = wc.DownloadString(url);
                    try
                    {
                        filePath = Path.Combine(Path.GetTempPath(), fname + extension);
                        File.WriteAllText(filePath, subtitle);
                        subs.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Cannot download subtitle: " + ex);
                    }

                }
                catch (Exception e)
                {
                    logger.Warn("Cannot download subtitle: " + e);
                }
            }
            return new Tuple<string, List<string>>(uri, subs);
        }


    }
}
