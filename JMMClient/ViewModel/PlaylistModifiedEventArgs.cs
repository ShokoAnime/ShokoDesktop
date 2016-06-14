using System;

namespace JMMClient.ViewModel
{
    public class PlaylistModifiedEventArgs : EventArgs
    {
        public readonly int? PlaylistID;

        public PlaylistModifiedEventArgs(int? playlistID)
        {
            PlaylistID = playlistID;
        }
    }
}
