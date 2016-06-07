using NLog;
using System;
using System.IO;
using System.Xml.Serialization;

namespace JMMClient.AutoUpdates
{
    public class JMMAutoUpdatesHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static long ConvertToAbsoluteVersion(string version)
        {
            string[] numbers = version.Split('.');
            if (numbers.Length != 4) return 0;

            return (int.Parse(numbers[3]) * 100) +
                (int.Parse(numbers[2]) * 100 * 100) +
                (int.Parse(numbers[1]) * 100 * 100 * 100) +
                (int.Parse(numbers[0]) * 100 * 100 * 100 * 100);
        }

        public static AutoUpdates.JMMVersions GetLatestVersionInfo()
        {
            try
            {
                // get the latest version as according to the release
                string uri = string.Format("http://www.jmediamanager.org/latestdownloads/versions.xml");
                string xml = Utils.DownloadWebPage(uri);

                XmlSerializer x = new XmlSerializer(typeof(AutoUpdates.JMMVersions));
                AutoUpdates.JMMVersions myTest = (AutoUpdates.JMMVersions)x.Deserialize(new StringReader(xml));
                JMMServerVM.Instance.ApplicationVersionLatest = myTest.versions.DesktopVersionFriendly;

                return myTest;

            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
                return null;
            }
        }
    }
}
