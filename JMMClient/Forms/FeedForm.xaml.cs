using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for AboutForm.xaml
	/// </summary>
	public partial class FeedForm : Window
	{
        public FeedForm()
        {
            InitializeComponent();
        }

        private void lnkGoToArt_Click(object sender, RoutedEventArgs e)
        {
            String url = (sender as Hyperlink).Tag as String;

            if (String.IsNullOrWhiteSpace(url)) return;

            System.Diagnostics.Process.Start(url);
        }
    }
}
