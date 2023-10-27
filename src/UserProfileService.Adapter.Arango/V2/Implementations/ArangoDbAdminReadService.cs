using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Configuration;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoDbAdminReadService : ArangoRepositoryBase, IAdminReadService
{
    private readonly IDbInitializer _databaseInitializer;
    private readonly ArangoPrefixSettings _prefixSettings;
    protected override string ArangoDbClientName { get; }

    public ArangoDbAdminReadService(
        IDbInitializer databaseInitializer,
        ILogger<ArangoDbAdminReadService> logger,
        IServiceProvider services,
        string arangoDbClientName,
        ArangoPrefixSettings prefixSettings)
        : base(logger, services)
    {
        _databaseInitializer = databaseInitializer;
        _prefixSettings = prefixSettings;
        ArangoDbClientName = arangoDbClientName;
    }

    private static CreateCursorBody AddStringFilter(
        CreateCursorBody request,
        string key,
        IList<string> names)
    {
        if (names == null || names.Count == 0)
        {
            return request;
        }

        request.BindVars.Add(key, JArray.FromObject(names));

        return request;
    }

    private static GroupedProjectionState AddTotalCount(
        GroupedProjectionState result,
        long? totalCount)
    {
        if (result == null)
        {
            return new GroupedProjectionState();
        }

        result.TotalCount = totalCount ?? -1;

        return result;
    }

    private string GetCorrectPropertyName(string orderedBy)
    {
        if (string.IsNullOrEmpty(orderedBy))
        {
            return null;
        }

        PropertyInfo sortingProperty = typeof(ProjectionState)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(
                p => p.Name.Equals(
                    orderedBy,
                    StringComparison.OrdinalIgnoreCase));

        if (sortingProperty == null)
        {
            Logger.LogInfoMessage(
                "Sorting property {sortingProperty} unknown for project state entry.",
                LogHelpers.Arguments(orderedBy));

            return null;
        }

        return sortingProperty.Name;
    }

    private (string sortingQueryPart, string paginationQueryPart)
        GetSortingAndPaginationQueryParts(
            IQueryObject paginationSettings,
            string iterator)
    {
        string correctOrderBy = GetCorrectPropertyName(paginationSettings?.OrderedBy);

        string sortingString = paginationSettings != null && !string.IsNullOrEmpty(correctOrderBy)
            ? $"SORT {iterator}.{paginationSettings.OrderedBy} {paginationSettings.SortOrder:G}"
            : string.Empty;

        string limitString = paginationSettings != null
            ? $"LIMIT {paginationSettings.Offset}, {paginationSettings.Limit}"
            : string.Empty;

        return (sortingString, limitString);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<ProjectionState>> GetFirstLevelProjectionStateAsync(
        PaginationQueryObject paginationSettings = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        string collectionName = DefaultModelConstellation
            .CreateNewSecondLevelProjection(_prefixSettings.FirstLevelCollectionPrefix)
            .ModelsInfo
            .GetCollectionName<ProjectionState>();

        await _databaseInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        (string sortingQueryPart, string paginationQueryPart) =
            GetSortingAndPaginationQueryParts(paginationSettings, "o");

        var aqlQuery = @$"
                             FOR o IN @@collection
                               {
                                   paginationQueryPart
                               }
                               {
                                   sortingQueryPart
                               }
                               RETURN o";

        MultiApiResponse<ProjectionState> multiResponse = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<ProjectionState>(
                new CreateCursorBody
                {
                    Options = new PostCursorOptions
                    {
                        FullCount = true
                    },
                    BindVars = new Dictionary<string, object>
                    {
                        { "@collection", collectionName }
                    },
                    Query = aqlQuery
                },
                cancellationToken: cancellationToken),
            cancellationToken: cancellationToken);

        long? totalCount = (multiResponse.Responses?
                .FirstOrDefault() as CursorResponse<ProjectionState>)
            ?.CursorDetails?.Extra?.Stats?.FullCount;

        return Logger.ExitMethod(
            multiResponse
                .QueryResult
                .ToPaginatedList(totalCount ?? -1));
    }

    /// <inheritdoc />
    public async Task<IList<ProjectionStateStatisticEntry>> GetProjectionStateStatisticAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _databaseInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        string aqlQuery = WellKnownAqlQueries.RetrieveProjectionStateStatistics(_prefixSettings);

        MultiApiResponse<ProjectionStateStatisticEntry> queryResponse = await
            SendRequestAsync(
                c => c.ExecuteQueryWithCursorOptionsAsync<ProjectionStateStatisticEntry>(
                    new CreateCursorBody
                    {
                        Options = new PostCursorOptions
                        {
                            FullCount = true
                        },
                        Query = aqlQuery
                    },
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);

        return Logger.ExitMethod(queryResponse.QueryResult.ToList());
    }

    /// <inheritdoc />
    public async Task<GroupedProjectionState> GetServiceProjectionStateAsync(
        IList<string> streamNames = null,
        PaginationQueryObject paginationSettings = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        string collectionName = DefaultModelConstellation
            .CreateNewSecondLevelProjection(_prefixSettings.ServiceCollectionPrefix)
            .ModelsInfo
            .GetCollectionName<ProjectionState>();

        await _databaseInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        (string sortingQueryPart, string paginationQueryPart) =
            GetSortingAndPaginationQueryParts(paginationSettings, "e");

        string filter = streamNames?.Count > 0
            ? $"FILTER @streamFilter ANY == o.{nameof(ProjectionState.StreamName)}"
            : string.Empty;

        // will result in one or none object with several properties limited by pagination settings
        var aqlQuery = @$"
                             FOR o in @@collection
                               {
                                   filter
                               }
                               COLLECT streamName = o.{
                                   nameof(ProjectionState.StreamName)
                               } INTO entries = o
                               {
                                   paginationQueryPart
                               }
                               RETURN {{
                                 [streamName]: (
                                                FOR e IN entries {
                                                    sortingQueryPart
                                                } RETURN e
                                               )
                               }}";

        MultiApiResponse<string> multiResponse = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<string>(
                AddStringFilter(
                    new CreateCursorBody
                    {
                        Options = new PostCursorOptions
                        {
                            FullCount = true
                        },
                        BindVars = new Dictionary<string, object>
                        {
                            { "@collection", collectionName }
                        },
                        Query = aqlQuery
                    },
                    "streamFilter",
                    streamNames),
                cancellationToken: cancellationToken),
            cancellationToken: cancellationToken);

        long? totalCount = (multiResponse.Responses?
                .FirstOrDefault() as CursorResponse<string>)
            ?.CursorDetails?.Extra?.Stats?.FullCount;

        var result = new GroupedProjectionState();

        foreach (string json in multiResponse.QueryResult)
        {
            JProperty jProperty = JObject.Parse(json).Properties().FirstOrDefault();

            if (jProperty == null)
            {
                continue;
            }

            var stateEntries = jProperty.Value.ToObject<List<ProjectionState>>();
            result.Add(jProperty.Name, stateEntries);
        }

        return Logger.ExitMethod(AddTotalCount(result, totalCount));
    }
}
