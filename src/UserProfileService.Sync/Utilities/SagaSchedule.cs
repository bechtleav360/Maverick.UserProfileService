using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Utilities;

/// <summary>
///     Produces a schedule for a sage synchronization that depends on the
///     source system and the configuration for it. The first and last step
///     are predefined.
/// </summary>
public static class SagaSchedule
{
    private static bool TryGetNextStep(string currentStepId, Models.State.System currentSystem, out Step nextStep)
    {
        // first step
        if (string.IsNullOrWhiteSpace(currentStepId))
        {
            currentSystem.StartedAt = DateTime.Now;
            currentSystem.UpdatedAt = DateTime.Now;

            nextStep = currentSystem.Steps.FirstOrDefault(s => currentSystem.Steps.Values.All(v => v.Next != s.Key))
                .Value;

            // system has no steps/entities to synchronize defined.
            if (nextStep == null)
            {
                return false;
            }

            nextStep.StartedAt = DateTime.UtcNow;
            nextStep.UpdatedAt = DateTime.UtcNow;

            return true;
        }

        Step currentStep = currentSystem.Steps[currentStepId];

        currentStep.UpdatedAt = DateTime.UtcNow;
        currentStep.FinishedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(currentStep.Next))
        {
            nextStep = null;

            return false;
        }

        // next step
        nextStep = currentSystem.Steps[currentStep.Next];

        return true;
    }

    /// <summary>
    ///     Set the next saga step that depends on the current sync system data..
    /// </summary>
    /// <param name="process">The current sync process.</param>
    /// <param name="logger">The optional logger.</param>
    /// <returns>The next saga step for the synchronization.</returns>
    public static void SetNextSystemStep(Process process, ILogger logger = null)
    {
        // First system
        if (string.IsNullOrWhiteSpace(process.System))
        {
            logger?.LogInfoMessage("First system will be selected.", LogHelpers.Arguments());

            process.System = process.Systems.FirstOrDefault(s => process.Systems.Values.All(v => v.Next != s.Key))
                .Key;

            if (string.IsNullOrWhiteSpace(process.System))
            {
                return;
            }
        }

        Models.State.System currentSystem = process.Systems[process.System];

        bool hasNextStep = TryGetNextStep(process.Step, currentSystem, out Step nextStep);

        if (hasNextStep)
        {
            nextStep.StartedAt = DateTime.UtcNow;
            process.Step = nextStep.Id;

            process.UpdateStepTime();

            return;
        }

        process.UpdateSystemTime();
        currentSystem.FinishedAt = DateTime.UtcNow;

        process.System = process.Systems[process.System].Next;
        process.Step = null;

        if (string.IsNullOrWhiteSpace(process.System))
        {
            return;
        }

        SetNextSystemStep(process, logger);
    }

    /// <summary>
    ///     Build a saga schedule for a saga synchronization that is depends on the source system.
    /// </summary>
    public static Process BuildSagaSchedule(
        Guid id,
        SyncConfiguration configuration,
        ILogger logger = null)
    {
        logger?.EnterMethod();

        var process = new Process
        {
            Id = id,
            StartedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (logger?.IsEnabled(LogLevel.Trace) == true)
        {
            logger.LogInfoMessage(
                "Start building sync saga schedule base on sync configuration {configuration}.",
                JsonConvert.SerializeObject(configuration).AsArgumentList());
        }
        else
        {
            logger?.LogInfoMessage(
                "Start building sync saga schedule base on sync configuration.",
                LogHelpers.Arguments());
        }

        Models.State.System lastSystem = null;

        foreach ((string systemKey, SourceSystemConfiguration systemConfiguration) in configuration
                     .SourceConfiguration.Systems)
        {
            logger?.LogInfoMessage(
                "Start building sync process for system '{system}'.",
                LogHelpers.Arguments(systemKey));

            if (lastSystem != null)
            {
                lastSystem.Next = systemKey;
            }

            var system = new Models.State.System
            {
                Id = systemKey
            };

            process.Systems.Add(system.Id, system);

            if (systemConfiguration == null)
            {
                var errorMessage =
                    $"A configuration for the source system {systemKey} could not be found. No schedule could be created. Synchronization for this system not possible.";

                logger?.LogWarnMessage(errorMessage, LogHelpers.Arguments());

                throw new ArgumentNullException(errorMessage);
            }

            logger?.LogDebugMessage(
                "System configuration for '{system}' extracted successful.",
                LogHelpers.Arguments(systemKey));

            Step lastStep = null;

            logger?.LogDebugMessage("Build saga steps for system '{system}'.", LogHelpers.Arguments(systemKey));

            foreach ((string entityKey, SynchronizationOperations entitySystemOperation) in systemConfiguration
                         .Source)
            {
                if (lastStep != null)
                {
                    lastStep.Next = entityKey;
                }

                var step = new Step
                {
                    Id = entityKey,
                    Operations = entitySystemOperation.Operations
                };

                system.Steps.Add(step.Id, step);

                if (logger?.IsEnabled(LogLevel.Trace) == true)
                {
                    logger?.LogTraceMessage(
                        "Add step '{system}' for system '{system}' and operations '{operations}'.",
                        LogHelpers.Arguments(
                            entityKey,
                            systemKey,
                            JsonConvert.SerializeObject(entitySystemOperation)));
                }
                else
                {
                    logger?.LogDebugMessage(
                        "Add step '{system}' for system '{system}' and operations '{operations}'.",
                        LogHelpers.Arguments(entityKey, systemKey, entitySystemOperation.Operations));
                }

                lastStep = step;
            }

            lastSystem = system;

            // No step defined in source system
            if (lastStep == null)
            {
                continue;
            }

            // If none of the steps has one of the following operations,
            // the relations should not be processed. 
            if (system.Steps.All(
                    s => !s.Value.Operations.HasFlag(SynchronizationOperation.Add)
                        && !s.Value.Operations.HasFlag(SynchronizationOperation.Update)
                        && !s.Value.Operations.HasFlag(SynchronizationOperation.Delete)))
            {
                continue;
            }

            lastStep.Next = SyncConstants.SagaStep.DeletedRelationStep;

            var deleteRelationStep = new Step
            {
                Id = SyncConstants.SagaStep.DeletedRelationStep,
                Next = SyncConstants.SagaStep.AddedRelationStep
            };

            system.Steps.Add(deleteRelationStep.Id, deleteRelationStep);

            var addRelationStep = new Step
            {
                Id = SyncConstants.SagaStep.AddedRelationStep
            };

            system.Steps.Add(addRelationStep.Id, addRelationStep);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
            {
                logger.LogTraceMessage(
                    "Finished building sync process for system '{system}' and state '{system}'.",
                    LogHelpers.Arguments(systemKey, JsonConvert.SerializeObject(system)));
            }
            else
            {
                logger?.LogInfoMessage(
                    "Finished building sync process for system '{system}'.",
                    LogHelpers.Arguments(systemKey));
            }
        }

        process.UpdateProcessTime();

        logger?.LogInfoMessage(
            "Finished building sync saga schedule base on sync configuration.",
            LogHelpers.Arguments());

        return logger.ExitMethod(process);
    }

    /// <summary>
    ///     Coordinate next saga step message.
    /// </summary>
    /// <param name="process">System state of sync process to return the next message for.</param>
    /// <param name="logger">The optional logger.</param>
    /// <returns>Return the next saga step message.</returns>
    public static object CoordinateNextStepMessage(Process process, ILogger logger = null)
    {
        logger.EnterMethod();

        try
        {
            Type nextStepType = process.Step.GetSagaMessageType();
            object nextSagaMessage = Activator.CreateInstance(nextStepType, process.Id);

            return logger.ExitMethod(nextSagaMessage);
        }
        catch (Exception e)
        {
            logger?.LogErrorMessage(
                e,
                "An unexpected error occurred while coordinating next step message for system '{id}'.",
                LogHelpers.Arguments(process.System));

            throw;
        }
    }
}
