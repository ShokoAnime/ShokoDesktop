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
using System.Diagnostics;
using JMMClient.ViewModel;
using System.IO;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for AvdumpFileControl.xaml
	/// </summary>
	public partial class AvdumpFileControl : UserControl
	{
		BackgroundWorker workerAvdump = new BackgroundWorker();

		public AniDB_AnimeVM SelectedAnime { get; set; }

		public ICollectionView ViewAnime { get; set; }
		public ObservableCollection<AniDB_AnimeVM> AllAnime { get; set; }

		public static readonly DependencyProperty IsAnimeNotPopulatedProperty = DependencyProperty.Register("IsAnimeNotPopulated",
			typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(true, null));

		public bool IsAnimeNotPopulated
		{
			get { return (bool)GetValue(IsAnimeNotPopulatedProperty); }
			set { SetValue(IsAnimeNotPopulatedProperty, value); }
		}

		public static readonly DependencyProperty IsAnimePopulatedProperty = DependencyProperty.Register("IsAnimePopulated",
			typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));

		
		public bool IsAnimePopulated
		{
			get { return (bool)GetValue(IsAnimePopulatedProperty); }
			set { SetValue(IsAnimePopulatedProperty, value); }
		}

		public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
			typeof(string), typeof(AvdumpFileControl), new UIPropertyMetadata("", null));


		public string AnimeURL
		{
			get { return (string)GetValue(AnimeURLProperty); }
			set { SetValue(AnimeURLProperty, value); }
		}

		public static readonly DependencyProperty AvdumpDetailsNotValidProperty = DependencyProperty.Register("AvdumpDetailsNotValid",
			typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


		public bool AvdumpDetailsNotValid
		{
			get { return (bool)GetValue(AvdumpDetailsNotValidProperty); }
			set { SetValue(AvdumpDetailsNotValidProperty, value); }
		}

		public static readonly DependencyProperty ValidED2KDumpProperty = DependencyProperty.Register("ValidED2KDump",
			typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


		public bool ValidED2KDump
		{
			get { return (bool)GetValue(ValidED2KDumpProperty); }
			set { SetValue(ValidED2KDumpProperty, value); }
		}

		public AvdumpFileControl()
		{
			InitializeComponent();

			SetSelectedAnime(null);

			AllAnime = new ObservableCollection<AniDB_AnimeVM>();

			btnClearAnimeSearch.Click += new RoutedEventHandler(btnClearAnimeSearch_Click);
			txtAnimeSearch.TextChanged += new TextChangedEventHandler(txtAnimeSearch_TextChanged);
			lbAnime.SelectionChanged += new SelectionChangedEventHandler(lbAnime_SelectionChanged);
			hlURL.Click += new RoutedEventHandler(hlURL_Click);
			btnAvdumpFile.Click += new RoutedEventHandler(btnAvdumpFile_Click);
			btnClipboard.Click += new RoutedEventHandler(btnClipboard_Click);

			workerAvdump.DoWork += new DoWorkEventHandler(workerAvdump_DoWork);
			workerAvdump.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerAvdump_RunWorkerCompleted);

			AvdumpDetailsNotValid = string.IsNullOrEmpty(JMMServerVM.Instance.AniDB_AVDumpClientPort) || string.IsNullOrEmpty(JMMServerVM.Instance.AniDB_AVDumpKey);

			try
			{
				ViewAnime = CollectionViewSource.GetDefaultView(AllAnime);
				ViewAnime.SortDescriptions.Add(new SortDescription("MainTitle", ListSortDirection.Ascending));

				List<JMMServerBinary.Contract_AniDBAnime> animeRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();
				foreach (JMMServerBinary.Contract_AniDBAnime anime in animeRaw)
				{
					AniDB_AnimeVM animeNew = new AniDB_AnimeVM(anime);
					AllAnime.Add(animeNew);
				}

				ViewAnime.Filter = AnimeSearchFilter;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnClipboard_Click(object sender, RoutedEventArgs e)
		{
			GetED2KDump(txtOutput.Text);
		}

		private void GetED2KDump(string result)
		{
			// try and get the ed2k dump from the keyboard
			string[] lines = result.Split('\r');
			foreach (string line in lines)
			{
				string editedLine = line.Replace('\n', ' ');
				editedLine = editedLine.Trim();

				if (editedLine.StartsWith(@"ed2k://"))
				{
					Clipboard.Clear();
					Clipboard.SetText(editedLine);
					ValidED2KDump = true;
					return;
				}
			}

			ValidED2KDump = false;
		}

		void btnAvdumpFile_Click(object sender, RoutedEventArgs e)
		{
			btnAvdumpFile.IsEnabled = false;
			btnClipboard.IsEnabled = false;
			ValidED2KDump = false;
			txtOutput.Text = "Processing...";
			workerAvdump.RunWorkerAsync(this.DataContext as VideoLocalVM);
		}

		void workerAvdump_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			
			btnAvdumpFile.IsEnabled = true;
			btnClipboard.IsEnabled = true;

			string result = e.Result.ToString();
			txtOutput.Text = result;

			GetED2KDump(result);
		}

		void workerAvdump_DoWork(object sender, DoWorkEventArgs e)
		{
			VideoLocalVM vid = e.Argument as VideoLocalVM;

			//Create process
			System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

			//strCommand is path and file name of command to run
			string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string filePath = System.IO.Path.Combine(appPath, "AVDump2CL.exe");

			if (!File.Exists(filePath))
			{
				e.Result = "Could not find AvDump2 CLI: " + filePath;
				return;
			}

			if (!File.Exists(vid.FullPath))
			{
				e.Result = "Could not find Video File: " + vid.FullPath;
				return;
			}

			pProcess.StartInfo.FileName = filePath;

			//strCommandParameters are parameters to pass to program
			string fileName = (char)34 + vid.FullPath + (char)34;

			pProcess.StartInfo.Arguments =
				string.Format(@" --Auth={0}:{1} --LPort={2} --PrintEd2kLink {3}", JMMServerVM.Instance.AniDB_Username, JMMServerVM.Instance.AniDB_AVDumpKey, 
				JMMServerVM.Instance.AniDB_AVDumpClientPort, fileName);

			pProcess.StartInfo.UseShellExecute = false;
			pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
			pProcess.StartInfo.RedirectStandardOutput = true;
			pProcess.Start();
			string strOutput = pProcess.StandardOutput.ReadToEnd();

			//Wait for process to finish
			pProcess.WaitForExit();

			e.Result = strOutput;
		}

		void hlURL_Click(object sender, RoutedEventArgs e)
		{
			Uri uri = new Uri(string.Format(Constants.URLS.AniDB_Series_NewRelease, SelectedAnime.AnimeID));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void btnClearAnimeSearch_Click(object sender, RoutedEventArgs e)
		{
			txtAnimeSearch.Text = "";
		}

		void lbAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			AniDB_AnimeVM anime = lbAnime.SelectedItem as AniDB_AnimeVM;
			if (anime == null) return;

			SetSelectedAnime(anime);
		}

		private void SetSelectedAnime(AniDB_AnimeVM anime)
		{
			if (anime != null)
			{
				IsAnimeNotPopulated = false;
				IsAnimePopulated = true;
				SelectedAnime = anime;
			}
			else
			{
				IsAnimeNotPopulated = true;
				IsAnimePopulated = false;
				SelectedAnime = null;
			}
		}

		void txtAnimeSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewAnime.Refresh();
		}

		private bool AnimeSearchFilter(object obj)
		{
			AniDB_AnimeVM anime = obj as AniDB_AnimeVM;
			if (anime == null) return true;

			return GroupSearchFilterHelper.EvaluateAnimeTextSearch(anime, txtAnimeSearch.Text);
		}
	}
}
