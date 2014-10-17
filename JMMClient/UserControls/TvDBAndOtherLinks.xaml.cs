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

			btnSearchExistingMovieDB.Click += new RoutedEventHandler(btnSearchExistingMovieDB_Click);
			btnSearchMovieDB.Click += new RoutedEventHandler(btnSearchMovieDB_Click);
			btnDeleteMovieDBLink.Click += new RoutedEventHandler(btnDeleteMovieDBLink_Click);

			btnSearchExistingTrakt.Click += new RoutedEventHandler(btnSearchExistingTrakt_Click);
			btnSearchTrakt.Click += new RoutedEventHandler(btnSearchTrakt_Click);

			btnSearchExistingMAL.Click += new RoutedEventHandler(btnSearchExistingMAL_Click);
			btnSearchMAL.Click += new RoutedEventHandler(btnSearchMAL_Click);
		}

		

		

		

		#region MAL

		void btnSearchMAL_Click(object sender, RoutedEventArgs e)
		{
			SearchMAL();
		}

		void btnSearchExistingMAL_Click(object sender, RoutedEventArgs e)
		{
			SearchMAL();
		}

		private void SearchMAL()
		{
			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SearchMALForm frm = new SearchMALForm();
				frm.Owner = wdw;
				frm.Init(anime.AnimeID, anime.FormattedTitle, anime.MainTitle);
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

		void btnEditMALDetails_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// prompt to select details
				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_EditMALLink(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				if (obj.GetType() == typeof(CrossRef_AniDB_MALVM))
				{
					this.Cursor = Cursors.Wait;
					CrossRef_AniDB_MALVM malLink = obj as CrossRef_AniDB_MALVM;

					// prompt to select details
					Window wdw = Window.GetWindow(this);

					SelectMALStartForm frm = new SelectMALStartForm();
					frm.Owner = wdw;
					frm.Init(malLink.AnimeID, anime.FormattedTitle, malLink.MALTitle, malLink.MALID, malLink.StartEpisodeType, malLink.StartEpisodeNumber);
					bool? result = frm.ShowDialog();
					if (result.Value)
					{
						// update info
						RefreshData();
					}
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

        private void CommandBinding_DeleteMALLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                if (obj.GetType() == typeof(CrossRef_AniDB_MALVM))
                {
                    this.Cursor = Cursors.Wait;
                    CrossRef_AniDB_MALVM malLink = obj as CrossRef_AniDB_MALVM;

                    // prompt to select details
                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format("Are you sure you want to delete this link?");
                    MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Cursor = Cursors.Wait;

                        string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBMAL(anime.AnimeID, malLink.StartEpisodeType, malLink.StartEpisodeNumber);
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

		

		

		private void CommandBinding_ToggleAutoLinkMovieDB(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				this.Cursor = Cursors.Wait;

				anime.IsMovieDBLinkDisabled = !anime.IsMovieDBLinkDisabled;
				anime.UpdateDisableExternalLinksFlag();
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

		private void CommandBinding_ToggleAutoLinkMAL(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				this.Cursor = Cursors.Wait;

				anime.IsMALLinkDisabled = !anime.IsMALLinkDisabled;
				anime.UpdateDisableExternalLinksFlag();
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

		#endregion

		#region Trakt

        private void CommandBinding_ReportTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 link = obj as CrossRef_AniDB_TraktVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

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

        private void CommandBinding_DeleteTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 link = obj as CrossRef_AniDB_TraktVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format("Are you sure you want to delete this link?");
                    MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Cursor = Cursors.Wait;
                        string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTrakt(link.AnimeID, link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber,
                            link.TraktID, link.TraktSeasonNumber, link.TraktStartEpisodeNumber);
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

        private void CommandBinding_EditTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 link = obj as CrossRef_AniDB_TraktVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    Window wdw = Window.GetWindow(this);

                    this.Cursor = Cursors.Wait;
                    SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
                    frm.Owner = wdw;
                    frm.Init(anime.AnimeID, anime.FormattedTitle, (EpisodeType)link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber, link.TraktID,
                        link.TraktSeasonNumber, link.TraktStartEpisodeNumber, link.TraktTitle, anime, link.CrossRef_AniDB_TraktV2ID);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // update info
                        RefreshData();
                    }
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

        private void CommandBinding_UpdateTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    foreach (CrossRef_AniDB_TraktVMV2 xref in AniDB_AnimeCrossRefs.CrossRef_AniDB_TraktV2)
                        JMMServerVM.Instance.clientBinaryHTTP.UpdateTraktData(xref.TraktID);

                    anime.ClearTraktData();

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

        private void CommandBinding_ToggleAutoLinkTrakt(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                this.Cursor = Cursors.Wait;

                anime.IsTraktLinkDisabled = !anime.IsTraktLinkDisabled;
                anime.UpdateDisableExternalLinksFlag();
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

		void btnSearchTrakt_Click(object sender, RoutedEventArgs e)
		{
			SearchTrakt("");
		}

		void btnSearchExistingTrakt_Click(object sender, RoutedEventArgs e)
		{
            SearchTrakt("");
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
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingTraktID, anime);
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

		#endregion

		#region TvDB

		void btnSearchExisting_Click(object sender, RoutedEventArgs e)
		{
			SearchTvDB(null);
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
				frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingSeriesID, anime);
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

        private void CommandBinding_ToggleAutoLinkTvDB(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                this.Cursor = Cursors.Wait;

                anime.IsTvDBLinkDisabled = !anime.IsTvDBLinkDisabled;
                anime.UpdateDisableExternalLinksFlag();
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

        private void CommandBinding_ReportTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 link = obj as CrossRef_AniDB_TvDBVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

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

        private void CommandBinding_EditTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 link = obj as CrossRef_AniDB_TvDBVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    Window wdw = Window.GetWindow(this);

                    this.Cursor = Cursors.Wait;
                    SelectTvDBSeasonForm frm = new SelectTvDBSeasonForm();
                    frm.Owner = wdw;
                    //TODO
                    frm.Init(anime.AnimeID, anime.FormattedTitle, (EpisodeType)link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber, link.TvDBID,
                        link.TvDBSeasonNumber, link.TvDBStartEpisodeNumber, link.TvDBTitle, anime, link.CrossRef_AniDB_TvDBV2ID);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // update info
                        RefreshData();
                    }
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

        private void CommandBinding_DeleteTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 link = obj as CrossRef_AniDB_TvDBVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format("Are you sure you want to delete this link?");
                    MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Cursor = Cursors.Wait;
                        string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTvDB(link.AnimeID, link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber,
                            link.TvDBID, link.TvDBSeasonNumber, link.TvDBStartEpisodeNumber);
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

        private void CommandBinding_UpdateTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    foreach (CrossRef_AniDB_TvDBVMV2 xref in AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDBV2)
                        JMMServerVM.Instance.clientBinaryHTTP.UpdateTvDBData(xref.TvDBID);

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

		#endregion

		#region MovieDB

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

		#endregion

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
