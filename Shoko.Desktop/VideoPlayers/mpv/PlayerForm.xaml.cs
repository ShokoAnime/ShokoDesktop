using System;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using NutzCode.MPVPlayer.WPF.Wrapper;
using NutzCode.MPVPlayer.WPF.Wrapper.Models;

namespace Shoko.Desktop.VideoPlayers.mpv
{
    /// <summary>
    /// Interaction logic for PlayerForm.xaml
    /// </summary>
    public partial class PlayerForm : Window
    {
        private VideoInfo _vinfo;
        public event BaseVideoPlayer.FilePositionHandler FilePosition;

        static PlayerForm()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public PlayerForm(MPVVideoPlayer player)
        {
            InitializeComponent();
            Player.SetTopControl(this);
            Player.PositionChanged += (position) =>
            {
                _vinfo?.ChangePosition(position*1000);
            };
            Player.SettingsChange += (settings) =>
            {
                AppSettings.MpvPlayerSettings = settings;
            };
        }
        public void Stop()
        {
            Player.Pause();
            _vinfo?.ForceChange((long)(Player.Time*1000));
            FilePosition?.Invoke(_vinfo, (long)(Player.Time * 1000));
            if (_vinfo != null)
                BaseVideoPlayer.PlaybackStopped(_vinfo, (long)Player.Time);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == "NutzCode.MPVPlayer.WPF.Wrapper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
            {
                // Have to use FullName because relative path will return the same exception otherwise.
                return Assembly.LoadFile(new System.IO.FileInfo("NutzCode.MPVPlayer.WPF.Wrapper.dll")
                    .FullName); 
            }

            return null;
        }


        public void Quit()
        {
            Player.Pause();
            _vinfo?.ForceChange((long)(Player.Time * 1000));
            FilePosition?.Invoke(_vinfo, (long)(Player.Time * 1000));
            if (_vinfo != null)
                BaseVideoPlayer.PlaybackStopped(_vinfo, (long)Player.Time);
            Player.Close();
        }

        public void Play(VideoInfo info)
        {
            _vinfo = info;
            PlayRequest r=new PlayRequest();
            r.Autoplay = true;
            r.ExternalSubtitles = info.SubtitlePaths;
            r.IsPlaylist = info.IsPlaylist;
            r.TakeScreenshotOnStart = false;
            r.ResumePosition = info.ResumePosition/1000;
            r.Uri = info.Uri;
            Player.Play(r, AppSettings.MpvPlayerSettings);
        }
    }
}
