using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Data;
using JMMClient.ViewModel;

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
	}
}
