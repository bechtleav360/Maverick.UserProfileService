using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ProfileVertexEdgeAttribute: Attribute
{
    public string ObjectId { get; }
    
    public string RelatedId { get; }

    public ProfileVertexEdgeAttribute(string relatedId, string objectId)
    {
        ObjectId = objectId;
        RelatedId = relatedId;
    }
}
