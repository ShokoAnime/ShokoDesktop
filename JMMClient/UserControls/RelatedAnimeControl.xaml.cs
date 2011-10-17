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
using NLog;
using JMMClient.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace JMMClient.UserControls
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

		public ObservableCollection<AniDB_Anime_RelationVM> RelatedAnimeLinks { get; set; }

		public RelatedAnimeControl()
		{
			InitializeComponent();

			RelatedAnimeLinks = new ObservableCollection<AniDB_Anime_RelationVM>();


			this.DataContextChanged += new DependencyPropertyChangedEventHandler(RelatedAnimeControl_DataContextChanged);

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
				foreach (AniDB_Anime_RelationVM rel in RelatedAnimeLinks)
				{
					if (rel.AnimeInfoNotExists)
					{
						string result = JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(rel.RelatedAnimeID);
						if (string.IsNullOrEmpty(result))
						{
							JMMServerBinary.Contract_AniDBAnime animeContract = JMMServerVM.Instance.clientBinaryHTTP.GetAnime(rel.RelatedAnimeID);
							System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)delegate()
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
				AniDB_AnimeVM anime = null;

				AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
				if (animeSeries == null)
				{
					RelatedAnimeLinks.Clear();
					return;
				}
				RelatedAnimeLinks.Clear();
				anime = animeSeries.AniDB_Anime;

				if (anime == null) return;


				List<JMMServerBinary.Contract_AniDB_Anime_Relation> links = JMMServerVM.Instance.clientBinaryHTTP.GetRelatedAnimeLinks(anime.AnimeID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);


				List<AniDB_Anime_RelationVM> tempList = new List<AniDB_Anime_RelationVM>();
				foreach (JMMServerBinary.Contract_AniDB_Anime_Relation link in links)
				{
					AniDB_Anime_RelationVM rel = new AniDB_Anime_RelationVM();
					rel.Populate(link);
					tempList.Add(rel);
				}

				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("SortPriority", false, SortType.eInteger));
				tempList = Sorting.MultiSort<AniDB_Anime_RelationVM>(tempList, sortCriteria);

				foreach (AniDB_Anime_RelationVM rel in tempList)
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
	}
}
