using System.Collections.Generic;
using JMMClient.Utilities;

namespace JMMClient.VideoPlayers
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
