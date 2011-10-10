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

		//private AnimeSeriesVM animeSeries = null;

		public MoveSeries()
		{
			InitializeComponent();

			txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);
			btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
			btnOK.Click += new RoutedEventHandler(btnOK_Click);

			lbGroups.MouseDoubleClick += new MouseButtonEventHandler(lbGroups_MouseDoubleClick);
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
			if (lbGroups.SelectedItem == null)
			{
				MessageBox.Show("Select a group first", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			SelectedGroup = lbGroups.SelectedItem as AnimeGroupVM;
			this.DialogResult = true;
			this.Close();
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
