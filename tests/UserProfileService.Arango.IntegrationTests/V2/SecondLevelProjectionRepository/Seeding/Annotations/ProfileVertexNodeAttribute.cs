using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ProfileVertexNodeAttribute : Attribute
{
    public string RelatedEntityId { get; }
    
    public ProfileVertexNodeAttribute(string relatedEntityId)
    {
        RelatedEntityId = relatedEntityId;
    }
}
