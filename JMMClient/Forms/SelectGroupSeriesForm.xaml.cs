using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for SelectGroupSeriesForm.xaml
    /// </summary>
    public partial class SelectGroupSeriesForm : Window
    {
        public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<AnimeGroupVM> AllGroups { get; set; }

        public ICollectionView ViewSeries { get; set; }
        public ObservableCollection<AnimeSeriesVM> AllSeries { get; set; }

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
            this.DialogResult = false;
            SelectedObject = null;
            this.Close();
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

            AllGroups = new ObservableCollection<AnimeGroupVM>();
            AllSeries = new ObservableCollection<AnimeSeriesVM>();

            try
            {

                ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
                ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

                ViewSeries = CollectionViewSource.GetDefaultView(AllSeries);
                ViewSeries.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));

                List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
                {
                    AnimeGroupVM grpNew = new AnimeGroupVM(grp);
                    AllGroups.Add(grpNew);
                }

                List<JMMServerBinary.Contract_AnimeSeries> sersRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllSeries(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                foreach (JMMServerBinary.Contract_AnimeSeries ser in sersRaw)
                {
                    AnimeSeriesVM serNew = new AnimeSeriesVM(ser);
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
            AnimeGroupVM grpvm = obj as AnimeGroupVM;
            if (grpvm != null)
            {
                return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
            }

            AnimeSeriesVM ser = obj as AnimeSeriesVM;
            if (ser != null)
            {
                return GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, txtGroupSearch.Text);
            }

            return false;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                this.SelectedObject = obj;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }
    }
}
