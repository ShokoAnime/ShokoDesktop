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
	/// Interaction logic for SelectMALStartForm.xaml
	/// </summary>
	public partial class SelectMALStartForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
			typeof(int), typeof(SelectMALStartForm), new UIPropertyMetadata(0, null));

		public int AnimeID
		{
			get { return (int)GetValue(AnimeIDProperty); }
			set { SetValue(AnimeIDProperty, value); }
		}

		public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
			typeof(string), typeof(SelectMALStartForm), new UIPropertyMetadata("", null));

		public string AnimeName
		{
			get { return (string)GetValue(AnimeNameProperty); }
			set { SetValue(AnimeNameProperty, value); }
		}

		public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
			typeof(string), typeof(SelectMALStartForm), new UIPropertyMetadata("", null));

		public string AnimeURL
		{
			get { return (string)GetValue(AnimeURLProperty); }
			set { SetValue(AnimeURLProperty, value); }
		}

		public static readonly DependencyProperty MALIDProperty = DependencyProperty.Register("MALID",
			typeof(int), typeof(SelectMALStartForm), new UIPropertyMetadata(0, null));

		public int MALID
		{
			get { return (int)GetValue(MALIDProperty); }
			set { SetValue(MALIDProperty, value); }
		}

		public static readonly DependencyProperty MALTitleProperty = DependencyProperty.Register("MALTitle",
			typeof(string), typeof(SelectMALStartForm), new UIPropertyMetadata("", null));

		public string MALTitle
		{
			get { return (string)GetValue(MALTitleProperty); }
			set { SetValue(MALTitleProperty, value); }
		}


		public static readonly DependencyProperty MALURLProperty = DependencyProperty.Register("MALURL",
			typeof(string), typeof(SelectMALStartForm), new UIPropertyMetadata("", null));

		public string MALURL
		{
			get { return (string)GetValue(MALURLProperty); }
			set { SetValue(MALURLProperty, value); }
		}

		public SelectMALStartForm()
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

				string selType = cboEpisodeType.SelectedItem.ToString();
				int epType = (int)EnumTranslator.EpisodeTypeTranslatedReverse(selType);

				int epNumber = 0;
				int.TryParse(txtEpNumber.Text, out epNumber);
				if (epNumber <= 0 || epNumber > 2500)
				{
					MessageBox.Show("Please enter a valid episode number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtEpNumber.Focus();
					return;
				}

				string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBMAL(AnimeID, MALID, MALTitle, epType, epNumber);
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

		public void Init(int animeID, string animeName, string malTitle, int malID)
		{
			AnimeID = animeID;
			AnimeName = animeName;
			MALTitle = malTitle;
			MALID = malID;

			AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
			MALURL = string.Format(Constants.URLS.MAL_Series, MALID);

			// get the seasons

			try
			{
				cboEpisodeType.Items.Clear();
				
				cboEpisodeType.Items.Add(EnumTranslator.EpisodeTypeTranslated(EpisodeType.Episode));
				cboEpisodeType.Items.Add(EnumTranslator.EpisodeTypeTranslated(EpisodeType.Special));

				cboEpisodeType.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}
