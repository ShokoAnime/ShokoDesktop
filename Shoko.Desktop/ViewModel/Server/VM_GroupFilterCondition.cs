using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Enums;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupFilterCondition : GroupFilterCondition, INotifyPropertyChanged, INotifyPropertyChangedExt
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
                this.SetField(()=>base.ConditionType,(r)=> base.ConditionType = r, value, ()=>ConditionType, ()=> ConditionTypeEnum, ()=>ConditionTypeString, ()=>ConditionParameterString, ()=>NiceDescription);
            }
        }

        public new int ConditionOperator
        {
            get { return base.ConditionOperator; }
            set
            {
                this.SetField(()=>base.ConditionOperator,(r)=> base.ConditionOperator = r, value, ()=>ConditionOperator, ()=>ConditionOperatorEnum, ()=>ConditionOperatorString, () => ConditionParameterString, () => NiceDescription);
            }
        }

        public new string ConditionParameter
        {
            get { return base.ConditionParameter; }
            set
            {
                this.SetField(()=>base.ConditionParameter,(r)=> base.ConditionParameter = r, value, () => ConditionParameter, () => ConditionParameterString, () => NiceDescription);
            }
        }

        [JsonIgnore, XmlIgnore]
        public GroupFilterConditionType ConditionTypeEnum => (GroupFilterConditionType)ConditionType;

        [JsonIgnore, XmlIgnore]
        public GroupFilterOperator ConditionOperatorEnum => (GroupFilterOperator)ConditionOperator;

        [JsonIgnore, XmlIgnore]
        public string ConditionTypeString => ConditionTypeEnum.GetTextForEnum_ConditionType();

        [JsonIgnore, XmlIgnore]
        public string ConditionOperatorString => ConditionOperatorEnum.GetTextForEnum_Operator();

        [JsonIgnore, XmlIgnore]
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
                    case GroupFilterConditionType.Season:
                        par += ConditionParameter;
                        break;
                }
                return par;
            }
        }

        [JsonIgnore, XmlIgnore]
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
