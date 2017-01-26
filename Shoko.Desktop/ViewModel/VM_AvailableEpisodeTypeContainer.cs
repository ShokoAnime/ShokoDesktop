using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Shoko.Models.Enums;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_AvailableEpisodeTypeContainer
    {
        public AvailableEpisodeType AvailableEpisodeType { get; set; }
        public string AvailableEpisodeTypeDescription { get; set; }

        public VM_AvailableEpisodeTypeContainer()
        {
        }

        public VM_AvailableEpisodeTypeContainer(AvailableEpisodeType eptype, string desc)
        {
            AvailableEpisodeType = eptype;
            AvailableEpisodeTypeDescription = desc;
        }

        public static List<VM_AvailableEpisodeTypeContainer> GetAll()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            List<VM_AvailableEpisodeTypeContainer> eptypes = new List<VM_AvailableEpisodeTypeContainer> {new VM_AvailableEpisodeTypeContainer(AvailableEpisodeType.All, Properties.Resources.Episodes_AvAll), new VM_AvailableEpisodeTypeContainer(AvailableEpisodeType.Available, Properties.Resources.Episodes_AvOnly), new VM_AvailableEpisodeTypeContainer(AvailableEpisodeType.NoFiles, Properties.Resources.Episodes_AvMissing)};
            return eptypes;
        }
    }
}