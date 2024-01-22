using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     The class offers methods that can be used to create or deleted
///     relations between objects.
/// </summary>
/// <typeparam name="TEntity">The entity for with the relations should be created.</typeparam>
public abstract class RelationHandlerBase<TEntity> : IRelationHandler<TEntity> where TEntity : ISyncModel
{
    /// <summary>
    /// The logger that is used for logging purposes.
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// The object is used to get an object from the system you want to sync in.
    /// </summary>
    protected readonly ISynchronizationReadDestination<TEntity> _entityReadDestination;

    /// <summary>
    /// The object is used to write an object in the system where you want to sync.
    /// </summary>
    protected readonly ISynchronizationWriteDestination<TEntity> _entityWriteDestination;

    /// <summary>
    ///     A service that handle the entity states during the synchronization process. It store
    ///     all entities and their relations.
    /// </summary>
    protected readonly IProcessTempHandler _processTempHandler;

    /// <summary>
    ///     Constructor to create an object.
    /// </summary>
    /// <param name="entityReadDestination">The object is used to get an object from the system you want to sync in.</param>
    /// <param name="writeDestination">The object is used to write an object in the system where you want to sync.</param>
    /// <param name="logger">The logger is used for logging purposes.</param>
    /// <param name="processTempHandler">
    ///     A service that handle the entity states during the synchronization process. It store
    ///     all entities and their relations.
    /// </param>
    protected RelationHandlerBase(
        ISynchronizationReadDestination<TEntity> entityReadDestination,
        ISynchronizationWriteDestination<TEntity> writeDestination,
        ILogger logger,
        IProcessTempHandler processTempHandler)
    {
        _entityReadDestination = entityReadDestination;
        _entityWriteDestination = writeDestination;
        _processTempHandler = processTempHandler;
        _logger = logger;
    }

    /// <inheritdoc />
    public abstract Task HandleRelationsAsync(
        Process process,
        bool addRelation,
        bool deleteRelation,
        ObjectType objectType);

    /// <summary>
    ///     Get all entities and their relation between each other.
    /// </summary>
    /// <param name="type">The type of object that are holding relation</param>
    /// <param name="process">The saga process the relation step is running it.</param>
    /// <typeparam name="TSyncModel">The type of object for whose the relation should be created.</typeparam>
    /// <returns>Returns an  list of <see cref="IRelation" /> where the entities and their relations are stored.</returns>
    protected virtual async Task<IList<IRelation>> GetAllEntityRelationsAsync<TSyncModel>(
        ObjectType type,
        Process process) where TSyncModel : ISyncModel
    {
        _logger.EnterMethod();

        var relations = new List<IRelation>();

        IList<Guid> tempObjects =
            await _processTempHandler.GetTemporaryObjectKeysAsync<TSyncModel>(process.Id.ToString());

        _logger.LogDebugMessage(
            "Get all temp entities which have not been synchronized. Total: '{total}'",
            LogHelpers.Arguments(tempObjects.Count));

        foreach (Guid operationId in tempObjects)
        {
            var operationState = await _processTempHandler.GetTemporaryObjectAsync<TSyncModel>(
                process.Id.ToString(),
                operationId);

            relations.Add(
                new Relation(
                    new LookUpObject(
                        operationState.ExternalIds.FirstOrDefault(),
                        operationState.Id,
                        operationState.Source,
                        type),
                    operationState.RelatedObjects));
        }

        _logger.LogDebugMessage(
            "Found '{total}' relations to check and synchronize.",
            LogHelpers.Arguments(relations.Count));

        return _logger.ExitMethod(relations);
    }

    /// <summary>
    ///     The method extracts the missing relations that have to be created.
    ///     The method compares the relation that comes from the source sync system and
    ///     compares it against the relation that are store in the destination system.
    /// </summary>
    /// <param name="syncSourceRelations">The relation that are coming from the sync source system.</param>
    /// <param name="originalSourceRelations">
    ///     The relation that are store in the destination system where the relation should
    ///     be synced.
    /// </param>
    /// <returns>Return a list <see cref="IRelation" /> that have to be created</returns>
    protected virtual IList<IRelation> ExtractMissingRelationToCreate(
        IList<IRelation> syncSourceRelations,
        IList<IRelation> originalSourceRelations)
    {
        _logger.EnterMethod();

        var relationsToCreate = new Dictionary<string, IRelation>();

        foreach (IRelation relation in syncSourceRelations)
        {
            foreach (ObjectRelation relatedRepoRelation in relation.RelatedObjects)
            {
                _logger.LogDebugMessage(
                    "Check relation between {id} ({type}) and {id} ({type}) ",
                    LogHelpers.Arguments(
                        relation.OriginalObject.MaverickId,
                        relation.OriginalObject.ObjectType,
                        relatedRepoRelation.MaverickId,
                        relatedRepoRelation.ObjectType));

                // Checks if the existing relation exists in the same direction. 
                bool sameRelationExists = originalSourceRelations.Any(
                    or => CompareObjects(or.OriginalObject, relation.OriginalObject)
                        && or.RelatedObjects.Any(
                            ro => CompareObjects(ro, relatedRepoRelation)
                                && ro.AssignmentType == relatedRepoRelation.AssignmentType));

                if (sameRelationExists)
                {
                    _logger.LogDebugMessage(
                        "The same relation exists between {id} ({type}) and {id} ({type}) ",
                        LogHelpers.Arguments(
                            relation.OriginalObject.MaverickId,
                            relation.OriginalObject.ObjectType,
                            relatedRepoRelation.MaverickId,
                            relatedRepoRelation.ObjectType));

                    continue;
                }

                // Checks if the relation between the objects with the opposite label exists.
                // Father -> children (children), children -> father (parent)
                bool oppositeRelation = originalSourceRelations.Any(
                    or =>
                    {
                        ObjectRelation matchedRelatedObject = or.RelatedObjects.FirstOrDefault(
                            ro => CompareObjects(ro, relation.OriginalObject));

                        if (matchedRelatedObject != null)
                        {
                            return CompareObjects(relatedRepoRelation, or.OriginalObject)
                                && matchedRelatedObject.AssignmentType.CompareOppositeRelationType(
                                    relatedRepoRelation.AssignmentType);
                        }

                        return false;
                    });

                if (oppositeRelation)
                {
                    _logger.LogDebugMessage(
                        "The same relation (opposite direction) exists between {id} ({type}) and {id} ({type}).",
                        LogHelpers.Arguments(
                            relation.OriginalObject.MaverickId,
                            relation.OriginalObject.ObjectType,
                            relatedRepoRelation.MaverickId,
                            relatedRepoRelation.ObjectType));

                    continue;
                }

                _logger.LogDebugMessage(
                    "Add relation between {id} ({type}) and {id} ({type}) to create.",
                    LogHelpers.Arguments(
                        relation.OriginalObject.MaverickId,
                        relation.OriginalObject.ObjectType,
                        relatedRepoRelation.MaverickId,
                        relatedRepoRelation.ObjectType));

                if (relationsToCreate.TryGetValue(relation.OriginalObject.MaverickId, out IRelation rel))
                {
                    rel.RelatedObjects.Add(relatedRepoRelation);
                }
                else
                {
                    relationsToCreate.Add(
                        relation.OriginalObject.MaverickId,
                        new AddedRelation(
                            relation.OriginalObject,
                            new List<ObjectRelation>
                            {
                                relatedRepoRelation
                            }));
                }
            }
        }

        List<IRelation> relations = relationsToCreate.Values.ToList();

        _logger.LogInfoMessage("Extracted {count} relation to create.", LogHelpers.Arguments(relations.Count));

        return _logger.ExitMethod(relations);
    }

    /// <summary>
    ///     The method compares the <see cref="ILookUpObject" />.
    /// </summary>
    /// <param name="source">The source <see cref="ILookUpObject" />.</param>
    /// <param name="target">The target <see cref="ILookUpObject" />.</param>
    /// <returns>True if the objects are equal, false otherwise.</returns>
    protected bool CompareObjects(ILookUpObject source, ILookUpObject target)
    {
        _logger.EnterMethod();

        _logger.LogTraceMessage(
            "Compare source {maverickId} (maverickId) and {type} (type) with target {maverickId} (maverickId) and {type} (type)",
            LogHelpers.Arguments(source.MaverickId, source.ObjectType, target.MaverickId, target.ObjectType));

        bool equal = source.MaverickId == target.MaverickId && source.ObjectType == target.ObjectType;

        return _logger.ExitMethod(equal);
    }

    /// <summary>
    ///     The method extracts the relations that have to be deleted.
    ///     The method compares the relation that comes from the source sync system and
    ///     compares it against the relation that are store in the destination system.
    /// </summary>
    /// <param name="syncSourceRelations">The relation that are coming from the sync source system.</param>
    /// <param name="originalSourceRelations">
    ///     The relation that are store in the destination system where the relation should
    ///     be synced.
    /// </param>
    /// <param name="process">The saga process the relation step is running it</param>
    /// <returns>Return a list <see cref="IRelation" /> that have to be deleted.</returns>
    protected virtual IList<IRelation> ExtractRelationToDelete(
        IList<IRelation> originalSourceRelations,
        IList<IRelation> syncSourceRelations,
        Process process)
    {
        _logger.EnterMethod();

        var relationsToDelete = new Dictionary<string, IRelation>();

        foreach (IRelation relation in originalSourceRelations)
        {
            foreach (ObjectRelation relatedRepoRelation in relation.RelatedObjects)
            {
                // Only update relations between objects that originate from the current system.
                if (relation.OriginalObject.Source != process.System
                    || relation.OriginalObject.Source != process.System)
                {
                    _logger.LogDebugMessage(
                        "The relation (opposite direction) between {id} ({type}) and {id} ({type}) is not from the current system {system}.",
                        LogHelpers.Arguments(
                            relation.OriginalObject.MaverickId,
                            relation.OriginalObject.ObjectType,
                            relatedRepoRelation.MaverickId,
                            relatedRepoRelation.ObjectType,
                            process.System));

                    continue;
                }

                // Checks if the existing relation exists in the same direction. 
                bool sameRelationExists = syncSourceRelations.Any(
                    or => CompareObjects(or.OriginalObject, relation.OriginalObject)
                        && or.RelatedObjects.Any(
                            ro => CompareObjects(ro, relatedRepoRelation)
                                && ro.AssignmentType == relatedRepoRelation.AssignmentType));

                if (sameRelationExists)
                {
                    _logger.LogDebugMessage(
                        "The relation between {id} ({type}) and {id} ({type}) exists.",
                        LogHelpers.Arguments(
                            relation.OriginalObject.MaverickId,
                            relation.OriginalObject.ObjectType,
                            relatedRepoRelation.MaverickId,
                            relatedRepoRelation.ObjectType,
                            process.System));

                    continue;
                }

                // Checks if the relation between the objects with the opposite label exists.
                // Father -> children (children), children -> father (parent)
                bool oppositeRelation = syncSourceRelations.Any(
                    or =>
                    {
                        ObjectRelation matchedRelatedObject = or.RelatedObjects.FirstOrDefault(
                            ro => CompareObjects(ro, relation.OriginalObject));

                        if (matchedRelatedObject != null)
                        {
                            return CompareObjects(relatedRepoRelation, or.OriginalObject)
                                && matchedRelatedObject.AssignmentType.CompareOppositeRelationType(
                                    relatedRepoRelation.AssignmentType);
                        }

                        return false;
                    });

                if (oppositeRelation)
                {
                    _logger.LogDebugMessage(
                        "The relation (opposite direction) between {requestId} ({type}) and {requestId} ({type}) exists.",
                        LogHelpers.Arguments(
                            relation.OriginalObject.MaverickId,
                            relation.OriginalObject.ObjectType,
                            relatedRepoRelation.MaverickId,
                            relatedRepoRelation.ObjectType,
                            process.System));

                    continue;
                }

                _logger.LogDebugMessage(
                    "Add relation between {oId} ({oType}) and {rId} ({rType}) to delete.",
                    LogHelpers.Arguments(
                        relation.OriginalObject.MaverickId,
                        relation.OriginalObject.ObjectType,
                        relatedRepoRelation.MaverickId,
                        relatedRepoRelation.ObjectType));

                if (relationsToDelete.TryGetValue(relation.OriginalObject.MaverickId, out IRelation rel))
                {
                    rel.RelatedObjects.Add(relatedRepoRelation);
                }
                else
                {
                    relationsToDelete.Add(
                        relation.OriginalObject.MaverickId,
                        new DeletedRelation(
                            relation.OriginalObject,
                            new List<ObjectRelation>
                            {
                                relatedRepoRelation
                            }));
                }
            }
        }

        List<IRelation> relations = relationsToDelete.Values.ToList();

        _logger.LogInfoMessage("Extracted {count} relation to delete.", LogHelpers.Arguments(relations.Count));

        return _logger.ExitMethod(relations);
    }
}
