using System;
using System.Collections.Generic;
using System.IO;
using JMMClient.VideoPlayers;
using NLog;

namespace JMMClient.Utilities
{
    public abstract class BaseVideoPlayer
    {
        private System.Timers.Timer handleTimer = null;
        public bool IsPlaying { get; set; }
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private FileSystemWatcher watcher = null;


        internal string PlayerPath { get; set; }
        public bool Active { get; internal set; }
        public abstract void Init();
        internal abstract void FileChangeEvent(string path);
        private string iniPath;

        public delegate void FilesPositionsHandler(Dictionary<string, long> positions);

        public delegate void FilePositionHandler(VideoInfo info, long position);
        public event FilesPositionsHandler FilePositionsChange;
        public event FilePositionHandler VideoInfoChange;
        protected void OnPositionChangeEvent(Dictionary<string, long> positions)
        {
            FilePositionsChange?.Invoke(positions);
        }

        protected void OnPositionChangeEvent(VideoInfo info, long position)
        {
            VideoInfoChange?.Invoke(info,position);
        }
        internal virtual void StartWatcher(string path)
        {

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                watcher = new FileSystemWatcher(path, "*.ini");
                watcher.IncludeSubdirectories = false;
                watcher.Changed += (a, e) =>
                {
                    // delay by 200ms since player will update the file multiple times in quick succession
                    // and also the delay allows us access to the file
                    iniPath = e.FullPath;
                    handleTimer?.Stop();
                    handleTimer = new System.Timers.Timer();
                    handleTimer.AutoReset = false;
                    handleTimer.Interval = 200; // 200 ms
                    handleTimer.Elapsed += (aq, b) =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                        {
                            FileChangeEvent(iniPath);
                        });
                    };
                    handleTimer.Enabled = true;

                };
                watcher.EnableRaisingEvents = true;
            }
        }

        internal virtual void StopWatcher()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }
        }

        public abstract void Play(VideoInfo video);
    }
}
