using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Desktop.Downloads;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls.Downloads
{
    /// <summary>
    /// Interaction logic for DownloadsBrowseTorrentsControl.xaml
    /// </summary>
    public partial class DownloadsBrowseTorrentsControl : UserControl
    {
        BackgroundWorker torrentDetailsWorker = new BackgroundWorker();

        public ICollectionView ViewTorrentLinks { get; set; }
        public ObservableCollection<TorrentLinkVM> TorrentLinks { get; set; }

        public static readonly DependencyProperty TorrentCountProperty = DependencyProperty.Register("TorrentCount",
            typeof(int), typeof(DownloadsBrowseTorrentsControl), new UIPropertyMetadata(0, null));

        public int TorrentCount
        {
            get { return (int)GetValue(TorrentCountProperty); }
            set { SetValue(TorrentCountProperty, value); }
        }

        public static readonly DependencyProperty HasAttachedSeriesProperty = DependencyProperty.Register("HasAttachedSeries",
            typeof(bool), typeof(DownloadsBrowseTorrentsControl), new UIPropertyMetadata(false, null));

        public bool HasAttachedSeries
        {
            get { return (bool)GetValue(HasAttachedSeriesProperty); }
            set { SetValue(HasAttachedSeriesProperty, value); }
        }

        public DownloadsBrowseTorrentsControl()
        {
            InitializeComponent();

            TorrentLinks = new ObservableCollection<TorrentLinkVM>();
            ViewTorrentLinks = CollectionViewSource.GetDefaultView(TorrentLinks);
            //ViewTorrentLinks.SortDescriptions.Add(new SortDescription("FullPath", ListSortDirection.Ascending));
            ViewTorrentLinks.Filter = LinkSearchFilter;

            btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
            txtFileSearch.TextChanged += new TextChangedEventHandler(txtFileSearch_TextChanged);

            dgTorrents.SelectionChanged += new SelectionChangedEventHandler(dgTorrents_SelectionChanged);
            dgTorrents.MouseLeftButtonUp += new MouseButtonEventHandler(dgTorrents_MouseLeftButtonUp);
            dgTorrents.LoadingRow += new EventHandler<DataGridRowEventArgs>(dgTorrents_LoadingRow);

            torrentDetailsWorker.DoWork += new DoWorkEventHandler(torrentDetailsWorker_DoWork);
            torrentDetailsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(torrentDetailsWorker_RunWorkerCompleted);
        }

        private void CommandBinding_AddSubGroupFilter(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(SubGroupSimple))
                {
                    SubGroupSimple sub = (SubGroupSimple)obj;
                    txtFileSearch.Text = txtFileSearch.Text + " " + sub.GroupNameShort;
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void ShowTorrentDetails(TorrentLinkVM torLink)
        {
            SetAttachedSeries(null);

            if (!torrentDetailsWorker.IsBusy)
                torrentDetailsWorker.RunWorkerAsync(torLink);
        }

        private void SetAttachedSeries(VM_AnimeSeries_User ser)
        {
            try
            {
                HasAttachedSeries = ser != null;

                if (ser == null)
                {
                    DataContext = null;
                    ucFileSummary.DataContext = null;
                    return;
                }

                DataContext = ser;
                ucFileSummary.DataContext = ser.AniDBAnime.AniDBAnime;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        void torrentDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Result as VM_AnimeSeries_User;

            SetAttachedSeries(ser);
        }

        void torrentDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TorrentLinkVM torLink = e.Argument as TorrentLinkVM;

            // try and find the series
            foreach (VM_AniDB_Anime anime in VM_AniDB_Anime.BestLevenshteinDistanceMatchesCache(torLink.ClosestAnimeMatchString, 10))
            {
                // get the series for the anime
                VM_AnimeSeries_User ser = VM_MainListHelper.Instance.GetSeriesForAnime(anime.AnimeID);
                if (ser != null)
                {
                    e.Result = ser;
                    return;
                }
            }

            e.Result = null;
        }

        void dgTorrents_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid _DataGrid = sender as DataGrid;

            TorrentLinkVM torLink = _DataGrid.SelectedItem as TorrentLinkVM;
            if (torLink == null) return;

            ShowTorrentDetails(torLink);
        }

        void dgTorrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid _DataGrid = sender as DataGrid;

            TorrentLinkVM torLink = _DataGrid.SelectedItem as TorrentLinkVM;
            if (torLink == null) return;

            ShowTorrentDetails(torLink);
        }

        void dgTorrents_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseRightButtonDown += new MouseButtonEventHandler(Row_MouseRightButtonDown);
        }

        void Row_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = sender as DataGridRow;
            if (dgr == null) return;

            dgTorrents.SelectedItem = dgr.DataContext;

            ContextMenu m = new ContextMenu();

            TorrentLinkVM torLink = dgTorrents.SelectedItem as TorrentLinkVM;
            if (torLink == null) return;

            MenuItem itemStart = new MenuItem();
            itemStart.Header = "Download";
            itemStart.Click += new RoutedEventHandler(torrentDownload);
            itemStart.CommandParameter = torLink;
            m.Items.Add(itemStart);

            if (!string.IsNullOrEmpty(torLink.TorrentLink))
            {
                MenuItem itemLink = new MenuItem();
                itemLink.Header = "Go to Website";
                itemLink.Click += new RoutedEventHandler(torrentBrowseWebsite);
                itemLink.CommandParameter = torLink;
                m.Items.Add(itemLink);
            }

            m.IsOpen = true;
        }

        void torrentBrowseWebsite(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = e.Source as MenuItem;
                MenuItem itemSender = sender as MenuItem;

                if (item == null || itemSender == null) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item != null && item.CommandParameter != null)
                {
                    Window parentWindow = Window.GetWindow(this);
                    parentWindow.Cursor = Cursors.Wait;
                    IsEnabled = false;

                    TorrentLinkVM torLink = item.CommandParameter as TorrentLinkVM;

                    Uri uri = new Uri(torLink.TorrentLinkFull);
                    Process.Start(new ProcessStartInfo(uri.AbsoluteUri));


                    parentWindow.Cursor = Cursors.Arrow;
                    IsEnabled = true;

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void torrentDownload(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                IsEnabled = false;

                MenuItem item = e.Source as MenuItem;
                MenuItem itemSender = sender as MenuItem;

                if (item == null || itemSender == null) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item != null && item.CommandParameter != null)
                {
                    Window parentWindow = Window.GetWindow(this);
                    parentWindow.Cursor = Cursors.Wait;
                    IsEnabled = false;

                    TorrentLinkVM torLink = item.CommandParameter as TorrentLinkVM;
                    torLink.Source.PopulateTorrentDownloadLink(ref torLink);
                    if (!AppSettings.TorrentBlackhole)
                    {
                        VM_UTorrentHelper.Instance.AddTorrentFromURL(torLink.TorrentDownloadLink);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(AppSettings.TorrentBlackholeFolder))
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFileAsync(new Uri(torLink.TorrentDownloadLink), AppSettings.TorrentBlackholeFolder + "\\" + GetValidFileName(torLink.TorrentName + ".torrent"));
                            }
                        }
                    }

                    parentWindow.Cursor = Cursors.Arrow;
                    IsEnabled = true;

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                IsEnabled = true;
            }
        }

        private static string GetValidFileName(string fileName)
        {
            Regex illegalInFileName = new Regex(@"[\\/:*?""<>|]");
            return illegalInFileName.Replace(fileName, "");
        }

        private bool LinkSearchFilter(object obj)
        {
            TorrentLinkVM torLink = obj as TorrentLinkVM;
            if (torLink == null) return true;

            int index = torLink.TorrentName.IndexOf(txtFileSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase);
            if (index > -1) return true;
            return false;
        }

        void txtFileSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewTorrentLinks.Refresh();
        }

        void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtFileSearch.Text = "";
        }

        private void CommandBinding_BrowseTorrents(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;

            try
            {
                Window parentWindow = Window.GetWindow(this);
                IsEnabled = false;

                if (obj.GetType() == typeof(TorrentSourceVM))
                {
                    TorrentSourceVM tor = obj as TorrentSourceVM;

                    TorrentLinks.Clear();
                    ViewTorrentLinks.Refresh();

                    List<TorrentLinkVM> links = tor.BrowseTorrents();
                    TorrentCount = links.Count;

                    foreach (TorrentLinkVM link in links)
                        TorrentLinks.Add(link);

                    ViewTorrentLinks.Refresh();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                IsEnabled = true;
            }
        }
    }
}
