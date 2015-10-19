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

            Utils.PopulateScheduledComboBox(cboUpdateFrequencyCalendar, JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency);
			cboUpdateFrequencyCalendar.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyCalendar_SelectionChanged);

            Utils.PopulateScheduledComboBox(cboUpdateFrequencyAnime, JMMServerVM.Instance.AniDB_Anime_UpdateFrequency);
			cboUpdateFrequencyAnime.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyAnime_SelectionChanged);

            Utils.PopulateScheduledComboBox(cboUpdateFrequencyMyList, JMMServerVM.Instance.AniDB_MyList_UpdateFrequency);
			cboUpdateFrequencyMyList.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyMyList_SelectionChanged);

            Utils.PopulateScheduledComboBox(cboUpdateFrequencyMyListStats, JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency);
			cboUpdateFrequencyMyListStats.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyMyListStats_SelectionChanged);

            Utils.PopulateScheduledComboBox(cboUpdateFrequencyAniDBFiles, JMMServerVM.Instance.AniDB_File_UpdateFrequency);
			cboUpdateFrequencyAniDBFiles.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequencyAniDBFiles_SelectionChanged);
		}

        

		void cboUpdateFrequencyAniDBFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyAniDBFiles.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.AniDB_File_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.AniDB_File_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.AniDB_File_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.AniDB_File_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.AniDB_File_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.AniDB_File_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyMyListStats_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyMyListStats.SelectedIndex)
			{
                case 0: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.AniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
            }

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyMyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyMyList.SelectedIndex)
			{
                case 0: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.AniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyAnime.SelectedIndex)
			{
                case 0: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.AniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequencyCalendar_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequencyCalendar.SelectedIndex)
			{
                case 0: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.AniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

	}
}
