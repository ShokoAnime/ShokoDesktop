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
	/// Interaction logic for AnimeGroupControl.xaml
	/// </summary>
	public partial class AnimeGroupControl : UserControl
	{
		public AnimeGroupControl()
		{
			InitializeComponent();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeGroupControl_DataContextChanged);
			btnPlayNextEpisode.Click += new RoutedEventHandler(btnPlayNextEpisode_Click);
		}

		void btnPlayNextEpisode_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.SeriesNextEpisodeExpanded = !UserSettingsVM.Instance.SeriesNextEpisodeExpanded;

			ShowNextEpisode();
		}

		void AnimeGroupControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ShowNextEpisode();
		}

		private void ShowNextEpisode()
		{
			if (UserSettingsVM.Instance.SeriesNextEpisodeExpanded)
			{
				AnimeGroupVM grp = this.DataContext as AnimeGroupVM;
				if (grp == null) return;

				JMMServerBinary.Contract_AnimeEpisode ep = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisodeForGroup(grp.AnimeGroupID.Value, 
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				if (ep != null)
				{
					AnimeEpisodeVM aniep = new AnimeEpisodeVM(ep);
					aniep.SetTvDBImageAndOverview();
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

		/// <summary>
		/// This event bubbles up from PlayEpisodeControl
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			this.Cursor = Cursors.Wait;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					bool newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					MainListHelperVM.Instance.UpdateHeirarchy(vid);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					bool newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);
				}

				ShowNextEpisode();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
			}
		}
	}
}
