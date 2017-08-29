using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using Shoko.Commons.Extensions;
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
                if (vid.Media?.Parts == null || vid.Media.Parts.Count == 0)
                    throw new Exception("There is no media information loaded in the video selected, we're unable to stream the media");
                Tuple<string, List<string>> t = GetInfoFromMedia(vid.Media);
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
                if (vid.Media?.Parts == null || vid.Media.Parts.Count == 0)
                    throw new Exception("There is no media information loaded in the video selected, we're unable to stream the media");
                Tuple<string, List<string>> t = GetInfoFromMedia(vid.Media);
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

        static Tuple<string, List<string>> GetInfoFromMedia(Media m)
        {
            WebClient wc = new WebClient();
            List<string> subs = new List<string>();
            //Only Support one part for now

            Part p = m.Parts[0];
            string fullname = p.Key.Replace("\\", "/").Replace("//", "/").Replace(":", string.Empty);
            string uri = $"http://{AppSettings.JMMServer_Address}:{AppSettings.JMMServer_Port}" +
                $"{new Uri(p.Key).GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)}";
            string fname = Path.GetFileNameWithoutExtension(fullname);
            if (p.Streams != null)
            {
                foreach (Models.PlexAndKodi.Stream s in p.Streams.Where(a => a.File != null && a.StreamType == "3"))
                {
                    string extension = Path.GetExtension(s.File);
                    string filePath = Path.Combine(Path.GetTempPath(), Path.GetDirectoryName(fullname));
                    try
                    {
                        string subtitle = wc.DownloadString(s.Key);
                        /*   try
                           {
                               Directory.CreateDirectory(filePath);
                               string fullpath = Path.Combine(filePath, fname + extension);
                               File.WriteAllText(filePath, subtitle);
                           }
                           catch (Exception)
                           {
                           }*/
                        try
                        {
                            filePath = Path.Combine(Path.GetTempPath(), fname + extension);
                            File.WriteAllText(filePath, subtitle);
                            subs.Add(filePath);
                        }
                        catch (Exception)
                        {
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Warn("Cannot download subtitle: " + e.ToString());
                    }
                }
            }
            return new Tuple<string, List<string>>(uri, subs);
        }


    }
}
