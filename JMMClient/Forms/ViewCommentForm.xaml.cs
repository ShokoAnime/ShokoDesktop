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
        }

        public void Init(object comment)
        {
            try
            {
                if (comment.GetType() == typeof(Trakt_ShoutUserVM))
                {
                    Trakt_ShoutUserVM trakt = comment as Trakt_ShoutUserVM;

                    txtFrom.Text = "From Trakt";
                    txtUsername.Text = trakt.User.Username;
                    txtDate.Text = trakt.Shout.ShoutDateString;
                    txtComment.Text = trakt.Comment;

                    urlWebsite.URL = trakt.Shout.Comment_Url;

                }
                else if (comment.GetType() == typeof(AniDB_RecommendationVM))
                {
                    AniDB_RecommendationVM anidb = comment as AniDB_RecommendationVM;

                    txtFrom.Text = "From AniDB";
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
