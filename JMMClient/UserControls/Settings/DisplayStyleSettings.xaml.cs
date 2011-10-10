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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DisplayStyleSettings.xaml
	/// </summary>
	public partial class DisplayStyleSettings : UserControl
	{
		public DisplayStyleSettings()
		{
			InitializeComponent();

			cboStyleGroupList.Items.Clear();
			cboStyleGroupList.Items.Add(Properties.Resources.Style_GroupList_MediumDetail);
			cboStyleGroupList.Items.Add(Properties.Resources.Style_GroupList_Simple);

			switch (AppSettings.DisplayStyle_GroupList)
			{
				case 1:
					cboStyleGroupList.SelectedIndex = 0;
					break;

				case 2:
					cboStyleGroupList.SelectedIndex = 1;
					break;

				default:
					cboStyleGroupList.SelectedIndex = 0;
					break;
			}

			cboStyleGroupList.SelectionChanged += new SelectionChangedEventHandler(cboStyleGroupList_SelectionChanged);

			cboStyleEpisodeDetail.Items.Clear();
			cboStyleEpisodeDetail.Items.Add(Properties.Resources.EpisodeDetailStyle_Option1);
			cboStyleEpisodeDetail.Items.Add(Properties.Resources.EpisodeDetailStyle_Option2);
			cboStyleEpisodeDetail.Items.Add(Properties.Resources.EpisodeDetailStyle_Option3);

			switch (AppSettings.EpisodeImageOverviewStyle)
			{
				case 1:
					cboStyleEpisodeDetail.SelectedIndex = 0;
					break;

				case 2:
					cboStyleEpisodeDetail.SelectedIndex = 1;
					break;

				case 3:
					cboStyleEpisodeDetail.SelectedIndex = 2;
					break;

				default:
					cboStyleGroupList.SelectedIndex = 0;
					break;
			}

			cboStyleEpisodeDetail.SelectionChanged += new SelectionChangedEventHandler(cboStyleEpisodeDetail_SelectionChanged);
		}

		void cboStyleEpisodeDetail_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			
			switch (cboStyleEpisodeDetail.SelectedIndex)
			{
				case 0: UserSettingsVM.Instance.EpisodeImageOverviewStyle = 1; break;
				case 1: UserSettingsVM.Instance.EpisodeImageOverviewStyle = 2; break;
				case 2: UserSettingsVM.Instance.EpisodeImageOverviewStyle = 3; break;
				default: UserSettingsVM.Instance.EpisodeImageOverviewStyle = 1; break;	
			}
		}

		void cboStyleGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboStyleGroupList.SelectedIndex)
			{
				case 0: UserSettingsVM.Instance.DisplayStyle_GroupList = 1; break;
				case 1: UserSettingsVM.Instance.DisplayStyle_GroupList = 2; break;
				default: UserSettingsVM.Instance.DisplayStyle_GroupList = 1; break;
			}
		}
	}
}
