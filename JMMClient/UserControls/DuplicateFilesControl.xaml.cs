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
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DuplicateFilesControl.xaml
	/// </summary>
	public partial class DuplicateFilesControl : UserControl
	{
		public ICollectionView ViewFiles { get; set; }
		public ObservableCollection<DuplicateFileVM> DuplicateFilesCollection { get; set; }

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(DuplicateFilesControl), new UIPropertyMetadata(0, null));

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set { SetValue(FileCountProperty, value); }
		}

		private int lastSelIndex = 0;

		public DuplicateFilesControl()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            DuplicateFilesCollection = new ObservableCollection<DuplicateFileVM>();
			ViewFiles = CollectionViewSource.GetDefaultView(DuplicateFilesCollection);
			ViewFiles.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
			ViewFiles.SortDescriptions.Add(new SortDescription("EpisodeNumber", ListSortDirection.Ascending));

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			lbDuplicateFiles.SelectionChanged += new SelectionChangedEventHandler(lbDuplicateFiles_SelectionChanged);

			btnReevaluate.Click += new RoutedEventHandler(btnReevaluate_Click);
		}

		void btnReevaluate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Cursor = Cursors.Wait;
				JMMServerVM.Instance.clientBinaryHTTP.ReevaluateDuplicateFiles();
				RefreshDuplicateFiles();
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

		void lbDuplicateFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lbDuplicateFiles.Items.Count > 0)
				lastSelIndex = lbDuplicateFiles.SelectedIndex;
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshDuplicateFiles();
		}

		public void RefreshDuplicateFiles()
		{
			try
			{
				this.Cursor = Cursors.Wait;
				DuplicateFilesCollection.Clear();

				List<JMMServerBinary.Contract_DuplicateFile> dfs = JMMServerVM.Instance.clientBinaryHTTP.GetAllDuplicateFiles();
				FileCount = dfs.Count;

				foreach (JMMServerBinary.Contract_DuplicateFile df in dfs)
					DuplicateFilesCollection.Add(new DuplicateFileVM(df));

				// move to the next item
				if (lastSelIndex <= lbDuplicateFiles.Items.Count)
				{
					lbDuplicateFiles.SelectedIndex = lastSelIndex;
					lbDuplicateFiles.Focus();
					lbDuplicateFiles.ScrollIntoView(lbDuplicateFiles.SelectedItem);
				}
				else
				{
					// move to the previous item
					if (lastSelIndex - 1 <= lbDuplicateFiles.Items.Count)
					{
						lbDuplicateFiles.SelectedIndex = lastSelIndex - 1;
						lbDuplicateFiles.Focus();
						lbDuplicateFiles.ScrollIntoView(lbDuplicateFiles.SelectedItem);
					}
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

		private void CommandBinding_OpenFolder1(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;
					if (File.Exists(df.FullPath1))
						Utils.OpenFolderAndSelectFile(df.FullPath1);
					else
						MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_OpenFolder2(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;
					if (File.Exists(df.FullPath2))
						Utils.OpenFolderAndSelectFile(df.FullPath2);
					else
						MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_PlayVideo1(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;
					Process.Start(new ProcessStartInfo(df.FullPath1));
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

		private void CommandBinding_PlayVideo2(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;
					Process.Start(new ProcessStartInfo(df.FullPath2));
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

		private void CommandBinding_DeleteFile1(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;

					MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.DuplicateFiles_ConfirmDelete),
                        Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

					if (res == MessageBoxResult.Yes)
					{
						this.Cursor = Cursors.Wait;
						string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteDuplicateFile(df.DuplicateFileID, 1);
						if (result.Length > 0)
							MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						else
						{
							DuplicateFilesCollection.Remove(df);
							FileCount = DuplicateFilesCollection.Count;
							//RefreshDuplicateFiles();
						}
					}

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

		private void CommandBinding_DeleteFile2(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;

					MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.DuplicateFiles_ConfirmDelete),
                        Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

					if (res == MessageBoxResult.Yes)
					{
						this.Cursor = Cursors.Wait;
						string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteDuplicateFile(df.DuplicateFileID, 2);
						if (result.Length > 0)
							MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						else
						{
							DuplicateFilesCollection.Remove(df);
							FileCount = DuplicateFilesCollection.Count;
							//RefreshDuplicateFiles();
						}
					}

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

		private void CommandBinding_DeleteFileDB(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(DuplicateFileVM))
				{
					DuplicateFileVM df = obj as DuplicateFileVM;

					MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.DuplicateFiles_DeleteEntry),
						Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);

					if (res == MessageBoxResult.Yes)
					{
						this.Cursor = Cursors.Wait;
						string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteDuplicateFile(df.DuplicateFileID, 0);
						if (result.Length > 0)
							MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						else
						{
							DuplicateFilesCollection.Remove(df);
							FileCount = DuplicateFilesCollection.Count;
							//RefreshDuplicateFiles();
						}
					}

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
