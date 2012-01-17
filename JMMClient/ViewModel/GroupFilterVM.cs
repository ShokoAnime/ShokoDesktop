using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JMMClient.ViewModel;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace JMMClient
{
	public class GroupFilterVM : MainListWrapper, INotifyPropertyChanged, IComparable<GroupFilterVM>
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public int? GroupFilterID { get; set; }
		public bool AllowEditing { get; set; }

		public ObservableCollection<GroupFilterConditionVM> FilterConditions { get; set; }
		public ObservableCollection<GroupFilterSortingCriteria> SortCriteriaList { get; set; }

		private Boolean isLocked = true;
		public Boolean IsLocked
		{
			get { return isLocked; }
			set
			{
				isLocked = value;
				NotifyPropertyChanged("IsLocked");
			}
		}

		private Boolean isBeingEdited = false;
		public Boolean IsBeingEdited
		{
			get { return isBeingEdited; }
			set
			{
				isBeingEdited = value;
				NotifyPropertyChanged("IsBeingEdited");
			}
		}

		private string filterName = "";
		public string FilterName
		{
			get { return filterName; }
			set
			{
				filterName = value;
				NotifyPropertyChanged("FilterName");
			}
		}

		private int applyToSeries = 0;
		public int ApplyToSeries
		{
			get { return applyToSeries; }
			set
			{
				applyToSeries = value;
				IsApplyToSeries = applyToSeries == 1;
				NotifyPropertyChanged("ApplyToSeries");
			}
		}

		private Boolean isApplyToSeries = false;
		public Boolean IsApplyToSeries
		{
			get { return isApplyToSeries; }
			set
			{
				isApplyToSeries = value;
				NotifyPropertyChanged("IsApplyToSeries");
			}
		}

		private int baseCondition = 0;
		public int BaseCondition
		{
			get { return baseCondition; }
			set
			{
				baseCondition = value;
				NotifyPropertyChanged("BaseCondition");
			}
		}

		public string Summary
		{
			get
			{
				int groupsCount = GroupsCount;
				string summ = "";
				if (groupsCount > 0)
					summ = string.Format("{0} Groups", groupsCount);

				return summ;
			}
		}

		public int GroupsCount
		{
			get
			{
				return GetDirectChildren().Count;
			}
		}

		public GroupFilterVM()
		{
			GroupFilterID = null;
			this.SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();
			this.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();
		}


		public GroupFilterVM(JMMServerBinary.Contract_GroupFilter contract)
		{
			this.SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();
			this.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();

			// read only members
			Populate(contract);
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", GroupFilterID, FilterName);
		}

		public bool EvaluateGroupFilter(AnimeGroupVM grp)
		{
			// sub groups don't count
			if (grp.AnimeGroupParentID.HasValue) return false;

			// make sure the user has not filtered this out
			if (!JMMServerVM.Instance.CurrentUser.EvaluateGroup(grp)) return false;

			// first check for anime groups which are included exluded every time
			foreach (GroupFilterConditionVM gfc in FilterConditions)
			{
				if (gfc.ConditionTypeEnum != GroupFilterConditionType.AnimeGroup) continue;

				int groupID = 0;
				int.TryParse(gfc.ConditionParameter, out groupID);
				if (groupID == 0) break;

				if (gfc.ConditionOperatorEnum == GroupFilterOperator.Equals)
					if (groupID == grp.AnimeGroupID.Value) return true;

				if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotEquals)
					if (groupID == grp.AnimeGroupID.Value) return false;
			}

			if (this.BaseCondition == (int)GroupFilterBaseCondition.Exclude) return false;

			// now check other conditions
			foreach (GroupFilterConditionVM gfc in FilterConditions)
			{
				switch (gfc.ConditionTypeEnum)
				{
					case GroupFilterConditionType.Favourite:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.IsFave == 0) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.IsFave == 1) return false;
						break;

					case GroupFilterConditionType.MissingEpisodes:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.HasMissingEpisodesAny == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.HasMissingEpisodesAny == true) return false;
						break;

					case GroupFilterConditionType.MissingEpisodesCollecting:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.HasMissingEpisodesGroups == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.HasMissingEpisodesGroups == true) return false;
						break;

					case GroupFilterConditionType.HasWatchedEpisodes:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.AnyFilesWatched == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.AnyFilesWatched == true) return false;
						break;

					case GroupFilterConditionType.HasUnwatchedEpisodes:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.HasUnwatchedFiles == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.HasUnwatchedFiles == true) return false;
						break;

					case GroupFilterConditionType.AssignedTvDBInfo:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.Stat_HasTvDBLink == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.Stat_HasTvDBLink == true) return false;
						break;


					case GroupFilterConditionType.AssignedMALInfo:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.Stat_HasMALLink == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.Stat_HasMALLink == true) return false;
						break;
					case GroupFilterConditionType.AssignedMovieDBInfo:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.Stat_HasMovieDBLink == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.Stat_HasMovieDBLink == true) return false;
						break;

					case GroupFilterConditionType.AssignedTvDBOrMovieDBInfo:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.Stat_HasMovieDBOrTvDBLink == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.Stat_HasMovieDBOrTvDBLink == true) return false;
						break;

					case GroupFilterConditionType.CompletedSeries:

						/*if (grp.IsComplete != grp.Stat_IsComplete)
						{
							Debug.Print("IsComplete DIFF  {0}", grp.GroupName);
						}*/

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.Stat_IsComplete == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.Stat_IsComplete == true) return false;
						break;

					case GroupFilterConditionType.FinishedAiring:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.Stat_HasFinishedAiring == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.Stat_HasFinishedAiring == true) return false;
						break;

					case GroupFilterConditionType.UserVoted:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.UserHasVoted == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.UserHasVoted == true) return false;
						break;

					case GroupFilterConditionType.UserVotedAny:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && grp.UserHasVotedAny == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && grp.UserHasVotedAny == true) return false;
						break;

					case GroupFilterConditionType.AirDate:
						DateTime filterDate;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDate = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDate = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (grp.AnimeGroupID.Value == 250)
							Console.Write("");

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							if (!grp.Stat_AirDate_Min.HasValue || !grp.Stat_AirDate_Max.HasValue) return false;
							if (grp.Stat_AirDate_Max.Value < filterDate) return false;
						}
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
						{
							if (!grp.Stat_AirDate_Min.HasValue || !grp.Stat_AirDate_Max.HasValue) return false;
							if (grp.Stat_AirDate_Min.Value > filterDate) return false;
						}
						break;

					case GroupFilterConditionType.SeriesCreatedDate:
						DateTime filterDateSeries;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDateSeries = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDateSeries = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							if (!grp.Stat_SeriesCreatedDate.HasValue) return false;
							if (grp.Stat_SeriesCreatedDate.Value < filterDateSeries) return false;
						}
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
						{
							if (!grp.Stat_SeriesCreatedDate.HasValue) return false;
							if (grp.Stat_SeriesCreatedDate.Value > filterDateSeries) return false;
						}
						break;

					case GroupFilterConditionType.EpisodeWatchedDate:
						DateTime filterDateEpsiodeWatched;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDateEpsiodeWatched = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDateEpsiodeWatched = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							if (!grp.WatchedDate.HasValue) return false;
							if (grp.WatchedDate.Value < filterDateEpsiodeWatched) return false;
						}
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
						{
							if (!grp.WatchedDate.HasValue) return false;
							if (grp.WatchedDate.Value > filterDateEpsiodeWatched) return false;
						}
						break;

					case GroupFilterConditionType.EpisodeAddedDate:
						DateTime filterDateEpisodeAdded;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDateEpisodeAdded = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDateEpisodeAdded = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							if (!grp.EpisodeAddedDate.HasValue) return false;
							if (grp.EpisodeAddedDate.Value < filterDateEpisodeAdded) return false;
						}
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
						{
							if (!grp.EpisodeAddedDate.HasValue) return false;
							if (grp.EpisodeAddedDate.Value > filterDateEpisodeAdded) return false;
						}
						break;

					case GroupFilterConditionType.AniDBRating:

						decimal dRating = -1;
						decimal.TryParse(gfc.ConditionParameter, out dRating);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan && grp.AniDBRating < dRating) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan && grp.AniDBRating > dRating) return false;
						break;

					case GroupFilterConditionType.UserRating:

						if (!grp.Stat_UserVoteOverall.HasValue) return false;

						decimal dUserRating = -1;
						decimal.TryParse(gfc.ConditionParameter, out dUserRating);

						if (grp.AnimeGroupID.Value == 122)
						{
							Debug.Write("");
						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan && grp.Stat_UserVoteOverall.Value < dUserRating) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan && grp.Stat_UserVoteOverall.Value > dUserRating) return false;
						break;

					case GroupFilterConditionType.EpisodeCount:

						int epCount = -1;
						int.TryParse(gfc.ConditionParameter, out epCount);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan && grp.Stat_EpisodeCount < epCount) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan && grp.Stat_EpisodeCount > epCount) return false;
						break;

					case GroupFilterConditionType.Category:

						string filterParm = gfc.ConditionParameter.Trim();

						string[] cats = filterParm.Split(',');
						bool foundCat = false;
						int index = 0;
						foreach (string cat in cats)
						{
							if (cat.Trim().Length == 0) continue;
							if (cat.Trim() == ",") continue;

							index = grp.Stat_AllCategories.IndexOf(cat, 0, StringComparison.InvariantCultureIgnoreCase);
							if (index > -1)
							{
								foundCat = true;
								break;
							}
						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundCat) return false;
						
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundCat) return false;
						break;

					case GroupFilterConditionType.AnimeType:

						filterParm = gfc.ConditionParameter.Trim();
						List<string> grpTypeList = grp.AnimeTypesList;

						string[] atypes = filterParm.Split(',');
						bool foundAnimeType = false;
						index = 0;
						foreach (string atype in atypes)
						{
							if (atype.Trim().Length == 0) continue;
							if (atype.Trim() == ",") continue;

							foreach (string thisAType in grpTypeList)
							{
								if (string.Equals(thisAType, atype, StringComparison.InvariantCultureIgnoreCase))
								{
									foundAnimeType = true; 
									break;
								}
							}
						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundAnimeType) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundAnimeType) return false;
						break;

					

					case GroupFilterConditionType.VideoQuality:

						filterParm = gfc.ConditionParameter.Trim();

						string[] vidQuals = filterParm.Split(',');
						bool foundVid = false;
						bool foundVidAllEps = false;
						index = 0;
						foreach (string vidq in vidQuals)
						{
							if (vidq.Trim().Length == 0) continue;
							if (vidq.Trim() == ",") continue;

							index = grp.Stat_AllVideoQuality.IndexOf(vidq, 0, StringComparison.InvariantCultureIgnoreCase);
							if (index > -1) foundVid = true;

							index = grp.Stat_AllVideoQualityEpisodes.IndexOf(vidq, 0, StringComparison.InvariantCultureIgnoreCase);
							if (index > -1) foundVidAllEps = true;
	
						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundVid) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundVid) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.InAllEpisodes)
							if (!foundVidAllEps) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotInAllEpisodes)
							if (foundVidAllEps) return false;

						break;

					case GroupFilterConditionType.AudioLanguage:
					case GroupFilterConditionType.SubtitleLanguage:

						filterParm = gfc.ConditionParameter.Trim();

						string[] languages = filterParm.Split(',');
						bool foundLan = false;
						index = 0;
						foreach (string lanName in languages)
						{
							if (lanName.Trim().Length == 0) continue;
							if (lanName.Trim() == ",") continue;

							if (gfc.ConditionTypeEnum == GroupFilterConditionType.AudioLanguage)
								index = grp.Stat_AudioLanguages.IndexOf(lanName, 0, StringComparison.InvariantCultureIgnoreCase);

							if (gfc.ConditionTypeEnum == GroupFilterConditionType.SubtitleLanguage)
								index = grp.Stat_SubtitleLanguages.IndexOf(lanName, 0, StringComparison.InvariantCultureIgnoreCase);

							if (index > -1) foundLan = true;

						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundLan) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundLan) return false;

						break;
				}
			}

			return true;
		}

		public bool EvaluateGroupFilter(AnimeSeriesVM ser)
		{
			foreach (GroupFilterConditionVM gfc in FilterConditions)
			{
				switch (gfc.ConditionTypeEnum)
				{

					case GroupFilterConditionType.MissingEpisodes:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.HasMissingEpisodesAny == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.HasMissingEpisodesAny == true) return false;
						break;
					
					case GroupFilterConditionType.MissingEpisodesCollecting:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.HasMissingEpisodesGroups == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.HasMissingEpisodesGroups == true) return false;
						break;
					
					case GroupFilterConditionType.HasUnwatchedEpisodes:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.HasUnwatchedFiles == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.HasUnwatchedFiles == true) return false;
						break;
					
					case GroupFilterConditionType.CompletedSeries:

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.IsComplete == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.IsComplete == true) return false;
						break;

					case GroupFilterConditionType.AssignedTvDBInfo:

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.CrossRef_AniDB_TvDB == null) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.CrossRef_AniDB_TvDB != null) return false;
						break;

					case GroupFilterConditionType.AssignedMALInfo:

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.CrossRef_AniDB_MAL == null) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.CrossRef_AniDB_MAL != null) return false;
						break;

					case GroupFilterConditionType.AssignedMovieDBInfo:

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.CrossRef_AniDB_MovieDB == null) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.CrossRef_AniDB_MovieDB != null) return false;
						break;

					case GroupFilterConditionType.AssignedTvDBOrMovieDBInfo:

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.CrossRef_AniDB_TvDB == null && ser.CrossRef_AniDB_MovieDB == null) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.CrossRef_AniDB_TvDB != null && ser.CrossRef_AniDB_MovieDB != null) return false;
						break;
					
					case GroupFilterConditionType.FinishedAiring:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.FinishedAiring == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.FinishedAiring == true) return false;
						break;
						
					case GroupFilterConditionType.UserVoted:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.UserHasVotedPerm == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.UserHasVotedPerm == true) return false;
						break;

					case GroupFilterConditionType.UserVotedAny:
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Include && ser.UserHasVotedAny == false) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.Exclude && ser.UserHasVotedAny == true) return false;
						break;

					case GroupFilterConditionType.AirDate:
						DateTime filterDate;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDate = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDate = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);


						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
							if (!ser.AniDB_Anime.AirDate.HasValue || ser.AniDB_Anime.AirDate.Value < filterDate) return false;
						
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
							if (!ser.AniDB_Anime.AirDate.HasValue || ser.AniDB_Anime.AirDate.Value > filterDate) return false;
						
						break;
					
					case GroupFilterConditionType.SeriesCreatedDate:
						DateTime filterDateSeries;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDateSeries = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDateSeries = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
							if (ser.DateTimeCreated < filterDateSeries) return false;
						
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
							if (ser.DateTimeCreated > filterDateSeries) return false;
						break;
					
					case GroupFilterConditionType.EpisodeWatchedDate:
						DateTime filterDateEpsiodeWatched;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDateEpsiodeWatched = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDateEpsiodeWatched = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							if (!ser.WatchedDate.HasValue) return false;
							if (ser.WatchedDate.Value < filterDateEpsiodeWatched) return false;
						}
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
						{
							if (!ser.WatchedDate.HasValue) return false;
							if (ser.WatchedDate.Value > filterDateEpsiodeWatched) return false;
						}
						break;
					
					case GroupFilterConditionType.EpisodeAddedDate:
						DateTime filterDateEpisodeAdded;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							int days = 0;
							int.TryParse(gfc.ConditionParameter, out days);
							filterDateEpisodeAdded = DateTime.Today.AddDays(0 - days);
						}
						else
							filterDateEpisodeAdded = GroupFilterHelper.GetDateFromString(gfc.ConditionParameter);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan || gfc.ConditionOperatorEnum == GroupFilterOperator.LastXDays)
						{
							if (!ser.EpisodeAddedDate.HasValue) return false;
							if (ser.EpisodeAddedDate.Value < filterDateEpisodeAdded) return false;
						}
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan)
						{
							if (!ser.EpisodeAddedDate.HasValue) return false;
							if (ser.EpisodeAddedDate.Value > filterDateEpisodeAdded) return false;
						}
						break;

					case GroupFilterConditionType.AniDBRating:

						decimal dRating = -1;
						decimal.TryParse(gfc.ConditionParameter, out dRating);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan && ser.AniDB_Anime.AniDBRating < dRating) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan && ser.AniDB_Anime.AniDBRating > dRating) return false;
						break;
					
					case GroupFilterConditionType.UserRating:

						if (ser.AniDB_Anime == null || ser.AniDB_Anime.Detail == null || ser.AniDB_Anime.Detail.UserVote == null) return false;

						decimal dUserRating = -1;
						decimal.TryParse(gfc.ConditionParameter, out dUserRating);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan && ser.AniDB_Anime.Detail.UserRating < dUserRating) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan && ser.AniDB_Anime.Detail.UserRating > dUserRating) return false;
						break;

					case GroupFilterConditionType.EpisodeCount:

						int epCount = -1;
						int.TryParse(gfc.ConditionParameter, out epCount);

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.GreaterThan && ser.AniDB_Anime.EpisodeCountNormal < epCount) return false;
						if (gfc.ConditionOperatorEnum == GroupFilterOperator.LessThan && ser.AniDB_Anime.EpisodeCountNormal > epCount) return false;
						break;
					
					case GroupFilterConditionType.Category:

						string filterParm = gfc.ConditionParameter.Trim();

						string[] cats = filterParm.Split(',');
						bool foundCat = false;
						int index = 0;
						foreach (string cat in cats)
						{
							if (cat.Trim().Length == 0) continue;
							if (cat.Trim() == ",") continue;

							index = ser.CategoriesString.IndexOf(cat, 0, StringComparison.InvariantCultureIgnoreCase);
							if (index > -1)
							{
								foundCat = true;
								break;
							}
						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundCat) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundCat) return false;
						break;

					case GroupFilterConditionType.AnimeType:

						filterParm = gfc.ConditionParameter.Trim();

						string[] atypes = filterParm.Split(',');
						bool foundAnimeType = false;
						index = 0;
						foreach (string atype in atypes)
						{
							if (atype.Trim().Length == 0) continue;
							if (atype.Trim() == ",") continue;

							if (string.Equals(ser.AniDB_Anime.AnimeTypeDescription, atype, StringComparison.InvariantCultureIgnoreCase))
							{
								foundAnimeType = true;
								break;
							}
						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundAnimeType) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundAnimeType) return false;
						break;


					
					case GroupFilterConditionType.VideoQuality:

						filterParm = gfc.ConditionParameter.Trim();

						string[] vidQuals = filterParm.Split(',');
						bool foundVid = false;
						bool foundVidAllEps = false;
						index = 0;

						string stat_VidQualSeries = ser.AniDB_Anime.Detail.Stat_AllVideoQuality;
						string stat_VidQualEpisodes = ser.AniDB_Anime.Detail.Stat_AllVideoQuality_Episodes;


						foreach (string vidq in vidQuals)
						{
							if (vidq.Trim().Length == 0) continue;
							if (vidq.Trim() == ",") continue;

							index = stat_VidQualSeries.IndexOf(vidq, 0, StringComparison.InvariantCultureIgnoreCase);
							if (index > -1) foundVid = true;

							index = stat_VidQualEpisodes.IndexOf(vidq, 0, StringComparison.InvariantCultureIgnoreCase);
							if (index > -1) foundVidAllEps = true;

						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundVid) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundVid) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.InAllEpisodes)
							if (!foundVidAllEps) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotInAllEpisodes)
							if (foundVidAllEps) return false;

						break;
					
					case GroupFilterConditionType.AudioLanguage:
					case GroupFilterConditionType.SubtitleLanguage:

						filterParm = gfc.ConditionParameter.Trim();

						string[] languages = filterParm.Split(',');
						bool foundLan = false;
						index = 0;

						string stat_AudioLanguages = ser.AniDB_Anime.Detail.Stat_AudioLanguages;
						string stat_SubtitleLanguages = ser.AniDB_Anime.Detail.Stat_SubtitleLanguages;

						foreach (string lanName in languages)
						{
							if (lanName.Trim().Length == 0) continue;
							if (lanName.Trim() == ",") continue;

							if (gfc.ConditionTypeEnum == GroupFilterConditionType.AudioLanguage)
								index = stat_AudioLanguages.IndexOf(lanName, 0, StringComparison.InvariantCultureIgnoreCase);

							if (gfc.ConditionTypeEnum == GroupFilterConditionType.SubtitleLanguage)
								index = stat_SubtitleLanguages.IndexOf(lanName, 0, StringComparison.InvariantCultureIgnoreCase);

							if (index > -1) foundLan = true;

						}

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.In)
							if (!foundLan) return false;

						if (gfc.ConditionOperatorEnum == GroupFilterOperator.NotIn)
							if (foundLan) return false;

						break;

					
				}
			}

			return true;
		}

		public override List<MainListWrapper> GetDirectChildren()
		{
			List<MainListWrapper> wrappers = new List<MainListWrapper>();

			//if (MainListHelperVM.Instance.AllGroups.Count == 0)
			//	MainListHelperVM.Instance.RefreshGroupsSeriesData();

			AnimeGroupVM.SortMethod = AnimeGroupSortMethod.SortName;
			List<AnimeGroupVM> grps = new List<AnimeGroupVM>(MainListHelperVM.Instance.AllGroups);
			//grps.Sort();

			foreach (AnimeGroupVM grp in grps)
			{
				// ignore sub groups
				if (grp.AnimeGroupParentID.HasValue) continue;

				if (EvaluateGroupFilter(grp))
				{
					if (grp.AllAnimeSeries.Count == 1)
						wrappers.Add(grp.AllAnimeSeries[0]);
					else
						wrappers.Add(grp);
				}
			}

			return wrappers;
		}

		public JMMServerBinary.Contract_GroupFilter ToContract()
		{
			JMMServerBinary.Contract_GroupFilter contract = new JMMServerBinary.Contract_GroupFilter();
			contract.GroupFilterID = this.GroupFilterID;
			contract.GroupFilterName = this.FilterName;
			contract.ApplyToSeries = this.ApplyToSeries;
			contract.BaseCondition = this.BaseCondition;
			
			contract.FilterConditions = new List<JMMServerBinary.Contract_GroupFilterCondition>();
			foreach (GroupFilterConditionVM gfc in FilterConditions)
				contract.FilterConditions.Add(gfc.ToContract());

			// derive the sorting
			contract.SortingCriteria = "";
			foreach (GroupFilterSortingCriteria gfsc in this.SortCriteriaList)
			{
				if (contract.SortingCriteria.Length > 0) contract.SortingCriteria += "|";
				contract.SortingCriteria += ((int)gfsc.SortType).ToString();
				contract.SortingCriteria += ";";
				contract.SortingCriteria += ((int)gfsc.SortDirection).ToString();
			}

			return contract;
		}

		public void Populate(JMMServerBinary.Contract_GroupFilter contract)
		{
			this.GroupFilterID = contract.GroupFilterID;
			this.FilterName = contract.GroupFilterName;
			this.ApplyToSeries = contract.ApplyToSeries;
			this.BaseCondition = contract.BaseCondition;
			//this.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();
			this.FilterConditions.Clear();
			if (contract.FilterConditions != null)
			{
				foreach (JMMServerBinary.Contract_GroupFilterCondition gfc_con in contract.FilterConditions)
					FilterConditions.Add(new GroupFilterConditionVM(gfc_con));
			}
			//SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();
			SortCriteriaList.Clear();

			string sortCriteriaRaw = contract.SortingCriteria;

			if (!string.IsNullOrEmpty(sortCriteriaRaw))
			{
				string[] scrit = sortCriteriaRaw.Split('|');
				foreach (string sortpair in scrit)
				{
					string[] spair = sortpair.Split(';');
					if (spair.Length != 2) continue;

					int stype = 0;
					int sdir = 0;

					int.TryParse(spair[0], out stype);
					int.TryParse(spair[1], out sdir);

					if (stype > 0 && sdir > 0)
					{
						GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
						gfsc.GroupFilterID = this.GroupFilterID;
						gfsc.SortType = (GroupFilterSorting)stype;
						gfsc.SortDirection = (GroupFilterSortDirection)sdir;
						SortCriteriaList.Add(gfsc);
					}
				}
			}

			//SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>(SortCriteriaList.OrderBy(p => p.GroupFilterSortingString));
			FilterConditions = new ObservableCollection<GroupFilterConditionVM>(FilterConditions.OrderBy(p => p.ConditionTypeString));
		}

		public bool Save()
		{
			try
			{
				JMMServerBinary.Contract_GroupFilter_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.SaveGroupFilter(this.ToContract());
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					MessageBox.Show(response.ErrorMessage);
					return false;
				}
				else
				{
					Populate(response.GroupFilter);
				}

				return true;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			return false;
		}

		public bool Validate()
		{
			if (string.IsNullOrEmpty(this.FilterName))
			{
				MessageBox.Show("Filter Name must be populated");
				return false;
			}

			return true;
		}

		public bool Delete()
		{
			try
			{
				if (!this.GroupFilterID.HasValue) return true;

				string msg = JMMServerVM.Instance.clientBinaryHTTP.DeleteGroupFilter(this.GroupFilterID.Value);
				if (!string.IsNullOrEmpty(msg))
				{
					MessageBox.Show(msg);
					return false;
				}
				else
					return true;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
				return false;
			}
		}

		public int CompareTo(GroupFilterVM obj)
		{
			return FilterName.CompareTo(obj.FilterName);
		}

	}
}
