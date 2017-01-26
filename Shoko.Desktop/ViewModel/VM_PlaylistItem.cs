using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_PlaylistItem 
    {
        public int PlaylistID { get; set; }
        public PlaylistItemType ItemType { get; set; }
        public object PlaylistItem { get; set; }

        public VM_AnimeSeries_User Series { get; set; }
        public VM_AnimeEpisode_User Episode { get; set; }

        public VM_PlaylistItem()
        {
        }

        public VM_PlaylistItem(int playlistID, PlaylistItemType itemType, object playlistItem)
        {
            PlaylistID = playlistID;
            ItemType = itemType;
            PlaylistItem = playlistItem;

            if (ItemType == PlaylistItemType.AnimeSeries) Series = PlaylistItem as VM_AnimeSeries_User;

            if (ItemType == PlaylistItemType.Episode)
            {
                Episode = (VM_AnimeEpisode_User) PlaylistItem;
                if (VM_MainListHelper.Instance.AllSeriesDictionary.ContainsKey(Episode.AnimeSeriesID))
                    Series = VM_MainListHelper.Instance.AllSeriesDictionary[Episode.AnimeSeriesID];
            }
        }
    }
}
