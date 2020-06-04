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

                    AnimeSeries ctrl = new AnimeSeries();
                    ctrl.DataContext = ser;
                    DataContext = ctrl;

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
