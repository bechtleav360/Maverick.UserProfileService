using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.States;
using UserProfileService.Sync.Utilities;

namespace UserProfileService.Sync.Services;

/// <summary>
///     Class to handle relations for entities.
/// </summary>
public class RelationHandler
{
    private readonly SagaConsumeContext<ProcessState, ISyncMessage> _context;
    private readonly ILogger<RelationHandler> _logger;

    private readonly ISynchronizationReadDestination<OrganizationSync> _organizationDestination;
    private readonly ISynchronizationWriteDestination<OrganizationSync> _organizationWriteDestination;
    private readonly IProcessTempHandler _processTempHandler;

    private Step CurrentStep => _context.Saga.Process.CurrentStep;

    private Process Process => _context.Saga.Process;

    /// <summary>
    ///     Create an instance of <see cref="RelationHandler" />.
    /// </summary>
    /// <param name="context">Context of related sync process.</param>
    /// <param name="serviceProvider">Provider to retrieve services.</param>
    public RelationHandler(
        SagaConsumeContext<ProcessState, ISyncMessage> context,
        IServiceProvider serviceProvider)
    {
        _context = context;

        _logger = serviceProvider.GetRequiredService<ILogger<RelationHandler>>();

        _organizationDestination =
            serviceProvider.GetRequiredService<ISynchronizationReadDestination<OrganizationSync>>();

        _organizationWriteDestination =
            serviceProvider.GetRequiredService<ISynchronizationWriteDestination<OrganizationSync>>();

        _processTempHandler = serviceProvider.GetRequiredService<IProcessTempHandler>();
    }

    private async Task HandleAddOrganizationRelations(
        IList<IRelation> organizationRelations,
        IList<IRelation> organizationRepoRelations)
    {
        _logger.EnterMethod();

        IList<IRelation> relationsToCreate =
            ExtractMissingRelationToCreate(organizationRelations, organizationRepoRelations);

        _logger.LogInfoMessage(
            "Found {total} relations to create.",
            LogHelpers.Arguments(relationsToCreate.Count));

        IList<RelationProcessingObject> relationCreationOperations =
            await _organizationWriteDestination.HandleRelationsAsync(
                _context.Saga.Process.CurrentStep.CollectingId.GetValueOrDefault(),
                relationsToCreate);

        IEnumerable<Guid> createOperations =
            relationCreationOperations
                .Select(ro => ro.CommandId)
                .Distinct()
                .ToList();

        CurrentStep.Final.Create = createOperations.Count();

        _logger.LogInfoMessage(
            "Create {total} relations.",
            LogHelpers.Arguments(CurrentStep.Final.Create));

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTraceMessage(
                "Create {total} relations with ids {ids}.",
                LogHelpers.Arguments(
                    CurrentStep.Final.Create,
                    string.Join(" , ", createOperations)));
        }

        _logger.ExitMethod();
    }

    private async Task HandleDeleteOrganizationRelations(
        IList<IRelation> organizationRelations,
        IList<IRelation> organizationRepoRelations)
    {
        _logger.EnterMethod();

        IList<IRelation> relationsToDelete =
            ExtractRelationToDelete(organizationRepoRelations, organizationRelations);

        _logger.LogDebugMessage(
            "Found {total} relations to delete.",
            LogHelpers.Arguments(relationsToDelete.Count));

        IList<RelationProcessingObject> relationDeletionOperations =
            await _organizationWriteDestination.HandleRelationsAsync(
                _context.Saga.Process.CurrentStep.CollectingId.GetValueOrDefault(),
                relationsToDelete,
                true);

        List<Guid> deleteOperations = relationDeletionOperations
            .Select(ro => ro.CommandId)
            .Distinct()
            .ToList();

        CurrentStep.Final.Delete = deleteOperations.Count;

        _logger.LogInfoMessage(
            "Delete {total} relations.",
            LogHelpers.Arguments(CurrentStep.Final.Delete));

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTraceMessage(
                "Delete {total} relations with ids {ids}.",
                LogHelpers.Arguments(
                    CurrentStep.Final.Delete,
                    string.Join(" , ", deleteOperations)));
        }

        _logger.ExitMethod();
    }

    private async Task<IList<IRelation>> GetAllEntityRelationsAsync<TSyncModel>(ObjectType type)
        where TSyncModel : ISyncModel
    {
        _logger.EnterMethod();

        var relations = new List<IRelation>();

        IList<Guid> tempObjects =
            await _processTempHandler.GetTemporaryObjectKeysAsync<TSyncModel>(_context.Saga.Process.Id.ToString());

        _logger.LogDebugMessage(
            "Get all temp entities which have not been synchronized. Total: '{total}'",
            LogHelpers.Arguments(tempObjects.Count));

        foreach (Guid operationId in tempObjects)
        {
            var operationState =
                await _processTempHandler.GetTemporaryObjectAsync<TSyncModel>(
                    _context.Saga.Process.Id.ToString(),
                    operationId);

            relations.Add(
                new Relation(
                    new LookUpObject(
                        operationState.ExternalIds.FirstOrDefault(),
                        operationState.Id,
                        operationState.Source,
                        type),
                    operationState.RelatedObjects)); // TODO: ExternalId Refactoring
        }

        _logger.LogDebugMessage(
            "Found '{total}' relations to check and synchronize.",
            LogHelpers.Arguments(relations.Count));

        return _logger.ExitMethod(relations);
    }

    private IList<IRelation> ExtractMissingRelationToCreate(
        IList<IRelation> organizationRelations,
        IList<IRelation> repoRelations)
    {
        _logger.EnterMethod();

        var relationsToCreate = new Dictionary<string, IRelation>();

        foreach (IRelation relation in organizationRelations)
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
                bool sameRelationExists = repoRelations.Any(
                    or =>
                        CompareObjects(or.OriginalObject, relation.OriginalObject)
                        && or.RelatedObjects.Any(
                            ro =>
                                CompareObjects(ro, relatedRepoRelation)
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
                bool oppositeRelation = repoRelations.Any(
                    or =>
                    {
                        ObjectRelation matchedRelatedObject = or.RelatedObjects.FirstOrDefault(
                            ro =>
                                CompareObjects(ro, relation.OriginalObject));

                        if (matchedRelatedObject != null)
                        {
                            return CompareObjects(relatedRepoRelation, or.OriginalObject)
                                && matchedRelatedObject.AssignmentType.CompareOppositeRelationType(
                                    relatedRepoRelation
                                        .AssignmentType);
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

    private bool CompareObjects(ILookUpObject source, ILookUpObject target)
    {
        _logger.EnterMethod();

        _logger.LogTraceMessage(
            "Compare source {maverickId} (maverickId) and {type} (type) with target {maverickId} (maverickId) and {type} (type)",
            LogHelpers.Arguments(source.MaverickId, source.ObjectType, target.MaverickId, target.ObjectType));

        bool equal = source.MaverickId == target.MaverickId && source.ObjectType == target.ObjectType;

        return _logger.ExitMethod(equal);
    }

    private IList<IRelation> ExtractRelationToDelete(
        IList<IRelation> repoRelations,
        IList<IRelation> organizationRelations)
    {
        _logger.EnterMethod();

        var relationsToDelete = new Dictionary<string, IRelation>();

        foreach (IRelation relation in repoRelations)
        {
            foreach (ObjectRelation relatedRepoRelation in relation.RelatedObjects)
            {
                // Only update relations between objects that originate from the current system.
                if (relation.OriginalObject.Source != _context.Saga.Process.System
                    || relation.OriginalObject.Source != _context.Saga.Process.System)
                {
                    _logger.LogDebugMessage(
                        "The relation (opposite direction) between {id} ({type}) and {id} ({type}) is not from the current system {system}.",
                        LogHelpers.Arguments(
                            relation.OriginalObject.MaverickId,
                            relation.OriginalObject.ObjectType,
                            relatedRepoRelation.MaverickId,
                            relatedRepoRelation.ObjectType,
                            _context.Saga.Process.System));

                    continue;
                }

                // Checks if the existing relation exists in the same direction. 
                bool sameRelationExists = organizationRelations.Any(
                    or =>
                        CompareObjects(or.OriginalObject, relation.OriginalObject)
                        && or.RelatedObjects.Any(
                            ro =>
                                CompareObjects(ro, relatedRepoRelation)
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
                            _context.Saga.Process.System));

                    continue;
                }

                // Checks if the relation between the objects with the opposite label exists.
                // Father -> children (children), children -> father (parent)
                bool oppositeRelation = organizationRelations.Any(
                    or =>
                    {
                        ObjectRelation matchedRelatedObject = or.RelatedObjects.FirstOrDefault(
                            ro =>
                                CompareObjects(ro, relation.OriginalObject));

                        if (matchedRelatedObject != null)
                        {
                            return CompareObjects(relatedRepoRelation, or.OriginalObject)
                                && matchedRelatedObject.AssignmentType.CompareOppositeRelationType(
                                    relatedRepoRelation
                                        .AssignmentType);
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
                            _context.Saga.Process.System));

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

    /// <summary>
    ///     Handle relations for organizations.
    /// </summary>
    /// <param name="addRelation">Specifies whether relations are to be added.</param>
    /// <param name="deleteRelation">Specifies whether relations are to be removed.</param>
    /// <returns>Represents an asynchronous operation of handling relations.</returns>
    public async Task HandleOrganizationRelations(
        bool addRelation,
        bool deleteRelation)
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "Start sync for relation of type {objectType}",
            LogHelpers.Arguments(ObjectType.Organization));

        if (!addRelation && !deleteRelation)
        {
            _logger.LogInfoMessage(
                "No relation to add or delete for step {step}.",
                LogHelpers.Arguments(CurrentStep.Id));

            return;
        }

        _logger.LogDebugMessage("Get relations for organizations.", LogHelpers.Arguments());

        IList<IRelation> organizationRelations;

        try
        {
            organizationRelations =
                await GetAllEntityRelationsAsync<OrganizationSync>(ObjectType.Organization);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "An error occurred while retrieving all relations for {objectType} in source.",
                LogHelpers.Arguments(ObjectType.Organization));

            Process.SetStepStatus(StepStatus.Failure);

            return;
        }

        _logger.LogDebugMessage(
            "Found {total} relations for organizations.",
            LogHelpers.Arguments(organizationRelations.Count));

        // Set maverick id to related objects
        foreach (IRelation organizationRelation in organizationRelations)
        {
            _logger.LogTraceMessage(
                "Set maverick id for related objects of entity with external id {externalId}",
                LogHelpers.Arguments(organizationRelation.OriginalObject.ExternalId));

            foreach (ObjectRelation relatedObject in organizationRelation.RelatedObjects)
            {
                _logger.LogTraceMessage(
                    "Set maverick id for related object {relatedId} of entity with id {maverickId}",
                    LogHelpers.Arguments(
                        relatedObject.ExternalId.Id,
                        organizationRelation.OriginalObject.MaverickId));

                IRelation relatedOrg = organizationRelations.FirstOrDefault(
                    or =>
                        or.OriginalObject.ExternalId.Id == relatedObject.ExternalId.Id);

                if (relatedOrg != null)
                {
                    _logger.LogTraceMessage(
                        "Could found maverick id for related object {relatedId} of entity with id {maverickId}",
                        LogHelpers.Arguments(
                            relatedObject.ExternalId.Id,
                            organizationRelation.OriginalObject.MaverickId));

                    relatedObject.MaverickId = relatedOrg.OriginalObject.MaverickId;
                }
                else
                {
                    _logger.LogWarnMessage(
                        "Could not found maverick id for related object {relatedId} of entity with id {maverickId}",
                        LogHelpers.Arguments(
                            relatedObject.ExternalId.Id,
                            organizationRelation.OriginalObject.MaverickId));
                }
            }

            if (string.IsNullOrWhiteSpace(organizationRelation.OriginalObject.MaverickId))
            {
                _logger.LogWarnMessage(
                    "Could not found maverick id for original object with external id {relatedId}.",
                    LogHelpers.Arguments(organizationRelation.OriginalObject.ExternalId.Id));
            }
        }

        _logger.LogDebugMessage(
            "Remove organizations from list without relations.",
            LogHelpers.Arguments());

        // Remove all organizations without relation.
        organizationRelations = organizationRelations.Where(
                t =>
                    organizationRelations.Any(
                        x =>
                            x.RelatedObjects.Any(
                                r => r.ExternalId.Id
                                    == t.OriginalObject.ExternalId.Id))
                    || t.RelatedObjects.Any())
            .ToList();

        // When no relation are present, there is nothing to do.
        if (organizationRelations.Count == 0 || organizationRelations.All(r => !r.RelatedObjects.Any()))
        {
            _logger.LogInfoMessage(
                "No relations for organizations to synchronize.",
                LogHelpers.Arguments());

            Process.SetStepStatus(StepStatus.Success);
            CurrentStep.FinishedAt = DateTime.UtcNow;

            _logger.ExitMethod();

            return;
        }

        IList<OrganizationSync> repoOrganizations;

        try
        {
            _logger.LogInfoMessage(
                "Getting all organizations in destination system via batch.",
                LogHelpers.Arguments());

            // Get all entities from destination (maverick)
            repoOrganizations =
                await BatchUtility.GetAllEntities(_organizationDestination.GetObjectsAsync);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "An error occurred while retrieving all relations for {objectType} in destination.",
                LogHelpers.Arguments(ObjectType.Organization));

            Process.SetStepStatus(StepStatus.Failure);

            return;
        }

        _logger.LogInfoMessage(
            "Found {total} organizations in destination system.",
            LogHelpers.Arguments(repoOrganizations.Count));

        // Build collection of IRelation models
        List<IRelation> organizationRepoRelations = repoOrganizations
            .Select(
                t =>
                    new Relation(
                        new LookUpObject(
                            t.ExternalIds.FirstOrDefault(),
                            t.Id,
                            t.Source,
                            ObjectType.Organization),
                        t.RelatedObjects))
            .Cast<IRelation>()
            .ToList();

        _logger.LogDebugMessage(
            "Build collection of relation for organizations.",
            LogHelpers.Arguments(repoOrganizations.Count));

        try
        {
            // Extract relations to create.
            if (addRelation)
            {
                await HandleAddOrganizationRelations(
                    organizationRelations,
                    organizationRepoRelations);
            }

            // Extract relations to delete.
            if (deleteRelation)
            {
                await HandleDeleteOrganizationRelations(
                    organizationRelations,
                    organizationRepoRelations);
            }
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "An error occurred while creating or updating the relation for {objectType}.",
                LogHelpers.Arguments(ObjectType.Organization));

            if (CurrentStep.Final.Total == 0)
            {
                _logger.LogInfoMessage(
                    "Since no other operations were performed and the relation step failed, possible operations of the relation are lost. This means that the step has failed.",
                    LogHelpers.Arguments());

                Process.SetStepStatus(StepStatus.Failure);

                return;
            }

            _logger.LogInfoMessage(
                "The error from performing relation operations are ignored because other operations were performed. Thus, a partial success can be ensured.",
                LogHelpers.Arguments());
        }

        _logger.ExitMethod();
    }
}
