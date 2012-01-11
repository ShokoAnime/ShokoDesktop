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
	/// Interaction logic for AnimeSeries_Hulu.xaml
	/// </summary>
	public partial class AnimeSeries_Hulu : UserControl
	{
		public AnimeSeries_Hulu()
		{
			InitializeComponent();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeSeries_Hulu_DataContextChanged);
		}

		void AnimeSeries_Hulu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ShowNextEpisode();

			ucExternalLinks.DataContext = this.DataContext;
		}

		private void ShowNextEpisode()
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			JMMServerBinary.Contract_AnimeEpisode ep = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisode(ser.AnimeSeriesID.Value,
				JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
			if (ep != null)
			{
				AnimeEpisodeVM aniep = new AnimeEpisodeVM(ep);
				aniep.SetTvDBInfo();
				ucNextEpisode.DataContext = aniep;
			}
			else
			{
				ucNextEpisode.EpisodeExists = false;
				ucNextEpisode.EpisodeMissing = true;
				ucNextEpisode.DataContext = null;
			}
		}
	}
}
