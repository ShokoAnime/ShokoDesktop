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
using System.ComponentModel;
using System.Collections.ObjectModel;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DownloadRecommendationsControl.xaml
	/// </summary>
	public partial class DownloadRecommendationsControl : UserControl
	{
		public ICollectionView ViewRecs { get; set; }
		public ObservableCollection<RecommendationVM> Recs { get; set; }

		BackgroundWorker getMissingDataWorker = new BackgroundWorker();

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
			typeof(bool), typeof(DownloadRecommendationsControl), new UIPropertyMetadata(false, null));

		public bool IsLoading
		{
			get { return (bool)GetValue(IsLoadingProperty); }
			set
			{
				SetValue(IsLoadingProperty, value);
				IsNotLoading = !IsLoading;
			}
		}

		public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
			typeof(bool), typeof(DownloadRecommendationsControl), new UIPropertyMetadata(true, null));

		public bool IsNotLoading
		{
			get { return (bool)GetValue(IsNotLoadingProperty); }
			set { SetValue(IsNotLoadingProperty, value); }
		}


		public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
			typeof(string), typeof(DownloadRecommendationsControl), new UIPropertyMetadata("", null));

		public string StatusMessage
		{
			get { return (string)GetValue(StatusMessageProperty); }
			set { SetValue(StatusMessageProperty, value); }
		}

		public DownloadRecommendationsControl()
		{
			InitializeComponent();

			IsLoading = false;

			Recs = new ObservableCollection<RecommendationVM>();
			ViewRecs = CollectionViewSource.GetDefaultView(Recs);

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			btnGetRecDownloadMissingInfo.Click += new RoutedEventHandler(btnGetRecDownloadMissingInfo_Click);

			getMissingDataWorker.DoWork += new DoWorkEventHandler(getMissingDataWorker_DoWork);
			getMissingDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getMissingDataWorker_RunWorkerCompleted);
		}

		void btnGetRecDownloadMissingInfo_Click(object sender, RoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			IsLoading = true;
			this.IsEnabled = false;
			parentWindow.Cursor = Cursors.Wait;
			getMissingDataWorker.RunWorkerAsync();
		}

		void getMissingDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			RefreshData();

			parentWindow.Cursor = Cursors.Arrow;
			this.IsEnabled = true;
			IsLoading = false;
		}

		void getMissingDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				foreach (RecommendationVM rec in Recs)
				{
					if (rec.Recommended_AnimeInfoNotExists)
					{
						JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(rec.RecommendedAnimeID);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void RefreshData()
		{
			try
			{
				Window parentWindow = Window.GetWindow(this);
				parentWindow.Cursor = Cursors.Wait;

				Recs.Clear();

				List<JMMServerBinary.Contract_Recommendation> contracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetRecommendations(UserSettingsVM.Instance.DownloadsRecItems, JMMServerVM.Instance.CurrentUser.JMMUserID.Value,
					(int)RecommendationType.Download);

				foreach (JMMServerBinary.Contract_Recommendation contract in contracts)
				{
					RecommendationVM rec = new RecommendationVM();
					rec.Populate(contract);
					Recs.Add(rec);
				}

				ViewRecs.Refresh();

				parentWindow.Cursor = Cursors.Arrow;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshData();
		}
	}
}
