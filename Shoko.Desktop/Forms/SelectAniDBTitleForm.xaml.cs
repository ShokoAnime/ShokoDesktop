using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectAniDBTitle.xaml
    /// </summary>
    public partial class SelectAniDBTitleForm : Window
    {
        public static readonly DependencyProperty AllTitlesProperty = DependencyProperty.Register("AllTitles",
            typeof(List<VM_AnimeTitle>), typeof(SelectAniDBTitleForm), new UIPropertyMetadata(null, null));

        public List<VM_AnimeTitle> AllTitles
        {
            get { return (List<VM_AnimeTitle>)GetValue(AllTitlesProperty); }
            set { SetValue(AllTitlesProperty, value); }
        }

        public string SelectedTitle = "";

        public SelectAniDBTitleForm()
        {
            InitializeComponent();

            btnClose.Click += new RoutedEventHandler(btnClose_Click);
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedTitle = "";
            Close();
        }

        public void Init(List<VM_AnimeTitle> titles)
        {
            AllTitles = titles;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeTitle))
                {
                    Cursor = Cursors.Wait;
                    VM_AnimeTitle title = obj as VM_AnimeTitle;
                    SelectedTitle = title.Title;

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
