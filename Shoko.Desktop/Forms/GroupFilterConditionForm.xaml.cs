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
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for GroupFilterConditionForm.xaml
    /// </summary>
    public partial class GroupFilterConditionForm : Window
    {
        public VM_GroupFilter groupFilter;
        public VM_GroupFilterCondition groupFilterCondition;
        public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<VM_AnimeGroup_User> AllGroups { get; set; }

        public ICollectionView ViewTagNames { get; set; }
        public ObservableCollection<string> AllTagNames { get; set; }

        public ICollectionView ViewCustomTagNames { get; set; }
        public ObservableCollection<string> AllCustomTagNames { get; set; }

        public ICollectionView ViewYears { get; set; }
        public ObservableCollection<string> AllYears { get; set; }

        public ICollectionView ViewSeasons { get; set; }
        public ObservableCollection<string> AllSeasons { get; set; }

        public ObservableCollection<string> AllVideoQuality { get; set; }
        public ObservableCollection<string> AllAnimeTypes { get; set; }
        public ObservableCollection<string> AllAudioLanguages { get; set; }
        public ObservableCollection<string> AllSubtitleLanguages { get; set; }

        public static readonly DependencyProperty IsParameterDateProperty = DependencyProperty.Register("IsParameterDate",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterDate
        {
            get => (bool)GetValue(IsParameterDateProperty);
            set => SetValue(IsParameterDateProperty, value);
        }

        public static readonly DependencyProperty IsParameterAnimeGroupProperty = DependencyProperty.Register("IsParameterAnimeGroup",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterAnimeGroup
        {
            get => (bool)GetValue(IsParameterAnimeGroupProperty);
            set => SetValue(IsParameterAnimeGroupProperty, value);
        }

        public static readonly DependencyProperty IsParameterAnimeTypeProperty = DependencyProperty.Register("IsParameterAnimeType",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterAnimeType
        {
            get => (bool)GetValue(IsParameterAnimeTypeProperty);
            set => SetValue(IsParameterAnimeTypeProperty, value);
        }

        public static readonly DependencyProperty IsParameterTextProperty = DependencyProperty.Register("IsParameterText",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterText
        {
            get => (bool)GetValue(IsParameterTextProperty);
            set => SetValue(IsParameterTextProperty, value);
        }

        public static readonly DependencyProperty IsParameterInNotInProperty = DependencyProperty.Register("IsParameterInNotIn",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterInNotIn
        {
            get => (bool)GetValue(IsParameterInNotInProperty);
            set => SetValue(IsParameterInNotInProperty, value);
        }

        public static readonly DependencyProperty IsParameterTagProperty = DependencyProperty.Register("IsParameterTag",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterTag
        {
            get => (bool)GetValue(IsParameterTagProperty);
            set => SetValue(IsParameterTagProperty, value);
        }

        public static readonly DependencyProperty IsParameterCustomTagProperty = DependencyProperty.Register("IsParameterCustomTag",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterCustomTag
        {
            get => (bool)GetValue(IsParameterCustomTagProperty);
            set => SetValue(IsParameterCustomTagProperty, value);
        }

        public static readonly DependencyProperty IsParameterVideoQualityProperty = DependencyProperty.Register("IsParameterVideoQuality",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterVideoQuality
        {
            get => (bool)GetValue(IsParameterVideoQualityProperty);
            set => SetValue(IsParameterVideoQualityProperty, value);
        }

        public static readonly DependencyProperty IsParameterAudioLanguageProperty = DependencyProperty.Register("IsParameterAudioLanguage",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterAudioLanguage
        {
            get => (bool)GetValue(IsParameterAudioLanguageProperty);
            set => SetValue(IsParameterAudioLanguageProperty, value);
        }

        public static readonly DependencyProperty IsParameterSubtitleLanguageProperty = DependencyProperty.Register("IsParameterSubtitleLanguage",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterSubtitleLanguage
        {
            get => (bool)GetValue(IsParameterSubtitleLanguageProperty);
            set => SetValue(IsParameterSubtitleLanguageProperty, value);
        }


        public static readonly DependencyProperty IsParameterRatingProperty = DependencyProperty.Register("IsParameterRating",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterRating
        {
            get => (bool)GetValue(IsParameterRatingProperty);
            set => SetValue(IsParameterRatingProperty, value);
        }

        public static readonly DependencyProperty IsParameterIntegerProperty = DependencyProperty.Register("IsParameterInteger",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterInteger
        {
            get => (bool)GetValue(IsParameterIntegerProperty);
            set => SetValue(IsParameterIntegerProperty, value);
        }

        public static readonly DependencyProperty IsParameterLastXDaysProperty = DependencyProperty.Register("IsParameterLastXDays",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterLastXDays
        {
            get => (bool)GetValue(IsParameterLastXDaysProperty);
            set => SetValue(IsParameterLastXDaysProperty, value);
        }

        public static readonly DependencyProperty IsParameterYearProperty = DependencyProperty.Register("IsParameterYear",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterYear
        {
            get => (bool)GetValue(IsParameterYearProperty);
            set => SetValue(IsParameterYearProperty, value);
        }

        public static readonly DependencyProperty IsParameterSeasonProperty = DependencyProperty.Register("IsParameterSeason",
            typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

        public bool IsParameterSeason
        {
            get => (bool)GetValue(IsParameterSeasonProperty);
            set => SetValue(IsParameterSeasonProperty, value);
        }

        public GroupFilterConditionForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            txtGroupSearch.TextChanged += txtGroupSearch_TextChanged;
            btnClearGroupSearch.Click += btnClearGroupSearch_Click;

            txtTagSearch.TextChanged += txtTagSearch_TextChanged;
            btnClearTagSearch.Click += btnClearTagSearch_Click;

            btnCancel.Click += btnCancel_Click;
            btnConfirm.Click += btnConfirm_Click;

            lbTags.MouseDoubleClick += lbTags_MouseDoubleClick;
            lbVideoQuality.MouseDoubleClick += lbVideoQuality_MouseDoubleClick;
            lbAnimeTypes.MouseDoubleClick += lbAnimeTypes_MouseDoubleClick;
            lbAudioLanguages.MouseDoubleClick += lbAudioLanguages_MouseDoubleClick;
            lbSubtitleLanguages.MouseDoubleClick += lbSubtitleLanguages_MouseDoubleClick;
            lbYears.MouseDoubleClick += lbYears_MouseDoubleClick;
            lbSeasons.MouseDoubleClick += lbSeasons_MouseDoubleClick;

            btnClearCustomTagSearch.Click += btnClearCustomTagSearch_Click;
            txtCustomTagSearch.TextChanged += txtCustomTagSearch_TextChanged;
            lbCustomTags.MouseDoubleClick += lbCustomTags_MouseDoubleClick;
        }

        void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // get the details from the form
            groupFilterCondition.ConditionType = (int)cboConditionType.SelectedItem.ToString().GetEnumForText_ConditionType();
            groupFilterCondition.ConditionOperator = (int)cboConditionOperator.SelectedItem.ToString().GetEnumForText_Operator();

            NumberStyles style = NumberStyles.Number;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");


            // get the parameter details
            if (IsParameterDate)
            {
                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_SelectDate, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    dpDate.Focus();
                    return;
                }
                groupFilterCondition.ConditionParameter = dpDate.SelectedDate.Value.GetDateAsString();
            }

            if (IsParameterRating)
            {
                if (txtParameter.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                decimal dRating = -1;
                decimal.TryParse(txtParameter.Text, style, culture, out dRating);
                if (dRating <= 0 || dRating > 10)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_RatingValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }

                groupFilterCondition.ConditionParameter = txtParameter.Text.Trim();
            }

            if (IsParameterInteger)
            {
                if (txtParameter.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                int parmInt = -1;
                if (!int.TryParse(txtParameter.Text, out parmInt))
                {
                    MessageBox.Show(Commons.Properties.Resources.GroupFilter_IntegerOnly, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }

                groupFilterCondition.ConditionParameter = parmInt.ToString();
            }

            if (IsParameterLastXDays)
            {
                if (txtParameter.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                int days = -1;
                int.TryParse(txtParameter.Text, out days);
                if (days < 1 || days > int.MaxValue)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_DaysValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }

                groupFilterCondition.ConditionParameter = txtParameter.Text.Trim();
            }


            if (IsParameterTag)
            {
                if (txtSelectedTags.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] cats = txtSelectedTags.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string cat in cats)
                {
                    if (cat.Trim().Length == 0) continue;
                    if (cat.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += cat;
                }
            }

            if (IsParameterCustomTag)
            {
                if (txtSelectedCustomTags.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtSelectedCustomTags.Focus();
                    return;
                }
                // validate
                string[] tags = txtSelectedCustomTags.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string tag in tags)
                {
                    if (tag.Trim().Length == 0) continue;
                    if (tag.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += tag;
                }
            }

            if (IsParameterVideoQuality)
            {

                if (txtSelectedVideoQuality.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] vidQuals = txtSelectedVideoQuality.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string vidq in vidQuals)
                {
                    if (vidq.Trim().Length == 0) continue;
                    if (vidq.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += vidq;
                }
            }

            if (IsParameterAudioLanguage)
            {

                if (txtSelectedAudioLanguages.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] languages = txtSelectedAudioLanguages.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string lanName in languages)
                {
                    if (lanName.Trim().Length == 0) continue;
                    if (lanName.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += lanName;
                }
            }

            if (IsParameterSubtitleLanguage)
            {

                if (txtSelectedSubtitleLanguages.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] languages = txtSelectedSubtitleLanguages.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string lanName in languages)
                {
                    if (lanName.Trim().Length == 0) continue;
                    if (lanName.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += lanName;
                }
            }

            if (IsParameterYear)
            {

                if (txtSelectedYears.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] years = txtSelectedYears.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string year in years)
                {
                    if (year.Trim().Length == 0) continue;
                    if (year.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += year;
                }
            }

            if (IsParameterSeason)
            {

                if (txtSelectedSeasons.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] seasons = txtSelectedSeasons.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string season in seasons)
                {
                    if (season.Trim().Length == 0) continue;
                    if (season.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += season;
                }
            }

            if (IsParameterAnimeType)
            {


                if (txtSelectedAnimeTypes.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                // validate
                string[] aTypes = txtSelectedAnimeTypes.Text.Trim().Split(',');
                groupFilterCondition.ConditionParameter = "";
                foreach (string aType in aTypes)
                {
                    if (aType.Trim().Length == 0) continue;
                    if (aType.Trim() == ", ") continue;

                    if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ", ";
                    groupFilterCondition.ConditionParameter += aType;
                }
            }

            if (IsParameterAnimeGroup)
            {
                if (lbGroups.SelectedItem == null)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_GroupSelectionRequired, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    lbGroups.Focus();
                    return;
                }
                VM_AnimeGroup_User grp = lbGroups.SelectedItem as VM_AnimeGroup_User;
                groupFilterCondition.ConditionParameter = grp.AnimeGroupID.ToString();
            }

            if (IsParameterText)
            {
                if (txtParameter.Text.Trim().Length == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_EnterValue, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtParameter.Focus();
                    return;
                }
                groupFilterCondition.ConditionParameter = txtParameter.Text.Trim();
            }

            DialogResult = true;
            Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void EvaluateConditionsAndOperators()
        {
            if (cboConditionType.SelectedItem == null || cboConditionOperator.SelectedItem == null) return;

            GroupFilterConditionType conditionType = cboConditionType.SelectedItem.ToString().GetEnumForText_ConditionType();
            GroupFilterOperator opType = cboConditionOperator.SelectedItem.ToString().GetEnumForText_Operator();

            IsParameterDate = false;
            IsParameterAnimeGroup = false;
            IsParameterAnimeType = false;
            IsParameterText = false;
            IsParameterInNotIn = false;
            IsParameterTag = false;
            IsParameterCustomTag = false;
            IsParameterRating = false;
            IsParameterLastXDays = false;
            IsParameterVideoQuality = false;
            IsParameterAudioLanguage = false;
            IsParameterSubtitleLanguage = false;
            IsParameterInteger = false;

            switch (conditionType)
            {
                case GroupFilterConditionType.AirDate:
                case GroupFilterConditionType.SeriesCreatedDate:
                case GroupFilterConditionType.EpisodeAddedDate:
                case GroupFilterConditionType.EpisodeWatchedDate:
                case GroupFilterConditionType.LatestEpisodeAirDate:
                    if (opType == GroupFilterOperator.LastXDays)
                    {
                        IsParameterLastXDays = true;
                        IsParameterText = true;
                    }
                    else
                        IsParameterDate = true;
                    break;

                case GroupFilterConditionType.AnimeGroup:
                    IsParameterAnimeGroup = true;
                    break;

                case GroupFilterConditionType.AnimeType:
                    IsParameterAnimeType = true;
                    break;

                case GroupFilterConditionType.Tag:
                    IsParameterInNotIn = true;
                    IsParameterTag = true;
                    break;

                case GroupFilterConditionType.CustomTags:
                    IsParameterInNotIn = true;
                    IsParameterCustomTag = true;
                    break;

                case GroupFilterConditionType.AudioLanguage:
                    IsParameterInNotIn = true;
                    IsParameterAudioLanguage = true;
                    break;

                case GroupFilterConditionType.SubtitleLanguage:
                    IsParameterInNotIn = true;
                    IsParameterSubtitleLanguage = true;
                    break;

                case GroupFilterConditionType.Year:
                    IsParameterInNotIn = true;
                    IsParameterYear = true;
                    break;

                case GroupFilterConditionType.Season:
                    IsParameterInNotIn = true;
                    IsParameterSeason = true;
                    break;

                case GroupFilterConditionType.VideoQuality:
                    IsParameterInNotIn = true;
                    IsParameterVideoQuality = true;
                    break;

                case GroupFilterConditionType.AniDBRating:
                case GroupFilterConditionType.UserRating:
                    IsParameterText = true;
                    IsParameterRating = true;
                    break;

                case GroupFilterConditionType.EpisodeCount:
                    IsParameterText = true;
                    IsParameterInteger = true;
                    break;

            }
        }

        void cboConditionOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EvaluateConditionsAndOperators();
        }

        void cboConditionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupFilterConditionType conditionType = cboConditionType.SelectedItem.ToString().GetEnumForText_ConditionType();

            cboConditionOperator.Items.Clear();
            foreach (string op in conditionType.GetAllowedOperators())
                cboConditionOperator.Items.Add(op);

            cboConditionOperator.SelectedIndex = 0;

            EvaluateConditionsAndOperators();
        }

        private void PopulateTags()
        {
            AllTagNames = new ObservableCollection<string>();

            ViewTagNames = CollectionViewSource.GetDefaultView(AllTagNames);
            List<string> tagsRaw = VM_ShokoServer.Instance.ShokoServices.GetAllTagNames();

            foreach (string tag in tagsRaw)
                AllTagNames.Add(tag);

            ViewTagNames.Filter = TagFilter;
        }

        private void PopulateCustomTags()
        {
            AllCustomTagNames = new ObservableCollection<string>();

            ViewCustomTagNames = CollectionViewSource.GetDefaultView(AllCustomTagNames);

            foreach (CustomTag tag in VM_ShokoServer.Instance.AllCustomTags.OrderBy(a => a.TagName.ToLower(CultureInfo.InvariantCulture)))
                AllCustomTagNames.Add(tag.TagName);


            ViewCustomTagNames.Filter = CustomTagFilter;
            ViewCustomTagNames.Refresh();
        }

        private void PopulateYears()
        {
            AllYears = new ObservableCollection<string>();

            ViewYears = CollectionViewSource.GetDefaultView(AllYears);
            List<string> years = VM_ShokoServer.Instance.ShokoServices.GetAllYears();

            foreach (string year in years)
                AllYears.Add(year);
        }

        private void PopulateSeasons()
        {
            AllSeasons = new ObservableCollection<string>();

            ViewSeasons = CollectionViewSource.GetDefaultView(AllSeasons);
            List<string> seasons = VM_ShokoServer.Instance.ShokoServices.GetAllSeasons();

            foreach (string season in seasons)
                AllSeasons.Add(season);
        }

        private void PopulateVideoQuality()
        {
            AllVideoQuality = new ObservableCollection<string>();

            List<string> vidsRaw = VM_ShokoServer.Instance.ShokoServices.GetAllUniqueVideoQuality();
            vidsRaw.Sort();

            foreach (string vidq in vidsRaw)
                AllVideoQuality.Add(vidq);
        }

        private void PopulateLanguages()
        {
            AllAudioLanguages = new ObservableCollection<string>();

            List<string> audioRaw = VM_ShokoServer.Instance.ShokoServices.GetAllUniqueAudioLanguages();
            audioRaw.Sort();

            foreach (string aud in audioRaw)
                AllAudioLanguages.Add(aud);

            AllSubtitleLanguages = new ObservableCollection<string>();

            List<string> subRaw = VM_ShokoServer.Instance.ShokoServices.GetAllUniqueSubtitleLanguages();
            subRaw.Sort();

            foreach (string sub in subRaw)
                AllSubtitleLanguages.Add(sub);
        }

        private void PopulateAnimeTypes()
        {
            AllAnimeTypes = new ObservableCollection<string>();
            AllAnimeTypes.Add(Commons.Properties.Resources.AnimeType_Movie);
            AllAnimeTypes.Add(Commons.Properties.Resources.AnimeType_Other);
            AllAnimeTypes.Add(Commons.Properties.Resources.AnimeType_OVA);
            AllAnimeTypes.Add(Commons.Properties.Resources.AnimeType_TVSeries);
            AllAnimeTypes.Add(Commons.Properties.Resources.AnimeType_TVSpecial);
            AllAnimeTypes.Add(Commons.Properties.Resources.AnimeType_Web);
        }

        private void PopulateAnimeGroups()
        {
            AllGroups = new ObservableCollection<VM_AnimeGroup_User>();

            ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
            ViewGroups.SortDescriptions.Add(GroupFilterSorting.SortName.GetSortDescription(GroupFilterSortDirection.Asc));

            List<VM_AnimeGroup_User> grpsRaw = VM_ShokoServer.Instance.ShokoServices.GetAllGroups(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeGroup_User>();

            foreach (VM_AnimeGroup_User grpNew in grpsRaw)
            {
                AllGroups.Add(grpNew);
            }

            ViewGroups.Filter = GroupSearchFilter;
        }

        public void Init(VM_GroupFilter gf, VM_GroupFilterCondition gfc)
        {
            groupFilter = gf;
            groupFilterCondition = gfc;


            try
            {
                cboConditionType.Items.Clear();
                foreach (string cond in Commons.Extensions.Models.GetAllConditionTypes())
                    cboConditionType.Items.Add(cond);

                PopulateAnimeGroups();
                PopulateTags();
                PopulateCustomTags();
                PopulateVideoQuality();
                PopulateAnimeTypes();
                PopulateLanguages();
                PopulateYears();
                PopulateSeasons();

                // find the right condition
                int idx = 0;
                for (int i = 0; i < cboConditionType.Items.Count; i++)
                {
                    GroupFilterConditionType conditionTypeTemp = cboConditionType.Items[i].ToString().GetEnumForText_ConditionType();
                    if (conditionTypeTemp == gfc.ConditionTypeEnum)
                    {
                        idx = i;
                        break;
                    }
                }
                cboConditionType.SelectedIndex = idx;
                GroupFilterConditionType conditionType = cboConditionType.SelectedItem.ToString().GetEnumForText_ConditionType();

                cboConditionOperator.Items.Clear();
                foreach (string op in conditionType.GetAllowedOperators())
                    cboConditionOperator.Items.Add(op);

                cboConditionType.SelectionChanged += cboConditionType_SelectionChanged;
                cboConditionOperator.SelectionChanged += cboConditionOperator_SelectionChanged;

                // find the right operator
                idx = 0;
                for (int i = 0; i < cboConditionOperator.Items.Count; i++)
                {
                    GroupFilterOperator opTypeTemp = cboConditionOperator.Items[i].ToString().GetEnumForText_Operator();
                    if (opTypeTemp == gfc.ConditionOperatorEnum)
                    {
                        idx = i;
                        break;
                    }
                }
                cboConditionOperator.SelectedIndex = idx;
                GroupFilterOperator opType = cboConditionOperator.Items[idx].ToString().GetEnumForText_Operator();

                // display the selected filter value
                switch (conditionType)
                {
                    case GroupFilterConditionType.AirDate:
                    case GroupFilterConditionType.SeriesCreatedDate:
                    case GroupFilterConditionType.EpisodeAddedDate:
                    case GroupFilterConditionType.EpisodeWatchedDate:
                    case GroupFilterConditionType.LatestEpisodeAirDate:

                        if (opType == GroupFilterOperator.LastXDays)
                            txtParameter.Text = gfc.ConditionParameter;
                        else
                        {
                            DateTime airDate = gfc.ConditionParameter.GetDateFromString();
                            dpDate.SelectedDate = airDate;
                        }
                        break;

                    case GroupFilterConditionType.AnimeGroup:

                        // don't display anything
                        break;

                    case GroupFilterConditionType.AnimeType:
                    case GroupFilterConditionType.Tag:
                    case GroupFilterConditionType.CustomTags:

                    case GroupFilterConditionType.VideoQuality:
                    case GroupFilterConditionType.AniDBRating:
                    case GroupFilterConditionType.UserRating:
                    case GroupFilterConditionType.AudioLanguage:
                    case GroupFilterConditionType.SubtitleLanguage:
                    case GroupFilterConditionType.Year:
                    case GroupFilterConditionType.Season:
                        txtParameter.Text = gfc.ConditionParameter;
                        break;
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        private bool GroupSearchFilter(object obj)
        {
            VM_AnimeGroup_User grpvm = obj as VM_AnimeGroup_User;
            if (grpvm == null) return true;

            return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text.Replace("'", "`"));
        }

        private bool TagFilter(object obj)
        {
            string tagName = obj as string;
            if (tagName == null) return true;

            int index = tagName.IndexOf(txtTagSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase);
            if (index > -1) return true;
            return false;
        }

        private bool CustomTagFilter(object obj)
        {
            string tagName = obj as string;
            if (tagName == null) return true;


            int index = tagName.IndexOf(txtCustomTagSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase);
            if (index > -1) return true;
            return false;
        }

        void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewGroups.Refresh();
        }

        void btnClearGroupSearch_Click(object sender, RoutedEventArgs e)
        {
            txtGroupSearch.Text = "";
        }

        void btnClearTagSearch_Click(object sender, RoutedEventArgs e)
        {
            txtTagSearch.Text = "";
        }

        void txtTagSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewTagNames.Refresh();
        }

        void btnClearCustomTagSearch_Click(object sender, RoutedEventArgs e)
        {
            txtCustomTagSearch.Text = "";
        }

        void txtCustomTagSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewCustomTagNames.Refresh();
        }

        void lbSubtitleLanguages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            object obj = lbSubtitleLanguages.SelectedItem;
            if (obj == null) return;

            string lanName = obj.ToString();
            List<string> currentList = txtSelectedSubtitleLanguages.Text.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // add to the selected list
            if (currentList.Contains(lanName))
            {
                currentList.Remove(lanName);
            }
            else
            {
                currentList.Add(lanName);
            }

            txtSelectedSubtitleLanguages.Text = string.Join(", ", currentList);
        }

        void lbAudioLanguages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            object obj = lbAudioLanguages.SelectedItem;
            if (obj == null) return;

            string lanName = obj.ToString();
            List<string> currentList = txtSelectedAudioLanguages.Text.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // add to the selected list
            if (currentList.Contains(lanName))
            {
                currentList.Remove(lanName);
            }
            else
            {
                currentList.Add(lanName);
            }

            txtSelectedAudioLanguages.Text = string.Join(", ", currentList);
        }

        void lbVideoQuality_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbVideoQuality.SelectedItem;
            if (obj == null) return;

            string vidQual = obj.ToString();
            List<string> currentList = txtSelectedVideoQuality.Text.Trim().Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries).ToList();

            // add to the selected list
            if (currentList.Contains(vidQual))
            {
                currentList.Remove(vidQual);
            }
            else
            {
                currentList.Add(vidQual);
            }

            txtSelectedVideoQuality.Text = string.Join(", ", currentList);
        }

        void lbAnimeTypes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbAnimeTypes.SelectedItem;
            if (obj == null) return;

            string aType = obj.ToString();
            List<string> currentList = txtSelectedAnimeTypes.Text.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // add to the selected list
            if (currentList.Contains(aType))
            {
                currentList.Remove(aType);
            }
            else
            {
                currentList.Add(aType);
            }

            txtSelectedAnimeTypes.Text = string.Join(", ", currentList);
        }

        void lbTags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbTags.SelectedItem;
            if (obj == null) return;

            string catName = obj.ToString();
            List<string> currentList = txtSelectedTags.Text.Trim().Split(new[] { ','}, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();

            // add to the selected list
            if (currentList.Contains(catName))
            {
                currentList.Remove(catName);
            }
            else
            {
                currentList.Add(catName);
            }

            txtSelectedTags.Text = string.Join(", ", currentList);
        }

        void lbCustomTags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbCustomTags.SelectedItem;
            if (obj == null) return;

            string tagName = obj.ToString();
            List<string> currentList = txtSelectedCustomTags.Text.Trim().Split(new[] { ','}, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();

            // add to the selected list
            if (currentList.Contains(tagName))
            {
                currentList.Remove(tagName);
            }
            else
            {
                currentList.Add(tagName);
            }

            txtSelectedCustomTags.Text = string.Join(", ", currentList);
        }

        void lbYears_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbYears.SelectedItem;
            if (obj == null) return;

            string year = obj.ToString();
            List<string> currentList = txtSelectedYears.Text.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // add to the selected list
            if (currentList.Contains(year))
            {
                currentList.Remove(year);
            }
            else
            {
                currentList.Add(year);
            }

            txtSelectedYears.Text = string.Join(", ", currentList);
        }

        void lbSeasons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbSeasons.SelectedItem;
            if (obj == null) return;

            string season = obj.ToString();
            List<string> currentList = txtSelectedSeasons.Text.Trim().Split(new[] { ','}, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();

            // add to the selected list
            if (currentList.Contains(season))
            {
                currentList.Remove(season);
            }
            else
            {
                currentList.Add(season);
            }

            txtSelectedSeasons.Text = string.Join(", ", currentList);
        }
    }
}
