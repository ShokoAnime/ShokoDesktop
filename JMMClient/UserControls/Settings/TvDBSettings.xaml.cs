using JMMClient.Forms;
using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for TvDBSettings.xaml
    /// </summary>
    public partial class TvDBSettings : UserControl
    {
        public TvDBSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            chkTvDB_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTvDB_PosterAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTvDB_WideBannerAutoDownload.Click += new RoutedEventHandler(settingChanged);
            udMaxFanarts.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxFanarts_ValueChanged);
            udMaxPosters.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxPosters_ValueChanged);
            udMaxWideBanners.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxWideBanners_ValueChanged);

            Utils.PopulateScheduledComboBox(cboUpdateFrequency, JMMServerVM.Instance.TvDB_UpdateFrequency);
            cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

            btnChangeLanguage.Click += new RoutedEventHandler(btnChangeLanguage_Click);
			btnUpdateImages.Click += new RoutedEventHandler(btnUpdateImages_Click);
		}



        void btnChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window wdw = Window.GetWindow(this);

                wdw.Cursor = Cursors.Wait;
                List<JMMServerBinary.Contract_TvDBLanguage> lans = JMMServerVM.Instance.clientBinaryHTTP.GetTvDBLanguages();
                List<TvDB_LanguageVM> languages = new List<TvDB_LanguageVM>();
                foreach (JMMServerBinary.Contract_TvDBLanguage lan in lans)
                    languages.Add(new TvDB_LanguageVM(lan));
                wdw.Cursor = Cursors.Arrow;

                SelectTvDBLanguage frm = new SelectTvDBLanguage();
                frm.Owner = wdw;
                frm.Init(languages);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    JMMServerVM.Instance.TvDB_Language = frm.SelectedLanguage;
                    JMMServerVM.Instance.SaveServerSettingsAsync();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

		void btnUpdateImages_Click(object sender, RoutedEventArgs e)
		{
			Window wdw = Window.GetWindow(this);

			wdw.Cursor = Cursors.Wait;
			ImageDownload.ImageDownloader imageHelper = MainWindow.imageHelper;

			// Download posters from TvDB
			List<JMMServerBinary.Contract_TvDB_ImagePoster> posters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBPosters(null);
			foreach (JMMServerBinary.Contract_TvDB_ImagePoster poster in posters)
			{
				imageHelper.DownloadTvDBPoster(new TvDB_ImagePosterVM(poster), true);
			}

			// Download posters from MovieDB
			List<JMMServerBinary.Contract_MovieDB_Poster> moviePosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBPosters(null);
			foreach (JMMServerBinary.Contract_MovieDB_Poster poster in moviePosters)
			{
				imageHelper.DownloadMovieDBPoster(new MovieDB_PosterVM(poster), true);
			}

			// Download wide banners from TvDB
			List<JMMServerBinary.Contract_TvDB_ImageWideBanner> banners = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBWideBanners(null);
			foreach (JMMServerBinary.Contract_TvDB_ImageWideBanner banner in banners)
			{
				imageHelper.DownloadTvDBWideBanner(new TvDB_ImageWideBannerVM(banner), true);
			}

			// Download fanart from TvDB
			List<JMMServerBinary.Contract_TvDB_ImageFanart> fanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBFanart(null);
			foreach (JMMServerBinary.Contract_TvDB_ImageFanart fanart in fanarts)
			{
				imageHelper.DownloadTvDBFanart(new TvDB_ImageFanartVM(fanart), true);
			}

			// Download fanart from MovieDB
			List<JMMServerBinary.Contract_MovieDB_Fanart> movieFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBFanart(null);
			foreach (JMMServerBinary.Contract_MovieDB_Fanart fanart in movieFanarts)
			{
				imageHelper.DownloadMovieDBFanart(new MovieDB_FanartVM(fanart), true);
			}

			// Download episode images from TvDB
			List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(null);
			foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
			{
				imageHelper.DownloadTvDBEpisode(new TvDB_EpisodeVM(episode), true);
			}

			// Download posters from Trakt
			List<JMMServerBinary.Contract_Trakt_ImagePoster> traktPosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktPosters(null);
			foreach (JMMServerBinary.Contract_Trakt_ImagePoster traktposter in traktPosters)
			{
				if (string.IsNullOrEmpty(traktposter.ImageURL)) continue;
				imageHelper.DownloadTraktPoster(new Trakt_ImagePosterVM(traktposter), true);
			}

			// Download fanart from Trakt
			List<JMMServerBinary.Contract_Trakt_ImageFanart> traktFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktFanart(null);
			foreach (JMMServerBinary.Contract_Trakt_ImageFanart traktFanart in traktFanarts)
			{
				if (string.IsNullOrEmpty(traktFanart.ImageURL)) continue;
				imageHelper.DownloadTraktFanart(new Trakt_ImageFanartVM(traktFanart), true);
			}

			// Download episode images from Trakt
			List<JMMServerBinary.Contract_Trakt_Episode> traktEpisodes = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktEpisodes(null);
			foreach (JMMServerBinary.Contract_Trakt_Episode traktEp in traktEpisodes)
			{
				if (string.IsNullOrEmpty(traktEp.EpisodeImage)) continue;

				// special case for trak episodes
				// Trakt will return the fanart image when no episode image exists, but we don't want this
				int pos = traktEp.EpisodeImage.IndexOf(@"episodes/");
				if (pos <= 0) continue;

				//logger.Trace("Episode image: {0} - {1}/{2}", traktEp.Trakt_ShowID, traktEp.Season, traktEp.EpisodeNumber);

				imageHelper.DownloadTraktEpisode(new Trakt_EpisodeVM(traktEp), true);
			}
			wdw.Cursor = Cursors.Arrow;
		}

        void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboUpdateFrequency.SelectedIndex)
            {
                case 0: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
            }

            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void udMaxFanarts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            JMMServerVM.Instance.TvDB_AutoFanartAmount = udMaxFanarts.Value.Value;
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void udMaxWideBanners_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            JMMServerVM.Instance.TvDB_AutoWideBannersAmount = udMaxWideBanners.Value.Value;
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void udMaxPosters_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            JMMServerVM.Instance.TvDB_AutoPostersAmount = udMaxPosters.Value.Value;
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }
    }
}
