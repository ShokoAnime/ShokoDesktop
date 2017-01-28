using System;
using System.ComponentModel;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Enums;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupFilterCondition : GroupFilterCondition, INotifyPropertyChangedExt
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        public new int ConditionType
        {
            get { return base.ConditionType; }
            set
            {
                base.ConditionType = this.SetField(base.ConditionType,value, ()=>ConditionType, ()=> ConditionTypeEnum, ()=>ConditionTypeString, ()=>ConditionParameterString, ()=>NiceDescription);
            }
        }

        public new int ConditionOperator
        {
            get { return base.ConditionOperator; }
            set
            {
                base.ConditionOperator = this.SetField(base.ConditionOperator, value, ()=>ConditionOperator, ()=>ConditionOperatorEnum, ()=>ConditionOperatorString, () => ConditionParameterString, () => NiceDescription);
            }
        }

        public new string ConditionParameter
        {
            get { return base.ConditionParameter; }
            set
            {
                base.ConditionParameter = this.SetField(base.ConditionParameter, value, () => ConditionParameter, () => ConditionParameterString, () => NiceDescription);
            }
        }

        public GroupFilterConditionType ConditionTypeEnum => (GroupFilterConditionType)ConditionType;

        public GroupFilterOperator ConditionOperatorEnum => (GroupFilterOperator)ConditionOperator;

        public string ConditionTypeString => ConditionTypeEnum.GetTextForEnum_ConditionType();

        public string ConditionOperatorString => ConditionOperatorEnum.GetTextForEnum_Operator();

        public string ConditionParameterString
        {
            get
            {
                string par = string.Empty;

                // only set if a proper match

                switch (ConditionTypeEnum)
                {
                    case GroupFilterConditionType.AirDate:
                    case GroupFilterConditionType.SeriesCreatedDate:
                    case GroupFilterConditionType.EpisodeAddedDate:
                    case GroupFilterConditionType.EpisodeWatchedDate:
                    case GroupFilterConditionType.LatestEpisodeAirDate:

                        if (ConditionOperatorEnum == GroupFilterOperator.LastXDays)
                            par += ConditionParameter;
                        else
                        {
                            DateTime airDate = ConditionParameter.GetDateFromString();
                            par += airDate.GetDateAsFriendlyString();
                        }
                        break;

                    case GroupFilterConditionType.AnimeGroup:

                        int groupId;
                        int.TryParse(ConditionParameter, out groupId);
                        if (groupId == 0) break;

                        if (!VM_MainListHelper.Instance.AllGroupsDictionary.ContainsKey(groupId)) break;

                        par += VM_MainListHelper.Instance.AllGroupsDictionary[groupId].GroupName;
                        break;

                    case GroupFilterConditionType.AnimeType:
                    case GroupFilterConditionType.Tag:
                    case GroupFilterConditionType.CustomTags:
                    case GroupFilterConditionType.VideoQuality:
                    case GroupFilterConditionType.AniDBRating:
                    case GroupFilterConditionType.UserRating:
                    case GroupFilterConditionType.AudioLanguage:
                    case GroupFilterConditionType.SubtitleLanguage:
                    case GroupFilterConditionType.Year:
                        par += ConditionParameter;
                        break;
                }
                return par;
            }
        }

        public string NiceDescription
        {
            get
            {
                string ret = string.Empty;
                ret += ConditionTypeEnum.GetTextForEnum_ConditionType();
                ret += " - ";

                ret += ConditionOperatorEnum.GetTextForEnum_Operator();

                if (ConditionTypeEnum == GroupFilterConditionType.AirDate)
                {
                    ret += " ";
                    DateTime airDate = ConditionParameter.GetDateFromString();
                    ret += airDate.GetDateAsFriendlyString();
                }

                return ret;

                // TODO - cater for parameters
            }
        }

        public VM_GroupFilterCondition()
        {
            GroupFilterConditionID = 0;
            GroupFilterID = 0;
            ConditionType = 1;
            ConditionOperator = 1;
            ConditionParameter = "";
        }

    }
}
