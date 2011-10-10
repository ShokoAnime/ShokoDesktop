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
using NLog;
using System.IO;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for MultipleFilesControl.xaml
	/// </summary>
	public partial class MultipleFilesControl : UserControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ICollectionView ViewEpisodes { get; set; }
		public ObservableCollection<AnimeEpisodeVM> CurrentEpisodes { get; set; }

		public static readonly DependencyProperty EpisodeCountProperty = DependencyProperty.Register("EpisodeCount",
			typeof(int), typeof(MultipleFilesControl), new UIPropertyMetadata(0, null));

		private int lastSelIndex = 0;

		public int EpisodeCount
		{
			get { return (int)GetValue(EpisodeCountProperty); }
			set { SetValue(EpisodeCountProperty, value); }
		}

		public MultipleFilesControl()
		{
			InitializeComponent();

			CurrentEpisodes = new ObservableCollection<AnimeEpisodeVM>();
			ViewEpisodes = CollectionViewSource.GetDefaultView(CurrentEpisodes);
			ViewEpisodes.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
			ViewEpisodes.SortDescriptions.Add(new SortDescription("EpisodeTypeAndNumberAbsolute", ListSortDirection.Ascending));

			
			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			lbMultipleFiles.SelectionChanged += new SelectionChangedEventHandler(lbMultipleFiles_SelectionChanged);

		}

		void lbMultipleFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lbMultipleFiles.Items.Count > 0)
				lastSelIndex = lbMultipleFiles.SelectedIndex;
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshMultipleFiles();
		}

		public void RefreshMultipleFiles()
		{
			try
			{
				this.Cursor = Cursors.Wait;
				CurrentEpisodes.Clear();

				List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllEpisodesWithMultipleFiles(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				EpisodeCount = eps.Count;

				foreach (JMMServerBinary.Contract_AnimeEpisode ep in eps)
					CurrentEpisodes.Add(new AnimeEpisodeVM(ep));

				// move to the next item
				/*if (lastSelIndex <= lbDuplicateFiles.Items.Count)
				{
					lbDuplicateFiles.SelectedIndex = lastSelIndex;
					lbDuplicateFiles.Focus();
					lbDuplicateFiles.ScrollIntoView(lbDuplicateFiles.SelectedItem);
				}
				else
				{
					// move to the previous item
					if (lastSelIndex - 1 <= lbDuplicateFiles.Items.Count)
					{
						lbDuplicateFiles.SelectedIndex = lastSelIndex - 1;
						lbDuplicateFiles.Focus();
						lbDuplicateFiles.ScrollIntoView(lbDuplicateFiles.SelectedItem);
					}
				}*/
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
					Utils.PlayVideo(vid);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		

	}
}
