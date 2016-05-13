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
using JMMClient.ViewModel;
using System.Threading;
using System.Globalization;

namespace JMMClient.Forms
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
                if (comment.GetType() == typeof(Trakt_CommentUserVM))
                {
                    Trakt_CommentUserVM trakt = comment as Trakt_CommentUserVM;

                    txtFrom.Text = Properties.Resources.ViewComment_FromTrakt;
                    txtUsername.Text = trakt.User.Username;
                    txtDate.Text = trakt.Comment.CommentDateString;
                    txtComment.Text = trakt.CommentText;

                    urlWebsite.URL = trakt.Comment.Comment_Url;

                }
                else if (comment.GetType() == typeof(AniDB_RecommendationVM))
                {
                    AniDB_RecommendationVM anidb = comment as AniDB_RecommendationVM;

                    txtFrom.Text = Properties.Resources.ViewComment_FromAniDB;
                    txtUsername.Text = anidb.UserID.ToString();
                    txtDate.Text = anidb.RecommendationTypeText;
                    txtComment.Text = anidb.Comment;

                    urlWebsite.URL = string.Format("http://anidb.net/perl-bin/animedb.pl?show=threads&do=anime&id={0}", anidb.AnimeID);

                    
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
