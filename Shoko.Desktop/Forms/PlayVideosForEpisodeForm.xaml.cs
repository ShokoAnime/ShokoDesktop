using System;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
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

        public void Init(VM_AnimeEpisode_User episode)
        {
            DataContext = episode;
        }

        private void CommandBinding_PlayVideo(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    bool force = true;

                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() != Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.VideoLocal_ResumePosition);
                            ask.Owner = GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }

                    MainWindow.videoHandler.PlayVideo(vid,force);
                    Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
