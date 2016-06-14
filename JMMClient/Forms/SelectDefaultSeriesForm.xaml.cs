using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for SelectDefaultSeriesForm.xaml
    /// </summary>
    public partial class SelectDefaultSeriesForm : Window
    {
        public static readonly DependencyProperty SeriesForGroupProperty = DependencyProperty.Register("SeriesForGroup",
            typeof(List<AnimeSeriesVM>), typeof(SelectDefaultSeriesForm), new UIPropertyMetadata(null, null));

        public List<AnimeSeriesVM> SeriesForGroup
        {
            get { return (List<AnimeSeriesVM>)GetValue(SeriesForGroupProperty); }
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
            this.DialogResult = false;
            SelectedSeriesID = null;
            this.Close();
        }

        public void Init(AnimeGroupVM grp)
        {
            SeriesForGroup = grp.AllAnimeSeries;
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(AnimeSeriesVM))
                {
                    this.Cursor = Cursors.Wait;
                    AnimeSeriesVM ser = obj as AnimeSeriesVM;
                    SelectedSeriesID = ser.AnimeSeriesID;

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
