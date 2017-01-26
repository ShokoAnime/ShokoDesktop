using System.Windows;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for DialogTextMultiline.xaml
    /// </summary>
    public partial class DialogTextMultiline : Window
    {
        public string EnteredText { get; set; }

        public DialogTextMultiline()
        {
            InitializeComponent();

            btnOK.Click += new RoutedEventHandler(btnOK_Click);
            Loaded += new RoutedEventHandler(DialogText_Loaded);
        }

        void DialogText_Loaded(object sender, RoutedEventArgs e)
        {
            txtData.Focus();
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            EnteredText = txtData.Text.Trim();
            Close();
        }

        public void Init(string prompt, string defaultText)
        {
            txtPrompt.Text = prompt;
            EnteredText = defaultText;
            txtData.Text = EnteredText;
        }
    }
}
