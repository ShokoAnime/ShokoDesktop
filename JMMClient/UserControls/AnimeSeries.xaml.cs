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
using System.ComponentModel;
using NLog;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for AnimeSeries.xaml
	/// </summary>
	public partial class AnimeSeries : UserControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty AllPostersProperty = DependencyProperty.Register("AllPosters",
			typeof(List<PosterContainer>), typeof(AnimeSeries), new UIPropertyMetadata(null, null));

		public List<PosterContainer> AllPosters
		{
			get { return (List<PosterContainer>)GetValue(AllPostersProperty); }
			set { SetValue(AllPostersProperty, value); }
		}

		public static readonly DependencyProperty AllFanartsProperty = DependencyProperty.Register("AllFanarts",
			typeof(List<FanartContainer>), typeof(AnimeSeries), new UIPropertyMetadata(null, null));

		public List<FanartContainer> AllFanarts
		{
			get { return (List<FanartContainer>)GetValue(AllFanartsProperty); }
			set { SetValue(AllFanartsProperty, value); }
		}

		public static readonly DependencyProperty SeriesTvDBWideBannersProperty = DependencyProperty.Register("SeriesTvDBWideBanners",
			typeof(List<TvDB_ImageWideBannerVM>), typeof(AnimeSeries), new UIPropertyMetadata(null, null));

		public List<TvDB_ImageWideBannerVM> SeriesTvDBWideBanners
		{
			get { return (List<TvDB_ImageWideBannerVM>)GetValue(SeriesTvDBWideBannersProperty); }
			set { SetValue(SeriesTvDBWideBannersProperty, value); }
		}

		public static readonly DependencyProperty ImageListBoxWidthProperty = DependencyProperty.Register("ImageListBoxWidth",
			typeof(double), typeof(AnimeSeries), new UIPropertyMetadata((double)300, null));

		public double ImageListBoxWidth
		{
			get { return (double)GetValue(ImageListBoxWidthProperty); }
			set { SetValue(ImageListBoxWidthProperty, value); }
		}

		public static readonly DependencyProperty ImageListBoxWidthFullProperty = DependencyProperty.Register("ImageListBoxWidthFull",
			typeof(double), typeof(AnimeSeries), new UIPropertyMetadata((double)300, null));

		public double ImageListBoxWidthFull
		{
			get { return (double)GetValue(ImageListBoxWidthFullProperty); }
			set { SetValue(ImageListBoxWidthFullProperty, value); }
		}

		public static readonly DependencyProperty SeriesPos_TvDBLinksProperty = DependencyProperty.Register("SeriesPos_TvDBLinks",
			typeof(int), typeof(AnimeSeries), new UIPropertyMetadata((int)6, null));

		public int SeriesPos_TvDBLinks
		{
			get { return (int)GetValue(SeriesPos_TvDBLinksProperty); }
			set { SetValue(SeriesPos_TvDBLinksProperty, value); }
		}

		public static readonly DependencyProperty SeriesPos_PlayNextEpisodeProperty = DependencyProperty.Register("SeriesPos_PlayNextEpisode",
			typeof(int), typeof(AnimeSeries), new UIPropertyMetadata((int)6, null));

		public int SeriesPos_PlayNextEpisode
		{
			get { return (int)GetValue(SeriesPos_PlayNextEpisodeProperty); }
			set { SetValue(SeriesPos_PlayNextEpisodeProperty, value); }
		}

		public static readonly DependencyProperty SeriesPos_FileSummaryProperty = DependencyProperty.Register("SeriesPos_FileSummary",
			typeof(int), typeof(AnimeSeries), new UIPropertyMetadata((int)6, null));

		public int SeriesPos_FileSummary
		{
			get { return (int)GetValue(SeriesPos_FileSummaryProperty); }
			set { SetValue(SeriesPos_FileSummaryProperty, value); }
		}

		public static readonly DependencyProperty SeriesPos_CategoriesProperty = DependencyProperty.Register("SeriesPos_Categories",
			typeof(int), typeof(AnimeSeries), new UIPropertyMetadata((int)6, null));

		public int SeriesPos_Categories
		{
			get { return (int)GetValue(SeriesPos_CategoriesProperty); }
			set { SetValue(SeriesPos_CategoriesProperty, value); }
		}

		public static readonly DependencyProperty SeriesPos_TitlesProperty = DependencyProperty.Register("SeriesPos_Titles",
			typeof(int), typeof(AnimeSeries), new UIPropertyMetadata((int)6, null));

		public int SeriesPos_Titles
		{
			get { return (int)GetValue(SeriesPos_TitlesProperty); }
			set { SetValue(SeriesPos_TitlesProperty, value); }
		}

		public static readonly DependencyProperty SeriesPos_TagsProperty = DependencyProperty.Register("SeriesPos_Tags",
			typeof(int), typeof(AnimeSeries), new UIPropertyMetadata((int)6, null));

		public int SeriesPos_Tags
		{
			get { return (int)GetValue(SeriesPos_TagsProperty); }
			set { SetValue(SeriesPos_TagsProperty, value); }
		}

		public static readonly DependencyProperty TruncatedDescriptionProperty = DependencyProperty.Register("TruncatedDescription",
			typeof(bool), typeof(AnimeSeries), new UIPropertyMetadata(true, null));

		public bool TruncatedDescription
		{
			get { return (bool)GetValue(TruncatedDescriptionProperty); }
			set { SetValue(TruncatedDescriptionProperty, value); }
		}

		public static readonly DependencyProperty FullDescriptionProperty = DependencyProperty.Register("FullDescription",
			typeof(bool), typeof(AnimeSeries), new UIPropertyMetadata(false, null));

		public bool FullDescription
		{
			get { return (bool)GetValue(FullDescriptionProperty); }
			set { SetValue(FullDescriptionProperty, value); }
		}

		public AnimeSeries()
		{
			InitializeComponent();

			cRating.OnRatingValueChangedEvent += new RatingControl.RatingValueChangedHandler(cRating_OnRatingValueChangedEvent);

			this.Loaded += new RoutedEventHandler(AnimeSeries_Loaded);
			btnAnimeGroupShow.Click += new RoutedEventHandler(btnAnimeGroupShow_Click);
			btnFileSummary.Click += new RoutedEventHandler(btnFileSummary_Click);
			btnTvDBLinks.Click += new RoutedEventHandler(btnTvDBLinks_Click);
			//btnPlayNextEpisode.Click += new RoutedEventHandler(btnPlayNextEpisode_Click);
			btnGetRelMissingInfo.Click += new RoutedEventHandler(btnGetRelMissingInfo_Click);
			btnGetSimMissingInfo.Click += new RoutedEventHandler(btnGetSimMissingInfo_Click);

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeSeries_DataContextChanged);

			tabContainer.SelectionChanged += new SelectionChangedEventHandler(tabContainer_SelectionChanged);

			btnUpdateAniDBInfo.Click += new RoutedEventHandler(btnUpdateAniDBInfo_Click);
			this.LayoutUpdated += new EventHandler(AnimeSeries_LayoutUpdated);

			btnEditSeries.Click += new RoutedEventHandler(btnEditSeries_Click);
			btnEditSeriesFinish.Click += new RoutedEventHandler(btnEditSeriesFinish_Click);

			btnAnimeTitles.Click += new RoutedEventHandler(btnAnimeTitles_Click);
			btnAnimeTags.Click += new RoutedEventHandler(btnAnimeTags_Click);
			btnAnimeCategories.Click += new RoutedEventHandler(btnAnimeCategories_Click);

			/*SeriesPos_PlayNextEpisode = JMMServerVM.Instance.SeriesPos_PlayNextEpisode;
			SeriesPos_TvDBLinks = JMMServerVM.Instance.SeriesPos_TvDBLinks;
			SeriesPos_FileSummary = JMMServerVM.Instance.SeriesPos_FileSummary;
			SeriesPos_Categories = JMMServerVM.Instance.SeriesPos_Categories;
			SeriesPos_Titles = JMMServerVM.Instance.SeriesPos_Titles;
			SeriesPos_Tags = JMMServerVM.Instance.SeriesPos_Tags;*/

			SetSeriesWidgetOrder();
		}


		private void Handle_Click(object sender, MouseButtonEventArgs e)
		{
			string tag = ((TextBlock)sender).Tag.ToString();

			if (tag.Equals("txtDescription", StringComparison.InvariantCultureIgnoreCase))
			{
				TruncatedDescription = !TruncatedDescription;
				FullDescription = !FullDescription;
			}
		}

		void btnGetSimMissingInfo_Click(object sender, RoutedEventArgs e)
		{
			ucSimilarAnime.GetMissingSimilarData();
		}

		void btnGetRelMissingInfo_Click(object sender, RoutedEventArgs e)
		{
			
			ucRelatedAnime.GetMissingSimilarData();
		}

		

		

		

		void btnEditSeriesFinish_Click(object sender, RoutedEventArgs e)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			ser.IsBeingEdited = false;
			ser.IsReadOnly = true;
		}

		void btnEditSeries_Click(object sender, RoutedEventArgs e)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			ser.IsBeingEdited = true;
			ser.IsReadOnly = false;
		}

		private void SetSeriesWidgetOrder()
		{

			SeriesPos_PlayNextEpisode = UserSettingsVM.Instance.GetSeriesWidgetPosition(SeriesWidgets.PlayNextEpisode) + 4;
			SeriesPos_TvDBLinks = UserSettingsVM.Instance.GetSeriesWidgetPosition(SeriesWidgets.TvDBLinks) + 4;
			SeriesPos_FileSummary = UserSettingsVM.Instance.GetSeriesWidgetPosition(SeriesWidgets.FileSummary) + 4;
			SeriesPos_Categories = UserSettingsVM.Instance.GetSeriesWidgetPosition(SeriesWidgets.Categories) + 4;
			SeriesPos_Titles = UserSettingsVM.Instance.GetSeriesWidgetPosition(SeriesWidgets.Titles) + 4;
			SeriesPos_Tags = UserSettingsVM.Instance.GetSeriesWidgetPosition(SeriesWidgets.Tags) + 4;
		}

		

		void AnimeSeries_LayoutUpdated(object sender, EventArgs e)
		{
			double temp = Scroller2.ViewportWidth - 210;
			if (temp >= 300)
				ImageListBoxWidth = temp;
			else
				ImageListBoxWidth = 300;

			temp = Scroller2.ViewportWidth - 50;
			if (temp >= 300)
				ImageListBoxWidthFull = temp;
			else
				ImageListBoxWidthFull = 300;

			//tabContainer.Width = Scroller2.ViewportWidth - 50;
		}

		private void CommandBinding_MoveUpWidget(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			SeriesWidgets swid = (SeriesWidgets)int.Parse(obj.ToString());

			UserSettingsVM.Instance.MoveUpSeriesWidget(swid);
			SetSeriesWidgetOrder();
		}

		private void CommandBinding_MoveDownWidget(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			SeriesWidgets swid = (SeriesWidgets)int.Parse(obj.ToString());

			UserSettingsVM.Instance.MoveDownSeriesWidget(swid);
			SetSeriesWidgetOrder();
		}

		private void CommandBinding_ViewImage(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(PosterContainer))
				{
					PosterContainer poster = (PosterContainer)obj;
					Utils.OpenFile(poster.FullImagePath);
				}

				if (obj.GetType() == typeof(FanartContainer))
				{
					FanartContainer fanart = (FanartContainer)obj;
					Utils.OpenFile(fanart.FullImagePath);
				}

				/*
				if (obj.GetType() == typeof(TvDB_ImagePosterVM))
				{
					TvDB_ImagePosterVM poster = (TvDB_ImagePosterVM)obj;
					Utils.OpenFile(poster.FullImagePath);
				}

				if (obj.GetType() == typeof(TvDB_ImageFanartVM))
				{
					TvDB_ImageFanartVM fanart = (TvDB_ImageFanartVM)obj;
					Utils.OpenFile(fanart.FullImagePath);
				}*/

				if (obj.GetType() == typeof(TvDB_ImageWideBannerVM))
				{
					TvDB_ImageWideBannerVM banner = (TvDB_ImageWideBannerVM)obj;
					Utils.OpenFile(banner.FullImagePath);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DisableImage(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			EnableDisableImage(false, obj); 
		}

		private void CommandBinding_EnableImage(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			EnableDisableImage(true, obj); 
		}

		private void EnableDisableImage(bool enabled, object img)
		{
			if (!JMMServerVM.Instance.ServerOnline) return;

			try
			{
				AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
				if (ser == null) return;

				this.Cursor = Cursors.Wait;
				string res = "";


				// NOTE if we are disabling an image we should also make sure it is not the default

				if (img.GetType() == typeof(PosterContainer))
				{
					PosterContainer poster = (PosterContainer)img;

					if (!enabled && poster.IsImageDefault)
						SetDefaultImage(false, img, false);

					switch (poster.ImageType)
					{
						case ImageEntityType.TvDB_Cover:
							TvDB_ImagePosterVM tvPoster = poster.PosterObject as TvDB_ImagePosterVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, tvPoster.TvDB_ImagePosterID, (int)ImageEntityType.TvDB_Cover);
							tvPoster.Enabled = enabled ? 1 : 0;
							tvPoster.IsImageEnabled = enabled;
							tvPoster.IsImageDisabled = !enabled;
							break;

						case ImageEntityType.Trakt_Poster:
							Trakt_ImagePosterVM traktPoster = poster.PosterObject as Trakt_ImagePosterVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, traktPoster.Trakt_ImagePosterID, (int)ImageEntityType.Trakt_Poster);
							traktPoster.Enabled = enabled ? 1 : 0;
							traktPoster.IsImageEnabled = enabled;
							traktPoster.IsImageDisabled = !enabled;
							break;

						case ImageEntityType.AniDB_Cover:
							AniDB_AnimeVM anime = poster.PosterObject as AniDB_AnimeVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, anime.AnimeID,(int)ImageEntityType.AniDB_Cover);
							anime.ImageEnabled = enabled ? 1 : 0;
							anime.IsImageEnabled = enabled;
							anime.IsImageDisabled = !enabled;
							break;

						case ImageEntityType.MovieDB_Poster:
							MovieDB_PosterVM moviePoster = poster.PosterObject as MovieDB_PosterVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, moviePoster.MovieDB_PosterID, (int)ImageEntityType.MovieDB_Poster);
							moviePoster.Enabled = enabled ? 1 : 0;
							moviePoster.IsImageEnabled = enabled;
							moviePoster.IsImageDisabled = !enabled;
							break;
					}
					poster.IsImageEnabled = enabled;
					poster.IsImageDisabled = !enabled;
				}

				if (img.GetType() == typeof(FanartContainer))
				{
					FanartContainer fanart = (FanartContainer)img;

					if (!enabled && fanart.IsImageDefault)
						SetDefaultImage(false, img, false);

					switch (fanart.ImageType)
					{
						case ImageEntityType.TvDB_FanArt:
							TvDB_ImageFanartVM tvFanart = fanart.FanartObject as TvDB_ImageFanartVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, tvFanart.TvDB_ImageFanartID, (int)ImageEntityType.TvDB_FanArt);
							tvFanart.Enabled = enabled ? 1 : 0;
							tvFanart.IsImageEnabled = enabled;
							tvFanart.IsImageDisabled = !enabled;
							break;

						case ImageEntityType.Trakt_Fanart:
							Trakt_ImageFanartVM traktFanart = fanart.FanartObject as Trakt_ImageFanartVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, traktFanart.Trakt_ImageFanartID, (int)ImageEntityType.Trakt_Fanart);
							traktFanart.Enabled = enabled ? 1 : 0;
							traktFanart.IsImageEnabled = enabled;
							traktFanart.IsImageDisabled = !enabled;
							break;

						case ImageEntityType.MovieDB_FanArt:
							MovieDB_FanartVM movieFanart = fanart.FanartObject as MovieDB_FanartVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, movieFanart.MovieDB_FanartID, (int)ImageEntityType.MovieDB_FanArt);
							movieFanart.Enabled = enabled ? 1 : 0;
							movieFanart.IsImageEnabled = enabled;
							movieFanart.IsImageDisabled = !enabled;
							break;
					}
					fanart.IsImageEnabled = enabled;
					fanart.IsImageDisabled = !enabled;
				}

				if (img.GetType() == typeof(TvDB_ImageWideBannerVM))
				{
					TvDB_ImageWideBannerVM banner = (TvDB_ImageWideBannerVM)img;

					if (!enabled && banner.IsImageDefault)
						SetDefaultImage(false, img, false);

					res = JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, banner.TvDB_ImageWideBannerID, (int)ImageEntityType.TvDB_Banner);
					banner.Enabled = enabled ? 1 : 0;
					banner.IsImageEnabled = enabled;
					banner.IsImageDisabled = !enabled;
				}

				if (res.Length > 0)
				{
					MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					MainListHelperVM.Instance.UpdateHeirarchy(ser);
					MainListHelperVM.Instance.UpdateAnime(ser.AniDB_ID);
					RefreshImagesData();
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

		private void CommandBinding_SetDefaultImageON(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			SetDefaultImage(true, obj, true);
		}

		private void CommandBinding_SetDefaultImageOFF(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			SetDefaultImage(false, obj, true);
		}

		private void SetDefaultImage(bool isDefault, object img, bool refreshData)
		{
			if (!JMMServerVM.Instance.ServerOnline) return;

			try
			{
				AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
				if (ser == null) return;

				this.Cursor = Cursors.Wait;
				string res = "";

				string disabledMessage = "Cannot set a disabled image as the default";

				if (img.GetType() == typeof(PosterContainer))
				{
					PosterContainer poster = (PosterContainer)img;

					if (isDefault && poster.IsImageDisabled)
					{
						MessageBox.Show(disabledMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					switch (poster.ImageType)
					{
						case ImageEntityType.TvDB_Cover:
							TvDB_ImagePosterVM tvPoster = poster.PosterObject as TvDB_ImagePosterVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID, 
								tvPoster.TvDB_ImagePosterID, (int)ImageEntityType.TvDB_Cover, (int)ImageSizeType.Poster);
							tvPoster.IsImageDefault = isDefault;
							tvPoster.IsImageNotDefault = !isDefault;
							break;

						case ImageEntityType.Trakt_Poster:
							Trakt_ImagePosterVM traktPoster = poster.PosterObject as Trakt_ImagePosterVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								traktPoster.Trakt_ImagePosterID, (int)ImageEntityType.Trakt_Poster, (int)ImageSizeType.Poster);
							traktPoster.IsImageDefault = isDefault;
							traktPoster.IsImageNotDefault = !isDefault;
							break;

						case ImageEntityType.AniDB_Cover:
							AniDB_AnimeVM anime = poster.PosterObject as AniDB_AnimeVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								anime.AnimeID, (int)ImageEntityType.AniDB_Cover, (int)ImageSizeType.Poster);
							anime.IsImageDefault = isDefault;
							anime.IsImageNotDefault = !isDefault;
							break;

						case ImageEntityType.MovieDB_Poster:
							MovieDB_PosterVM moviePoster = poster.PosterObject as MovieDB_PosterVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								moviePoster.MovieDB_PosterID, (int)ImageEntityType.MovieDB_Poster, (int)ImageSizeType.Poster);
							moviePoster.IsImageDefault = isDefault;
							moviePoster.IsImageNotDefault = !isDefault;
							break;
					}
					poster.IsImageDefault = isDefault;
					poster.IsImageNotDefault = !isDefault;
				}

				if (img.GetType() == typeof(FanartContainer))
				{
					FanartContainer fanart = (FanartContainer)img;

					if (isDefault && fanart.IsImageDisabled)
					{
						MessageBox.Show(disabledMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					switch (fanart.ImageType)
					{
						case ImageEntityType.TvDB_FanArt:
							TvDB_ImageFanartVM tvFanart = fanart.FanartObject as TvDB_ImageFanartVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								tvFanart.TvDB_ImageFanartID, (int)ImageEntityType.TvDB_FanArt, (int)ImageSizeType.Fanart);
							tvFanart.IsImageDefault = isDefault;
							tvFanart.IsImageNotDefault = !isDefault;
							break;

						case ImageEntityType.Trakt_Fanart:
							Trakt_ImageFanartVM traktFanart = fanart.FanartObject as Trakt_ImageFanartVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								traktFanart.Trakt_ImageFanartID, (int)ImageEntityType.Trakt_Fanart, (int)ImageSizeType.Fanart);
							traktFanart.IsImageDefault = isDefault;
							traktFanart.IsImageNotDefault = !isDefault;
							break;

						case ImageEntityType.MovieDB_FanArt:
							MovieDB_FanartVM movieFanart = fanart.FanartObject as MovieDB_FanartVM;
							res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								movieFanart.MovieDB_FanartID, (int)ImageEntityType.MovieDB_FanArt, (int)ImageSizeType.Fanart);
							movieFanart.IsImageDefault = isDefault;
							movieFanart.IsImageNotDefault = !isDefault;
							break;
					}
					fanart.IsImageDefault = isDefault;
					fanart.IsImageNotDefault = !isDefault;
				}

				if (img.GetType() == typeof(TvDB_ImageWideBannerVM))
				{
					TvDB_ImageWideBannerVM banner = (TvDB_ImageWideBannerVM)img;

					if (isDefault && banner.IsImageDisabled)
					{
						MessageBox.Show(disabledMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					
					res = JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, ser.AniDB_ID,
								banner.TvDB_ImageWideBannerID, (int)ImageEntityType.TvDB_Banner, (int)ImageSizeType.WideBanner);
					banner.IsImageDefault = isDefault;
					banner.IsImageNotDefault = !isDefault;
				}

				if (res.Length > 0)
				{
					MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					if (refreshData)
					{
						MainListHelperVM.Instance.UpdateHeirarchy(ser);
						MainListHelperVM.Instance.UpdateAnime(ser.AniDB_ID);
						RefreshImagesData();
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

		void btnUpdateAniDBInfo_Click(object sender, RoutedEventArgs e)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			try
			{
				this.Cursor = Cursors.Wait;
				JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(ser.AniDB_ID);

				// refresh the data
				MainListHelperVM.Instance.UpdateHeirarchy(ser);
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

		void tabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.Source is TabControl)
			{
				TabControl tab = e.Source as TabControl;
				if (tab.SelectedIndex == 1) // episodes
				{
					this.Cursor = Cursors.Wait;
					epListMain.PopulateToolbars();
					this.Cursor = Cursors.Arrow;
				}
				else if (tab.SelectedIndex == 2) // images
				{
					RefreshImagesData();
				}
				else if (tab.SelectedIndex == 3) // related and similar
				{
					this.Cursor = Cursors.Wait;
					ucSimilarAnime.RefreshData();
					ucRelatedAnime.RefreshData();
					this.Cursor = Cursors.Arrow;
				}
				else if (tab.SelectedIndex == 4) // trakt shouts
				{
					this.Cursor = Cursors.Wait;
					ucTraktShouts.RefreshShouts();
					this.Cursor = Cursors.Arrow;
				}

			}
		}

		private void RefreshImagesData()
		{
			this.Cursor = Cursors.Wait;

			try
			{
				AniDB_AnimeCrossRefsVM AniDB_AnimeCrossRefs = null;
				SeriesTvDBWideBanners = null;
				AllPosters = null;
				AllFanarts = null;

				AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
				if (ser == null) return;

				JMMServerBinary.Contract_AniDB_AnimeCrossRefs xrefDetails = JMMServerVM.Instance.clientBinaryHTTP.GetCrossRefDetails(ser.AniDB_ID);
				if (xrefDetails == null) return;

				AniDB_AnimeCrossRefs = new AniDB_AnimeCrossRefsVM();
				AniDB_AnimeCrossRefs.Populate(xrefDetails);

				SeriesTvDBWideBanners = AniDB_AnimeCrossRefs.TvDBImageWideBanners;

				AniDB_AnimeCrossRefs.AllPosters.Insert(0, new PosterContainer(ImageEntityType.AniDB_Cover, ser.AniDB_Anime));
				AllPosters = AniDB_AnimeCrossRefs.AllPosters;

				AllFanarts = AniDB_AnimeCrossRefs.AllFanarts;
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

		void btnTvDBLinks_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.SeriesTvDBLinksExpanded = !UserSettingsVM.Instance.SeriesTvDBLinksExpanded;

			ShowTvDBLinks();


		}

		void btnAnimeTitles_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.TitlesExpanded = !UserSettingsVM.Instance.TitlesExpanded;
		}

		void btnAnimeTags_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.TagsExpanded = !UserSettingsVM.Instance.TagsExpanded;
		}

		void btnAnimeCategories_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.CategoriesExpanded = !UserSettingsVM.Instance.CategoriesExpanded;
		}

		/*void btnPlayNextEpisode_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.SeriesNextEpisodeExpanded = !UserSettingsVM.Instance.SeriesNextEpisodeExpanded;

			ShowNextEpisode();
		}*/

		void AnimeSeries_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Cursor = Cursors.Wait;

			FullDescription = false;
			TruncatedDescription = true;

			ShowFileSummary();
			ShowTvDBLinks();
			ShowNextEpisode();

			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			ser.PopulateIsFave();

			epListMain.DataContext = ser;
			ucSimilarAnime.DataContext = ser;
			ucRelatedAnime.DataContext = ser;
			ucTraktShouts.DataContext = ser;

			if (tabContainer.SelectedIndex == 1) // episodes
			{
				epListMain.PopulateToolbars();
			}
			else if (tabContainer.SelectedIndex == 2) // images
			{
				RefreshImagesData();
			}
			else if (tabContainer.SelectedIndex == 3) // related and similar
			{
				this.Cursor = Cursors.Wait;
				ucSimilarAnime.RefreshData();
				ucRelatedAnime.RefreshData();
				this.Cursor = Cursors.Arrow;
			}
			else if (tabContainer.SelectedIndex == 4) // trakt shouts
			{
				this.Cursor = Cursors.Wait;
				ucTraktShouts.RefreshShouts();
				this.Cursor = Cursors.Arrow;
			}

			cboVoteType.Items.Clear();
			cboVoteType.Items.Add(Properties.Resources.VoteTypeAnimeTemporary);
			if (ser.AniDB_Anime.FinishedAiring)
				cboVoteType.Items.Add(Properties.Resources.VoteTypeAnimePermanent);

			if (ser.AniDB_Anime.FinishedAiring && ser.AllFilesWatched)
				cboVoteType.SelectedIndex = 1;
			else
				cboVoteType.SelectedIndex = 0;

			this.Cursor = Cursors.Arrow;
		}

		void btnFileSummary_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.SeriesFileSummaryExpanded = !UserSettingsVM.Instance.SeriesFileSummaryExpanded;

			ShowFileSummary();
		}

		private void ShowFileSummary()
		{
			if (UserSettingsVM.Instance.SeriesFileSummaryExpanded)
			{
				AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
				if (ser == null) return;
				ucFileSummary.DataContext = ser.AniDB_Anime;
			}
		}

		private void ShowTvDBLinks()
		{
			if (UserSettingsVM.Instance.SeriesTvDBLinksExpanded)
			{
				AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
				if (ser == null) return;
				ucTvDBLinks.DataContext = ser.AniDB_Anime;
			}
		}

		private void ShowNextEpisode()
		{
			//if (UserSettingsVM.Instance.SeriesNextEpisodeExpanded)
			//{
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
			//}
		}

		void btnAnimeGroupShow_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.SeriesGroupExpanded = !UserSettingsVM.Instance.SeriesGroupExpanded;
		}

		

		void AnimeSeries_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			try
			{
				decimal rating = (decimal)ev.RatingValue;

				int voteType = 1;
				if (cboVoteType.SelectedItem.ToString() == Properties.Resources.VoteTypeAnimeTemporary) voteType = 2;
				if (cboVoteType.SelectedItem.ToString() == Properties.Resources.VoteTypeAnimePermanent) voteType = 1;

				JMMServerVM.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

				// refresh the data
				//ser.RefreshBase();
				//ser.AniDB_Anime.Detail.RefreshBase();
				MainListHelperVM.Instance.UpdateHeirarchy(ser);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
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
				AnimeSeriesVM serTemp = null;
				bool newStatus = false;

				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					MainListHelperVM.Instance.UpdateHeirarchy(vid);

					serTemp = MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

					serTemp = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
				}

				ShowNextEpisode();

				if (newStatus == true && serTemp != null)
				{
					Utils.PromptToRateSeries(serTemp, parentWindow);
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
