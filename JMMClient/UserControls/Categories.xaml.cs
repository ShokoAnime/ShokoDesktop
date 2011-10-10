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
using JMMClient.ViewModel;
using System.Collections;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for CategoriesCollapsed.xaml
	/// </summary>
	public partial class Categories : UserControl
	{

		public Categories()
		{
			InitializeComponent();

			btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
		}

		void btnToggleExpander_Click(object sender, RoutedEventArgs e)
		{

			UserSettingsVM.Instance.CategoriesExpanded = !UserSettingsVM.Instance.CategoriesExpanded;
		}
	}
}
