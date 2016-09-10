using JMMClient.ViewModel;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JMMClient.Forms;

namespace JMMClient.UserControls
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
                VideoLocalVM vidLocal = this.DataContext as VideoLocalVM;
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
                MainWindow MainWindow = (MainWindow)Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
                    VideoLocalVM vid = this.DataContext as VideoLocalVM;
                    MainWindow.ShowPinnedFileAvDump(vid);
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

                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    VideoLocalVM vid = this.DataContext as VideoLocalVM;
                    JMMServerVM.Instance.clientBinaryHTTP.RescanFile(vid.VideoLocalID);
                }

                MessageBox.Show(Properties.Resources.MSG_INFO_AddedQueueCmds, Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    VideoLocalVM vid = this.DataContext as VideoLocalVM;
                    bool force = true;
                    if (vid.ResumePosition > 0)
                    {
                        AskResumeVideo ask=new AskResumeVideo(vid.ResumePosition);
                        ask.Owner = Window.GetWindow(this);
                        if (ask.ShowDialog() == true)
                            force = false;
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
                if (obj.GetType() == typeof(AnimeEpisodeVM))
                {
                    AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
                    VideoLocalVM vid = this.DataContext as VideoLocalVM;

                    string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveAssociationOnFile(vid.VideoLocalID, ep.AniDB_EpisodeID);
                    if (res.Length > 0)
                    {
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        if (ep != null)
                        {
                            MainListHelperVM.Instance.UpdateHeirarchy(ep);
                            DisplayEpisodes();
                        }
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
