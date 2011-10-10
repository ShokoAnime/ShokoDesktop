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
using System.Windows.Shapes;
using JMMClient.ViewModel;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for GroupFilterConditionForm.xaml
	/// </summary>
	public partial class GroupFilterConditionForm : Window
	{
		public GroupFilterVM groupFilter = null;
		public GroupFilterConditionVM groupFilterCondition = null;
		public ICollectionView ViewGroups { get; set; }
		public ObservableCollection<AnimeGroupVM> AllGroups { get; set; }

		public ICollectionView ViewCategoryNames { get; set; }
		public ObservableCollection<string> AllCategoryNames { get; set; }

		public ObservableCollection<string> AllVideoQuality { get; set; }
		public ObservableCollection<string> AllAnimeTypes { get; set; }
		public ObservableCollection<string> AllAudioLanguages { get; set; }
		public ObservableCollection<string> AllSubtitleLanguages { get; set; }

		public static readonly DependencyProperty IsParameterDateProperty = DependencyProperty.Register("IsParameterDate",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterDate
		{
			get { return (bool)GetValue(IsParameterDateProperty); }
			set { SetValue(IsParameterDateProperty, value); }
		}

		public static readonly DependencyProperty IsParameterAnimeGroupProperty = DependencyProperty.Register("IsParameterAnimeGroup",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterAnimeGroup
		{
			get { return (bool)GetValue(IsParameterAnimeGroupProperty); }
			set { SetValue(IsParameterAnimeGroupProperty, value); }
		}

		public static readonly DependencyProperty IsParameterAnimeTypeProperty = DependencyProperty.Register("IsParameterAnimeType",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterAnimeType
		{
			get { return (bool)GetValue(IsParameterAnimeTypeProperty); }
			set { SetValue(IsParameterAnimeTypeProperty, value); }
		}

		public static readonly DependencyProperty IsParameterTextProperty = DependencyProperty.Register("IsParameterText",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterText
		{
			get { return (bool)GetValue(IsParameterTextProperty); }
			set { SetValue(IsParameterTextProperty, value); }
		}

		public static readonly DependencyProperty IsParameterInNotInProperty = DependencyProperty.Register("IsParameterInNotIn",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterInNotIn
		{
			get { return (bool)GetValue(IsParameterInNotInProperty); }
			set { SetValue(IsParameterInNotInProperty, value); }
		}

		public static readonly DependencyProperty IsParameterCategoryProperty = DependencyProperty.Register("IsParameterCategory",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterCategory
		{
			get { return (bool)GetValue(IsParameterCategoryProperty); }
			set { SetValue(IsParameterCategoryProperty, value); }
		}

		public static readonly DependencyProperty IsParameterVideoQualityProperty = DependencyProperty.Register("IsParameterVideoQuality",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterVideoQuality
		{
			get { return (bool)GetValue(IsParameterVideoQualityProperty); }
			set { SetValue(IsParameterVideoQualityProperty, value); }
		}

		public static readonly DependencyProperty IsParameterAudioLanguageProperty = DependencyProperty.Register("IsParameterAudioLanguage",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterAudioLanguage
		{
			get { return (bool)GetValue(IsParameterAudioLanguageProperty); }
			set { SetValue(IsParameterAudioLanguageProperty, value); }
		}

		public static readonly DependencyProperty IsParameterSubtitleLanguageProperty = DependencyProperty.Register("IsParameterSubtitleLanguage",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterSubtitleLanguage
		{
			get { return (bool)GetValue(IsParameterSubtitleLanguageProperty); }
			set { SetValue(IsParameterSubtitleLanguageProperty, value); }
		}


		public static readonly DependencyProperty IsParameterRatingProperty = DependencyProperty.Register("IsParameterRating",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterRating
		{
			get { return (bool)GetValue(IsParameterRatingProperty); }
			set { SetValue(IsParameterRatingProperty, value); }
		}

		public static readonly DependencyProperty IsParameterLastXDaysProperty = DependencyProperty.Register("IsParameterLastXDays",
			typeof(bool), typeof(GroupFilterConditionForm), new UIPropertyMetadata(false, null));

		public bool IsParameterLastXDays
		{
			get { return (bool)GetValue(IsParameterLastXDaysProperty); }
			set { SetValue(IsParameterLastXDaysProperty, value); }
		}

		public GroupFilterConditionForm()
		{
			InitializeComponent();

			txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);
			btnClearGroupSearch.Click += new RoutedEventHandler(btnClearGroupSearch_Click);

			txtCategorySearch.TextChanged += new TextChangedEventHandler(txtCategorySearch_TextChanged);
			btnClearCategorySearch.Click += new RoutedEventHandler(btnClearCategorySearch_Click);

			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
			btnConfirm.Click += new RoutedEventHandler(btnConfirm_Click);

			lbCategories.MouseDoubleClick += new MouseButtonEventHandler(lbCategories_MouseDoubleClick);
			lbVideoQuality.MouseDoubleClick += new MouseButtonEventHandler(lbVideoQuality_MouseDoubleClick);
			lbAnimeTypes.MouseDoubleClick += new MouseButtonEventHandler(lbAnimeTypes_MouseDoubleClick);
			lbAudioLanguages.MouseDoubleClick += new MouseButtonEventHandler(lbAudioLanguages_MouseDoubleClick);
			lbSubtitleLanguages.MouseDoubleClick += new MouseButtonEventHandler(lbSubtitleLanguages_MouseDoubleClick);
		}

		void btnConfirm_Click(object sender, RoutedEventArgs e)
		{
			// get the details from the form
			groupFilterCondition.ConditionType = (int)GroupFilterHelper.GetEnumForText_ConditionType(cboConditionType.SelectedItem.ToString());
			groupFilterCondition.ConditionOperator = (int)GroupFilterHelper.GetEnumForText_Operator(cboConditionOperator.SelectedItem.ToString());

			// get the parameter details
			if (IsParameterDate)
			{
				if (dpDate.SelectedDate == null)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_SelectDate, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					dpDate.Focus();
					return;
				}
				else
				{
					groupFilterCondition.ConditionParameter = GroupFilterHelper.GetDateAsString(dpDate.SelectedDate.Value);
				}

			}

			if (IsParameterRating)
			{
				if (txtParameter.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					decimal dRating = -1;
					decimal.TryParse(txtParameter.Text, out dRating);
					if (dRating <= 0 || dRating > 10)
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_RatingValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						txtParameter.Focus();
						return;
					}

					groupFilterCondition.ConditionParameter = txtParameter.Text.Trim();
				}

			}

			if (IsParameterLastXDays)
			{
				if (txtParameter.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					int days = -1;
					int.TryParse(txtParameter.Text, out days);
					if (days < 1 || days > int.MaxValue)
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_DaysValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						txtParameter.Focus();
						return;
					}

					groupFilterCondition.ConditionParameter = txtParameter.Text.Trim();
				}

			}


			if (IsParameterCategory)
			{
				if (txtSelectedCategories.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					// validate
					string[] cats = txtSelectedCategories.Text.Trim().Split(',');
					groupFilterCondition.ConditionParameter = "";
					foreach (string cat in cats)
					{
						if (cat.Trim().Length == 0) continue;
						if (cat.Trim() == ",") continue;

						if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ",";
						groupFilterCondition.ConditionParameter += cat;
					}

				}

			}

			if (IsParameterVideoQuality)
			{

				if (txtSelectedVideoQuality.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					// validate
					string[] vidQuals = txtSelectedVideoQuality.Text.Trim().Split(',');
					groupFilterCondition.ConditionParameter = "";
					foreach (string vidq in vidQuals)
					{
						if (vidq.Trim().Length == 0) continue;
						if (vidq.Trim() == ",") continue;

						if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ",";
						groupFilterCondition.ConditionParameter += vidq;
					}

				}

			}

			if (IsParameterAudioLanguage)
			{

				if (txtSelectedAudioLanguages.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					// validate
					string[] languages = txtSelectedAudioLanguages.Text.Trim().Split(',');
					groupFilterCondition.ConditionParameter = "";
					foreach (string lanName in languages)
					{
						if (lanName.Trim().Length == 0) continue;
						if (lanName.Trim() == ",") continue;

						if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ",";
						groupFilterCondition.ConditionParameter += lanName;
					}

				}

			}

			if (IsParameterSubtitleLanguage)
			{

				if (txtSelectedSubtitleLanguages.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					// validate
					string[] languages = txtSelectedSubtitleLanguages.Text.Trim().Split(',');
					groupFilterCondition.ConditionParameter = "";
					foreach (string lanName in languages)
					{
						if (lanName.Trim().Length == 0) continue;
						if (lanName.Trim() == ",") continue;

						if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ",";
						groupFilterCondition.ConditionParameter += lanName;
					}

				}

			}

			if (IsParameterAnimeType)
			{


				if (txtSelectedAnimeTypes.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					// validate
					string[] aTypes = txtSelectedAnimeTypes.Text.Trim().Split(',');
					groupFilterCondition.ConditionParameter = "";
					foreach (string aType in aTypes)
					{
						if (aType.Trim().Length == 0) continue;
						if (aType.Trim() == ",") continue;

						if (groupFilterCondition.ConditionParameter.Length > 0) groupFilterCondition.ConditionParameter += ",";
						groupFilterCondition.ConditionParameter += aType;
					}

				}

			}

			if (IsParameterAnimeGroup)
			{
				if (lbGroups.SelectedItem == null)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_GroupSelectionRequired, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					lbGroups.Focus();
					return;
				}
				else
				{
					AnimeGroupVM grp = lbGroups.SelectedItem as AnimeGroupVM;
					groupFilterCondition.ConditionParameter = grp.AnimeGroupID.Value.ToString();
				}
			}

			if (IsParameterText)
			{
				if (txtParameter.Text.Trim().Length == 0)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_EnterValue, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtParameter.Focus();
					return;
				}
				else
				{
					groupFilterCondition.ConditionParameter = txtParameter.Text.Trim();
				}

			}

			this.DialogResult = true;
			this.Close();
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void EvaluateConditionsAndOperators()
		{
			if (cboConditionType.SelectedItem == null || cboConditionOperator.SelectedItem == null) return;

			GroupFilterConditionType conditionType = GroupFilterHelper.GetEnumForText_ConditionType(cboConditionType.SelectedItem.ToString());
			GroupFilterOperator opType = GroupFilterHelper.GetEnumForText_Operator(cboConditionOperator.SelectedItem.ToString());

			IsParameterDate = false;
			IsParameterAnimeGroup = false;
			IsParameterAnimeType = false;
			IsParameterText = false;
			IsParameterInNotIn = false;
			IsParameterCategory = false;
			IsParameterRating = false;
			IsParameterLastXDays = false;
			IsParameterVideoQuality = false;
			IsParameterAudioLanguage = false;
			IsParameterSubtitleLanguage = false;

			switch (conditionType)
			{
				case GroupFilterConditionType.AirDate:
				case GroupFilterConditionType.SeriesCreatedDate:
				case GroupFilterConditionType.EpisodeAddedDate:
				case GroupFilterConditionType.EpisodeWatchedDate:
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
					IsParameterAnimeType = true; break;

				case GroupFilterConditionType.ReleaseGroup:
				case GroupFilterConditionType.Studio:
					IsParameterInNotIn = true; break;

				case GroupFilterConditionType.Category:
					IsParameterInNotIn = true;
					IsParameterCategory = true;
					break;

				case GroupFilterConditionType.AudioLanguage:
					IsParameterInNotIn = true;
					IsParameterAudioLanguage = true;
					break;

				case GroupFilterConditionType.SubtitleLanguage:
					IsParameterInNotIn = true;
					IsParameterSubtitleLanguage = true;
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

			}
		}

		void cboConditionOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			EvaluateConditionsAndOperators();
		}

		void cboConditionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GroupFilterConditionType conditionType = GroupFilterHelper.GetEnumForText_ConditionType(cboConditionType.SelectedItem.ToString());

			cboConditionOperator.Items.Clear();
			foreach (string op in GroupFilterHelper.GetAllowedOperators(conditionType))
				cboConditionOperator.Items.Add(op);

			cboConditionOperator.SelectedIndex = 0;

			EvaluateConditionsAndOperators();
		}

		private void PopulateCategories()
		{
			AllCategoryNames = new ObservableCollection<string>();

			ViewCategoryNames = CollectionViewSource.GetDefaultView(AllCategoryNames);
			List<string> catsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllCategoryNames();

			foreach (string cat in catsRaw)
				AllCategoryNames.Add(cat);

			ViewCategoryNames.Filter = CategoryFilter;
		}

		private void PopulateVideoQuality()
		{
			AllVideoQuality = new ObservableCollection<string>();

			List<string> vidsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllUniqueVideoQuality();
			vidsRaw.Sort();

			foreach (string vidq in vidsRaw)
				AllVideoQuality.Add(vidq);
		}

		private void PopulateLanguages()
		{
			AllAudioLanguages = new ObservableCollection<string>();

			List<string> audioRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllUniqueAudioLanguages();
			audioRaw.Sort();

			foreach (string aud in audioRaw)
				AllAudioLanguages.Add(aud);

			AllSubtitleLanguages = new ObservableCollection<string>();

			List<string> subRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllUniqueSubtitleLanguages();
			subRaw.Sort();

			foreach (string sub in subRaw)
				AllSubtitleLanguages.Add(sub);
		}

		private void PopulateAnimeTypes()
		{
			AllAnimeTypes = new ObservableCollection<string>();
			AllAnimeTypes.Add(JMMClient.Properties.Resources.AnimeType_Movie);
			AllAnimeTypes.Add(JMMClient.Properties.Resources.AnimeType_Other);
			AllAnimeTypes.Add(JMMClient.Properties.Resources.AnimeType_OVA);
			AllAnimeTypes.Add(JMMClient.Properties.Resources.AnimeType_TVSeries);
			AllAnimeTypes.Add(JMMClient.Properties.Resources.AnimeType_TVSpecial);
			AllAnimeTypes.Add(JMMClient.Properties.Resources.AnimeType_Web);
		}

		private void PopulateAnimeGroups()
		{
			AllGroups = new ObservableCollection<AnimeGroupVM>();

			ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
			ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

			List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

			foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
			{
				AnimeGroupVM grpNew = new AnimeGroupVM(grp);
				AllGroups.Add(grpNew);
			}

			ViewGroups.Filter = GroupSearchFilter;
		}

		public void Init(GroupFilterVM gf, GroupFilterConditionVM gfc)
		{
			groupFilter = gf;
			groupFilterCondition = gfc;

			
			try
			{
				cboConditionType.Items.Clear();
				foreach (string cond in GroupFilterHelper.GetAllConditionTypes())
					cboConditionType.Items.Add(cond);

				PopulateAnimeGroups();
				PopulateCategories();
				PopulateVideoQuality();
				PopulateAnimeTypes();
				PopulateLanguages();

				// find the right condition
				int idx = 0;
				for (int i = 0; i < cboConditionType.Items.Count; i++ )
				{
					GroupFilterConditionType conditionTypeTemp = GroupFilterHelper.GetEnumForText_ConditionType(cboConditionType.Items[i].ToString());
					if (conditionTypeTemp == gfc.ConditionTypeEnum)
					{
						idx = i;
						break;
					}
				}
				cboConditionType.SelectedIndex = idx;
				GroupFilterConditionType conditionType = GroupFilterHelper.GetEnumForText_ConditionType(cboConditionType.SelectedItem.ToString());

				cboConditionOperator.Items.Clear();
				foreach (string op in GroupFilterHelper.GetAllowedOperators(conditionType))
					cboConditionOperator.Items.Add(op);

				cboConditionType.SelectionChanged += new SelectionChangedEventHandler(cboConditionType_SelectionChanged);
				cboConditionOperator.SelectionChanged += new SelectionChangedEventHandler(cboConditionOperator_SelectionChanged);

				// find the right operator
				idx = 0;
				for (int i = 0; i < cboConditionOperator.Items.Count; i++)
				{
					GroupFilterOperator opTypeTemp = GroupFilterHelper.GetEnumForText_Operator(cboConditionOperator.Items[i].ToString());
					if (opTypeTemp == gfc.ConditionOperatorEnum)
					{
						idx = i;
						break;
					}
				}
				cboConditionOperator.SelectedIndex = idx;
				GroupFilterOperator opType = GroupFilterHelper.GetEnumForText_Operator(cboConditionOperator.Items[idx].ToString());

				// display the selected filter value
				switch (conditionType)
				{
					case GroupFilterConditionType.AirDate:
					case GroupFilterConditionType.SeriesCreatedDate:
					case GroupFilterConditionType.EpisodeAddedDate:
					case GroupFilterConditionType.EpisodeWatchedDate:

						if (opType == GroupFilterOperator.LastXDays)
							txtParameter.Text = gfc.ConditionParameter;
						else
						{
							DateTime airDate = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);
							dpDate.SelectedDate = airDate;
						}
						break;

					case GroupFilterConditionType.AnimeGroup:

						// don't display anything
						break;

					case GroupFilterConditionType.AnimeType:
					case GroupFilterConditionType.Category:
					case GroupFilterConditionType.ReleaseGroup:
					case GroupFilterConditionType.Studio:
					case GroupFilterConditionType.VideoQuality:
					case GroupFilterConditionType.AniDBRating:
					case GroupFilterConditionType.UserRating:
					case GroupFilterConditionType.AudioLanguage:
					case GroupFilterConditionType.SubtitleLanguage:
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
			AnimeGroupVM grpvm = obj as AnimeGroupVM;
			if (grpvm == null) return true;
			

			return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
		}

		private bool CategoryFilter(object obj)
		{
			string catName = obj as string;
			if (catName == null) return true;

			int index = catName.IndexOf(txtCategorySearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase);
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

		void btnClearCategorySearch_Click(object sender, RoutedEventArgs e)
		{
			txtCategorySearch.Text = "";
		}

		void txtCategorySearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewCategoryNames.Refresh();
		}

		void lbSubtitleLanguages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

			object obj = lbSubtitleLanguages.SelectedItem;
			if (obj == null) return;

			string lanName = obj.ToString();
			string currentList = txtSelectedSubtitleLanguages.Text.Trim();

			// add to the selected list
			int index = currentList.IndexOf(lanName, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += lanName;

			txtSelectedSubtitleLanguages.Text = currentList;
		}

		void lbAudioLanguages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

			object obj = lbAudioLanguages.SelectedItem;
			if (obj == null) return;

			string lanName = obj.ToString();
			string currentList = txtSelectedAudioLanguages.Text.Trim();

			// add to the selected list
			int index = currentList.IndexOf(lanName, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += lanName;

			txtSelectedAudioLanguages.Text = currentList;
		}

		void lbVideoQuality_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object obj = lbVideoQuality.SelectedItem;
			if (obj == null) return;

			string vidQual = obj.ToString();
			string currentList = txtSelectedVideoQuality.Text.Trim();

			// add to the selected list
			int index = currentList.IndexOf(vidQual, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += vidQual;

			txtSelectedVideoQuality.Text = currentList;
		}

		void lbAnimeTypes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object obj = lbAnimeTypes.SelectedItem;
			if (obj == null) return;

			string aType = obj.ToString();
			string currentList = txtSelectedAnimeTypes.Text.Trim();

			// add to the selected list
			int index = currentList.IndexOf(aType, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += aType;

			txtSelectedAnimeTypes.Text = currentList;
		}

		void lbCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object obj = lbCategories.SelectedItem;
			if (obj == null) return;

			string catName = obj.ToString();
			string currentList = txtSelectedCategories.Text.Trim();

			// add to the selected list
			int index = currentList.IndexOf(catName, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += catName;

			txtSelectedCategories.Text = currentList;
		}

		
	}
}
