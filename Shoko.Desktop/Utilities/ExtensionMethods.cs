using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.Utilities
{
    public static class ExtensionMethods
    {
        public static T SureGet<T>(this Dictionary<int, T> dict, int val) where T:class
        {
            if (dict.ContainsKey(val))
                return dict[val];
            return null;
        }
        public static T SureGet<T>(this ObservableListDictionary<int, T> dict, int val) where T : class
        {
            if (dict.ContainsKey(val))
                return dict[val];
            return null;
        }
        public static bool SubContains(this IEnumerable<string> list, string part)
        {
            foreach (string n in list)
            {
                if (n.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
        /*static public bool SetSelectedItem(this TreeView treeView, object item)
		{
			return SetSelected(treeView, item);
		}

		static private bool SetSelected(ItemsControl parent, object child)
		{
			if (parent == null || child == null)
			{
				return false;
			}

			TreeViewItem childNode = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;

			if (childNode != null)
			{
				childNode.Focus();
				return childNode.IsSelected = true;
			}

			if (parent.Items.Count > 0)
			{
				foreach (object childItem in parent.Items)
				{
					ItemsControl childControl = parent.ItemContainerGenerator.ContainerFromItem(childItem) as ItemsControl;

					if (SetSelected(childControl, child))
					{
						return true;
					}
				}
			}

			return false;
		}*/

        public static List<SortDescription> GetSortDescriptions(this VM_GroupFilter gf)
        {
            List<SortDescription> sortlist = new List<SortDescription>();
            foreach (VM_GroupFilterSortingCriteria gfsc in gf.SortCriteriaList)
            {
                sortlist.Add(gfsc.SortType.GetSortDescription(gfsc.SortDirection));
            }
            return sortlist;
        }

        public static SortDescription GetSortDescription(this GroupFilterSorting sortType, GroupFilterSortDirection sortDirection)
        {
            string sortColumn = "";
            ListSortDirection sortd = ListSortDirection.Ascending;

            switch (sortType)
            {
                case GroupFilterSorting.AniDBRating:
                    sortColumn = "AniDBRating"; break;
                case GroupFilterSorting.EpisodeAddedDate:
                    sortColumn = "EpisodeAddedDate"; break;
                case GroupFilterSorting.EpisodeAirDate:
                    sortColumn = "AirDate"; break;
                case GroupFilterSorting.EpisodeWatchedDate:
                    sortColumn = "WatchedDate"; break;
                case GroupFilterSorting.GroupName:
                    sortColumn = "GroupName"; break;
                case GroupFilterSorting.SortName:
                    sortColumn = "SortName"; break;
                case GroupFilterSorting.MissingEpisodeCount:
                    sortColumn = "MissingEpisodeCount"; break;
                case GroupFilterSorting.SeriesAddedDate:
                    sortColumn = "Stat_SeriesCreatedDate"; break;
                case GroupFilterSorting.SeriesCount:
                    sortColumn = "AllSeriesCount"; break;
                case GroupFilterSorting.UnwatchedEpisodeCount:
                    sortColumn = "UnwatchedEpisodeCount"; break;
                case GroupFilterSorting.UserRating:
                    sortColumn = "Stat_UserVoteOverall"; break;
                case GroupFilterSorting.Year:
                    if (sortDirection == GroupFilterSortDirection.Asc)
                        sortColumn = "Stat_AirDate_Min"; 
                    else
                        sortColumn = "Stat_AirDate_Max";
                    break;
                case GroupFilterSorting.GroupFilterName:
                    sortColumn = "FilterName"; break;
                default:
                    sortColumn = "GroupName"; break;
            }

            if (sortDirection == GroupFilterSortDirection.Asc)
                sortd = ListSortDirection.Ascending;
            else
                sortd = ListSortDirection.Descending;

            return new SortDescription(sortColumn, sortd);
        }

        public static void Clone(this object source, object destination)
        {
            Type type = source.GetType();
            FieldInfo[] myObjectFields = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo fi in myObjectFields)
            {
                try
                {
                    fi.SetValue(destination, fi.GetValue(source));
                }
                catch { }
            }
        }
    }
}
