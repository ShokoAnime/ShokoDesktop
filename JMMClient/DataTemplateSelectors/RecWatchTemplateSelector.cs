using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using JMMClient.ViewModel;
using JMMClient.UserControls;

namespace JMMClient
{
	public class RecWatchTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null)
			{
				if (item.GetType() == typeof(RecommendationVM))
				{
					return element.FindResource("RecWatch_Detailed") as DataTemplate;
				}
				if (item.GetType() == typeof(SyncVotesDummy))
				{
					return element.FindResource("MylistVotesDownloadTemplate") as DataTemplate;
				}
			}

			return null;
		}
	}
}
