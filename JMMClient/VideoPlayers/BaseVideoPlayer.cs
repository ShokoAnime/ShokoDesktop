using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace JMMClient.Utilities
{
    public abstract class BaseVideoPlayer
    {
        private System.Timers.Timer handleTimer = null;

        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private FileSystemWatcher watcher = null;


        internal string PlayerPath { get; set; }
        public bool Active { get; internal set; }
        public abstract void Init();
        internal abstract void FileChangeEvent(string path);
        private string iniPath;

        public delegate void FilesPositionsHandler(Dictionary<string, long> positions);
        public event FilesPositionsHandler PositionChange;

        protected void OnPositionChangeEvent(Dictionary<string, long> positions)
        {
            PositionChange?.Invoke(positions);
        }
        internal virtual void StartWatchingFiles(string path)
        {
            if (!AppSettings.VideoAutoSetWatched) return;

            StopWatchingFiles();

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                watcher = new FileSystemWatcher(path, "*.ini");
                watcher.IncludeSubdirectories = false;
                watcher.Changed += (a, e) =>
                {
                    // delay by 200ms since player will update the file multiple times in quick succession
                    // and also the delay allows us access to the file
                    iniPath = e.FullPath;
                    if (handleTimer != null)
                        handleTimer.Stop();
                    handleTimer = new System.Timers.Timer();
                    handleTimer.AutoReset = false;
                    handleTimer.Interval = 200; // 200 ms
                    handleTimer.Elapsed += new System.Timers.ElapsedEventHandler(handleTimer_Elapsed);
                    handleTimer.Enabled = true;

                };
                watcher.EnableRaisingEvents = true;
            }
        }
        void handleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                FileChangeEvent(iniPath);
            });
        }

        private void StopWatchingFiles()
        {
            if (watcher!=null)
                watcher.EnableRaisingEvents = false;
        }

        public virtual void PlayVideoOrPlaylist(string path)
        {
            Process.Start(PlayerPath, '"' + path + '"');
        }


        public virtual void PlayUrl(string url, List<string> subtitlespath)
        {
            string init = '"' + url + '"';
            if (subtitlespath != null && subtitlespath.Count > 0)
            {
                foreach (string s in subtitlespath)
                {
                    init += " /sub \"" + s + "\"";
                }
            }
            Process.Start(PlayerPath, init);
        }
    }
}
