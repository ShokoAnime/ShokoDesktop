using System.ComponentModel;
using Shoko.Models.Enums;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_GroupFilterSortingCriteria : INotifyPropertyChangedExt
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public int? GroupFilterID { get; set; }

        private GroupFilterSorting sortType = GroupFilterSorting.AniDBRating;
        public GroupFilterSorting SortType
        {
            get { return sortType; }
            set
            {
                this.SetField(()=>sortType,value);
                GroupFilterSortingString = sortType.GetTextForEnum_Sorting();
            }
        }

        private GroupFilterSortDirection sortDirection = GroupFilterSortDirection.Asc;
        public GroupFilterSortDirection SortDirection
        {
            get { return sortDirection; }
            set
            {
                this.SetField(()=>sortDirection,value);
                GroupFilterSortDirectionString = sortDirection.GetTextForEnum_SortDirection();
            }
        }

        private string groupFilterSortingString = "";
        public string GroupFilterSortingString
        {
            get { return groupFilterSortingString; }
            set
            {
                this.SetField(()=>groupFilterSortingString,value);
            }
        }

        private string groupFilterSortDirectionString = "";
        public string GroupFilterSortDirectionString
        {
            get { return groupFilterSortDirectionString; }
            set
            {
                this.SetField(()=>groupFilterSortDirectionString,value);
            }
        }

    }
}
