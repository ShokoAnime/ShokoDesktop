using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using GongSolutions.Wpf.DragDrop;
using System.Windows;

namespace JMMClient.ViewModel
{
	public class PlaylistVM : GongSolutions.Wpf.DragDrop.IDropTarget
	{
		public int? PlaylistID { get; set; }
		public string PlaylistName { get; set; }
		public string PlaylistItems { get; set; }
		public int DefaultPlayOrder { get; set; }
		public int PlayWatched { get; set; }
		public int PlayUnwatched { get; set; }

		public ObservableCollection<object> PlaylistObjects { get; set; }

		public PlaylistPlayOrder DefaultPlayOrderEnum
		{
			get
			{
				return (PlaylistPlayOrder)DefaultPlayOrder;
			}
		}

		public bool PlayWatchedBool
		{
			get
			{
				return PlayWatched == 1;
			}
		}

		public bool PlayUnwatchedBool
		{
			get
			{
				return PlayUnwatched == 1;
			}
		}

		public PlaylistVM(JMMServerBinary.Contract_Playlist contract)
		{
			PlaylistObjects = new ObservableCollection<object>();

			Populate(contract);
			//PopulatePlaylistObjects();
		}

		void GongSolutions.Wpf.DragDrop.IDropTarget.DragOver(DropInfo dropInfo)
		{
			AnimeSeriesVM sourceItem = dropInfo.Data as AnimeSeriesVM;
			AnimeSeriesVM targetItem = dropInfo.TargetItem as AnimeSeriesVM;

			if (sourceItem != null && targetItem != null)
			{
				dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
				dropInfo.Effects = DragDropEffects.Copy;
			}
		}

		void GongSolutions.Wpf.DragDrop.IDropTarget.Drop(DropInfo dropInfo)
		{
			PlaylistItemType itemType = PlaylistItemType.AnimeSeries;
			int objIDOld = -1;

			if (dropInfo.Data is AnimeEpisodeVM)
			{
				AnimeEpisodeVM ep = (AnimeEpisodeVM)dropInfo.Data;
				itemType = PlaylistItemType.Episode;
				objIDOld = ep.AnimeEpisodeID;
			}
			if (dropInfo.Data is AnimeSeriesVM)
			{
				AnimeSeriesVM ep = (AnimeSeriesVM)dropInfo.Data;
				itemType = PlaylistItemType.AnimeSeries;
				objIDOld = ep.AnimeSeriesID.Value;
			}

			int iType = (int)itemType;

			// find where this item was previously

			if (string.IsNullOrEmpty(this.PlaylistItems)) return;

			string[] items = this.PlaylistItems.Split('|');

			// create a new list without the moved item
			string newItemList = "";
			foreach (string pitem in items)
			{
				string[] parms = pitem.Split(';');
				if (parms.Length != 2) continue;

				int objType = -1;
				int objID = -1;

				if (!int.TryParse(parms[0], out objType)) continue;
				if (!int.TryParse(parms[1], out objID)) continue;

				if (objType == iType && objID == objIDOld)
				{
					// skip the old item
				}
				else
				{
					if (newItemList.Length > 0) newItemList += "|";
					newItemList += string.Format("{0};{1}", objType, objID);
				}
			}

			// insert the moved item into it's new position
			items = newItemList.Split('|');

			this.PlaylistItems = "";
			int curPos = 0;
			foreach (string pitem in items)
			{
				string[] parms = pitem.Split(';');
				if (parms.Length != 2) continue;

				int objType = -1;
				int objID = -1;

				int.TryParse(parms[0], out objType);
				int.TryParse(parms[1], out objID);

				if (curPos == dropInfo.InsertIndex)
				{
					// insert moved item
					if (PlaylistItems.Length > 0) PlaylistItems += "|";
					PlaylistItems += string.Format("{0};{1}", iType, objIDOld);
				}


				if (PlaylistItems.Length > 0) PlaylistItems += "|";
				PlaylistItems += string.Format("{0};{1}", objType, objID);

				curPos++;
			}

			// moved to the end of the list
			if (dropInfo.InsertIndex > items.Length)
			{
				if (PlaylistItems.Length > 0) PlaylistItems += "|";
				PlaylistItems += string.Format("{0};{1}", iType, objIDOld);
			}

			Save();
			PopulatePlaylistObjects();
		}

		private void Save()
		{
			JMMServerBinary.Contract_Playlist pl = new JMMServerBinary.Contract_Playlist();
			pl.PlaylistID = this.PlaylistID;
			pl.DefaultPlayOrder = this.DefaultPlayOrder;
			pl.PlaylistItems = this.PlaylistItems;
			pl.PlaylistName = this.PlaylistName;
			pl.PlayUnwatched = this.PlayUnwatched;
			pl.PlayWatched = this.PlayWatched;
			string result = JMMServerVM.Instance.clientBinaryHTTP.SavePlaylist(pl);

			if (!string.IsNullOrEmpty(result))
			{
				Utils.ShowErrorMessage(result);
				return;
			}
		}


		public void PopulatePlaylistObjects()
		{
			PlaylistObjects.Clear();

			if (string.IsNullOrEmpty(this.PlaylistItems)) return;

			string[] items = this.PlaylistItems.Split('|');
			foreach (string pitem in items)
			{
				string[] parms = pitem.Split(';');
				if (parms.Length != 2) continue;

				int objType = -1;
				int objID = -1;

				if (!int.TryParse(parms[0], out objType)) continue;
				if (!int.TryParse(parms[1], out objID)) continue;

				if ((PlaylistItemType)objType == PlaylistItemType.AnimeSeries)
				{
					// get the series
					JMMServerBinary.Contract_AnimeSeries serContract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(objID, 
						JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (serContract != null)
					{
						AnimeSeriesVM ser = new AnimeSeriesVM(serContract);
						PlaylistObjects.Add(ser);
					}
				}
				else
				{
					// get the episode
					JMMServerBinary.Contract_AnimeEpisode epContract = JMMServerVM.Instance.clientBinaryHTTP.GetEpisode(objID,
						JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (epContract != null)
					{
						AnimeEpisodeVM ep = new AnimeEpisodeVM(epContract);
						PlaylistObjects.Add(ep);
					}
				}
			}
		}

		public void Populate(JMMServerBinary.Contract_Playlist contract)
		{
			this.PlaylistID = contract.PlaylistID;
			this.PlaylistName = contract.PlaylistName;
			this.PlaylistItems = contract.PlaylistItems;
			this.DefaultPlayOrder = contract.DefaultPlayOrder;
			this.PlayWatched = contract.PlayWatched;
			this.PlayUnwatched = contract.PlayUnwatched;
		}
	}
}
