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
using System.ComponentModel;
using System.Collections.ObjectModel;
using JMMClient.Downloads;
using JMMClient.ViewModel;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DownloadsSearchTorrentsControl.xaml
	/// </summary>
	public partial class DownloadsSearchTorrentsControl : UserControl
	{
		BackgroundWorker torrentDetailsWorker = new BackgroundWorker();
		BackgroundWorker searchWorker = new BackgroundWorker();

		public ICollectionView ViewTorrentLinks { get; set; }
		public ObservableCollection<TorrentLinkVM> TorrentLinks { get; set; }
		public ObservableCollection<SubGroupSimple> SubGroups { get; set; }

		public static readonly DependencyProperty TorrentCountProperty = DependencyProperty.Register("TorrentCount",
			typeof(int), typeof(DownloadsSearchTorrentsControl), new UIPropertyMetadata(0, null));

		public int TorrentCount
		{
			get { return (int)GetValue(TorrentCountProperty); }
			set { SetValue(TorrentCountProperty, value); }
		}

		public static readonly DependencyProperty HasAttachedSeriesProperty = DependencyProperty.Register("HasAttachedSeries",
			typeof(bool), typeof(DownloadsSearchTorrentsControl), new UIPropertyMetadata(false, null));

		public bool HasAttachedSeries
		{
			get { return (bool)GetValue(HasAttachedSeriesProperty); }
			set { SetValue(HasAttachedSeriesProperty, value); }
		}

		public static readonly DependencyProperty TorrentSearchDescriptionProperty = DependencyProperty.Register("TorrentSearchDescription",
			typeof(string), typeof(DownloadsSearchTorrentsControl), new UIPropertyMetadata("", null));

		public string TorrentSearchDescription
		{
			get { return (string)GetValue(TorrentSearchDescriptionProperty); }
			set { SetValue(TorrentSearchDescriptionProperty, value); }
		}

		public static readonly DependencyProperty CurrentSearchCriteriaProperty = DependencyProperty.Register("CurrentSearchCriteria",
			typeof(DownloadSearchCriteria), typeof(DownloadsSearchTorrentsControl), new UIPropertyMetadata(null, null));

		public DownloadSearchCriteria CurrentSearchCriteria
		{
			get { return (DownloadSearchCriteria)GetValue(CurrentSearchCriteriaProperty); }
			set { SetValue(CurrentSearchCriteriaProperty, value); }
		}

		public static readonly DependencyProperty TorrentSearchStatusProperty = DependencyProperty.Register("TorrentSearchStatus",
			typeof(string), typeof(DownloadsSearchTorrentsControl), new UIPropertyMetadata("", null));

		public string TorrentSearchStatus
		{
			get { return (string)GetValue(TorrentSearchStatusProperty); }
			set { SetValue(TorrentSearchStatusProperty, value); }
		}

		public DownloadsSearchTorrentsControl()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            TorrentLinks = new ObservableCollection<TorrentLinkVM>();
			ViewTorrentLinks = CollectionViewSource.GetDefaultView(TorrentLinks);
			ViewTorrentLinks.Filter = LinkSearchFilter;

			SubGroups = new ObservableCollection<SubGroupSimple>();

			btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
			txtFileSearch.TextChanged += new TextChangedEventHandler(txtFileSearch_TextChanged);

			dgTorrents.SelectionChanged += new SelectionChangedEventHandler(dgTorrents_SelectionChanged);
			dgTorrents.MouseLeftButtonUp += new MouseButtonEventHandler(dgTorrents_MouseLeftButtonUp);
			dgTorrents.LoadingRow += new EventHandler<DataGridRowEventArgs>(dgTorrents_LoadingRow);

			torrentDetailsWorker.DoWork += new DoWorkEventHandler(torrentDetailsWorker_DoWork);
			torrentDetailsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(torrentDetailsWorker_RunWorkerCompleted);

			searchWorker.DoWork += new DoWorkEventHandler(searchWorker_DoWork);
			searchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(searchWorker_RunWorkerCompleted);

			btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
		}

		private void CommandBinding_ToggleSource(object sender, ExecutedRoutedEventArgs e)
		{
			//object obj = lbGroupsSeries.SelectedItem;
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(TorrentSourceVM))
				{
					TorrentSourceVM src = (TorrentSourceVM)obj;
					src.IsEnabled = !src.IsEnabled;
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_AddSubGroupSearch(object sender, ExecutedRoutedEventArgs e)
		{
			//object obj = lbGroupsSeries.SelectedItem;
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(SubGroupSimple))
				{
					SubGroupSimple sub = (SubGroupSimple)obj;
					txtSearch.Text = txtSearch.Text + " " + sub.GroupNameShort;
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_AddSubGroupFilter(object sender, ExecutedRoutedEventArgs e)
		{
			//object obj = lbGroupsSeries.SelectedItem;
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(SubGroupSimple))
				{
					SubGroupSimple sub = (SubGroupSimple)obj;

					string newSearch = txtFileSearch.Text.Trim();
					if (!string.IsNullOrEmpty(newSearch))
						newSearch += " ";
					newSearch += sub.GroupNameShort;

					txtFileSearch.Text = newSearch;
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void searchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				List<TorrentLinkVM> links = e.Result as List<TorrentLinkVM>;
				TorrentCount = links.Count;

				foreach (TorrentLinkVM link in links)
					TorrentLinks.Add(link);

				ViewTorrentLinks.Refresh();

				List<GroupVideoQualityVM> vidQualListTemp = new List<GroupVideoQualityVM>();

				if (CurrentSearchCriteria.SearchType == DownloadSearchType.Episode)
				{
					AnimeEpisodeVM ep = CurrentSearchCriteria.SearchParameter as AnimeEpisodeVM;
					if (ep.AniDB_Anime == null) ep.RefreshAnime();
					if (ep.AniDB_Anime != null)
					{
						
						List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(ep.AniDB_Anime.AnimeID);
						foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
						{
							GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
							vidQualListTemp.Add(vidQual);
						}
					}
				}
				if (CurrentSearchCriteria.SearchType == DownloadSearchType.Series)
				{
					AniDB_AnimeVM anime = CurrentSearchCriteria.SearchParameter as AniDB_AnimeVM;
					if (anime != null)
					{
						List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(anime.AnimeID);
						foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
						{
							GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
							vidQualListTemp.Add(vidQual);
						}
					}
				}

				ShowSubGroupSuggestions(vidQualListTemp);

				TorrentSearchStatus = string.Format(Properties.Resources.Downloads_Results, links.Count);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
				this.IsEnabled = true;
			}
		}

		private void ShowSubGroupSuggestions(List<GroupVideoQualityVM> vidQualList)
		{
			SubGroups.Clear();
			Dictionary<string, GroupVideoQualityVM> vidQuals = new Dictionary<string, GroupVideoQualityVM>();

			foreach (GroupVideoQualityVM vidQual in vidQualList)
				vidQuals[vidQual.GroupNameShort] = vidQual;

			foreach (GroupVideoQualityVM vidq in vidQuals.Values)
			{
				if (vidq.GroupNameShort != "NO GROUP INFO")
				{
					SubGroupSimple sub = new SubGroupSimple();
					sub.GroupName = vidq.GroupName;
					sub.GroupNameShort = vidq.GroupNameShort;
					SubGroups.Add(sub);
				}
			}
			
		}

		void searchWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				DownloadSearchCriteria crit = e.Argument as DownloadSearchCriteria;
				List<TorrentLinkVM> links = DownloadHelper.SearchTorrents(crit);
				e.Result = links;
			}
			catch (Exception ex)
			{
				
			}
		}

		public void PerformSearch(DownloadSearchCriteria crit)
		{
			this.Cursor = Cursors.Wait;
			TorrentSearchStatus = string.Format(Properties.Resources.Downloads_Searching);

			try
			{
				CurrentSearchCriteria = crit;
				this.IsEnabled = false;

				if (crit.SearchType != DownloadSearchType.Manual)
				{
					string desc = "";
					foreach (string parm in crit.GetParms())
					{
						if (!string.IsNullOrEmpty(desc))
							desc += " ";
						desc += parm;
					}
					txtSearch.Text = desc;
				}

				SubGroups.Clear();
				TorrentLinks.Clear();
				ViewTorrentLinks.Refresh();

				searchWorker.RunWorkerAsync(crit);
				

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtSearch.Text)) return;

			DownloadSearchCriteria crit = new DownloadSearchCriteria(DownloadSearchType.Manual, txtSearch.Text.Trim());
			PerformSearch(crit);
		}

		private void ShowTorrentDetails(DetailsContainer details)
		{
			SetAttachedSeries(null);

			if (!torrentDetailsWorker.IsBusy)
				torrentDetailsWorker.RunWorkerAsync(details);
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

				List<GroupVideoQualityVM> vidQuals = new List<GroupVideoQualityVM>(this.ucFileSummary.VideoQualityRecords);
				ShowSubGroupSuggestions(vidQuals);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void torrentDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			AnimeSeriesVM ser = e.Result as AnimeSeriesVM;

			SetAttachedSeries(ser);
		}

		void torrentDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			DetailsContainer details = e.Argument as DetailsContainer;
			if (details == null) return;

			if (details.SearchCritera != null)
			{
				if (details.SearchCritera.SearchType == DownloadSearchType.Episode)
				{
					AnimeEpisodeVM ep = details.SearchCritera.SearchParameter as AnimeEpisodeVM;
					AnimeSeriesVM ser = MainListHelperVM.Instance.GetSeries(ep.AnimeSeriesID);
					if (ser != null)
					{
						e.Result = ser;
						return;
					}
				}

				if (details.SearchCritera.SearchType == DownloadSearchType.Series)
				{
					AniDB_AnimeVM anime = details.SearchCritera.SearchParameter as AniDB_AnimeVM;
					AnimeSeriesVM ser = MainListHelperVM.Instance.GetSeriesForAnime(anime.AnimeID);
					if (ser != null)
					{
						e.Result = ser;
						return;
					}
				}
			}

			// try and find the series
			foreach (AniDB_AnimeVM anime in AniDB_AnimeVM.BestLevenshteinDistanceMatchesCache(details.TorLink.ClosestAnimeMatchString, 10))
			{
				// get the series for the anime
				AnimeSeriesVM ser = MainListHelperVM.Instance.GetSeriesForAnime(anime.AnimeID);
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

			DetailsContainer details = new DetailsContainer();
			details.TorLink = torLink;
			details.SearchCritera = CurrentSearchCriteria;

			ShowTorrentDetails(details);
		}

		void dgTorrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DataGrid _DataGrid = sender as DataGrid;

			TorrentLinkVM torLink = _DataGrid.SelectedItem as TorrentLinkVM;
			if (torLink == null) return;

			DetailsContainer details = new DetailsContainer();
			details.TorLink = torLink;
			details.SearchCritera = CurrentSearchCriteria;

			ShowTorrentDetails(details);
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
			itemStart.Header = Properties.Resources.Downloads_Download;
			itemStart.Click += new RoutedEventHandler(torrentDownload);
			itemStart.CommandParameter = torLink;
			m.Items.Add(itemStart);

			if (!string.IsNullOrEmpty(torLink.TorrentLink))
			{
				MenuItem itemLink = new MenuItem();
				itemLink.Header = Properties.Resources.Downloads_GoWebsite;
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
					this.IsEnabled = false;

					TorrentLinkVM torLink = item.CommandParameter as TorrentLinkVM;

					Uri uri = new Uri(torLink.TorrentLinkFull);
					Process.Start(new ProcessStartInfo(uri.AbsoluteUri));


					parentWindow.Cursor = Cursors.Arrow;
					this.IsEnabled = true;

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

        private static string GetValidFileName(string fileName)
        {
            Regex illegalInFileName = new Regex(@"[\\/:*?""<>|]");
            return illegalInFileName.Replace(fileName, "");
        }


		void torrentDownload(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Cursor = Cursors.Wait;
				this.IsEnabled = false;

				MenuItem item = e.Source as MenuItem;
				MenuItem itemSender = sender as MenuItem;

				if (item == null || itemSender == null) return;
				if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

				if (item != null && item.CommandParameter != null)
				{
					Window parentWindow = Window.GetWindow(this);
					parentWindow.Cursor = Cursors.Wait;
					this.IsEnabled = false;

					TorrentLinkVM torLink = item.CommandParameter as TorrentLinkVM;
					torLink.Source.PopulateTorrentDownloadLink(ref torLink);

                    if (!AppSettings.TorrentBlackhole)
                    {
                        UTorrentHelperVM.Instance.AddTorrentFromURL(torLink.TorrentDownloadLink);
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
                    this.IsEnabled = true;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
				this.IsEnabled = true;
			}
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

	}

	public class DetailsContainer
	{
		public TorrentLinkVM TorLink { get; set; }
		public DownloadSearchCriteria SearchCritera { get; set; }
	}
}
