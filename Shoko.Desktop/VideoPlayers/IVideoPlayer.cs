using Shoko.Desktop.Enums;

namespace Shoko.Desktop.VideoPlayers
{
    public interface IVideoPlayer
    {
        void Play(VideoInfo video);
        void Init();
        bool Active { get; }
        VideoPlayer Player { get; }
        event BaseVideoPlayer.FilesPositionsHandler FilePositionsChange;
        event BaseVideoPlayer.FilePositionHandler VideoInfoChange;
    }
}
