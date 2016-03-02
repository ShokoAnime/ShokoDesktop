using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace JMMClient
{
	public class MetroDashSection 
	{
		public DashboardMetroProcessType SectionType { get; set; }
		public bool Enabled { get; set; }

		public Visibility WinVisibility { get; set; }

		public bool Disabled
		{
			get { return !Enabled; }
		}

		public string SectionName
		{
			get
			{
				switch (SectionType)
				{
					case DashboardMetroProcessType.ContinueWatching: return JMMClient.Properties.Resources.Metro_Continue;
					case DashboardMetroProcessType.NewEpisodes: return JMMClient.Properties.Resources.Metro_New;
					case DashboardMetroProcessType.RandomSeries: return JMMClient.Properties.Resources.Metro_Random;
					case DashboardMetroProcessType.TraktActivity: return JMMClient.Properties.Resources.Metro_Trakt;
				}
				return JMMClient.Properties.Resources.Metro_Continue;
			}
		}
	}
}
