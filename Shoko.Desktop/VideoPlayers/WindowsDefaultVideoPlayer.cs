using System.Diagnostics;
using System.Threading.Tasks;
using Shoko.Desktop.Enums;

namespace Shoko.Desktop.VideoPlayers
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
