using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private LinkPlexVM _vm;

        public LinkPlexForm(JMMUser user)
        {
            _user = user;
            InitializeComponent();
            btnClose.Click += BtnClose_Click;
            btnLink.Click += BtnLinkOnClick;
            btnInvalidate.Click += BtnInvalidateOnClick;

            this.DataContext = _vm = new LinkPlexVM();

            _timer = new Timer {Interval = 1*1000};//10 seconds
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void BtnInvalidateOnClick(object sender, RoutedEventArgs routedEventArgs) => VM_ShokoServer.Instance.ShokoServices.RemovePlexAuth(_user.JMMUserID);

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _vm.Authorized = VM_ShokoServer.Instance.ShokoServices.IsPlexAuthenticated(_user.JMMUserID);

            if (!_vm.Authorized)
                return;

            _timer.Stop();
        }

        private void BtnLinkOnClick(object sender, RoutedEventArgs routedEventArgs) => _vm.LinkUrl = VM_ShokoServer.Instance.ShokoServices.LoginUrl(_user.JMMUserID);

        private void BtnClose_Click(object sender, RoutedEventArgs routedEventArgs) => Close();
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
    }

    public sealed class LinkPlexVM : INotifyPropertyChanged
    {
        private string _linkUrl;
        private bool _isAuthorized;

        public string LinkUrl
        {
            get => _linkUrl;
            set
            {
                _linkUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LinkGenerated));
                OnPropertyChanged(nameof(ShouldDisplayLink));
            }
        }

        public string LinkDisplay => _isAuthorized
            ? Shoko.Commons.Properties.Resources.Done
            : Shoko.Commons.Properties.Resources.Plex_UsageMessage;

        public bool ShouldDisplayLink => !string.IsNullOrEmpty(LinkUrl) && !_isAuthorized;

        public bool Authorized
        {
            get => _isAuthorized;
            set
            {
                _isAuthorized = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShouldDisplayLink));
                OnPropertyChanged(nameof(LinkDisplay));
            }
        }

        public bool LinkGenerated => !string.IsNullOrEmpty(LinkUrl);

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
