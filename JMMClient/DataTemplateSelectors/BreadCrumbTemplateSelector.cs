using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using JMMClient.ViewModel;

namespace JMMClient
{
	public class BreadCrumbTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null)
			{
				if (item == null)
				{
					return element.FindResource("BreadCrumb_TopViewTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(GroupFilterVM))
				{
					return element.FindResource("BreadCrumb_GroupFilterTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeGroupVM))
				{
					return element.FindResource("BreadCrumb_GroupTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeSeriesVM))
				{
					return element.FindResource("BreadCrumb_SeriesTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeEpisodeTypeVM))
				{
					return element.FindResource("BreadCrumb_EpisodeTypeTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(AnimeEpisodeVM))
				{
					return element.FindResource("BreadCrumb_EpisodeTemplate") as DataTemplate;
				}
			}

			return null;
		}
	}
}
