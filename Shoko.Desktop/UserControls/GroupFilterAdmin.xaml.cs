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
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for GroupFilterAdmin.xaml
    /// </summary>
    public partial class GroupFilterAdmin
    {
        public GroupFilterAdmin()
        {
            InitializeComponent();


            cboBaseCondition.Items.Clear();
            cboBaseCondition.Items.Add(Commons.Properties.Resources.GroupFilter_BaseCondition_IncludeAll);
            cboBaseCondition.Items.Add(Commons.Properties.Resources.GroupFilter_BaseCondition_ExcludeAll);

            cboBaseConditionEditing.SelectedIndex = 0;
            cboBaseConditionEditing.Items.Clear();
            cboBaseConditionEditing.Items.Add(Commons.Properties.Resources.GroupFilter_BaseCondition_IncludeAll);
            cboBaseConditionEditing.Items.Add(Commons.Properties.Resources.GroupFilter_BaseCondition_ExcludeAll);

            cboBaseConditionEditing.SelectionChanged += cboBaseConditionEditing_SelectionChanged;


            DataContextChanged += GroupFilterAdmin_DataContextChanged;
            cboBaseCondition.SelectedIndex = 0;

            chkApplyToSeriesEditing.Click += chkApplyToSeriesEditing_Click;

            lbFilterConditions_Editing.MouseDoubleClick += lbFilterConditions_Editing_MouseDoubleClick;

            btnRandomSeries.Click += btnRandomSeries_Click;
            btnRandomEpisode.Click += btnRandomEpisode_Click;

            lbGroups.PreviewMouseWheel += LbGroups_PreviewMouseWheel;

        }

        private void LbGroups_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3D);
            }
            catch
            {
                // ignore
            }
        }
        void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is VM_GroupFilter gf)) return;

            RandomEpisodeForm frm = new RandomEpisodeForm {Owner = Window.GetWindow(this)};
            frm.Init(RandomSeriesEpisodeLevel.GroupFilter, gf);
            frm.ShowDialog();
        }

        void btnRandomSeries_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is VM_GroupFilter gf)) return;

            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

            RandomSeriesForm frm = new RandomSeriesForm {Owner = Window.GetWindow(this)};
            frm.Init(RandomSeriesEpisodeLevel.GroupFilter, gf);
            bool? result = frm.ShowDialog();
            if (result.HasValue && result.Value && frm.Series != null)
                mainwdw?.ShowPinnedSeries(frm.Series);
        }

        void lbFilterConditions_Editing_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is VM_GroupFilter gf)) return;

            if (!(lbFilterConditions_Editing.SelectedItem is VM_GroupFilterCondition gfc)) return;

            try
            {

                GroupFilterConditionForm frm = new GroupFilterConditionForm {Owner = Window.GetWindow(this)};
                frm.Init(gf, gfc);
                bool? result = frm.ShowDialog();
                if (!result.HasValue || !result.Value) return;
                Window win = Window.GetWindow(this);
                MainWindow main = win as MainWindow;
                gf.IsBeingEdited = true;

                if (main == null) return;
                VM_MainListHelper.Instance.ViewGroupsForms.Filter = main.GroupFilter_GroupSearch;
                VM_MainListHelper.Instance.SetGroupFilterSortingOnForms(gf);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void chkApplyToSeriesEditing_Click(object sender, RoutedEventArgs e)
        {
            chkApplyToSeries.IsChecked = chkApplyToSeriesEditing.IsChecked;

            if (!(DataContext is VM_GroupFilter gf)) return;

            gf.ApplyToSeries = chkApplyToSeriesEditing.IsChecked != null && chkApplyToSeriesEditing.IsChecked.Value ? 1 : 0;
        }

        void cboBaseConditionEditing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(DataContext is VM_GroupFilter gf)) return;

            gf.BaseCondition = cboBaseConditionEditing.SelectedIndex == 0 ? 1 : 2;

            cboBaseCondition.SelectedIndex = cboBaseConditionEditing.SelectedIndex;
        }

        void GroupFilterAdmin_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(DataContext is VM_GroupFilter gf)) return;

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
