using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

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
        public bool AllowDeletion { get; set; }

        public ObservableCollection<GroupFilterConditionVM> FilterConditions { get; set; }
        public ObservableCollection<GroupFilterSortingCriteria> SortCriteriaList { get; set; }

		public Dictionary<int, HashSet<int>> Groups { get; set; }
        public Dictionary<int, HashSet<int>> Series { get; set; }
        public HashSet<int> Childs { get; set; }

		private Boolean isSystemGroupFilter = false;
        public Boolean IsSystemGroupFilter
        {
            get { return isSystemGroupFilter; }
            set
            {
                isSystemGroupFilter = value;
                IsNotSystemGroupFilter = !isSystemGroupFilter;
                NotifyPropertyChanged("IsSystemGroupFilter");
            }
        }

        private Boolean isNotSystemGroupFilter = true;
        public Boolean IsNotSystemGroupFilter
        {
            get { return isNotSystemGroupFilter; }
            set
            {
                isNotSystemGroupFilter = value;
                NotifyPropertyChanged("IsNotSystemGroupFilter");
            }
        }



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
                NotifyPropertyChanged("IsNotBeingEdited");
            }
        }

        public Boolean IsNotBeingEdited
        {
            get { return !isBeingEdited; }
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

        private int filterType = (int)GroupFilterType.UserDefined;
        public int FilterType
        {
            get { return filterType; }
            set
            {
                filterType = value;
                NotifyPropertyChanged("FilterType");
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

		private int? groupFilterParentId;
		public int? GroupFilterParentId
		{
			get { return groupFilterParentId; }
			set
			{
				groupFilterParentId = value;
				NotifyPropertyChanged("GroupFilterParentId");
			}
		}

		private int invisibleInClients = 0;
		public int InvisibleInClients
		{
			get { return invisibleInClients; }
			set
			{
				invisibleInClients = value;
				NotifyPropertyChanged("InvisibleInClients");
			}
		}

		public string Summary
		{
			get
			{
				int groupsCount = GroupsCount;
				string summ = "";
				if (groupsCount > 1)
					summ = string.Format(Childs.Count > 0 ? "{0} " + Properties.Resources.Anime_Filters : "{0} " + Properties.Resources.Anime_Groups, groupsCount);
                else if (groupsCount > 0)
                    summ = string.Format(Childs.Count > 0 ? "{0} " + Properties.Resources.Anime_Filter : "{0} " + Properties.Resources.Anime_Group, groupsCount);
                return summ;
            }
		}

		private string predefinedCriteria = "";
		public string PredefinedCriteria
		{
			get { return predefinedCriteria; }
			set
			{
				predefinedCriteria = value;
				NotifyPropertyChanged("PredefinedCriteria");
			}
		}

        private int _groupsCount=0;
        public int GroupsCount
		{
			get
			{
			    return _groupsCount;

			}
            set
            {
                _groupsCount = value;
                NotifyPropertyChanged("GroupsCount");
            }
        }

		public GroupFilterVM()
		{
			GroupFilterID = null;
			this.SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();
			this.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();
            FilterConditions.CollectionChanged += (a, b) =>
            {
                CollectionChanged = true;
            };
            SortCriteriaList.CollectionChanged += (a, b) =>
            {
                CollectionChanged = true;
            };
        }

        public GroupFilterVM(JMMServerBinary.Contract_GroupFilter contract)
		{
			this.SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();
			this.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();
			// read only members
			Populate(contract);
		    FilterConditions.CollectionChanged += (a, b) =>
		    {
		        CollectionChanged = true;
		    };
		    SortCriteriaList.CollectionChanged += (a, b) =>
		    {
		        CollectionChanged = true;
		    };
		}



        public override string ToString()
		{
			return string.Format("{0} - {1}", GroupFilterID, FilterName);
		}

        private bool CollectionChanged = false;

	    public bool EvaluateGroupFilter(AnimeGroupVM grp)
	    {
	        if (this.IsBeingEdited && CollectionChanged)
	        {
	            Populate(JMMServerVM.Instance.clientBinaryHTTP.EvaluateGroupFilter(this.ToContract()));
	            CollectionChanged = false;
	        }
	        if (Groups == null || !Groups.ContainsKey(JMMServerVM.Instance.CurrentUser.JMMUserID.Value))
	            return false;
            if (grp.AnimeGroupID.HasValue)
    	        return Groups[JMMServerVM.Instance.CurrentUser.JMMUserID.Value].Contains(grp.AnimeGroupID.Value);
	        return false;
	    }

	    public bool EvaluateGroupFilter(AnimeSeriesVM ser)
	    {
	        if (Series == null || !Series.ContainsKey(JMMServerVM.Instance.CurrentUser.JMMUserID.Value))
	            return false;
            if (ser.AnimeSeriesID.HasValue)
    	        return Series[JMMServerVM.Instance.CurrentUser.JMMUserID.Value].Contains(ser.AnimeSeriesID.Value);
	        return false;
	    }

	    public bool HasGroupChilds()
	    {
	        int id = JMMServerVM.Instance.CurrentUser.JMMUserID.Value;
            return (Groups != null && Groups.ContainsKey(id) && Groups[id].Count > 0);            
	    }

        public override List<MainListWrapper> GetDirectChildren()
        {
            List<MainListWrapper> wrappers = new List<MainListWrapper>();

            AnimeGroupVM.SortMethod = AnimeGroupSortMethod.SortName;
            if (Childs.Count > 0)
            {
                wrappers.AddRange(Childs.Where(a=> MainListHelperVM.Instance.AllGroupFiltersDictionary.ContainsKey(a)).Select(a => MainListHelperVM.Instance.AllGroupFiltersDictionary[a]).Where(a=>!a.IsLocked || (a.IsLocked && a.HasGroupChilds())).OrderBy(a => a.FilterName));
            }
            else
            {
                if (Groups.ContainsKey(JMMServerVM.Instance.CurrentUser.JMMUserID.Value))
                {
                    foreach (
                        AnimeGroupVM grp in
                            Groups[JMMServerVM.Instance.CurrentUser.JMMUserID.Value].Select(
                                a => MainListHelperVM.Instance.AllGroupsDictionary[a]))
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

        public JMMServerBinary.Contract_GroupFilter ToContract()
        {
            JMMServerBinary.Contract_GroupFilter contract = new JMMServerBinary.Contract_GroupFilter();
            contract.GroupFilterID = this.GroupFilterID;
            contract.GroupFilterName = this.FilterName;
            contract.ApplyToSeries = this.ApplyToSeries;
            contract.BaseCondition = this.BaseCondition;
            contract.Locked = null;
            if (isLocked) 
                contract.Locked = 1;
            contract.FilterType = this.FilterType;

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
			this.isLocked = contract.Locked.HasValue && contract.Locked == 1;
            this.FilterType = contract.FilterType;
            this.PredefinedCriteria = "";
			this.InvisibleInClients = contract.InvisibleInClients;
			this.GroupFilterParentId = contract.ParentGroupFilterID;
			this.Groups = contract.Groups.ToDictionary(a=>a.Key,a=>new HashSet<int>(a.Value));
		    this.Series = contract.Series.ToDictionary(a => a.Key, a => new HashSet<int>(a.Value));
		    this.Childs = new HashSet<int>(contract.Childs);
			this.AllowDeletion = true;
            this.AllowEditing = true;
            if (this.isLocked) {
                this.AllowDeletion = false;
                this.AllowEditing = false;
            } 
            if (this.FilterType == (int)GroupFilterType.ContinueWatching) this.AllowDeletion = false;

            this.IsSystemGroupFilter = false;
            this.IsNotSystemGroupFilter = true;

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
            //Update GroupCount
	
		    //SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>(SortCriteriaList.OrderBy(p => p.GroupFilterSortingString));
		    //FilterConditions = new ObservableCollection<GroupFilterConditionVM>(FilterConditions.OrderBy(p => p.ConditionTypeString));
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
                MessageBox.Show(Properties.Resources.Anime_FilterName, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool Delete()
        {
            try
            {
                if (!this.GroupFilterID.HasValue || this.GroupFilterID.Value==0) return true;

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
