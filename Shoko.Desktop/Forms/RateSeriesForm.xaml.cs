using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.UserControls;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for RateSeriesForm.xaml
    /// </summary>
    public partial class RateSeriesForm : Window
    {
        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
            typeof(VM_AnimeSeries_User), typeof(RateSeriesForm), new UIPropertyMetadata(null, null));

        public VM_AnimeSeries_User Series
        {
            get { return (VM_AnimeSeries_User)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty TraktLinkExistsProperty = DependencyProperty.Register("TraktLinkExists",
            typeof(bool), typeof(RateSeriesForm), new UIPropertyMetadata(false, null));

        public bool TraktLinkExists
        {
            get { return (bool)GetValue(TraktLinkExistsProperty); }
            set { SetValue(TraktLinkExistsProperty, value); }
        }

        public RateSeriesForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cRating.OnRatingValueChangedEvent += new RatingControl.RatingValueChangedHandler(cRating_OnRatingValueChangedEvent);
            DataContextChanged += new DependencyPropertyChangedEventHandler(RateSeriesForm_DataContextChanged);

            Loaded += new RoutedEventHandler(RateSeriesForm_Loaded);
        }

        void RateSeriesForm_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboVoteType.Items.Clear();
            cboVoteType.Items.Add(Shoko.Commons.Properties.Resources.VoteTypeAnimeTemporary);
            if (ser.AniDBAnime.AniDBAnime.FinishedAiring)
                cboVoteType.Items.Add(Shoko.Commons.Properties.Resources.VoteTypeAnimePermanent);

            if (ser.AniDBAnime.AniDBAnime.FinishedAiring && ser.AllFilesWatched)
                cboVoteType.SelectedIndex = 1;
            else
                cboVoteType.SelectedIndex = 0;

            TraktLinkExists = ser.AniDBAnime.AniDBAnime != null && ser.AniDBAnime.AniDBAnime.AniDB_AnimeCrossRefs != null && ser.AniDBAnime.AniDBAnime.AniDB_AnimeCrossRefs.TraktCrossRefExists;
        }

        private void CommandBinding_RevokeVote(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Parameter as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                VM_ShokoServer.Instance.RevokeVote(ser.AniDB_ID);
                VM_MainListHelper.Instance.UpdateHeirarchy(ser);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                decimal rating = (decimal)ev.RatingValue;

                int voteType = 1;
                if (cboVoteType.SelectedItem.ToString() == Shoko.Commons.Properties.Resources.VoteTypeAnimeTemporary) voteType = 2;
                if (cboVoteType.SelectedItem.ToString() == Shoko.Commons.Properties.Resources.VoteTypeAnimePermanent) voteType = 1;

                VM_ShokoServer.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

                // refresh the data
                VM_MainListHelper.Instance.UpdateHeirarchy(ser);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void RateSeriesForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (TraktLinkExists)
            {
                //this.Cursor = Cursors.Wait;
                ucTraktComments.RefreshComments();
                //this.Cursor = Cursors.Arrow;
            }
        }

        public void Init(VM_AnimeSeries_User series)
        {
            Series = series;
            DataContext = Series;
            ucTraktComments.DataContext = Series;
        }
    }
}
