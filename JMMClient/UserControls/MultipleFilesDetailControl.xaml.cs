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
using JMMClient.ViewModel;
using System.IO;
using System.Threading;
using System.Globalization;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for MultipleFilesDetailControl.xaml
	/// </summary>
	public partial class MultipleFilesDetailControl : UserControl
	{
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
			typeof(bool), typeof(MultipleFilesDetailControl), new UIPropertyMetadata(false, isExpandedCallback));

		public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
			typeof(bool), typeof(MultipleFilesDetailControl), new UIPropertyMetadata(true, isCollapsedCallback));

		public bool IsExpanded
		{
			get { return (bool)GetValue(IsExpandedProperty); }
			set { SetValue(IsExpandedProperty, value); }
		}

		public bool IsCollapsed
		{
			get { return (bool)GetValue(IsCollapsedProperty); }
			set { SetValue(IsCollapsedProperty, value); }
		}

		private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//EpisodeDetail input = (EpisodeDetail)d;
			//input.tbTest.Text = e.NewValue as string;
		}

		private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//EpisodeDetail input = (EpisodeDetail)d;
			//input.tbTest.Text = e.NewValue as string;
		}

		public MultipleFilesDetailControl()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
		}

		private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;

					MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.MultipleFiles_ConfirmDelete),
                        Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

					if (res == MessageBoxResult.Yes)
					{
						this.Cursor = Cursors.Wait;
						string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalAndFile(vid.VideoLocalID);
						if (result.Length > 0)
							MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
						else
						{
							// find the entry and remove it
							AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
							if (ep != null)
							{
								MainListHelperVM.Instance.UpdateHeirarchy(ep);
								ep.LocalFileCount--;
							}
							DisplayFiles();
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

		private void CommandBinding_ToggleVariation(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;

					vid.VideoLocal_IsVariation = vid.Variation ? 0 : 1;

					string result = JMMServerVM.Instance.clientBinaryHTTP.SetVariationStatusOnFile(vid.VideoLocalID, vid.Variation);
					if (result.Length > 0)
						MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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

		public void DisplayFiles()
		{
			try
			{
				AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
				if (ep != null)
				{
					ep.RefreshFilesForEpisode();
					ep.RefreshAnime();
					fileSummary.DataContext = null;
					fileSummary.DataContext = ep.AniDB_Anime;

					lbFiles.ItemsSource = ep.FilesForEpisode;

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnToggleExpander_Click(object sender, RoutedEventArgs e)
		{
			IsExpanded = !IsExpanded;
			IsCollapsed = !IsCollapsed;

			if (IsExpanded)
			{
				DisplayFiles();
			}
		}
	}
}
