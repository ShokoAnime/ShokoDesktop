using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for SelectTvDBLanguage.xaml
    /// </summary>
    public partial class SelectTvDBLanguage : Window
    {
        public static readonly DependencyProperty AllLanguagesProperty = DependencyProperty.Register("AllLanguages",
            typeof(List<TvDB_LanguageVM>), typeof(SelectTvDBLanguage), new UIPropertyMetadata(null, null));

        public List<TvDB_LanguageVM> AllLanguages
        {
            get { return (List<TvDB_LanguageVM>)GetValue(AllLanguagesProperty); }
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
            this.DialogResult = false;
            SelectedLanguage = "";
            this.Close();
        }

        public void Init(List<TvDB_LanguageVM> lans)
        {
            AllLanguages = lans;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(TvDB_LanguageVM))
                {
                    this.Cursor = Cursors.Wait;
                    TvDB_LanguageVM lan = obj as TvDB_LanguageVM;
                    SelectedLanguage = lan.Abbreviation;

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
