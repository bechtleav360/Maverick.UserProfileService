using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;

public interface IPathTreeRepository
{
    internal Task<SecondLevelProjectionProfileEdgeData> RemoveRangeConditionsFromPathTreeEdgeAsync(
        string relatedEntityId,
        string parentId,
        string memberId,
        IList<RangeCondition> conditionsToRemove);

    internal Task<string> DeletePathTreeEdgeAsync(
        string relatedEntityId,
        string parentId,
        string memberId
    );
}
