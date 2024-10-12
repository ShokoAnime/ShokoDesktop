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
using DevExpress.Data.PLinq.Helpers;
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
            worker.DoWork += (o, e) =>
            {
                do Thread.Sleep(TimeSpan.FromSeconds(10));
                while (VM_ShokoServer.Instance.CurrentUser == null);

                RefreshAsync();
            };
            worker.RunWorkerAsync();

            btnRefresh.Click += RefreshClick;
        }

        private async void RefreshClick(object sender, RoutedEventArgs e) => await Task.Run(RefreshAsync);

        private void RefreshAsync()
        {
            _settings.UpdateServers();
            var currentServer = VM_ShokoServer.Instance.ShokoPlex.CurrentDevice(VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            if (currentServer == null) return;

            var index = -1;
            for (var i = 0; i < _settings.PlexDevices.Count; i++)
            {
                if (_settings.PlexDevices[i].ClientIdentifier == currentServer.ClientIdentifier)
                    index = i;
            }

            if (index != -1)
                Application.Current.Dispatcher.Invoke(() => cbServer.SelectedIndex = index);

            var toSelect = lstPlexIDs.Items.Cast<Directory>()
                .Where(itm => VM_ShokoServer.Instance.Plex_Sections.Contains(itm.Key)).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                lstPlexIDs.SelectedItems.Clear();
                foreach (var directory in toSelect)
                {
                    lstPlexIDs.SelectedItems.Add(directory);
                }
            });
        }
    }
}
