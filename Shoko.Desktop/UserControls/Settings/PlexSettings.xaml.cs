using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Shoko.Desktop.ViewModel;
using Shoko.Models.Plex.Connections;
using Shoko.Models.Plex.Libraries;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for PlexSettings.xaml
    /// </summary>
    public partial class PlexSettings : UserControl
    {
        private BackgroundWorker worker;
        private VM_PlexSettings _settings;
        public PlexSettings()
        {
            InitializeComponent();
            
            cbServer.SelectionChanged += (sender, args) =>
            {
                if (cbServer.SelectedItem == null) return;

                VM_ShokoServer.Instance.ShokoPlex.UseDevice(VM_ShokoServer.Instance.CurrentUser.JMMUserID, cbServer.SelectedItem as MediaDevice);
                _settings.UpdateDirectories();
            };

            lstPlexIDs.SelectionChanged += (sender, args) =>
            {
                VM_ShokoServer.Instance.ShokoPlex.UseDirectories(VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                    lstPlexIDs.SelectedItems.Cast<Directory>().ToList());
            };

            this.DataContext = _settings = new VM_PlexSettings();
            worker = new BackgroundWorker();
            worker.DoWork += async (o, e) =>
            {
                do Thread.Sleep(TimeSpan.FromSeconds(10));
                while (VM_ShokoServer.Instance.CurrentUser == null);

                await RefreshAsync();
            };
            worker.RunWorkerAsync();

            btnRefresh.Click += RefreshClick;
        }

        private async void RefreshClick(object sender, RoutedEventArgs e) => await RefreshAsync();

        private async Task RefreshAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => _settings.UpdateServers());
                var currentServer = VM_ShokoServer.Instance.ShokoPlex.CurrentDevice(VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (currentServer == null) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    for (var i = 0; i < _settings.PlexDevices.Count; i++)
                    {
                        if (_settings.PlexDevices[i].ClientIdentifier == currentServer.ClientIdentifier)
                            cbServer.SelectedIndex = i;
                    }

                    lstPlexIDs.SelectedItems.Clear();
                    foreach (var itm in lstPlexIDs.Items)
                        if (VM_ShokoServer.Instance.Plex_Sections.Contains(((Directory)itm).Key))
                            lstPlexIDs.SelectedItems.Add(itm);
                });
            });
        }
    }
}
