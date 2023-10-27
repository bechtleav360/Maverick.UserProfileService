using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding
{
    public static class ProfileTreeDataBuilder
    {
        public static (IList<ExtendedProfileVertexData> vertices, ExtendedProfileEdgeData edge) CreateTreeData(
            string relatedProfileId,
            string parentObjectId,
            string childObjectId,
            IList<TagAssignment> parentTags = null,
            IList<TagAssignment> childTags = null,
            IList<RangeCondition> conditions = null)
        {
            var fromVertexKey = $"{relatedProfileId}-{childObjectId}";
            var toVertexKey = $"{relatedProfileId}-{parentObjectId}";

            return (new List<ExtendedProfileVertexData>
                {
                    new ExtendedProfileVertexData
                    {
                        RelatedProfileId = relatedProfileId,
                        ObjectId = childObjectId,
                        Key = fromVertexKey,
                        Tags = childTags != null && childTags.Count > 0
                            ? childTags
                            : new List<TagAssignment>()
                    },
                    new ExtendedProfileVertexData
                    {
                        RelatedProfileId = relatedProfileId,
                        ObjectId = parentObjectId,
                        Key = toVertexKey,
                        Tags = parentTags != null && parentTags.Count > 0
                            ? parentTags
                            : new List<TagAssignment>()
                    }
                },
                new ExtendedProfileEdgeData
                {
                    RelatedProfileId = relatedProfileId,
                    FromId = fromVertexKey,
                    ToId = toVertexKey,
                    Conditions = conditions
                        ?? new List<RangeCondition>
                        {
                            new RangeCondition()
                        }
                });
        }
    }
}
