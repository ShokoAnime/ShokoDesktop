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
using NLog;
using JMMClient.ViewModel;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for TvDBAndOtherLinks.xaml
	/// </summary>
	public partial class TvDBAndOtherLinks : UserControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty AniDB_AnimeCrossRefsProperty = DependencyProperty.Register("AniDB_AnimeCrossRefs",
			typeof(AniDB_AnimeCrossRefsVM), typeof(TvDBAndOtherLinks), new UIPropertyMetadata(null, null));

		public AniDB_AnimeCrossRefsVM AniDB_AnimeCrossRefs
		{
			get { return (AniDB_AnimeCrossRefsVM)GetValue(AniDB_AnimeCrossRefsProperty); }
			set { SetValue(AniDB_AnimeCrossRefsProperty, value); }
		}

		public TvDBAndOtherLinks()
		{
			InitializeComponent();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(TvDBAndOtherLinks_DataContextChanged);

			btnSearchTvDB.Click += new RoutedEventHandler(btnSearch_Click);
			btnSearchExistingTvDB.Click += new RoutedEventHandler(btnSearchExisting_Click);

			btnDeleteTvDBLink.Click += new RoutedEventHandler(btnDeleteTvDBLink_Click);
			btnSwitchTvDBSeason.Click += new RoutedEventHandler(btnSwitchTvDBSeason_Click);
			btnUpdateTvDBInfo.Click += new RoutedEventHandler(btnUpdateTvDBInfo_Click);

			btnSearchExistingMovieDB.Click += new RoutedEventHandler(btnSearchExistingMovieDB_Click);
			btnSearchMovieDB.Click += new RoutedEventHandler(btnSearchMovieDB_Click);
			btnDeleteMovieDBLink.Click += new RoutedEventHandler(btnDeleteMovieDBLink_Click);

			btnSearchExistingTrakt.Click += new RoutedEventHandler(btnSearchExistingTrakt_Click);
			btnSearchTrakt.Click += new RoutedEventHandler(btnSearchTrakt_Click);

			btnDeleteTraktLink.Click += new RoutedEventHandler(btnDeleteTraktLink_Click);
			btnSwitchTraktSeason.Click += new RoutedEventHandler(btnSwitchTraktSeason_Click);
			btnUpdateTraktInfo.Click += new RoutedEventHandler(btnUpdateTraktInfo_Click);
		}

		void btnUpdateTraktInfo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				this.Cursor = Cursors.Wait;
				JMMServerVM.Instance.clientBinaryHTTP.UpdateTraktData(AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktID);

				// find the series for this anime
				foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
				{
					if (anime.AnimeID == ser.AniDB_ID)
					{
						MainListHelperVM.Instance.UpdateHeirarchy(ser);
						break;
					}
				}

				RefreshData();
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

		void btnSwitchTraktSeason_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
				frm.Owner = wdw;
				frm.Init(anime.AnimeID, anime.FormattedTitle, AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktID,
					AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktSeasonNumber, AniDB_AnimeCrossRefs.TraktShow.Title);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					RefreshData();
				}

				this.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnDeleteTraktLink_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				string msg = string.Format("Are you sure you want to delete this link?");
				MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
				{
					this.Cursor = Cursors.Wait;
					string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTrakt(anime.AnimeID);
					if (res.Length > 0)
						MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					else
					{
						// update info
						RefreshData();
					}

					this.Cursor = Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnSearchTrakt_Click(object sender, RoutedEventArgs e)
		{
			SearchTrakt("");
		}

		void btnSearchExistingTrakt_Click(object sender, RoutedEventArgs e)
		{
			SearchTrakt(AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktID);
		}

		private void SearchTrakt(string ExistingTraktID)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SearchTraktForm frm = new SearchTraktForm();
				frm.Owner = wdw;
				frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingTraktID);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					RefreshData();
				}

				this.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		

		void btnUpdateTvDBInfo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				this.Cursor = Cursors.Wait;
				JMMServerVM.Instance.clientBinaryHTTP.UpdateTvDBData(AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDB.TvDBID);

				anime.ClearTvDBData();

				// find the series for this anime
				foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
				{
					if (anime.AnimeID == ser.AniDB_ID)
					{
						MainListHelperVM.Instance.UpdateHeirarchy(ser);
						break;
					}
				}

				RefreshData();
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

		void btnSwitchTvDBSeason_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SelectTvDBSeasonForm frm = new SelectTvDBSeasonForm();
				frm.Owner = wdw;
				frm.Init(anime.AnimeID, anime.FormattedTitle, AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDB.TvDBID, 
					AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDB.TvDBSeasonNumber, AniDB_AnimeCrossRefs.TvDBSeries.SeriesName);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					RefreshData();
				}

				this.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnDeleteTvDBLink_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				string msg = string.Format("Are you sure you want to delete this link?");
				MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
				{
					this.Cursor = Cursors.Wait;
					string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTvDB(anime.AnimeID);
					if (res.Length > 0)
						MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					else
					{
						// update info
						RefreshData();
					}

					this.Cursor = Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnSearchExisting_Click(object sender, RoutedEventArgs e)
		{
			SearchTvDB(AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDB.TvDBID);
		}

		void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			SearchTvDB(null);
		}

		private void SearchTvDB(int? ExistingSeriesID)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SearchTvDBForm frm = new SearchTvDBForm();
				frm.Owner = wdw;
				frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingSeriesID);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					RefreshData();
				}

				this.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		

		void TvDBAndOtherLinks_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				if (this.DataContext == null)
				{
					AniDB_AnimeCrossRefs = null;
					return;
				}

				RefreshData();


			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void SearchMovieDB()
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SearchMovieDBForm frm = new SearchMovieDBForm();
				frm.Owner = wdw;
				frm.Init(anime.AnimeID, anime.FormattedTitle);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					RefreshData();
				}

				this.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnSearchMovieDB_Click(object sender, RoutedEventArgs e)
		{
			SearchMovieDB();
		}

		void btnSearchExistingMovieDB_Click(object sender, RoutedEventArgs e)
		{
			SearchMovieDB();
		}

		void btnDeleteMovieDBLink_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				string msg = string.Format("Are you sure you want to delete this link?");
				MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
				{
					this.Cursor = Cursors.Wait;
					string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBOther(anime.AnimeID, (int)CrossRefType.MovieDB);
					if (res.Length > 0)
						MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					else
					{
						// update info
						RefreshData();
					}

					this.Cursor = Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void RefreshData()
		{
			try
			{
				AniDB_AnimeCrossRefs = null;

				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				JMMServerBinary.Contract_AniDB_AnimeCrossRefs xrefDetails = JMMServerVM.Instance.clientBinaryHTTP.GetCrossRefDetails(anime.AnimeID);
				if (xrefDetails == null) return;

				AniDB_AnimeCrossRefs = new AniDB_AnimeCrossRefsVM();
				AniDB_AnimeCrossRefs.Populate(xrefDetails);

				MainListHelperVM.Instance.UpdateAnime(anime.AnimeID);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}
