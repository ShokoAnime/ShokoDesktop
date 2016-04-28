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
using JMMClient.ViewModel;
using JMMClient.Forms;
using System.Threading;
using System.Globalization;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for PlaylistControl.xaml
	/// </summary>
	public partial class PlaylistControl : UserControl
	{
		public static readonly DependencyProperty HasPlaylistsProperty = DependencyProperty.Register("HasPlaylists",
			typeof(bool), typeof(PlaylistControl), new UIPropertyMetadata(false, null));


		public bool HasPlaylists
		{
			get { return (bool)GetValue(HasPlaylistsProperty); }
			set
			{
				SetValue(HasPlaylistsProperty, value);
			}
		}

		public PlaylistControl()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            PlaylistHelperVM.Instance.OnPlaylistModifiedEvent += new PlaylistHelperVM.PlaylistModifiedHandler(Instance_OnPlaylistModifiedEvent);
			btnEditPlaylist.Click += new RoutedEventHandler(btnEditPlaylist_Click);
			btnEditPlaylistFinish.Click += new RoutedEventHandler(btnEditPlaylistFinish_Click);

			btnIncreaseHeaderImageSize.Click += new RoutedEventHandler(btnIncreaseHeaderImageSize_Click);
			btnDecreaseHeaderImageSize.Click += new RoutedEventHandler(btnDecreaseHeaderImageSize_Click);

			togFanart.IsChecked = UserSettingsVM.Instance.UseFanartOnPlaylistHeader;
			togFanart.Click += new RoutedEventHandler(togFanart_Click);

			cboPlayOrderEdit.Items.Clear();
			cboPlayOrderEdit.Items.Add(Properties.Resources.PlaylistPlayOrderSeq);
			cboPlayOrderEdit.Items.Add(Properties.Resources.PlaylistPlayOrderRandom);
			cboPlayOrderEdit.SelectedIndex = 0;


			cboPlayOrderLocked.Items.Clear();
			cboPlayOrderLocked.Items.Add(Properties.Resources.PlaylistPlayOrderSeq);
			cboPlayOrderLocked.Items.Add(Properties.Resources.PlaylistPlayOrderRandom);
			cboPlayOrderLocked.SelectedIndex = 0;

			btnRandomEpisode.Click += new RoutedEventHandler(btnRandomEpisode_Click);

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlaylistControl_DataContextChanged);
			MainWindow.videoHandler.VideoWatchedEvent += new Utilities.VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

			btnPlayAll.Click += new RoutedEventHandler(btnPlayAll_Click);
		}

		void btnPlayAll_Click(object sender, RoutedEventArgs e)
		{
			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			this.Cursor = Cursors.Wait;
			List<AnimeEpisodeVM> eps = pl.GetAllEpisodes(false);
			this.Cursor = Cursors.Arrow;

			MainWindow.videoHandler.PlayEpisodes(eps);
		}

		void videoHandler_VideoWatchedEvent(Utilities.VideoWatchedEventArgs ev)
		{
			if (MainWindow.CurrentMainTabIndex == MainWindow.TAB_MAIN_Playlists)
			{
				PlaylistVM pl = this.DataContext as PlaylistVM;
				if (pl == null) return;

				pl.PopulatePlaylistObjects();
				ShowNextEpisode();
			}
		}

		void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
		{
			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			this.Cursor = Cursors.Wait;
			pl.SetNextEpisode(true);
			ShowNextEpisode();
			this.Cursor = Cursors.Arrow;
		}

		void togFanart_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.UseFanartOnPlaylistHeader = !UserSettingsVM.Instance.UseFanartOnPlaylistHeader;

			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			pl.SetDependendProperties();
		}

		void btnDecreaseHeaderImageSize_Click(object sender, RoutedEventArgs e)
		{

			UserSettingsVM.Instance.PlaylistHeader_Image_Height = UserSettingsVM.Instance.PlaylistHeader_Image_Height - 10;
		}

		void btnIncreaseHeaderImageSize_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.PlaylistHeader_Image_Height = UserSettingsVM.Instance.PlaylistHeader_Image_Height + 10;
		}

		void btnEditPlaylistFinish_Click(object sender, RoutedEventArgs e)
		{
			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			pl.PlaylistName = txtPlaylistName.Text.Trim();
			pl.PlayUnwatched = chkPlayUnwatchedEdit.IsChecked.Value ? 1 : 0;
			pl.PlayWatched = chkPlayWatchedEdit.IsChecked.Value ? 1 : 0;

			if (cboPlayOrderEdit.SelectedIndex == 0) pl.DefaultPlayOrder = (int)PlaylistPlayOrder.Sequential;
			if (cboPlayOrderEdit.SelectedIndex == 1) pl.DefaultPlayOrder = (int)PlaylistPlayOrder.Random;

			pl.Save();

			pl.IsBeingEdited = false;
			pl.IsReadOnly = true;


			if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Sequential)
			{
				cboPlayOrderEdit.SelectedIndex = 0;
				cboPlayOrderLocked.SelectedIndex = 0;
			}
			if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Random)
			{
				cboPlayOrderEdit.SelectedIndex = 1;
				cboPlayOrderLocked.SelectedIndex = 1;
			}

			pl.SetDependendProperties();
		}

		void btnEditPlaylist_Click(object sender, RoutedEventArgs e)
		{
			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			pl.IsBeingEdited = true;
			pl.IsReadOnly = false;
		}

		void Instance_OnPlaylistModifiedEvent(PlaylistModifiedEventArgs ev)
		{
			ShowNextEpisode();

			
		}

		void PlaylistControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			HasPlaylists = false;

			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			ShowNextEpisode();

			if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Sequential)
			{
				cboPlayOrderEdit.SelectedIndex = 0;
				cboPlayOrderLocked.SelectedIndex = 0;
			}
			if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Random)
			{
				cboPlayOrderEdit.SelectedIndex = 1;
				cboPlayOrderLocked.SelectedIndex = 1;
			}

			HasPlaylists = true;
		}

		

		private void ShowNextEpisode()
		{
			ucNextEpisodePlaylist.EpisodeExists = false;
			ucNextEpisodePlaylist.EpisodeMissing = true;
			ucNextEpisodePlaylist.DataContext = null;

			PlaylistVM pl = this.DataContext as PlaylistVM;
			if (pl == null) return;

			if (pl.NextEpisode == null) return;

			if (pl.NextEpisode != null)
				ucNextEpisodePlaylist.DataContext = pl.NextEpisode;

		}

		private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(PlaylistItemVM))
				{
					PlaylistItemVM pli = obj as PlaylistItemVM;

					AnimeEpisodeVM ep = null;

					if (pli.ItemType == JMMClient.PlaylistItemType.Episode)
					{
						ep = pli.PlaylistItem as AnimeEpisodeVM;
					}
					if (pli.ItemType == JMMClient.PlaylistItemType.AnimeSeries)
					{
						AnimeSeriesVM ser = pli.PlaylistItem as AnimeSeriesVM;
						if (ser == null) return;

						JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisode(ser.AnimeSeriesID.Value,
							JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
						if (contract != null)
							ep = new AnimeEpisodeVM(contract);
					}

					if (ep == null) return;
					ep.SetTvDBInfo();

					if (ep.FilesForEpisode.Count == 1)
						MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0]);
					else if (ep.FilesForEpisode.Count > 1)
					{
						if (AppSettings.AutoFileSingleEpisode)
						{
							VideoDetailedVM vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
							if (vid != null)
								MainWindow.videoHandler.PlayVideo(vid);
						}
						else
						{
							PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
							frm.Owner = parentWindow;
							frm.Init(ep);
							frm.ShowDialog();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		/// <summary>
		/// This event bubbles up from PlayEpisodeControl
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			this.Cursor = Cursors.Wait;

			try
			{
				Window parentWindow = Window.GetWindow(this);
				AnimeSeriesVM serTemp = null;
				bool newStatus = false;

				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					MainListHelperVM.Instance.UpdateHeirarchy(vid);

					serTemp = MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

					serTemp = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
				}

				if (obj.GetType() == typeof(PlaylistItemVM))
				{
					PlaylistItemVM pli = obj as PlaylistItemVM;
					AnimeEpisodeVM ep = pli.PlaylistItem as AnimeEpisodeVM;

					newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

					serTemp = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
				}

				PlaylistVM pl = this.DataContext as PlaylistVM;
				if (pl == null) return;

				pl.PopulatePlaylistObjects();
				ShowNextEpisode();

				if (newStatus == true && serTemp != null)
				{
					Utils.PromptToRateSeries(serTemp, parentWindow);
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
			}
		}
	}
}
