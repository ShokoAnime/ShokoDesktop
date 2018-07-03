using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for TvDBMatchPreview.xaml
    /// </summary>
    public partial class TvDBMatchPreview : Window
    {
        private VM_TvDBDetails TvDetails;
        private bool IsAdditive;

        public static readonly DependencyProperty MatchesProperty = DependencyProperty.Register("Matches",
            typeof(List<VM_CrossRef_AniDB_TvDB_Episode>), typeof(TvDBMatchPreview), new UIPropertyMetadata(null, null));

        public List<VM_CrossRef_AniDB_TvDB_Episode> Matches
        {
            get => (List<VM_CrossRef_AniDB_TvDB_Episode>) GetValue(MatchesProperty);
            set => SetValue(MatchesProperty, value);
        }

        public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
            typeof(int), typeof(TvDBMatchPreview), new UIPropertyMetadata(0, null));

        public int AnimeID
        {
            get => (int) GetValue(AnimeIDProperty);
            set => SetValue(AnimeIDProperty, value);
        }

        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register("IsPreview",
            typeof(bool), typeof(TvDBMatchPreview), new UIPropertyMetadata(false, null));

        public bool IsPreview
        {
            get => (bool) GetValue(IsPreviewProperty);
            set => SetValue(IsPreviewProperty, value);
        }

        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
            typeof(string), typeof(TvDBMatchPreview), new UIPropertyMetadata("", null));

        public string AnimeName
        {
            get => (string) GetValue(AnimeNameProperty);
            set => SetValue(AnimeNameProperty, value);
        }

        public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
            typeof(string), typeof(TvDBMatchPreview), new UIPropertyMetadata("", null));

        public string AnimeURL
        {
            get => (string) GetValue(AnimeURLProperty);
            set => SetValue(AnimeURLProperty, value);
        }

        public static readonly DependencyProperty TvDBIDProperty = DependencyProperty.Register("TvDBID",
            typeof(int), typeof(TvDBMatchPreview), new UIPropertyMetadata(0, null));

        public int TvDBID
        {
            get => (int) GetValue(TvDBIDProperty);
            set => SetValue(TvDBIDProperty, value);
        }

        public static readonly DependencyProperty TvDBSeriesNameProperty = DependencyProperty.Register("TvDBSeriesName",
            typeof(string), typeof(TvDBMatchPreview), new UIPropertyMetadata("", null));

        public string TvDBSeriesName
        {
            get => (string)GetValue(TvDBSeriesNameProperty);
            set => SetValue(TvDBSeriesNameProperty, value);
        }

        public static readonly DependencyProperty TvDBURLProperty = DependencyProperty.Register("TvDBURL",
            typeof(string), typeof(TvDBMatchPreview), new UIPropertyMetadata("", null));

        public string TvDBURL
        {
            get => (string)GetValue(TvDBURLProperty);
            set => SetValue(TvDBURLProperty, value);
        }

        private static List<VM_AniDB_Episode> AniDBEpisodes = new List<VM_AniDB_Episode>();
        private static List<VM_TvDB_Episode> TvDBEpisodes = new List<VM_TvDB_Episode>();

        public TvDBMatchPreview()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnClose.Click += btnClose_Click;
            btnUpdate.Click += btnUpdate_Click;
        }

        void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;

                var xref = new CrossRef_AniDB_TvDBV2
                {
                    AnimeID = AnimeID,
                    TvDBID = TvDBID,
                    CrossRef_AniDB_TvDBV2ID = 0,
                    CrossRefSource = (int) CrossRefSource.User,
                    IsAdditive = IsAdditive
                };
                string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBTvDB(xref);
                if (res.Length > 0)
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Init(int animeID, string animeName, int tvDBID, string tvSeriesName, VM_AniDB_Anime anime, bool isAdditive, bool preview = false)
        {
            AnimeID = animeID;
            AnimeName = animeName;
            TvDBID = tvDBID;
            TvDBSeriesName = tvSeriesName;
            IsAdditive = isAdditive;
            IsPreview = preview;

            AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
            TvDBURL = string.Format(Constants.URLS.TvDB_Series, TvDBID);

            AniDBEpisodes = new List<VM_AniDB_Episode>();
            foreach (VM_AniDB_Episode contract in VM_ShokoServer.Instance.ShokoServices.GetAniDBEpisodesForAnime(AnimeID).Cast<VM_AniDB_Episode>()) AniDBEpisodes.Add(contract);

            // get the seasons

            try
            {
                TvDetails = anime.TvSummary.TvDetails.ContainsKey(tvDBID)
                    ? anime.TvSummary.TvDetails[tvDBID]
                    : new VM_TvDBDetails(tvDBID);

                TvDBEpisodes = TvDetails.TvDBEpisodes;

                Matches = VM_ShokoServer.Instance.ShokoServices.GetTvDBEpisodeMatchPreview(animeID, tvDBID).Where(a => a != null).Select(a =>
                    new VM_CrossRef_AniDB_TvDB_Episode(a, GetAniDBEpisode(a.AniDBEpisodeID),
                        GetTvDBEpisode(a.TvDBEpisodeID))).Where(a => a.IsValid).ToList();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public VM_AniDB_Episode GetAniDBEpisode(int id)
        {
            return AniDBEpisodes?.FirstOrDefault(a => a.EpisodeID == id);
        }

        public VM_TvDB_Episode GetTvDBEpisode(int id)
        {
            return TvDBEpisodes?.FirstOrDefault(a => a.Id == id);
        }

        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        private void anidb_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(dgAniDB, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(dgTvDB, typeof(ScrollViewer)) as ScrollViewer;
            if (_listboxScrollViewer1 != null && _listboxScrollViewer2 != null)
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void tvdb_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(dgAniDB, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(dgTvDB, typeof(ScrollViewer)) as ScrollViewer;
            if (_listboxScrollViewer1 != null && _listboxScrollViewer2 != null)
                _listboxScrollViewer1.ScrollToVerticalOffset(_listboxScrollViewer2.VerticalOffset);
        }

        public static Brush Green { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(178, 255, 186));
        public static Brush Lavender { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(236, 188, 255));
        public static Brush Red { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 198, 198));
        public static Brush Orange { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 223, 178));
        public static Brush White { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
    }

    public class VM_CrossRef_AniDB_TvDB_Episode : CrossRef_AniDB_TvDB_Episode
    {
        private VM_AniDB_Episode aniep;
        private VM_TvDB_Episode tvep;

        public VM_CrossRef_AniDB_TvDB_Episode(CrossRef_AniDB_TvDB_Episode baselink, VM_AniDB_Episode ani,
            VM_TvDB_Episode tv)
        {
            CrossRef_AniDB_TvDB_EpisodeID = baselink.CrossRef_AniDB_TvDB_EpisodeID;
            AniDBEpisodeID = baselink.AniDBEpisodeID;
            TvDBEpisodeID = baselink.TvDBEpisodeID;
            MatchRating = baselink.MatchRating;

            aniep = ani;
            tvep = tv;
        }

        public bool IsValid => aniep != null && tvep != null;

        public bool GoodRating => MatchRating == MatchRating.Good;

        public string AniDB_Type
        {
            get
            {
                switch (aniep.EpisodeType)
                {
                    case 1: return @"EP";
                    case 2: return @"C";
                    case 3: return @"S";
                    case 4: return @"T";
                    case 5: return @"P";
                    default: return @"O";
                }
            }
        }

        public string AniDB_Number => aniep.EpisodeNumber.ToString();
        public string AniDB_Name => aniep.EpisodeName;

        public string TvDB_Season => tvep.SeasonNumber.ToString();
        public string TvDB_Number => tvep.EpisodeNumber.ToString();
        public string TvDB_Name => tvep.EpisodeName;

        public Brush Color
        {
            get
            {
                switch (MatchRating)
                {
                    case MatchRating.UserVerified:
                        return TvDBMatchPreview.Lavender;
                    case MatchRating.Good:
                        return TvDBMatchPreview.Green;
                    case MatchRating.Mkay:
                    case MatchRating.Bad:
                        return TvDBMatchPreview.Orange;
                    case MatchRating.Ugly:
                    case MatchRating.SarahJessicaParker:
                        return TvDBMatchPreview.Red;
                }

                return TvDBMatchPreview.White;
            }
        }
    }
}

