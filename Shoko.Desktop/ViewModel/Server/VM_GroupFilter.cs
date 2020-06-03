﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using ImpromptuInterface;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Commons.Properties;
using Shoko.Desktop.Utilities;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupFilter : CL_GroupFilter, IListWrapper, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        [JsonIgnore, XmlIgnore]
        public bool IsEditable => false;
        [JsonIgnore, XmlIgnore]
        public int ObjectType => 0;
        [JsonIgnore, XmlIgnore]
        public bool AllowEditing => !IsLocked;
        [JsonIgnore, XmlIgnore]
        public bool AllowDeletion => !IsLocked;

        [JsonIgnore, XmlIgnore]
        public string SortName
        {
            get
            {
                if (FilterType == ((int) GroupFilterType.Season))
                {
                    string[] split = GroupFilterName.Split(' ');
                    if (split.Length == 2)
                    {
                        string part2;
                        switch (split[0])
                        {
                            case "Winter":
                            {
                                part2 = string.Intern("1 Winter");
                                break;
                            }
                            case "Spring":
                            {
                                part2 = string.Intern("2 Spring");
                                break;
                            }
                            case "Summer":
                            {
                                part2 = string.Intern("3 Summer");
                                break;
                            }
                            case "Fall":
                            {
                                part2 = string.Intern("4 Fall");
                                break;
                            }
                            default:
                            {
                                part2 = string.Empty;
                                break;
                            }
                        }
                        return split[1] + part2;
                    }
                }
                return GroupFilterName;
            }
        }

        [JsonIgnore, XmlIgnore]
        public string GroupName => SortName;


        public new List<VM_GroupFilterCondition> FilterConditions
        {
            // ReSharper disable once UnusedMember.Local
            get => base.FilterConditions.CastList<VM_GroupFilterCondition>();
            set
            {
                if (value == null || value.Count <= 0)
                {
                    _filterConditions.Clear();
                    this.OnPropertyChanged(() => Obs_FilterConditions);
                    return;
                }
                _filterConditions.ReplaceRange(value.CastList<VM_GroupFilterCondition>());
                this.OnPropertyChanged(() => Obs_FilterConditions);
                base.FilterConditions = value.CastList<GroupFilterCondition>();
            }
        }

        public new string SortingCriteria
        {
            // ReSharper disable once UnusedMember.Local
            get => base.SortingCriteria;
            /*

            */
            set
            {
                List<VM_GroupFilterSortingCriteria> ls = new List<VM_GroupFilterSortingCriteria>();
                if (!string.IsNullOrEmpty(value))
                {
                    string[] scrit = value.Split('|');
                    foreach (string sortpair in scrit)
                    {
                        string[] spair = sortpair.Split(';');
                        if (spair.Length != 2) continue;

                        int stype;
                        int sdir;

                        int.TryParse(spair[0], out stype);
                        int.TryParse(spair[1], out sdir);

                        if (stype > 0 && sdir > 0)
                        {
                            VM_GroupFilterSortingCriteria gfsc = new VM_GroupFilterSortingCriteria
                            {
                                GroupFilterID = GroupFilterID,
                                SortType = (GroupFilterSorting) stype,
                                SortDirection = (GroupFilterSortDirection) sdir
                            };
                            ls.Add(gfsc);
                        }
                    }
                }
                
                Dispatcher.CurrentDispatcher.Invoke(() => _sortingCriteriaList.ReplaceRange(ls));
                this.OnPropertyChanged(() => SortCriteriaList);
                base.SortingCriteria = value;
            }
        }

        private readonly TrulyObservableCollection<VM_GroupFilterCondition> _filterConditions;
        [JsonIgnore, XmlIgnore]
        public TrulyObservableCollection<VM_GroupFilterCondition> Obs_FilterConditions => _filterConditions;

        private readonly TrulyObservableCollection<VM_GroupFilterSortingCriteria> _sortingCriteriaList;
        [JsonIgnore, XmlIgnore]
        public TrulyObservableCollection<VM_GroupFilterSortingCriteria> SortCriteriaList => _sortingCriteriaList;

        public new int? Locked
        {
            get => base.Locked;
            set
            {
                this.SetField(()=>base.Locked,r=> base.Locked = r, value,()=>IsLocked);
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsLocked => base.Locked.HasValue && base.Locked == 1;

        private bool isBeingEdited;
        [JsonIgnore, XmlIgnore]
        public bool IsBeingEdited
        {
            get => isBeingEdited;
            set
            {
                this.SetField(()=>isBeingEdited, value);
            }
        }

        public new string GroupFilterName
        {
            get => base.GroupFilterName;
            set
            {
                this.SetField(()=>base.GroupFilterName,r=> base.GroupFilterName = r, value);
            }
        }

        public new int FilterType
        {
            get => base.FilterType;
            set
            {
                this.SetField(()=>base.FilterType,r=> base.FilterType = r, value);
                if (value == (int) GroupFilterType.ContinueWatching)
                    Locked = 1;
            }
        }

        public new int ApplyToSeries
        {
            get => base.ApplyToSeries;
            set
            {
                this.SetField(()=>base.ApplyToSeries,r=> base.ApplyToSeries = r, value,()=>ApplyToSeries, ()=>IsApplyToSeries);
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsApplyToSeries
        {
            get => ApplyToSeries == 1;
            set => ApplyToSeries = value ? 1 : 0;
        }
        
        [JsonIgnore, XmlIgnore]
        public bool IsInvisibleInClients
        {
            get => InvisibleInClients == 1;
            set => InvisibleInClients = value ? 1 : 0;
        }

        public new int BaseCondition
        {
            get => base.BaseCondition;
            set
            {
                this.SetField(()=>base.BaseCondition,r=> base.BaseCondition = r, value);
            }
        }

        public new int? ParentGroupFilterID
        {
            get => base.ParentGroupFilterID;
            set
            {
                this.SetField(()=>base.ParentGroupFilterID,r=> base.ParentGroupFilterID = r, value);
            }
        }

        public new int InvisibleInClients
        {
            get => base.InvisibleInClients;
            set
            {
                this.SetField(()=>base.InvisibleInClients,r=> base.InvisibleInClients = r, value);
            }
        }

        [JsonIgnore, XmlIgnore]
        public string Summary
        {
            get
            {
                int groupsCount = GroupsCount;
                string summ = "";
                if (groupsCount > 1)
                    summ = string.Format(Childs.Count > 0 ? "{0} " + Resources.Anime_Filters : "{0} " + Resources.Anime_Groups, groupsCount);
                else if (groupsCount > 0)
                    summ = string.Format(Childs.Count > 0 ? "{0} " + Resources.Anime_Filter : "{0} " + Resources.Anime_Group, groupsCount);
                return summ;
            }
        }

        private int _groupsCount ;
        [JsonIgnore, XmlIgnore]
        public int GroupsCount
        {
            get => _groupsCount;
            set
            {
                this.SetField(()=>_groupsCount, value);
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool IsDirectoryFilter => (FilterType & (int) GroupFilterType.Directory) ==
                                         (int) GroupFilterType.Directory;

        public VM_GroupFilter()
        {
            GroupFilterID = 0;
            FilterType = (int)GroupFilterType.UserDefined;
            _filterConditions = new TrulyObservableCollection<VM_GroupFilterCondition>();
            _sortingCriteriaList = new TrulyObservableCollection<VM_GroupFilterSortingCriteria>();
            _filterConditions.CollectionChanged += (a, b) =>
            {
                base.FilterConditions = _filterConditions.CastList<GroupFilterCondition>();
                collectionChanged = true;
            };
            _sortingCriteriaList.CollectionChanged += (a, b) =>
            {
                string sortingCriteria = "";
                foreach (VM_GroupFilterSortingCriteria gfsc in SortCriteriaList)
                {
                    if (sortingCriteria.Length > 0) sortingCriteria += "|";
                    sortingCriteria += ((int)gfsc.SortType).ToString();
                    sortingCriteria += ";";
                    sortingCriteria += ((int)gfsc.SortDirection).ToString();
                }
                base.SortingCriteria = sortingCriteria;
                collectionChanged = true;
            };
        }

        private bool collectionChanged;

        public bool EvaluateGroupFilter(VM_AnimeGroup_User grp)
        {
            if (IsBeingEdited && collectionChanged)
            {
                Populate((VM_GroupFilter)VM_ShokoServer.Instance.ShokoServices.EvaluateGroupFilter(this));
                collectionChanged = false;
            }
            if (Groups == null || !Groups.ContainsKey(VM_ShokoServer.Instance.CurrentUser.JMMUserID))
                return false;
            if (grp.AnimeGroupID != 0)
                return Groups[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Contains(grp.AnimeGroupID);
            return false;
        }

        public bool EvaluateGroupFilter(VM_AnimeSeries_User ser)
        {
            if (Series == null || !Series.ContainsKey(VM_ShokoServer.Instance.CurrentUser.JMMUserID))
                return false;
            if (ser.AnimeSeriesID != 0)
                return Series[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Contains(ser.AnimeSeriesID);
            return false;
        }

        public bool HasGroupChilds()
        {
            int id = VM_ShokoServer.Instance.CurrentUser.JMMUserID;
            return (Groups != null && Groups.ContainsKey(id) && Groups[id].Count > 0);
        }

        public List<IListWrapper> GetDirectChildren()
        {
            Childs = Childs ?? new HashSet<int>(); //Hack around ASP Nulls.
            List<IListWrapper> wrappers = new List<IListWrapper>();

            VM_AnimeGroup_User.SortMethod = AnimeGroupSortMethod.SortName;
            if (Childs.Count > 0)
            {
                wrappers.AddRange(Childs.Where(a=> VM_MainListHelper.Instance.AllGroupFiltersDictionary.ContainsKey(a)).Select(a => VM_MainListHelper.Instance.AllGroupFiltersDictionary[a]).Where(a=>!a.IsLocked || (a.IsLocked && a.HasGroupChilds())).OrderBy(a => a.GroupFilterName));
            }
            else
            {
                if (Groups.ContainsKey(VM_ShokoServer.Instance.CurrentUser.JMMUserID))
                {
                    foreach (
                        VM_AnimeGroup_User grp in
                            Groups[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Where(a => VM_MainListHelper.Instance.AllGroupsDictionary.ContainsKey(a)).Select(
                                a => VM_MainListHelper.Instance.AllGroupsDictionary[a]))
                    {
                        if (grp.AnimeGroupParentID.HasValue) continue;
                        if (grp.AllAnimeSeries.Count == 1)
                            wrappers.Add(grp.AllAnimeSeries[0]);
                        else
                            wrappers.Add(grp);
                    }
                }
            }
            if (wrappers.Count != GroupsCount)
            {
                GroupsCount = wrappers.Count;
            }
            return wrappers;
        }


        public void Populate(VM_GroupFilter contract)
        {
            GroupFilterID = contract.GroupFilterID;
            GroupFilterName = contract.GroupFilterName;
            ApplyToSeries = contract.ApplyToSeries;
            BaseCondition = contract.BaseCondition;
            Locked = contract.Locked;
            FilterType = contract.FilterType;
            InvisibleInClients = contract.InvisibleInClients;
            ParentGroupFilterID = contract.ParentGroupFilterID;
            Groups = contract.Groups;
            Series = contract.Series;
            Childs = contract.Childs;
#pragma warning disable 618
            SortingCriteria = contract.SortingCriteria;
            FilterConditions = contract.FilterConditions;
#pragma warning restore 618
        }

        public bool Save()
        {
            try
            {
                CL_Response<CL_GroupFilter> response = VM_ShokoServer.Instance.ShokoServices.SaveGroupFilter(this);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    MessageBox.Show(response.ErrorMessage);
                    return false;
                }
                Populate((VM_GroupFilter)response.Result);

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
            if (string.IsNullOrEmpty(GroupFilterName))
            {
                MessageBox.Show(Resources.Anime_FilterName, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool Delete()
        {
            try
            {
                if (GroupFilterID == 0) return true;

                string msg = VM_ShokoServer.Instance.ShokoServices.DeleteGroupFilter(GroupFilterID);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg);
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                return false;
            }
        }



    }
}
