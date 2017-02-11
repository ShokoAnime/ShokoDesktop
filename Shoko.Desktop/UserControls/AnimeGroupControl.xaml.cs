using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Models.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AnimeGroupControl.xaml
    /// </summary>
    public partial class AnimeGroupControl : UserControl
    {
        public AnimeGroupControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeGroupControl_DataContextChanged);

            btnSelectDefaultSeries.Click += new RoutedEventHandler(btnSelectDefaultSeries_Click);
            btnRemoveDefaultSeries.Click += new RoutedEventHandler(btnRemoveDefaultSeries_Click);
            btnRandomEpisode.Click += new RoutedEventHandler(btnRandomEpisode_Click);

            lbSeriesList.MouseDoubleClick += new MouseButtonEventHandler(lbSeriesList_MouseDoubleClick);
        }

        void lbSeriesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbSeriesList.SelectedItem == null) return;

            VM_AnimeSeries_User ser = lbSeriesList.SelectedItem as VM_AnimeSeries_User;
            if (ser == null) return;

            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

            mainwdw.ShowChildrenForCurrentGroup(ser);
        }

        void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeGroup_User grp = DataContext as VM_AnimeGroup_User;
            if (grp == null) return;

            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

            RandomEpisodeForm frm = new RandomEpisodeForm();
            frm.Owner = Window.GetWindow(this); ;
            frm.Init(RandomSeriesEpisodeLevel.Group, grp);
            bool? result = frm.ShowDialog();
        }

        void btnRemoveDefaultSeries_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeGroup_User grp = DataContext as VM_AnimeGroup_User;
            if (grp == null) return;

            if (grp.AnimeGroupID==0) return;

            VM_ShokoServer.Instance.ShokoServices.RemoveDefaultSeriesForGroup(grp.AnimeGroupID);
            grp.DefaultAnimeSeriesID = null;
        }

        void btnSelectDefaultSeries_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeGroup_User grp = DataContext as VM_AnimeGroup_User;
            if (grp == null) return;

            Window wdw = Window.GetWindow(this);

            SelectDefaultSeriesForm frm = new SelectDefaultSeriesForm();
            frm.Owner = wdw;
            frm.Init(grp);
            bool? result = frm.ShowDialog();
            if (result.Value)
            {
                // update info
                grp.DefaultAnimeSeriesID = frm.SelectedSeriesID.Value;
                VM_ShokoServer.Instance.ShokoServices.SetDefaultSeriesForGroup(grp.AnimeGroupID, frm.SelectedSeriesID.Value);
            }
        }

        void AnimeGroupControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ShowNextEpisode();
        }

        private void ShowNextEpisode()
        {
            VM_AnimeGroup_User grp = DataContext as VM_AnimeGroup_User;
            if (grp == null) return;

            if (grp.AnimeGroupID==0)
            {
                ucNextEpisode.EpisodeExists = false;
                ucNextEpisode.EpisodeMissing = true;
                ucNextEpisode.DataContext = null;
                return;
            }

            VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetNextUnwatchedEpisodeForGroup(grp.AnimeGroupID,VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            if (ep != null)
            {
                ep.SetTvDBInfo();
                ucNextEpisode.DataContext = ep;
            }
            else
            {
                ucNextEpisode.EpisodeExists = false;
                ucNextEpisode.EpisodeMissing = true;
                ucNextEpisode.DataContext = null;
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

            Cursor = Cursors.Wait;

            try
            {
                Window parentWindow = Window.GetWindow(this);
                VM_AnimeSeries_User ser = null;
                bool newStatus = false;

                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    newStatus = !vid.Watched;
                    VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_MainListHelper.Instance.UpdateHeirarchy(vid);

                    ser = VM_MainListHelper.Instance.GetSeriesForVideo(vid.VideoLocalID);
                }

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    newStatus = !ep.Watched;
                    CL_Response<CL_AnimeEpisode_User> response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    ser = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                ShowNextEpisode();

                if (newStatus == true && ser != null)
                {
                    Utils.PromptToRateSeries(ser, parentWindow);
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

    public class ContentAwareScrollViewer : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var parentElement = Parent as UIElement;
            if (parentElement != null)
            {
                if ((e.Delta > 0 && VerticalOffset == 0) ||
                    (e.Delta < 0 && VerticalOffset == ScrollableHeight))
                {
                    e.Handled = true;

                    var routedArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                    routedArgs.RoutedEvent = MouseWheelEvent;
                    parentElement.RaiseEvent(routedArgs);
                }
            }

            base.OnMouseWheel(e);
        }
    }
}
