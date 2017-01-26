using System.Windows;
using Shoko.Desktop.Enums;

namespace Shoko.Desktop.ViewModel.Metro
{
    public class MetroDashSection
    {
        public DashboardMetroProcessType SectionType { get; set; }
        public bool Enabled { get; set; }

        public Visibility WinVisibility { get; set; }

        public bool Disabled => !Enabled;

        public string SectionName
        {
            get
            {
                switch (SectionType)
                {
                    case DashboardMetroProcessType.ContinueWatching: return Properties.Resources.Metro_Continue;
                    case DashboardMetroProcessType.NewEpisodes: return Properties.Resources.Metro_New;
                    case DashboardMetroProcessType.RandomSeries: return Properties.Resources.Metro_Random;
                    case DashboardMetroProcessType.TraktActivity: return Properties.Resources.Metro_Trakt;
                }
                return Properties.Resources.Metro_Continue;
            }
        }
    }
}
