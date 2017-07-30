using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NLog;
using Shoko.Desktop.Utilities;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for Titles.xaml
    /// </summary>
    public partial class Titles : UserControl
    {
        public Titles()
        {
            InitializeComponent();
        }

        private void CommandBinding_SelectTextAndCopy(object sender, ExecutedRoutedEventArgs e)
        {
            string obj = e.Parameter as string;
            Utils.CopyToClipboard(obj);
        }
    }
}
