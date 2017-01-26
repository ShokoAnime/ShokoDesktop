using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using NLog;
using Shoko.Desktop.Downloads;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_UTorrentHelper : INotifyPropertyChangedExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static VM_UTorrentHelper _instance;
        private static UTorrentHelper uTorrent;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        public static VM_UTorrentHelper Instance => _instance ?? (_instance = new VM_UTorrentHelper());


        public ObservableCollection<Torrent> Torrents { get; set; }
        public ICollectionView ViewTorrents { get; set; }

        private bool credentialsValid;
        public bool CredentialsValid
        {
            get { return credentialsValid; }
            set { credentialsValid = this.SetField(credentialsValid, value); }
        }

        private bool credentialsInvalid = true;
        public bool CredentialsInvalid
        {
            get { return credentialsInvalid; }
            set { credentialsInvalid = this.SetField(credentialsInvalid, value); }
        }

        private string downloadSpeedSummaryFormatted = @"0 KB/sec";
        public string DownloadSpeedSummaryFormatted
        {
            get { return downloadSpeedSummaryFormatted; }
            set { downloadSpeedSummaryFormatted = this.SetField(downloadSpeedSummaryFormatted, value); }
        }

        private string uploadSpeedSummaryFormatted = @"0 KB/sec";
        public string UploadSpeedSummaryFormatted
        {
            get { return uploadSpeedSummaryFormatted; }
            set
            {
                uploadSpeedSummaryFormatted = this.SetField(uploadSpeedSummaryFormatted, value);
            }
        }

        private string connectionStatus = "Not Connected";
        public string ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                connectionStatus = this.SetField(connectionStatus, value);
            }
        }

        private VM_UTorrentHelper()
        {
            Torrents = new ObservableCollection<Torrent>();
            ViewTorrents = CollectionViewSource.GetDefaultView(Torrents);

            ConnectionStatus = "Not Connected";
            uTorrent = new UTorrentHelper();

            uTorrent.ListRefreshedEvent += uTorrent_ListRefreshedEvent;
        }

        public bool ValidateCredentials()
        {
            try
            {
                CredentialsValid = uTorrent.ValidCredentials();
                CredentialsInvalid = !CredentialsValid;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
                return false;
            }

            return true;
        }

        public bool Init()
        {
            try
            {
                CredentialsValid = uTorrent.ValidCredentials();
                CredentialsInvalid = !CredentialsValid;
                ConnectionStatus = "Connecting...";
                uTorrent.Init();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
                return false;
            }

            return true;
        }

        public List<TorrentFile> GetFilesForTorrent(string hash)
        {
            List<TorrentFile> torFiles = new List<TorrentFile>();
            try
            {
                uTorrent.GetFileList(hash, ref torFiles);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
                return torFiles;
            }

            return torFiles;
        }

        public bool AreCredentialsValid()
        {
            return uTorrent.ValidCredentials();
        }

        public bool TestConnection()
        {
            if (!Init()) return false;

            List<Torrent> torrents = new List<Torrent>();
            if (uTorrent.GetTorrentList(ref torrents))
                return true;
            return false;
        }

        void uTorrent_ListRefreshedEvent(ListRefreshedEventArgs ev)
        {
            if (ev.Torrents.Count != Torrents.Count)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    Torrents.Clear();
                    ViewTorrents.Refresh();
                });
            }

            long dSpeed = 0, uSpeed = 0;

            // new or updated torrents
            foreach (Torrent tor in ev.Torrents)
            {
                dSpeed += tor.DownloadSpeed;
                uSpeed += tor.UploadSpeed;

                //logger.Trace(tor.ToString());

                bool foundTorrent = false;
                foreach (Torrent torExisting in Torrents)
                {
                    if (tor.Hash == torExisting.Hash)
                    {
                        foundTorrent = true;

                        // update details
                        torExisting.Availability = tor.Availability;
                        torExisting.Downloaded = tor.Downloaded;
                        torExisting.DownloadSpeed = tor.DownloadSpeed;
                        torExisting.ETA = tor.ETA;
                        torExisting.Label = tor.Label;
                        torExisting.Name = tor.Name;
                        torExisting.PeersConnected = tor.PeersConnected;
                        torExisting.PeersInSwarm = tor.PeersInSwarm;
                        torExisting.PercentProgress = tor.PercentProgress;
                        torExisting.Ratio = tor.Ratio;
                        torExisting.Remaining = tor.Remaining;
                        torExisting.SeedsConnected = tor.SeedsConnected;
                        torExisting.SeedsInSwarm = tor.SeedsInSwarm;
                        torExisting.Size = tor.Size;
                        torExisting.Status = tor.Status;
                        torExisting.TorrentQueueOrder = tor.TorrentQueueOrder;
                        torExisting.Uploaded = tor.Uploaded;
                        torExisting.UploadSpeed = tor.UploadSpeed;

                        break;
                    }
                }

                if (!foundTorrent)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                        Torrents.Add(tor);
                    });
                }
            }

            DownloadSpeedSummaryFormatted = Utils.FormatByteSize(dSpeed) + "/sec";
            UploadSpeedSummaryFormatted = Utils.FormatByteSize(uSpeed) + "/sec";

            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                ViewTorrents.Refresh();
            });
        }

        public void RefreshTorrents()
        {
            // this will trigger the even, so need to handle the return torrents
            List<Torrent> torrents = new List<Torrent>();
            uTorrent.GetTorrentList(ref torrents);
        }

        public void StopTorrent(string hash)
        {
            uTorrent.StopTorrent(hash);
        }

        public void StartTorrent(string hash)
        {
            uTorrent.StartTorrent(hash);
        }

        public void PauseTorrent(string hash)
        {
            uTorrent.PauseTorrent(hash);
        }

        public void AddTorrentFromURL(string downloadURL)
        {
            uTorrent.AddTorrentFromURL(downloadURL);
        }

        public void RemoveTorrent(string hash)
        {
            uTorrent.RemoveTorrent(hash);
        }

        public void RemoveTorrentAndData(string hash)
        {
            uTorrent.RemoveTorrentAndData(hash);
        }

    }
}
