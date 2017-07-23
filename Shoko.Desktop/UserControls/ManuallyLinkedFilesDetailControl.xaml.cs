using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for ManuallyLinkedFilesDetailControl.xaml
    /// </summary>
    public partial class ManuallyLinkedFilesDetailControl : UserControl
    {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(ManuallyLinkedFilesDetailControl), new UIPropertyMetadata(false, isExpandedCallback));

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
            typeof(bool), typeof(ManuallyLinkedFilesDetailControl), new UIPropertyMetadata(true, isCollapsedCallback));

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

        public bool IsLocalFile
        {
            get
            {
                VM_VideoLocal vidLocal = DataContext as VM_VideoLocal;
                if (vidLocal != null)
                {
                    if (!string.IsNullOrEmpty(vidLocal.GetLocalFileSystemFullPath()))
                        return true;
                    return false;
                }
                return true;
            }
        }

        public bool IsHashed
        {
            get
            {
                VM_VideoLocal vidLocal = DataContext as VM_VideoLocal;
                if (vidLocal != null)
                {
                    if (!string.IsNullOrEmpty(vidLocal.Hash))
                        return true;
                    return false;
                }
                return true;
            }
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

        public ManuallyLinkedFilesDetailControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
        }

        public void DisplayEpisodes()
        {
            try
            {
                VM_VideoLocal vidLocal = DataContext as VM_VideoLocal;
                if (vidLocal != null)
                {
                    lbEpisodes.ItemsSource = vidLocal.GetEpisodes();
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
                DisplayEpisodes();
            }
        }

        private void CommandBinding_AvdumpFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = (MainWindow)Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null || mainWindow == null) return;

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    VM_VideoLocal vid = DataContext as VM_VideoLocal;
                    mainWindow.ShowPinnedFileAvDump(vid);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private void CommandBinding_RescanFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_VideoLocal vid = DataContext as VM_VideoLocal;
                    VM_ShokoServer.Instance.ShokoServices.RescanFile(vid.VideoLocalID);
                }

                MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_INFO_AddedQueueCmds, Shoko.Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_VideoLocal vid = DataContext as VM_VideoLocal;
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid,force);
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
                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    VM_VideoLocal vid = DataContext as VM_VideoLocal;
                    if (ep == null || vid == null)
                    {
                        MessageBox.Show("ep or vid is null. This is not okay, so report it.", Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string res = VM_ShokoServer.Instance.ShokoServices.RemoveAssociationOnFile(vid.VideoLocalID, ep.AniDB_EpisodeID);
                    if (res.Length > 0)
                    {
                        MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        VM_MainListHelper.Instance.UpdateHeirarchy(ep);
                        DisplayEpisodes();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
