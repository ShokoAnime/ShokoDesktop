using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for LinkPlexForm.xaml
    /// </summary>
    public partial class LinkPlexForm : Window
    {
        private readonly JMMUser _user;
        private readonly Timer _timer;

        public LinkPlexForm(JMMUser user)
        {
            _user = user;
            InitializeComponent();
            btnClose.Click += BtnClose_Click;
            btnLink.Click += BtnLinkOnClick;
            btnInvalidate.Click += BtnInvalidateOnClick;

            _timer = new Timer {Interval = 10*1000};//10 seconds
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void BtnInvalidateOnClick(object sender, RoutedEventArgs routedEventArgs) => VM_ShokoServer.Instance.ShokoServices.RemovePlexAuth(_user.JMMUserID);

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            bool isAuthorized = VM_ShokoServer.Instance.ShokoServices.IsPlexAuthenticated(_user.JMMUserID);

            if (!isAuthorized)
                return;

            this.Dispatcher.Invoke(() => txtPin.Text = Commons.Properties.Resources.Done);
            _timer.Stop();
        }

        private void BtnLinkOnClick(object sender, RoutedEventArgs routedEventArgs) => txtPin.Text = VM_ShokoServer.Instance.ShokoServices.LinkToPlex(_user.JMMUserID);

        private void BtnClose_Click(object sender, RoutedEventArgs routedEventArgs) => Close();
    }
}
