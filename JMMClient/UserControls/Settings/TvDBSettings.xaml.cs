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
using Microsoft.Windows.Controls;
using JMMClient.Forms;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for TvDBSettings.xaml
	/// </summary>
	public partial class TvDBSettings : UserControl
	{
		public TvDBSettings()
		{
			InitializeComponent();

			chkTvDB_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
			chkTvDB_PosterAutoDownload.Click += new RoutedEventHandler(settingChanged);
			chkTvDB_WideBannerAutoDownload.Click += new RoutedEventHandler(settingChanged);
			udMaxFanarts.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxFanarts_ValueChanged);
			udMaxPosters.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxPosters_ValueChanged);
			udMaxWideBanners.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxWideBanners_ValueChanged);

			cboUpdateFrequency.Items.Clear();
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.TvDB_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequency.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequency.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequency.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequency.SelectedIndex = 3; break;
			}

			cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

			btnChangeLanguage.Click += new RoutedEventHandler(btnChangeLanguage_Click);
		}

		

		void btnChangeLanguage_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Window wdw = Window.GetWindow(this);

				wdw.Cursor = Cursors.Wait;
				List<JMMServerBinary.Contract_TvDBLanguage> lans = JMMServerVM.Instance.clientBinaryHTTP.GetTvDBLanguages();
				List<TvDB_LanguageVM> languages = new List<TvDB_LanguageVM>();
				foreach (JMMServerBinary.Contract_TvDBLanguage lan in lans)
					languages.Add(new TvDB_LanguageVM(lan));
				wdw.Cursor = Cursors.Arrow;

				SelectTvDBLanguage frm = new SelectTvDBLanguage();
				frm.Owner = wdw;
				frm.Init(languages);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					// update info
					JMMServerVM.Instance.TvDB_Language = frm.SelectedLanguage;
					JMMServerVM.Instance.SaveServerSettingsAsync();
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequency.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void udMaxFanarts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			JMMServerVM.Instance.TvDB_AutoFanartAmount = udMaxFanarts.Value.Value;
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void udMaxWideBanners_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			JMMServerVM.Instance.TvDB_AutoWideBannersAmount = udMaxWideBanners.Value.Value;
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void udMaxPosters_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			JMMServerVM.Instance.TvDB_AutoPostersAmount = udMaxPosters.Value.Value;
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}
	}
}
