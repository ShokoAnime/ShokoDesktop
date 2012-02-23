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

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for MissingEpsColumnsForm.xaml
	/// </summary>
	public partial class MissingEpsColumnsForm : Window
	{
		public MissingEpsColumnsForm()
		{
			InitializeComponent();

			btnConfirm.Click += new RoutedEventHandler(btnConfirm_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			string[] columns = AppSettings.MissingEpsExportColumns.Split(';');
			if (columns.Length != 7) return;

			chkAnimeName.IsChecked = columns[0] == "1";
			chkAnimeID.IsChecked = columns[1] == "1";
			chkEpisodeNumber.IsChecked = columns[2] == "1";
			chkEpisodeID.IsChecked = columns[3] == "1";
			chkFileSummary.IsChecked = columns[4] == "1";
			chkLinktoAnime.IsChecked = columns[5] == "1";
			chkLinktoEpisode.IsChecked = columns[6] == "1";
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		void btnConfirm_Click(object sender, RoutedEventArgs e)
		{
			string val = "";
			
			val += chkAnimeName.IsChecked.Value ? "1;" : "0;";
			val += chkAnimeID.IsChecked.Value ? "1;" : "0;";
			val += chkEpisodeNumber.IsChecked.Value ? "1;" : "0;";
			val += chkEpisodeID.IsChecked.Value ? "1;" : "0;";
			val += chkFileSummary.IsChecked.Value ? "1;" : "0;";
			val += chkLinktoAnime.IsChecked.Value ? "1;" : "0;";
			val += chkLinktoEpisode.IsChecked.Value ? "1;" : "0";

			AppSettings.MissingEpsExportColumns = val;

			this.Close();
		}
	}
}
