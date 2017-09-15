using System;
using System.Windows;
using System.Windows.Controls;
using NLog;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
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

            DataContextChanged += AnimeSeriesContainerControl_DataContextChanged;
        }

        private void AnimeSeriesContainerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // check the type of content
            try
            {
                if (DataContext == null) return;

                if (DataContext.GetType().Equals(typeof(VM_AnimeSeries_User)))
                {
                    VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                    if (AppSettings.DisplaySeriesSimple)
                    {
                        AnimeSeriesSimplifiedControl ctrl = new AnimeSeriesSimplifiedControl();
                        ctrl.DataContext = ser;
                        DataContext = ctrl;
                    }
                    else
                    {
                        AnimeSeries ctrl = new AnimeSeries();
                        ctrl.DataContext = ser;
                        DataContext = ctrl;
                    }
                }

                if (DataContext.GetType().Equals(typeof(AnimeSeriesSimplifiedControl)))
                {
                    //Console.WriteLine("simple");
                    AnimeSeriesSimplifiedControl ctrl = DataContext as AnimeSeriesSimplifiedControl;

                    ctrl.btnBack.Visibility = Visibility.Collapsed;
                    ctrl.btnSwitchView.Visibility = Visibility.Visible;

                }

                if (DataContext.GetType().Equals(typeof(AnimeSeries)))
                {
                    Console.WriteLine("full");
                }

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }
        }
    }
}
