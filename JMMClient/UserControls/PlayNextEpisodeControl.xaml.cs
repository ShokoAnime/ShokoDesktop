using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for PlayNextEpisodeControl.xaml
	/// </summary>
	public partial class PlayNextEpisodeControl : UserControl
	{
		public static readonly DependencyProperty EpisodeExistsProperty = DependencyProperty.Register("EpisodeExists",
			typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(false, null));

		public bool EpisodeExists
		{
			get { return (bool)GetValue(EpisodeExistsProperty); }
			set { SetValue(EpisodeExistsProperty, value); }
		}

		public static readonly DependencyProperty EpisodeMissingProperty = DependencyProperty.Register("EpisodeMissing",
			typeof(bool), typeof(PlayNextEpisodeControl), new UIPropertyMetadata(false, null));

		public bool EpisodeMissing
		{
			get { return (bool)GetValue(EpisodeMissingProperty); }
			set { SetValue(EpisodeMissingProperty, value); }
		}

		public PlayNextEpisodeControl()
		{
			InitializeComponent();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlayNextEpisodeControl_DataContextChanged);
		}

		void PlayNextEpisodeControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				if (this.DataContext == null)
				{
					EpisodeExists = false;
					EpisodeMissing = true;
				}
				else
				{
					EpisodeExists = true;
					EpisodeMissing = false;
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_PlayVideo(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					Utils.PlayVideo(vid);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
				if (ep.FilesForEpisode.Count > 0)
					Utils.PlayVideo(ep.FilesForEpisode[0]);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		
	}
}
