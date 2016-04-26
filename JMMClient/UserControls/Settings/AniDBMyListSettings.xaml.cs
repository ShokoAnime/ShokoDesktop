using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JMMClient.UserControls
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

			this.Loaded += new RoutedEventHandler(AniDBMyListSettings_Loaded);

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
			JMMServerVM.Instance.AniDB_MyList_DeleteType = (AniDBFileDeleteType)cboDeleteAction.SelectedIndex;

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboStorageState_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			JMMServerVM.Instance.AniDB_MyList_StorageState = cboStorageState.SelectedIndex;
			
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void AniDBMyListSettings_Loaded(object sender, RoutedEventArgs e)
		{
			cboStorageState.SelectedIndex = JMMServerVM.Instance.AniDB_MyList_StorageState;
			cboDeleteAction.SelectedIndex = (int)JMMServerVM.Instance.AniDB_MyList_DeleteType;
		}
	}
}
