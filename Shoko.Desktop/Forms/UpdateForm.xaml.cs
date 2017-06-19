using System;
using System.Windows;
using NLog;
using Shoko.Desktop.AutoUpdates;
using Shoko.Desktop.Utilities;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for UpdateForm.xaml
    /// </summary>
    public partial class UpdateForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public UpdateForm()
        {
            InitializeComponent();
            tbUpdateAvailable.Visibility = IsNewVersionAvailable() ? Visibility.Visible : Visibility.Hidden;
        }

        public bool IsNewVersionAvailable()
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                Utils.GetApplicationVersion(a);

                System.Reflection.AssemblyName an = a.GetName();
                var verCurrent = (an.Version.Revision * 100) +
                                 (an.Version.Build * 100 * 100) +
                                 (an.Version.Minor * 100 * 100 * 100) +
                                 (an.Version.Major * 100 * 100 * 100 * 100);

                var verNew = ShokoAutoUpdatesHelper.ConvertToAbsoluteVersion(
                    ShokoAutoUpdatesHelper.GetLatestVersionNumber(AppSettings.UpdateChannel));

                if (verNew > verCurrent)
                    return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
            return false;
        }
    }
}
