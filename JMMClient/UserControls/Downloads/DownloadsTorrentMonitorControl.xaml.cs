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

		public static readonly DependencyProperty HasAttachedSeriesProperty = DependencyProperty.Register("HasAttachedSeries",
			typeof(bool), typeof(DownloadsTorrentMonitorControl), new UIPropertyMetadata(false, null));

		public bool HasAttachedSeries
		{
			get { return (bool)GetValue(HasAttachedSeriesProperty); }
			set { SetValue(HasAttachedSeriesProperty, value); }
		}

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
			SetAttachedSeries(det.AnimeSeries);
		}

		void torrentDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Torrent tor = e.Argument as Torrent;
			List<TorrentFile> files = UTorrentHelperVM.Instance.GetFilesForTorrent(tor.Hash);

			TorrentDetails det = new TorrentDetails();
			det.TorrentFiles = files;

			// try and find the series
			foreach (AniDB_AnimeVM anime in AniDB_AnimeVM.BestLevenshteinDistanceMatchesCache(tor.ClosestAnimeMatchString, 10))
			{
				// get the series for the anime
				AnimeSeriesVM ser = MainListHelperVM.Instance.GetSeriesForAnime(anime.AnimeID);
				if (ser != null)
				{
					det.AnimeSeries = ser;
					break;
				}
				
			}

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

			if (dgTorrents.Items == null || dgTorrents.Items.Count == 0) return;

			if (dgTorrents.SelectedItems == null || dgTorrents.SelectedItems.Count == 0)
				dgTorrents.SelectedItem = dgr.DataContext;

			if (dgTorrents.SelectedItems.Count == 1)
				dgTorrents.SelectedItem = dgr.DataContext;

			ContextMenu m = new ContextMenu();

			List<Torrent> selectedTorrents = new List<Torrent>();
			foreach (object obj in dgTorrents.SelectedItems)
			{
				Torrent tor = obj as Torrent;
				selectedTorrents.Add(tor);
			}

			MenuItem itemStart = new MenuItem();
			itemStart.Header = "Start";
			itemStart.Click += new RoutedEventHandler(torrentStart);
			itemStart.CommandParameter = selectedTorrents;

			MenuItem itemStop = new MenuItem();
			itemStop.Header = "Stop";
			itemStop.Click += new RoutedEventHandler(torrentStop);
			itemStop.CommandParameter = selectedTorrents;

			MenuItem itemPause = new MenuItem();
			itemPause.Header = "Pause";
			itemPause.Click += new RoutedEventHandler(torrentPause);
			itemPause.CommandParameter = selectedTorrents;

			MenuItem itemRemove = new MenuItem();
			itemRemove.Header = "Remove Torrent";
			itemRemove.Click += new RoutedEventHandler(torrentRemove);
			itemRemove.CommandParameter = selectedTorrents;

			MenuItem itemRemoveData = new MenuItem();
			itemRemoveData.Header = "Remove Torrent and Files";
			itemRemoveData.Click += new RoutedEventHandler(torrentRemoveData);
			itemRemoveData.CommandParameter = selectedTorrents;

			if (selectedTorrents.Count == 1)
			{
				Torrent tor = selectedTorrents[0];

				if (tor.IsNotRunning || tor.IsPaused)
					m.Items.Add(itemStart);

				if (tor.IsRunning || tor.IsPaused)
					m.Items.Add(itemStop);

				if (tor.IsRunning)
					m.Items.Add(itemPause);
			}
			else
			{
				m.Items.Add(itemStart);
				m.Items.Add(itemStop);
				m.Items.Add(itemPause);
			}

			m.Items.Add(itemRemove);
			m.Items.Add(itemRemoveData);

			m.IsOpen = true;
		}

		void torrentRemoveData(object sender, RoutedEventArgs e)
		{
			try
			{
				MenuItem item = e.Source as MenuItem;
				MenuItem itemSender = sender as MenuItem;

				if (item == null || itemSender == null) return;
				if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

				if (item != null && item.CommandParameter != null)
				{
					List<Torrent> selectedTorrents = item.CommandParameter as List<Torrent>;

					string msg = "";
					if (selectedTorrents.Count == 1)
						msg = string.Format("Are you sure you want to remove this torrent and delete associated files: {0}", selectedTorrents[0].Name);
					else
						msg = string.Format("Are you sure you want to remove these {0} torrents and delete associated files?", selectedTorrents.Count);

					MessageBoxResult res = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (res == MessageBoxResult.Yes)
					{
						Window parentWindow = Window.GetWindow(this);
						parentWindow.Cursor = Cursors.Wait;
						this.IsEnabled = false;


						foreach (Torrent tor in selectedTorrents)
							UTorrentHelperVM.Instance.RemoveTorrentAndData(tor.Hash);

						parentWindow.Cursor = Cursors.Arrow;
						this.IsEnabled = true;
					}

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void torrentRemove(object sender, RoutedEventArgs e)
		{
			try
			{
				MenuItem item = e.Source as MenuItem;
				MenuItem itemSender = sender as MenuItem;

				if (item == null || itemSender == null) return;
				if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

				if (item != null && item.CommandParameter != null)
				{
					List<Torrent> selectedTorrents = item.CommandParameter as List<Torrent>;

					string msg = "";
					if (selectedTorrents.Count == 1)
						msg = string.Format("Are you sure you want to remove this torrent: {0}", selectedTorrents[0].Name);
					else
						msg = string.Format("Are you sure you want to remove these {0} torrents?", selectedTorrents.Count);

					MessageBoxResult res = MessageBox.Show(msg,"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (res == MessageBoxResult.Yes)
					{
						Window parentWindow = Window.GetWindow(this);
						parentWindow.Cursor = Cursors.Wait;
						this.IsEnabled = false;

						
						foreach (Torrent tor in selectedTorrents)
							UTorrentHelperVM.Instance.RemoveTorrent(tor.Hash);

						parentWindow.Cursor = Cursors.Arrow;
						this.IsEnabled = true;
					}

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
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

					List<Torrent> selectedTorrents = item.CommandParameter as List<Torrent>;
					foreach (Torrent tor in selectedTorrents)
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

					List<Torrent> selectedTorrents = item.CommandParameter as List<Torrent>;
					foreach (Torrent tor in selectedTorrents)
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

					List<Torrent> selectedTorrents = item.CommandParameter as List<Torrent>;
					foreach (Torrent tor in selectedTorrents)
					{
						//Debug.WriteLine(tor.ToString());
						UTorrentHelperVM.Instance.StartTorrent(tor.Hash);
					}

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
			SetAttachedSeries(null);
			dgTorrentFiles.ItemsSource = null;

			if (!torrentDetailsWorker.IsBusy)
				torrentDetailsWorker.RunWorkerAsync(tor);
		}

		private void SetAttachedSeries(AnimeSeriesVM ser)
		{
			try
			{
				HasAttachedSeries = ser != null;

				if (ser == null)
				{
					this.DataContext = null;
					this.ucFileSummary.DataContext = null;
					return;
				}

				this.DataContext = ser;
				this.ucFileSummary.DataContext = ser.AniDB_Anime;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}

	public class TorrentDetails
	{
		public List<TorrentFile> TorrentFiles { get; set; }
		public AnimeSeriesVM AnimeSeries { get; set; }
	}
}
