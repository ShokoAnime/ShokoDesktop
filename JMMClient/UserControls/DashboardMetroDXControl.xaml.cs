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
using DevExpress.Xpf.LayoutControl;
using System.ComponentModel;
using NLog;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Core;
using System.Diagnostics;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DashboardMetroDXControl.xaml
	/// </summary>
	public partial class DashboardMetroDXControl : UserControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		BackgroundWorker refreshDataWorker = new BackgroundWorker();
		BackgroundWorker refreshContinueWatchingWorker = new BackgroundWorker();
		BackgroundWorker refreshRandomSeriesWorker = new BackgroundWorker();
		BackgroundWorker refreshActivityWorker = new BackgroundWorker();

		public static readonly DependencyProperty IsLoadingContinueWatchingProperty = DependencyProperty.Register("IsLoadingContinueWatching",
			typeof(bool), typeof(DashboardMetroDXControl), new UIPropertyMetadata(false, null));

		public bool IsLoadingContinueWatching
		{
			get { return (bool)GetValue(IsLoadingContinueWatchingProperty); }
			set { SetValue(IsLoadingContinueWatchingProperty, value); }
		}

		public static readonly DependencyProperty IsLoadingRandomSeriesProperty = DependencyProperty.Register("IsLoadingRandomSeries",
			typeof(bool), typeof(DashboardMetroDXControl), new UIPropertyMetadata(false, null));

		public bool IsLoadingRandomSeries
		{
			get { return (bool)GetValue(IsLoadingRandomSeriesProperty); }
			set { SetValue(IsLoadingRandomSeriesProperty, value); }
		}

		public static readonly DependencyProperty IsLoadingTraktActivityProperty = DependencyProperty.Register("IsLoadingTraktActivity",
			typeof(bool), typeof(DashboardMetroDXControl), new UIPropertyMetadata(false, null));

		public bool IsLoadingTraktActivity
		{
			get { return (bool)GetValue(IsLoadingTraktActivityProperty); }
			set { SetValue(IsLoadingTraktActivityProperty, value); }
		}

		public DashboardMetroDXControl()
		{
			InitializeComponent();

			refreshDataWorker.DoWork += new DoWorkEventHandler(refreshDataWorker_DoWork);
			refreshDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshDataWorker_RunWorkerCompleted);

			refreshContinueWatchingWorker.DoWork += new DoWorkEventHandler(refreshContinueWatchingWorker_DoWork);
			refreshContinueWatchingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshContinueWatchingWorker_RunWorkerCompleted);

			refreshRandomSeriesWorker.DoWork += new DoWorkEventHandler(refreshRandomSeriesWorker_DoWork);
			refreshRandomSeriesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshRandomSeriesWorker_RunWorkerCompleted);

			refreshActivityWorker.DoWork += new DoWorkEventHandler(refreshActivityWorker_DoWork);
			refreshActivityWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshActivityWorker_RunWorkerCompleted);

			this.Loaded += new RoutedEventHandler(DashboardMetroDXControl_Loaded);
			btnToggleDash.Click += new RoutedEventHandler(btnToggleDash_Click);

			btnContinueWatchingIncrease.Click += new RoutedEventHandler(btnContinueWatchingIncrease_Click);
			btnContinueWatchingReduce.Click += new RoutedEventHandler(btnContinueWatchingReduce_Click);

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			btnRefreshRandomSeries.Click += new RoutedEventHandler(btnRefreshRandomSeries_Click);
			btnRefreshActivity.Click += new RoutedEventHandler(btnRefreshActivity_Click);

			btnOptions.Click += new RoutedEventHandler(btnOptions_Click);

			cboImageType.Items.Clear();
			cboImageType.Items.Add("Fanart");
			cboImageType.Items.Add("Posters");

			if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
				cboImageType.SelectedIndex = 0;
			else
				cboImageType.SelectedIndex = 1;

			cboImageType.SelectionChanged += new SelectionChangedEventHandler(cboImageType_SelectionChanged);

			DashboardMetroVM.Instance.OnFinishedProcessEvent += new DashboardMetroVM.FinishedProcessHandler(Instance_OnFinishedProcessEvent);
		}

		

		

		

		void cboImageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cboImageType.SelectedIndex == 0)
				AppSettings.DashMetroImageType = DashboardMetroImageType.Fanart;
			else
				AppSettings.DashMetroImageType = DashboardMetroImageType.Posters;

			UserSettingsVM.Instance.DashMetro_Image_Height = UserSettingsVM.Instance.DashMetro_Image_Height;
			RefreshAllData();
		}

		

		

		void btnOptions_Click(object sender, RoutedEventArgs e)
		{
			DashboardMetroVM.Instance.IsBeingEdited = !DashboardMetroVM.Instance.IsBeingEdited;
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			IsLoadingContinueWatching = true;
			refreshContinueWatchingWorker.RunWorkerAsync(false);
		}

		void btnRefreshRandomSeries_Click(object sender, RoutedEventArgs e)
		{
			IsLoadingRandomSeries = true;
			refreshRandomSeriesWorker.RunWorkerAsync(false);
		}

		void btnRefreshActivity_Click(object sender, RoutedEventArgs e)
		{
			FrameworkElements obj = tileLayoutActivity.GetChildren(false);
			IsLoadingTraktActivity = true;
			refreshActivityWorker.RunWorkerAsync(false);
		}

		void btnContinueWatchingReduce_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.DashMetro_Image_Height = UserSettingsVM.Instance.DashMetro_Image_Height - 7;
		}

		void btnContinueWatchingIncrease_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.DashMetro_Image_Height = UserSettingsVM.Instance.DashMetro_Image_Height + 7;
		}

		void btnToggleDash_Click(object sender, RoutedEventArgs e)
		{
			MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
			mainwdw.ShowDashMetroView(MetroViews.MainNormal);
		}

		void DashboardMetroDXControl_Loaded(object sender, RoutedEventArgs e)
		{
			MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
			DashboardMetroVM.Instance.InitNavigator(mainwdw);
		}

		public void RefreshAllData()
		{
			IsLoadingContinueWatching = true;
			IsLoadingRandomSeries = true;
			IsLoadingTraktActivity = true;
			refreshDataWorker.RunWorkerAsync(true);
		}

		void refreshDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//bool refreshAll = (bool)e.Result;
			//IsLoadingContinueWatching = false;
			

			//if (refreshAll)
			//	refreshRandomSeriesWorker.RunWorkerAsync(true);
		}

		void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				DashboardMetroVM.Instance.RefreshAllData();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void Instance_OnFinishedProcessEvent(FinishedProcessEventArgs ev)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
			{
				switch (ev.ProcessType)
				{
					case DashboardMetroProcessType.ContinueWatching: IsLoadingContinueWatching = false; break;
					case DashboardMetroProcessType.RandomSeries: IsLoadingRandomSeries = false; break;
					case DashboardMetroProcessType.TraktActivity: IsLoadingTraktActivity = false; break;
				}
			});
		}

		void refreshContinueWatchingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			
		}

		void refreshContinueWatchingWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				DashboardMetroVM.Instance.RefreshContinueWatching();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void refreshRandomSeriesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//bool refreshAll = (bool)e.Result;
			//IsLoadingRandomSeries = false;

			//if (refreshAll)
			//	refreshActivityWorker.RunWorkerAsync(true);
		}

		void refreshRandomSeriesWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			//bool refreshAll = (bool)e.Argument;
			DashboardMetroVM.Instance.RefreshRandomSeries();
			//e.Result = refreshAll;
		}

		void refreshActivityWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
		}

		void refreshActivityWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			//bool refreshAll = (bool)e.Argument;
			DashboardMetroVM.Instance.RefreshTraktActivity();
			//e.Result = refreshAll;
		}

		private void tileLayoutControl1_DragOver(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.Move;
			e.Handled = true;
		}

		private void tileLayoutControl1_Drop(object sender, DragEventArgs e)
		{
			/*

			SomeItem item = (SomeItem)e.Data.GetData(typeof(SomeItem));
			TileLayoutControl tileLayoutControl = (TileLayoutControl)sender;
			((ObservableCollection<SomeItem>)tileLayoutControl.ItemsSource).Add(item);*/
		}

		private void tileLayoutTraktActivity_TileClick(object sender, TileClickEventArgs e)
		{
			try
			{
				Tile mytile = e.Tile;
				object item = mytile.DataContext as object;
				if (item == null) return;



				if (item.GetType() == typeof(TraktActivityTile))
				{
					TraktActivityTile tile = item as TraktActivityTile;
					Uri uri = new Uri(tile.URL);
					Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
				}

				if (item.GetType() == typeof(TraktShoutTile))
				{
					TraktShoutTile tile = item as TraktShoutTile;
					Uri uri = new Uri(tile.URL);
					Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
				}

				if (item.GetType() == typeof(Trakt_SignupVM))
				{
					MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
					mainwdw.tabControl1.SelectedIndex = MainWindow.TAB_MAIN_Settings;
					mainwdw.tabSettingsChild.SelectedIndex = MainWindow.TAB_Settings_TvDB;
				}


			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void tileLayoutContinueWatching_TileClick(object sender, TileClickEventArgs e)
		{
			Tile mytile = e.Tile;
			ContinueWatchingTile item = mytile.DataContext as ContinueWatchingTile;
			if (item == null) return;

			DashboardMetroVM.Instance.NavigateForward(MetroViews.ContinueWatching, item.AnimeSeries);
		}

		private void tileLayoutRandomSeries_TileClick(object sender, TileClickEventArgs e)
		{
			Tile mytile = e.Tile;
			RandomSeriesTile item = mytile.DataContext as RandomSeriesTile;

			DashboardMetroVM.Instance.NavigateForward(MetroViews.ContinueWatching, item.AnimeSeries);
		}
	}
}
