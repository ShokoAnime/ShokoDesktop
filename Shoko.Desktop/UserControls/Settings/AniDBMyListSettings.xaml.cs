using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public static readonly Dictionary<AniDBFileDeleteType, string> DeleteSettings = new Dictionary<AniDBFileDeleteType, string>();
        public static readonly Dictionary<AniDBFile_State, string> StorageSettings = new Dictionary<AniDBFile_State, string>();

        static AniDBMyListSettings()
        {
            DeleteSettings.Add(AniDBFileDeleteType.Delete, Commons.Properties.Resources.AniDBMyListDelete);
            DeleteSettings.Add(AniDBFileDeleteType.DeleteLocalOnly, Commons.Properties.Resources.AniDBMyListDeleteLocal);
            DeleteSettings.Add(AniDBFileDeleteType.MarkDeleted, Commons.Properties.Resources.AniDBMyListMarkDeleted);
            DeleteSettings.Add(AniDBFileDeleteType.MarkExternalStorage, Commons.Properties.Resources.AniDBMyListMarkExternal);
            DeleteSettings.Add(AniDBFileDeleteType.MarkDisk, Commons.Properties.Resources.AniDBMyListMarkDisk);
            DeleteSettings.Add(AniDBFileDeleteType.MarkUnknown, Commons.Properties.Resources.AniDBMyListMarkUnknown);

            StorageSettings.Add(AniDBFile_State.Unknown, Commons.Properties.Resources.AniDBMyListUnknown);
            StorageSettings.Add(AniDBFile_State.HDD, Commons.Properties.Resources.AniDBMyListHDD);
            StorageSettings.Add(AniDBFile_State.Disk, Commons.Properties.Resources.AniDBMyListDVD);
            StorageSettings.Add(AniDBFile_State.Remote, Commons.Properties.Resources.AniDBMyListRemote);
        }

        public AniDBMyListSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboStorageState.Items.Clear();
            cboStorageState.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListHDD);
            cboStorageState.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListDVD);
            cboStorageState.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListRemote);
            cboStorageState.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListUnknown);
            cboStorageState.SelectedIndex = 0;

            cboDeleteAction.Items.Clear();
            cboDeleteAction.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListDelete);
            cboDeleteAction.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListDeleteLocal);
            cboDeleteAction.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListMarkDeleted);
            cboDeleteAction.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListMarkExternal);
            cboDeleteAction.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListMarkDisk);
            cboDeleteAction.Items.Add(Shoko.Commons.Properties.Resources.AniDBMyListMarkUnknown);
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
            VM_ShokoServer.Instance.AniDB_MyList_DeleteType = DeleteSettings.Keys.FirstOrDefault(a => DeleteSettings[a] == (string) cboDeleteAction.SelectedItem);

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboStorageState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM_ShokoServer.Instance.AniDB_MyList_StorageState = (int)StorageSettings.Keys.FirstOrDefault(a => StorageSettings[a] == (string) cboStorageState.SelectedItem);

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void AniDBMyListSettings_Loaded(object sender, RoutedEventArgs e)
        {
            cboStorageState.SelectedIndex = cboStorageState.Items.IndexOf(StorageSettings[(AniDBFile_State) VM_ShokoServer.Instance.AniDB_MyList_StorageState]);
            cboDeleteAction.SelectedIndex = cboDeleteAction.Items.IndexOf(DeleteSettings[VM_ShokoServer.Instance.AniDB_MyList_DeleteType]);
        }
    }
}
