using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for AniDBMyListSettings.xaml
    /// </summary>
    public partial class AniDBMyListSettings : UserControl
    {
        public AniDBMyListSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboStorageState.Items.Clear();
            cboStorageState.Items.Add(Properties.Resources.AniDBMyListUnknown);
            cboStorageState.Items.Add(Properties.Resources.AniDBMyListHDD);
            cboStorageState.Items.Add(Properties.Resources.AniDBMyListDVD);
            cboStorageState.SelectedIndex = 0;

            cboDeleteAction.Items.Clear();
            cboDeleteAction.Items.Add(Properties.Resources.AniDBMyListDelete);
            cboDeleteAction.Items.Add(Properties.Resources.AniDBMyListDeleteLocal);
            cboDeleteAction.Items.Add(Properties.Resources.AniDBMyListMarkDeleted);
            cboDeleteAction.Items.Add(Properties.Resources.AniDBMyListMarkExternal);
            cboDeleteAction.Items.Add(Properties.Resources.AniDBMyListMarkUnknown);
            cboDeleteAction.SelectedIndex = 0;

            Loaded += new RoutedEventHandler(AniDBMyListSettings_Loaded);

            cboStorageState.SelectionChanged += new SelectionChangedEventHandler(cboStorageState_SelectionChanged);
            cboDeleteAction.SelectionChanged += new SelectionChangedEventHandler(cboDeleteAction_SelectionChanged);

            chkMyListAdd.Click += new RoutedEventHandler(settingChanged);
            chkMyListReadUnwatched.Click += new RoutedEventHandler(settingChanged);
            chkMyListReadWatched.Click += new RoutedEventHandler(settingChanged);
            chkMyListSetUnwatched.Click += new RoutedEventHandler(settingChanged);
            chkMyListSetWatched.Click += new RoutedEventHandler(settingChanged);
        }

        void cboDeleteAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM_ShokoServer.Instance.AniDB_MyList_DeleteType = (AniDBFileDeleteType)cboDeleteAction.SelectedIndex;

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboStorageState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM_ShokoServer.Instance.AniDB_MyList_StorageState = cboStorageState.SelectedIndex;

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void AniDBMyListSettings_Loaded(object sender, RoutedEventArgs e)
        {
            cboStorageState.SelectedIndex = VM_ShokoServer.Instance.AniDB_MyList_StorageState;
            cboDeleteAction.SelectedIndex = (int)VM_ShokoServer.Instance.AniDB_MyList_DeleteType;
        }
    }
}
