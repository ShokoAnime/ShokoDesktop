using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for MultipleFilesDetailControl.xaml
    /// </summary>
    public partial class MultipleFilesDetailControl : UserControl
    {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(MultipleFilesDetailControl), new UIPropertyMetadata(false, isExpandedCallback));

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
            typeof(bool), typeof(MultipleFilesDetailControl), new UIPropertyMetadata(true, isCollapsedCallback));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        public MultipleFilesDetailControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;


                    AskDeleteFile dlg = new AskDeleteFile(string.Format(Shoko.Commons.Properties.Resources.DeleteFile_Title, vid.GetFileName()), Shoko.Commons.Properties.Resources.MultipleFiles_ConfirmDelete + "\r\n\r\n" + Shoko.Commons.Properties.Resources.DeleteFile_Confirm, vid.Places);
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        string tresult = string.Empty;
                        Cursor = Cursors.Wait;
                        foreach (CL_VideoLocal_Place lv in dlg.Selected)
                        {
                            string result =
                                VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(
                                    lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                        if (ep != null)
                        {
                            VM_MainListHelper.Instance.UpdateHeirarchy(ep);
                            ep.LocalFileCount--;
                        }
                        DisplayFiles();
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

        private void CommandBinding_ToggleVariation(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;

                    vid.VideoLocal_IsVariation = vid.Variation ? 0 : 1;

                    string result = VM_ShokoServer.Instance.ShokoServices.SetVariationStatusOnFile(vid.VideoLocalID, vid.Variation);
                    if (result.Length > 0)
                        MessageBox.Show(result, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void DisplayFiles()
        {
            try
            {
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                if (ep != null)
                {
                    ep.RefreshFilesForEpisode();
                    ep.RefreshAnime();
                    fileSummary.DataContext = null;
                    fileSummary.DataContext = ep.AniDB_Anime;

                    lbFiles.ItemsSource = ep.FilesForEpisode;

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
