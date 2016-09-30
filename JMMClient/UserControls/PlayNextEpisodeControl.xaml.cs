using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for PlayNextEpisodeControl.xaml
    /// </summary>
    public partial class PlayNextEpisodeControl : UserControl
    {
        public static readonly DependencyProperty EpisodeExistsProperty = DependencyProperty.Register("EpisodeExists",
            typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(false, null));

        public bool EpisodeExists
        {
            get { return (bool)GetValue(EpisodeExistsProperty); }
            set { SetValue(EpisodeExistsProperty, value); }
        }

        public static readonly DependencyProperty EpisodeMissingProperty = DependencyProperty.Register("EpisodeMissing",
            typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(false, null));

        public bool EpisodeMissing
        {
            get { return (bool)GetValue(EpisodeMissingProperty); }
            set { SetValue(EpisodeMissingProperty, value); }
        }

        public static readonly DependencyProperty FullOverviewProperty = DependencyProperty.Register("FullOverview",
            typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(false, null));

        public bool FullOverview
        {
            get { return (bool)GetValue(FullOverviewProperty); }
            set { SetValue(FullOverviewProperty, value); }
        }

        public static readonly DependencyProperty TruncatedOverviewProperty = DependencyProperty.Register("TruncatedOverview",
            typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(true, null));

        public bool TruncatedOverview
        {
            get { return (bool)GetValue(TruncatedOverviewProperty); }
            set { SetValue(TruncatedOverviewProperty, value); }
        }

        public static readonly DependencyProperty HideOverviewProperty = DependencyProperty.Register("HideOverview",
            typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(false, null));

        public bool HideOverview
        {
            get { return (bool)GetValue(HideOverviewProperty); }
            set { SetValue(HideOverviewProperty, value); }
        }

        public PlayNextEpisodeControl()
        {
            InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlayNextEpisodeControl_DataContextChanged);

        }

        private void Handle_Click(object sender, MouseButtonEventArgs e)
        {
            string tag = ((TextBlock)sender).Tag.ToString();

            if (tag.Equals("txtOverview", StringComparison.InvariantCultureIgnoreCase))
            {
                AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
                if (ep != null)
                {
                    if (ep.ShowEpisodeOverviewInSummary)
                    {
                        TruncatedOverview = false;
                        FullOverview = true;
                        HideOverview = false;
                    }
                    else
                    {
                        TruncatedOverview = false;
                        FullOverview = false;
                        HideOverview = true;
                    }
                }
            }
        }

        void PlayNextEpisodeControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;

                if (ep == null)
                {
                    EpisodeExists = false;
                    EpisodeMissing = true;
                }
                else
                {
                    EpisodeExists = true;
                    EpisodeMissing = false;

                    if (ep.ShowEpisodeOverviewInSummary)
                    {
                        TruncatedOverview = true;
                        FullOverview = false;
                        HideOverview = false;
                    }
                    else
                    {
                        TruncatedOverview = false;
                        FullOverview = false;
                        HideOverview = true;
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
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.VideoLocal_ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid, force);
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
                if (ep.FilesForEpisode.Count > 0)
                {
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (ep.FilesForEpisode[0].VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(ep.FilesForEpisode[0].VideoLocal_ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0], force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


    }
}
