using Shoko.Desktop.VideoPlayers.mpv;

namespace Shoko.Desktop.VideoPlayers
{
    public class MPVVideoPlayer : IVideoPlayer
    {
        private PlayerForm form;
        public void Play(VideoInfo video)
        {
            form =new PlayerForm(this);
            form.FilePosition += (v, position) =>
            {
                VideoInfoChange?.Invoke(v, position);
            };
            form.Show();
            
            form.Play(video);
        }

        public void Init()
        {
            Active = true;
        }

  
        public bool Active { get; set;  }
        public Enums.VideoPlayer Player => Enums.VideoPlayer.MPV;
        public event BaseVideoPlayer.FilesPositionsHandler FilePositionsChange;
        public event BaseVideoPlayer.FilePositionHandler VideoInfoChange;

        public void Stop()
        {
            form.Stop();
        }



        
        public void Quit()
        {
            form.Quit();
            form = null;
        }
    }
}
