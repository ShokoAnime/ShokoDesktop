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
using System.ComponentModel;
using System.Collections.ObjectModel;
using JMMClient.ViewModel;
using JMMClient.Forms;

namespace JMMClient.UserControls
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
			cboBaseCondition.Items.Add(Properties.Resources.GroupFilter_BaseCondition_IncludeAll);
			cboBaseCondition.Items.Add(Properties.Resources.GroupFilter_BaseCondition_ExcludeAll);

			cboBaseConditionEditing.SelectedIndex = 0;
			cboBaseConditionEditing.Items.Clear();
			cboBaseConditionEditing.Items.Add(Properties.Resources.GroupFilter_BaseCondition_IncludeAll);
			cboBaseConditionEditing.Items.Add(Properties.Resources.GroupFilter_BaseCondition_ExcludeAll);

			cboBaseConditionEditing.SelectionChanged += new SelectionChangedEventHandler(cboBaseConditionEditing_SelectionChanged);


			this.DataContextChanged += new DependencyPropertyChangedEventHandler(GroupFilterAdmin_DataContextChanged);
			cboBaseCondition.SelectedIndex = 0;

			chkApplyToSeriesEditing.Click += new RoutedEventHandler(chkApplyToSeriesEditing_Click);

			lbFilterConditions_Editing.MouseDoubleClick += new MouseButtonEventHandler(lbFilterConditions_Editing_MouseDoubleClick);

		}

		void lbFilterConditions_Editing_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			GroupFilterVM gf = this.DataContext as GroupFilterVM;
			if (gf == null) return;

			GroupFilterConditionVM gfc = lbFilterConditions_Editing.SelectedItem as GroupFilterConditionVM;
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
					//gf.FilterConditions.Add(gfc);

					MainListHelperVM.Instance.ViewGroupsForms.Filter = main.GroupFilter_GroupSearch;
					MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
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

			GroupFilterVM gf = this.DataContext as GroupFilterVM;
			if (gf == null) return;

			gf.ApplyToSeries = chkApplyToSeriesEditing.IsChecked.Value ? 1 : 0;
		}

		void cboBaseConditionEditing_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GroupFilterVM gf = this.DataContext as GroupFilterVM;
			if (gf == null) return;

			if (cboBaseConditionEditing.SelectedIndex == 0)
				gf.BaseCondition = 1;
			else
				gf.BaseCondition = 2;

			cboBaseCondition.SelectedIndex = cboBaseConditionEditing.SelectedIndex;
		}

		void GroupFilterAdmin_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			GroupFilterVM gf = this.DataContext as GroupFilterVM;
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
