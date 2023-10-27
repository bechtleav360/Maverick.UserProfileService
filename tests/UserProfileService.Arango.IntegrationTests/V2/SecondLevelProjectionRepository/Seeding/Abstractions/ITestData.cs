using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions
{
    internal interface ITestData
    {
        IList<FunctionObjectEntityModel> Functions { get; }
        IList<GroupEntityModel> Groups { get; }
        IList<OrganizationEntityModel> Organizations { get; }
        IList<RoleObjectEntityModel> Roles { get; }
        IList<Tag> Tags { get; }
        IList<UserEntityModel> Users { get; }
        IList<ExtendedProfileEdgeData> EdgeData { get; }
        IList<ExtendedProfileVertexData> VertexData { get; }
    }
}
