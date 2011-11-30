using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			cboStorageState.Items.Clear();
			cboStorageState.Items.Add("Unknown");
			cboStorageState.Items.Add("HDD");
			cboStorageState.Items.Add("CD/DVD");
			cboStorageState.SelectedIndex = 0;

			this.Loaded += new RoutedEventHandler(AniDBMyListSettings_Loaded);

			cboStorageState.SelectionChanged += new SelectionChangedEventHandler(cboStorageState_SelectionChanged);

			chkMyListAdd.Click += new RoutedEventHandler(settingChanged);
			chkMyListReadUnwatched.Click += new RoutedEventHandler(settingChanged);
			chkMyListReadWatched.Click += new RoutedEventHandler(settingChanged);
			chkMyListSetUnwatched.Click += new RoutedEventHandler(settingChanged);
			chkMyListSetWatched.Click += new RoutedEventHandler(settingChanged);
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
		}
	}
}
