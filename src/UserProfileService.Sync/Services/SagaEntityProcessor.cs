using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.Utilities;
using UserProfileService.Validation.Abstractions.Configuration;

namespace UserProfileService.Sync.Services;

/// <summary>
///     Handles the entire synchronization process.
/// </summary>
/// <typeparam name="TSyncEntity"> The sync entity that will be synchronized. </typeparam>
public class SagaEntityProcessor<TSyncEntity> : ISagaEntityProcessor<TSyncEntity>
    where TSyncEntity : class, ISyncModel
{
    /// <summary>
    ///     The systems holds the whole synchronization configuration for the source and the
    ///     destination system.
    /// </summary>
    protected readonly SyncConfiguration Configuration;

    /// <summary>
    ///     A converter factory to create converters <see cref="IConverter{T}" />
    /// </summary>
    protected readonly IConverterFactory<TSyncEntity> ConverterFactory;

    /// <summary>
    ///     Logger instance that will be used to log incoming messages of the registration process.
    /// </summary>
    protected readonly ILogger<SagaEntityProcessor<TSyncEntity>> Logger;

    /// <summary>
    ///     Handler to save entities temporary for further process.
    /// </summary>
    protected readonly IProcessTempHandler ProcessTempHandler;

    /// <summary>
    ///     This interface is used to read from the destination system.
    /// </summary>
    protected readonly ISynchronizationReadDestination<TSyncEntity> ReadDestination;

    /// <summary>
    ///     Describes the implementation of a factory to create instances of <see cref="ISynchronizationSourceSystem{T}" />.
    /// </summary>
    protected readonly ISyncSourceSystemFactory SourceSystemFactory;

    /// <summary>
    ///     Describes a service to compare objects of <see cref="ISyncModel" />
    /// </summary>
    protected readonly ISyncModelComparer<TSyncEntity> SyncModelComparer;

    /// <summary>
    ///     This interface is used to write objects in the destination target system.
    /// </summary>
    protected readonly ISynchronizationWriteDestination<TSyncEntity> WriteDestination;

    /// <summary>
    ///     Creates an instance <see cref="SagaEntityProcessor{TSyncEntity}" />.
    /// </summary>
    /// <param name="serviceProvider"> The service provider to get registered services. </param>
    /// <param name="loggerFactory"> The logger factory for logging purposes. </param>
    /// <param name="syncConfiguration"> The synchronization configuration. </param>
    public SagaEntityProcessor(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        SyncConfiguration syncConfiguration)
    {
        // Read from maverick
        ReadDestination = serviceProvider.GetRequiredService<ISynchronizationReadDestination<TSyncEntity>>();
        // Write in Maverick
        WriteDestination = serviceProvider.GetRequiredService<ISynchronizationWriteDestination<TSyncEntity>>();

        // Read from source system like AD 
        SourceSystemFactory = serviceProvider.GetRequiredService<ISyncSourceSystemFactory>();

        var comparerFactory = serviceProvider.GetRequiredService<ISyncModelComparerFactory>();
        SyncModelComparer = comparerFactory.CreateComparer<TSyncEntity>();

        ConverterFactory = serviceProvider.GetRequiredService<IConverterFactory<TSyncEntity>>();

        ProcessTempHandler = serviceProvider.GetRequiredService<IProcessTempHandler>();

        Configuration = syncConfiguration;
        Logger = loggerFactory.CreateLogger<SagaEntityProcessor<TSyncEntity>>();
    }

    private KeyProperties ModifyExternalIdFilterForValidationConfiguration(
        TSyncEntity entity,
        KeyProperties externalId,
        ModelAttribute syncModelAttribute)
    {
        Logger.EnterMethod();

        ValidationConfiguration validationConfiguration = Configuration.SourceConfiguration.Validation;

        var externalIdDefinition = new Definitions
        {
            FieldName = nameof(ISyncModel.ExternalIds),
            Values = new[] { externalId.Id },
            BinaryOperator = BinaryOperator.And,
            Operator = FilterOperator.Equals
        };

        if (!validationConfiguration.Internal.Group.Name.Duplicate
            && syncModelAttribute.Model == SyncConstants.Models.Group
            && entity is GroupSync group)
        {
            bool ignoreCase = validationConfiguration.Internal.Group.Name.IgnoreCase;

            Logger.LogDebugMessage(
                "Add duplicate name filter to external id to identify groups with same name while synchronization of entity with external id {id}.",
                LogHelpers.Arguments(entity.Id));

            externalId.Filter = new Filter
            {
                CombinedBy = BinaryOperator.Or,
                Definition = new List<Definitions>
                {
                    new Definitions
                    {
                        FieldName = nameof(GroupSync.Name),
                        Values = new[] { group.Name, group.DisplayName },
                        BinaryOperator = BinaryOperator.Or,
                        Operator = ignoreCase
                            ? FilterOperator.Contains
                            : FilterOperator.Equals
                    },
                    new Definitions
                    {
                        FieldName = nameof(GroupSync.DisplayName),
                        Values = new[] { group.Name, group.DisplayName },
                        BinaryOperator = BinaryOperator.Or,
                        Operator = ignoreCase
                            ? FilterOperator.Contains
                            : FilterOperator.Equals
                    },
                    externalIdDefinition
                }
            };

            if (ignoreCase)
            {
                externalId.PostFilter = models =>
                {
                    return models
                        .Cast<GroupSync>()
                        .Where(
                            m => ValidationLogicProvider.Group.CompareNames(
                                    group.Name,
                                    group.DisplayName,
                                    m.Name,
                                    m.DisplayName)
                                // To differentiate if a group was renamed and has to be
                                // updated.
                                || (group.ExternalIds?.FirstOrDefaultUnconverted()?.Id != null
                                    && group.ExternalIds?.FirstOrDefaultUnconverted()?.Id
                                    == m.ExternalIds.FirstOrDefaultUnconverted()?.Id));
                };
            }
        }

        if (!validationConfiguration.Internal.User.DuplicateEmailAllowed
            && syncModelAttribute.Model == SyncConstants.Models.User
            && entity is UserSync user)
        {
            Logger.LogDebugMessage(
                "Add duplicate email filter to external id to identify users with same email while synchronization of entity with external id {id}.",
                LogHelpers.Arguments(entity.Id));

            externalId.Filter = new Filter
            {
                CombinedBy = BinaryOperator.Or,
                Definition = new List<Definitions>
                {
                    externalIdDefinition
                }
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                externalId.Filter.Definition.Add(
                    new Definitions
                    {
                        FieldName = nameof(UserSync.Email),
                        Values = new[] { user.Email },
                        BinaryOperator = BinaryOperator.And
                    });
            }
        }

        externalId.Filter ??= new Filter
        {
            CombinedBy = BinaryOperator.Or,
            Definition = new List<Definitions>
            {
                externalIdDefinition
            }
        };

        return Logger.ExitMethod(externalId);
    }

    private async Task<CommandResult> CreateEntity(TSyncEntity entity, Process syncProcess, CancellationToken ctx)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Create new entity for external identifier '{externalId}'",
            LogHelpers.Arguments(entity.Id));

        CommandResult result =
            await WriteDestination.CreateObjectAsync(
                syncProcess.CurrentStep.CollectingId.GetValueOrDefault(),
                entity,
                ctx);

        Logger.LogDebugMessage(
            "Created new entity for external identifier '{externalId}' with request id '{requestId}'.",
            LogHelpers.Arguments(entity.Id, result.Id));

        syncProcess.CurrentStep.Final.Create++;

        return Logger.ExitMethod(result);
    }

    private async Task<CommandResult> UpdateEntity(
        TSyncEntity repoEntity,
        TSyncEntity entity,
        Process syncProcess,
        Step stepData,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Update entity for id and external identifier '{externalId}'",
            LogHelpers.Arguments(repoEntity.Id, entity.Id));

        // Adding the repo external ids to the updated object,
        // since it contains only the external id of the original system.
        // This prevents the comparison of the two objects from being incorrect.
        entity.ExternalIds = ConcatRepoExternalIdsOfOtherSystems(repoEntity, entity, syncProcess.System);

        bool equal = SyncModelComparer.CompareObject(
            repoEntity,
            entity,
            out IDictionary<string, object> modifiedProperties);

        if (!equal)
        {
            Logger.LogInfoMessage(
                "Update entity for maverick id '{id}' for {count} properties '{properties}', because repo entity and new entity are not equal.",
                LogHelpers.Arguments(
                    entity.Id,
                    modifiedProperties.Count,
                    string.Join(", ", modifiedProperties.Keys)));

            TSyncEntity modifiedRepoEntity = OverwriteEntityWithProperties(repoEntity, modifiedProperties);

            modifiedRepoEntity.RelatedObjects = entity.RelatedObjects;

            CommandResult result = await WriteDestination.UpdateObjectAsync(
                syncProcess.CurrentStep.CollectingId.GetValueOrDefault(),
                modifiedRepoEntity,
                modifiedProperties.Keys.ToHashSet(),
                cancellationToken);

            if (!result.Success)
            {
                Logger.LogErrorMessage(
                    null,
                    "Updated entity for maverick id '{id}' failed while sending to destination system.",
                    LogHelpers.Arguments(repoEntity.Id));
            }

            stepData.Final.Update++;

            Logger.LogInfoMessage(
                "Updated entity for maverick id '{id}' with request id '{requestId}'",
                LogHelpers.Arguments(repoEntity.Id, result.Id));

            return result;
        }

        Logger.LogInfoMessage(
            "No update for entity with id '{id}', because repo entity and new entity are equal.",
            LogHelpers.Arguments(
                entity.Id,
                modifiedProperties.Count,
                string.Join(", ", modifiedProperties.Keys)));

        return Logger.ExitMethod<CommandResult>(null);
    }

    private async Task DeleteEntities(
        IList<TSyncEntity> entities,
        Process syncProcess,
        Step stepData,
        CancellationToken ctx)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Start synchronizing entity for operation 'delete' in system '{system}'.",
            LogHelpers.Arguments(syncProcess.System));

        // Remove all entities that are not in the list and fit the current system.
        IList<TSyncEntity> deletionEntities =
            await BatchUtility.GetAllEntities(
                ReadDestination.GetObjectsAsync,
                re => entities.All(e => !e.CompareInternalAndExternalId(re))
                    && re.ExternalIds.Any(ei => ei.Source == syncProcess.System),
                ctx);

        if (!deletionEntities.Any())
        {
            Logger.LogInfoMessage(
                "Finished synchronizing entity for operation 'delete' in system '{system}'. No entities to delete.",
                LogHelpers.Arguments(syncProcess.System));

            Logger.ExitMethod();

            return;
        }

        Logger.LogInfoMessage(
            "Found {system} entities to delete for system '{system}' and step {system}.",
            LogHelpers.Arguments(
                deletionEntities.Count,
                syncProcess.System,
                syncProcess.Step));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Found {system} entities to delete for system '{system}' and step {system}. Entities to delete: {entities}",
                LogHelpers.Arguments(
                    deletionEntities.Count,
                    syncProcess.System,
                    syncProcess.Step,
                    string.Join(" , ", deletionEntities.Select(d => d.Id))));
        }

        IList<CommandResult> deletionResult =
            await WriteDestination.DeleteObjectsAsync(
                syncProcess.CurrentStep.CollectingId.GetValueOrDefault(),
                deletionEntities,
                syncProcess.System,
                ctx);

        stepData.Final.Delete = deletionResult.Select(dr => dr.Id).Distinct().Count();

        Logger.LogInfoMessage(
            "Finished synchronizing entity for operation 'delete' in system '{system}'. Total deleted entities : {total} / .",
            LogHelpers.Arguments(syncProcess.System, stepData.Final.Delete, deletionEntities.Count));

        Logger.ExitMethod();
    }

    private IList<KeyProperties> ConcatRepoExternalIdsOfOtherSystems(
        TSyncEntity repoEntity,
        TSyncEntity entity,
        string system)
    {
        Logger.EnterMethod();

        IEnumerable<KeyProperties> otherExternalIds = repoEntity.ExternalIds.Where(ei => ei.Source != system);

        List<KeyProperties> keyProperties = entity.ExternalIds.Concat(otherExternalIds).ToList();

        return Logger.ExitMethod(keyProperties);
    }

    private TSyncEntity OverwriteEntityWithProperties(
        TSyncEntity repoEntity,
        IDictionary<string, object> modifiedProperties)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "Overwrite entity properties with new modified properties (Total: {total}).",
            LogHelpers.Arguments(modifiedProperties.Count));

        foreach (KeyValuePair<string, object> modifiedProperty in modifiedProperties)
        {
            Logger.LogTraceMessage(
                "Overwrite entity property for key '{property}'.",
                LogHelpers.Arguments(modifiedProperty.Key));

            PropertyInfo propertyInfo = repoEntity
                .GetType()
                .GetProperty(modifiedProperty.Key);

            if (propertyInfo == null)
            {
                Logger.LogWarnMessage(
                    "Could not find modified property '{property}' of entity '{id}'",
                    LogHelpers.Arguments(modifiedProperty.Key, repoEntity.Id));

                continue;
            }

            Logger.LogTraceMessage(
                "Find modified property '{property}' of entity '{id}' and value will be set,",
                LogHelpers.Arguments(modifiedProperty.Key, repoEntity.Id));

            try
            {
                object convertedValue = modifiedProperty.Value.GetType() == propertyInfo.DeclaringType
                    || propertyInfo.PropertyType.IsInstanceOfType(modifiedProperty.Value)
                        ? modifiedProperty.Value
                        : Convert.ChangeType(modifiedProperty.Value, propertyInfo.PropertyType);

                propertyInfo.SetValue(repoEntity, convertedValue);

                Logger.LogTraceMessage(
                    "Successfully set modified property '{property}' to entity '{id}'.",
                    LogHelpers.Arguments(modifiedProperty.Key, repoEntity.Id));
            }
            catch (Exception e)
            {
                Logger.LogWarnMessage(
                    e,
                    "Could not set modified property '{property}' to entity '{id}'.",
                    LogHelpers.Arguments(modifiedProperty.Key, repoEntity.Id));
            }
        }

        Logger.LogDebugMessage(
            "Finished overwriting properties for entity with id '{id}'.",
            LogHelpers.Arguments(repoEntity.Id));

        return Logger.ExitMethod(repoEntity);
    }

    /// <summary>
    ///     Handles the entities that should be synchronized.
    /// </summary>
    /// <param name="syncProcess">Contains the sync states that the sync is into. </param>
    /// <param name="correlationId"> The correlation id. </param>
    /// <param name="saveAction">
    ///     Action that can be executed to write the current status to the database. During the process in
    ///     order to keep the data as up-to-date as possible.
    /// </param>
    /// <param name="ctx">Propagates notification that operations should be canceled.</param>
    /// <returns></returns>
    public virtual async Task HandleEntitySync(
        Process syncProcess,
        string correlationId,
        Func<Process, Task> saveAction = null,
        CancellationToken ctx = default)
    {
        Logger.EnterMethod();

        ModelAttribute syncModelAttribute = typeof(TSyncEntity)
            .GetCustomAttributeValue<ModelAttribute, ModelAttribute>(t => t);

        Step stepData = syncProcess.CurrentStep;

        syncProcess.SetStepStatus(StepStatus.InProgress);

        if (saveAction != null)
        {
            await saveAction(syncProcess);
        }

        if (syncModelAttribute == null)
        {
            Logger.LogErrorMessage(
                null,
                "Attribute '{nameof(SyncModelAttribute)}' of entity '{typeof(TSyncEntity).Name}' is not allowed to be null.",
                LogHelpers.Arguments(nameof(ModelAttribute), typeof(TSyncEntity).Name));

            throw new ArgumentNullException(
                nameof(syncModelAttribute),
                $"Attribute '{nameof(ModelAttribute)}' of entity '{typeof(TSyncEntity).Name}' is not allowed to be null.");
        }

        Logger.LogInfoMessage(
            "Start synchronization of entity {entity} and system {system}",
            LogHelpers.Arguments(syncModelAttribute.Model, syncProcess.System));

        Logger.LogDebugMessage("Create source system for '{system}'.", LogHelpers.Arguments(syncProcess.System));

        ISynchronizationSourceSystem<TSyncEntity> sourceSystemHandler =
            SourceSystemFactory.Create<TSyncEntity>(syncProcess.System);

        Logger.LogDebugMessage("Getting all entities via batch.", LogHelpers.Arguments());

        IList<TSyncEntity> entities;

        try
        {
            entities = await BatchUtility.GetAllEntities(sourceSystemHandler.GetBatchAsync, ctx);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(e, "An error occurred when fetching the entities.", LogHelpers.Arguments());
            syncProcess.SetStepStatus(StepStatus.Failure);

            return;
        }

        if (Configuration.SourceConfiguration.Systems.TryGetValue(
                syncProcess.System,
                out SourceSystemConfiguration sourceSystemConfiguration))
        {
            IConverter<TSyncEntity> converter = ConverterFactory.CreateConverter(
                sourceSystemConfiguration,
                syncProcess.Step);

            if (converter != null)
            {
                Logger.LogInfoMessage(
                    "Start converting external ids of entities with the {converter}.",
                    LogHelpers.Arguments(nameof(converter)));

                entities = entities.Select(converter.Convert).ToList();
            }
        }

        Logger.LogInfoMessage(
            "Fetched {total} entities for {entity} in source system {system}.",
            LogHelpers.Arguments(entities.Count, syncModelAttribute.Model, syncProcess.System));

        syncProcess.SetStepStatus(StepStatus.Fetching);

        SynchronizationOperation syncOperation = stepData.Operations;

        if (saveAction != null)
        {
            await saveAction(syncProcess);
        }

        if (entities.Count == 0)
        {
            Logger.LogErrorMessage(
                null,
                "The synchronization process is aborted prematurely. No entities were found in the source system. An error seems to have occurred during the query of entities.",
                LogHelpers.Arguments());

            syncProcess.SetStepStatus(StepStatus.SuccessWithHints);

            if (saveAction != null)
            {
                await saveAction?.Invoke(syncProcess);
            }

            return;
        }

        stepData.Temporary.Entities = entities.Count;

        bool addFlag = syncOperation.HasFlag(SynchronizationOperation.Add);
        bool updateFlag = syncOperation.HasFlag(SynchronizationOperation.Update);

        Logger.LogDebugMessage(
            "Synchronization process for system {system} includes add = {add} and update = {update}.",
            LogHelpers.Arguments(syncProcess.System, addFlag, updateFlag));

        if (addFlag || updateFlag)
        {
            Logger.LogInfoMessage(
                "Start synchronizing entity for operation 'add' ({add}) or/and 'update' ({update})  in system '{system}'",
                LogHelpers.Arguments(addFlag, updateFlag, syncProcess.System));

            stepData.Temporary.Analyzed = 0;
            stepData.UpdatedAt = DateTime.UtcNow;

            int totalEntities = entities.Count;

            foreach (TSyncEntity entity in entities)
            {
                Logger.LogInfoMessage(
                    "({current}/{total}) Start sync process for entity with external id '{id}' and source '{source}'.",
                    LogHelpers.Arguments(
                        syncProcess.CurrentStep.Temporary.Analyzed,
                        totalEntities,
                        entity.Id,
                        entity.Source));

                var entityCommandId = Guid.NewGuid();

                stepData.Temporary.Analyzed++;
                stepData.UpdatedAt = DateTime.UtcNow;

                // After every ten entities save
                if (stepData.Temporary.Analyzed % 10 == 0)
                {
                    await saveAction?.Invoke(syncProcess);
                }

                try
                {
                    KeyProperties externalId =
                        entity.ExternalIds.FirstOrDefaultUnconverted(syncProcess.System);

                    if (externalId == null)
                    {
                        Logger.LogWarnMessage(
                            "No external identifier is set for the external entity with id '{id}'.",
                            LogHelpers.Arguments(entity.Id));

                        continue;
                    }

                    externalId = ModifyExternalIdFilterForValidationConfiguration(
                        entity,
                        externalId,
                        syncModelAttribute);

                    // Maverick
                    IEnumerable<TSyncEntity> repoEntities = await BatchUtility.GetAllEntities(
                        ReadDestination.GetObjectsAsync,
                        externalId,
                        ctx);

                    Logger.LogDebugMessage(
                        "Found {count} repoEntities for entity with external id '{id}' and source '{source}'.",
                        LogHelpers.Arguments(repoEntities?.Count() ?? 0, entity.Id, entity.Source));

                    // If no entities exists in destination and entity is configured to create.
                    bool repoEntityNotExists = repoEntities == null || !repoEntities.Any();

                    if (repoEntityNotExists && addFlag)
                    {
                        Logger.LogInfoMessage(
                            "Create entity with id '{id}' for system '{system}'.",
                            LogHelpers.Arguments(entity.Id, syncProcess.System));

                        // Create entity
                        await CreateEntity(entity, syncProcess, ctx);
                    }

                    if (!repoEntityNotExists && updateFlag)
                    {
                        Logger.LogInfoMessage(
                            "Update entity with id '{id}' for system '{system}'.",
                            LogHelpers.Arguments(entity.Id, syncProcess.System));

                        // All repo entities that is already linked to the current entity with external id.
                        IEnumerable<TSyncEntity> linkedEntities = repoEntities
                            .Where(
                                repoEntity => repoEntity.ExternalIds.Any(
                                    r => r.Id == externalId.Id
                                        && r.Source == externalId.Source))
                            .ToList();

                        // If there are already linked entities, only these should be edited. 
                        // Otherwise, all entities are processed,
                        // as it cannot be ensured which of the entities is the "most important".
                        ICollection<TSyncEntity> editableEntities =
                            linkedEntities.Any() ? linkedEntities.ToList() : repoEntities.ToList();

                        Logger.LogInfoMessage(
                            "Found {count} editable entities for external id '{id}'. Editable entities: {editableEntities}",
                            LogHelpers.Arguments(
                                editableEntities.Count,
                                entity.Id,
                                string.Join(" , ", editableEntities.Select(e => e.Id))));

                        foreach (TSyncEntity repoEntity in editableEntities)
                        {
                            Logger.LogInfoMessage(
                                "Update repo entity {id} for entity with external id {id}",
                                LogHelpers.Arguments(repoEntity.Id, entity.Id));

                            if (repoEntity.ExternalIds.All(r => r.Id != externalId.Id))
                            {
                                Logger.LogDebugMessage(
                                    "The repo entity with id '{id}' was not found by the external id, but by an extended filter.",
                                    LogHelpers.Arguments(repoEntity.Id));
                            }

                            // Prepare external ids like extern to make sure the filter is equal.
                            repoEntity.ExternalIds = repoEntity.ExternalIds.Select(
                                    e =>
                                        ModifyExternalIdFilterForValidationConfiguration(
                                            repoEntity,
                                            e,
                                            syncModelAttribute))
                                .ToList();

                            entity.Id = repoEntity.Id;

                            CommandResult result = await UpdateEntity(
                                repoEntity,
                                entity,
                                syncProcess,
                                stepData,
                                ctx);

                            Logger.LogInfoMessage(
                                "Finished synchronization for entity with id '{id}' and external identifier '{externalId}'. Operation processed because entities are not equal: {equal}",
                                LogHelpers.Arguments(repoEntity?.Id, entity.Id, result == null));
                        }

                        Logger.LogInfoMessage(
                            "Finish updating entity with id '{id}' for system '{system}'.",
                            LogHelpers.Arguments(entity.Id, syncProcess.System));
                    }

                    Logger.LogInfoMessage(
                        "No add or update operation was processed for the entity with external id '{id}'.",
                        LogHelpers.Arguments(entity.Id));

                    await ProcessTempHandler.AddTemporaryObjectAsync(
                        syncProcess.Id.ToString(),
                        entityCommandId,
                        entity);
                }
                catch (Exception e)
                {
                    Logger.LogErrorMessage(
                        e,
                        "An unexpected error occurs while processing entity with id '{id}' for system '{system}'",
                        LogHelpers.Arguments(entity.Id, syncProcess.System));
                }
            }

            Logger.LogInfoMessage(
                "Finished synchronizing entity for operation 'add' in system '{system}'. Total entities to create: {total}",
                LogHelpers.Arguments(syncProcess.System, stepData.Final.Create));
        }
        else
        {
            Logger.LogInfoMessage(
                "Synchronizing entity for operation 'add' and 'update' in system '{system}' is not be defined and will be skipped.",
                LogHelpers.Arguments(syncProcess.System));
        }

        stepData.UpdatedAt = DateTime.UtcNow;

        await saveAction?.Invoke(syncProcess);

        // Process entity deletion

        if (syncOperation.HasFlag(SynchronizationOperation.Delete))
        {
            Logger.LogInfoMessage(
                "Synchronizing entity for operation 'delete' in system '{system}'",
                LogHelpers.Arguments(syncProcess.System));

            try
            {
                await DeleteEntities(entities, syncProcess, stepData, ctx);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(e, "An error occurred while deleting the entities.", LogHelpers.Arguments());

                if (stepData.Final.Total == 0)
                {
                    Logger.LogInfoMessage(
                        "Since no other operations were performed and the delete failed, possible operations of the delete are lost. This means that the step has failed.",
                        LogHelpers.Arguments());

                    syncProcess.SetStepStatus(StepStatus.Failure);

                    return;
                }

                Logger.LogInfoMessage(
                    "The error from deleting the entities is ignored because other operations were performed. Thus, a partial success can be ensured.",
                    LogHelpers.Arguments());
            }
        }

        syncProcess.SetStepStatus(StepStatus.WaitingForResponse);

        await saveAction?.Invoke(syncProcess);

        Logger.LogInfoMessage(
            "Finished synchronization of entity {entity} and system {system}. Total entities to handle: {total}",
            LogHelpers.Arguments(syncModelAttribute.Model, syncProcess.System, stepData.Final.Total));

        await saveAction?.Invoke(syncProcess);

        Logger.ExitMethod();
    }
}
