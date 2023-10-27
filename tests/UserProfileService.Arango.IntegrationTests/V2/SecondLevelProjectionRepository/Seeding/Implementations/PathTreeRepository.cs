using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Implementations;

internal class PathTreeRepository : IPathTreeRepository
{
    private readonly IArangoDbClient _arangoDbClient;
    private readonly ModelBuilderOptions _options;
    private readonly JsonSerializer _jsonSerializer;

    internal PathTreeRepository(IArangoDbClient arangoDbClient, string prefix, JsonSerializer jsonSerializer)
    {
        _arangoDbClient = arangoDbClient;
        _options = DefaultModelConstellation.CreateNewSecondLevelProjection(prefix).ModelsInfo;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<SecondLevelProjectionProfileEdgeData> RemoveRangeConditionsFromPathTreeEdgeAsync(
        string relatedEntityId,
        string parentId,
        string memberId,
        IList<RangeCondition> conditionsToRemove)
    {
        ParameterizedAql query = WellKnownSecondLevelProjectionQueries.RemoveConditionsFromEdge(
            relatedEntityId,
            parentId,
            memberId,
            conditionsToRemove,
            _jsonSerializer,
            _options);

        var cursorQuery = new CreateCursorBody
                          {
                              Query = query.Query,
                              BindVars = query.Parameter,
                              BatchSize = 2
                          };

        MultiApiResponse<SecondLevelProjectionProfileEdgeData> edgeObject =
            await _arangoDbClient.ExecuteQueryWithCursorOptionsAsync<SecondLevelProjectionProfileEdgeData>(cursorQuery);

        if (edgeObject.QueryResult.Count == 0)
        {
            return null;
        }

        return edgeObject.QueryResult.First();
    }

    public async Task<string> DeletePathTreeEdgeAsync(
        string relatedEntityId,
        string parentId,
        string memberId)
    {
        ParameterizedAql query = WellKnownSecondLevelProjectionQueries.GetDeletePathTreeEdgesAql(
            relatedEntityId,
            parentId,
            memberId,
            _options);

        var cursorQuery = new CreateCursorBody
                          {
                              Query = query.Query,
                              BindVars = query.Parameter
                          };

        var edgeObject =
            await _arangoDbClient.ExecuteQueryWithCursorOptionsAsync<string>(cursorQuery);

        if (edgeObject.QueryResult.Count == 0)
        {
            return null;
        }

        return edgeObject.QueryResult.First();
    }
}
