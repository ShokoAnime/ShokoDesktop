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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for VideoPlayerOptionsControl.xaml
	/// </summary>
	public partial class VideoPlayerOptionsControl : UserControl
	{
		public VideoPlayerOptionsControl()
		{
			InitializeComponent();

			cboAutoFileFirst.Items.Clear();
			cboAutoFileFirst.Items.Add("Best Source / Resolution");
			cboAutoFileFirst.SelectedIndex = AppSettings.AutoFileFirst;
			cboAutoFileFirst.SelectionChanged += new SelectionChangedEventHandler(cboAutoFileFirst_SelectionChanged);


			cboAutoFileSubsequent.Items.Clear();
			cboAutoFileSubsequent.Items.Add("Release Group From Previously Played Episode");
			cboAutoFileSubsequent.Items.Add("Best Source / Resolution");
			cboAutoFileSubsequent.SelectedIndex = AppSettings.AutoFileSubsequent;
			cboAutoFileSubsequent.SelectionChanged += new SelectionChangedEventHandler(cboAutoFileSubsequent_SelectionChanged);

			chkAutoSelectFile.IsChecked = AppSettings.AutoFileSingleEpisode;
			chkAutoSelectFile.Click += new RoutedEventHandler(chkAutoSelectFile_Click);
		}

		void chkAutoSelectFile_Click(object sender, RoutedEventArgs e)
		{
			AppSettings.AutoFileSingleEpisode = chkAutoSelectFile.IsChecked.Value;
		}

		void cboAutoFileSubsequent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			AppSettings.AutoFileSubsequent = cboAutoFileSubsequent.SelectedIndex;
		}

		void cboAutoFileFirst_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			AppSettings.AutoFileFirst = cboAutoFileFirst.SelectedIndex;
		}
	}
}
