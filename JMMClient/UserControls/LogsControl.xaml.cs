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
using JMMClient.ViewModel;
using System.ComponentModel;
using NLog;
using System.IO;
using System.Diagnostics;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for LogsControl.xaml
	/// </summary>
	public partial class LogsControl : UserControl
	{
		public ObservableCollection<LogMessageVM> AllLogs = null;
		public ListCollectionView ViewLogs { get; set; }

		private static Logger logger = LogManager.GetCurrentClassLogger();

		BackgroundWorker workerFiles = new BackgroundWorker();

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
			typeof(bool), typeof(LogsControl), new UIPropertyMetadata(false, null));

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
			typeof(bool), typeof(LogsControl), new UIPropertyMetadata(true, null));

		public bool IsNotLoading
		{
			get { return (bool)GetValue(IsNotLoadingProperty); }
			set { SetValue(IsNotLoadingProperty, value); }
		}

		public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
			typeof(string), typeof(LogsControl), new UIPropertyMetadata("", null));

		public string StatusMessage
		{
			get { return (string)GetValue(StatusMessageProperty); }
			set { SetValue(StatusMessageProperty, value); }
		}

		public LogsControl()
		{
			InitializeComponent();

			AllLogs = new ObservableCollection<LogMessageVM>();
			ViewLogs = new ListCollectionView(AllLogs);
			ViewLogs.Filter = LogSearchFilter;

			cboTypes.Items.Add("All");
			cboTypes.Items.Add(Constants.DBLogType.APIAniDBHTTP);
			cboTypes.Items.Add(Constants.DBLogType.APIAniDBUDP);
			cboTypes.Items.Add(Constants.DBLogType.APIAzureHTTP);
			cboTypes.SelectedIndex = 0;

			workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
			workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			btnExport.Click += new RoutedEventHandler(btnExport_Click);

			txtLogSearch.TextChanged += new TextChangedEventHandler(txtFileSearch_TextChanged);
		}

		private bool LogSearchFilter(object obj)
		{
			LogMessageVM log = obj as LogMessageVM;
			if (log == null) return true;

			int index = log.LogContent.IndexOf(txtLogSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;
			return false;
		}

		void txtFileSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewLogs.Refresh();
		}

		void btnExport_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				IsLoading = true;
				btnRefresh.IsEnabled = false;
				btnExport.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				StatusMessage = "Exporting...";

				string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				string logName = System.IO.Path.Combine(appPath, "LogMessages.txt");

				string export = "";
				foreach (LogMessageVM log in ViewLogs)
				{
					string msg = string.Format("{0} - {1} - {2}", log.LogDate, log.LogType, log.LogContent);
					export += msg;
					export += Environment.NewLine;
				}

				StreamWriter Tex = new StreamWriter(logName);
				Tex.Write(export);
				Tex.Flush();
				Tex.Close();

				Process.Start(logName);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			IsLoading = false;
			btnRefresh.IsEnabled = true;
			btnExport.IsEnabled = true;
			this.Cursor = Cursors.Arrow;

			StatusMessage = "";
		}

		void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				List<LogMessageVM> logs = e.Result as List<LogMessageVM>;
				foreach (LogMessageVM rating in logs)
					AllLogs.Add(rating);
				ViewLogs.Refresh();

				IsLoading = false;
				btnRefresh.IsEnabled = true;
				this.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void workerFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				string logType = e.Argument as string;
				if (logType.Equals("All", StringComparison.InvariantCultureIgnoreCase)) logType = "";

				List<JMMServerBinary.Contract_LogMessage> rawLogs = JMMServerVM.Instance.clientBinaryHTTP.GetLogMessages(logType);

				List<LogMessageVM> logs = new List<LogMessageVM>();
				foreach (JMMServerBinary.Contract_LogMessage contract in rawLogs)
				{
					LogMessageVM log = new LogMessageVM(contract);
					logs.Add(log);
				}

				e.Result = logs;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshData();
		}

		private void RefreshData()
		{
			if (workerFiles.IsBusy) return;

			IsLoading = true;
			btnRefresh.IsEnabled = false;
			AllLogs.Clear();

			StatusMessage = "Loading...";

			workerFiles.RunWorkerAsync(cboTypes.SelectedItem.ToString());
		}
	}
}
