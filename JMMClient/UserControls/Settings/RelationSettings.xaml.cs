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
    /// Interaction logic for RelationSettings.xaml
    /// </summary>
    public partial class RelationSettings : UserControl
    {
        public RelationSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            EvaulateVisibility();

            chkRelationSettings_AutoGroupSeries.Click += chkRelationSettings_AutoGroupSeries_Click;

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
            JMMServerVM.Instance.SaveServerSettingsAsync();
            EvaulateVisibility();
        }

        private void EvaulateVisibility()
        {
            System.Windows.Visibility vis = System.Windows.Visibility.Collapsed;
            if (JMMServerVM.Instance.AutoGroupSeries) vis = System.Windows.Visibility.Visible;

            visRelations_Row1.Visibility = vis;
            visRelations_Row2.Visibility = vis;
            visRelations_Row3.Visibility = vis;
            visRelations_Info.Visibility = vis;

            btnRecreateGroups.Visibility = vis;

        }

        void btnRecreateGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow.Cursor = Cursors.Wait;

                JMMServerVM.Instance.clientBinaryHTTP.RecreateAllGroups();
                MainListHelperVM.Instance.RefreshGroupsSeriesData();
                MainListHelperVM.Instance.ShowChildWrappers(null);

                parentWindow.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }
    }
}
