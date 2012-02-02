using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class PlaylistVM : GongSolutions.Wpf.DragDrop.IDropTarget, INotifyPropertyChanged
	{
		public int? PlaylistID { get; set; }
		public string PlaylistItems { get; set; }
		public int DefaultPlayOrder { get; set; }
		public int PlayWatched { get; set; }
		public int PlayUnwatched { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public ObservableCollection<PlaylistItemVM> PlaylistObjects { get; set; }

		private AniDB_AnimeVM aniDB_Anime = null;
		public AniDB_AnimeVM AniDB_Anime
		{
			get { return aniDB_Anime; }
			set
			{
				aniDB_Anime = value;
				NotifyPropertyChanged("AniDB_Anime");
			}
		}

		private AnimeSeriesVM series = null;
		public AnimeSeriesVM Series
		{
			get { return series; }
			set
			{
				series = value;
				NotifyPropertyChanged("Series");
			}
		}

		private Boolean isReadOnly = true;
		public Boolean IsReadOnly
		{
			get { return isReadOnly; }
			set
			{
				isReadOnly = value;
				NotifyPropertyChanged("IsReadOnly");
			}
		}

		private Boolean isBeingEdited = false;
		public Boolean IsBeingEdited
		{
			get { return isBeingEdited; }
			set
			{
				isBeingEdited = value;
				NotifyPropertyChanged("IsBeingEdited");
			}
		}

		private string playlistName = "";
		public string PlaylistName
		{
			get { return playlistName; }
			set
			{
				playlistName = value;
				NotifyPropertyChanged("PlaylistName");
			}
		}

		private Boolean playWatchedBool = true;
		public bool PlayWatchedBool
		{
			get { return playWatchedBool; }
			set
			{
				playWatchedBool = value;
				NotifyPropertyChanged("PlayWatchedBool");
			}
		}

		private Boolean playUnwatchedBool = true;
		public bool PlayUnwatchedBool
		{
			get { return playUnwatchedBool; }
			set
			{
				playUnwatchedBool = value;
				NotifyPropertyChanged("PlayUnwatchedBool");
			}
		}

		public void SetDependendProperties()
		{
			AniDB_Anime = null;
			Series = null;

			if (PlaylistObjects.Count == 0) return;

			PlaylistItemVM plItem = PlaylistObjects[0];


			if (plItem.ItemType == PlaylistItemType.Episode)
			{
				AnimeEpisodeVM aniep = plItem.PlaylistItem as AnimeEpisodeVM;
				aniep.RefreshAnime();
				AniDB_Anime = aniep.AniDB_Anime;

				if (MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(aniep.AnimeSeriesID))
					Series = MainListHelperVM.Instance.AllSeriesDictionary[aniep.AnimeSeriesID];
			}
			if (plItem.ItemType == PlaylistItemType.AnimeSeries)
			{
				AnimeSeriesVM ser = plItem.PlaylistItem as AnimeSeriesVM;
				AniDB_Anime = ser.AniDB_Anime;
				Series = ser;
			}
		}

		public PlaylistPlayOrder DefaultPlayOrderEnum
		{
			get
			{
				return (PlaylistPlayOrder)DefaultPlayOrder;
			}
		}

		

		public PlaylistVM(JMMServerBinary.Contract_Playlist contract)
		{
			PlaylistObjects = new ObservableCollection<PlaylistItemVM>();

			Populate(contract);
			//PopulatePlaylistObjects();
		}

		void GongSolutions.Wpf.DragDrop.IDropTarget.DragOver(DropInfo dropInfo)
		{
			PlaylistItemVM sourceItem = dropInfo.Data as PlaylistItemVM;
			PlaylistItemVM targetItem = dropInfo.TargetItem as PlaylistItemVM;

			if (sourceItem != null && targetItem != null)
			{
				dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
				dropInfo.Effects = DragDropEffects.Copy;
			}
		}

		void GongSolutions.Wpf.DragDrop.IDropTarget.Drop(DropInfo dropInfo)
		{
			PlaylistItemType itemType = PlaylistItemType.AnimeSeries;
			int objIDOld = -1;

			PlaylistItemVM pli = dropInfo.Data as PlaylistItemVM;
			if (pli == null) return;

			if (pli.ItemType == PlaylistItemType.Episode)
			{
				AnimeEpisodeVM ep = (AnimeEpisodeVM)pli.PlaylistItem;
				itemType = PlaylistItemType.Episode;
				objIDOld = ep.AnimeEpisodeID;
			}
			if (pli.ItemType == PlaylistItemType.AnimeSeries)
			{
				AnimeSeriesVM ep = (AnimeSeriesVM)pli.PlaylistItem;
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

			if (string.IsNullOrEmpty(newItemList))
			{
				// means there was only one item in list to begin with
				PlaylistItems += string.Format("{0};{1}", iType, objIDOld);
			}
			else
			{
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
			}

			// moved to the end of the list
			if (dropInfo.InsertIndex > items.Length)
			{
				if (PlaylistItems.Length > 0) PlaylistItems += "|";
				PlaylistItems += string.Format("{0};{1}", iType, objIDOld);
			}

			Save();
			PopulatePlaylistObjects();

			PlaylistHelperVM.Instance.OnPlaylistModified(new PlaylistModifiedEventArgs(PlaylistID));
		}

		public void Save()
		{
			JMMServerBinary.Contract_Playlist pl = new JMMServerBinary.Contract_Playlist();
			pl.PlaylistID = this.PlaylistID;
			pl.DefaultPlayOrder = this.DefaultPlayOrder;
			pl.PlaylistItems = this.PlaylistItems;
			pl.PlaylistName = this.PlaylistName;
			pl.PlayUnwatched = this.PlayUnwatched;
			pl.PlayWatched = this.PlayWatched;
			JMMServerBinary.Contract_Playlist_SaveResponse resp = JMMServerVM.Instance.clientBinaryHTTP.SavePlaylist(pl);

			if (!string.IsNullOrEmpty(resp.ErrorMessage))
			{
				Utils.ShowErrorMessage(resp.ErrorMessage);
				return;
			}

			this.PlayWatchedBool = PlayWatched == 1;
			this.PlayUnwatchedBool = PlayUnwatched == 1;
		}

		public void AddSeries(int animeSeriesID)
		{
			if (IsAlreadyInPlaylist(PlaylistItemType.AnimeSeries, animeSeriesID)) return;

			if (!string.IsNullOrEmpty(PlaylistItems))
				PlaylistItems += "|";

			PlaylistItems += string.Format("{0};{1}", (int)PlaylistItemType.AnimeSeries, animeSeriesID);
		}

		public void RemoveSeries(int animeSeriesID)
		{
			if (string.IsNullOrEmpty(this.PlaylistItems)) return;

			string[] items = this.PlaylistItems.Split('|');
			this.PlaylistItems = "";

			// create a new list without the moved item
			foreach (string pitem in items)
			{
				string[] parms = pitem.Split(';');
				if (parms.Length != 2) continue;

				int objType = -1;
				int objID = -1;

				if (!int.TryParse(parms[0], out objType)) continue;
				if (!int.TryParse(parms[1], out objID)) continue;

				if (objType == (int)PlaylistItemType.AnimeSeries && objID == animeSeriesID)
				{
					// remove this old item
				}
				else
				{
					if (PlaylistItems.Length > 0) PlaylistItems += "|";
					PlaylistItems += string.Format("{0};{1}", objType, objID);
				}
			}
		}

		public void AddEpisode(int animeEpisodeID)
		{
			if (IsAlreadyInPlaylist(PlaylistItemType.Episode, animeEpisodeID)) return;

			if (!string.IsNullOrEmpty(PlaylistItems))
				PlaylistItems += "|";

			PlaylistItems += string.Format("{0};{1}", (int)PlaylistItemType.Episode, animeEpisodeID);
		}

		public void RemoveEpisode(int animeEpisodeID)
		{
			if (string.IsNullOrEmpty(this.PlaylistItems)) return;

			string[] items = this.PlaylistItems.Split('|');
			this.PlaylistItems = "";

			// create a new list without the moved item
			foreach (string pitem in items)
			{
				string[] parms = pitem.Split(';');
				if (parms.Length != 2) continue;

				int objType = -1;
				int objID = -1;

				if (!int.TryParse(parms[0], out objType)) continue;
				if (!int.TryParse(parms[1], out objID)) continue;

				if (objType == (int)PlaylistItemType.Episode && objID == animeEpisodeID)
				{
					// remove this old item
				}
				else
				{
					if (PlaylistItems.Length > 0) PlaylistItems += "|";
					PlaylistItems += string.Format("{0};{1}", objType, objID);
				}
			}
		}

		public bool IsAlreadyInPlaylist(PlaylistItemType pType, int id)
		{
			string[] items = this.PlaylistItems.Split('|');

			foreach (string pitem in items)
			{
				string[] parms = pitem.Split(';');
				if (parms.Length != 2) continue;

				int objType = -1;
				int objID = -1;

				if (!int.TryParse(parms[0], out objType)) continue;
				if (!int.TryParse(parms[1], out objID)) continue;

				if (objType == (int)pType && objID == id)
					return true;
				
			}

			return false;
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
						PlaylistObjects.Add(new PlaylistItemVM(this.PlaylistID.Value, PlaylistItemType.AnimeSeries, ser));
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
						//ep.SetTvDBInfo();
						PlaylistObjects.Add(new PlaylistItemVM(this.PlaylistID.Value, PlaylistItemType.Episode, ep));
					}
				}
			}

			SetDependendProperties();
		}

		public void Populate(JMMServerBinary.Contract_Playlist contract)
		{
			this.PlaylistID = contract.PlaylistID;
			this.PlaylistName = contract.PlaylistName;
			this.PlaylistItems = contract.PlaylistItems;
			this.DefaultPlayOrder = contract.DefaultPlayOrder;
			this.PlayWatched = contract.PlayWatched;
			this.PlayUnwatched = contract.PlayUnwatched;

			this.PlayWatchedBool = PlayWatched == 1;
			this.PlayUnwatchedBool = PlayUnwatched == 1;
		}
	}
}
