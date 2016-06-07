using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for LanguagePreferenceSettings.xaml
    /// </summary>
    public partial class LanguagePreferenceSettings : UserControl
    {

        public LanguagePreferenceSettings()
        {
            InitializeComponent();

            btnMoveRight.Click += new RoutedEventHandler(btnMoveRight_Click);
            btnMoveLeft.Click += new RoutedEventHandler(btnMoveLeft_Click);
            btnMoveUp.Click += new RoutedEventHandler(btnMoveUp_Click);
            btnMoveDown.Click += new RoutedEventHandler(btnMoveDown_Click);

            chkLanguageUseSynonyms.Click += new RoutedEventHandler(chkLanguageUseSynonyms_Click);

            btnRenameGroups.Click += new RoutedEventHandler(btnRenameGroups_Click);
        }

        void btnRenameGroups_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            JMMServerVM.Instance.RenameAllGroups();
            this.Cursor = Cursors.Arrow;
        }

        void chkLanguageUseSynonyms_Click(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbSelectedLanguages.SelectedItem == null) return;

            NamingLanguage nLan = lbSelectedLanguages.SelectedItem as NamingLanguage;

            int newPos = JMMServerVM.Instance.MoveDownNamingLanguage(nLan.Language);
            if (newPos >= 0)
            {
                lbSelectedLanguages.SelectedIndex = newPos;
                lbSelectedLanguages.Focus();
            }
        }

        void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (lbSelectedLanguages.SelectedItem == null) return;

            NamingLanguage nLan = lbSelectedLanguages.SelectedItem as NamingLanguage;

            int newPos = JMMServerVM.Instance.MoveUpNamingLanguage(nLan.Language);
            if (newPos >= 0)
            {
                lbSelectedLanguages.SelectedIndex = newPos;
                lbSelectedLanguages.Focus();
            }
        }

        void btnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (lbSelectedLanguages.SelectedItem == null) return;

            NamingLanguage nLan = lbSelectedLanguages.SelectedItem as NamingLanguage;

            string lan = nLan.Language.ToUpper().Trim();
            if (lan == "EN" || lan == "X-JAT") return;

            JMMServerVM.Instance.RemoveNamingLanguage(nLan.Language);
        }

        void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (lbUnselectedLanguages.SelectedItem == null) return;

            NamingLanguage nLan = lbUnselectedLanguages.SelectedItem as NamingLanguage;

            JMMServerVM.Instance.AddNamingLanguage(nLan.Language);
        }
    }
}
