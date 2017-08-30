using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for NewSeries.xaml
    /// </summary>
    public partial class NewSeries : Window
    {
        public VM_AnimeSeries_User AnimeSeries { get; set; }
        public VM_AnimeSearch SelectedAnime { get; set; }

        public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<VM_AnimeGroup_User> AllGroups { get; set; }

        public ICollectionView ViewSearchResults { get; set; }
        public ObservableCollectionEx<VM_AnimeSearch> SearchResults { get; set; }
        
        public static readonly DependencyProperty IsNewGroupProperty = DependencyProperty.Register("IsNewGroup",
            typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty IsExistingGroupProperty = DependencyProperty.Register("IsExistingGroup",
            typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty IsAnimeDisplayedProperty = DependencyProperty.Register("IsAnimeDisplayed",
            typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

        public bool IsNewGroup
        {
            get { return (bool)GetValue(IsNewGroupProperty); }
            set { SetValue(IsNewGroupProperty, value); }
        }

        public bool IsExistingGroup
        {
            get { return (bool)GetValue(IsExistingGroupProperty); }
            set { SetValue(IsExistingGroupProperty, value); }
        }

        public bool IsAnimeDisplayed
        {
            get { return (bool)GetValue(IsAnimeDisplayedProperty); }
            set { SetValue(IsAnimeDisplayedProperty, value); }
        }



        public static readonly DependencyProperty IsAnimeSelectedProperty = DependencyProperty.Register("IsAnimeSelected",
            typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

        public bool IsAnimeSelected
        {
            get { return (bool)GetValue(IsAnimeSelectedProperty); }
            set { SetValue(IsAnimeSelectedProperty, value); }
        }


        public static readonly DependencyProperty IsAnimeNotSelectedProperty = DependencyProperty.Register("IsAnimeNotSelected",
            typeof(bool), typeof(NewSeries), new UIPropertyMetadata(true, null));

        public bool IsAnimeNotSelected
        {
            get { return (bool)GetValue(IsAnimeNotSelectedProperty); }
            set { SetValue(IsAnimeNotSelectedProperty, value); }
        }


        public NewSeries()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            AnimeSeries = null;

            txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);

            btnClearGroupSearch.Click += new RoutedEventHandler(btnClearGroupSearch_Click);
            btnAnimeSearch.Click += new RoutedEventHandler(btnAnimeSearch_Click);
            btnStep1.Click += new RoutedEventHandler(btnStep1_Click);

            rbGroupExisting.Checked += new RoutedEventHandler(rbGroupExisting_Checked);
            rbGroupNew.Checked += new RoutedEventHandler(rbGroupNew_Checked);

            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnConfirm.Click += new RoutedEventHandler(btnConfirm_Click);

            lbAnime.SelectionChanged += new SelectionChangedEventHandler(lbAnime_SelectionChanged);
            Loaded += new RoutedEventHandler(NewSeries_Loaded);
        }

        void NewSeries_Loaded(object sender, RoutedEventArgs e)
        {
            txtAnimeSearch.Focus();
        }

        void btnStep1_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedAnime(null);

            IsAnimeDisplayed = false;

            EvaluateRadioButtons();
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeSearch))
                {
                    Cursor = Cursors.Wait;
                    VM_AnimeSearch searchResult = obj as VM_AnimeSearch;

                    SetSelectedAnime(searchResult);

                    txtGroupName.Text = searchResult.MainTitle;
                    txtGroupSortName.Text = searchResult.MainTitle;
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

        void btnAnimeSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetSelectedAnime(null);

                IsAnimeDisplayed = false;

                Cursor = Cursors.Wait;
                Window parentWindow = GetWindow(this);
                btnAnimeSearch.IsEnabled = false;
                btnConfirm.IsEnabled = false;
                btnCancel.IsEnabled = false;
                SearchResults.ReplaceRange(VM_ShokoServer.Instance.ShokoServices
                    .OnlineAnimeTitleSearch(txtAnimeSearch.Text.Replace("'", "`").Trim()).Cast<VM_AnimeSearch>());
                ViewSearchResults.Refresh();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                btnAnimeSearch.IsEnabled = true;
                btnConfirm.IsEnabled = true;
                btnCancel.IsEnabled = true;
                Cursor = Cursors.Arrow;
            }
        }

        void lbAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsAnimeDisplayed = false;
            VM_AnimeSearch searchResult = lbAnime.SelectedItem as VM_AnimeSearch;
            if (searchResult == null) return;

            SetAnimeDisplay(searchResult);
        }

        private void SetAnimeDisplay(VM_AnimeSearch searchResult)
        {
            txtTitles.Text = "";
            txtMainTitle.Text = searchResult.MainTitle;

            lnkAniDB.DisplayText = searchResult.AnimeID_Friendly;
            lnkAniDB.URL = searchResult.AniDB_SiteURL;

            try
            {
                //make sure list is unique
                SortedDictionary<string, string> sortedTitles = new SortedDictionary<string, string>();
                foreach (string tit in searchResult.Titles)
                {
                    if (!string.IsNullOrEmpty(tit))
                    {
                        sortedTitles[tit] = tit;
                    }
                }

                foreach (string tit in sortedTitles.Values)
                {
                    txtTitles.Text += tit;
                    txtTitles.Text += Environment.NewLine;
                }
            }
            catch { }

            //txtDescription.Text = searchResult.Titles;

            IsAnimeDisplayed = true;
        }

        void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //VM_AnimeGroup_User grp = null;
            int animeID = 0;
            int? groupID = null;

            try
            {


                if (IsAnimeNotSelected)
                {
                    MessageBox.Show(Commons.Properties.Resources.NewSeries_SelectAnime,
                        Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtAnimeSearch.Focus();
                    return;
                }

                if (IsExistingGroup)
                {
                    if (lbGroups.SelectedItem == null)
                    {
                        MessageBox.Show(Commons.Properties.Resources.MSG_ERR_GroupSelectionRequired,
                            Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        lbGroups.Focus();
                        return;
                    }
                    else
                    {
                        VM_AnimeGroup_User grp = lbGroups.SelectedItem as VM_AnimeGroup_User;
                        groupID = grp.AnimeGroupID;
                    }
                }

                if (IsNewGroup)
                {
                    if (txtGroupName.Text.Trim().Length == 0)
                    {
                        MessageBox.Show(Commons.Properties.Resources.MSG_ERR_GroupNameRequired,
                            Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        txtGroupName.Focus();
                        return;
                    }
                }

                if (SelectedAnime != null)
                    animeID = SelectedAnime.AnimeID;

                Cursor = Cursors.Wait;


                CL_Response<CL_AnimeSeries_User> response = VM_ShokoServer.Instance.ShokoServices.CreateSeriesFromAnime(
                    animeID, groupID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (response.ErrorMessage.Length > 0)
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show(response.ErrorMessage, Commons.Properties.Resources.Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                AnimeSeries = (VM_AnimeSeries_User)response.Result;
                VM_MainListHelper.Instance.AllSeriesDictionary[response.Result.AnimeSeriesID] =
                    (VM_AnimeSeries_User) response.Result;
                if (!VM_MainListHelper.Instance.AllGroupsDictionary.ContainsKey(response.Result.AnimeGroupID))
                {
                    var group = VM_ShokoServer.Instance.ShokoServices.GetGroup(response.Result.AnimeGroupID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    VM_MainListHelper.Instance.AllGroupsDictionary[response.Result.AnimeGroupID] =
                        (VM_AnimeGroup_User) group;
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

            DialogResult = true;
            Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            AnimeSeries = null;
            Close();
        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {


            //Uri uri = new Uri(string.Format(Shoko.Models.Constants.URLS.AniDB_Series, id));
            //Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void rbGroupNew_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
            txtGroupName.Focus();
        }

        void rbGroupExisting_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        private void EvaluateRadioButtons()
        {
            IsNewGroup = rbGroupNew.IsChecked.Value && IsAnimeSelected;
            IsExistingGroup = rbGroupExisting.IsChecked.Value && IsAnimeSelected;
        }


        void btnClearGroupSearch_Click(object sender, RoutedEventArgs e)
        {
            txtGroupSearch.Text = "";
        }



        private void SetSelectedAnime(VM_AnimeSearch searchResult)
        {
            if (searchResult != null)
            {
                //IsAnimeNotPopulated = false;
                //IsAnimePopulated = true;
                IsAnimeSelected = true;
                IsAnimeNotSelected = false;
                SelectedAnime = searchResult;
                SetAnimeDisplay(SelectedAnime);
            }
            else
            {
                //IsAnimeNotPopulated = true;
                //IsAnimePopulated = false;
                IsAnimeSelected = false;
                IsAnimeNotSelected = true;
                SelectedAnime = null;
            }

            EvaluateRadioButtons();
        }

        public void Init(VM_AniDB_Anime anime, string defaultGroupName)
        {
            VM_AnimeSearch srch = new VM_AnimeSearch();
            srch.AnimeID = anime.AnimeID;
            srch.MainTitle = anime.MainTitle;
            srch.Titles = new HashSet<string>(anime.GetAllTitles(),StringComparer.InvariantCultureIgnoreCase);

            SetSelectedAnime(srch);
            EvaluateRadioButtons();

            rbGroupExisting.IsChecked = true;

            AllGroups = new ObservableCollection<VM_AnimeGroup_User>();
            SearchResults = new ObservableCollectionEx<VM_AnimeSearch>();

            try
            {

                ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
                ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

                ViewSearchResults = CollectionViewSource.GetDefaultView(SearchResults);
                ViewSearchResults.SortDescriptions.Add(new SortDescription("MainTitle", ListSortDirection.Ascending));

                List<VM_AnimeGroup_User> grpsRaw = VM_ShokoServer.Instance.ShokoServices
                    .GetAllGroups(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeGroup_User>();

                foreach (VM_AnimeGroup_User grp in grpsRaw)
                {
                    AllGroups.Add(grp);
                }

                ViewGroups.Filter = GroupSearchFilter;

                txtGroupName.Text = defaultGroupName;
                txtGroupSortName.Text = defaultGroupName;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void Init(int animeID, string defaultGroupName)
        {
            //SetSelectedAnime(animeID);

            SetSelectedAnime(null);
            EvaluateRadioButtons();

            rbGroupExisting.IsChecked = true;

            AllGroups = new ObservableCollection<VM_AnimeGroup_User>();
            SearchResults = new ObservableCollectionEx<VM_AnimeSearch>();

            try
            {

                ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
                ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

                ViewSearchResults = CollectionViewSource.GetDefaultView(SearchResults);
                ViewSearchResults.SortDescriptions.Add(new SortDescription("MainTitle", ListSortDirection.Ascending));

                List<VM_AnimeGroup_User> grpsRaw = VM_ShokoServer.Instance.ShokoServices
                    .GetAllGroups(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeGroup_User>();

                foreach (VM_AnimeGroup_User grp in grpsRaw)
                {
                    AllGroups.Add(grp);
                }

                ViewGroups.Filter = GroupSearchFilter;

                txtGroupName.Text = defaultGroupName;
                txtGroupSortName.Text = defaultGroupName;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewGroups.Refresh();
        }


        private bool GroupSearchFilter(object obj)
        {
            VM_AnimeGroup_User grpvm = obj as VM_AnimeGroup_User;
            if (grpvm == null) return true;

            return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
        }
    }
}
