using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for AnimeSeriesContainerControl.xaml
    /// </summary>
    public partial class AnimeSeriesContainerControl : UserControl
    {
        public static readonly DependencyProperty IsMetroDashProperty = DependencyProperty.Register("IsMetroDash",
            typeof(bool), typeof(AnimeSeriesContainerControl), new UIPropertyMetadata(true, null));

        /// <summary>
        /// Is this content is the simplified Series control, is it also being used on the metro dashboard
        /// </summary>
        public bool IsMetroDash
        {
            get { return (bool)GetValue(IsMetroDashProperty); }
            set
            {
                SetValue(IsMetroDashProperty, value);
            }
        }

        public AnimeSeriesContainerControl()
        {
            InitializeComponent();

            this.DataContextChanged += AnimeSeriesContainerControl_DataContextChanged;
        }

        private void AnimeSeriesContainerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // check the type of content
            try
            {
                if (this.DataContext == null) return;

                if (this.DataContext.GetType().Equals(typeof(AnimeSeriesVM)))
                {
                    AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
                    if (AppSettings.DisplaySeriesSimple)
                    {
                        AnimeSeriesSimplifiedControl ctrl = new AnimeSeriesSimplifiedControl();
                        ctrl.DataContext = ser;
                        this.DataContext = ctrl;
                    }
                    else
                    {
                        AnimeSeries ctrl = new AnimeSeries();
                        ctrl.DataContext = ser;
                        this.DataContext = ctrl;
                    }
                }

                if (this.DataContext.GetType().Equals(typeof(AnimeSeriesSimplifiedControl)))
                {
                    //Console.WriteLine("simple");
                    AnimeSeriesSimplifiedControl ctrl = this.DataContext as AnimeSeriesSimplifiedControl;

                    ctrl.btnBack.Visibility = Visibility.Collapsed;
                    ctrl.btnSwitchView.Visibility = Visibility.Visible;
                    
                }

                if (this.DataContext.GetType().Equals(typeof(AnimeSeries)))
                {
                    Console.WriteLine("full");
                }

            }
            catch { }
        }
    }
}
