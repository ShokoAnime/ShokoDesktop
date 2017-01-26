using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for BookmarksControl.xaml
    /// </summary>
    public partial class BookmarksControl : UserControl
    {
        public BookmarksControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            chkDownloading.Click += new RoutedEventHandler(chkDownloading_Click);
            chkNotDownloading.Click += new RoutedEventHandler(chkNotDownloading_Click);
        }

        void chkNotDownloading_Click(object sender, RoutedEventArgs e)
        {
            VM_MainListHelper.Instance.BookmarkFilter_NotDownloading = chkNotDownloading.IsChecked.Value;
        }

        void chkDownloading_Click(object sender, RoutedEventArgs e)
        {
            VM_MainListHelper.Instance.BookmarkFilter_Downloading = chkDownloading.IsChecked.Value;
        }

        private void CommandBinding_ToggleDownloading(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            try
            {
                object obj = e.Parameter;
                if (obj == null) return;

                VM_BookmarkedAnime ba = obj as VM_BookmarkedAnime;
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

                VM_BookmarkedAnime ba = obj as VM_BookmarkedAnime;
                if (ba == null) return;

                DialogTextMultiline dlg = new DialogTextMultiline();
                dlg.Init(Properties.Resources.Bookmarks_Notes + " ", ba.Notes);
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

                VM_BookmarkedAnime ba = obj as VM_BookmarkedAnime;
                if (ba == null) return;

                DialogInteger dlg = new DialogInteger();
                dlg.Init(Properties.Resources.Bookmarks_Priority + " ", ba.Priority, 1, 100);
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

                VM_BookmarkedAnime ba = obj as VM_BookmarkedAnime;
                if (ba == null) return;

                MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.Bookmarks_Delete, ba.Anime.FormattedTitle),
                        Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (res == MessageBoxResult.Yes)
                {
                    Cursor = Cursors.Wait;
                    string result = VM_ShokoServer.Instance.ShokoServices.DeleteBookmarkedAnime(ba.BookmarkedAnimeID);
                    if (result.Length > 0)
                        MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        VM_MainListHelper.Instance.BookmarkedAnime.Remove(ba);
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }
    }
}
