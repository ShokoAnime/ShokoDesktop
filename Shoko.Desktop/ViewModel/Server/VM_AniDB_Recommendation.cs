using Shoko.Commons.Extensions;
using Shoko.Models.Enums;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Recommendation : AniDB_Recommendation
    {
        public string CommentTruncated => this.GetCommentTruncated();
        public string Comment => this.GetComment();
        public AniDBRecommendationType RecommendationTypeEnum => this.GetRecommendationTypeEnum();

        public string RecommendationTypeText => this.GetRecommendationTypeText();
    }
}
