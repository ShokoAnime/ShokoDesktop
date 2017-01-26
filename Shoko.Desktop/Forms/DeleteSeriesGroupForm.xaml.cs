using System.Windows;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for DeleteSeriesGroupForm.xaml
    /// </summary>
    public partial class DeleteSeriesGroupForm : Window
    {
        public bool DeleteFiles { get; set; }
        public bool DeleteGroups { get; set; }

        public DeleteSeriesGroupForm()
        {
            InitializeComponent();

            DeleteFiles = false;
            DeleteGroups = true;
            btnOK.Click += new RoutedEventHandler(btnOK_Click);
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DeleteFiles = chkDeleteFiles.IsChecked.Value;
            DeleteGroups = chkDeleteGroups.IsChecked.Value;

            DialogResult = true;
            Close();
        }
    }
}
