using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_PlaylistHelper :INotifyPropertyChanged, INotifyPropertyChangedExt
    {

        private static VM_PlaylistHelper _instance;

        public ObservableCollection<object> CurrentPlaylistObjects { get; set; }
        public ICollectionView ViewCurrentPlaylistObjects { get; set; }

        public ObservableCollection<VM_Playlist> Playlists { get; set; }
        public ICollectionView ViewPlaylists { get; set; }

        public delegate void PlaylistModifiedHandler(PlaylistModifiedEventArgs ev);
        public event PlaylistModifiedHandler OnPlaylistModifiedEvent;
        public void OnPlaylistModified(PlaylistModifiedEventArgs ev)
        {
            OnPlaylistModifiedEvent?.Invoke(ev);
        }

        private Boolean isLoadingData = true;
        public Boolean IsLoadingData
        {
            get { return isLoadingData; }
            set
            {
                isLoadingData = this.SetField(isLoadingData, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public static VM_PlaylistHelper Instance => _instance ?? (_instance = new VM_PlaylistHelper());

        private VM_PlaylistHelper()
        {
            CurrentPlaylistObjects = new ObservableCollection<object>();
            ViewCurrentPlaylistObjects = CollectionViewSource.GetDefaultView(CurrentPlaylistObjects);

            Playlists = new ObservableCollection<VM_Playlist>();
            ViewPlaylists = CollectionViewSource.GetDefaultView(Playlists);
        }

        public void RefreshData()
        {
            try
            {
                IsLoadingData = true;

                // clear all displayed data
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    Playlists.Clear();
                    CurrentPlaylistObjects.Clear();

                    ViewCurrentPlaylistObjects.Refresh();
                    ViewPlaylists.Refresh();
                });

                // load the playlists
                List<VM_Playlist> rawPlaylists = VM_ShokoServer.Instance.ShokoServices.GetAllPlaylists().CastList<VM_Playlist>();
                foreach (VM_Playlist contract in rawPlaylists)
                {
                    Playlists.Add(contract);
                }



                IsLoadingData = false;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public static VM_Playlist CreatePlaylist(Window owner)
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                DialogText dlg = new DialogText();
                dlg.Init(Shoko.Commons.Properties.Resources.Playlist_Name + " ", "");
                dlg.Owner = owner;
                bool? res = dlg.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    if (string.IsNullOrEmpty(dlg.EnteredText))
                    {
                        Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.Playlist_NameBlank);
                        return null;
                    }

                    Playlist pl = new Playlist
                    {
                        DefaultPlayOrder = (int) PlaylistPlayOrder.Sequential,
                        PlaylistItems = "",
                        PlaylistName = dlg.EnteredText,
                        PlayUnwatched = 1,
                        PlayWatched = 0
                    };
                    CL_Response<Playlist> resp = VM_ShokoServer.Instance.ShokoServices.SavePlaylist(pl);

                    if (!string.IsNullOrEmpty(resp.ErrorMessage))
                    {
                        Utils.ShowErrorMessage(resp.ErrorMessage);
                        return null;
                    }

                    // refresh data
                    Instance.RefreshData();
                    return (VM_Playlist)resp.Result;
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
