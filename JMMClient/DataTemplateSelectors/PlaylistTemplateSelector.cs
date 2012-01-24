using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using JMMClient.ViewModel;

namespace JMMClient
{
	public class PlaylistTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null)
			{
				if (item.GetType() == typeof(PlaylistVM))
				{
					return element.FindResource("Playlist_PlaylistTemplate") as DataTemplate;
				}
				
				if (item.GetType() == typeof(AnimeSeriesVM))
				{
					return element.FindResource("Playlist_AnimeSeriesTemplate") as DataTemplate;
				}
				
				if (item.GetType() == typeof(AnimeEpisodeVM))
				{
					return element.FindResource("Playlist_AnimeEpisodeTemplate") as DataTemplate;
				}
			}

			return null;
		}
	}
}
