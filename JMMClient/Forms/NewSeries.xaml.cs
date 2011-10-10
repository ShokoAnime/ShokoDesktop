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
using System.ComponentModel;
using System.Collections.ObjectModel;
using JMMClient.ViewModel;
using System.Diagnostics;
using System.IO;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for NewSeries.xaml
	/// </summary>
	public partial class NewSeries : Window
	{
		public AnimeSeriesVM AnimeSeries { get; set; }
		public AniDB_AnimeVM SelectedAnime { get; set; }

		public ICollectionView ViewGroups { get; set; }
		public ObservableCollection<AnimeGroupVM> AllGroups { get; set; }

		public ICollectionView ViewAnime { get; set; }
		public ObservableCollection<AniDB_AnimeVM> AllAnime { get; set; }

		public static readonly DependencyProperty IsNewGroupProperty = DependencyProperty.Register("IsNewGroup",
			typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingGroupProperty = DependencyProperty.Register("IsExistingGroup",
			typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsNewAnimeProperty = DependencyProperty.Register("IsNewAnime",
			typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingAnimeProperty = DependencyProperty.Register("IsExistingAnime",
			typeof(bool), typeof(NewSeries), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsAnimeNotPopulatedProperty = DependencyProperty.Register("IsAnimeNotPopulated",
			typeof(bool), typeof(NewSeries), new UIPropertyMetadata(true, null));

		public static readonly DependencyProperty IsAnimePopulatedProperty = DependencyProperty.Register("IsAnimePopulated",
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

		public bool IsNewAnime
		{
			get { return (bool)GetValue(IsNewAnimeProperty); }
			set { SetValue(IsNewAnimeProperty, value); }
		}

		public bool IsExistingAnime
		{
			get { return (bool)GetValue(IsExistingAnimeProperty); }
			set { SetValue(IsExistingAnimeProperty, value); }
		}

		public bool IsAnimeNotPopulated
		{
			get { return (bool)GetValue(IsAnimeNotPopulatedProperty); }
			set { SetValue(IsAnimeNotPopulatedProperty, value); }
		}

		public bool IsAnimePopulated
		{
			get { return (bool)GetValue(IsAnimePopulatedProperty); }
			set { SetValue(IsAnimePopulatedProperty, value); }
		}

		public bool IsAnimeDisplayed
		{
			get { return (bool)GetValue(IsAnimeDisplayedProperty); }
			set { SetValue(IsAnimeDisplayedProperty, value); }
		}

		public NewSeries()
		{
			InitializeComponent();

			AnimeSeries = null;

			txtGroupSearch.TextChanged +=new TextChangedEventHandler(txtGroupSearch_TextChanged);
			txtAnimeSearch.TextChanged += new TextChangedEventHandler(txtAnimeSearch_TextChanged);

			btnClearGroupSearch.Click += new RoutedEventHandler(btnClearGroupSearch_Click);
			btnClearAnimeSearch.Click += new RoutedEventHandler(btnClearAnimeSearch_Click);

			rbGroupExisting.Checked += new RoutedEventHandler(rbGroupExisting_Checked);
			rbGroupNew.Checked += new RoutedEventHandler(rbGroupNew_Checked);
			rbAnimeExisting.Checked += new RoutedEventHandler(rbAnimeExisting_Checked);
			rbAnimeNew.Checked += new RoutedEventHandler(rbAnimeNew_Checked);
			hlURL.Click += new RoutedEventHandler(hlURL_Click);

			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
			btnConfirm.Click += new RoutedEventHandler(btnConfirm_Click);

			lbAnime.SelectionChanged += new SelectionChangedEventHandler(lbAnime_SelectionChanged);
		}

		void lbAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			IsAnimeDisplayed = false;
			AniDB_AnimeVM anime = lbAnime.SelectedItem as AniDB_AnimeVM;
			if (anime == null) return;

			SetAnimeDisplay(anime);
		}

		void btnConfirm_Click(object sender, RoutedEventArgs e)
		{
			//AnimeGroupVM grp = null;
			int animeID = 0;
			int? groupID = null;

			try
			{
				if (IsExistingGroup)
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
						groupID = grp.AnimeGroupID.Value;
					}
				}

				if (IsNewGroup)
				{
					if (txtGroupName.Text.Trim().Length == 0)
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_GroupNameRequired, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						txtGroupName.Focus();
						return;
					}
				}

				if (IsExistingAnime)
				{
					if (lbAnime.SelectedItem == null && SelectedAnime == null)
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_AnimeSelectionRequired, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						lbAnime.Focus();
						return;
					}
					else
					{
						if (lbAnime.SelectedItem != null)
							SelectedAnime = lbAnime.SelectedItem as AniDB_AnimeVM;
					}
						
				}


				if (IsNewAnime)
				{
					int id = 0;
					int.TryParse(txtAnimeID.Text, out id);
					if (id <= 0)
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_AnimeIDRequired, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						txtAnimeID.Focus();
						return;
					}
					animeID = id;
				}

				if (SelectedAnime != null)
					animeID = SelectedAnime.AnimeID;


				JMMServerBinary.Contract_AnimeSeries_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.CreateSeriesFromAnime(animeID, groupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				if (response.ErrorMessage.Length > 0)
				{
					MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				AnimeSeries = new AnimeSeriesVM(response.AnimeSeries);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
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
			int id = 0;
			int.TryParse(txtAnimeID.Text, out id);
			if (id <= 0) return;

			Uri uri = new Uri(string.Format(Constants.URLS.AniDB_Series, id));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void rbAnimeNew_Checked(object sender, RoutedEventArgs e)
		{
			EvaluateRadioButtons();
			txtAnimeID.Focus();
		}

		void rbAnimeExisting_Checked(object sender, RoutedEventArgs e)
		{
			EvaluateRadioButtons();
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
			IsNewGroup = rbGroupNew.IsChecked.Value;
			IsExistingGroup = rbGroupExisting.IsChecked.Value;
			IsNewAnime = rbAnimeNew.IsChecked.Value;
			IsExistingAnime = rbAnimeExisting.IsChecked.Value;
		}
		

		void btnClearAnimeSearch_Click(object sender, RoutedEventArgs e)
		{
			txtAnimeSearch.Text = "";
		}

		void btnClearGroupSearch_Click(object sender, RoutedEventArgs e)
		{
			txtGroupSearch.Text = "";
		}

		private void SetAnimeDisplay(AniDB_AnimeVM anime)
		{
			BitmapImage bmp = new BitmapImage();
			bmp.BeginInit();
			bmp.UriSource = new Uri(anime.DefaultPosterPath);
			bmp.DecodePixelHeight = 200;
			bmp.EndInit();

			imgPoster.Height = 200;
			imgPoster.Source = bmp;

			lnkAniDB.DisplayText = anime.AnimeID_Friendly;
			lnkAniDB.URL = anime.AniDB_SiteURL;

			txtDescription.Text = anime.Description;

			IsAnimeDisplayed = true;
		}

		private void SetSelectedAnime(AniDB_AnimeVM anime)
		{
			if (anime != null)
			{
				IsAnimeNotPopulated = false;
				IsAnimePopulated = true;
				SelectedAnime = anime;
				SetAnimeDisplay(SelectedAnime);
			}
			else
			{
				IsAnimeNotPopulated = true;
				IsAnimePopulated = false;
				SelectedAnime = null;
			}
		}

		public void Init(AniDB_AnimeVM defaultAnime, string defaultGroupName)
		{
			SetSelectedAnime(defaultAnime);

			rbAnimeExisting.IsChecked = true;
			rbGroupExisting.IsChecked = true;

			AllGroups = new ObservableCollection<AnimeGroupVM>();
			AllAnime = new ObservableCollection<AniDB_AnimeVM>();

			try
			{

				ViewGroups = CollectionViewSource.GetDefaultView(AllGroups);
				ViewGroups.SortDescriptions.Add(new SortDescription("SortName", ListSortDirection.Ascending));

				ViewAnime = CollectionViewSource.GetDefaultView(AllAnime);
				ViewAnime.SortDescriptions.Add(new SortDescription("MainTitle", ListSortDirection.Ascending));

				List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
				{
					AnimeGroupVM grpNew = new AnimeGroupVM(grp);
					AllGroups.Add(grpNew);
				}

				List<JMMServerBinary.Contract_AniDBAnime> animeRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();
				foreach (JMMServerBinary.Contract_AniDBAnime anime in animeRaw)
				{
					AniDB_AnimeVM animeNew = new AniDB_AnimeVM(anime);
					AllAnime.Add(animeNew);
				}

				ViewGroups.Filter = GroupSearchFilter;
				ViewAnime.Filter = AnimeSearchFilter;

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

		void txtAnimeSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewAnime.Refresh();
		}

		private bool GroupSearchFilter(object obj)
		{
			AnimeGroupVM grpvm = obj as AnimeGroupVM;
			if (grpvm == null) return true;

			return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
		}

		private bool AnimeSearchFilter(object obj)
		{
			AniDB_AnimeVM anime = obj as AniDB_AnimeVM;
			if (anime == null) return true;

			return GroupSearchFilterHelper.EvaluateAnimeTextSearch(anime, txtAnimeSearch.Text);
		}
	}
}
