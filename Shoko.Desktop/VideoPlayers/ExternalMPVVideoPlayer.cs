using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Win32;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Utilities;

namespace Shoko.Desktop.VideoPlayers
{
    public class ExternalMPVVideoPlayer : BaseVideoPlayer, IVideoPlayer
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

        public VideoPlayer Player => VideoPlayer.ExternalMPV;

        public override void Play(VideoInfo video)
        {
            if (IsPlaying)
                return;
            Task.Factory.StartNew(() =>
            {
                Process process;
                if (video.IsPlaylist)
                    process = Process.Start(new ProcessStartInfo
                    {
                        FileName = PlayerPath,
                        Arguments = '"' + video.Uri + ' '+ video.VideoDetailed.FileName,
                        UseShellExecute = true
                    });
                else
                {
                    string init = '"' + video.Uri + ' ' + video.VideoDetailed.FileName;
                    if (video.ResumePosition > 0)
                    {
                        double n = video.ResumePosition;
                        n /= 1000;
                        init += " --start=\"" + n.ToString(CultureInfo.InvariantCulture) + "\"";
                    }
                    if (video.SubtitlePaths != null && video.SubtitlePaths.Count > 0)
                    {
                        foreach (string s in video.SubtitlePaths)
                        {
                            init += " --sub-file=\"" + s + "\"";
                        }
                    }
                    process = Process.Start(new ProcessStartInfo
                    {
                        FileName = PlayerPath,
                        Arguments = init,
                        UseShellExecute = true
                    });
                }
                if (process != null)
                {
                    IsPlaying = true;
                    process.WaitForExit();
                    IsPlaying = false;
                }
            });
        }

        internal override void FileChangeEvent(string filePath)
        {
        }
    }
}
