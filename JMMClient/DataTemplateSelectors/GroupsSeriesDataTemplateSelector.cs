using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
	public class GroupsSeriesDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null)
			{
				if (item.GetType() == typeof(GroupFilterVM))
				{
					return element.FindResource("groupFilterDetailTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeGroupVM))
				{
					return element.FindResource("groupDetailTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeSeriesVM))
				{
					return element.FindResource("seriesDetailTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeEpisodeVM))
				{
					return element.FindResource("episodeDetailTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(VideoDetailedVM))
				{
					return element.FindResource("videoDetailTemplate") as DataTemplate;
				}
			}

			return null;
		}
	}
}
