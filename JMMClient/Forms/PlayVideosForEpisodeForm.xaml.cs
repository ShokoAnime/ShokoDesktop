using System;
using System.Windows;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for PlayVideosForEpisodeForm.xaml
    /// </summary>
    public partial class PlayVideosForEpisodeForm : Window
    {

        public PlayVideosForEpisodeForm()
        {
            InitializeComponent();
        }

        public void Init(AnimeEpisodeVM episode)
        {
            this.DataContext = episode;
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
                    if (vid.VideoLocal_ResumePosition > 0)
                    {
                        AskResumeVideo ask = new AskResumeVideo(vid.VideoLocal_ResumePosition);
                        ask.Owner = Window.GetWindow(this);
                        if (ask.ShowDialog() == true)
                            force = false;
                    }
                    MainWindow.videoHandler.PlayVideo(vid,force);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
