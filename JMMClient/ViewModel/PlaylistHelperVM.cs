using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Data;
using JMMClient.ViewModel;
using JMMClient.Forms;
using System.Windows;
using System.Threading;
using System.Globalization;

namespace JMMClient
{
	public class PlaylistHelperVM : INotifyPropertyChanged
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static PlaylistHelperVM _instance;

		public ObservableCollection<object> CurrentPlaylistObjects { get; set; }
		public ICollectionView ViewCurrentPlaylistObjects { get; set; }

		public ObservableCollection<PlaylistVM> Playlists { get; set; }
		public ICollectionView ViewPlaylists { get; set; }

		public delegate void PlaylistModifiedHandler(PlaylistModifiedEventArgs ev);
		public event PlaylistModifiedHandler OnPlaylistModifiedEvent;
		public void OnPlaylistModified(PlaylistModifiedEventArgs ev)
		{
			if (OnPlaylistModifiedEvent != null)
			{
				OnPlaylistModifiedEvent(ev);
			}
		}

		private Boolean isLoadingData = true;
		public Boolean IsLoadingData
		{
			get { return isLoadingData; }
			set
			{
				isLoadingData = value;
				NotifyPropertyChanged("IsLoadingData");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public static PlaylistHelperVM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new PlaylistHelperVM();
				}
				return _instance;
			}
		}

		private PlaylistHelperVM()
		{
			CurrentPlaylistObjects = new ObservableCollection<object>();
			ViewCurrentPlaylistObjects = CollectionViewSource.GetDefaultView(CurrentPlaylistObjects);

			Playlists = new ObservableCollection<PlaylistVM>();
			ViewPlaylists = CollectionViewSource.GetDefaultView(Playlists);
		}

		public void RefreshData()
		{
			try
			{
				IsLoadingData = true;

				// clear all displayed data
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					Playlists.Clear();
					CurrentPlaylistObjects.Clear();

					ViewCurrentPlaylistObjects.Refresh();
					ViewPlaylists.Refresh();
				});

				// load the playlists
				List<JMMServerBinary.Contract_Playlist> rawPlaylists = JMMServerVM.Instance.clientBinaryHTTP.GetAllPlaylists();
				foreach (JMMServerBinary.Contract_Playlist contract in rawPlaylists)
				{
					PlaylistVM pl = new PlaylistVM(contract);
					Playlists.Add(pl);
				}

				

				IsLoadingData = false;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public static PlaylistVM CreatePlaylist(Window owner)
		{
			try
			{
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                DialogText dlg = new DialogText();
				dlg.Init(Properties.Resources.Playlist_Name + " ", "");
				dlg.Owner = owner;
				bool? res = dlg.ShowDialog();
				if (res.HasValue && res.Value)
				{
					if (string.IsNullOrEmpty(dlg.EnteredText))
					{
						Utils.ShowErrorMessage(Properties.Resources.Playlist_NameBlank);
						return null;
					}

					JMMServerBinary.Contract_Playlist pl = new JMMServerBinary.Contract_Playlist();
					pl.DefaultPlayOrder = (int)PlaylistPlayOrder.Sequential;
					pl.PlaylistItems = "";
					pl.PlaylistName = dlg.EnteredText;
					pl.PlayUnwatched = 1;
					pl.PlayWatched = 0;
					JMMServerBinary.Contract_Playlist_SaveResponse resp = JMMServerVM.Instance.clientBinaryHTTP.SavePlaylist(pl);

					if (!string.IsNullOrEmpty(resp.ErrorMessage))
					{
						Utils.ShowErrorMessage(resp.ErrorMessage);
						return null;
					}

					// refresh data
					PlaylistHelperVM.Instance.RefreshData();

					PlaylistVM plRet = new PlaylistVM(resp.Playlist);
					return plRet;
				}

				return null;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
				return null;
			}
		}
	}
}
