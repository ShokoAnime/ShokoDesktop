using System.Windows.Controls;

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


            cboStyleEpisodeTitle.Items.Clear();
            cboStyleEpisodeTitle.Items.Add("AniDB");
            cboStyleEpisodeTitle.Items.Add("The TvDB");
            switch (JMMServerVM.Instance.EpisodeTitleSource)
            {
                case DataSourceType.AniDB:
                    cboStyleEpisodeTitle.SelectedIndex = 0;
                    break;

                case DataSourceType.TheTvDB:
                    cboStyleEpisodeTitle.SelectedIndex = 1;
                    break;
            }
            cboStyleEpisodeTitle.SelectionChanged += new SelectionChangedEventHandler(cboStyleEpisodeTitle_SelectionChanged);


            cboStyleSeriesOverview.Items.Clear();
            cboStyleSeriesOverview.Items.Add("AniDB");
            cboStyleSeriesOverview.Items.Add("The TvDB");
            switch (JMMServerVM.Instance.SeriesDescriptionSource)
            {
                case DataSourceType.AniDB:
                    cboStyleSeriesOverview.SelectedIndex = 0;
                    break;

                case DataSourceType.TheTvDB:
                    cboStyleSeriesOverview.SelectedIndex = 1;
                    break;
            }
            cboStyleSeriesOverview.SelectionChanged += new SelectionChangedEventHandler(cboStyleSeriesOverview_SelectionChanged);

            cboStyleSeriesName.Items.Clear();
            cboStyleSeriesName.Items.Add("AniDB");
            cboStyleSeriesName.Items.Add("The TvDB");
            switch (JMMServerVM.Instance.SeriesNameSource)
            {
                case DataSourceType.AniDB:
                    cboStyleSeriesName.SelectedIndex = 0;
                    break;

                case DataSourceType.TheTvDB:
                    cboStyleSeriesName.SelectedIndex = 1;
                    break;
            }
            cboStyleSeriesName.SelectionChanged += new SelectionChangedEventHandler(cboStyleSeriesName_SelectionChanged);
        }

        void cboStyleSeriesName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleSeriesName.SelectedIndex)
            {
                case 0: JMMServerVM.Instance.SeriesNameSource = DataSourceType.AniDB; break;
                case 1: JMMServerVM.Instance.SeriesNameSource = DataSourceType.TheTvDB; break;
                default: JMMServerVM.Instance.SeriesNameSource = DataSourceType.AniDB; break;
            }
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void cboStyleSeriesOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleSeriesOverview.SelectedIndex)
            {
                case 0: JMMServerVM.Instance.SeriesDescriptionSource = DataSourceType.AniDB; break;
                case 1: JMMServerVM.Instance.SeriesDescriptionSource = DataSourceType.TheTvDB; break;
                default: JMMServerVM.Instance.SeriesDescriptionSource = DataSourceType.AniDB; break;
            }
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void cboStyleEpisodeTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboStyleEpisodeTitle.SelectedIndex)
            {
                case 0: JMMServerVM.Instance.EpisodeTitleSource = DataSourceType.AniDB; break;
                case 1: JMMServerVM.Instance.EpisodeTitleSource = DataSourceType.TheTvDB; break;
                default: JMMServerVM.Instance.EpisodeTitleSource = DataSourceType.AniDB; break;
            }
            JMMServerVM.Instance.SaveServerSettingsAsync();
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
