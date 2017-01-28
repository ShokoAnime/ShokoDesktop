using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Shoko.Models.Enums;

namespace Shoko.Desktop.ViewModel
{
    public class VM_WatchedStatusContainer
    {
        public WatchedStatus WatchedStatus { get; set; }
        public string WatchedStatusDescription { get; set; }

        public VM_WatchedStatusContainer()
        {
        }

        public VM_WatchedStatusContainer(WatchedStatus status, string desc)
        {
            WatchedStatus = status;
            WatchedStatusDescription = desc;
        }

        public static List<VM_WatchedStatusContainer> GetAll()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            List<VM_WatchedStatusContainer> statuses = new List<VM_WatchedStatusContainer> {new VM_WatchedStatusContainer(WatchedStatus.All, Shoko.Commons.Properties.Resources.Episodes_Watched_All), new VM_WatchedStatusContainer(WatchedStatus.Unwatched, Shoko.Commons.Properties.Resources.Episodes_Watched_Unwatched), new VM_WatchedStatusContainer(WatchedStatus.Watched, Shoko.Commons.Properties.Resources.Episodes_Watched_Watched)};
            return statuses;
        }
    }
}