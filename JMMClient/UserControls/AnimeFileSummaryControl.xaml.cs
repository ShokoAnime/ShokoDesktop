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
using System.Collections.ObjectModel;
using NLog;
using JMMClient.ViewModel;
using System.ComponentModel;
using System.Threading;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for AnimeFileSummaryControl.xaml
	/// </summary>
	public partial class AnimeFileSummaryControl : UserControl
	{
		BackgroundWorker dataWorker = new BackgroundWorker();

		public static readonly DependencyProperty TotalFileCountProperty = DependencyProperty.Register("TotalFileCount",
			typeof(int), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(0, null));

		public int TotalFileCount
		{
			get { return (int)GetValue(TotalFileCountProperty); }
			set { SetValue(TotalFileCountProperty, value); }
		}

        public static readonly DependencyProperty TotalFileSizeProperty = DependencyProperty.Register("TotalFileSize",
            typeof(string), typeof(AnimeFileSummaryControl), new UIPropertyMetadata("", null));

        public string TotalFileSize
        {
            get { return (string)GetValue(TotalFileSizeProperty); }
            set { SetValue(TotalFileSizeProperty, value); }
        }

		public static readonly DependencyProperty IsDataLoadingProperty = DependencyProperty.Register("IsDataLoading",
			typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(true, null));

		public bool IsDataLoading
		{
			get { return (bool)GetValue(IsDataLoadingProperty); }
			set { SetValue(IsDataLoadingProperty, value); }
		}

		public static readonly DependencyProperty IsDataFinishedLoadingProperty = DependencyProperty.Register("IsDataFinishedLoading",
			typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(false, null));

		public bool IsDataFinishedLoading
		{
			get { return (bool)GetValue(IsDataFinishedLoadingProperty); }
			set 
			{ 
				SetValue(IsDataFinishedLoadingProperty, value);
				if (!value)
				{
					DisplayGroupSummary = false;
					DisplayGroupQualityDetails = false;
				}
				else
				{
					DisplayGroupSummary = IsGroupSummary;
					DisplayGroupQualityDetails = IsGroupQualityDetails;
				}
			}
		}

		public static readonly DependencyProperty IsGroupSummaryProperty = DependencyProperty.Register("IsGroupSummary",
			typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(false, null));

		public bool IsGroupSummary
		{
			get { return (bool)GetValue(IsGroupSummaryProperty); }
			set { SetValue(IsGroupSummaryProperty, value); }
		}

		public static readonly DependencyProperty IsGroupQualityDetailsProperty = DependencyProperty.Register("IsGroupQualityDetails",
			typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(true, null));

		public bool IsGroupQualityDetails
		{
			get { return (bool)GetValue(IsGroupQualityDetailsProperty); }
			set { SetValue(IsGroupQualityDetailsProperty, value); }
		}

		public static readonly DependencyProperty DisplayGroupSummaryProperty = DependencyProperty.Register("DisplayGroupSummary",
			typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(false, null));

		public bool DisplayGroupSummary
		{
			get { return (bool)GetValue(DisplayGroupSummaryProperty); }
			set { SetValue(DisplayGroupSummaryProperty, value); }
		}

		public static readonly DependencyProperty DisplayGroupQualityDetailsProperty = DependencyProperty.Register("DisplayGroupQualityDetails",
			typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(true, null));

		public bool DisplayGroupQualityDetails
		{
			get { return (bool)GetValue(DisplayGroupQualityDetailsProperty); }
			set { SetValue(DisplayGroupQualityDetailsProperty, value); }
		}


		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ObservableCollection<GroupVideoQualityVM> VideoQualityRecords { get; set; }
		public ICollectionView ViewSummary { get; set; }

		public ObservableCollection<GroupFileSummaryVM> GroupSummaryRecords { get; set; }
		public ICollectionView ViewGroupSummary { get; set; }

		public AnimeFileSummaryControl()
		{
			InitializeComponent();

			VideoQualityRecords = new ObservableCollection<GroupVideoQualityVM>();
			ViewSummary = CollectionViewSource.GetDefaultView(VideoQualityRecords);

			GroupSummaryRecords = new ObservableCollection<GroupFileSummaryVM>();
			ViewGroupSummary = CollectionViewSource.GetDefaultView(GroupSummaryRecords);

			ViewGroupSummary.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));

			cboSortGroupQual.Items.Clear();
			cboSortGroupQual.Items.Add("By Quality Ranking");
			cboSortGroupQual.Items.Add("By Release Group");
			cboSortGroupQual.SelectionChanged += new SelectionChangedEventHandler(cboSortGroupQual_SelectionChanged);

			cboFileSummaryType.Items.Clear();
			cboFileSummaryType.Items.Add("Quality/Release Group Details");
			cboFileSummaryType.Items.Add("Release Group Summary");
			cboFileSummaryType.SelectedIndex = 0;
			cboFileSummaryType.SelectionChanged += new SelectionChangedEventHandler(cboFileSummaryType_SelectionChanged);

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeFileSummaryControl_DataContextChanged);
			//dataWorker.DoWork += new DoWorkEventHandler(dataWorker_DoWork);
			//dataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dataWorker_RunWorkerCompleted);
		}

		void cboFileSummaryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cboFileSummaryType.SelectedIndex == 0)
			{
				IsGroupQualityDetails = true;
				IsGroupSummary = false;
				AppSettings.FileSummaryTypeDefault = 0;
			}

			if (cboFileSummaryType.SelectedIndex == 1)
			{
				IsGroupQualityDetails = false;
				IsGroupSummary = true;
				AppSettings.FileSummaryTypeDefault = 1;
			}

			RefreshRecords();
		}

		void cboSortGroupQual_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewSummary.SortDescriptions.Clear();

			if (cboSortGroupQual.SelectedIndex == 0)
			{
				ViewSummary.SortDescriptions.Add(new SortDescription("Ranking", ListSortDirection.Descending));
				ViewSummary.SortDescriptions.Add(new SortDescription("FileCountNormal", ListSortDirection.Descending));
				AppSettings.FileSummaryQualSortDefault = 0;
			}

			if (cboSortGroupQual.SelectedIndex == 1)
			{
				ViewSummary.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));
				ViewSummary.SortDescriptions.Add(new SortDescription("Ranking", ListSortDirection.Descending));
				AppSettings.FileSummaryQualSortDefault = 1;
			}
			ViewSummary.Refresh();
		}

		void AnimeFileSummaryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				if (this.DataContext == null)
				{
					VideoQualityRecords.Clear();
					GroupSummaryRecords.Clear();
					return;
				}

				bool changedCombo = false;

				if (cboSortGroupQual.SelectedIndex != AppSettings.FileSummaryQualSortDefault && cboSortGroupQual.SelectedIndex >= 0) changedCombo = true;
				if (cboFileSummaryType.SelectedIndex != AppSettings.FileSummaryTypeDefault && cboFileSummaryType.SelectedIndex >= 0) changedCombo = true;

				cboSortGroupQual.SelectedIndex = AppSettings.FileSummaryQualSortDefault;
				cboFileSummaryType.SelectedIndex = AppSettings.FileSummaryTypeDefault;

				if (!changedCombo)
					RefreshRecords();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void RefreshRecords()
		{
			try
			{
				IsDataLoading = true;
				IsDataFinishedLoading = false;

				VideoQualityRecords.Clear();
				GroupSummaryRecords.Clear();

				TotalFileCount = 0;
                double fileSize = 0;

				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				if (IsGroupQualityDetails)
				{
					List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(anime.AnimeID);
					foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
					{
						GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
						TotalFileCount += vidQual.FileCountNormal + vidQual.FileCountSpecials;
                        fileSize += vidQual.TotalFileSize;
						VideoQualityRecords.Add(vidQual);
					}
				}

				if (IsGroupSummary)
				{
					List<JMMServerBinary.Contract_GroupFileSummary> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupFileSummary(anime.AnimeID);
					foreach (JMMServerBinary.Contract_GroupFileSummary contract in summ)
					{
						GroupFileSummaryVM vidQual = new GroupFileSummaryVM(contract);
						TotalFileCount += vidQual.FileCountNormal + vidQual.FileCountSpecials;
                        fileSize += vidQual.TotalFileSize;
						GroupSummaryRecords.Add(vidQual);
					}
				}

                TotalFileSize = Utils.FormatFileSize(fileSize);


				IsDataLoading = false;
				IsDataFinishedLoading = true;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DeleteAllFiles(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				Window wdw = Window.GetWindow(this);
				if (obj.GetType() == typeof(GroupVideoQualityVM))
				{
					GroupVideoQualityVM gvq = (GroupVideoQualityVM)obj;

					this.Cursor = Cursors.Wait;
					DeleteFilesForm frm = new DeleteFilesForm();
					frm.Owner = wdw;
					frm.Init(anime.AnimeID, gvq);
					bool? result = frm.ShowDialog();
					if (result.Value)
					{
						// refresh
						RefreshRecords();
					}

					this.Cursor = Cursors.Arrow;
				}

				if (obj.GetType() == typeof(GroupFileSummaryVM))
				{
					GroupFileSummaryVM gfs = (GroupFileSummaryVM)obj;

					this.Cursor = Cursors.Wait;
					DeleteFilesForm frm = new DeleteFilesForm();
					frm.Owner = wdw;
					frm.Init(anime.AnimeID, gfs);
					bool? result = frm.ShowDialog();
					if (result.Value)
					{
						// refresh
						RefreshRecords();
					}

					this.Cursor = Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}
