using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for RelatedAnimeControl.xaml
    /// </summary>
    public partial class RelatedAnimeControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private BackgroundWorker missingDataWorker = new BackgroundWorker();

        public static readonly DependencyProperty DataExistsProperty = DependencyProperty.Register("DataExists",
            typeof(bool), typeof(RelatedAnimeControl), new UIPropertyMetadata(false, null));

        public bool DataExists
        {
            get { return (bool)GetValue(DataExistsProperty); }
            set { SetValue(DataExistsProperty, value); }
        }

        public static readonly DependencyProperty DataMissingProperty = DependencyProperty.Register("DataMissing",
            typeof(bool), typeof(RelatedAnimeControl), new UIPropertyMetadata(false, null));

        public bool DataMissing
        {
            get { return (bool)GetValue(DataMissingProperty); }
            set { SetValue(DataMissingProperty, value); }
        }

        public string SeriesName
        {
            get
            {
                if (DataContext == null) { return ""; }
                VM_AnimeSeries_User series = (VM_AnimeSeries_User)DataContext;
                return series.SeriesName + " " + Properties.Resources.NoRelatedAnime;
            }
        }

        public ObservableCollection<VM_AniDB_Anime_Relation> RelatedAnimeLinks { get; set; }

        public RelatedAnimeControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            RelatedAnimeLinks = new ObservableCollection<VM_AniDB_Anime_Relation>();
        
            DataContextChanged += new DependencyPropertyChangedEventHandler(RelatedAnimeControl_DataContextChanged);

            btnGetRelMissingInfo.Click += new RoutedEventHandler(btnGetRelMissingInfo_Click);

            missingDataWorker.DoWork += new DoWorkEventHandler(missingDataWorker_DoWork);
            missingDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(missingDataWorker_RunWorkerCompleted);
        }

        void missingDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Window wdw = Window.GetWindow(this);
            wdw.Cursor = Cursors.Arrow;
            wdw.IsEnabled = true;
        }

        void missingDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (VM_AniDB_Anime_Relation rel in RelatedAnimeLinks)
                {
                    if (!rel.AnimeInfoExists)
                    {
                        string result = VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(rel.RelatedAnimeID);
                        if (string.IsNullOrEmpty(result))
                        {
                            VM_AniDB_Anime animeContract = (VM_AniDB_Anime)VM_ShokoServer.Instance.ShokoServices.GetAnime(rel.RelatedAnimeID);
                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)delegate ()
                            {
                                rel.PopulateAnime(animeContract);
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void RelatedAnimeControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public void RefreshData()
        {
            try
            {
                VM_AniDB_Anime anime = null;

                VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
                if (animeSeries == null)
                {
                    RelatedAnimeLinks.Clear();
                    return;
                }
                RelatedAnimeLinks.Clear();
                anime = animeSeries.AniDBAnime.AniDBAnime;

                if (anime == null) return;



                List<VM_AniDB_Anime_Relation> tempList = VM_ShokoServer.Instance.ShokoServices.GetRelatedAnimeLinks(anime.AnimeID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AniDB_Anime_Relation>().OrderBy(a => a.SortPriority).ToList();

                foreach (VM_AniDB_Anime_Relation rel in tempList)
                    RelatedAnimeLinks.Add(rel);

                if (RelatedAnimeLinks.Count == 0)
                {
                    DataExists = false;
                    DataMissing = true;
                }
                else
                {
                    DataExists = true;
                    DataMissing = false;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void GetMissingSimilarData()
        {
            Window wdw = Window.GetWindow(this);
            wdw.Cursor = Cursors.Wait;
            wdw.IsEnabled = false;

            missingDataWorker.RunWorkerAsync();
        }

        void btnGetRelMissingInfo_Click(object sender, RoutedEventArgs e)
        {

            GetMissingSimilarData();
        }
    }
}
