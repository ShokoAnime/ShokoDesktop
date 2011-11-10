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
using JMMClient.Forms;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for SeriesWithoutFilesControl.xaml
	/// </summary>
	public partial class SeriesWithoutFilesControl : UserControl
	{
		BackgroundWorker workerFiles = new BackgroundWorker();
		public ICollectionView ViewSeries { get; set; }
		public ObservableCollection<AnimeSeriesVM> MissingSeriesCollection { get; set; }

		public static readonly DependencyProperty SeriesCountProperty = DependencyProperty.Register("SeriesCount",
			typeof(int), typeof(SeriesWithoutFilesControl), new UIPropertyMetadata(0, null));

		public int SeriesCount
		{
			get { return (int)GetValue(SeriesCountProperty); }
			set { SetValue(SeriesCountProperty, value); }
		}

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
			typeof(bool), typeof(SeriesWithoutFilesControl), new UIPropertyMetadata(false, null));

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
			typeof(bool), typeof(SeriesWithoutFilesControl), new UIPropertyMetadata(true, null));

		public bool IsNotLoading
		{
			get { return (bool)GetValue(IsNotLoadingProperty); }
			set { SetValue(IsNotLoadingProperty, value); }
		}

		public SeriesWithoutFilesControl()
		{
			InitializeComponent();

			IsLoading = false;

			MissingSeriesCollection = new ObservableCollection<AnimeSeriesVM>();
			ViewSeries = CollectionViewSource.GetDefaultView(MissingSeriesCollection);
			ViewSeries.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

			workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
			workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);
		}

		void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<JMMServerBinary.Contract_AnimeSeries> contracts = e.Result as List<JMMServerBinary.Contract_AnimeSeries>;
			foreach (JMMServerBinary.Contract_AnimeSeries mf in contracts)
				MissingSeriesCollection.Add(new AnimeSeriesVM(mf));
			SeriesCount = contracts.Count;
			btnRefresh.IsEnabled = true;
			IsLoading = false;
			this.Cursor = Cursors.Arrow;
		}

		void workerFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				List<JMMServerBinary.Contract_AnimeSeries> contractsTemp = JMMServerVM.Instance.clientBinaryHTTP.GetSeriesWithoutAnyFiles(
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				e.Result = contractsTemp;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			IsLoading = true;
			btnRefresh.IsEnabled = false;
			MissingSeriesCollection.Clear();
			SeriesCount = 0;

			workerFiles.RunWorkerAsync();
		}

		private void CommandBinding_DeleteSeries(object sender, ExecutedRoutedEventArgs e)
		{
			AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
			if (ser == null) return;

			Window parentWindow = Window.GetWindow(this);

			try
			{
				DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
				frm.Owner = parentWindow;
				bool? result = frm.ShowDialog();

				if (result.HasValue && result.Value == true)
				{
					//bool deleteFiles = frm.DeleteFiles;

					this.Cursor = Cursors.Wait;
					JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeSeries(ser.AnimeSeriesID.Value, frm.DeleteFiles, frm.DeleteGroups);

					MainListHelperVM.Instance.RefreshGroupsSeriesData();
					MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);

					MissingSeriesCollection.Remove(ser);
					ViewSeries.Refresh();

					this.Cursor = Cursors.Arrow;
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
	}
}
