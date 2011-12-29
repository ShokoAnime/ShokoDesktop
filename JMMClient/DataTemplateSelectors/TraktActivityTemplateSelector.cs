using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using JMMClient.ViewModel;
using System.Windows.Controls;

namespace JMMClient
{
	public class TraktActivityTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null)
			{
				if (item.GetType() == typeof(Trakt_FriendVM))
				{
					return element.FindResource("TraktFriendTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(Trakt_ActivityScrobbleVM))
				{
					return element.FindResource("TraktActivityScrobbleTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(Trakt_ActivityShoutEpisodeVM))
				{
					return element.FindResource("TraktActivityShoutEpisodeTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(Trakt_ActivityShoutShowVM))
				{
					return element.FindResource("TraktActivityShoutShowTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(Trakt_FriendRequestVM))
				{
					return element.FindResource("TraktFriendRequestTemplate") as DataTemplate;
				}
				if (item.GetType() == typeof(Trakt_SignupVM))
				{
					return element.FindResource("TraktSignupTemplate") as DataTemplate;
				}
			}

			return null;
		}
	}
}
