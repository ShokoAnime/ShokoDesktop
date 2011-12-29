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
	/// Interaction logic for SelectTvDBSeasonForm.xaml
	/// </summary>
	public partial class SelectTvDBSeasonForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
			typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

		public int AnimeID
		{
			get { return (int)GetValue(AnimeIDProperty); }
			set { SetValue(AnimeIDProperty, value); }
		}

		public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
			typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

		public string AnimeName
		{
			get { return (string)GetValue(AnimeNameProperty); }
			set { SetValue(AnimeNameProperty, value); }
		}

		public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
			typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

		public string AnimeURL
		{
			get { return (string)GetValue(AnimeURLProperty); }
			set { SetValue(AnimeURLProperty, value); }
		}

		public static readonly DependencyProperty TvDBIDProperty = DependencyProperty.Register("TvDBID",
			typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

		public int TvDBID
		{
			get { return (int)GetValue(TvDBIDProperty); }
			set { SetValue(TvDBIDProperty, value); }
		}

		public static readonly DependencyProperty TvDBSeasonProperty = DependencyProperty.Register("TvDBSeason",
			typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

		public int TvDBSeason
		{
			get { return (int)GetValue(TvDBSeasonProperty); }
			set { SetValue(TvDBSeasonProperty, value); }
		}

		public static readonly DependencyProperty TvDBSeriesNameProperty = DependencyProperty.Register("TvDBSeriesName",
			typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

		public string TvDBSeriesName
		{
			get { return (string)GetValue(TvDBSeriesNameProperty); }
			set { SetValue(TvDBSeriesNameProperty, value); }
		}

		public static readonly DependencyProperty TvDBURLProperty = DependencyProperty.Register("TvDBURL",
			typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

		public string TvDBURL
		{
			get { return (string)GetValue(TvDBURLProperty); }
			set { SetValue(TvDBURLProperty, value); }
		}

		public SelectTvDBSeasonForm()
		{
			InitializeComponent();

			btnClose.Click += new RoutedEventHandler(btnClose_Click);
			btnUpdate.Click += new RoutedEventHandler(btnUpdate_Click);
		}

		void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (cboSeasonNumber.Items.Count == 0)
				{
					MessageBox.Show("No seasons available, check the TvDB ID again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				this.Cursor = Cursors.Wait;

				TvDBSeason = int.Parse(cboSeasonNumber.SelectedItem.ToString());

				string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTvDB(AnimeID, TvDBID, TvDBSeason);
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

		public void Init(int animeID, string animeName, int tvDBID, int tvSeason, string tvSeriesName)
		{
			AnimeID = animeID;
			AnimeName = animeName;
			TvDBID = tvDBID;
			TvDBSeason = tvSeason;
			TvDBSeriesName = tvSeriesName;

			AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
			TvDBURL = string.Format(Constants.URLS.TvDB_Series, TvDBID);

			// get the seasons

			try
			{
				cboSeasonNumber.Items.Clear();
				List<int> seasons = JMMServerVM.Instance.clientBinaryHTTP.GetSeasonNumbersForSeries(tvDBID);
				int i = 0;
				int idx = 0;
				foreach (int season in seasons)
				{
					cboSeasonNumber.Items.Add(season.ToString());
					if (season == tvSeason) idx = i;
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
