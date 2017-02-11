using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for MoveSeries.xaml
    /// </summary>
    public partial class MoveSeries : Window
    {
        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
            typeof(VM_AnimeSeries_User), typeof(MoveSeries), new UIPropertyMetadata(null, seriesCallback));

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register("SelectedGroup",
            typeof(VM_AnimeGroup_User), typeof(MoveSeries), new UIPropertyMetadata(null, groupCallback));

        public static readonly DependencyProperty IsNewGroupProperty = DependencyProperty.Register("IsNewGroup",
            typeof(bool), typeof(MoveSeries), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty IsExistingGroupProperty = DependencyProperty.Register("IsExistingGroup",
            typeof(bool), typeof(MoveSeries), new UIPropertyMetadata(true, null));

        public bool IsNewGroup
        {
            get { return (bool)GetValue(IsNewGroupProperty); }
            set { SetValue(IsNewGroupProperty, value); }
        }

        public bool IsExistingGroup
        {
            get { return (bool)GetValue(IsExistingGroupProperty); }
            set { SetValue(IsExistingGroupProperty, value); }
        }

        public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<VM_AnimeGroup_User> AllGroups { get; set; }

        //private VM_AnimeSeries_User animeSeries = null;

        public MoveSeries()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);
            btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
            btnOK.Click += new RoutedEventHandler(btnOK_Click);

            rbGroupExisting.Checked += new RoutedEventHandler(rbGroupExisting_Checked);
            rbGroupNew.Checked += new RoutedEventHandler(rbGroupNew_Checked);

            lbGroups.MouseDoubleClick += new MouseButtonEventHandler(lbGroups_MouseDoubleClick);
        }

        void rbGroupNew_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
            txtGroupName.Focus();
        }

        void rbGroupExisting_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        private void EvaluateRadioButtons()
        {
            IsNewGroup = rbGroupNew.IsChecked.Value;
            IsExistingGroup = rbGroupExisting.IsChecked.Value;
        }


        public VM_AnimeGroup_User SelectedGroup
        {
            get { return (VM_AnimeGroup_User)GetValue(SelectedGroupProperty); }
            set { SetValue(SelectedGroupProperty, value); }
        }

        public VM_AnimeSeries_User Series
        {
            get { return (VM_AnimeSeries_User)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        private static void seriesCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void groupCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        void lbGroups_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbGroups.SelectedItem == null) return;

            SelectedGroup = lbGroups.SelectedItem as VM_AnimeGroup_User;
            DialogResult = true;
            Close();
        }


        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            int? groupID = null;

            if (IsExistingGroup)
            {
                if (lbGroups.SelectedItem == null)
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_GroupSelectionRequired, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    lbGroups.Focus();
                    return;
                }
                else
                {
                    SelectedGroup = lbGroups.SelectedItem as VM_AnimeGroup_User;
                    DialogResult = true;
                    Close();
                }

            }

            if (IsNewGroup)
            {
                if (txtGroupName.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_GroupNameRequired, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtGroupName.Focus();
                    return;
                }

                VM_AnimeGroup_User grp = new VM_AnimeGroup_User();
                grp.GroupName = txtGroupName.Text.Trim();
                grp.SortName = txtGroupName.Text.Trim();
                grp.AnimeGroupParentID = null;
                grp.Description = "";
                grp.IsFave = 0;
                grp.IsManuallyNamed = 0;
                grp.OverrideDescription = 0;


                if (grp.Validate())
                {
                    grp.IsReadOnly = true;
                    grp.IsBeingEdited = false;
                    if (grp.Save())
                    {

                        VM_MainListHelper.Instance.ViewGroups.Refresh();
                        groupID = grp.AnimeGroupID;
                    }

                }
                SelectedGroup = grp;
                DialogResult = true;
                Close();
            }




        }

        void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtGroupSearch.Text = "";
        }



        public void Init(VM_AnimeSeries_User series)
        {
            AllGroups = new ObservableCollection<VM_AnimeGroup_User>();
            ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
            ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

            List<VM_AnimeGroup_User> grpsRaw = VM_ShokoServer.Instance.ShokoServices.GetAllGroups(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeGroup_User>();

            foreach (VM_AnimeGroup_User grpNew in grpsRaw)
            {
                AllGroups.Add(grpNew);
            }

            ViewGroups.Filter = GroupSearchFilter;

            Series = series;

            txtGroupName.Text = Series.SeriesName;
            txtGroupSortName.Text = Series.SeriesName;
        }

        void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewGroups.Refresh();
        }

        private bool GroupSearchFilter(object obj)
        {
            VM_AnimeGroup_User grpvm = obj as VM_AnimeGroup_User;
            if (grpvm == null) return true;

            return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
        }
    }
}
