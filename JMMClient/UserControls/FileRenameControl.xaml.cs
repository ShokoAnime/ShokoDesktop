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
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using JMMClient.ViewModel;
using System.Threading;
using JMMClient.Forms;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for FileRenameControl.xaml
	/// </summary>
	public partial class FileRenameControl : UserControl
	{
		public ICollectionView ViewFiles { get; set; }
		public ObservableCollection<VideoLocalRenamedVM> FileResults { get; set; }

		public ICollectionView ViewTags { get; set; }
		public ObservableCollection<RenameTag> RenameTags { get; set; }

		public ICollectionView ViewTests { get; set; }
		public ObservableCollection<RenameTest> RenameTests { get; set; }

		//public ICollectionView ViewScripts { get; set; }
		public ObservableCollection<RenameScriptVM> RenameScripts { get; set; }

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(FileRenameControl), new UIPropertyMetadata(0, null));

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set 
			{ 
				SetValue(FileCountProperty, value);
				FileCountStatus = string.Format(JMMClient.Properties.Resources.Rename_Files, value);
			}
		}

		public static readonly DependencyProperty FileCountStatusProperty = DependencyProperty.Register("FileCountStatus",
			typeof(string), typeof(FileRenameControl), new UIPropertyMetadata("", null));

		public string FileCountStatus
		{
			get { return (string)GetValue(FileCountStatusProperty); }
			set { SetValue(FileCountStatusProperty, value); }
		}

		public static readonly DependencyProperty LoadTypeIsRandomProperty = DependencyProperty.Register("LoadTypeIsRandom",
			typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(true, null));

		public bool LoadTypeIsRandom
		{
			get { return (bool)GetValue(LoadTypeIsRandomProperty); }
			set { SetValue(LoadTypeIsRandomProperty, value); }
		}

		public static readonly DependencyProperty LoadTypeIsSeriesProperty = DependencyProperty.Register("LoadTypeIsSeries",
			typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

		public bool LoadTypeIsSeries
		{
			get { return (bool)GetValue(LoadTypeIsSeriesProperty); }
			set { SetValue(LoadTypeIsSeriesProperty, value); }
		}

		public static readonly DependencyProperty LoadTypeIsAllProperty = DependencyProperty.Register("LoadTypeIsAll",
			typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

		public bool LoadTypeIsAll
		{
			get { return (bool)GetValue(LoadTypeIsAllProperty); }
			set { SetValue(LoadTypeIsAllProperty, value); }
		}

		public static readonly DependencyProperty WorkerRunningProperty = DependencyProperty.Register("WorkerRunning",
			typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

		public bool WorkerRunning
		{
			get { return (bool)GetValue(WorkerRunningProperty); }
			set { SetValue(WorkerRunningProperty, value); }
		}

		public static readonly DependencyProperty WorkerNotRunningProperty = DependencyProperty.Register("WorkerNotRunning",
			typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(true, null));

		public bool WorkerNotRunning
		{
			get { return (bool)GetValue(WorkerNotRunningProperty); }
			set { SetValue(WorkerNotRunningProperty, value); }
		}

		public static readonly DependencyProperty WorkerStatusProperty = DependencyProperty.Register("WorkerStatus",
			typeof(string), typeof(FileRenameControl), new UIPropertyMetadata("", null));

		public string WorkerStatus
		{
			get { return (string)GetValue(WorkerStatusProperty); }
			set { SetValue(WorkerStatusProperty, value); }
		}

		private readonly string LoadTypeRandom = JMMClient.Properties.Resources.Rename_Random;
		private readonly string LoadTypeSeries = JMMClient.Properties.Resources.Rename_Series;
		private readonly string LoadTypeAll = JMMClient.Properties.Resources.Rename_All;

		private readonly string FilterTypeAll = JMMClient.Properties.Resources.Random_All;
		private readonly string FilterTypeFailed = JMMClient.Properties.Resources.Rename_Failed;
		private readonly string FilterTypePassed = JMMClient.Properties.Resources.Rename_Passed;

		private int? defaultScriptID = null;

		BackgroundWorker previewWorker = new BackgroundWorker();
		BackgroundWorker renameWorker = new BackgroundWorker();
		private bool stopWorker = false;

		public FileRenameControl()
		{
			InitializeComponent();

			FileResults = new ObservableCollection<VideoLocalRenamedVM>();
			ViewFiles = CollectionViewSource.GetDefaultView(FileResults);

			RenameTags = new ObservableCollection<RenameTag>();
			foreach (RenameTag tag in RenameHelper.GetAllTags())
				RenameTags.Add(tag);
			ViewTags = CollectionViewSource.GetDefaultView(RenameTags);
			cboTagType.SelectedIndex = 0;

			RenameTests = new ObservableCollection<RenameTest>();
			foreach (RenameTest test in RenameHelper.GetAllTests())
				RenameTests.Add(test);
			ViewTests = CollectionViewSource.GetDefaultView(RenameTests);
			cboTestType.SelectedIndex = 0;

			RenameScripts = new ObservableCollection<RenameScriptVM>();
			//ViewScripts = CollectionViewSource.GetDefaultView(RenameScripts);
			

			/*string testScript = "IF A(69),A(1),A(2) DO FAIL" + Environment.NewLine + //Do not rename file if it is Naruto
				//// Group Name
				"DO ADD '[%grp] '" + Environment.NewLine + //sub group name short
				//// Anime Name
				"IF I(eng) DO ADD '%eng - '" + Environment.NewLine + // if the anime has an official/main title english add it to the string
				"IF I(ann);I(!eng) DO ADD '%ann - '" + Environment.NewLine + //If the anime has a romaji title but not an english title add the romaji anime title
				//// Episode Number
				"DO ADD '%enr - '" + Environment.NewLine + //Add the base, same for all files
				//// FILE Version
				"IF F(!1) DO ADD ' [v%ver]'" + Environment.NewLine + //If the episode has an english title add to string
				//// Episode Title
				"IF I(epr) DO ADD '%epr'" + Environment.NewLine + //If the episode has an english title add to string
				"IF I(epn);I(!epr) DO ADD '%epn'" + Environment.NewLine + //If the episode has an romaji title but not an english title add the romaji episode title
				//// Codecs
				"DO ADD ' [%vid/%aud]'" + Environment.NewLine +
				//// video depth
				"IF Z(10) DO ADD '[%bitbit]'" + Environment.NewLine +
				//// Blu-ray and DVD
				"IF R(Blu-ray),R(DVD) DO ADD '[%src]'" + Environment.NewLine +
				//// CRC
				"DO ADD '[%CRC]'" + Environment.NewLine;
				


			txtRenameScript.Text = testScript;*/

			btnLoadFiles.Click += new RoutedEventHandler(btnLoadFiles_Click);
			btnPreviewFiles.Click += new RoutedEventHandler(btnPreviewFiles_Click);
			btnRenameFiles.Click += new RoutedEventHandler(btnRenameFiles_Click);
			btnPreviewStop.Click += new RoutedEventHandler(btnPreviewStop_Click);
			btnClearList.Click += new RoutedEventHandler(btnClearList_Click);
			btnAddTag.Click += new RoutedEventHandler(btnAddTag_Click);
			btnAddTest.Click += new RoutedEventHandler(btnAddTest_Click);

			btnNewScript.Click += new RoutedEventHandler(btnNewScript_Click);
			btnSaveScript.Click += new RoutedEventHandler(btnSaveScript_Click);
			btnDeleteScript.Click += new RoutedEventHandler(btnDeleteScript_Click);

            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string cult = appSettings["Culture"];
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cult);

            cboLoadType.Items.Clear();
			cboLoadType.Items.Add(JMMClient.Properties.Resources.Rename_Random);
			cboLoadType.Items.Add(JMMClient.Properties.Resources.Rename_Series);
			cboLoadType.Items.Add(JMMClient.Properties.Resources.Rename_All);
			cboLoadType.SelectedIndex = 0;

			cboLoadType.SelectionChanged += new SelectionChangedEventHandler(cboLoadType_SelectionChanged);

			cboFilterType.Items.Clear();
			cboFilterType.Items.Add(JMMClient.Properties.Resources.Random_All);
			cboFilterType.Items.Add(JMMClient.Properties.Resources.Rename_Failed);
			cboFilterType.Items.Add(JMMClient.Properties.Resources.Rename_Passed);
			cboFilterType.SelectionChanged += new SelectionChangedEventHandler(cboFilterType_SelectionChanged);
			cboFilterType.SelectedIndex = 0;

			cboScript.SelectionChanged += new SelectionChangedEventHandler(cboScript_SelectionChanged);

			previewWorker.DoWork += new DoWorkEventHandler(previewWorker_DoWork);
			previewWorker.WorkerSupportsCancellation = true;
			previewWorker.WorkerReportsProgress = true;
			previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewWorker_RunWorkerCompleted);
			previewWorker.ProgressChanged += new ProgressChangedEventHandler(previewWorker_ProgressChanged);


			renameWorker.DoWork += new DoWorkEventHandler(renameWorker_DoWork);
			renameWorker.WorkerSupportsCancellation = true;
			renameWorker.WorkerReportsProgress = true;
			renameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(renameWorker_RunWorkerCompleted);
			renameWorker.ProgressChanged += new ProgressChangedEventHandler(renameWorker_ProgressChanged);

			txtRenameScript.LostFocus += new RoutedEventHandler(txtRenameScript_LostFocus);

			ViewTags.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
			ViewTests.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
			//ViewScripts.SortDescriptions.Add(new SortDescription("ScriptName", ListSortDirection.Ascending));
		}

		

		void btnDeleteScript_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (cboScript.Items.Count == 0) return;
				if (cboScript.SelectedItem == null) return;
				RenameScriptVM script = cboScript.SelectedItem as RenameScriptVM;

				string msg = string.Format(JMMClient.Properties.Resources.Rename_DeleteScript, script.ScriptName);
				MessageBoxResult res = MessageBox.Show(msg, JMMClient.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

				if (res == MessageBoxResult.Yes)
				{
					this.Cursor = Cursors.Wait;

					string errorMsg = JMMServerVM.Instance.clientBinaryHTTP.DeleteRenameScript(script.RenameScriptID.Value);
					if (!string.IsNullOrEmpty(errorMsg))
					{
						Utils.ShowErrorMessage(errorMsg);
					}
					else
						defaultScriptID = null;

					// refresh data
					RefreshScripts();
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			this.Cursor = Cursors.Arrow;
		}

		void btnSaveScript_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (cboScript.Items.Count == 0) return;
				if (cboScript.SelectedItem == null) return;

				RenameScriptVM script = cboScript.SelectedItem as RenameScriptVM;
				script.IsEnabledOnImport = chkIsUsedForImports.IsChecked.Value ? 1 : 0;
				script.Script = txtRenameScript.Text;
				if (script.Save())
				{
					defaultScriptID = script.RenameScriptID;

					// refresh data
					RefreshScripts();
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnNewScript_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogText dlg = new DialogText();
				dlg.Init(JMMClient.Properties.Resources.Rename_EnterScriptName, "");
				dlg.Owner = Window.GetWindow(this);
				bool? res = dlg.ShowDialog();
				if (res.HasValue && res.Value)
				{
					if (string.IsNullOrEmpty(dlg.EnteredText))
					{
						Utils.ShowErrorMessage(JMMClient.Properties.Resources.Rename_BlankScript);
						return;
					}

					JMMServerBinary.Contract_RenameScript script = new JMMServerBinary.Contract_RenameScript();
					script.IsEnabledOnImport = 0;
					script.Script = "";
					script.ScriptName = dlg.EnteredText;
					JMMServerBinary.Contract_RenameScript_SaveResponse resp = JMMServerVM.Instance.clientBinaryHTTP.SaveRenameScript(script);

					if (!string.IsNullOrEmpty(resp.ErrorMessage))
					{
						Utils.ShowErrorMessage(resp.ErrorMessage);
						return;
					}

					RenameScriptVM plRet = new RenameScriptVM(resp.RenameScript);
					defaultScriptID = plRet.RenameScriptID;

					// refresh data
					RefreshScripts();
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void cboScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cboScript.Items.Count == 0) return;
			if (cboScript.SelectedItem == null) return;

			RenameScriptVM script = cboScript.SelectedItem as RenameScriptVM;
			txtRenameScript.Text = script.Script;
			chkIsUsedForImports.IsChecked = script.IsEnabledOnImportBool;
		}

		public void RefreshScripts()
		{
			try
			{
				RenameScripts.Clear();

				List<RenameScriptVM> scripts = new List<RenameScriptVM>();
				
				foreach (JMMServerBinary.Contract_RenameScript raw in JMMServerVM.Instance.clientBinaryHTTP.GetAllRenameScripts())
					scripts.Add(new RenameScriptVM(raw));

				// sort by scrit name
				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("ScriptName", false, SortType.eString));
				scripts = Sorting.MultiSort<RenameScriptVM>(scripts, sortCriteria);

				if (scripts.Count > 0)
				{
					int idx = 0;
					if (defaultScriptID.HasValue)
					{
						foreach (RenameScriptVM scr in scripts)
						{
							if (scr.RenameScriptID.Value == defaultScriptID.Value) break;
							idx++;
						}
					}

					foreach (RenameScriptVM scr in scripts)
						RenameScripts.Add(scr);

					cboScript.SelectedIndex = idx;
				}
				else
					defaultScriptID = null;

				
						
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void cboFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewFiles.Filter = FileFilter;
		}

		private bool FileFilter(object obj)
		{
			VideoLocalRenamedVM vid = obj as VideoLocalRenamedVM;
			if (vid == null) return false;

			string filterType = cboFilterType.SelectedItem.ToString();

			if (filterType.Equals(FilterTypeAll)) return true;

			if (filterType.Equals(FilterTypeFailed) && !vid.Success) return true;
			if (filterType.Equals(FilterTypePassed) && vid.Success) return true;

			return false;
		}

		void txtRenameScript_LostFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
		}

		void btnAddTag_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (cboTagType.SelectedItem == null) return;

				RenameTag tag = cboTagType.SelectedItem as RenameTag;
				txtRenameScript.Text = txtRenameScript.Text.Insert(txtRenameScript.CaretIndex, tag.Tag);
			}
			catch { }
		}

		void btnAddTest_Click(object sender, RoutedEventArgs e)
		{
			try
			{

				if (cboTestType.SelectedItem == null) return;

				RenameTest test = cboTestType.SelectedItem as RenameTest;
				txtRenameScript.Text = txtRenameScript.Text.Insert(txtRenameScript.CaretIndex, test.Test);
			}
			catch { }
		}

		void btnClearList_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FileResults.Clear();
				ViewFiles.Refresh();
				FileCount = 0;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnPreviewStop_Click(object sender, RoutedEventArgs e)
		{
			stopWorker = true;
		}

		void renameWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ViewFiles.Refresh();
			WorkerStatusContainer status = e.UserState as WorkerStatusContainer;
			WorkerStatus = string.Format(JMMClient.Properties.Resources.Rename_Changed, status.CurrentFile, status.TotalFileCount);
		}

		void renameWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			ViewFiles.Refresh();
			WorkerRunning = false;
			WorkerNotRunning = true;
			stopWorker = false;
			EnableDisableControls(true);
		}

		void renameWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			WorkerJob job = e.Argument as WorkerJob;
			BackgroundWorker worker = sender as BackgroundWorker;

			foreach (VideoLocalRenamedVM ren in job.FileResults)
			{
				if (stopWorker) return;
				ren.NewFileName = "";
			}

			int curFile = 0;
			int delay = 0;
			foreach (VideoLocalRenamedVM ren in job.FileResults)
			{
				if (stopWorker) return;

				curFile++;
				delay++;

				JMMServerBinary.Contract_VideoLocalRenamed raw = JMMServerVM.Instance.clientBinaryHTTP.RenameFile(
					ren.VideoLocalID, job.RenameScript);

				ren.NewFileName = raw.NewFileName;
				ren.Success = raw.Success;

				// do this so we don't lock the UI
				if (delay == 20)
				{
					renameWorker.ReportProgress(0, new WorkerStatusContainer(job.FileResults.Count, curFile));
					delay = 0;
				}
			}
		}

		void btnRenameFiles_Click(object sender, RoutedEventArgs e)
		{
			string msg = string.Format(JMMClient.Properties.Resources.Rename_Confirm);
			MessageBoxResult res = MessageBox.Show(msg, JMMClient.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (res != MessageBoxResult.Yes) return;

			EnableDisableControls(false);

			WorkerRunning = true;
			WorkerNotRunning = false;
			stopWorker = false;

			WorkerJob job = new WorkerJob();
			job.RenameScript = txtRenameScript.Text;
			job.FileResults = FileResults;

			renameWorker.RunWorkerAsync(job);
		}

		void previewWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ViewFiles.Refresh();
			WorkerStatusContainer status = e.UserState as WorkerStatusContainer;
			WorkerStatus = string.Format(JMMClient.Properties.Resources.Rename_Changed, status.CurrentFile, status.TotalFileCount);
		}

		void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			ViewFiles.Refresh();
			WorkerRunning = false;
			WorkerNotRunning = true;
			stopWorker = false;
			EnableDisableControls(true);
			//HideShowControls(System.Windows.Visibility.Visible);
		}

		void previewWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			WorkerJob job = e.Argument as WorkerJob;
			BackgroundWorker worker = sender as BackgroundWorker;

			foreach (VideoLocalRenamedVM ren in job.FileResults)
			{
				if (stopWorker) return;
				ren.NewFileName = "";
			}

			int curFile = 0;
			int delay = 0;
			foreach (VideoLocalRenamedVM ren in job.FileResults)
			{
				if (stopWorker) return;

				curFile++;
				delay++;

				JMMServerBinary.Contract_VideoLocalRenamed raw = JMMServerVM.Instance.clientBinaryHTTP.RenameFilePreview(
					ren.VideoLocalID, job.RenameScript);

				ren.NewFileName = raw.NewFileName;
				ren.Success = raw.Success;
			
				// do this so we don't lock the UI
				if (delay == 20)
				{
					previewWorker.ReportProgress(0, new WorkerStatusContainer(job.FileResults.Count, curFile));
					delay = 0;
				}
			}
		}

		void btnPreviewFiles_Click(object sender, RoutedEventArgs e)
		{
			EnableDisableControls(false);
			//HideShowControls(System.Windows.Visibility.Hidden);
			WorkerRunning = true;
			WorkerNotRunning = false;
			stopWorker = false;

			WorkerJob job = new WorkerJob();
			job.RenameScript = txtRenameScript.Text;
			job.FileResults = FileResults;

			previewWorker.RunWorkerAsync(job);
			
		}

		void cboLoadType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LoadTypeIsRandom = (cboLoadType.SelectedItem.ToString() == LoadTypeRandom);
			LoadTypeIsSeries = (cboLoadType.SelectedItem.ToString() == LoadTypeSeries);
			LoadTypeIsAll = (cboLoadType.SelectedItem.ToString() == LoadTypeAll);
		}

		void btnLoadFiles_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!JMMServerVM.Instance.ServerOnline) return;

				ViewFiles.Refresh();

				this.Cursor = Cursors.Wait;
				EnableDisableControls(false);
				List<JMMServerBinary.Contract_VideoLocal> rawVids = new List<JMMServerBinary.Contract_VideoLocal>();

				if (LoadTypeIsRandom)
				{
					rawVids = JMMServerVM.Instance.clientBinaryHTTP.RandomFileRenamePreview(udRandomFiles.Value.Value, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					/*List<int> testIDs = new List<int>();

					testIDs.Add(6041); // Gekijouban Bleach: Fade to Black Kimi no Na o Yobu
					testIDs.Add(6784); // Fate/Stay Night: Unlimited Blade Works
					testIDs.Add(5975); // Toaru Majutsu no Index
					testIDs.Add(7599); // Toaru Majutsu no Index II
					testIDs.Add(8694); // Gekijouban Toaru Majutsu no Index (movie)
					testIDs.Add(6071); // Quiz Magic Academy: The Original Animation
					testIDs.Add(4145); // Amaenaide yo!! Katsu!!
					testIDs.Add(2369); // Bleach
					testIDs.Add(69); // One Piece
					foreach (int animeID in testIDs)
					{
						List<JMMServerBinary.Contract_VideoLocal> raws = JMMServerVM.Instance.clientBinaryHTTP.GetVideoLocalsForAnime(animeID,
							JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

						rawVids.AddRange(raws);
					}*/
				}

				if (LoadTypeIsAll)
				{
					rawVids = JMMServerVM.Instance.clientBinaryHTTP.RandomFileRenamePreview(int.MaxValue, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				}
				

				if (LoadTypeIsSeries)
				{
					Window wdw = Window.GetWindow(this);
					SelectGroupSeriesForm frm = new SelectGroupSeriesForm();
					frm.Owner = wdw;
					frm.Init();

					bool? result = frm.ShowDialog();
					if (result.HasValue && result.Value == true)
					{
						if (frm.SelectedObject.GetType() == typeof(AnimeGroupVM))
						{
							AnimeGroupVM grp = frm.SelectedObject as AnimeGroupVM;
							foreach (AnimeSeriesVM ser in grp.AllAnimeSeries)
							{
								List<JMMServerBinary.Contract_VideoLocal> raws = JMMServerVM.Instance.clientBinaryHTTP.GetVideoLocalsForAnime(ser.AniDB_ID,
								JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

								rawVids.AddRange(raws);
							}
						}
						if (frm.SelectedObject.GetType() == typeof(AnimeSeriesVM))
						{
							AnimeSeriesVM ser = frm.SelectedObject as AnimeSeriesVM;
							List<JMMServerBinary.Contract_VideoLocal> raws = JMMServerVM.Instance.clientBinaryHTTP.GetVideoLocalsForAnime(ser.AniDB_ID,
							JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

							rawVids.AddRange(raws);
						}
					}
				}

				foreach (JMMServerBinary.Contract_VideoLocal raw in rawVids)
				{
					VideoLocalVM vid = new VideoLocalVM(raw);
					VideoLocalRenamedVM ren = new VideoLocalRenamedVM();
					ren.VideoLocalID = vid.VideoLocalID;
					ren.VideoLocal = vid;
					ren.Success = false;
					FileResults.Add(ren);
				}

				FileCount = FileResults.Count;

				this.Cursor = Cursors.Arrow;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
				EnableDisableControls(true);
			}
		}

		private void EnableDisableControls(bool val)
		{
			btnLoadFiles.IsEnabled = val;
			btnPreviewFiles.IsEnabled = val;
			cboLoadType.IsEnabled = val;
		}

		private void HideShowControls(System.Windows.Visibility val)
		{
			btnLoadFiles.Visibility = val;
			btnPreviewFiles.Visibility = val;
			cboLoadType.Visibility = val;
		}
	}

	public class WorkerJob
	{
		public string RenameScript { get; set; }
		public int MaxFiles { get; set; }
		public ObservableCollection<VideoLocalRenamedVM> FileResults { get; set; }
	}

	public class WorkerStatusContainer
	{
        public int TotalFileCount { get; set; }
		public int CurrentFile { get; set; }

		public WorkerStatusContainer()
		{
		}

		public WorkerStatusContainer(int totalFileCount, int currentFile)
		{
            TotalFileCount = totalFileCount;
			CurrentFile = currentFile;
		}
	}
}
