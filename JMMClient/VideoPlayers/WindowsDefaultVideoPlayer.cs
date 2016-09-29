using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class WindowsDefaultVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {

        public override void Play(VideoInfo video)
        {
            if (IsPlaying)
                return;
            Task.Factory.StartNew(() =>
            {
                Process process;
                process = Process.Start(video.Uri);
                process.WaitForExit();
                StopWatcher();
                IsPlaying = false;

            });
        }

        internal override void FileChangeEvent(string filePath)
        {
            // Not used
        }

        public override void Init()
        {
            Active = true;

        }

        public VideoPlayer Player => VideoPlayer.WindowsDefault;
    }
}
