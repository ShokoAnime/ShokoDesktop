using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class PlaylistItemVM //: INotifyPropertyChanged
	{
		public int PlaylistID { get; set; }
		public PlaylistItemType ItemType { get; set; }
		public object PlaylistItem { get; set; }

		public AnimeSeriesVM Series { get; set; }
		public AnimeEpisodeVM Episode { get; set; }

		public PlaylistItemVM()
		{
		}

		public PlaylistItemVM(int playlistID, PlaylistItemType itemType, object playlistItem)
		{
			PlaylistID = playlistID;
			ItemType = itemType;
			PlaylistItem = playlistItem;

			if (ItemType == PlaylistItemType.AnimeSeries) Series = PlaylistItem as AnimeSeriesVM;

			if (ItemType == PlaylistItemType.Episode)
			{
				Episode = PlaylistItem as AnimeEpisodeVM;
				if (MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(Episode.AnimeSeriesID))
					Series = MainListHelperVM.Instance.AllSeriesDictionary[Episode.AnimeSeriesID];
			}
		}
	}
}
