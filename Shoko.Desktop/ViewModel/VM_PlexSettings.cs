using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Plex.Connections;
using Shoko.Models.Plex.Libraries;

namespace Shoko.Desktop.ViewModel
{
    public class VM_PlexSettings : INotifyPropertyChangedExt
    {
        private ObservableCollectionEx<Directory> _plexDirectories;
        private ObservableCollectionEx<MediaDevice> _plexDevices;
        private bool _isAuthenticated;

        public VM_PlexSettings()
        {
            _plexDirectories = new ObservableCollectionEx<Directory>();
            _plexDevices = new ObservableCollectionEx<MediaDevice>();

            //PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PlexDirectories):
                    break;
                case nameof(PlexDevices):
                    break;
            }
        }

        public ObservableCollectionEx<Directory> PlexDirectories
        {
            get => _plexDirectories;
            set { _plexDirectories = value; NotifyPropertyChanged(); }
        }

        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
            set { _isAuthenticated = value; }
        }

        public ObservableCollectionEx<MediaDevice> PlexDevices
        {
            get => _plexDevices;
            set
            {
                _plexDevices = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(PlexDirectories));
                UpdateDirectories();
            }
        }

        public void UpdateDirectories()
        {
            _plexDirectories.ReplaceRange(
                VM_ShokoServer.Instance.ShokoPlex.Directories(VM_ShokoServer.Instance.CurrentUser.JMMUserID));
            NotifyPropertyChanged(nameof(PlexDirectories));
        }

        public void UpdateServers()
        {
            var devices = VM_ShokoServer.Instance.ShokoPlex.AvailableDevices(VM_ShokoServer.Instance.CurrentUser.JMMUserID);

            Application.Current.Dispatcher.Invoke(() => _plexDevices.ReplaceRange(devices));
            NotifyPropertyChanged(nameof(PlexDirectories));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propname = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
