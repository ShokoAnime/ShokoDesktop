using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for NewSeries.xaml
    /// </summary>
    public partial class NewSeries : Window
    {
        public AnimeSeriesVM AnimeSeries { get; set; }
        public AnimeSearchVM SelectedAnime { get; set; }

        public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<AnimeGroupVM> AllGroups { get; set; }

        public ICollectionView ViewSearchResults { get; set; }
        public ObservableCollection<AnimeSearchVM> SearchResults { get; set; }

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
            this.Loaded += new RoutedEventHandler(NewSeries_Loaded);
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
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(AnimeSearchVM))
                {
                    this.Cursor = Cursors.Wait;
                    AnimeSearchVM searchResult = obj as AnimeSearchVM;

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
                this.Cursor = Cursors.Arrow;
            }
        }

        void btnAnimeSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetSelectedAnime(null);

                IsAnimeDisplayed = false;

                this.Cursor = Cursors.Wait;
                Window parentWindow = Window.GetWindow(this);
                btnAnimeSearch.IsEnabled = false;
                btnConfirm.IsEnabled = false;
                btnCancel.IsEnabled = false;

                SearchResults.Clear();
                ViewSearchResults.Refresh();

                List<JMMServerBinary.Contract_AnimeSearch> searchResults = JMMServerVM.Instance.clientBinaryHTTP.OnlineAnimeTitleSearch(txtAnimeSearch.Text.Replace("'", "`").Trim());
                foreach (JMMServerBinary.Contract_AnimeSearch res in searchResults)
                {
                    AnimeSearchVM ser = new AnimeSearchVM(res);
                    SearchResults.Add(ser);
                }
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
                this.Cursor = Cursors.Arrow;
            }
        }

        void lbAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsAnimeDisplayed = false;
            AnimeSearchVM searchResult = lbAnime.SelectedItem as AnimeSearchVM;
            if (searchResult == null) return;

            SetAnimeDisplay(searchResult);
        }

        private void SetAnimeDisplay(AnimeSearchVM searchResult)
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
            //AnimeGroupVM grp = null;
            int animeID = 0;
            int? groupID = null;

            try
            {


                if (IsAnimeNotSelected)
                {
                    MessageBox.Show(Properties.Resources.NewSeries_SelectAnime, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtAnimeSearch.Focus();
                    return;
                }

                if (IsExistingGroup)
                {
                    if (lbGroups.SelectedItem == null)
                    {
                        MessageBox.Show(Properties.Resources.MSG_ERR_GroupSelectionRequired, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        lbGroups.Focus();
                        return;
                    }
                    else
                    {
                        AnimeGroupVM grp = lbGroups.SelectedItem as AnimeGroupVM;
                        groupID = grp.AnimeGroupID.Value;
                    }
                }

                if (IsNewGroup)
                {
                    if (txtGroupName.Text.Trim().Length == 0)
                    {
                        MessageBox.Show(Properties.Resources.MSG_ERR_GroupNameRequired, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        txtGroupName.Focus();
                        return;
                    }
                }

                if (SelectedAnime != null)
                    animeID = SelectedAnime.AnimeID;

                this.Cursor = Cursors.Wait;

                if (IsNewGroup)
                {
                    AnimeGroupVM grp = new AnimeGroupVM();
                    grp.GroupName = txtGroupName.Text.Trim();
                    grp.SortName = txtGroupName.Text.Trim();
                    grp.AnimeGroupParentID = null;
                    grp.Description = "";
                    grp.IsFave = 0;
                    grp.IsManuallyNamed = 0;
                    grp.OverrideDescription = 0;


                    if (grp.Validate())
                    {
                        grp.IsReadOnly = true;
                        grp.IsBeingEdited = false;
                        if (grp.Save())
                        {
                            MainListHelperVM.Instance.ViewGroups.Refresh();
                            groupID = grp.AnimeGroupID;
                        }

                    }
                }


                JMMServerBinary.Contract_AnimeSeries_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.CreateSeriesFromAnime(animeID, groupID,
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                if (response.ErrorMessage.Length > 0)
                {
                    this.Cursor = Cursors.Arrow;
                    MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AnimeSeries = new AnimeSeriesVM(response.AnimeSeries);
                MainListHelperVM.Instance.AllSeriesDictionary[AnimeSeries.AnimeSeriesID.Value] = AnimeSeries;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

            this.DialogResult = true;
            this.Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            AnimeSeries = null;
            this.Close();
        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {


            //Uri uri = new Uri(string.Format(Constants.URLS.AniDB_Series, id));
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



        private void SetSelectedAnime(AnimeSearchVM searchResult)
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

		public void Init(AniDB_AnimeVM anime, string defaultGroupName)
		{
			AnimeSearchVM srch = new AnimeSearchVM();
			srch.AnimeID = anime.AnimeID;
			srch.MainTitle = anime.MainTitle;
			srch.Titles = new HashSet<string>(anime.AllTitles,StringComparer.InvariantCultureIgnoreCase);

            SetSelectedAnime(srch);
            EvaluateRadioButtons();

            rbGroupExisting.IsChecked = true;

            AllGroups = new ObservableCollection<AnimeGroupVM>();
            SearchResults = new ObservableCollection<AnimeSearchVM>();

            try
            {

                ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
                ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

                ViewSearchResults = CollectionViewSource.GetDefaultView(SearchResults);
                ViewSearchResults.SortDescriptions.Add(new SortDescription("MainTitle", ListSortDirection.Ascending));

                List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
                {
                    AnimeGroupVM grpNew = new AnimeGroupVM(grp);
                    AllGroups.Add(grpNew);
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

            AllGroups = new ObservableCollection<AnimeGroupVM>();
            SearchResults = new ObservableCollection<AnimeSearchVM>();

            try
            {

                ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
                ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

                ViewSearchResults = CollectionViewSource.GetDefaultView(SearchResults);
                ViewSearchResults.SortDescriptions.Add(new SortDescription("MainTitle", ListSortDirection.Ascending));

                List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
                {
                    AnimeGroupVM grpNew = new AnimeGroupVM(grp);
                    AllGroups.Add(grpNew);
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
            AnimeGroupVM grpvm = obj as AnimeGroupVM;
            if (grpvm == null) return true;

            return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
        }
    }
}
