using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for GroupFilterAdmin.xaml
    /// </summary>
    public partial class GroupFilterAdmin : UserControl
    {


        public GroupFilterAdmin()
        {
            InitializeComponent();


            cboBaseCondition.Items.Clear();
            cboBaseCondition.Items.Add(Shoko.Commons.Properties.Resources.GroupFilter_BaseCondition_IncludeAll);
            cboBaseCondition.Items.Add(Shoko.Commons.Properties.Resources.GroupFilter_BaseCondition_ExcludeAll);

            cboBaseConditionEditing.SelectedIndex = 0;
            cboBaseConditionEditing.Items.Clear();
            cboBaseConditionEditing.Items.Add(Shoko.Commons.Properties.Resources.GroupFilter_BaseCondition_IncludeAll);
            cboBaseConditionEditing.Items.Add(Shoko.Commons.Properties.Resources.GroupFilter_BaseCondition_ExcludeAll);

            cboBaseConditionEditing.SelectionChanged += new SelectionChangedEventHandler(cboBaseConditionEditing_SelectionChanged);


            DataContextChanged += new DependencyPropertyChangedEventHandler(GroupFilterAdmin_DataContextChanged);
            cboBaseCondition.SelectedIndex = 0;

            chkApplyToSeriesEditing.Click += new RoutedEventHandler(chkApplyToSeriesEditing_Click);

            lbFilterConditions_Editing.MouseDoubleClick += new MouseButtonEventHandler(lbFilterConditions_Editing_MouseDoubleClick);

            btnRandomSeries.Click += new RoutedEventHandler(btnRandomSeries_Click);
            btnRandomEpisode.Click += new RoutedEventHandler(btnRandomEpisode_Click);

            lbGroups.PreviewMouseWheel += LbGroups_PreviewMouseWheel;

        }

        private void LbGroups_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3);
            }
            catch { }
        }
        void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
        {
            VM_GroupFilter gf = DataContext as VM_GroupFilter;
            if (gf == null) return;

            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

            RandomEpisodeForm frm = new RandomEpisodeForm();
            frm.Owner = Window.GetWindow(this); ;
            frm.Init(RandomSeriesEpisodeLevel.GroupFilter, gf);
            bool? result = frm.ShowDialog();

        }

        void btnRandomSeries_Click(object sender, RoutedEventArgs e)
        {
            VM_GroupFilter gf = DataContext as VM_GroupFilter;
            if (gf == null) return;

            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

            RandomSeriesForm frm = new RandomSeriesForm();
            frm.Owner = Window.GetWindow(this); ;
            frm.Init(RandomSeriesEpisodeLevel.GroupFilter, gf);
            bool? result = frm.ShowDialog();
            if (result.HasValue && result.Value && frm.Series != null)
            {
                if (mainwdw == null) return;
                mainwdw.ShowPinnedSeries(frm.Series);
            }
        }

        void lbFilterConditions_Editing_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VM_GroupFilter gf = DataContext as VM_GroupFilter;
            if (gf == null) return;

            VM_GroupFilterCondition gfc = lbFilterConditions_Editing.SelectedItem as VM_GroupFilterCondition;
            if (gfc == null) return;

            try
            {

                GroupFilterConditionForm frm = new GroupFilterConditionForm();
                frm.Owner = Window.GetWindow(this);
                frm.Init(gf, gfc);
                bool? result = frm.ShowDialog();
                if (result.HasValue && result.Value == true)
                {

                    Window win = Window.GetWindow(this);
                    MainWindow main = win as MainWindow;
                    gf.IsBeingEdited = true;

                    //gf.FilterConditions.Add(gfc);

                    VM_MainListHelper.Instance.ViewGroupsForms.Filter = main.GroupFilter_GroupSearch;
                    VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void chkApplyToSeriesEditing_Click(object sender, RoutedEventArgs e)
        {
            chkApplyToSeries.IsChecked = chkApplyToSeriesEditing.IsChecked;

            VM_GroupFilter gf = DataContext as VM_GroupFilter;
            if (gf == null) return;

            gf.ApplyToSeries = chkApplyToSeriesEditing.IsChecked.Value ? 1 : 0;
        }

        void cboBaseConditionEditing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM_GroupFilter gf = DataContext as VM_GroupFilter;
            if (gf == null) return;

            if (cboBaseConditionEditing.SelectedIndex == 0)
                gf.BaseCondition = 1;
            else
                gf.BaseCondition = 2;

            cboBaseCondition.SelectedIndex = cboBaseConditionEditing.SelectedIndex;
        }

        void GroupFilterAdmin_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_GroupFilter gf = DataContext as VM_GroupFilter;
            if (gf == null) return;

            if (gf.BaseCondition == 1)
            {
                cboBaseConditionEditing.SelectedIndex = 0;
                cboBaseConditionEditing.SelectedIndex = 0;
            }
            else
            {
                cboBaseConditionEditing.SelectedIndex = 1;
                cboBaseConditionEditing.SelectedIndex = 1;
            }
        }
    }
}
