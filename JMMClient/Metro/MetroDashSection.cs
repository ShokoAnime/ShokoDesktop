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
					case DashboardMetroProcessType.ContinueWatching: return "Continue Watching";
					case DashboardMetroProcessType.NewEpisodes: return "New Episodes";
					case DashboardMetroProcessType.RandomSeries: return "Random Series";
					case DashboardMetroProcessType.TraktActivity: return "Trakt Activity";
				}
				return "Continue Watching";
			}
		}
	}
}
