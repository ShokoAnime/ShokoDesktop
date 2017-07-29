using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NLog;

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
            if (obj == null) return;
            obj = obj.Replace('`', '\'');
            try
            {
                Clipboard.SetDataObject(obj);
            }
            catch (COMException ex)
            {
                try
                {
                    Clipboard.SetText(obj);
                }
                catch (COMException exception)
                {
                    LogManager.GetCurrentClassLogger().Error($"There was an error copying to the clipboard: {exception}");
                }
            }
        }
    }
}
