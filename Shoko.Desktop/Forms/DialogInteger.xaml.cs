using System.Windows;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for DialogInteger.xaml
    /// </summary>
    public partial class DialogInteger : Window
    {
        public int EnteredInteger { get; set; }

        public DialogInteger()
        {
            InitializeComponent();

            btnOK.Click += new RoutedEventHandler(btnOK_Click);
            Loaded += new RoutedEventHandler(DialogInteger_Loaded);
        }

        void DialogInteger_Loaded(object sender, RoutedEventArgs e)
        {

            udInput.Focus();
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            EnteredInteger = udInput.Value.Value;
            Close();
        }

        public void Init(string prompt, int defaultInt, int minValue, int maxValue)
        {
            txtPrompt.Text = prompt;
            EnteredInteger = defaultInt;
            udInput.Value = EnteredInteger;
            udInput.Minimum = minValue;
            udInput.Maximum = maxValue;
        }
    }
}
