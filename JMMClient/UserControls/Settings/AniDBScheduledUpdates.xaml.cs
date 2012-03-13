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
	/// Interaction logic for AniDBScheduledUpdates.xaml
	/// </summary>
	public partial class AniDBScheduledUpdates : UserControl
	{
		public AniDBScheduledUpdates()
		{
			InitializeComponent();

			cboUpdateFrequencyCalendar.Items.Clear();
			cboUpdateFrequencyCalendar.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequencyCalendar.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequencyCalendar.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequencyCalendar.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequencyCalendar.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequencyCalendar.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequencyCalendar.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequencyCalendar.SelectedIndex = 3; break;
			}

			cboUpdateFrequencyCalendar.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyCalendar_SelectionChanged);


			cboUpdateFrequencyAnime.Items.Clear();
			cboUpdateFrequencyAnime.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequencyAnime.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequencyAnime.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequencyAnime.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.AniDB_Anime_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequencyAnime.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequencyAnime.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequencyAnime.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequencyAnime.SelectedIndex = 3; break;
			}

			cboUpdateFrequencyAnime.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyAnime_SelectionChanged);

			cboUpdateFrequencyMyList.Items.Clear();
			cboUpdateFrequencyMyList.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequencyMyList.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequencyMyList.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequencyMyList.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.AniDB_MyList_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequencyMyList.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequencyMyList.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequencyMyList.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequencyMyList.SelectedIndex = 3; break;
			}

			cboUpdateFrequencyMyList.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyMyList_SelectionChanged);




			cboUpdateFrequencyMyListStats.Items.Clear();
			cboUpdateFrequencyMyListStats.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequencyMyListStats.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequencyMyListStats.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequencyMyListStats.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequencyMyListStats.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequencyMyListStats.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequencyMyListStats.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequencyMyListStats.SelectedIndex = 3; break;
			}

			cboUpdateFrequencyMyListStats.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyMyListStats_SelectionChanged);
		}

		void cboUpdateFrequencyMyListStats_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyMyListStats.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyMyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyMyList.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyAnime.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyCalendar_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyCalendar.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

	}
}
