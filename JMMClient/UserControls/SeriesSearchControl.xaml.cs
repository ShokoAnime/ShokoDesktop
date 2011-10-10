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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for SeriesSearchControl.xaml
	/// </summary>
	public partial class SeriesSearchControl : UserControl
	{
		

		public SeriesSearchControl()
		{
			InitializeComponent();

			//AllSeries = new ObservableCollection<AnimeSeriesVM>();

			btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
			txtSeriesSearch.TextChanged += new TextChangedEventHandler(txtSeriesSearch_TextChanged);

			this.GotFocus += new RoutedEventHandler(SeriesSearchControl_GotFocus);
		}

		void SeriesSearchControl_GotFocus(object sender, RoutedEventArgs e)
		{
			txtSeriesSearch.Focus();
		}

		void txtSeriesSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			
		}

		void btnClearSearch_Click(object sender, RoutedEventArgs e)
		{
			txtSeriesSearch.Text = "";
		}

		

		
	}
}
