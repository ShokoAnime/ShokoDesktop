using System;

namespace Shoko.Desktop.ViewModel.Helpers
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
