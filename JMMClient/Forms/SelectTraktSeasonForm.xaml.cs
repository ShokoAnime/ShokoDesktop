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
using System.Windows.Shapes;
using NLog;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for SelectTraktSeasonForm.xaml
	/// </summary>
	public partial class SelectTraktSeasonForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
			typeof(int), typeof(SearchTvDBForm), new UIPropertyMetadata(0, null));

		public int AnimeID
		{
			get { return (int)GetValue(AnimeIDProperty); }
			set { SetValue(AnimeIDProperty, value); }
		}

		public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
			typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

		public string AnimeName
		{
			get { return (string)GetValue(AnimeNameProperty); }
			set { SetValue(AnimeNameProperty, value); }
		}

		public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
			typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

		public string AnimeURL
		{
			get { return (string)GetValue(AnimeURLProperty); }
			set { SetValue(AnimeURLProperty, value); }
		}

		public static readonly DependencyProperty TraktIDProperty = DependencyProperty.Register("TraktID",
			typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

		public string TraktID
		{
			get { return (string)GetValue(TraktIDProperty); }
			set { SetValue(TraktIDProperty, value); }
		}

		public static readonly DependencyProperty TraktSeasonProperty = DependencyProperty.Register("TraktSeason",
			typeof(int), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(0, null));

		public int TraktSeason
		{
			get { return (int)GetValue(TraktSeasonProperty); }
			set { SetValue(TraktSeasonProperty, value); }
		}

		public static readonly DependencyProperty TraktSeriesNameProperty = DependencyProperty.Register("TraktSeriesName",
			typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

		public string TraktSeriesName
		{
			get { return (string)GetValue(TraktSeriesNameProperty); }
			set { SetValue(TraktSeriesNameProperty, value); }
		}

		public static readonly DependencyProperty TraktURLProperty = DependencyProperty.Register("TraktURL",
			typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

		public string TraktURL
		{
			get { return (string)GetValue(TraktURLProperty); }
			set { SetValue(TraktURLProperty, value); }
		}

		public SelectTraktSeasonForm()
		{
			InitializeComponent();

			btnClose.Click += new RoutedEventHandler(btnClose_Click);
			btnUpdate.Click += new RoutedEventHandler(btnUpdate_Click);
		}

		void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Cursor = Cursors.Wait;

				TraktSeason = int.Parse(cboSeasonNumber.SelectedItem.ToString());

				string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTrakt(AnimeID, TraktID, TraktSeason);
				if (res.Length > 0)
					MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				else
				{
					this.DialogResult = true;
					this.Close();
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

		void btnClose_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		public void Init(int animeID, string animeName, string traktID, int traktSeason, string trakSeriesName)
		{
			AnimeID = animeID;
			AnimeName = animeName;
			TraktID = traktID;
			TraktSeason = traktSeason;
			TraktSeriesName = trakSeriesName;

			AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
			TraktURL = string.Format(Constants.URLS.Trakt_Series, TraktID);

			// get the seasons

			try
			{
				cboSeasonNumber.Items.Clear();
				List<int> seasons = JMMServerVM.Instance.clientBinaryHTTP.GetSeasonNumbersForTrakt(traktID);
				int i = 0;
				int idx = 0;
				foreach (int season in seasons)
				{
					cboSeasonNumber.Items.Add(season.ToString());
					if (season == traktSeason) idx = i;
					i++;
				}
				cboSeasonNumber.SelectedIndex = idx;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}
