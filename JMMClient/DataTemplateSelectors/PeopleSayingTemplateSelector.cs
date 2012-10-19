using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using JMMClient.ViewModel;

namespace JMMClient
{
	public class PeopleSayingTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null)
			{
				if (item.GetType() == typeof(Trakt_ShoutUserVM))
					return element.FindResource("TraktShoutTemplate") as DataTemplate;

				if (item.GetType() == typeof(AniDB_RecommendationVM))
					return element.FindResource("AniDBShoutTemplate") as DataTemplate;
				
			}

			return null;
		}
	}
}
