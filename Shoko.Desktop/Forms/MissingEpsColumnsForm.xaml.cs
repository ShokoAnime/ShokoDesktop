using System.Windows;

namespace Shoko.Desktop.Forms
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
            if (columns.Length != 8) return;

            chkAnimeName.IsChecked = columns[0] == "1";
            chkAnimeID.IsChecked = columns[1] == "1";
            chkEpisodeNumber.IsChecked = columns[2] == "1";
            chkEpisodeID.IsChecked = columns[3] == "1";
            chkFileSummary.IsChecked = columns[4] == "1";
            chkGroupSummary.IsChecked = columns[5] == "1";
            chkLinktoAnime.IsChecked = columns[6] == "1";
            chkLinktoEpisode.IsChecked = columns[7] == "1";
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string val = "";

            val += chkAnimeName.IsChecked.Value ? "1;" : "0;";
            val += chkAnimeID.IsChecked.Value ? "1;" : "0;";
            val += chkEpisodeNumber.IsChecked.Value ? "1;" : "0;";
            val += chkEpisodeID.IsChecked.Value ? "1;" : "0;";
            val += chkFileSummary.IsChecked.Value ? "1;" : "0;";
            val += chkGroupSummary.IsChecked.Value ? "1;" : "0;";
            val += chkLinktoAnime.IsChecked.Value ? "1;" : "0;";
            val += chkLinktoEpisode.IsChecked.Value ? "1;" : "0";

            AppSettings.MissingEpsExportColumns = val;

            Close();
        }
    }
}
