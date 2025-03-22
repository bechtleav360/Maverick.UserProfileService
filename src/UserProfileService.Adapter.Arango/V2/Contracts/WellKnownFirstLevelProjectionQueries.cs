using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using RangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

internal static class WellKnownFirstLevelProjectionQueries
{
    /// <summary>
    ///     Returns an Query which will insert a new or update the existing edge connecting the given entities.
    /// </summary>
    /// <param name="edgeCollection">The edge-collection name to insert the values into.</param>
    /// <param name="fromCollection">The collection name of the from-entity.</param>
    /// <param name="fromId">The id of the from-entity.</param>
    /// <param name="toCollection">The collection name of the to-entity.</param>
    /// <param name="toId">The id of the to-entity.</param>
    /// <param name="fromExtraProperties">A string-array containing all properties to copy from the from-entity.</param>
    /// <param name="toExtraProperties">A string-array containing all properties to copy from the to-entity.</param>
    /// <param name="extraProperties">Defines additional properties which will be stored in the edge.</param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql InsertEdge(
        string edgeCollection,
        string fromCollection,
        string fromId,
        string toCollection,
        string toId,
        string[] fromExtraProperties = null,
        string[] toExtraProperties = null,
        JObject extraProperties = null)
    {
        return new ParameterizedAql
        {
            // ReSharper disable StringLiteralTypo
            Query = @"
                            LET toProps = NOT_NULL(@extraToProps, []) 
                            LET fromProps = NOT_NULL(@extraFromProps, []) 
                            LET froms = [DOCUMENT(@fromCollection, @fromId)] 
                            LET tos = [DOCUMENT(@toCollection, @toId)]

                            FOR to IN tos
                            FILTER to != null
                                FOR from IN froms
                                FILTER from != null
                                LET edge = MERGE({_from: from._id, _to: to._id}, 
                                    ZIP(toProps[* RETURN CONCAT('to_', CURRENT)], toProps[* RETURN to[CURRENT]]),
                                    ZIP(fromProps[* RETURN CONCAT('from_', CURRENT)], fromProps[* RETURN from[CURRENT]]),
                                    NOT_NULL(@extraProps, {}))
                                UPSERT {_from: from._id, _to: to._id } 
                                INSERT edge REPLACE edge INTO @@targetEdgeCollection
                                RETURN NEW._id",
            // ReSharper restore StringLiteralTypo
            Parameter = new Dictionary<string, object>
            {
                { "@targetEdgeCollection", edgeCollection },
                { "fromCollection", fromCollection },
                { "fromId", fromId },
                { "toCollection", toCollection },
                { "toId", toId },
                { "extraFromProps", fromExtraProperties },
                { "extraToProps", toExtraProperties },
                { "extraProps", extraProperties }
            }
        };
    }

    internal static ParameterizedAql GetClientSettings(
        string profileId,
        string profilesCollection,
        string clientSettingsCollection,
        string assignmentsEdge,
        string clientSettingsLinks,
        string functionsCollection)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            WITH @@profilesCollection, @@clientSettingsCollection, @@functionCollection
                            LET startVertex = DOCUMENT(@@profilesCollection, @profileId)

                            FOR v, e, p IN 1..100 OUTBOUND startVertex @@assignmentsCollection, @@settingsLinks
                            OPTIONS {{ vertexCollections: ['{
                                profilesCollection
                            }', '{
                                clientSettingsCollection
                            }'] }}
                            FILTER IS_SAME_COLLECTION(@@clientSettingsCollection, v)
                            LET conditions = POP(p.edges)

                            RETURN {{
                                {
                                    nameof(FirstLevelProjectionsClientSetting.SettingsKey)
                                }: v.Key,
                                {
                                    nameof(FirstLevelProjectionsClientSetting.Value)
                                }: v.Value,
                                {
                                    nameof(FirstLevelProjectionsClientSetting.ProfileId)
                                }: v.ProfileId,
                                {
                                    nameof(FirstLevelProjectionsClientSetting.Hops)
                                }: COUNT(p.edges) - 1,
                                {
                                    nameof(FirstLevelProjectionsClientSetting.Weight)
                                }: NOT_NULL(LAST(POP(p.vertices)).{
                                    nameof(FirstLevelProjectionGroup.Weight)
                                }, 0),
                                {
                                    nameof(FirstLevelProjectionsClientSetting.UpdatedAt)
                                }: LAST(POP(p.vertices)).{
                                    nameof(IFirstLevelProjectionProfile.UpdatedAt)
                                },
                                {
                                    nameof(FirstLevelProjectionsClientSetting.Conditions)
                                }: ZIP(
                                    conditions[*RETURN PARSE_IDENTIFIER(CURRENT._to).key],
                                    conditions[*RETURN CURRENT.Conditions])
                            }}",
            Parameter = new Dictionary<string, object>
            {
                { "profileId", profileId },
                { "@profilesCollection", profilesCollection },
                { "@clientSettingsCollection", clientSettingsCollection },
                { "@assignmentsCollection", assignmentsEdge },
                { "@settingsLinks", clientSettingsLinks },
                { "@functionCollection", functionsCollection}
            }
        };
    }

    /// <summary>
    ///     Inserts the given document or replaces a document matching the filter with the new value.
    /// </summary>
    /// <param name="collection">The collection in which to look for the value.</param>
    /// <param name="filter">The object which should be used for filtering.</param>
    /// <param name="replaceWith">THe value to insert into the collection.</param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql UpsertDocument(
        string collection,
        JObject filter,
        JObject replaceWith)
    {
        return new ParameterizedAql
        {
            Query = "UPSERT @predicate INSERT @value REPLACE @value INTO @@collection RETURN NEW._key",
            Parameter = new Dictionary<string, object>
            {
                { "predicate", filter },
                { "value", replaceWith },
                { "@collection", collection }
            }
        };
    }

    internal static ParameterizedAql GetParents(ModelBuilderOptions options, string profileId)
    {
        return new ParameterizedAql
        {
            Query = @"
                            WITH @@profilesCollection, @@rolesCollection, @@functionsCollection
                            LET startVertex = DOCUMENT(@@profilesCollection, @profileId)

                            FOR relation IN (
                                FOR v, e, p IN 1..2 OUTBOUND startVertex @@assignmentsCollection, @@functionLinks
                                LET isFunction = IS_SAME_COLLECTION(@@functionLinks, e)
                                FILTER isFunction || (COUNT(p.edges) == 1 && !isFunction)
                                
                                RETURN {
                                    Container: (!isFunction ? v : p.vertices[1]).Id,
                                    Vertex: v,
                                    IsFunction: isFunction
                                }
                            )

                            COLLECT container = relation.Container INTO containerVertices = relation
                            LET mainVertex = FIRST(containerVertices[* FILTER !CURRENT.IsFunction])
                            RETURN MERGE(
                                mainVertex.Vertex, 
                                IS_SAME_COLLECTION(mainVertex.Vertex, @@functionsCollection) 
                                ? {
                                    Role: FIRST(containerVertices[* FILTER IS_SAME_COLLECTION(@@rolesCollection, CURRENT.Vertex)]).Vertex,
                                    Organization: FIRST(containerVertices[* FILTER IS_SAME_COLLECTION(@@profilesCollection, CURRENT.Vertex)]).Vertex
                                }
                                : {}
                            )",
            Parameter = new Dictionary<string, object>
            {
                { "@profilesCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "@rolesCollection", options.GetCollectionName<FirstLevelProjectionRole>() },
                { "@functionsCollection", options.GetCollectionName<FirstLevelProjectionFunction>() },
                { "profileId", profileId },
                {
                    "@assignmentsCollection", options
                        .GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>()
                        ?.EdgeCollection
                    ?? throw new InvalidOperationException("The model builder does not seem to be st up correctly")
                },
                {
                    "@functionLinks", options.GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionRole>()
                        ?.EdgeCollection
                    ?? throw new InvalidOperationException("The model builder does not seem to be st up correctly")
                }
            }
        };
    }

    internal static ParameterizedAql GetFunction(ModelBuilderOptions options, string functionId)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            WITH @@profilesCollection,  @@rolesCollection, @@functionsCollection
                            LET function = DOCUMENT(@@functionsCollection, @functionId)

                            LET relations = (
                                FOR v IN 1..1 OUTBOUND function @@functionLinks RETURN v
                            )

                            RETURN MERGE(
                                function, 
                                {{
                                    {
                                        nameof(FirstLevelProjectionFunction.Role)
                                    }: FIRST(relations[* FILTER IS_SAME_COLLECTION(@@rolesCollection, CURRENT)]),
                                    {
                                        nameof(FirstLevelProjectionFunction.Organization)
                                    }: FIRST(relations[* FILTER IS_SAME_COLLECTION(@@profilesCollection, CURRENT)])
                                }})",
            Parameter = new Dictionary<string, object>
            {
                { "@profilesCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "@rolesCollection", options.GetCollectionName<FirstLevelProjectionRole>() },
                { "@functionsCollection", options.GetCollectionName<FirstLevelProjectionFunction>() },
                { "functionId", functionId },
                {
                    "@functionLinks", options
                        .GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionRole>()
                        ?.EdgeCollection
                    ?? throw new NotSupportedException("The model builder seems to has a missing implementation")
                }
            }
        };
    }

    internal static ParameterizedAql GetFunctionsOfOrganization(ModelBuilderOptions options, string organizationId)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            WITH @@profilesCollection,  @@rolesCollection, @@functionsCollection
                            let organization = Document(@@profilesCollection, @organizationId)
                            FOR function in 1..1 INBOUND organization @@functionLinks
                                LET relations = (
                                    FOR v IN 1..1 OUTBOUND function @@functionLinks RETURN v
                                )

                                RETURN MERGE(
                                    function, 
                                    {{
                                        {
                                            nameof(FirstLevelProjectionFunction.Role)
                                        }: FIRST(relations[* FILTER IS_SAME_COLLECTION(@@rolesCollection, CURRENT)]),
                                        {
                                            nameof(FirstLevelProjectionFunction.Organization)
                                        }: FIRST(relations[* FILTER IS_SAME_COLLECTION(@@profilesCollection, CURRENT)])
                                    }})",
            Parameter = new Dictionary<string, object>
            {
                { "@profilesCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "@rolesCollection", options.GetCollectionName<FirstLevelProjectionRole>() },
                { "@functionsCollection", options.GetCollectionName<FirstLevelProjectionFunction>() },
                { "organizationId", organizationId },
                {
                    "@functionLinks", options
                        .GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionRole>()
                        ?.EdgeCollection
                    ?? throw new NotSupportedException("The model builder seems to has a missing implementation")
                }
            }
        };
    }

    internal static ParameterizedAql GetDifferencesInParentTree(
        ModelBuilderOptions options,
        string profileId,
        IList<string> relatedIds)
    {
        return new ParameterizedAql
        {
            Query = $@"
                        WITH @@profilesCollection, @@tagsCollection, @@rolesCollection, @@functionsCollection
                        LET startVertex = DOCUMENT(@@profilesCollection, @profileId)
                        LET referenceProfiles = @referenceProfiles[* RETURN DOCUMENT(@@profilesCollection, CURRENT)]

                        LET profileTags = (
                            FOR v,e IN 1 OUTBOUND startVertex @@tagLinks
                            RETURN {{
                                {
                                    nameof(TagAssignment.IsInheritable)
                                }: NOT_NULL(e.{
                                    nameof(FirstLevelProjectionTagAssignment.IsInheritable)
                                }, false),
                                {
                                    nameof(TagAssignment.TagDetails)
                                }: v
                            }}
                        )

                        LET parentTreeRelations = (
                            FOR relation IN (
                                FOR v, e, p IN 1..100 OUTBOUND startVertex @@assignmentsCollection, @@tagLinks, @@functionLinks
                                LET isFunction = IS_SAME_COLLECTION(@@functionLinks, e)
                                FILTER isFunction || COUNT(p.edges[* FILTER IS_SAME_COLLECTION(@@functionLinks, CURRENT)]) == 0       
                                LET isTagLink = IS_SAME_COLLECTION(@@tagLinks, e)

                                RETURN {{
                                    Container: ((v.ContainerType != null && !isFunction) ? v : p.vertices[LENGTH(p.vertices) - 2]).{
                                        nameof(IFirstLevelProjectionProfile.Id)
                                    },
                                    Vertex: v,
                                    Edge: e,
                                    Child: p.vertices[LENGTH(p.vertices) - (isFunction || isTagLink ? 3 : 2)]
                                }})
                            
                            COLLECT container = relation.Container, child = relation.Child INTO containerVertices = relation
                            LET mainVertex = FIRST(containerVertices[* FILTER IS_SAME_COLLECTION(@@assignmentsCollection, CURRENT.Edge)])
                            FILTER mainVertex != null
                            RETURN {{ 
                                {
                                    nameof(FirstLevelProjectionTreeEdgeRelation.Parent)
                                }: MERGE(
                                    mainVertex.Vertex, 
                                    IS_SAME_COLLECTION(mainVertex.Vertex, @@functionsCollection) 
                                    ? {{
                                        {
                                            nameof(FirstLevelProjectionFunction.Role)
                                        }: FIRST(containerVertices[* FILTER IS_SAME_COLLECTION(@@rolesCollection, CURRENT.Vertex)]).Vertex,
                                        {
                                            nameof(FirstLevelProjectionFunction.Organization)
                                        }: FIRST(containerVertices[* FILTER IS_SAME_COLLECTION(@@profilesCollection, CURRENT.Vertex)]).Vertex
                                    }}
                                    : {{}}),
                                {
                                    nameof(FirstLevelProjectionTreeEdgeRelation.ParentTags)
                                }: containerVertices[* 
                                    FILTER IS_SAME_COLLECTION(@@tagsCollection, CURRENT.Vertex) 
                                    RETURN {{
                                        {
                                            nameof(TagAssignment.IsInheritable)
                                        }: NOT_NULL(CURRENT.Edge.{
                                            nameof(FirstLevelProjectionTagAssignment.IsInheritable)
                                        }, false),
                                        {
                                            nameof(TagAssignment.TagDetails)
                                        }: CURRENT.Vertex
                                    }}
                                ],
                                {
                                    nameof(FirstLevelProjectionTreeEdgeRelation.Child)
                                }: {{
                                    {
                                        nameof(ObjectIdent.Id)
                                    }: child.{
                                        nameof(IFirstLevelProjectionProfile.Id)
                                    },
                                    {
                                        nameof(ObjectIdent.Type)
                                    }: child.{
                                        nameof(IFirstLevelProjectionProfile.Kind)
                                    }
                                }},
                                {
                                    nameof(FirstLevelProjectionTreeEdgeRelation.Conditions)
                                }: mainVertex.Edge.Conditions
                            }}
                        )

                        FOR profile IN referenceProfiles
                            LET parents = (FOR v IN 1..100 OUTBOUND profile @@assignmentsCollection RETURN v.{
                                nameof(IFirstLevelProjectionProfile.Id)
                            })
                            RETURN {{
                                {
                                    nameof(FirstLevelProjectionParentsTreeDifferenceResult.MissingRelations)
                                }: parentTreeRelations[*FILTER CURRENT.Child.Id NOT IN parents],
                                {
                                    nameof(FirstLevelProjectionParentsTreeDifferenceResult.ReferenceProfileId)
                                }: profile.Id,
                                {
                                    nameof(FirstLevelProjectionParentsTreeDifferenceResult.Profile)
                                }: startVertex,
                                {
                                    nameof(FirstLevelProjectionParentsTreeDifferenceResult.ProfileTags)
                                }: profileTags
                            }}
                        ",
            Parameter = new Dictionary<string, object>
            {
                { "@profilesCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "@tagsCollection", options.GetCollectionName<FirstLevelProjectionTag>() },
                { "@rolesCollection", options.GetCollectionName<FirstLevelProjectionRole>() },
                { "@functionsCollection", options.GetCollectionName<FirstLevelProjectionFunction>() },
                { "profileId", profileId },
                { "referenceProfiles", relatedIds },
                {
                    "@assignmentsCollection", options
                        .GetRelation<IFirstLevelProjectionProfile,
                            FirstLevelProjectionGroup>()
                        ?.EdgeCollection
                    ?? throw new InvalidOperationException("The model builder does not seem to be st up correctly")
                },
                {
                    "@tagLinks", options.GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionTag>()
                        ?.EdgeCollection
                    ?? throw new InvalidOperationException("The model builder does not seem to be st up correctly")
                },
                {
                    "@functionLinks", options
                        .GetRelation<FirstLevelProjectionFunction,
                            FirstLevelProjectionRole>()
                        ?.EdgeCollection
                    ?? throw new InvalidOperationException("The model builder does not seem to be st up correctly")
                }
            }
        };
    }

    /// <summary>
    ///     Fetches the key of the client settings document based on the profileId and key of the client setting.
    /// </summary>
    /// <param name="collection">The collection in which to look for the value.</param>
    /// <param name="profileId">The id of the profile.</param>
    /// <param name="key">THe key of the client settings.</param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql FindClientSettings(
        string collection,
        string profileId,
        string key)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            FOR cs IN @@collection
                            FILTER cs.{
                                nameof(FirstLevelProjectionClientSettingsBasic.Key)
                            } == @key AND cs.{
                                nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                            } == @profileId
                            RETURN cs._key",
            Parameter = new Dictionary<string, object>
            {
                { "key", key },
                { "profileId", profileId },
                { "@collection", collection }
            }
        };
    }

    internal static ParameterizedAql GetDirectContainerMembers(
        string entityId,
        string entityCollection,
        ModelBuilderOptions modelsInfo)
    {
        if (modelsInfo == null)
        {
            throw new ArgumentNullException(nameof(modelsInfo));
        }

        string assignmentsCollection =
            modelsInfo.GetRelation(typeof(IFirstLevelProjectionProfile), typeof(FirstLevelProjectionGroup))
                ?.EdgeCollection
            ?? throw new ArgumentException("The model builder des not seem set up correctly.", nameof(modelsInfo));

        string functionLinksCollection =
            modelsInfo.GetRelation(typeof(FirstLevelProjectionFunction), typeof(FirstLevelProjectionRole))
                ?.EdgeCollection
            ?? throw new ArgumentException("The model builder des not seem set up correctly.", nameof(modelsInfo));

        string profilesCollection = modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>();
        string functionsCollection = modelsInfo.GetCollectionName<FirstLevelProjectionFunction>();

        return new ParameterizedAql
        {
            Query = $@"
                           WITH @@entityCollection, @@profilesCollection, @@functionCollection
                           LET startVertex = DOCUMENT('{
                               entityCollection
                           }', '{
                               entityId
                           }')
                           RETURN {{
                               ""{
                                   nameof(FirstLevelProjectionTraversalResponse<IFirstLevelProjectionProfile>
                                       .IsStartVertexKnown)
                               }"": startVertex != null,
                               ""{
                                   nameof(FirstLevelProjectionTraversalResponse<ObjectIdent>.Response)
                               }"": (
                                   FOR v,e,p IN 1..1 INBOUND startVertex  @@assignmentCollection, @@functionLinksCollection
                                   FILTER IS_SAME_COLLECTION(@@profilesCollection, v)
                                   AND (
                                        LENGTH(REMOVE_NTH(p.vertices, 1)[* FILTER IS_SAME_COLLECTION(@@functionCollection, CURRENT)]) == 0 
                                        OR IS_SAME_COLLECTION(@@functionCollection, p.vertices[0]))
                                   RETURN DISTINCT {{""{
                                       nameof(ObjectIdent.Id)
                                   }"":v.{
                                       nameof(IFirstLevelProjectionProfile.Id)
                                   },
                                                     ""{
                                                         nameof(ObjectIdent.Type)
                                                     }"":NOT_NULL(v.ContainerType, v.Kind, ""Unknown"")
                                                    }} 
                                    )
                                }}",
            Parameter = new Dictionary<string, object>
            {
                { "@assignmentCollection", assignmentsCollection },
                { "@functionCollection", functionsCollection },
                { "@profilesCollection", profilesCollection },
                { "@functionLinksCollection", functionLinksCollection },
                { "@entityCollection", entityCollection }
            }
        };
    }

    /// <summary>
    ///     Gets temporary assignments currently active
    /// </summary>
    /// <param name="collection">The collection in which to look for the value.</param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql GetTemporaryAssignments(
        string collection)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            LET now = DATE_ISO8601(DATE_NOW())
                            FOR a IN @@collection
                              LET start = DATE_ISO8601(NOT_NULL(a.{
                                  nameof(FirstLevelProjectionTemporaryAssignment.Start)
                              },
                                 ""0000-01-01 00:00:00.000Z""))
                              LET end = DATE_ISO8601(NOT_NULL(a.{
                                  nameof(FirstLevelProjectionTemporaryAssignment.End)
                              },
                                 ""9999-12-31 23:59:59.999Z""))
                              LET state = NOT_NULL(a.{
                                  nameof(FirstLevelProjectionTemporaryAssignment.State)
                              }, ""{
                                  nameof(TemporaryAssignmentState.NotProcessed)
                              }"")
                              LET notificationState = NOT_NULL(a.{
                                  nameof(FirstLevelProjectionTemporaryAssignment.NotificationStatus)
                              }, ""{
                                  nameof(NotificationStatus.NoneSent)
                              }"")
                              FILTER start <= now
                                   AND end >= now
                                   AND state == ""{
                                       nameof(TemporaryAssignmentState.NotProcessed)
                                   }"" AND notificationState == ""{
                                       nameof(NotificationStatus.NoneSent)
                                   }""
                                   OR  end < now AND state == ""{
                                       nameof(TemporaryAssignmentState.ActiveWithExpiration)
                                   }"" 
                                       AND (notificationState == ""{
                                           nameof(NotificationStatus.ActivationSent)
                                       }"" 
                                            OR notificationState == ""{
                                                nameof(NotificationStatus.NoneSent)
                                            }"" 
                                           )
                            RETURN a",
            Parameter = new Dictionary<string, object>
            {
                { "@collection", collection }
            }
        };
    }

    /// <summary>
    ///     Updates <paramref name="desiredState" /> in the backend. The result of the query will be a list of ids of all
    ///     replaced entities.
    /// </summary>
    /// <param name="desiredState">The state to be persisted.</param>
    /// <param name="collection">The collection in which to look for the value.</param>
    /// <returns>A <see cref="ParameterizedAql" /> containing the query and all necessary parameter.</returns>
    internal static ParameterizedAql ReplaceExistingTemporaryAssignments(
        IList<FirstLevelProjectionTemporaryAssignment> desiredState,
        string collection)
    {
        JToken input = desiredState.GetJsonDocument();

        string keptProperties = string.Join(
            "\",\"",
            nameof(FirstLevelProjectionTemporaryAssignment.Id),
            nameof(FirstLevelProjectionTemporaryAssignment.ProfileId),
            nameof(FirstLevelProjectionTemporaryAssignment.ProfileType),
            nameof(FirstLevelProjectionTemporaryAssignment.TargetId),
            nameof(FirstLevelProjectionTemporaryAssignment.TargetType),
            nameof(FirstLevelProjectionTemporaryAssignment.Start),
            nameof(FirstLevelProjectionTemporaryAssignment.End));

        // Status:
        // (we expect start < end, because of the validation during processing requests to make thins easier)
        // [also seen in TemporaryAssignmentState enum comments]
        //
        //   active: start less equal than now() AND end equals max() or null
        //   inactive: end less than now()
        //   notProcessed: start greater than now()
        //   activeWithExpiration. start less equal than now() AND end greater now() BUT less than max()
        //
        //    [---inactive---]
        //                            [-------------active--------------]
        //            [---inactive----]
        //                          [---active w/ expiration---]
        //                                       [--not processed--]
        //                          
        // MIN/NULL <------ PAST ------>| NOW |<----- FUTURE --------> MAX/NULL

        return new ParameterizedAql
        {
            Query = $@"
                            FOR a IN @@collection
                            LET existing = FIRST(
                               FOR o IN @input
                                  FILTER KEEP(o,
                                          ""{
                                              keptProperties
                                          }"") == 
                                        KEEP(a,
                                          ""{
                                              keptProperties
                                          }"")
                                  LIMIT 1
                                  RETURN o)
                            FILTER existing != NULL
                            REPLACE MERGE(existing,
                                         {{ 
                                           {
                                               AConstants.KeySystemProperty
                                           }: a.{
                                               AConstants.KeySystemProperty
                                           },
                                          {
                                              nameof(FirstLevelProjectionTemporaryAssignment.State)
                                          }: existing.{
                                              nameof(FirstLevelProjectionTemporaryAssignment.State)
                                          },
                                          {
                                              nameof(FirstLevelProjectionTemporaryAssignment.NotificationStatus)
                                          }: existing.{
                                              nameof(FirstLevelProjectionTemporaryAssignment.NotificationStatus)
                                          },
                                          {
                                              nameof(FirstLevelProjectionTemporaryAssignment.LastModified)
                                          }: DATE_ISO8601(DATE_NOW()) 
                                        }})
                               IN @@collection
                            RETURN NEW.Id",
            Parameter = new Dictionary<string, object>
            {
                { "input", input },
                { "@collection", collection }
            }
        };
    }

    internal static ParameterizedAql InsertTemporaryAssignmentEntries(
        ObjectType profileType,
        IList<FirstLevelProjectionTemporaryAssignment> newSet,
        string tempAssignmentCollection)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            FOR a IN @newSet 
                               INSERT MERGE(a,
                                   {{
                                      {
                                          AConstants.KeySystemProperty
                                      }: a.{
                                          nameof(FirstLevelProjectionTemporaryAssignment.Id)
                                      },
                                      {
                                          nameof(FirstLevelProjectionTemporaryAssignment.ProfileType)
                                      }: @pType
                                   }})
                               INTO @@mainCollection
                               RETURN NEW",
            Parameter = new Dictionary<string, object>
            {
                { "newSet", newSet.GetJsonDocument() },
                { "pType", profileType.ToString("G") },
                { "@mainCollection", tempAssignmentCollection }
            }
        };
    }

    internal static ParameterizedAql DeleteAllTemporaryAssignmentsRelatedToObject(
        string profileId,
        string collectionName)
    {
        return new ParameterizedAql
        {
            // all temporary assignments related to profileId (regardless the "relationship direction")
            Query = $@"
                            FOR a IN @@collection
                               FILTER a.{
                                   nameof(FirstLevelProjectionTemporaryAssignment.ProfileId)
                               }==@deletedId
                                 OR a.{
                                     nameof(FirstLevelProjectionTemporaryAssignment.TargetId)
                                 }==@deletedId
                               REMOVE a IN @@collection RETURN OLD.{
                                   nameof(FirstLevelProjectionTemporaryAssignment.Id)
                               }",
            Parameter = new Dictionary<string, object>
            {
                { "deletedId", profileId },
                { "@collection", collectionName }
            }
        };
    }

    internal static ParameterizedAql DeleteSpecifiedTemporaryAssignments(
        string profileId,
        string targetId,
        IList<RangeCondition> conditions,
        string collectionName)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            FOR a IN @@collection
                               FILTER @condition ANY == a.{
                                   nameof(FirstLevelProjectionTemporaryAssignment.CompoundKey)
                               }
                               REMOVE a IN @@collection RETURN OLD.{
                                   nameof(FirstLevelProjectionTemporaryAssignment.Id)
                               }",
            Parameter = new Dictionary<string, object>
            {
                {
                    "condition", conditions
                        .CalculateTemporaryAssignmentCompoundKeys(
                            profileId,
                            targetId)
                        .ToList()
                        .GetJsonDocument()
                },
                { "@collection", collectionName }
            }
        };
    }

    /// <summary>
    ///     Returns a <see cref="ParameterizedAql" /> containing a query to get entities that are involved in a property
    ///     changed on a certain entity.<br/>
    ///     This can been useful, i.e. if the name of an organization has been changed, which is also
    ///     part of a function. Modifying the organizations name leads to a change of the function name as well.
    /// </summary>
    /// <param name="entityId">The id of the entity whose properties has been changed.</param>
    /// <param name="entityType">The type of the entity whose properties has been changed.</param>
    /// <param name="options">The model builder options containing all information about the used mode.</param>
    /// <returns>The query container that can be executed to get all relevant objects.</returns>
    /// <exception cref="NotSupportedException"><paramref name="entityType" /> does not have any relations to a profile.</exception>
    public static ParameterizedAql GetAllRelevantObjectsBecauseOfPropertyChangedQuery(
        string entityId,
        Type entityType,
        ModelBuilderOptions options)
    {
        ModelBuilderOptionsTypeRelation assignmentsCollection =
            options.GetRelation(typeof(IFirstLevelProjectionProfile), entityType)
            ?? throw new NotSupportedException($"The entity-type {entityType.Name} is not suited.");

        ModelBuilderOptionsTypeRelation functionLinksCollection =
            options.GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionOrganization>();

        string entityCollection = options.GetCollectionName(entityType);
        string functionsCollection = options.GetCollectionName<FirstLevelProjectionFunction>();
        string rolesCollection = options.GetCollectionName<FirstLevelProjectionRole>();
        string profilesCollection = options.GetCollectionName<IFirstLevelProjectionProfile>();

        return new ParameterizedAql
        {
            Query = @$"
                        WITH {
                            profilesCollection
                        }, {
                            functionsCollection
                        }, {
                            rolesCollection
                        } 
                        LET startVertex = DOCUMENT('{
                            entityCollection
                        }', '{
                            entityId
                        }' )

                        LET objects = UNION(
                            (
                                FOR v,e,p IN 1..100 INBOUND startVertex {
                                    functionLinksCollection.EdgeCollection
                                }, {
                                    assignmentsCollection.EdgeCollection
                                }
                                FILTER LENGTH(REMOVE_NTH(p.vertices, 1)[* FILTER IS_SAME_COLLECTION('{
                                    functionsCollection
                                }', CURRENT)]) == 0 
                                    OR IS_SAME_COLLECTION('{
                                        functionsCollection
                                    }', p.vertices[0])
                                RETURN merge(v, {{ steps: POSITION(p.vertices, v, true), path: p.vertices[*]._key }})
                            ),
                            (
                                FOR v IN 1..1 OUTBOUND startVertex {
                                    assignmentsCollection.EdgeCollection
                                }
                                RETURN merge(v, {{ steps: 1, path: [ startVertex._key ] }})
                            ),
                            [ merge(startVertex, {{ steps: 0, path: [ ] }}) ]
                        )

                        FOR o IN objects
                        FILTER o != null
                        RETURN DISTINCT {{ 
                            Id: o.Id, 
                            Steps: o.steps,
                            Path: o.path,
                            Type: NOT_NULL(o.ContainerType, o.Kind, ""Unknown"")
                        }}",
            Parameter = new Dictionary<string, object>()
        };
    }

    public static ParameterizedAql GetAllChildrenQuery(
        string entityCollection,
        string entityId,
        ModelBuilderOptions modelsInfo)
    {
        if (modelsInfo == null)
        {
            throw new ArgumentNullException(nameof(modelsInfo));
        }

        string assignmentsCollection =
            modelsInfo.GetRelation(typeof(IFirstLevelProjectionProfile), typeof(FirstLevelProjectionGroup))
                ?.EdgeCollection
            ?? throw new ArgumentException("The model builder des not seem set up correctly.", nameof(modelsInfo));

        string functionLinksCollection =
            modelsInfo.GetRelation(typeof(FirstLevelProjectionFunction), typeof(FirstLevelProjectionRole))
                ?.EdgeCollection
            ?? throw new ArgumentException("The model builder des not seem set up correctly.", nameof(modelsInfo));

        string profilesCollection = modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>();
        string functionsCollection = modelsInfo.GetCollectionName<FirstLevelProjectionFunction>();

        return new ParameterizedAql
        {
            Query = $@"
                           WITH {
                               entityCollection
                           }, {
                               profilesCollection
                           } 

                           LET startVertex =  FIRST(FOR entity in {
                               entityCollection
                           }
                                              FILTER entity.Id == ""{
                                                  entityId
                                              }""
                                              RETURN entity)
                            
                           LET isFunction = IS_SAME_COLLECTION(startVertex, ""{
                               functionsCollection
                           }"")
                           LET EdgeCount = IS_SAME_COLLECTION(startVertex, ""{
                               functionsCollection
                           }"") ? 2:1

                           RETURN {{
                               ""{
                                   nameof(FirstLevelProjectionTraversalResponse<IFirstLevelProjectionProfile>
                                       .IsStartVertexKnown)
                               }"": startVertex != null,
                               ""{
                                   nameof(FirstLevelProjectionTraversalResponse<IFirstLevelProjectionProfile>.Response)
                               }"": (
                                   FOR v,e,p IN 1..100 INBOUND startVertex {
                                       assignmentsCollection
                                   }, {
                                       functionLinksCollection
                                   }
                                   FILTER IS_SAME_COLLECTION({
                                       profilesCollection
                                   }, v)
                                   AND (
                                        LENGTH(REMOVE_NTH(p.vertices, 1)[* FILTER IS_SAME_COLLECTION({
                                            functionsCollection
                                        }, CURRENT)]) == 0 
                                        OR IS_SAME_COLLECTION({
                                            functionsCollection
                                        }, p.vertices[0]))
                                   RETURN DISTINCT {{ ""{
                                       nameof(FirstLevelRelationProfile.Profile)
                                   }"": v,
                                                     ""{
                                                         nameof(FirstLevelRelationProfile.Relation)
                                                     }"": isFunction ? COUNT(p.edges) >= EdgeCount? 
                                                                         ""{
                                                                             nameof(FirstLevelMemberRelation
                                                                                 .IndirectMember)
                                                                         }""
                                                                           :
                                                                         ""{
                                                                             nameof(FirstLevelMemberRelation
                                                                                 .DirectMember)
                                                                         }""                                                                    

                                                                    :     
                                                                    COUNT(p.edges) > EdgeCount?
                                                         
                                                                    ""{
                                                                        nameof(FirstLevelMemberRelation.IndirectMember)
                                                                    }""
                                                                          :
                                                                     ""{
                                                                         nameof(FirstLevelMemberRelation.DirectMember)
                                                                     }""
                                                    }} 
                                    )
                                }}",
            Parameter = new Dictionary<string, object>()
        };
    }

    public static ParameterizedAql DeleteEntityWithEdges(
        string entityCollection,
        string entityId,
        IList<string> edgeCollections)
    {
        var bindVars = new Dictionary<string, object>
        {
            { "@collection", entityCollection },
            { "id", entityId }
        };

        var builder = new StringBuilder();

        builder.Append(
            "LET deleted = (FOR x IN @@collection FILTER x._key == @id REMOVE x IN @@collection RETURN x._id) ");

        for (var i = 0; i < edgeCollections.Count; i++)
        {
            bindVars.Add($"@edge{i}", edgeCollections[i]);

            builder.Append(
                @$"LET a{
                    i
                } = (FOR x{
                    i
                } IN @@edge{
                    i
                } 
                       FILTER x{
                           i
                       }._from IN deleted OR x{
                           i
                       }._to IN deleted 
                       REMOVE x{
                           i
                       } IN @@edge{
                           i
                       }) ");
        }

        builder.Append("FOR d IN deleted RETURN d");

        return new ParameterizedAql
        {
            Query = builder.ToString(),
            Parameter = bindVars
        };
    }

    public static ParameterizedAql DeleteClientSettingsOfUser(string clientSettingsCollection, string profileId)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            FOR cs IN @@collection
                            FILTER cs.{
                                nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                            } == @profileId
                            REMOVE cs IN @@collection
                            RETURN cs.{
                                nameof(FirstLevelProjectionClientSettingsBasic.Key)
                            }",
            Parameter = new Dictionary<string, object>
            {
                { "profileId", profileId },
                { "@collection", clientSettingsCollection }
            }
        };
    }

    public static ParameterizedAql CreateAssignment(
        string fromId,
        string targetId,
        Type targetType,
        RangeCondition[] conditions,
        ModelBuilderOptions options)
    {
        ModelBuilderOptionsTypeRelation relation = options.GetRelation(
            typeof(IFirstLevelProjectionProfile),
            targetType);

        return new ParameterizedAql
        {
            Query = $@"
                           LET toProps = NOT_NULL(@extraToProps, []) 
                           LET fromProps = NOT_NULL(@extraFromProps, []) 
                           LET fromDoc = DOCUMENT(@fromCollection, @fromId) 
                           LET toDoc = DOCUMENT(@toCollection, @toId)
                           
                           FOR to IN [toDoc]
                               FOR from IN [fromDoc]
                               FILTER from != null && to != null
                               LET existingEdge = FIRST(
                                   FOR edge IN @@targetEdgeCollection 
                                   FILTER edge._from == from._id AND edge._to == to._id
                                   LIMIT 1
                                   RETURN edge
                               )

                               LET edge = MERGE(
                                   ZIP(toProps[* RETURN CONCAT('to_', CURRENT)], toProps[* RETURN to[CURRENT]]),
                                   ZIP(fromProps[* RETURN CONCAT('from_', CURRENT)], fromProps[* RETURN from[CURRENT]]),
                                   {{
                                       _from: from._id, 
                                       _to: to._id, 
                                       {
                                           nameof(Assignment.Conditions)
                                       }: UNION_DISTINCT(
                                           NOT_NULL(existingEdge[""{
                                               nameof(Assignment.Conditions)
                                           }""], []), 
                                           NOT_NULL(@conditions, []))
                                   }})
                               UPSERT {{_from: from._id, _to: to._id }} INSERT edge REPLACE edge INTO @@targetEdgeCollection
                               RETURN NEW",
            Parameter = new Dictionary<string, object>
            {
                { "@targetEdgeCollection", relation?.EdgeCollection ?? throw new NotSupportedException() },
                { "fromCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "fromId", fromId },
                { "toCollection", options.GetCollectionName(targetType) },
                { "toId", targetId },
                { "extraFromProps", relation.FromProperties },
                { "extraToProps", relation.ToProperties },
                { "conditions", JArray.FromObject(conditions) }
            }
        };
    }

    public static ParameterizedAql UpdateAssignments(
        string fromId,
        string targetId,
        Type targetType,
        RangeCondition[] conditions,
        ModelBuilderOptions options)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            LET edge = (
                                FOR e IN @@targetEdgeCollection 
                                FILTER LIKE(e._from, CONCAT('%/', @fromId)) 
                                   AND LIKE(e._to, CONCAT('%/', @toId))
                                LIMIT 1
                                RETURN e
                            )

                            FOR e IN edge
                                LET conditionsToRemove = NOT_NULL(@conditionsToRemove, [])
                                LET conditions = FIRST(edge).{
                                    nameof(Assignment.Conditions)
                                }[* FILTER conditionsToRemove ALL != CURRENT]

                                UPDATE e WITH {{ {
                                    nameof(Assignment.Conditions)
                                }: conditions }} IN @@targetEdgeCollection 
                                RETURN NEW ",
            Parameter = new Dictionary<string, object>
            {
                {
                    "@targetEdgeCollection", options.GetRelation(typeof(IFirstLevelProjectionProfile), targetType)
                        ?.EdgeCollection
                    ?? throw new NotSupportedException()
                },
                { "fromId", fromId },
                { "toId", targetId },
                { "conditionsToRemove", conditions.Select(JObject.FromObject) }
            }
        };
    }

    public static ParameterizedAql GetTagLinks(
        string fromArangoId,
        string tagLinksCollection,
        string[] tagIdFilter = null)
    {
        return new ParameterizedAql
        {
            Query = @"
                            FOR tagLink IN @@tagLinksCollection
                            FILTER tagLink._from == @fromId
                               AND (
                                     @tagIdFilter == NULL
                                     OR COUNT(@tagIdFilter) == 0
                                     OR @tagIdFilter ANY == PARSE_IDENTIFIER(tagLink._to).key
                                   )
                            RETURN {
                                TagId: PARSE_IDENTIFIER(tagLink._to).key,
                                IsInheritable: tagLink.IsInheritable
                            }",
            Parameter = new Dictionary<string, object>
            {
                { "@tagLinksCollection", tagLinksCollection },
                { "fromId", fromArangoId },
                { "tagIdFilter", tagIdFilter != null ? JToken.FromObject(tagIdFilter) : null }
            }
        };
    }

    public static ParameterizedAql GetLinkedObjectsToTags(
        string tagId,
        ModelBuilderOptions options)
    {
        return new ParameterizedAql
        {
            Query = $@"
                            WITH @@profilesCollection, @@functionsCollection, @@rolesCollection
                            LET startVertex = DOCUMENT(@@tagsCollection, @tagId)                          

                            FOR v,e,p IN 1..100 INBOUND startVertex @@tagLinksCollection, @@assignmentsCollection
                            FILTER FIRST(p.edges).{
                                nameof(FirstLevelProjectionTagAssignment.IsInheritable)
                            } OR LENGTH(p.edges) == 1
                            RETURN DISTINCT {{
                                Id: v.Id,
                                Type: NOT_NULL(
                                    v.{
                                        nameof(IFirstLevelProjectionProfile.Kind)
                                    }, 
                                    v.{
                                        nameof(IFirstLevelProjectionContainer.ContainerType)
                                    }, 
                                    ""Unknown"")
                            }}",
            Parameter = new Dictionary<string, object>
            {
                {
                    "@tagLinksCollection", options
                        .GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionTag>()
                        .EdgeCollection
                },
                {
                    "@assignmentsCollection", options
                        .GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionRole>()
                        .EdgeCollection
                },
                { "tagId", tagId },
                { "@tagsCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "@profilesCollection", options.GetCollectionName<IFirstLevelProjectionProfile>() },
                { "@functionsCollection", options.GetCollectionName<FirstLevelProjectionFunction>() },
                { "@rolesCollection", options.GetCollectionName<FirstLevelProjectionRole>() }
            }
        };
    }

    /// <summary>
    ///     Deletes all tag link between the given entities and returns the deleted edges.
    /// </summary>
    /// <param name="tagLinkCollection">The collection where tag links are stored.</param>
    /// <param name="arangoTagId">The arango-system-id of the tag.</param>
    /// <param name="arangoEntityId">The arango-system-id of the entity.</param>
    /// <returns>All deleted edges.</returns>
    public static ParameterizedAql DeleteTagLink(
        string tagLinkCollection,
        string arangoTagId,
        string arangoEntityId)
    {
        return new ParameterizedAql
        {
            Query = @"
                            FOR link IN @@tagLinks
                            FILTER link._from == @entityId AND link._to == @tagId
                            REMOVE link IN @@tagLinks
                            RETURN link",
            Parameter = new Dictionary<string, object>
            {
                { "@tagLinks", tagLinkCollection },
                { "entityId", arangoEntityId },
                { "tagId", arangoTagId }
            }
        };
    }

    /// <summary>
    /// Checks if a user exists in the specified collection based on their display name, email, or external ID.
    /// </summary>
    /// <param name="profileCollectionName">The name of the ArangoDB collection where the user profiles are stored.</param>
    /// <param name="arangoExternalId">The external ID to check against the profiles in the collection.</param>
    /// <param name="arangoEmail">The email to check against the profiles in the collection.</param>
    /// <param name="arangoDisplayName">The display name to check against the profiles in the collection.</param>
    /// <returns>A <see cref="ParameterizedAql"/> object containing the AQL query and parameters to check for the existence of a user based on the provided details.</returns>
    /// <remarks>
    /// The query returns a boolean indicating whether a user exists in the collection based on the provided criteria:
    /// - The user's `DisplayName` matches the provided display name.
    /// - The user's `Email` matches the provided email.
    /// - The user's `ExternalId` array contains an entry with the provided external ID.
    /// If any of these conditions are met, the user exists in the collection, and the query will return `true`.
    /// </remarks>
    public static ParameterizedAql UserExist(
        string profileCollectionName,
        string arangoExternalId,
        string arangoEmail,
        string arangoDisplayName)
    {
        return new ParameterizedAql
        {
            Query = @"      RETURN LENGTH(
                            FOR p IN @@profileCollection
                            FILTER
                            (LENGTH(TRIM(@externalId)) > 0 AND LENGTH(p.ExternalIds[* FILTER CURRENT.Id == @externalId ]) > 0) OR 
                            (LENGTH(TRIM(@displayName)) > 0 AND p.DisplayName == @displayName) OR
                            (LENGTH(TRIM(@email)) > 0 AND p.Email == @email)                                           
                            RETURN p) > 0",
            Parameter = new Dictionary<string, object>
            {
                {"@profileCollection", profileCollectionName},
                {"displayName", arangoDisplayName},
                {"email", arangoEmail},
                {"externalId", arangoExternalId}
            }
        };
    }
}