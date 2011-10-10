using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class GroupFilterSortingCriteria : INotifyPropertyChanged
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

		private GroupFilterSorting sortType = GroupFilterSorting.AniDBRating;
		public GroupFilterSorting SortType
		{
			get { return sortType; }
			set
			{
				sortType = value;
				NotifyPropertyChanged("SortType");
				GroupFilterSortingString = GroupFilterHelper.GetTextForEnum_Sorting(sortType);
			}
		}

		private GroupFilterSortDirection sortDirection = GroupFilterSortDirection.Asc;
		public GroupFilterSortDirection SortDirection
		{
			get { return sortDirection; }
			set
			{
				sortDirection = value;
				NotifyPropertyChanged("SortDirection");
				GroupFilterSortDirectionString = GroupFilterHelper.GetTextForEnum_SortDirection(sortDirection);
			}
		}

		private string groupFilterSortingString = "";
		public string GroupFilterSortingString
		{
			get { return groupFilterSortingString; }
			set
			{
				groupFilterSortingString = value;
				NotifyPropertyChanged("GroupFilterSortingString");
			}
		}

		private string groupFilterSortDirectionString = "";
		public string GroupFilterSortDirectionString
		{
			get { return groupFilterSortDirectionString; }
			set
			{
				groupFilterSortDirectionString = value;
				NotifyPropertyChanged("GroupFilterSortDirectionString");
			}
		}

	}
}
