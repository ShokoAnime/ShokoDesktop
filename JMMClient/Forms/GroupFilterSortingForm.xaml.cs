using JMMClient.ViewModel;
using System;
using System.Windows;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for GroupFilterSortingForm.xaml
    /// </summary>
    public partial class GroupFilterSortingForm : Window
    {
        public GroupFilterVM groupFilter = null;
        public GroupFilterSortingCriteria groupFilterSortingCriteria = null;

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
                this.DialogResult = false;
                this.Close();
            }

            // get the details from the form
            groupFilterSortingCriteria.GroupFilterID = groupFilter.GroupFilterID;
            groupFilterSortingCriteria.SortType = GroupFilterHelper.GetEnumForText_Sorting(cboSortType.SelectedItem.ToString());
            groupFilterSortingCriteria.SortDirection = GroupFilterHelper.GetEnumForText_SortDirection(cboDirection.SelectedItem.ToString());

            this.DialogResult = true;
            this.Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void Init(GroupFilterVM gf, GroupFilterSortingCriteria gfsc)
        {
            groupFilter = gf;
            groupFilterSortingCriteria = gfsc;

            try
            {
                cboSortType.Items.Clear();
                foreach (string stype in GroupFilterHelper.GetAllSortTypes())
                {
                    if (gf != null)
                    {
                        bool alreadyExists = false;
                        foreach (GroupFilterSortingCriteria gfsc_old in gf.SortCriteriaList)
                        {
                            if (GroupFilterHelper.GetTextForEnum_Sorting(gfsc_old.SortType) == stype)
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
                cboDirection.Items.Add(GroupFilterHelper.GetTextForEnum_SortDirection(GroupFilterSortDirection.Asc));
                cboDirection.Items.Add(GroupFilterHelper.GetTextForEnum_SortDirection(GroupFilterSortDirection.Desc));
                cboDirection.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }
    }
}
