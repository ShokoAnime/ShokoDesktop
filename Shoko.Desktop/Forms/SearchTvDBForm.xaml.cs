using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Azure;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SearchTvDBForm.xaml
    /// </summary>
    public partial class SearchTvDBForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
            typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
            typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty ReplaceAllProperty = DependencyProperty.Register("ReplaceAll",
            typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(true, null));

        public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
            typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty CrossRef_AniDB_TvDBResultProperty = DependencyProperty.Register("CrossRef_AniDB_TvDBResult",
            typeof(List<VM_CrossRef_AniDB_TvDBV2>), typeof(SearchTvDBForm), new UIPropertyMetadata(null, null));

        public static readonly DependencyProperty TVDBSeriesSearchResultsProperty = DependencyProperty.Register("TVDBSeriesSearchResults",
            typeof(List<VM_TVDB_Series_Search_Response>), typeof(SearchTvDBForm), new UIPropertyMetadata(null, null));

        public bool IsSearch
        {
            get => (bool)GetValue(IsSearchProperty);
            set => SetValue(IsSearchProperty, value);
        }

        public bool IsExisting
        {
            get => (bool)GetValue(IsExistingProperty);
            set => SetValue(IsExistingProperty, value);
        }

        public bool ReplaceAll
        {
            get => (bool)GetValue(ReplaceAllProperty);
            set => SetValue(ReplaceAllProperty, value);
        }

        public bool HasWebCacheRec
        {
            get => (bool)GetValue(HasWebCacheRecProperty);
            set => SetValue(HasWebCacheRecProperty, value);
        }

        public List<VM_CrossRef_AniDB_TvDBV2> CrossRef_AniDB_TvDBResult
        {
            get => (List<VM_CrossRef_AniDB_TvDBV2>)GetValue(CrossRef_AniDB_TvDBResultProperty);
            set => SetValue(CrossRef_AniDB_TvDBResultProperty, value);
        }

        public List<VM_TVDB_Series_Search_Response> TVDBSeriesSearchResults
        {
            get => (List<VM_TVDB_Series_Search_Response>)GetValue(TVDBSeriesSearchResultsProperty);
            set => SetValue(TVDBSeriesSearchResultsProperty, value);
        }

        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
            typeof(string), typeof(SearchTvDBForm), new UIPropertyMetadata("", null));

        public string AnimeName
        {
            get => (string)GetValue(AnimeNameProperty);
            set => SetValue(AnimeNameProperty, value);
        }

        private int AnimeID;
        private int? ExistingTVDBID;
        public int? SelectedTvDBID;
        private VM_AniDB_Anime Anime;

        private bool tvDbDataReady(int id)
        {

            VM_ShokoServer.Instance.ShokoServices.UpdateTvDBData(id);
            VM_TvDBDetails TvDetails = new VM_TvDBDetails(id);
            if (TvDetails.TvDBEpisodes == null || TvDetails.TvDBEpisodes.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public SearchTvDBForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            CrossRef_AniDB_TvDBResult = new List<VM_CrossRef_AniDB_TvDBV2>();

            rbExisting.Checked += rbExisting_Checked;
            rbSearch.Checked += rbSearch_Checked;

            hlURL.Click += hlURL_Click;

            rbSearch.IsChecked = true;
            rbExisting.IsChecked = false;

            txtSearch.KeyDown += (sender, evt) =>
            {
                if (evt.Key == Key.Enter || evt.Key == Key.Return)
                    btnSearch_Click(this, new RoutedEventArgs());
            };

            btnSearch.Click += btnSearch_Click;
            btnClose.Click += btnClose_Click;
            btnUseThis.Click += btnUseThis_Click;
            btnUseThisExisting.Click += btnUseThisExisting_Click;
        }

        void btnUseThisExisting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id;
                int.TryParse(txtSeriesID.Text, out id);
                if (id <= 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.Search_InvalidTvDB, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtSeriesID.Focus();
                    return;
                }

                Cursor = Cursors.Wait;

                // prompt to select season
                Window wdw = GetWindow(this);

                Cursor = Cursors.Wait;

                if (!tvDbDataReady(id))
                {
                    Utils.ShowErrorMessage("The series data is being downloaded, try again when the queue has cleared.");
                    return;
                }
                else
                {
                    TvDBMatchPreview frm = new TvDBMatchPreview {Owner = wdw};
                    frm.Init(AnimeID, AnimeName, id, AnimeName, Anime,
                        !ReplaceAll);
                    bool? result = frm.ShowDialog();
                    if (result != null && result.Value)
                    {
                        SelectedTvDBID = id;
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

        void btnUseThis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                // add links
                string res =
                    VM_ShokoServer.Instance.ShokoServices.LinkTvDBUsingWebCacheLinks(CrossRef_AniDB_TvDBResult
                        .Cast<CrossRef_AniDB_TvDBV2>().ToList());
                if (res.Length > 0)
                {
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    Cursor = Cursors.Arrow;
                    Close();
                    return;
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
            SelectedTvDBID = null;
            Close();
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_TVDB_Series_Search_Response))
                {
                    Cursor = Cursors.Wait;
                    VM_TVDB_Series_Search_Response searchResult = obj as VM_TVDB_Series_Search_Response;

                    // prompt to select season
                    Window wdw = GetWindow(this);

                    Cursor = Cursors.Wait;

                    if (!tvDbDataReady(searchResult.SeriesID))
                    {
                        Utils.ShowErrorMessage("The series data is being downloaded, try again when the queue has cleared.");
                        return;
                    }
                    else
                    {
                        TvDBMatchPreview frm = new TvDBMatchPreview {Owner = wdw};
                        frm.Init(AnimeID, AnimeName, searchResult.SeriesID, searchResult.SeriesName, Anime,
                            !ReplaceAll);
                        bool? result = frm.ShowDialog();
                        if (result != null && result.Value)
                        {
                            SelectedTvDBID = searchResult.SeriesID;
                            DialogResult = true;
                            Cursor = Cursors.Arrow;
                            Close();
                        }
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
                CrossRef_AniDB_TvDBResult.Clear();
                // first find what the community recommends
                List<Azure_CrossRef_AniDB_TvDB> xrefs = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefWebCache(AnimeID, false);
                if (xrefs != null && xrefs.Count > 0)
                {
                    foreach (Azure_CrossRef_AniDB_TvDB xref in xrefs)
                    {
                        CrossRef_AniDB_TvDBResult.Add((VM_CrossRef_AniDB_TvDBV2)xref);
                    }

                    HasWebCacheRec = true;
                }

                // now search the TvDB
                TVDBSeriesSearchResults = VM_ShokoServer.Instance.ShokoServices.SearchTheTvDB(txtSearch.Text.Replace("`", "'").Trim()).CastList<VM_TVDB_Series_Search_Response>();
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
            int id = 0;
            int.TryParse(txtSeriesID.Text, out id);
            if (id <= 0) return;

            Uri uri = new Uri(string.Format(Constants.URLS.TvDB_Series, id));
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
        }

        void hlURLWebCache_Click(object sender, RoutedEventArgs e)
        {
            //Uri uri = new Uri(string.Format(Shoko.Models.Constants.URLS.TvDB_Series, CrossRef_AniDB_TvDBResult.TvDBID));
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

            HasWebCacheRec = IsSearch && CrossRef_AniDB_TvDBResult != null && CrossRef_AniDB_TvDBResult.Count > 0;
        }

        public void Init(int animeID, string animeName, string searchCriteria, int? existingTVDBID, VM_AniDB_Anime anime)
        {
            Anime = anime;
            AnimeID = animeID;
            AnimeName = animeName;
            ExistingTVDBID = existingTVDBID;
            txtSearch.Text = searchCriteria;
        }
    }
}
