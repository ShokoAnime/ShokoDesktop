using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectGroupSeriesForm.xaml
    /// </summary>
    public partial class SelectGroupSeriesForm : Window
    {
        public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<VM_AnimeGroup_User> AllGroups { get; set; }

        public ICollectionView ViewSeries { get; set; }
        public ObservableCollection<VM_AnimeSeries_User> AllSeries { get; set; }

        public static readonly DependencyProperty IsGroupsProperty = DependencyProperty.Register("IsGroups",
            typeof(bool), typeof(SelectGroupSeriesForm), new UIPropertyMetadata(true, null));

        public bool IsGroups
        {
            get { return (bool)GetValue(IsGroupsProperty); }
            set { SetValue(IsGroupsProperty, value); }
        }

        public static readonly DependencyProperty IsSeriesProperty = DependencyProperty.Register("IsSeries",
            typeof(bool), typeof(SelectGroupSeriesForm), new UIPropertyMetadata(false, null));

        public bool IsSeries
        {
            get { return (bool)GetValue(IsSeriesProperty); }
            set { SetValue(IsSeriesProperty, value); }
        }

        public object SelectedObject = null;

        public SelectGroupSeriesForm()
        {
            InitializeComponent();

            txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);

            rbGroup.Checked += new RoutedEventHandler(rbGroup_Checked);
            rbSeries.Checked += new RoutedEventHandler(rbSeries_Checked);

            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedObject = null;
            Close();
        }

        void rbSeries_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        void rbGroup_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        private void EvaluateRadioButtons()
        {
            IsGroups = rbGroup.IsChecked.Value;
            IsSeries = rbSeries.IsChecked.Value;
        }

        void btnClearGroupSearch_Click(object sender, RoutedEventArgs e)
        {
            txtGroupSearch.Text = "";
        }

        public void Init()
        {

            rbGroup.IsChecked = true;
            rbSeries.IsChecked = false;

            AllGroups = new ObservableCollection<VM_AnimeGroup_User>();
            AllSeries = new ObservableCollection<VM_AnimeSeries_User>();

            try
            {

                ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
                ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

                ViewSeries = CollectionViewSource.GetDefaultView(AllSeries);
                ViewSeries.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));

                List<VM_AnimeGroup_User> grpsRaw = VM_MainListHelper.Instance.AllGroupsDictionary.Values
                    .OrderBy(a => a.SortName)
                    .ToList();

                foreach (VM_AnimeGroup_User grpNew in grpsRaw)
                {
                    AllGroups.Add(grpNew);
                }

                List<VM_AnimeSeries_User> sersRaw = VM_MainListHelper.Instance.AllSeriesDictionary.Values
                    .OrderBy(a => a.SortName)
                    .ToList();
                foreach (VM_AnimeSeries_User serNew in sersRaw)
                {
                    AllSeries.Add(serNew);
                }

                ViewGroups.Filter = GroupSeriesSearchFilter;
                ViewSeries.Filter = GroupSeriesSearchFilter;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewGroups.Refresh();
            ViewSeries.Refresh();
        }

        private bool GroupSeriesSearchFilter(object obj)
        {
            VM_AnimeGroup_User grpvm = obj as VM_AnimeGroup_User;
            if (grpvm != null)
            {
                return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
            }

            VM_AnimeSeries_User ser = obj as VM_AnimeSeries_User;
            if (ser != null)
            {
                return GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, txtGroupSearch.Text);
            }

            return false;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                SelectedObject = obj;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }
    }
}
