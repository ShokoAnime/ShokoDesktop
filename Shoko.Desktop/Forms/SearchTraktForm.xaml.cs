using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.UserControls.Settings;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Client;
using Shoko.Models.Enums;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SearchTraktForm.xaml
    /// </summary>
    public partial class SearchTraktForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ICollectionView ViewTraktSeriesSearchResults { get; set; }
        public ObservableCollectionEx<CL_TraktTVShowResponse> TraktSeriesSearchResults { get; set; }

        public ICollectionView ViewCrossRef_AniDB_TraktResult { get; set; }
        public ObservableCollectionEx<VM_CrossRef_AniDB_TraktV2> CrossRef_AniDB_TraktResult { get; set; }
        
        public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
            typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
            typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
            typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));


        public bool IsSearch
        {
            get { return (bool)GetValue(IsSearchProperty); }
            set { SetValue(IsSearchProperty, value); }
        }

        public bool IsExisting
        {
            get { return (bool)GetValue(IsExistingProperty); }
            set { SetValue(IsExistingProperty, value); }
        }

        public bool HasWebCacheRec
        {
            get { return (bool)GetValue(HasWebCacheRecProperty); }
            set { SetValue(HasWebCacheRecProperty, value); }
        }



        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
            typeof(string), typeof(SearchTraktForm), new UIPropertyMetadata("", null));

        public string AnimeName
        {
            get { return (string)GetValue(AnimeNameProperty); }
            set { SetValue(AnimeNameProperty, value); }
        }

        private int AnimeID = 0;
        private string ExistingTraktID = "";
        public string SelectedTraktID = "";
        private VM_AniDB_Anime Anime = null;

        public SearchTraktForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            TraktSeriesSearchResults = new ObservableCollectionEx<CL_TraktTVShowResponse>();
            ViewTraktSeriesSearchResults = CollectionViewSource.GetDefaultView(TraktSeriesSearchResults);

            CrossRef_AniDB_TraktResult = new ObservableCollectionEx<VM_CrossRef_AniDB_TraktV2>();
            ViewCrossRef_AniDB_TraktResult = CollectionViewSource.GetDefaultView(CrossRef_AniDB_TraktResult);

            rbExisting.Checked += new RoutedEventHandler(rbExisting_Checked);
            rbSearch.Checked += new RoutedEventHandler(rbSearch_Checked);

            hlURL.Click += new RoutedEventHandler(hlURL_Click);
            //hlURLWebCache.Click += new RoutedEventHandler(hlURLWebCache_Click);

            rbSearch.IsChecked = true;
            rbExisting.IsChecked = false;

            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            btnUseThis.Click += new RoutedEventHandler(btnUseThis_Click);
            btnUseThisExisting.Click += new RoutedEventHandler(btnUseThisExisting_Click);

            btnChkCred.Click += new RoutedEventHandler(btnChkCred_Click);
        }

        void btnUseThisExisting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // prompt to select season
                Window wdw = GetWindow(this);

                Cursor = Cursors.Wait;
                SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
                frm.Owner = wdw;
                frm.Init(AnimeID, AnimeName, enEpisodeType.Episode, 1, txtSeriesID.Text.Trim(), 1, 1, AnimeName, Anime, null);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    SelectedTraktID = txtSeriesID.Text.Trim();
                    DialogResult = true;
                    Cursor = Cursors.Arrow;
                    Close();
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnUseThis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;

                // remove any existing links
                string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTraktForAnime(AnimeID);
                if (res.Length > 0)
                {
                    MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    Cursor = Cursors.Arrow;
                    return;
                }

                // add links
                foreach (VM_CrossRef_AniDB_TraktV2 xref in CrossRef_AniDB_TraktResult)
                {
                    res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBTrakt(xref.AnimeID, xref.AniDBStartEpisodeType, xref.AniDBStartEpisodeNumber,
                        xref.TraktID, xref.TraktSeasonNumber, xref.TraktStartEpisodeNumber, null);
                    if (res.Length > 0)
                    {
                        MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        Cursor = Cursors.Arrow;
                        return;
                    }

                }

                DialogResult = true;
                Cursor = Cursors.Arrow;
                Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedTraktID = "";
            Close();
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CL_TraktTVShowResponse))
                {
                    Cursor = Cursors.Wait;
                    CL_TraktTVShowResponse searchResult = obj as CL_TraktTVShowResponse;

                    // prompt to select season
                    Window wdw = GetWindow(this);

                    Cursor = Cursors.Wait;
                    SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
                    frm.Owner = wdw;
                    frm.Init(AnimeID, AnimeName, enEpisodeType.Episode, 1, searchResult.GetTraktID(), 1, 1, AnimeName, Anime, null);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        SelectedTraktID = searchResult.GetTraktID();
                        DialogResult = true;
                        Cursor = Cursors.Arrow;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            HasWebCacheRec = false;
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            Cursor = Cursors.Wait;
            try
            {
                CrossRef_AniDB_TraktResult.ReplaceRange(VM_ShokoServer.Instance.ShokoServices.GetTraktCrossRefWebCache(AnimeID, false).Cast<VM_CrossRef_AniDB_TraktV2>());
                if (CrossRef_AniDB_TraktResult.Count>0)
                    HasWebCacheRec = true;

                // now search Trakt
                TraktSeriesSearchResults.ReplaceRange(VM_ShokoServer.Instance.ShokoServices.SearchTrakt(txtSearch.Text.Replace("`", "'").Trim()));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(string.Format(Constants.URLS.Trakt_Series, txtSeriesID.Text.Trim()));
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void hlURLWebCache_Click(object sender, RoutedEventArgs e)
        {
            //Uri uri = new Uri(string.Format(Shoko.Models.Constants.URLS.Trakt_Series, CrossRef_AniDB_TraktResult.TraktID));
            //Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void rbSearch_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        void rbExisting_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        private void EvaluateRadioButtons()
        {
            IsSearch = rbSearch.IsChecked.Value;
            IsExisting = rbExisting.IsChecked.Value;

            HasWebCacheRec = IsSearch && CrossRef_AniDB_TraktResult != null && CrossRef_AniDB_TraktResult.Count > 0;
        }

        public void Init(int animeID, string animeName, string searchCriteria, string existingTraktID, VM_AniDB_Anime anime)
        {
            Anime = anime;
            AnimeID = animeID;
            AnimeName = animeName;
            ExistingTraktID = existingTraktID;
            txtSearch.Text = searchCriteria;
        }

        private void btnChkCred_Click(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            TraktSettings OP = new TraktSettings();
            var host = new Window();
            host.Content = OP;
            host.Title = Shoko.Commons.Properties.Resources.Trakt_CheckCred;
            host.Width = 630;
            host.Height = 240;
            host.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            OP.Margin = new Thickness(5);
            host.ResizeMode = ResizeMode.NoResize;
            host.ShowDialog();
        }
    }
}
