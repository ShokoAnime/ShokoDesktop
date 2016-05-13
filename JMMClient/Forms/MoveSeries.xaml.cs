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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;
using System.Threading;
using System.Globalization;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for MoveSeries.xaml
	/// </summary>
	public partial class MoveSeries : Window
	{
		public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
			typeof(AnimeSeriesVM), typeof(MoveSeries), new UIPropertyMetadata(null, seriesCallback));

		public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register("SelectedGroup",
			typeof(AnimeGroupVM), typeof(MoveSeries), new UIPropertyMetadata(null, groupCallback));

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

		//private AnimeSeriesVM animeSeries = null;

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

		
		public AnimeGroupVM SelectedGroup
		{
			get { return (AnimeGroupVM)GetValue(SelectedGroupProperty); }
			set { SetValue(SelectedGroupProperty, value); }
		}

		public AnimeSeriesVM Series
		{
			get { return (AnimeSeriesVM)GetValue(SeriesProperty); }
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

			SelectedGroup = lbGroups.SelectedItem as AnimeGroupVM;
			this.DialogResult = true;
			this.Close();
		}


		void btnOK_Click(object sender, RoutedEventArgs e)
		{
			int? groupID = null;

			if (IsExistingGroup)
			{
				if (lbGroups.SelectedItem == null)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_GroupSelectionRequired, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					lbGroups.Focus();
					return;
				}
				else
				{
					SelectedGroup = lbGroups.SelectedItem as AnimeGroupVM;
					this.DialogResult = true;
					this.Close();
				}

			}

			if (IsNewGroup)
			{
				if (txtGroupName.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_GroupNameRequired, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtGroupName.Focus();
					return;
				}

				AnimeGroupVM grp = new AnimeGroupVM();
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
						MainListHelperVM.Instance.AllGroups.Add(grp);
						MainListHelperVM.Instance.AllGroupsDictionary[grp.AnimeGroupID.Value] = grp;
						MainListHelperVM.Instance.ViewGroups.Refresh();
						groupID = grp.AnimeGroupID;
					}

				}
				SelectedGroup = grp;
				this.DialogResult = true;
				this.Close();
			}



			
		}

		void btnClearSearch_Click(object sender, RoutedEventArgs e)
		{
			txtGroupSearch.Text = "";
		}

		

		public void Init(AnimeSeriesVM series)
		{
			Series = series;
			MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupSearchFilter;
			MainListHelperVM.Instance.SetGroupFilterSortingOnForms(null);

			txtGroupName.Text = Series.SeriesName;
			txtGroupSortName.Text = Series.SeriesName;
		}

		void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainListHelperVM.Instance.ViewGroupsForms.Refresh();
		}

		private bool GroupSearchFilter(object obj)
		{
			AnimeGroupVM grpvm = obj as AnimeGroupVM;
			if (grpvm == null) return true;

			return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
		}
	}
}
