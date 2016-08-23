using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JMMClient.JMMServerBinary;
using JMMClient.ViewModel;
using NLog;
using Stream = System.IO.Stream;

namespace JMMClient.VideoPlayers
{
    public static class Extensions
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public static VideoInfo ToVideoInfo(this VideoDetailedVM vid, bool forcebegining)
        {
            if (string.IsNullOrEmpty(vid.FullPath))
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
                    WasWatched = vid.WatchedDate.HasValue,
                    VideoDetailed = vid,
                };
            }
            return new VideoInfo
            {
                Uri = vid.FullPath,
                IsUrl = false,
                Duration = vid.VideoInfo_Duration,
                ResumePosition = forcebegining ? 0 : vid.VideoLocal_ResumePosition,
                VideoLocalId = vid.VideoLocalID,
                WasWatched = vid.WatchedDate.HasValue,
                VideoDetailed = vid
            };
        }
        public static VideoInfo ToVideoInfo(this VideoLocalVM vid, bool forcebegining)
        { 
            if (string.IsNullOrEmpty(vid.LocalFileSystemFullPath))
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
                Uri = vid.LocalFileSystemFullPath,
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
            string fname = Path.GetFileNameWithoutExtension(fullname);
            if (p.Streams != null)
            {
                foreach (JMMClient.JMMServerBinary.Stream s in p.Streams.Where(a => a.File != null && a.StreamType == "3"))
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
            return new Tuple<string, List<string>>(p.Key, subs);
        }


    }
}
