using System.Collections.Generic;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding
{
    public interface ITestData
    {
        string Name { get; }
        IList<ExtendedEntity<FirstLevelProjectionRole>> Roles { get; }
        IList<ExtendedProfile> Profiles { get; }
        IList<ExtendedEntity<FirstLevelProjectionFunction>> Functions { get; }
        IList<ExtendedEntity<FirstLevelProjectionTag>> Tags { get; }
        IList<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>> TemporaryAssignments { get; }
    }
}
