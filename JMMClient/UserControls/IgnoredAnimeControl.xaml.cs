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
	/// Interaction logic for IgnoredAnimeControl.xaml
	/// </summary>
	public partial class IgnoredAnimeControl : UserControl
	{
		BackgroundWorker workerAnime = new BackgroundWorker();
		public ICollectionView ViewAnime { get; set; }
		public ObservableCollection<IgnoreAnimeVM> MissingAnimeCollection { get; set; }

		public static readonly DependencyProperty AnimeCountProperty = DependencyProperty.Register("AnimeCount",
			typeof(int), typeof(IgnoredAnimeControl), new UIPropertyMetadata(0, null));

		public int AnimeCount
		{
			get { return (int)GetValue(AnimeCountProperty); }
			set { SetValue(AnimeCountProperty, value); }
		}

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
			typeof(bool), typeof(IgnoredAnimeControl), new UIPropertyMetadata(false, null));

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
			typeof(bool), typeof(IgnoredAnimeControl), new UIPropertyMetadata(true, null));

		public bool IsNotLoading
		{
			get { return (bool)GetValue(IsNotLoadingProperty); }
			set { SetValue(IsNotLoadingProperty, value); }
		}

		public IgnoredAnimeControl()
		{
			InitializeComponent();

			IsLoading = false;

			MissingAnimeCollection = new ObservableCollection<IgnoreAnimeVM>();
			ViewAnime = CollectionViewSource.GetDefaultView(MissingAnimeCollection);
			ViewAnime.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

			workerAnime.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
			workerAnime.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);
		}

		void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<JMMServerBinary.Contract_IgnoreAnime> contracts = e.Result as List<JMMServerBinary.Contract_IgnoreAnime>;
			foreach (JMMServerBinary.Contract_IgnoreAnime mf in contracts)
				MissingAnimeCollection.Add(new IgnoreAnimeVM(mf));
			AnimeCount = contracts.Count;
			btnRefresh.IsEnabled = true;
			IsLoading = false;
			this.Cursor = Cursors.Arrow;
		}

		void workerFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				List<JMMServerBinary.Contract_IgnoreAnime> contractsTemp = JMMServerVM.Instance.clientBinaryHTTP.GetIgnoredAnime(
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				e.Result = contractsTemp;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void RefreshData()
		{
			IsLoading = true;
			btnRefresh.IsEnabled = false;
			MissingAnimeCollection.Clear();
			AnimeCount = 0;

			workerAnime.RunWorkerAsync();
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshData();
		}

		private void CommandBinding_DeleteIgnoredAnime(object sender, ExecutedRoutedEventArgs e)
		{
			IgnoreAnimeVM ign = e.Parameter as IgnoreAnimeVM;
			if (ign == null) return;

			Window parentWindow = Window.GetWindow(this);

			try
			{
				if (MessageBox.Show("Are you sure you want to delete this?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					this.Cursor = Cursors.Wait;
					JMMServerVM.Instance.clientBinaryHTTP.RemoveIgnoreAnime(ign.IgnoreAnimeID);
					this.Cursor = Cursors.Arrow;

					RefreshData();
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
