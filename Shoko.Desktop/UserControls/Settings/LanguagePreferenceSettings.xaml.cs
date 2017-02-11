using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Languages;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
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
            Cursor = Cursors.Wait;
            VM_ShokoServer.Instance.RenameAllGroups();
            Cursor = Cursors.Arrow;
        }

        void chkLanguageUseSynonyms_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbSelectedLanguages.SelectedItem == null) return;

            NamingLanguage nLan = lbSelectedLanguages.SelectedItem as NamingLanguage;

            int newPos = VM_ShokoServer.Instance.MoveDownNamingLanguage(nLan.Language);
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

            int newPos = VM_ShokoServer.Instance.MoveUpNamingLanguage(nLan.Language);
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

            VM_ShokoServer.Instance.RemoveNamingLanguage(nLan.Language);
        }

        void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (lbUnselectedLanguages.SelectedItem == null) return;

            NamingLanguage nLan = lbUnselectedLanguages.SelectedItem as NamingLanguage;

            VM_ShokoServer.Instance.AddNamingLanguage(nLan.Language);
        }
    }
}
