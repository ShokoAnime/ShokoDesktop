using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AniDB_RecommendationVM : BindableObject
	{
		public int AniDB_RecommendationID { get; private set; }
		public int AnimeID { get; set; }
		public int UserID { get; set; }
		public int RecommendationType { get; set; }
		public string RecommendationText { get; set; }

		private string delayedUserImage = "";
		public string DelayedUserImage
		{
			get { return delayedUserImage; }
			set
			{
				delayedUserImage = value;
				base.RaisePropertyChanged("DelayedUserImage");
			}
		}

		public string UserImagePathForDisplay
		{
			get
			{
				return @"/Images/EpisodeThumb_NotFound.png";
			}
		}

		public AniDBRecommendationType RecommendationTypeEnum
		{
			get
			{
				return (AniDBRecommendationType)RecommendationType;
			}
		}

		public string RecommendationTypeText
		{
			get
			{
				switch (RecommendationTypeEnum)
				{
					case AniDBRecommendationType.ForFans: return "For Fans";
					case AniDBRecommendationType.Recommended: return "Recommended";
					case AniDBRecommendationType.MustSee: return "Must See";
					default: return "Recommended";
				}
			}
		}

		public AniDB_RecommendationVM()
		{
		}

		public AniDB_RecommendationVM(JMMServerBinary.Contract_AniDB_Recommendation contract)
		{
			this.AniDB_RecommendationID = contract.AniDB_RecommendationID;
			this.AnimeID = contract.AnimeID;
			this.UserID = contract.UserID;
			this.RecommendationType = contract.RecommendationType;
			this.RecommendationText = contract.RecommendationText;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", UserID, RecommendationText);
		}
	}
}
