using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using JMMClient.ViewModel;

namespace JMMClient
{
	public class MainListTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;


            if (element != null && item != null)
			{
				if (item.GetType() == typeof(GroupFilterVM))
				{
					return element.FindResource("MainList_GroupFilterTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeGroupVM))
				{
					switch (AppSettings.DisplayStyle_GroupList)
					{
						case 1: return element.FindResource("MainList_AnimeGroupTemplate") as DataTemplate;
						case 2: return element.FindResource("MainList_AnimeGroupSimpleTemplate") as DataTemplate;
						default: return element.FindResource("MainList_AnimeGroupTemplate") as DataTemplate;
					}
				}
				if (item.GetType() == typeof(AnimeSeriesVM))
				{
					return element.FindResource("MainList_AnimeSeriesTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeEpisodeTypeVM))
				{
					return element.FindResource("MainList_AnimeEpisodeTypeTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeEpisodeVM))
				{
					return element.FindResource("MainList_AnimeEpisodeTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(VideoDetailedVM))
				{
					return element.FindResource("MainList_VideoDetailedTemplate") as DataTemplate;
				}
			}

			return null;
		}
	}

	
}
