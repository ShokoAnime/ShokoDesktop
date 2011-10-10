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
			set { SetValue(IsDataFinishedLoadingProperty, value); }
		}


		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ObservableCollection<GroupVideoQualityVM> VideoQualityRecords { get; set; }
		public ICollectionView ViewSummary { get; set; }

		public AnimeFileSummaryControl()
		{
			InitializeComponent();

			VideoQualityRecords = new ObservableCollection<GroupVideoQualityVM>();
			ViewSummary = CollectionViewSource.GetDefaultView(VideoQualityRecords);
			ViewSummary.SortDescriptions.Add(new SortDescription("Ranking", ListSortDirection.Descending));
			ViewSummary.SortDescriptions.Add(new SortDescription("FileCountNormal", ListSortDirection.Descending));

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeFileSummaryControl_DataContextChanged);
			//dataWorker.DoWork += new DoWorkEventHandler(dataWorker_DoWork);
			//dataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dataWorker_RunWorkerCompleted);
		}

		void AnimeFileSummaryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				if (this.DataContext == null)
				{
					VideoQualityRecords.Clear();
					return;
				}


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
				TotalFileCount = 0;

				AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
				if (anime == null) return;

				//dataWorker.RunWorkerAsync(anime.AnimeID);

				List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(anime.AnimeID);
				foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
				{
					GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
					TotalFileCount += vidQual.FileCountNormal + vidQual.FileCountSpecials;
					VideoQualityRecords.Add(vidQual);
				}
				IsDataLoading = false;
				IsDataFinishedLoading = true;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		/*void dataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			IsDataLoading = false;
			IsDataFinishedLoading = true;
			List<JMMServerBinary.Contract_GroupVideoQuality> summ = e.Result as List<JMMServerBinary.Contract_GroupVideoQuality>;
			foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
			{
				GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
				TotalFileCount += vidQual.FileCountNormal + vidQual.FileCountSpecials;
				VideoQualityRecords.Add(vidQual);
			}
		}

		void dataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(int.Parse(e.Argument.ToString()));
				e.Result = summ;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}*/

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
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}
