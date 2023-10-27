using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions.Models;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

internal static class WellKnownSecondLevelProjectionQueries
{
    private static string GetUpdateMemberOfOrSecurityAssignmentOfProfileAql(string propertyName)
    {
        return @$"
                      FOR x in @@collection
                        FILTER COUNT(@updatedValue) > 0 AND x.{
                            nameof(IProfileEntityModel.Id)
                        } == @pId
                         LET currentConditions = FLATTEN(x.{
                             propertyName
                         }[* FILTER CURRENT.Id == @updatedValue.Id].Conditions)
                         LET allConditions = UNION_DISTINCT(currentConditions, @updatedValue.Conditions)
                         LET unionSet = APPEND(
                            x.{
                                propertyName
                            }[* FILTER CURRENT.Id != @updatedValue.Id], 
                            MERGE(@updatedValue, 
                                {{ 
                                    Conditions: allConditions,
                                    IsActive: COUNT(
                                        FOR c IN allConditions
                                        FILTER NOT_NULL(c.Start, DATE_ISO8601(DATE_NOW())) <= DATE_ISO8601(DATE_NOW())
                                        AND NOT_NULL(c.End, DATE_ISO8601(DATE_NOW())) >= DATE_ISO8601(DATE_NOW())
                                        RETURN c) > 0 
                                }}))
                        UPDATE x WITH {{ {
                            propertyName
                        }: unionSet }}
                          IN @@collection
                        RETURN NEW";
    }

    private static string GetRemoveMemberOfOrSecurityAssignmentOfProfileAql(string propertyName)
    {
        return @$"
                      FOR p IN @@collection
                      FILTER p.Id == @profileId

                      LET newAssignments = COUNT(NOT_NULL(@conditionsToRemove, [])) == 0 
                          ? [] 
                          : (
                              FOR assignment IN p.{
                                  propertyName
                              }
                              FILTER assignment.Id == @containerId 
                              LET conditions = assignment.Conditions[* FILTER CURRENT NOT IN @conditionsToRemove]
                              FILTER COUNT(conditions) > 0
                              RETURN MERGE(
                                  assignment, 
                                  {{
                                      Conditions: conditions,
                                      IsActive: COUNT(
                                          FOR c IN conditions
                                          FILTER NOT_NULL(c.Start, DATE_ISO8601(DATE_NOW())) <= DATE_ISO8601(DATE_NOW())
                                          AND NOT_NULL(c.End, DATE_ISO8601(DATE_NOW())) >= DATE_ISO8601(DATE_NOW())
                                          RETURN c) > 0 
                                  }})
                              )
                      
                      UPDATE p WITH {{{
                          propertyName
                      }: UNION(p.{
                          propertyName
                      }[* FILTER CURRENT.Id != @containerId], newAssignments)}} IN @@collection
                      RETURN NEW";
    }

    private static string GetRecalculateMemberOfOrSecurityAssignmentOfProfileAql(string propertyName)
    {
        return @$"
                      FOR p IN @@collection
                      FILTER p.Id == @profileId

                      LET newAssignments = (
                              FOR assignment IN p.{
                                  propertyName
                              }
                              FILTER assignment.Id == @elementId
                              RETURN MERGE(
                                  assignment, 
                                  {{
                                      IsActive: COUNT(
                                          FOR c IN assignment.Conditions
                                          FILTER NOT_NULL(c.Start, DATE_ISO8601(DATE_NOW())) <= DATE_ISO8601(DATE_NOW())
                                          AND NOT_NULL(c.End, DATE_ISO8601(DATE_NOW())) >= DATE_ISO8601(DATE_NOW())
                                          RETURN c) > 0 
                                  }})
                              )
                      
                      UPDATE p WITH {{{
                          propertyName
                      }: UNION(p.{
                          propertyName
                      }[* FILTER CURRENT.Id != @elementId], newAssignments)}} IN @@collection
                      RETURN NEW";
    }

    internal static ParameterizedAql AddMemberOfSetToProfile(
        string profileId,
        ModelBuilderOptions options,
        JsonSerializer serializer,
        Member memberOfCollection)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetUpdateMemberOfOrSecurityAssignmentOfProfileAql(nameof(IProfileEntityModel.MemberOf)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "pId", profileId },
                { "updatedValue", memberOfCollection.GetJsonDocument(serializer) }
            }
        };
    }

    internal static ParameterizedAql CreateEdgeDataPathTree(
        string fromId,
        string targetId,
        SecondLevelProjectionProfileEdgeData edgeData,
        JsonSerializer serializer,
        ModelBuilderOptions options)
    {
        return new ParameterizedAql
        {
            Query = @" LET existingEdge = NOT_NULL(
                                FIRST(
                                    FOR edge IN @@targetEdgeCollection
                                    FILTER edge._from == @fromId AND edge._to == @targetId
                                    LIMIT 1
                                    RETURN edge
                                ), {})
                                                          
                                LET edge = MERGE( 
                                    existingEdge, 
                                    @edgeData, 
                                    {  
                                        _from:@fromId, 
                                        _to:@targetId, 
                                        Conditions: UNION_DISTINCT(
                                            @edgeData.Conditions,
                                            NOT_NULL(existingEdge.Conditions, [])
                                        )
                                    }
                                )
                                UPSERT {_from: @fromId, _to: @targetId, } INSERT edge REPLACE edge INTO @@targetEdgeCollection
                                RETURN NEW",
            Parameter = new Dictionary<string, object>
            {
                { "fromId", fromId },
                { "targetId", targetId },
                { "edgeData", edgeData.GetJsonDocument(serializer) },
                {
                    "@targetEdgeCollection", options
                        .GetRelation<SecondLevelProjectionProfileVertexData,
                            SecondLevelProjectionProfileVertexData>()
                        .EdgeCollection
                }
            }
        };
    }

    internal static ParameterizedAql AddSecurityAssignmentEntriesToProfile(
        string profileId,
        ModelBuilderOptions options,
        JsonSerializer serializer,
        ILinkedObject linkedObjects)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetUpdateMemberOfOrSecurityAssignmentOfProfileAql(nameof(IProfileEntityModel.SecurityAssignments)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "pId", profileId },
                { "updatedValue", linkedObjects.GetJsonDocument(serializer) }
            }
        };
    }

    internal static ParameterizedAql RemoveSecurityAssignmentEntriesToProfile(
        string profileId,
        ModelBuilderOptions options,
        JsonSerializer serializer,
        string containerId,
        IList<RangeCondition> conditions)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetRemoveMemberOfOrSecurityAssignmentOfProfileAql(nameof(IProfileEntityModel.SecurityAssignments)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "profileId", profileId },
                { "conditionsToRemove", conditions?.GetJsonDocument(serializer) },
                { "containerId", containerId }
            }
        };
    }

    internal static ParameterizedAql RemoveMemberOfEntriesToProfile(
        string profileId,
        ModelBuilderOptions options,
        JsonSerializer serializer,
        string containerId,
        IList<RangeCondition> conditions)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetRemoveMemberOfOrSecurityAssignmentOfProfileAql(nameof(IProfileEntityModel.MemberOf)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "profileId", profileId },
                { "conditionsToRemove", conditions?.GetJsonDocument(serializer) },
                { "containerId", containerId }
            }
        };
    }

    internal static ParameterizedAql RecalculateMemberOfEntriesToProfile(
        string profileId,
        ModelBuilderOptions options,
        string containerId)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetRecalculateMemberOfOrSecurityAssignmentOfProfileAql(nameof(IProfileEntityModel.MemberOf)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "profileId", profileId },
                { "elementId", containerId }
            }
        };
    }

    internal static ParameterizedAql RecalculateSecurityAssignmentEntriesToProfile(
        string profileId,
        ModelBuilderOptions options,
        string containerId)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetRecalculateMemberOfOrSecurityAssignmentOfProfileAql(
                nameof(IProfileEntityModel.SecurityAssignments)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "profileId", profileId },
                { "elementId", containerId }
            }
        };
    }

    internal static ParameterizedAql RecalculateMemberEntriesForProfile(
        string profileId,
        ModelBuilderOptions options,
        string containerId)
    {
        string profileCollectionName = options.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(profileCollectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetRecalculateMemberOfOrSecurityAssignmentOfProfileAql(
                nameof(IContainerProfileEntityModel.Members)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", profileCollectionName },
                { "profileId", profileId },
                { "elementId", containerId }
            }
        };
    }

    internal static ParameterizedAql RecalculateLinkedProfilesForProfilesForRolesAndFunctions(
        string roleOrFunctionId,
        ModelBuilderOptions options,
        string memberId)
    {
        string roleOrFunctionCollection = options.GetCollectionName<IAssignmentObject>();

        if (string.IsNullOrWhiteSpace(roleOrFunctionCollection))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IAssignmentObject)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = GetRecalculateMemberOfOrSecurityAssignmentOfProfileAql(nameof(IAssignmentObject.LinkedProfiles)),
            Parameter = new Dictionary<string, object>
            {
                { "@collection", roleOrFunctionCollection },
                { "profileId", roleOrFunctionId },
                { "elementId", memberId }
            }
        };
    }

    internal static ParameterizedAql UnsetClientSettingsKey(
        string profileId,
        string settingsKey,
        ModelBuilderOptions options)
    {
        string collectionName = options.GetCollectionName<ClientSettingsEntityModel>();

        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(ClientSettingsEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = $@"
                           FOR cs IN @@collection
                             FILTER cs.{
                                 nameof(ClientSettingsEntityModel.ProfileId)
                             }==@pId
                               AND cs.{
                                   nameof(ClientSettingsEntityModel.SettingsKey)
                               }==@key
                             REMOVE cs IN @@collection RETURN OLD.{
                                 AConstants.IdSystemProperty
                             }",
            Parameter = new Dictionary<string, object>
            {
                { "pId", profileId },
                { "key", settingsKey },
                { "@collection", collectionName }
            }
        };
    }

    internal static ParameterizedAql SetClientSettingsKey(
        ClientSettingsEntityModel entity,
        ModelBuilderOptions options)
    {
        string collectionName = options.GetCollectionName<ClientSettingsEntityModel>();

        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(ClientSettingsEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = $@"
                           LET existing = FIRST(
                             FOR cs IN @@collection
                               FILTER cs.{
                                   nameof(ClientSettingsEntityModel.ProfileId)
                               }==@pId
                                  AND cs.{
                                      nameof(ClientSettingsEntityModel.SettingsKey)
                                  }==@key
                             LIMIT 1
                             RETURN cs)
                           INSERT MERGE(@entity,
                             existing!=null 
                               ? {{{
                                   AConstants.KeySystemProperty
                               }: existing.{
                                   AConstants.KeySystemProperty
                               }}}
                               : {{}})
                             INTO @@collection 
                             OPTIONS {{ overwriteMode: ""replace"" }}
                             RETURN NEW.{
                                 AConstants.IdSystemProperty
                             }",
            Parameter = new Dictionary<string, object>
            {
                { "pId", entity.ProfileId },
                { "key", entity.SettingsKey },
                { "entity", entity.GetJsonDocument() },
                { "@collection", collectionName }
            }
        };
    }

    internal static ParameterizedAql InvalidateClientSettingsKey(
        string profileId,
        IList<string> settingsKeysToKeep,
        ModelBuilderOptions options)
    {
        string collectionName = options.GetCollectionName<ClientSettingsEntityModel>();

        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(ClientSettingsEntityModel)}",
                ExceptionSeverity.Error);
        }

        return new ParameterizedAql
        {
            Query = $@"
                           FOR cs IN @@collection
                             FILTER cs.{
                                 nameof(ClientSettingsEntityModel.ProfileId)
                             }==@pId
                               AND @keys ALL != cs.{
                                   nameof(ClientSettingsEntityModel.SettingsKey)
                               }
                             REMOVE cs IN @@collection RETURN OLD.{
                                 nameof(ClientSettingsEntityModel.SettingsKey)
                             }",
            Parameter = new Dictionary<string, object>
            {
                { "pId", profileId },
                { "keys", settingsKeysToKeep.GetJsonDocument() },
                { "@collection", collectionName }
            }
        };
    }

    internal static ParameterizedAql CalculateTags(
        string rootVertexId,
        string profileId,
        ModelBuilderOptions options)
    {
        string pathTreeVertexCollection =
            options.GetCollectionName<SecondLevelProjectionProfileVertexData>();

        string pathTreeEdgeCollection =
            options.GetRelatedOutboundEdgeCollections<SecondLevelProjectionProfileVertexData>()
                .SingleOrDefault();

        if (string.IsNullOrWhiteSpace(pathTreeVertexCollection))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        if (string.IsNullOrWhiteSpace(pathTreeEdgeCollection))
        {
            throw new DatabaseException(
                $"Could not find a edge collection name for entity type {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        const string conditions = nameof(SecondLevelProjectionProfileEdgeData.Conditions);
        const string relatedProfile = nameof(SecondLevelProjectionProfileEdgeData.RelatedProfileId);
        const string objectId = nameof(SecondLevelProjectionProfileVertexData.ObjectId);

        return new ParameterizedAql
        {
            Query =
                $@"
                      WITH @@edgeCollection, @@vertexCollection
                      
                      LET set = (FOR v, e, p IN 1..100 OUTBOUND @rootVertex @@edgeCollection
                                   FILTER(FOR edge IN p.edges RETURN LENGTH(edge.{
                                       conditions
                                   }[*
                                              FILTER NOT_NULL(CURRENT.{
                                                  nameof(RangeCondition.Start)
                                              },
                                                              DATE_ISO8601(DATE_NOW())) <= DATE_ISO8601(DATE_NOW()) 
                                              AND NOT_NULL(CURRENT.{
                                                  nameof(RangeCondition.End)
                                              },
                                                              DATE_ISO8601(DATE_NOW())) >= DATE_ISO8601(DATE_NOW())])) ALL > 0
                                     AND p.edges[*].{
                                         relatedProfile
                                     } ALL == @profileId
                                 RETURN DISTINCT v)
                      
                      LET Tags =  (FOR vertex IN set 
                                     FOR tag IN vertex.{
                                         nameof(SecondLevelProjectionProfileVertexData.Tags)
                                     }                                                 
                                       FILTER tag.{
                                           nameof(TagAssignment.IsInheritable)
                                       }  OR vertex.{
                                           objectId
                                       } == @profileId
                                       RETURN MERGE (tag.{
                                           nameof(TagAssignment.TagDetails)
                                       },
                                            {{ {
                                                nameof(CalculatedTag.IsInherited)
                                            }: vertex.{
                                                objectId
                                            } != @profileId }}))
                      
                      LET inheritedTags = FLATTEN(FOR x IN Tags 
                                                  COLLECT id = x.{
                                                      nameof(Tag.Id)
                                                  } INTO TagsById = x
                                                  RETURN (
                                                          FOR t IN TagsById
                                                            SORT t.{
                                                                nameof(CalculatedTag.IsInherited)
                                                            }
                                                            LIMIT 1
                                                            RETURN t))
                      
                      LET ownTags = FLATTEN(FOR vertex IN @@vertexCollection
                                              FILTER vertex.{
                                                  nameof(SecondLevelProjectionProfileVertexData.RelatedProfileId)
                                              } == @profileId
                                                 AND vertex.{
                                                     nameof(SecondLevelProjectionProfileVertexData.ObjectId)
                                                 } == @profileId
                                              RETURN vertex.{
                                                  nameof(SecondLevelProjectionProfileVertexData.Tags)
                                              }
                                                            [* RETURN MERGE(CURRENT.{
                                                                nameof(TagAssignment.TagDetails)
                                                            },
                                                                {{{
                                                                    nameof(CalculatedTag.IsInherited)
                                                                }: false}})])
                      
                      FOR r IN UNION_DISTINCT(inheritedTags, ownTags) RETURN r",
            Parameter = new Dictionary<string, object>
            {
                { "rootVertex", rootVertexId },
                { "profileId", profileId },
                { "@edgeCollection", pathTreeEdgeCollection },
                { "@vertexCollection", pathTreeVertexCollection }
            }
        };
    }

    internal static ParameterizedAql CalculatePath(
        string rootVertexId,
        string relatedProfileId,
        ModelBuilderOptions options)
    {
        string pathTreeVertexCollection =
            options.GetCollectionName<SecondLevelProjectionProfileVertexData>();

        string pathTreeEdgeCollection =
            options.GetRelatedOutboundEdgeCollections<SecondLevelProjectionProfileVertexData>()
                .SingleOrDefault();

        if (string.IsNullOrWhiteSpace(pathTreeVertexCollection))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        if (string.IsNullOrWhiteSpace(pathTreeEdgeCollection))
        {
            throw new DatabaseException(
                $"Could not find a edge collection name for entity type {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        const string conditions = nameof(SecondLevelProjectionProfileEdgeData.Conditions);
        const string objectId = nameof(SecondLevelProjectionProfileVertexData.ObjectId);
        const string relatedProfile = nameof(SecondLevelProjectionProfileEdgeData.RelatedProfileId);

        return new ParameterizedAql
        {
            Query = $@"
                             WITH @@edgeCollection, @@vertexCollection
                             LET set = (FOR v, e, p IN 1..100 OUTBOUND @rootVertex @@edgeCollection
                             FILTER(FOR edge IN p.edges RETURN LENGTH(edge.{
                                 conditions
                             }[*
                                         FILTER NOT_NULL(CURRENT.{
                                             nameof(RangeCondition.Start)
                                         }, 
                                                             DATE_ISO8601(DATE_NOW())) <= DATE_ISO8601(DATE_NOW()) AND
                                                NOT_NULL(CURRENT.{
                                                    nameof(RangeCondition.End)
                                                },
                                                             DATE_ISO8601(DATE_NOW())) >= DATE_ISO8601(DATE_NOW())])) ALL > 0
                             AND p.edges[*].{
                                 relatedProfile
                             } ALL == @profileId
                             RETURN CONCAT_SEPARATOR(""/"", REVERSE(p.vertices[*].{
                                 objectId
                             })))
                             LET temp = (
                                 FOR x IN set
                                 FOR y IN set
                                 FILTER CONTAINS(y, x) AND x!= y
                             RETURN DISTINCT x)
                             FOR r IN UNION(MINUS(set, temp), [""{
                                 relatedProfileId
                             }""]) RETURN r",
            Parameter = new Dictionary<string, object>
            {
                { "rootVertex", rootVertexId },
                { "profileId", relatedProfileId },
                { "@edgeCollection", pathTreeEdgeCollection },
                { "@vertexCollection", pathTreeVertexCollection }
            }
        };
    }

    internal static ParameterizedAql GetDeletePathTreeEdgesAql(
        string relatedEntityId,
        string parentId,
        string memberId,
        ModelBuilderOptions options)
    {
        string pathTreeEdgeCollection = options
            .GetRelation<SecondLevelProjectionProfileVertexData,
                SecondLevelProjectionProfileVertexData>()
            .EdgeCollection;

        if (string.IsNullOrWhiteSpace(pathTreeEdgeCollection))
        {
            throw new DatabaseException(
                $"Could not find a edge collection name for relation between {nameof(SecondLevelProjectionProfileVertexData)} and {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        string pathVertexCollection = options.GetCollectionName<SecondLevelProjectionProfileVertexData>();

        if (string.IsNullOrWhiteSpace(pathVertexCollection))
        {
            throw new DatabaseException(
                $"Could not find a collection for {nameof(SecondLevelProjectionProfileVertexData)}.",
                ExceptionSeverity.Error);
        }

        string memberDocId = GetDocumentIdOfVertex(relatedEntityId, memberId, pathVertexCollection);
        string vertexDocId = GetDocumentIdOfVertex(relatedEntityId, parentId, pathVertexCollection);

        return new ParameterizedAql
        {
            Query = @" FOR x in @@pathTreeEdgeCollection                 
                            FILTER x._from == @memberDocId  
                              AND  x._to == @vertexDocId
                           REMOVE x IN @@pathTreeEdgeCollection RETURN OLD._key",
            Parameter = new Dictionary<string, object>
            {
                { "@pathTreeEdgeCollection", pathTreeEdgeCollection },
                { "memberDocId", memberDocId },
                { "vertexDocId", vertexDocId }
            }
        };
    }

    internal static ParameterizedAql GetDeleteRecursiveGraphOfPathTreeAql(
        string relatedEntityId,
        string parentId,
        ModelBuilderOptions options)
    {
        string pathTreeEdgeCollection = options
            .GetRelation<SecondLevelProjectionProfileVertexData,
                SecondLevelProjectionProfileVertexData>()
            .EdgeCollection;

        if (string.IsNullOrWhiteSpace(pathTreeEdgeCollection))
        {
            throw new DatabaseException(
                $"Could not find a edge collection name for relation between {nameof(SecondLevelProjectionProfileVertexData)} and {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        string pathVertexCollection = options.GetCollectionName<SecondLevelProjectionProfileVertexData>();

        if (string.IsNullOrWhiteSpace(pathVertexCollection))
        {
            throw new DatabaseException(
                $"Could not find a collection for {nameof(SecondLevelProjectionProfileVertexData)}.",
                ExceptionSeverity.Error);
        }

        string vertexDocId = GetDocumentIdOfVertex(relatedEntityId, parentId, pathVertexCollection);

        string relatedEntityVertexDocId = GetDocumentIdOfVertex(
            relatedEntityId,
            relatedEntityId,
            pathVertexCollection);

        return new ParameterizedAql
        {
            Query = @"
                            // child
                            let child = (FOR v,e,p in 0..100 OUTBOUND @relatedEntityVertexDocId @@pathTreeEdgeCollection return distinct {v: v, e: e}) 
                            let child_edges = UNIQUE(child[*].e)
                            let child_vertices = UNIQUE(child[*].v)


                            // parent
                            let parent = (for v, e, p in 0..100 OUTBOUND @vertexDocId @@pathTreeEdgeCollection return distinct { v: v, e: e})
                            let parent_edges = UNIQUE(parent[*].e)
                            let parent_vertices = UNIQUE(parent[*].v)

                            for edge in MINUS(parent_edges, child_edges)
                                remove edge in @@pathTreeEdgeCollection OPTIONS { ignoreErrors: true }

                            for vertex in MINUS(parent_vertices, child_vertices)
                                remove vertex in @@pathTreeVertexCollection OPTIONS { ignoreErrors: true }",
            Parameter = new Dictionary<string, object>
            {
                { "@pathTreeEdgeCollection", pathTreeEdgeCollection },
                { "@pathTreeVertexCollection", pathVertexCollection },
                { "relatedEntityVertexDocId", relatedEntityVertexDocId },
                { "vertexDocId", vertexDocId }
            }
        };
    }

    internal static string GetVertexId(string relatedProfileId, string objectId)
    {
        if (string.IsNullOrWhiteSpace(relatedProfileId) || string.IsNullOrWhiteSpace(objectId))
        {
            return null;
        }

        return $"{relatedProfileId}-{objectId}";
    }

    internal static string GetVertexId(SecondLevelProjectionProfileVertexData vertexData)
    {
        return vertexData == null ? null : $"{vertexData.RelatedProfileId}-{vertexData.ObjectId}";
    }

    internal static string GetDocumentIdOfVertex(
        SecondLevelProjectionProfileVertexData vertexData,
        string pathTreeVertexCollectionName)
    {
        return vertexData == null ? null : $"{pathTreeVertexCollectionName}/{GetVertexId(vertexData)}";
    }

    internal static string GetDocumentIdOfVertex(
        string relatedProfileId,
        string objectId,
        string pathTreeVertexCollectionName)
    {
        return $"{pathTreeVertexCollectionName}/{GetVertexId(relatedProfileId, objectId)}";
    }

    internal static string GetRootVertexDocumentId(string collectionName, string relatedProfileId)
    {
        return string.IsNullOrWhiteSpace(relatedProfileId)
            ? string.Empty
            : $"{collectionName}/{relatedProfileId}-{relatedProfileId}";
    }

    /// We remove here range conditions from an edge in the path tree.
    /// The edge is related from an parent to a child object (mostly from user to group).
    internal static ParameterizedAql RemoveConditionsFromEdge(
        string relatedEntityId,
        string parentId,
        string memberId,
        IList<RangeCondition> conditionsToRemove,
        JsonSerializer serializer,
        ModelBuilderOptions options)
    {
        // Getting the needed pathTreeEdgeCollection for the query
        string pathTreeEdgeCollection = options
            .GetRelation<SecondLevelProjectionProfileVertexData,
                SecondLevelProjectionProfileVertexData>()
            .EdgeCollection;

        string pathVertexCollection = options.GetCollectionName<SecondLevelProjectionProfileVertexData>();

        if (string.IsNullOrWhiteSpace(relatedEntityId))
        {
            throw new ArgumentException(nameof(relatedEntityId));
        }

        if (string.IsNullOrWhiteSpace(parentId))
        {
            throw new ArgumentException(nameof(parentId));
        }

        if (string.IsNullOrWhiteSpace(memberId))
        {
            throw new ArgumentException(nameof(parentId));
        }

        if (conditionsToRemove == null || !conditionsToRemove.Any())
        {
            throw new ArgumentException("The conditions can not be null or empty");
        }

        if (string.IsNullOrWhiteSpace(pathTreeEdgeCollection))
        {
            throw new DatabaseException(
                $"Could not find a edge collection name for relation between {nameof(SecondLevelProjectionProfileVertexData)} and {nameof(SecondLevelProjectionProfileVertexData)}",
                ExceptionSeverity.Error);
        }

        if (string.IsNullOrWhiteSpace(pathVertexCollection))
        {
            throw new DatabaseException(
                $"Could not find a collection for {nameof(SecondLevelProjectionProfileVertexData)}.",
                ExceptionSeverity.Error);
        }

        string memberDocId = GetDocumentIdOfVertex(relatedEntityId, memberId, pathVertexCollection);
        string parentDocId = GetDocumentIdOfVertex(relatedEntityId, parentId, pathVertexCollection);

        return new ParameterizedAql
        {
            Query = $@" LET conditionsToRemove = NOT_NULL(@conditionsToRemove, [])
                                   LET newEdge = (
                                                FOR pathTreeEdge in @@targetEdgeCollection
				                                FILTER pathTreeEdge._to == @toId AND pathTreeEdge._from == @fromId
				                                LET conditions = pathTreeEdge.Conditions[* FILTER CURRENT NOT IN @conditionsToRemove]
				                                RETURN MERGE(
                                                            pathTreeEdge,
                                                            {{
                                                             {
                                                                 nameof(SecondLevelProjectionProfileEdgeData.Conditions)
                                                             }: conditions 
							                                }}
							                            )
				                            )   
                          FOR edge in newEdge
                          UPDATE edge._key WITH {{ 
                                                {
                                                    nameof(SecondLevelProjectionProfileEdgeData.Conditions)
                                                }: edge.{
                                                    nameof(SecondLevelProjectionProfileEdgeData.Conditions)
                                                } 
                                                }} in @@targetEdgeCollection 
                          RETURN NEW",

            Parameter = new Dictionary<string, object>
            {
                { "fromId", memberDocId },
                { "toId", parentDocId },
                { "conditionsToRemove", conditionsToRemove.GetJsonDocument(serializer) },
                {
                    "@targetEdgeCollection", options
                        .GetRelation<SecondLevelProjectionProfileVertexData,
                            SecondLevelProjectionProfileVertexData>()
                        .EdgeCollection
                }
            }
        };
    }
}
