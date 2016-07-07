using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class MPVVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {
        public override void Init()
        {
            PlayerPath = Utils.CheckSysPath(new string[] {"mpv.exe"});
            if (string.IsNullOrEmpty(PlayerPath))
            {
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Applications\mpv.exe\shell\open\command", "", null);
                if (!string.IsNullOrEmpty(PlayerPath))
                {
                    int idx = PlayerPath.IndexOf("\"", 1);
                    PlayerPath = PlayerPath.Substring(1, idx - 1);
                }
            }


            if (string.IsNullOrEmpty(PlayerPath))
            {
                Active = false;
                return;
            }
            Active = true;
        }

        public VideoPlayer Player => VideoPlayer.MPV;

        public override void PlayUrl(string url, List<string> subtitlespath)
        {
            string init = '"' + url + '"';
            if (subtitlespath != null && subtitlespath.Count > 0)
            {
                foreach (string s in subtitlespath)
                {
                    init += " --sub-file=\"" + s + "\"";
                }
            }
            Process.Start(PlayerPath, init);
        }


        internal override void FileChangeEvent(string filePath)
        {
        }
    }
}
