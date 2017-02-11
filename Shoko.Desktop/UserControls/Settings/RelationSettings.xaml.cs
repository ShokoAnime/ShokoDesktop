using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for RelationSettings.xaml
    /// </summary>
    public partial class RelationSettings : UserControl
    {
        public RelationSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            chkRelationSettings_AutoGroupSeries.Click += chkRelationSettings_AutoGroupSeries_Click;
	        chkRelationSettings_AutoGroupSeriesUseScoreAlgorithm.Click += chkRelationSettings_AutoGroupSeriesUseScoreAlgorithm_Click;

	        chkRelationSettings_AllowDissimilarTitleExclusion.Click += new RoutedEventHandler(settingChanged);
	        chkRelationSettings_OVA.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_Movie.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_SameSetting.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_AltSetting.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_AltVersion.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_Character.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_SideStory.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_ParentStory.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_Summary.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_FullStory.Click += new RoutedEventHandler(settingChanged);
            chkRelationSettings_Other.Click += new RoutedEventHandler(settingChanged);

            btnRecreateGroups.Click += new RoutedEventHandler(btnRecreateGroups_Click);
        }

        void chkRelationSettings_AutoGroupSeries_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

	    void chkRelationSettings_AutoGroupSeriesUseScoreAlgorithm_Click(object sender, RoutedEventArgs e)
	    {
		    VM_ShokoServer.Instance.SaveServerSettingsAsync();
	    }

        void btnRecreateGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow.Cursor = Cursors.Wait;

                VM_ShokoServer.Instance.ShokoServices.RecreateAllGroups(false);
                VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                VM_MainListHelper.Instance.ShowChildWrappers(null);

                parentWindow.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }
    }
}
