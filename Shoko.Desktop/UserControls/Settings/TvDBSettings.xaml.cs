using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Settings
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

            chkAutoLink.Click += new RoutedEventHandler(settingChanged);
            chkTvDB_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTvDB_PosterAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTvDB_WideBannerAutoDownload.Click += new RoutedEventHandler(settingChanged);
            udMaxFanarts.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udMaxFanarts_ValueChanged);
            udMaxPosters.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udMaxPosters_ValueChanged);
            udMaxWideBanners.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udMaxWideBanners_ValueChanged);

            Utils.PopulateScheduledComboBox(cboUpdateFrequency, VM_ShokoServer.Instance.TvDB_UpdateFrequency);
            cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

            btnChangeLanguage.Click += new RoutedEventHandler(btnChangeLanguage_Click);
            EvaluateVisibility();
        }

        private void EvaluateVisibility()
        {
            // If we have images path set to server path hide the button and hint text to prevent accidental presses (i.e. user error)
            if (AppSettings.ImagesPath == AppSettings.JMMServerImagePath)
            {
                btnUpdateImages.Visibility = Visibility.Hidden;
                btnUpdateImages.IsEnabled = false;
                txtUpdateImagesHint.Visibility = Visibility.Hidden;
                txtUpdateImagesHint.IsEnabled = false;
            }
            else
            {
                btnUpdateImages.Visibility = Visibility.Visible;
                btnUpdateImages.IsEnabled = true;
                btnUpdateImages.Click += new RoutedEventHandler(btnUpdateImages_Click);
                txtUpdateImagesHint.Visibility = Visibility.Visible;
                txtUpdateImagesHint.IsEnabled = true;
            }
        }

        void btnChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window wdw = Window.GetWindow(this);

                wdw.Cursor = Cursors.Wait;
                List<VM_TvDB_Language> languages = VM_ShokoServer.Instance.ShokoServices.GetTvDBLanguages().CastList<VM_TvDB_Language>();
                wdw.Cursor = Cursors.Arrow;

                SelectTvDBLanguage frm = new SelectTvDBLanguage();
                frm.Owner = wdw;
                frm.Init(languages);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    VM_ShokoServer.Instance.TvDB_Language = frm.SelectedLanguage;
                    VM_ShokoServer.Instance.SaveServerSettingsAsync();
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
            List<VM_TvDB_ImagePoster> posters = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBPosters(null).CastList<VM_TvDB_ImagePoster>();
            foreach (VM_TvDB_ImagePoster poster in posters)
            {
                imageHelper.DownloadTvDBPoster(poster, true);
            }

            // Download posters from MovieDB
            List<VM_MovieDB_Poster> moviePosters = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBPosters(null).CastList<VM_MovieDB_Poster>();
            foreach (VM_MovieDB_Poster poster in moviePosters)
            {
                imageHelper.DownloadMovieDBPoster(poster, true);
            }

            // Download wide banners from TvDB
            List<VM_TvDB_ImageWideBanner> banners = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBWideBanners(null).CastList<VM_TvDB_ImageWideBanner>();
            foreach (VM_TvDB_ImageWideBanner banner in banners)
            {
                imageHelper.DownloadTvDBWideBanner(banner, true);
            }

            // Download fanart from TvDB
            List<VM_TvDB_ImageFanart> fanarts = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBFanart(null).CastList<VM_TvDB_ImageFanart>();
            foreach (VM_TvDB_ImageFanart fanart in fanarts)
            {
                imageHelper.DownloadTvDBFanart(fanart, true);
            }

            // Download fanart from MovieDB
            List<VM_MovieDB_Fanart> movieFanarts = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBFanart(null).CastList<VM_MovieDB_Fanart>();
            foreach (VM_MovieDB_Fanart fanart in movieFanarts)
            {
                imageHelper.DownloadMovieDBFanart(fanart, true);
            }

            // Download episode images from TvDB
            List<VM_TvDB_Episode> eps = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBEpisodes(null).CastList<VM_TvDB_Episode>();
            foreach (VM_TvDB_Episode episode in eps)
            {
                imageHelper.DownloadTvDBEpisode(episode, true);
            }
            wdw.Cursor = Cursors.Arrow;
        }

        void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboUpdateFrequency.SelectedIndex)
            {
                case 0: VM_ShokoServer.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: VM_ShokoServer.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: VM_ShokoServer.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: VM_ShokoServer.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: VM_ShokoServer.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: VM_ShokoServer.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
            }

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void udMaxFanarts_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_ShokoServer.Instance.TvDB_AutoFanartAmount = udMaxFanarts.Value;
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void udMaxWideBanners_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_ShokoServer.Instance.TvDB_AutoWideBannersAmount = udMaxWideBanners.Value;
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void udMaxPosters_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_ShokoServer.Instance.TvDB_AutoPostersAmount = udMaxPosters.Value;
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EvaluateVisibility();
        }
    }
}
