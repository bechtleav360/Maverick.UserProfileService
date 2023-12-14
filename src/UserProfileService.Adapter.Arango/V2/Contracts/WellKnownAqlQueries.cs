using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.Configuration;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Models;
using UserProfileService.EventCollector.Abstractions;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     Contains all hard-coded aql statements.
/// </summary>
public sealed class WellKnownAqlQueries
{
    /// <summary>
    ///     Defines the maximal traversal depth for aql queries.
    /// </summary>
    internal const int MaxTraversalDepth = 1000;

    /// <summary>
    ///     Defines the name of property containing the revision.
    /// </summary>
    public const string RevisionProperty = "Revision";

    /// <summary>
    ///     Returns a query which will be removed all outbound beginning at first traversal depth.
    /// </summary>
    /// <param name="edgeCollection">The edge-collection to check within.</param>
    /// <param name="entityCollection">The collection where the entity is stored. Used to generate the internal arango key.</param>
    /// <param name="entityId">The id of the entity for which the outbound entities should be returned.</param>
    /// <param name="traversalDepth">The max depth to visit. It is caped to <see cref="MaxTraversalDepth" /></param>
    /// <param name="filterString">
    ///     An optional filter string directly injected into the query. No check will be done, if it is
    ///     correct.
    /// </param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql RetrieveAllOutboundEdges(
        string edgeCollection,
        string entityCollection,
        string entityId,
        int traversalDepth,
        Func<string, string> filterString = null)
    {
        return new ParameterizedAql
        {
            Query = @$"with @@collection 
                        Let from = DOCUMENT(@@collection, @key) 
                        FOR v, p, e IN 1..@depth OUTBOUND from._id @@edgeCollection 
                        {
                            filterString?.Invoke("v") ?? string.Empty
                        }
                        RETURN v",
            Parameter = new Dictionary<string, object>
            {
                { "key", entityId },
                { "@collection", entityCollection },
                { "depth", traversalDepth > MaxTraversalDepth ? MaxTraversalDepth : traversalDepth },
                { "@edgeCollection", edgeCollection }
            }
        };
    }

    internal static ParameterizedAql GetMembersOfProfileFilteredByMemberIds(
        string profileCollection,
        string profileId,
        ProfileKind profileKind,
        IList<string> memberIdFilter = null)
    {
        return new ParameterizedAql
        {
            Query = @$"
                       WITH @@collection
                       LET parent = FIRST(
                         FOR profile IN @@collection 
                           FILTER profile.Id==@profileId
                           AND profile.Kind == @kind
                           LIMIT 0,1
                         RETURN profile
                       )
                       LET check = (
                         WARN(parent != null, ""Parent entity not found [{ArangoRepoErrorCodes.ProfileNotFound}]"")
                       )
                       FOR m IN NOT_NULL(parent.Members, [])
                       FILTER COUNT(NOT_NULL(@memberIdFilter, [])) == 0 || m.Id IN @memberIdFilter
                       RETURN m",
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollection },
                { "profileId", profileId },
                { "kind", profileKind.ToString("G") },
                { "memberIdFilter", memberIdFilter ?? Array.Empty<string>() }
            }
        };
    }

    /// <summary>
    ///     Return a <see cref="IProfile" /> by using the external id property. The profile can be an
    ///     <see cref="Organization" />,
    ///     <see cref="Group" /> or an <see cref="User" /> object.
    /// </summary>
    /// <param name="profileCollection">The collection that is used to retrieve the profile</param>
    /// <param name="profileId">
    ///     The profileId identifies the profile by its id, if the
    ///     <paramref name="allowExternalIds" /> uses the default value true.
    /// </param>
    /// <param name="allowExternalIds">
    ///     The parameter has as default true. If true only the external id property must match,
    ///     otherwise the "own" id property.
    /// </param>
    /// <param name="source">Specifies than the profile should match a special source.</param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql RetrieveProfileByExternalOrInternalId(
        string profileCollection,
        string profileId,
        string source = null,
        bool allowExternalIds = true)
    {
        if (string.IsNullOrWhiteSpace(profileCollection))
        {
            throw new ArgumentException(nameof(profileCollection));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException(nameof(profileId));
        }

        string sourceQueryParameter =
            source == null ? string.Empty : $"AND LIKE(CURRENT.Source, \"{source}\", true)";

        return new ParameterizedAql
        {
            Query = @$"
                        FOR profile in ( FOR p in @@profilesCollection
                                         FILTER p.Id == @profileId OR 
                                         @allowExternalIds AND
                                         COUNT (p.ExternalIds[* FILTER CURRENT.Id == @profileId 
                                        {
                                            sourceQueryParameter
                                        }
                                        ]) > 0
                                        RETURN {{ object:p, Weight:p.Id == @profileId? 2:1 }}
                                        )
                       SORT profile.Weight DESC
                       RETURN profile.object",
            Parameter = new Dictionary<string, object>
            {
                { "allowExternalIds", allowExternalIds },
                { "@profilesCollection", profileCollection },
                { "profileId", profileId }
            }
        };
    }

    internal static string RetrieveProjectionStateStatistics(ArangoPrefixSettings prefixSettings)
    {
        string collectionNameFirstLevel = DefaultModelConstellation
            .CreateNew(prefixSettings.FirstLevelCollectionPrefix)
            .ModelsInfo
            .GetCollectionName<ProjectionState>();

        string collectionNameService = DefaultModelConstellation
            .CreateNew(prefixSettings.ServiceCollectionPrefix)
            .ModelsInfo
            .GetCollectionName<ProjectionState>();

        string collectionNameAssignments = DefaultModelConstellation
            .CreateNew(prefixSettings.AssignmentsCollectionPrefix)
            .ModelsInfo
            .GetCollectionName<ProjectionState>();


        if (string.IsNullOrWhiteSpace(collectionNameFirstLevel))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(ProjectionState)}",
                ExceptionSeverity.Error);
        }

        return @$"
                                 LET projections = [
                                    {{
                                        Name: ""First Level"",
                                        Events: (FOR e IN {
                                            collectionNameFirstLevel
                                        } RETURN e)
                                    }},
                                    {{
                                        Name: ""Service"",
                                        Events: (FOR e IN {
                                            collectionNameService
                                        } RETURN e)
                                    }},
                                    {{
                                        Name: ""Assignments"",
                                        Events: (FOR e IN {
                                            collectionNameAssignments
                                        } RETURN e)
                                    }}
                                 ]
                                 
                                 FOR p IN projections
                                    RETURN
                                    {{
                                        ""{
                                            nameof(ProjectionStateStatisticEntry.Projection)
                                        }"": p.Name,
                                        ""{
                                            nameof(ProjectionStateStatisticEntry.Events)
                                        }"": COUNT(p.Events),
                                        ""{
                                            nameof(ProjectionStateStatisticEntry.Errors)
                                        }"": COUNT(p.Events[* FILTER CURRENT.ErrorOccurred]),
                                        ""{
                                            nameof(ProjectionStateStatisticEntry.Time)
                                        }"": NOT_NULL(DATE_DIFF(
                                            FIRST(FOR t in p.Events[*].ProcessedOn SORT t ASC LIMIT 1 RETURN t),
                                            FIRST(FOR t in p.Events[*].ProcessedOn SORT t DESC  LIMIT 1 RETURN t),
                                            ""i"", false), -1),
                                        ""{
                                            nameof(ProjectionStateStatisticEntry.Rate)
                                        }"": ROUND(COUNT(p.Events) / DATE_DIFF(
                                            FIRST(FOR t in p.Events[*].ProcessedOn SORT t ASC LIMIT 1 RETURN t),
                                            FIRST(FOR t in p.Events[*].ProcessedOn SORT t DESC  LIMIT 1 RETURN t),
                                            ""i"", true) * 100)/100,
                                        ""{
                                            nameof(ProjectionStateStatisticEntry.AverageTime)
                                        }"": ROUND(100 * AVG(
                                            FOR e IN p.Events
                                            FILTER e.ProcessingStartedAt != null
                                            RETURN DATE_DIFF(e.ProcessingStartedAt, e.ProcessedOn, ""f"", true))
                                        )/100
                                    }}";
    }

    internal static string GetDefaultFilterString(
        RequestedProfileKind expectedKind,
        string iterator)
    {
        return expectedKind is RequestedProfileKind.Organization or RequestedProfileKind.Group or RequestedProfileKind.User
                ? $"FILTER {iterator}.{nameof(IProfile.Kind)}==\"{expectedKind.Convert():G}\""
                : string.Empty;
    }

    internal static ParameterizedAql GetQueryToGetFirstLevelProjectionStateItemsForCleanup(
        ModelBuilderOptions modelsInfo,
        DateTime dateFilter)
    {
        string projectionStateCollection = modelsInfo.GetCollectionName<ProjectionState>();

        if (string.IsNullOrWhiteSpace(projectionStateCollection))
        {
            throw new ArgumentException(
                $"{nameof(modelsInfo)} did not contain any information about the documents collection of {nameof(ProjectionState)} instances.",
                nameof(modelsInfo));
        }

        return new ParameterizedAql
        {
            Query = @"
                                  WITH @@projectionStateCollection
                                  LET lastElement = FIRST(
                                    FOR o IN @@projectionStateCollection
                                    SORT o.EventNumber DESC
                                    LIMIT 1
                                    RETURN o._key
                                    )
                                  
                                  FOR o in @@projectionStateCollection
                                  FILTER o._key != lastElement
                                     AND o.ProcessedOn != NULL 
                                     AND DATE_ISO8601(o.ProcessedOn) < DATE_ISO8601(@dateFilter)
                                  RETURN PARSE_IDENTIFIER(o)
                                  ",
            Parameter = new Dictionary<string, object>
            {
                { "dateFilter", dateFilter.ToString("O") },
                { "@projectionStateCollection", projectionStateCollection }
            }
        };
    }

    internal static ParameterizedAql GetQueryToGetFirstLevelBatchLogItemsForCleanup(
        ModelBuilderOptions modelsInfo,
        DateTime dateFilter,
        params EventStatus[] statusFilter)
    {
        string logCollection = modelsInfo.GetCollectionName<EventBatch>();

        if (string.IsNullOrWhiteSpace(logCollection))
        {
            throw new ArgumentException(
                $"{nameof(modelsInfo)} did not contain any information about the documents collection of {nameof(EventBatch)} instances.",
                nameof(modelsInfo));
        }

        string logEventCollection = modelsInfo.GetCollectionName<EventLogTuple>();

        if (string.IsNullOrWhiteSpace(logEventCollection))
        {
            throw new ArgumentException(
                $"{nameof(modelsInfo)} did not contain any information about the documents collection of {nameof(EventLogTuple)} instances.",
                nameof(modelsInfo));
        }

        string logToEventEdgeCollection = modelsInfo.GetRelation<EventBatch, EventLogTuple>().EdgeCollection;

        if (string.IsNullOrWhiteSpace(logToEventEdgeCollection))
        {
            throw new ArgumentException(
                $"{nameof(modelsInfo)} did not contain any information about the edge collection between {nameof(EventBatch)} to {nameof(EventLogTuple)} instances.",
                nameof(modelsInfo));
        }

        return new ParameterizedAql
        {
            Query = @"
                                  WITH @@logCollection, @@logEventCollection
                                  
                                  FOR t IN UNIQUE(FLATTEN(
                                    FOR o IN @@logCollection
                                    FILTER DATE_ISO8601(o.UpdatedAt) < DATE_ISO8601(@dateFilter)
                                       AND o.Status IN NOT_NULL(@statusFilter,[])
                                    FOR v,e,p IN 1..1 OUTBOUND o._id @@logEdges
                                    RETURN UNION(p.edges[* RETURN PARSE_IDENTIFIER(CURRENT._id)],
                                           p.vertices[* RETURN PARSE_IDENTIFIER(CURRENT._id)])
                                  ))
                                  RETURN t
                                  ",
            Parameter = new Dictionary<string, object>
            {
                { "statusFilter", statusFilter.Select(o => o.ToString("G")) },
                { "dateFilter", dateFilter.ToString("O") },
                { "@logCollection", logCollection },
                { "@logEventCollection", logEventCollection },
                { "@logEdges", logToEventEdgeCollection }
            }
        };
    }

    internal static ParameterizedAql GetQueryToGetEventCollectorItemsForCleanup(
        ModelBuilderOptions modelsInfo,
        DateTime dateFilter)
    {
        string startDataCollection = modelsInfo.GetCollectionName<StartCollectingEventData>();

        if (string.IsNullOrWhiteSpace(startDataCollection))
        {
            throw new ArgumentException(
                $"{nameof(modelsInfo)} did not contain any information about the documents collection of {nameof(StartCollectingEventData)} instances.",
                nameof(modelsInfo));
        }

        string dataCollection = modelsInfo.GetCollectionName<EventData>();

        if (string.IsNullOrWhiteSpace(dataCollection))
        {
            throw new ArgumentException(
                $"{nameof(modelsInfo)} did not contain any information about the documents collection of {nameof(EventData)} instances.",
                nameof(modelsInfo));
        }

        return new ParameterizedAql
        {
            Query = @"
                                  WITH @@startDataCollection, @@dataCollection
                                  FOR t IN FLATTEN(
                                    FOR t1 IN @@startDataCollection
                                      FOR t2 IN @@dataCollection
                                      FILTER t1.CompletedAt != null
                                         AND t1.CollectingId == t2.CollectingId
                                         AND DATE_ISO8601(t1.CompletedAt) < DATE_ISO8601(@dateFilter)
                                    RETURN [
                                     PARSE_IDENTIFIER(t1._id),
                                     PARSE_IDENTIFIER(t2._id)
                                    ]
                                  )
                                  RETURN t
                                  ",
            Parameter = new Dictionary<string, object>
            {
                { "dateFilter", dateFilter.ToString("O") },
                { "@startDataCollection", startDataCollection },
                { "@dataCollection", dataCollection }
            }
        };
    }
}
