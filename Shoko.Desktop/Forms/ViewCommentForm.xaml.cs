using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for ViewCommentForm.xaml
    /// </summary>
    public partial class ViewCommentForm : Window
    {
        public ViewCommentForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
        }

        public void Init(object comment)
        {
            try
            {
                if (comment.GetType() == typeof(VM_Trakt_CommentUser))
                {
                    VM_Trakt_CommentUser trakt = comment as VM_Trakt_CommentUser;

                    txtFrom.Text = Shoko.Commons.Properties.Resources.ViewComment_FromTrakt;
                    txtUsername.Text = trakt.User.Username;
                    txtDate.Text = ((VM_Trakt_Comment)trakt.Comment).CommentDateString;
                    txtComment.Text = trakt.CommentText;

                    urlWebsite.URL = trakt.Comment.Comment_Url;

                }
                else if (comment.GetType() == typeof(VM_AniDB_Recommendation))
                {
                    VM_AniDB_Recommendation anidb = comment as VM_AniDB_Recommendation;

                    txtFrom.Text = Shoko.Commons.Properties.Resources.ViewComment_FromAniDB;
                    txtUsername.Text = anidb.UserID.ToString();
                    txtDate.Text = anidb.GetRecommendationTypeText();
                    txtComment.Text = anidb.GetComment();
                    
                    urlWebsite.URL = $"http://anidb.net/perl-bin/animedb.pl?show=threads&do=anime&id={anidb.AnimeID}";


                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
