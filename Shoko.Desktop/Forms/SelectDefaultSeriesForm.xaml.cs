using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectDefaultSeriesForm.xaml
    /// </summary>
    public partial class SelectDefaultSeriesForm : Window
    {
        public static readonly DependencyProperty SeriesForGroupProperty = DependencyProperty.Register("SeriesForGroup",
            typeof(List<VM_AnimeSeries_User>), typeof(SelectDefaultSeriesForm), new UIPropertyMetadata(null, null));

        public List<VM_AnimeSeries_User> SeriesForGroup
        {
            get { return (List<VM_AnimeSeries_User>)GetValue(SeriesForGroupProperty); }
            set { SetValue(SeriesForGroupProperty, value); }
        }

        public int? SelectedSeriesID = null;

        public SelectDefaultSeriesForm()
        {
            InitializeComponent();

            btnClose.Click += new RoutedEventHandler(btnClose_Click);
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedSeriesID = null;
            Close();
        }

        public void Init(VM_AnimeGroup_User grp)
        {
            SeriesForGroup = grp.AllAnimeSeries;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeSeries_User))
                {
                    Cursor = Cursors.Wait;
                    VM_AnimeSeries_User ser = obj as VM_AnimeSeries_User;
                    SelectedSeriesID = ser.AnimeSeriesID;

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
