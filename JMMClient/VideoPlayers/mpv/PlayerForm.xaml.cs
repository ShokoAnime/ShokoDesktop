using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JMMClient.VideoPlayers.mpv
{
    /// <summary>
    /// Interaction logic for PlayerForm.xaml
    /// </summary>
    public partial class PlayerForm : Window
    {
        public PlayerForm(MPVVideoPlayer player)
        {
            InitializeComponent();
            video.Player = player;
            KeyUp += VideoPlayer_KeyUp;
            MouseDown += PlayerForm_MouseDown;
            Closed += PlayerForm_Closed;
            clickTimer=new Timer();
            clickTimer.Elapsed += ClickTimer_Elapsed;
        }

        public void Resize(int width, int height)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                double rel = (double)width / (double)height;
                double currentrel = (double)video.ActualWidth / (double)video.ActualHeight;
                if (Math.Abs(currentrel - rel) > double.Epsilon)
                {
                    double h = video.ActualWidth / rel;
                    if (h <= video.ActualHeight)
                    {
                        double delta = video.ActualHeight - h;
                        this.Height = this.Height - delta;
                        this.Top = this.Top + (delta/2);
                    }
                    else
                    {
                        double w = video.ActualHeight * rel;
                        double delta = video.ActualWidth - w;
                        this.Width = this.ActualWidth - delta;
                        this.Left = this.Left + (delta/2);
                    }
                }
            });
            
        }
        private void ClickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (mSingleClick)
            {
                clickTimer.Stop();
                mSingleClick = false;
                if (video.Player.IsPaused)
                    video.Player.Play();
                else
                    video.Player.Pause();
            }
        }

        private bool mSingleClick;
        private Timer clickTimer = new Timer();
        private void PlayerForm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                mSingleClick = true;
                clickTimer.Interval = System.Windows.Forms.SystemInformation.DoubleClickTime;
                clickTimer.Start();
            }
            else if (e.ClickCount == 2)
            {
                clickTimer.Stop();
                mSingleClick = false;
                SwitchStates();
            }



        }



        private bool _inStateChange;

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Maximized && !_inStateChange)
            {
                _inStateChange = true;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                _inStateChange = false;
            }
            base.OnStateChanged(e);
        }

        public void Init()
        {

        }

        private void Maximize()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
        }

        private void Normal()
        {
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.CanResize;
            WindowStyle = WindowStyle.SingleBorderWindow;

        }

        private void SwitchStates()
        {
            if (WindowState == WindowState.Maximized)
            {
                Normal();
            }
            else
            {
                Maximize();
            }
        }
        private void VideoPlayer_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

            video.Player.ProcessKey(e);
            if (e.Key == Key.Escape)
            {
                if (WindowState == WindowState.Maximized)
                {
                    SwitchStates();
                }
                else
                {
                    video.Player.Stop();
                    this.Close();
                }
            }
            if (e.Key == Key.Q)
            {
                video.Player.Stop();
                this.Close();
            }
            if ((e.Key == Key.F11) || (e.Key==Key.Return))
            {
                SwitchStates();
            }

        }
        private void PlayerForm_Closed(object sender, EventArgs e)
        {
            video.Player.Quit();
        }

        public void Play(VideoInfo info)
        {
            video.PlayVideoInfo(info);
        }
    }
}
