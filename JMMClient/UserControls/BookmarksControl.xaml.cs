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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for BookmarksControl.xaml
	/// </summary>
	public partial class BookmarksControl : UserControl
	{
		public BookmarksControl()
		{
			InitializeComponent();

			chkDownloading.Click += new RoutedEventHandler(chkDownloading_Click);
			chkNotDownloading.Click += new RoutedEventHandler(chkNotDownloading_Click);
		}

		void chkNotDownloading_Click(object sender, RoutedEventArgs e)
		{
			MainListHelperVM.Instance.BookmarkFilter_NotDownloading = chkNotDownloading.IsChecked.Value;
		}

		void chkDownloading_Click(object sender, RoutedEventArgs e)
		{
			MainListHelperVM.Instance.BookmarkFilter_Downloading = chkDownloading.IsChecked.Value;
		}

		private void CommandBinding_ToggleDownloading(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				object obj = e.Parameter;
				if (obj == null) return;

				BookmarkedAnimeVM ba = obj as BookmarkedAnimeVM;
				if (ba == null) return;

				bool newStatus = !ba.DownloadingBool;
				ba.Downloading = newStatus ? 1 : 0;
				ba.Save();
				
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_EditNotes(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				object obj = e.Parameter;
				if (obj == null) return;

				BookmarkedAnimeVM ba = obj as BookmarkedAnimeVM;
				if (ba == null) return;

				DialogTextMultiline dlg = new DialogTextMultiline();
				dlg.Init("Notes: ", ba.Notes);
				dlg.Owner = Window.GetWindow(this);
				bool? res = dlg.ShowDialog();
				if (res.HasValue && res.Value)
				{
					ba.Notes = dlg.EnteredText;
					ba.Save();
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_EditPriority(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				object obj = e.Parameter;
				if (obj == null) return;

				BookmarkedAnimeVM ba = obj as BookmarkedAnimeVM;
				if (ba == null) return;

				DialogInteger dlg = new DialogInteger();
				dlg.Init("Priority: ", ba.Priority, 1, 100);
				dlg.Owner = Window.GetWindow(this);
				bool? res = dlg.ShowDialog();
				if (res.HasValue && res.Value)
				{
					ba.Priority = dlg.EnteredInteger;
					ba.Save();
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DeleteBookmark(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				object obj = e.Parameter;
				if (obj == null) return;

				BookmarkedAnimeVM ba = obj as BookmarkedAnimeVM;
				if (ba == null) return;

				MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete this bookmark: {0}", ba.AniDB_Anime.FormattedTitle),
						"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

				if (res == MessageBoxResult.Yes)
				{
					this.Cursor = Cursors.Wait;
					string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteBookmarkedAnime(ba.BookmarkedAnimeID.Value);
					if (result.Length > 0)
						MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					else
					{
						MainListHelperVM.Instance.BookmarkedAnime.Remove(ba);
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
	}
}
