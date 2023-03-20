using System.Windows.Controls;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Settings
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
            cboStyleGroupList.Items.Add(Shoko.Commons.Properties.Resources.Style_GroupList_MediumDetail);
            cboStyleGroupList.Items.Add(Shoko.Commons.Properties.Resources.Style_GroupList_Simple);

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
            cboStyleEpisodeDetail.Items.Add(Shoko.Commons.Properties.Resources.EpisodeDetailStyle_Option1);
            cboStyleEpisodeDetail.Items.Add(Shoko.Commons.Properties.Resources.EpisodeDetailStyle_Option2);
            cboStyleEpisodeDetail.Items.Add(Shoko.Commons.Properties.Resources.EpisodeDetailStyle_Option3);

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


            cboStyleEpisodeTitle.Items.Clear();
            cboStyleEpisodeTitle.Items.Add("AniDB");
            cboStyleEpisodeTitle.Items.Add("The TvDB");
            switch (VM_ShokoServer.Instance.EpisodeTitleSource)
            {
                case DataSourceType.AniDB:
                    cboStyleEpisodeTitle.SelectedIndex = 0;
                    break;

                case DataSourceType.TvDB:
                    cboStyleEpisodeTitle.SelectedIndex = 1;
                    break;
            }
            cboStyleEpisodeTitle.SelectionChanged += new SelectionChangedEventHandler(cboStyleEpisodeTitle_SelectionChanged);


            cboStyleSeriesOverview.Items.Clear();
            cboStyleSeriesOverview.Items.Add("AniDB");
            cboStyleSeriesOverview.Items.Add("The TvDB");
            switch (VM_ShokoServer.Instance.SeriesDescriptionSource)
            {
                case DataSourceType.AniDB:
                    cboStyleSeriesOverview.SelectedIndex = 0;
                    break;

                case DataSourceType.TvDB:
                    cboStyleSeriesOverview.SelectedIndex = 1;
                    break;
            }
            cboStyleSeriesOverview.SelectionChanged += new SelectionChangedEventHandler(cboStyleSeriesOverview_SelectionChanged);

            cboStyleSeriesName.Items.Clear();
            cboStyleSeriesName.Items.Add("AniDB");
            cboStyleSeriesName.Items.Add("The TvDB");
            switch (VM_ShokoServer.Instance.SeriesNameSource)
            {
                case DataSourceType.AniDB:
                    cboStyleSeriesName.SelectedIndex = 0;
                    break;

                case DataSourceType.TvDB:
                    cboStyleSeriesName.SelectedIndex = 1;
                    break;
            }
            cboStyleSeriesName.SelectionChanged += new SelectionChangedEventHandler(cboStyleSeriesName_SelectionChanged);
        }

        void cboStyleSeriesName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleSeriesName.SelectedIndex)
            {
                case 0: VM_ShokoServer.Instance.SeriesNameSource = DataSourceType.AniDB; break;
                case 1: VM_ShokoServer.Instance.SeriesNameSource = DataSourceType.TvDB; break;
                default: VM_ShokoServer.Instance.SeriesNameSource = DataSourceType.AniDB; break;
            }
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboStyleSeriesOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleSeriesOverview.SelectedIndex)
            {
                case 0: VM_ShokoServer.Instance.SeriesDescriptionSource = DataSourceType.AniDB; break;
                case 1: VM_ShokoServer.Instance.SeriesDescriptionSource = DataSourceType.TvDB; break;
                default: VM_ShokoServer.Instance.SeriesDescriptionSource = DataSourceType.AniDB; break;
            }
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboStyleEpisodeTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleEpisodeTitle.SelectedIndex)
            {
                case 0: VM_ShokoServer.Instance.EpisodeTitleSource = DataSourceType.AniDB; break;
                case 1: VM_ShokoServer.Instance.EpisodeTitleSource = DataSourceType.TvDB; break;
                default: VM_ShokoServer.Instance.EpisodeTitleSource = DataSourceType.AniDB; break;
            }
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboStyleEpisodeDetail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (cboStyleEpisodeDetail.SelectedIndex)
            {
                case 0: VM_UserSettings.Instance.EpisodeImageOverviewStyle = 1; break;
                case 1: VM_UserSettings.Instance.EpisodeImageOverviewStyle = 2; break;
                case 2: VM_UserSettings.Instance.EpisodeImageOverviewStyle = 3; break;
                default: VM_UserSettings.Instance.EpisodeImageOverviewStyle = 1; break;
            }
        }

        void cboStyleGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleGroupList.SelectedIndex)
            {
                case 0: VM_UserSettings.Instance.DisplayStyle_GroupList = 1; break;
                case 1: VM_UserSettings.Instance.DisplayStyle_GroupList = 2; break;
                default: VM_UserSettings.Instance.DisplayStyle_GroupList = 1; break;
            }
        }
    }
}
