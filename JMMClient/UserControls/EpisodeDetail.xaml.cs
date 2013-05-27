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
using System.IO;
using System.Diagnostics;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for EpisodeDetail.xaml
	/// </summary>
	public partial class EpisodeDetail : UserControl
	{
		private ContextMenu playlistMenu;

		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(false, isExpandedCallback));

		
		public bool IsExpanded
		{
			get { return (bool)GetValue(IsExpandedProperty); }
			set { 
				SetValue(IsExpandedProperty, value);
				SetVisibility();
			}
		}

		public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, isCollapsedCallback));


		public bool IsCollapsed
		{
			get { return (bool)GetValue(IsCollapsedProperty); }
			set { 
				SetValue(IsCollapsedProperty, value);
				SetVisibility();
			}
		}

		public static readonly DependencyProperty ShowEpisodeImageInSummaryProperty = DependencyProperty.Register("ShowEpisodeImageInSummary",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


		public bool ShowEpisodeImageInSummary
		{
			get { return (bool)GetValue(ShowEpisodeImageInSummaryProperty); }
			set { SetValue(ShowEpisodeImageInSummaryProperty, value); }
		}

		public static readonly DependencyProperty ShowEpisodeOverviewInSummaryProperty = DependencyProperty.Register("ShowEpisodeOverviewInSummary",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


		public bool ShowEpisodeOverviewInSummary
		{
			get { return (bool)GetValue(ShowEpisodeOverviewInSummaryProperty); }
			set { SetValue(ShowEpisodeOverviewInSummaryProperty, value); }
		}

		public static readonly DependencyProperty ShowEpisodeImageInExpandedProperty = DependencyProperty.Register("ShowEpisodeImageInExpanded",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


		public bool ShowEpisodeImageInExpanded
		{
			get { return (bool)GetValue(ShowEpisodeImageInExpandedProperty); }
			set { SetValue(ShowEpisodeImageInExpandedProperty, value); }
		}

		public static readonly DependencyProperty ShowEpisodeOverviewInExpandedProperty = DependencyProperty.Register("ShowEpisodeOverviewInExpanded",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


		public bool ShowEpisodeOverviewInExpanded
		{
			get { return (bool)GetValue(ShowEpisodeOverviewInExpandedProperty); }
			set { SetValue(ShowEpisodeOverviewInExpandedProperty, value); }
		}

		public static readonly DependencyProperty ShowDownloadButtonProperty = DependencyProperty.Register("ShowDownloadButton",
			typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


		public bool ShowDownloadButton
		{
			get { return (bool)GetValue(ShowDownloadButtonProperty); }
			set { SetValue(ShowDownloadButtonProperty, value); }
		}

		private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			EpisodeDetail input = (EpisodeDetail)d;
			//input.tbTest.Text = e.NewValue as string;
		}

		private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			EpisodeDetail input = (EpisodeDetail)d;
			//input.tbTest.Text = e.NewValue as string;
		}

		private void SetVisibility()
		{
			AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
			if (ep != null)
			{
				ShowEpisodeImageInSummary = IsCollapsed && ep.ShowEpisodeImageInSummary;
				ShowEpisodeOverviewInSummary = IsCollapsed && ep.ShowEpisodeOverviewInSummary;

				ShowEpisodeImageInExpanded = IsExpanded && ep.ShowEpisodeImageInExpanded;
				ShowEpisodeOverviewInExpanded = IsExpanded && ep.ShowEpisodeOverviewInExpanded;

				if ((ep.HasFiles && !AppSettings.HideDownloadButtonWhenFilesExist) || !ep.HasFiles)
					ShowDownloadButton = true;
				else
					ShowDownloadButton = false;
			}
		}

		public EpisodeDetail()
		{
			InitializeComponent();

			playlistMenu = new ContextMenu();

			btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
			this.DataContextChanged += new DependencyPropertyChangedEventHandler(EpisodeDetail_DataContextChanged);
			this.Loaded += new RoutedEventHandler(EpisodeDetail_Loaded);

			btnPlaylistAdd.ContextMenu = playlistMenu;
			btnPlaylistAdd.Click += new RoutedEventHandler(btnPlaylistAdd_Click);

			btnTvDBLinkAdd.Click += new RoutedEventHandler(btnTvDBLinkAdd_Click);
			btnTvDBLinkRemove.Click += new RoutedEventHandler(btnTvDBLinkRemove_Click);
            btnUpdateEpisode.Click += new RoutedEventHandler(btnUpdateEpisode_Click);
		}

        void btnUpdateEpisode_Click(object sender, RoutedEventArgs e)
        {
            AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;

            Window wdw = Window.GetWindow(this);

            this.Cursor = Cursors.Wait;

            try
            {
                string res = JMMServerVM.Instance.clientBinaryHTTP.UpdateEpisodeData(ep.AniDB_EpisodeID);
                if (res.Length > 0)
                {
                    this.Cursor = Cursors.Arrow;
                    Utils.ShowErrorMessage(res);
                    return;
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

		void btnTvDBLinkRemove_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// get the current tvdb link
				AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;

				string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTvDBEpisode(ep.AniDB_EpisodeID);
				if (res.Length > 0)
				{
					this.Cursor = Cursors.Arrow;
					Utils.ShowErrorMessage(res);
					return;
				}
				
				// update info
				JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetEpisode(
					ep.AnimeEpisodeID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				if (contract != null)
				{
					ep.RefreshAnime(true);
					ep.Populate(contract);
					ep.SetTvDBInfo();
				}


				this.Cursor = Cursors.Arrow;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnTvDBLinkAdd_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// get the current tvdb link
				AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
				ep.RefreshAnime();
				if (ep.AniDB_Anime == null || ep.AniDB_Anime.TvSummary == null || ep.AniDB_Anime.TvSummary.CrossRefTvDBV2 == null || 
					ep.AniDB_Anime.TvSummary.CrossRefTvDBV2.Count == 0)
				{
					Utils.ShowErrorMessage("The series must be linked to a TvDB series first");
					return;
				}

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SelectTvDBEpisodeForm frm = new SelectTvDBEpisodeForm();
				frm.Owner = wdw;
				frm.Init(ep, ep.AniDB_Anime);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetEpisode(
						ep.AnimeEpisodeID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (contract != null)
					{
						ep.RefreshAnime(true);
						ep.Populate(contract);
						ep.SetTvDBInfo();
					}
					
				}

				this.Cursor = Cursors.Arrow;
	
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnPlaylistAdd_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// get all playlists
				List<JMMServerBinary.Contract_Playlist> rawPlaylists = JMMServerVM.Instance.clientBinaryHTTP.GetAllPlaylists();
				PlaylistMenuCommand cmd = null;

				playlistMenu.Items.Clear();

				Separator sep = new Separator();

				MenuItem itemNew = new MenuItem();
				itemNew.Header = "New Playlist";
				itemNew.Click += new RoutedEventHandler(playlistMenuItem_Click);
				cmd = new PlaylistMenuCommand(PlaylistItemType.SingleEpisode, -1); // new playlist
				itemNew.CommandParameter = cmd;
				playlistMenu.Items.Add(itemNew);
				playlistMenu.Items.Add(sep);

				foreach (JMMServerBinary.Contract_Playlist contract in rawPlaylists)
				{
					MenuItem itemSeriesPL = new MenuItem();
					itemSeriesPL.Header = contract.PlaylistName;
					itemSeriesPL.Click += new RoutedEventHandler(playlistMenuItem_Click);
					cmd = new PlaylistMenuCommand(PlaylistItemType.SingleEpisode, contract.PlaylistID.Value);
					itemSeriesPL.CommandParameter = cmd;
					playlistMenu.Items.Add(itemSeriesPL);
				}

				playlistMenu.PlacementTarget = this;
				playlistMenu.IsOpen = true;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void playlistMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MenuItem item = e.Source as MenuItem;
				MenuItem itemSender = sender as MenuItem;

				if (item == null || itemSender == null) return;
				if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

				if (item != null && item.CommandParameter != null)
				{
					PlaylistMenuCommand cmd = item.CommandParameter as PlaylistMenuCommand;
					Debug.Write("Playlist Menu: " + cmd.ToString() + Environment.NewLine);

					AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
					if (ep == null) return;

					// get the playlist
					PlaylistVM pl = null;
					if (cmd.PlaylistID < 0)
					{
						pl = PlaylistHelperVM.CreatePlaylist(Window.GetWindow(this));
						if (pl == null) return;
					}
					else
					{
						JMMServerBinary.Contract_Playlist plContract = JMMServerVM.Instance.clientBinaryHTTP.GetPlaylist(cmd.PlaylistID);
						if (plContract == null)
						{
							MessageBox.Show("Could not find playlist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}
						pl = new PlaylistVM(plContract);
					}

					this.Cursor = Cursors.Wait;

					pl.AddEpisode(ep.AnimeEpisodeID);
					pl.Save();

					PlaylistHelperVM.Instance.RefreshData();

					this.Cursor = Cursors.Arrow;

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void EpisodeDetail_Loaded(object sender, RoutedEventArgs e)
		{
			DependencyObject parentObject = VisualTreeHelper.GetParent(this);
			while (parentObject != null)
			{
				parentObject = VisualTreeHelper.GetParent(parentObject);
				AnimeSeries seriesControl = parentObject as AnimeSeries;
				if (seriesControl != null)
				{
					double gridWidth = seriesControl.ActualWidth - 40;
					if (gridWidth > 0)
						epDetailMainGrid.Width = gridWidth;
					return;
				}
			}
		}

		void EpisodeDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			SetVisibility();
		}

		private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;

					if (File.Exists(vid.FullPath))
					{
						Utils.OpenFolderAndSelectFile(vid.FullPath);
					}
					else
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DeleteLink(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
					//AnimeEpisodeVM ep = MainListHelperVM.Instance.GetEpisodeForVideo(vid, ((MainWindow)parentWindow).epListMain);

					string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveAssociationOnFile(vid.VideoLocalID, ep.AniDB_EpisodeID);
					if (res.Length > 0)
					{
						MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
					else
					{
						if (ep != null)
						{
							MainListHelperVM.Instance.UpdateHeirarchy(ep);
							DisplayFiles();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;

					MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete this file, the physical video file will also be deleted"),
						"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

					if (res == MessageBoxResult.Yes)
					{
						this.Cursor = Cursors.Wait;
						string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalAndFile(vid.VideoLocalID);
						if (result.Length > 0)
							MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						else
						{
							// find the entry and remove it
							if (ep != null)
							{
								MainListHelperVM.Instance.UpdateHeirarchy(ep);
								DisplayFiles();
							}
							
						}
					}
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

		private void CommandBinding_PlayVideo(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					//AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
					MainWindow.videoHandler.PlayVideo(vid);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
				ep.RefreshFilesForEpisode();

				if (ep.FilesForEpisode.Count > 0)
					MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0]);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_ToggleFileDetails(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					vid.ShowMoreDetails = !vid.ShowMoreDetails;
					vid.ShowLessDetails = !vid.ShowLessDetails;
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

		private void CommandBinding_RehashFile(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				Window parentWindow = Window.GetWindow(this);

				object obj = e.Parameter;
				if (obj == null) return;

				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					JMMServerVM.Instance.clientBinaryHTTP.RehashFile(vid.VideoLocalID);
				}
				

				MessageBox.Show(Properties.Resources.MSG_INFO_AddedQueueCmds, "Done", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_ForceAddMyList(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;

					JMMServerVM.Instance.clientBinaryHTTP.ForceAddFileToMyList(vid.VideoLocal_Hash);
					
					MessageBox.Show("Command has been queued for processing on the server", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
					
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

        

		private void CommandBinding_ForceUpdate(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;

					JMMServerVM.Instance.clientBinaryHTTP.UpdateFileData(vid.VideoLocalID);

					MessageBox.Show("Command has been queued for processing on the server", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

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

		private void CommandBinding_ToggleVariation(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;

					vid.VideoLocal_IsVariation = vid.Variation ? 0 : 1;

					string result = JMMServerVM.Instance.clientBinaryHTTP.SetVariationStatusOnFile(vid.VideoLocalID, vid.Variation);
					if (result.Length > 0)
						MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

		public void DisplayFiles()
		{
			try
			{
				AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
				if (ep != null)
				{
					ep.RefreshFilesForEpisode();
					lbFiles.ItemsSource = ep.FilesForEpisode;

					if (!ep.HasFiles)
					{
						List<AniDBReleaseGroupVM> relGroups = ep.ReleaseGroups;
						if (relGroups.Count > 0)
						{
							string grpList = "";
							foreach (AniDBReleaseGroupVM rg in relGroups)
							{
								if (grpList.Length > 0)
									grpList += ", ";
								grpList += rg.GroupName;
							}
							tbFileDetailsGroups.Text = JMMClient.Properties.Resources.GroupsAvailableFrom + " " + grpList;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnToggleExpander_Click(object sender, RoutedEventArgs e)
		{
			IsExpanded = !IsExpanded;
			IsCollapsed = !IsCollapsed;

			if (IsExpanded)
			{
				DisplayFiles();
			}
		}
	}
}
