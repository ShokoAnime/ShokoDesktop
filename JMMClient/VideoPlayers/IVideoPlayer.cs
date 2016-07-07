using System.Collections.Generic;
using JMMClient.Utilities;

namespace JMMClient.VideoPlayers
{
    public interface IVideoPlayer
    {
        void PlayVideoOrPlaylist(string path);
        void PlayUrl(string url, List<string> subtitlespath);
        void Init();
        bool Active { get; }
        VideoPlayer Player { get; }
        event BaseVideoPlayer.FilesPositionsHandler PositionChange;
    }
}
