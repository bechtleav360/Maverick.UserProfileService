using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ProfileVertexRootNodeAttribute: Attribute
{
}