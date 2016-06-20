using System;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
    public class GroupFilterConditionVM : INotifyPropertyChanged, IComparable<GroupFilterConditionVM>
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

        public int? GroupFilterConditionID { get; set; }
        public int? GroupFilterID { get; set; }

        private int conditionType = 1;
        public int ConditionType
        {
            get { return conditionType; }
            set
            {
                conditionType = value;
                ConditionTypeString = GroupFilterHelper.GetTextForEnum_ConditionType(ConditionTypeEnum);
                SetConditionParameterString();
            }
        }

        private int conditionOperator = 1;
        public int ConditionOperator
        {
            get { return conditionOperator; }
            set
            {
                conditionOperator = value;
                ConditionOperatorString = GroupFilterHelper.GetTextForEnum_Operator(ConditionOperatorEnum);
                SetConditionParameterString();
            }
        }

        private string conditionParameter = "";
        public string ConditionParameter
        {
            get { return conditionParameter; }
            set
            {
                conditionParameter = value;
                SetConditionParameterString();
            }
        }

        public GroupFilterConditionType ConditionTypeEnum
        {
            get { return (GroupFilterConditionType)ConditionType; }
        }

        public GroupFilterOperator ConditionOperatorEnum
        {
            get { return (GroupFilterOperator)ConditionOperator; }
        }

        private string conditionTypeString = "";
        public string ConditionTypeString
        {
            get { return conditionTypeString; }
            set
            {
                conditionTypeString = value;
                NotifyPropertyChanged("ConditionTypeString");
            }
        }

        private string conditionOperatorString = "";
        public string ConditionOperatorString
        {
            get { return conditionOperatorString; }
            set
            {
                conditionOperatorString = value;
                NotifyPropertyChanged("ConditionOperatorString");
            }
        }

        private string conditionParameterString = "";
        public string ConditionParameterString
        {
            get { return conditionParameterString; }
            set
            {
                conditionParameterString = value;
                NotifyPropertyChanged("ConditionParameterString");
            }
        }

        private void SetConditionParameterString()
        {
            ConditionParameterString = "";

            // only set if a proper match

			switch (ConditionTypeEnum)
			{
				case GroupFilterConditionType.AirDate:
				case GroupFilterConditionType.SeriesCreatedDate:
				case GroupFilterConditionType.EpisodeAddedDate:
				case GroupFilterConditionType.EpisodeWatchedDate:
                case GroupFilterConditionType.LatestEpisodeAirDate:

                    if (ConditionOperatorEnum == GroupFilterOperator.LastXDays)
                        ConditionParameterString += ConditionParameter;
                    else
                    {
                        DateTime airDate = GroupFilterHelper.GetDateFromString(ConditionParameter);
                        ConditionParameterString += GroupFilterHelper.GetDateAsFriendlyString(airDate);
                    }
                    break;

                case GroupFilterConditionType.AnimeGroup:

                    int groupID = 0;
                    int.TryParse(ConditionParameter, out groupID);
                    if (groupID == 0) break;

                    if (!MainListHelperVM.Instance.AllGroupsDictionary.ContainsKey(groupID)) break;

                    ConditionParameterString += MainListHelperVM.Instance.AllGroupsDictionary[groupID].GroupName;
                    break;

                case GroupFilterConditionType.AnimeType:
                case GroupFilterConditionType.Tag:
                case GroupFilterConditionType.CustomTags:
				case GroupFilterConditionType.ReleaseGroup:
				case GroupFilterConditionType.Studio:
				case GroupFilterConditionType.VideoQuality:
				case GroupFilterConditionType.AniDBRating:
				case GroupFilterConditionType.UserRating:
				case GroupFilterConditionType.AudioLanguage:
				case GroupFilterConditionType.SubtitleLanguage:
                case GroupFilterConditionType.Year:
					ConditionParameterString += ConditionParameter;
					break;
			}
		}

        public int CompareTo(GroupFilterConditionVM obj)
        {
            return ConditionTypeString.CompareTo(obj.ConditionTypeString);
        }


        public string NiceDescription
        {
            get
            {
                string ret = string.Empty;
                ret += GroupFilterHelper.GetTextForEnum_ConditionType(ConditionTypeEnum);
                ret += " - ";

                ret += GroupFilterHelper.GetTextForEnum_Operator(ConditionOperatorEnum);

                if (ConditionTypeEnum == GroupFilterConditionType.AirDate)
                {
                    ret += " ";
                    DateTime airDate = GroupFilterHelper.GetDateFromString(ConditionParameter);
                    ret += GroupFilterHelper.GetDateAsFriendlyString(airDate);
                }

                return ret;

                // TODO - cater for parameters
            }
        }

        public GroupFilterConditionVM()
        {
            GroupFilterConditionID = null;
            GroupFilterID = null;
            ConditionType = 1;
            ConditionOperator = 1;
            ConditionParameter = "";
        }


        public GroupFilterConditionVM(JMMServerBinary.Contract_GroupFilterCondition contract)
        {
            // read only members
            this.GroupFilterConditionID = contract.GroupFilterConditionID;
            this.GroupFilterID = contract.GroupFilterID;
            this.ConditionOperator = contract.ConditionOperator;
            this.ConditionParameter = contract.ConditionParameter;
            this.ConditionType = contract.ConditionType;
        }

        public JMMServerBinary.Contract_GroupFilterCondition ToContract()
        {
            JMMServerBinary.Contract_GroupFilterCondition contract = new JMMServerBinary.Contract_GroupFilterCondition();
            contract.GroupFilterConditionID = this.GroupFilterConditionID;
            contract.GroupFilterID = this.GroupFilterID;
            contract.ConditionOperator = this.ConditionOperator;
            contract.ConditionParameter = this.ConditionParameter;
            contract.ConditionType = this.ConditionType;

            return contract;
        }
    }
}
