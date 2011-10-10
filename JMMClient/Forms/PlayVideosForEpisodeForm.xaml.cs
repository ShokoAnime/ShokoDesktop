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
using System.Windows.Shapes;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for PlayVideosForEpisodeForm.xaml
	/// </summary>
	public partial class PlayVideosForEpisodeForm : Window
	{

		public PlayVideosForEpisodeForm()
		{
			InitializeComponent();
		}

		public void Init(AnimeEpisodeVM episode)
		{
			this.DataContext = episode;
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
	}
}
