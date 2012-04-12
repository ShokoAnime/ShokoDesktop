using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JMMClient.Downloads;
using System.Diagnostics;
using System.ComponentModel;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DownloadsTorrentMonitorControl.xaml
	/// </summary>
	public partial class DownloadsTorrentMonitorControl : UserControl
	{
		BackgroundWorker torrentDetailsWorker = new BackgroundWorker();

		public DownloadsTorrentMonitorControl()
		{
			InitializeComponent();

			dgTorrents.SelectionChanged += new SelectionChangedEventHandler(dgTorrents_SelectionChanged);
			dgTorrents.MouseLeftButtonUp += new MouseButtonEventHandler(dgTorrents_MouseLeftButtonUp);

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

			dgTorrents.LoadingRow += new EventHandler<DataGridRowEventArgs>(dgTorrents_LoadingRow);

			torrentDetailsWorker.DoWork += new DoWorkEventHandler(torrentDetailsWorker_DoWork);
			torrentDetailsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(torrentDetailsWorker_RunWorkerCompleted);
		}

		void dgTorrents_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			DataGrid _DataGrid = sender as DataGrid;

			Torrent tor = _DataGrid.SelectedItem as Torrent;
			if (tor == null) return;

			ShowTorrentDetails(tor);
		}

		void torrentDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			TorrentDetails det = e.Result as TorrentDetails;

			// show files
			dgTorrentFiles.ItemsSource = det.TorrentFiles;

			// show details
			// try and guess the series based on the file name
		}

		void torrentDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Torrent tor = e.Argument as Torrent;
			List<TorrentFile> files = UTorrentHelperVM.Instance.GetFilesForTorrent(tor.Hash);

			TorrentDetails det = new TorrentDetails();
			det.TorrentFiles = files;

			e.Result = det;
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

			Torrent tor = dgTorrents.SelectedItem as Torrent;
			if (tor == null) return;

			if (tor.IsNotRunning || tor.IsPaused)
			{
				MenuItem itemStart = new MenuItem();
				itemStart.Header = "Start";
				itemStart.Click += new RoutedEventHandler(torrentStart);
				itemStart.CommandParameter = tor;
				m.Items.Add(itemStart);
			}

			if (tor.IsRunning || tor.IsPaused)
			{
				MenuItem itemStop = new MenuItem();
				itemStop.Header = "Stop";
				itemStop.Click += new RoutedEventHandler(torrentStop);
				itemStop.CommandParameter = tor;
				m.Items.Add(itemStop);
			}

			if (tor.IsRunning)
			{
				MenuItem itemPause = new MenuItem();
				itemPause.Header = "Pause";
				itemPause.Click += new RoutedEventHandler(torrentPause);
				itemPause.CommandParameter = tor;
				m.Items.Add(itemPause);
			}

			m.IsOpen = true;
		}

		void torrentPause(object sender, RoutedEventArgs e)
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
					this.IsEnabled = false;

					Torrent tor = item.CommandParameter as Torrent;
					UTorrentHelperVM.Instance.PauseTorrent(tor.Hash);

					parentWindow.Cursor = Cursors.Arrow;
					this.IsEnabled = true;

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void torrentStop(object sender, RoutedEventArgs e)
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
					this.IsEnabled = false;

					Torrent tor = item.CommandParameter as Torrent;
					UTorrentHelperVM.Instance.StopTorrent(tor.Hash);

					parentWindow.Cursor = Cursors.Arrow;
					this.IsEnabled = true;

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void torrentStart(object sender, RoutedEventArgs e)
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
					this.IsEnabled = false;

					Torrent tor = item.CommandParameter as Torrent;
					UTorrentHelperVM.Instance.StartTorrent(tor.Hash);

					parentWindow.Cursor = Cursors.Arrow;
					this.IsEnabled = true;

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			if (UserSettingsVM.Instance.UTorrentAutoRefresh)
			{
				MessageBox.Show("Only use when auto refresh is disabled", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			Window parentWindow = Window.GetWindow(this);
			parentWindow.Cursor = Cursors.Wait;
			this.IsEnabled = false;

			UTorrentHelperVM.Instance.RefreshTorrents();

			parentWindow.Cursor = Cursors.Arrow;
			this.IsEnabled = true;
		}

		void dgTorrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DataGrid _DataGrid = sender as DataGrid;

			Torrent tor = _DataGrid.SelectedItem as Torrent;
			if (tor == null) return;

			ShowTorrentDetails(tor);
		}

		private void ShowTorrentDetails(Torrent tor)
		{
			dgTorrentFiles.ItemsSource = null;
			if (!torrentDetailsWorker.IsBusy)
				torrentDetailsWorker.RunWorkerAsync(tor);
		}
	}

	public class TorrentDetails
	{
		public List<TorrentFile> TorrentFiles { get; set; }
	}
}
