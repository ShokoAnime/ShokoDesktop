using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace JMMClient.VideoPlayers.mpv
{
    public class VideoPlayer : System.Windows.Controls.UserControl
    {
        public MPVVideoPlayer Player { get; set; }
        private VideoInfo _vinfo;




        public void PlayVideoInfo(VideoInfo ifo)
        {
            this.Background = new SolidColorBrush(Colors.Black);
            if (Player==null)
                throw new Exception("Player not set");
            _vinfo = ifo;
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            Player.SetWindowsHandle(source.Handle);
            if (ifo.IsPlaylist)
            {
                Player.PlayPlaylist(_vinfo.Uri);
            }
            else
            {
                Player.PlayUrl(_vinfo.Uri);
            }
            if (_vinfo.SubtitlePaths != null && _vinfo.SubtitlePaths.Count > 0)
            {
                foreach (string s in _vinfo.SubtitlePaths)
                {
                    Player.AddSubtitle(s);
                }
            }
            if (_vinfo.ResumePosition != 0)
            {
                double val = _vinfo.ResumePosition;
                val /= 1000;
                Player.Time = val;
            }
        }
    }
}
