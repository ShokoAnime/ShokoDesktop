using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using JMMClient.ViewModel;
using System.Windows.Controls;

namespace JMMClient
{
	public class DashRecentAdditionsTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null)
			{
				if (item.GetType() == typeof(AnimeEpisodeVM))
				{
					return element.FindResource("RecentAdditionsEpisode_Detailed") as DataTemplate; // re-use this template
				}
				if (item.GetType() == typeof(AnimeSeriesVM))
				{
					return element.FindResource("RecentAdditionsSeries_Detailed") as DataTemplate;
				}
				
			}

			return null;
		}
	}
}
