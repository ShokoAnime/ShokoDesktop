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
using JMMClient.Forms;
using System.Globalization;
using System.Threading;

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

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeGroupControl_DataContextChanged);

			btnSelectDefaultSeries.Click += new RoutedEventHandler(btnSelectDefaultSeries_Click);
			btnRemoveDefaultSeries.Click += new RoutedEventHandler(btnRemoveDefaultSeries_Click);
			btnRandomEpisode.Click += new RoutedEventHandler(btnRandomEpisode_Click);

			lbSeriesList.MouseDoubleClick += new MouseButtonEventHandler(lbSeriesList_MouseDoubleClick);
		}

		void lbSeriesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (lbSeriesList.SelectedItem == null) return;

			AnimeSeriesVM ser = lbSeriesList.SelectedItem as AnimeSeriesVM;
			if (ser == null) return;

			MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

			mainwdw.ShowChildrenForCurrentGroup(ser);
		}

		void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
		{
			AnimeGroupVM grp = this.DataContext as AnimeGroupVM;
			if (grp == null) return;

			MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

			RandomEpisodeForm frm = new RandomEpisodeForm();
			frm.Owner = Window.GetWindow(this); ;
			frm.Init(RandomSeriesEpisodeLevel.Group, grp);
			bool? result = frm.ShowDialog();
		}

		void btnRemoveDefaultSeries_Click(object sender, RoutedEventArgs e)
		{
			AnimeGroupVM grp = this.DataContext as AnimeGroupVM;
			if (grp == null) return;

			if (!grp.AnimeGroupID.HasValue) return;

			JMMServerVM.Instance.clientBinaryHTTP.RemoveDefaultSeriesForGroup(grp.AnimeGroupID.Value);
			grp.DefaultAnimeSeriesID = null;
		}

		void btnSelectDefaultSeries_Click(object sender, RoutedEventArgs e)
		{
			AnimeGroupVM grp = this.DataContext as AnimeGroupVM;
			if (grp == null) return;

			Window wdw = Window.GetWindow(this);

			SelectDefaultSeriesForm frm = new SelectDefaultSeriesForm();
			frm.Owner = wdw;
			frm.Init(grp);
			bool? result = frm.ShowDialog();
			if (result.Value)
			{
				// update info
				grp.DefaultAnimeSeriesID = frm.SelectedSeriesID.Value;
				JMMServerVM.Instance.clientBinaryHTTP.SetDefaultSeriesForGroup(grp.AnimeGroupID.Value, frm.SelectedSeriesID.Value);
			}
		}

		void AnimeGroupControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ShowNextEpisode();
		}

		private void ShowNextEpisode()
		{
			AnimeGroupVM grp = this.DataContext as AnimeGroupVM;
			if (grp == null) return;

			if (!grp.AnimeGroupID.HasValue)
			{
				ucNextEpisode.EpisodeExists = false;
				ucNextEpisode.EpisodeMissing = true;
				ucNextEpisode.DataContext = null;
				return;
			}

			JMMServerBinary.Contract_AnimeEpisode ep = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisodeForGroup(grp.AnimeGroupID.Value, 
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
				Window parentWindow = Window.GetWindow(this);
				AnimeSeriesVM ser = null;
				bool newStatus = false;

				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					MainListHelperVM.Instance.UpdateHeirarchy(vid);

					ser = MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

					ser = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
				}

				ShowNextEpisode();

				if (newStatus == true && ser != null)
				{
					Utils.PromptToRateSeries(ser, parentWindow);
				}
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
