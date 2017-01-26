using System;
using System.Windows;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for GroupFilterSortingForm.xaml
    /// </summary>
    public partial class GroupFilterSortingForm : Window
    {
        public VM_GroupFilter groupFilter = null;
        public VM_GroupFilterSortingCriteria groupFilterSortingCriteria = null;

        public GroupFilterSortingForm()
        {
            InitializeComponent();

            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnConfirm.Click += new RoutedEventHandler(btnConfirm_Click);
        }

        void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (cboSortType.Items.Count == 0)
            {
                DialogResult = false;
                Close();
            }

            // get the details from the form
            groupFilterSortingCriteria.GroupFilterID = groupFilter.GroupFilterID;
            groupFilterSortingCriteria.SortType = Commons.Extensions.Models.GetEnumForText_Sorting(cboSortType.SelectedItem.ToString());
            groupFilterSortingCriteria.SortDirection = Commons.Extensions.Models.GetEnumForText_SortDirection(cboDirection.SelectedItem.ToString());

            DialogResult = true;
            Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void Init(VM_GroupFilter gf, VM_GroupFilterSortingCriteria gfsc)
        {
            groupFilter = gf;
            groupFilterSortingCriteria = gfsc;

            try
            {
                cboSortType.Items.Clear();
                foreach (string stype in Commons.Extensions.Models.GetAllSortTypes())
                {
                    if (gf != null)
                    {
                        bool alreadyExists = false;
                        foreach (VM_GroupFilterSortingCriteria gfsc_old in gf.SortCriteriaList)
                        {
                            if (gfsc_old.SortType.GetTextForEnum_Sorting() == stype)
                            {
                                alreadyExists = true;
                                break;
                            }
                        }

                        if (!alreadyExists) cboSortType.Items.Add(stype);
                    }
                }
                if (cboSortType.Items.Count > 0)
                    cboSortType.SelectedIndex = 0;



                cboDirection.Items.Clear();
                cboDirection.Items.Add(Commons.Extensions.Models.GetTextForEnum_SortDirection(GroupFilterSortDirection.Asc));
                cboDirection.Items.Add(Commons.Extensions.Models.GetTextForEnum_SortDirection(GroupFilterSortDirection.Desc));
                cboDirection.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }
    }
}
