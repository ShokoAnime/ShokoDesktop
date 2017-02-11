using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectTvDBLanguage.xaml
    /// </summary>
    public partial class SelectTvDBLanguage : Window
    {
        public static readonly DependencyProperty AllLanguagesProperty = DependencyProperty.Register("AllLanguages",
            typeof(List<VM_TvDB_Language>), typeof(SelectTvDBLanguage), new UIPropertyMetadata(null, null));

        public List<VM_TvDB_Language> AllLanguages
        {
            get { return (List<VM_TvDB_Language>)GetValue(AllLanguagesProperty); }
            set { SetValue(AllLanguagesProperty, value); }
        }

        public string SelectedLanguage = "";

        public SelectTvDBLanguage()
        {
            InitializeComponent();

            btnClose.Click += new RoutedEventHandler(btnClose_Click);
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedLanguage = "";
            Close();
        }

        public void Init(List<VM_TvDB_Language> lans)
        {
            AllLanguages = lans;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_TvDB_Language))
                {
                    Cursor = Cursors.Wait;
                    VM_TvDB_Language lan = obj as VM_TvDB_Language;
                    SelectedLanguage = lan.Abbreviation;

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
