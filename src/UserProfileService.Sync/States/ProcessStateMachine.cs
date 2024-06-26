﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Commands;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.EventCollector.Abstractions.Messages.Responses;
using UserProfileService.Saga.Events.Contracts;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Messages;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Messages.Responses;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.States.Messages;
using UserProfileService.Sync.Utilities;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace UserProfileService.Sync.States;

/// <summary>
///     State machine for synchronization processes.
/// </summary>
public class ProcessStateMachine :
    MassTransitStateMachine<ProcessState>
{
    private readonly ILogger<ProcessStateMachine> _logger;
    private readonly IMapper _mapper;
    private readonly IProfileService _profileService;
    private readonly IServiceProvider _serviceProvider;
    private readonly SyncConfiguration _syncConfiguration;
    private readonly ISyncProcessSynchronizer _synchronizer;
    private readonly IRelationFactory _relationFactory;

    #region States

    /// <summary>
    ///     State when the process has been triggered
    ///     and the schedule plan has been prepared for the next step.
    /// </summary>
    public State Initialized { get; private set; }

    /// <summary>
    ///     State when the next step initialized.
    /// </summary>
    public State InitializedStep { get; private set; }

    /// <summary>
    ///     State when the current state has been analyzed and processed.
    /// </summary>
    public State Analyzed { get; private set; }

    /// <summary>
    ///     State when all responses arrived.
    /// </summary>
    public State WaitedForResponse { get; private set; }

    /// <summary>
    ///     State when the process has been finalized.
    /// </summary>
    public State Finalized { get; private set; }


    #endregion

    #region Events

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="StartSyncCommand" />.
    /// </summary>
    public Event<StartSyncCommand> StartSyncCommand { get; private set; }

    /// <summary>
    ///     Defines the event to set next system/step, if step finished or new process was started.
    /// </summary>
    public Event<SetNextStepMessage> SetNextStepMessage { get; private set; }

    /// <summary>
    ///     Defines the event used to skip a step
    /// </summary>
    public Event<SkipStepMessage> SkipStepMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of adding relations for a specific system defined in
    ///     <see cref="ProcessState" />.
    /// </summary>
    public Event<AddedRelationSyncMessage> AddedRelationSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to retrieve information about the final collection status.
    /// </summary>
    public Event<CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>> CollectingItemsResponseMessage
    {
        get;
        private set;
    }

    /// <summary>
    ///     Defines the event to retrieve information about the current temporary collection status.
    /// </summary>
    public Event<CollectingItemsStatus> CollectingItemsStatusMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of deleting relations for a specific system defined in
    ///     <see cref="ProcessState" />.
    /// </summary>
    public Event<DeletedRelationSyncMessage> DeletedRelationSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to finalize sync process.
    /// </summary>
    public Event<FinalizeSyncMessage> FinalizeSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of functions for a specific system defined in
    ///     <see cref="ProcessState" />.
    /// </summary>
    public Event<FunctionSyncMessage> FunctionSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of groups for a specific system defined in <see cref="ProcessState" />.
    /// </summary>
    public Event<GroupSyncMessage> GroupSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of organizations for a specific system defined in
    ///     <see cref="ProcessState" />.
    /// </summary>
    public Event<OrganizationSyncMessage> OrganizationSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of roles for a specific system defined in <see cref="ProcessState" />.
    /// </summary>
    public Event<RoleSyncMessage> RoleSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to update process in state.
    ///     Can only be used in a single step, so the sender must remember the number of messages sent to increment the version
    ///     of its own saga independently.
    ///     Alternatively, a version conflict arises.
    /// </summary>
    public Event<UpdateProcessMessage> UpdateProcessMessage { get; private set; }

    /// <summary>
    ///     Defines the event to handle synchronization of users for a specific system defined in <see cref="ProcessState" />.
    /// </summary>
    public Event<UserSyncMessage> UserSyncMessage { get; private set; }

    /// <summary>
    ///     Defines the event to wait for responses of sync command.
    /// </summary>
    public Event<WaitingForResponseMessage> WaitingForResponseMessage { get; private set; }

    #endregion

    /// <summary>
    ///     Create an instance of <see cref="ProcessStateMachine" />.
    /// </summary>
    /// <param name="syncConfiguration">Current configuration of sync process.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">Provider to retrieve services.</param>
    public ProcessStateMachine(
        IOptionsMonitor<SyncConfiguration> syncConfiguration,
        ILogger<ProcessStateMachine> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _syncConfiguration = syncConfiguration.CurrentValue;
        _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
        _profileService = _serviceProvider.GetRequiredService<IProfileService>();
        _synchronizer = _serviceProvider.GetRequiredService<ISyncProcessSynchronizer>();
        _relationFactory = _serviceProvider.GetRequiredService<IRelationFactory>();

        DeclareStates();
        DeclareEvents();
        DeclareStateMachine();
    }

    /// <summary>
    ///     Defines the state relevant for state machine.
    /// </summary>
    private void DeclareStates()
    {
        _logger.EnterMethod();

        // all states must be registered here
        InstanceState(
            x => x.CurrentState,
            Initialized,
            InitializedStep,
            Analyzed,
            WaitedForResponse,
            Finalized);

        _logger.ExitMethod();
    }

    /// <summary>
    ///     Defines the events relevant for state machine.
    /// </summary>
    private void DeclareEvents()
    {
        _logger.EnterMethod();

        Event(
            () => StartSyncCommand,
            e =>
            {
                // id of the sync process.
                // Is included in the start message to return an id in the controller.
                e.CorrelateById(c => c.Message.CorrelationId ?? Guid.NewGuid());

                e.InsertOnInitial = true;

                e.SetSagaFactory(CreateProcessState);
            });

        Event(() => SetNextStepMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => SkipStepMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => WaitingForResponseMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => FinalizeSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => UpdateProcessMessage, e => e.CorrelateById(c => c.Message.Id));

        Event(
            () => CollectingItemsStatusMessage,
            e => e.CorrelateById(
                c => Guid.TryParse(c.Message.ExternalProcessId, out Guid correlationId)
                    ? correlationId
                    : Guid.NewGuid()));

        Event(
            () => CollectingItemsResponseMessage,
            e => e.CorrelateById(
                c => Guid.TryParse(c.Message.ExternalProcessId, out Guid correlationId)
                    ? correlationId
                    : Guid.NewGuid()));

        Event(() => UserSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => GroupSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => RoleSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => OrganizationSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => FunctionSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => AddedRelationSyncMessage, e => e.CorrelateById(c => c.Message.Id));
        Event(() => DeletedRelationSyncMessage, e => e.CorrelateById(c => c.Message.Id));

        _logger.ExitMethod();
    }

    private ProcessState CreateProcessState(ConsumeContext<StartSyncCommand> context)
    {
        _logger.EnterMethod();

        var initiator = new ActionInitiator
        {
            Id = context.Message.InitiatorId,
            DisplayName = "System",
            Name = "System"
        };

        Guid correlationId = context.Message.CorrelationId ?? context.CorrelationId ?? Guid.NewGuid();

        if (!string.IsNullOrWhiteSpace(initiator.Id))
        {
            try
            {
                _logger.LogInfoMessage(
                    "Try to get user with id {userId} for saga with id {sagaId}",
                    LogHelpers.Arguments(initiator.Id, context.CorrelationId));

                UserSync user = _profileService.GetProfileAsync<UserSync>(initiator.Id, context.CancellationToken)
                    .GetAwaiter()
                    .GetResult();

                initiator.Name = user.Name;
                initiator.DisplayName = user.DisplayName;

                _logger.LogInfoMessage(
                    "Got user with id {userId} and name {name} for saga with id {sagaId}",
                    LogHelpers.Arguments(initiator.Id, user.Name, context.CorrelationId));
            }
            catch (Exception)
            {
                _logger.LogInfoMessage(
                    "An error occurred while getting user for id {id}. Possibly the user is not known, correlation Id: {correlationId}.",
                    LogHelpers.Arguments(initiator.Id, correlationId));

                initiator.Name ??= "Unknown";
                initiator.DisplayName ??= "Unknown";
            }
        }
        else
        {
            _logger.LogInfoMessage(
                "User id for saga with id {sagaId} is null or empty. Initiator will be set to 'System', correlation Id: {correlationId}.",
                LogHelpers.Arguments(initiator.Id, correlationId));
        }

        var processState = new ProcessState
        {
            Process = null,
            CorrelationId = correlationId,
            Initiator = initiator
        };

        return _logger.ExitMethod(processState);
    }

    /// <summary>
    ///     Defines the process of the state machine.
    /// </summary>
    private void DeclareStateMachine()
    {
        _logger.EnterMethod();

        //  start event -> process is initialized and sync plan is built up.
        Initially(
            When(StartSyncCommand)
                .IfElseAsync(
                    async c => await _synchronizer.TryStartSync(c.Message),
                    ifBinder => ifBinder.Publish(
                            c => new StartSyncSuccess
                            {
                                CorrelationId = c.Message.CorrelationId,
                                InitiatorId = c.Message.InitiatorId
                            })
                        .Then(InitializeProcess)
                        .Publish(c => new SetNextStepMessage(c.Saga.CorrelationId))
                        .Publish(GenerateSyncStatusMessage)
                        .LogAndTransitionTo(Initial,Initialized, _logger),
                    elseBinder => elseBinder.Publish(
                            c => new StartSyncFailure
                            {
                                CorrelationId = c.Message.CorrelationId,
                                InitiatorId = c.Message.InitiatorId,
                                ErrorDescription = "Synchronization can not be started, another process is running"
                            })
                        .Then(p => AbortProcess(p, true))
                        .LogAndTransitionTo(Initial,Finalized, _logger)));

        DeclareNextStepMessage(Initialized);
        DeclareNextStepMessage(WaitedForResponse);

        During(
            InitializedStep,
            When(FinalizeSyncMessage)
                .LogEnterState(_logger,InitializedStep,FinalizeSyncMessage)
                .ThenAsync(FinalizeProcess)
                .Publish(GenerateSyncStatusMessage)
                .LogAndTransitionTo(InitializedStep, Finalized, _logger));

        // Entity step for all relevant entities
        DeclareEntityStep<GroupSyncMessage, GroupSync>(GroupSyncMessage);
        DeclareEntityStep<UserSyncMessage, UserSync>(UserSyncMessage);
        DeclareEntityStep<RoleSyncMessage, RoleSync>(RoleSyncMessage);
        DeclareEntityStep<OrganizationSyncMessage, OrganizationSync>(OrganizationSyncMessage);
        DeclareEntityStep<FunctionSyncMessage, FunctionSync>(FunctionSyncMessage);

        // Same as entity step but for adding relations.
        During(
            InitializedStep,
            When(AddedRelationSyncMessage)
                .LogEnterState(_logger, InitializedStep, AddedRelationSyncMessage)
                // For each step that changes the status by UpdateProcessMessage,
                // the version must be initially incremented by 1 to prevent a version conflict at the completion of the step.
                .Then(c => c.Saga.Version++)
                .ThenAsync(ProcessAddedRelationSyncMessage)
                .CatchException(_logger)
                .IfElse(
                    c => c.Saga.Process.CurrentStep.Final.Total != 0 || StepHasFailed(c),
                    ifBinder => ifBinder
                        .Publish(c =>
                            new SetCollectItemsAccountMessage
                            {
                                CollectItemsAccount = StepHasFailed(c)
                                    ? 0
                                    : c.Saga.Process.CurrentStep.Final.Total,
                                CollectingId = c.Saga.Process.CurrentStep.CollectingId
                                    .GetValueOrDefault()
                            })
                        .Publish(c => new TriggerStopWatchCommand
                        {
                            CorrelationId = c.CorrelationId,
                            CollectingId = c.Saga.Process.CurrentStep
                                .CollectingId
                                .GetValueOrDefault()
                        })
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(InitializedStep, Analyzed, _logger),
                    elseBinder => elseBinder
                        .Publish(c => new WaitingForResponseMessage(c.Saga.CorrelationId))
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(InitializedStep, Analyzed, _logger)));

        // Same as entity step but for deleting relations.
        During(
            InitializedStep,
            When(DeletedRelationSyncMessage)
                .LogEnterState(_logger, InitializedStep, DeletedRelationSyncMessage)
                // For each step that changes the status by UpdateProcessMessage,
                // the version must be initially incremented by 1 to prevent a version conflict at the completion of the step.
                .Then(c => c.Saga.Version++)
                .ThenAsync(ProcessDeletedRelationSyncMessage)
                .CatchException(_logger)
                .IfElse(
                    c => c.Saga.Process.CurrentStep.Final.Total != 0 || StepHasFailed(c),
                    ifBinder => ifBinder
                        .Publish(
                            c => new SetCollectItemsAccountMessage
                            {
                                CollectItemsAccount = StepHasFailed(c)
                                    ? 0
                                    : c.Saga.Process.CurrentStep.Final.Total,
                                CollectingId = c.Saga.Process.CurrentStep
                                    .CollectingId
                                    .GetValueOrDefault()
                            })
                        .Publish(c => new TriggerStopWatchCommand
                        {
                            CorrelationId = c.CorrelationId,
                            CollectingId = c.Saga.Process.CurrentStep
                                .CollectingId
                                .GetValueOrDefault()
                        })
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(InitializedStep, Analyzed, _logger),
                    elseBinder => elseBinder
                        .Publish(c => new WaitingForResponseMessage(c.Saga.CorrelationId))
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(InitializedStep, Analyzed, _logger)));

        // update of the process.
        During(
            InitializedStep,
            Ignore(CollectingItemsResponseMessage),
            Ignore(CollectingItemsStatusMessage),
            When(UpdateProcessMessage)
                .LogEnterState(_logger, InitializedStep, UpdateProcessMessage)
                .Then(c => { c.Saga.Process = c.Message.Process; })
                .Publish(GenerateSyncStatusMessage)
                .CatchException(_logger)
                .LogAndTransitionTo(InitializedStep,InitializedStep, _logger));

        // Final response of the event collector for a step.
        During(
            Analyzed,
            Ignore(UpdateProcessMessage),
            When(
                    CollectingItemsResponseMessage,
                    s => s.Message.CollectingId == s.Saga.Process.CurrentStep?.CollectingId
                        && s.Saga.Process.CurrentStep?.FinishedAt == null)
                .LogEnterState(_logger, Analyzed, CollectingItemsResponseMessage)
                .Then(
                    c =>
                    {
                        IncreaseHandledOperationCount(
                            c.Saga.Process,
                            c.Message.Successes,
                            s => s.Command,
                            true);

                        IncreaseHandledOperationCount(
                            c.Saga.Process,
                            c.Message.Failures,
                            s => s.Command,
                            false);

                        // The temporary number is applied to the final number.
                        c.Saga.Process.CurrentStep.Temporary.Handled = c.Saga.Process.CurrentStep.Handled.Total;
                    })
                .Publish(c => new SetNextStepMessage(c.Saga.CorrelationId))
                .Publish(GenerateSyncStatusMessage)
                .CatchException(_logger)
                .LogAndTransitionTo(Analyzed, WaitedForResponse, _logger));

        During(
            Analyzed,
            When(WaitingForResponseMessage)
                 .LogEnterState(_logger, Analyzed, WaitingForResponseMessage)
                .If(
                    c => CollectingCompleted(c) || StepHasFailed(c),
                    ifBinder =>
                        ifBinder
                            .Publish(c => new SetNextStepMessage(c.Saga.CorrelationId))
                            .Publish(GenerateSyncStatusMessage)
                            .CatchException(_logger)
                            .LogAndTransitionTo(Analyzed, WaitedForResponse, _logger)));

        During(
            Analyzed,
            When(
                    SkipStepMessage,
                    s => s.Message.CollectingId.GetValueOrDefault()
                         == s.Saga.Process.CurrentStep?.CollectingId.GetValueOrDefault())
                .Then(c => LogEnterDuring(c, Analyzed, SkipStepMessage))
                .IfElse(
                    c =>
                        c.Saga.Process.CurrentStep.StepLastUpdateIsOlderThan(
                            _syncConfiguration.DelayBeforeTimeoutForStep),
                    ifBinder => ifBinder
                        .Publish(c => new SetNextStepMessage(c.Saga.CorrelationId))
                        .LogAndTransitionTo(Analyzed, WaitedForResponse, _logger),
                    elseBinder => elseBinder
                        .Publish(
                            c => new TriggerStopWatchCommand
                            {
                                CorrelationId = c.CorrelationId,
                                CollectingId = c.Saga.Process
                                    .CurrentStep
                                    .CollectingId
                                    .GetValueOrDefault()
                            })
                        .LogAndTransitionTo(Analyzed, Analyzed, _logger)));

        // update of the meanwhile received responses at the event collector for a step.
        During(
            Analyzed,
            When(
                    CollectingItemsStatusMessage,
                    s => s.Message.CollectingId == s.Saga.Process.CurrentStep?.CollectingId)
                .LogEnterState(_logger, Analyzed, CollectingItemsStatusMessage)
                .Then(
                    c =>
                    {
                        c.Saga.Process.CurrentStep.Temporary.Handled =
                            c.Message.CollectedItemsAccount;

                        c.Saga.Process.CurrentStep.UpdatedAt = DateTime.UtcNow;
                    })
                .Publish(GenerateSyncStatusMessage)
                .CatchException(_logger)
                .LogAndTransitionTo(Analyzed, Analyzed, _logger));

        // The SkipStepMessage is only important for the analyzed step.
        During(InitializedStep,WaitedForResponse,Finalized, Ignore(SkipStepMessage));

        _logger.ExitMethod();
    }

    /// <summary>
    ///     Defines the steps to set new step for different state.
    /// </summary>
    /// <param name="state">Current state.</param>
    private void DeclareNextStepMessage(State state)
    {
        During(
            state,
            When(SetNextStepMessage, c => c.Saga.Process.CurrentStep?.FinishedAt == null)
                .LogEnterState(_logger, state, SetNextStepMessage)
                .Then(SetNextStep)
                .CatchException(_logger)
                .IfElse(
                    WasLastSystem,
                    ifBinder => ifBinder
                        .Publish(c => new FinalizeSyncMessage(c.Saga.CorrelationId))
                        .Publish(GenerateSyncStatusMessage)
                        .LogAndTransitionTo(state,InitializedStep, _logger),
                    elseBinder => elseBinder
                        .Publish(GenerateStartCollectingMessage)
                        .ThenAsync(PublishNextMessage)
                        .CatchException(_logger)
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(state, InitializedStep, _logger)));
    }

    private void DeclareEntityStep<TMessage, TEntity>(Event<TMessage> @event)
        where TMessage : class where TEntity : class, ISyncModel
    {
        _logger.EnterMethod();

        During(
            InitializedStep,
            When(@event)
                .LogEnterState(_logger, InitializedStep, @event)
                // For each step that changes the status by UpdateProcessMessage,
                // the version must be initially incremented by 1 to prevent a version conflict at the completion of the step.
                .Then(c => c.Saga.Version++)
                .ThenAsync(ProcessEntityStep<TMessage, TEntity>)
                .CatchException(_logger)
                .IfElse(
                    c => c.Saga.Process.CurrentStep.Final.Total != 0 || StepHasFailed(c),
                    ifBinder => ifBinder
                        .Publish(
                            c => new SetCollectItemsAccountMessage
                            {
                                CollectItemsAccount = StepHasFailed(c)
                                    ? 0
                                    : c.Saga.Process.CurrentStep.Final.Total,
                                CollectingId = c.Saga.Process.CurrentStep.CollectingId
                                    .GetValueOrDefault()
                            })
                        .Publish(c => new TriggerStopWatchCommand
                        {
                            CorrelationId = c.CorrelationId,
                            CollectingId = c.Saga.Process.CurrentStep
                                .CollectingId
                                .GetValueOrDefault()
                        })
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(InitializedStep,Analyzed, _logger),
                    elseBinder => elseBinder
                        .Publish(
                            c =>
                                new WaitingForResponseMessage(c.Saga.CorrelationId))
                        .Publish(GenerateSyncStatusMessage)
                        .CatchException(_logger)
                        .LogAndTransitionTo(InitializedStep,Analyzed, _logger)));

        _logger.ExitMethod();
    }

    private void InitializeProcess(SagaConsumeContext<ProcessState, StartSyncCommand> context)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Initialize process '{id}' with configuration: {config}",
                LogHelpers.Arguments(context.CorrelationId, _syncConfiguration.ToLogString()));
        }
        else
        {
            _logger.LogInfoMessage(
                "Initialize process '{id}' with configuration,",
                LogHelpers.Arguments(context.CorrelationId));
        }

        context.Saga.Process = SagaSchedule.BuildSagaSchedule(
            context.Saga.CorrelationId,
            _syncConfiguration,
            _logger);

        _logger.ExitMethod();
    }

    private bool StepHasFailed(SagaConsumeContext<ProcessState> context)
    {
        _logger.EnterMethod();

        Process currentProcess = context.Saga.Process;

        return currentProcess.CurrentStep.Status == StepStatus.Failure;
    }

    private bool CollectingCompleted(SagaConsumeContext<ProcessState> context)
    {
        _logger.EnterMethod();

        Process currentProcess = context.Saga.Process;

        if (string.IsNullOrWhiteSpace(currentProcess.System) || string.IsNullOrWhiteSpace(currentProcess.Step))
        {
            _logger.LogInfoMessage(
                "Collecting of saga '{id}' completed, because current system or step are not set.",
                context.CorrelationId.AsArgumentList());

            return _logger.ExitMethod(true);
        }

        bool result = currentProcess.CurrentStep.Final.Total <= currentProcess.CurrentStep.Handled.Total;

        _logger.LogInfoMessage(
            "Collecting of saga '{id}' completed: {result}.",
            LogHelpers.Arguments(context.CorrelationId, result));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Maps the responses to the corresponding number of operations already performed and increments them.
    /// </summary>
    private void IncreaseHandledOperationCount<TMessage>(
        Process process,
        IEnumerable<TMessage> responses,
        Func<TMessage, string> commandAccess,
        bool success)
    {
        _logger.EnterMethod();

        foreach (TMessage successResponse in responses)
        {
            string command = commandAccess.Invoke(successResponse);

            switch (command)
            {
                case CommandConstants.RoleCreate:
                case CommandConstants.GroupCreate:
                case CommandConstants.OrganizationCreate:
                case CommandConstants.FunctionCreate:
                case CommandConstants.UserCreate:
                    IncreaseStepOperationsCount(process.CurrentStep.Handled.Create, success);

                    break;
                case CommandConstants.RoleChange:
                case CommandConstants.ProfileChange:
                    IncreaseStepOperationsCount(process.CurrentStep.Handled.Update, success);

                    break;
                case CommandConstants.RoleDelete:
                case CommandConstants.FunctionDelete:
                case CommandConstants.ProfileDelete:
                    IncreaseStepOperationsCount(process.CurrentStep.Handled.Delete, success);

                    break;
                case CommandConstants.ObjectAssignment
                    when process.Step == SyncConstants.SagaStep.AddedRelationStep:
                    IncreaseStepOperationsCount(process.CurrentStep.Handled.Create, success);

                    break;

                case CommandConstants.ObjectAssignment
                    when process.Step == SyncConstants.SagaStep.DeletedRelationStep:
                    IncreaseStepOperationsCount(process.CurrentStep.Handled.Delete, success);

                    break;
            }
        }

        _logger.ExitMethod();
    }

    private static void IncreaseStepOperationsCount(StepOperationsCount count, bool success)
    {
        if (success)
        {
            count.Success++;
        }
        else
        {
            count.Failure++;
        }
    }

    private void SetNextStep<TMessage>(SagaConsumeContext<ProcessState, TMessage> context)
        where TMessage : class
    {
        _logger.EnterMethod();

        context.Saga.Process.SetProcessStatus(ProcessStatus.InitializeStep);

        SagaSchedule.SetNextSystemStep(context.Saga.Process, _logger);

        _logger.LogInfoMessage(
            "Check if the saga '{id}' has synchronized all systems.",
            context.CorrelationId.AsArgumentList());

        context.Saga.Process.UpdateStepTime();

        _logger.ExitMethod();
    }

    private void LogEnterDuring<TMessage>(
        SagaConsumeContext<ProcessState, TMessage> context,
        State currentState,
        Event<TMessage> configuredEvent)
        where TMessage : class
    {
        _logger.LogInfoMessage(
            "Executing During statement: Entering in state: {state}, consuming message of type: {messageType} with correlation id: {corrId}",
            LogHelpers.Arguments(currentState.Name, configuredEvent.Name, context.CorrelationId));
    }

    private bool WasLastSystem<TMessage>(SagaConsumeContext<ProcessState, TMessage> context) where TMessage : class
    {
        _logger.EnterMethod();

        bool isLastSystem = string.IsNullOrWhiteSpace(context.Saga.Process.Step)
            && string.IsNullOrWhiteSpace(context.Saga.Process.System);

        return _logger.ExitMethod(isLastSystem);
    }

    private async Task FinalizeProcess<TMessage>(SagaConsumeContext<ProcessState, TMessage> context)
        where TMessage : class
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "All systems for the saga '{id}' was synchronized. Finalize saga.",
            context.CorrelationId.AsArgumentList());

        bool allSystemSucceeded = context.Saga.Process.Systems.Values.All(s => s.Status == SystemStatus.Success);

        context.Saga.Process.SetProcessStatus(allSystemSucceeded ? ProcessStatus.Success : ProcessStatus.Failed);

        context.Saga.Process.FinishedAt = DateTime.UtcNow;

        _logger.LogInfoMessage(
            "Synchronization with {id} done, the lock object for the sync will be released.",
            context.Saga.Process.Id.AsArgumentList());

        await _synchronizer.ReleaseLockForRunningProcessAsync();

        _logger.LogInfoMessage(
            "Synchronization with {id} done, a completed status message will be send.",
            context.Saga.Process.Id.AsArgumentList());

        var process = _mapper.Map<ProcessView>(context.Saga);

        await context.Publish(
            new SyncCompleted
            {
                CorrelationId = context.Saga.CorrelationId,
                Initiator = context.Saga.Initiator,
                Process = process
            });

        _logger.ExitMethod();
    }

    private void AbortProcess<TMessage>(
        SagaConsumeContext<ProcessState, TMessage> context,
        bool processHasFailed = false) where TMessage : class
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "The sync process with saga: {id} has been aborted. Finalize saga.",
            context.CorrelationId.AsArgumentList());

        // should not be null, but just to be aware
        if (context.Saga.Process == null)
        {
            context.Saga.Process = AbortedProcessObject(processHasFailed);
        }
        else
        {
            context.Saga.Process.AbortProcess(processHasFailed);
        }

        _logger.ExitMethod();
    }

    
    private static Process AbortedProcessObject(bool processHasFailed)
    {
        return new Process
        {
            Status = processHasFailed ? ProcessStatus.Failed : ProcessStatus.Aborted,
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task PublishNextMessage<TMessage>(SagaConsumeContext<ProcessState, TMessage> context)
        where TMessage : class
    {
        _logger.EnterMethod();

        object nextMessage = SagaSchedule.CoordinateNextStepMessage(context.Saga.Process, _logger);

        _logger.LogInfoMessage(
            "Initialize next saga step for '{id}' with message {type}",
            LogHelpers.Arguments(context.CorrelationId, nextMessage.GetType().Name));

        await context.Publish(nextMessage, nextMessage.GetType(), context.CancellationToken);

        _logger.ExitMethod();
    }

    private StartCollectingMessage GenerateStartCollectingMessage<TMessage>(
        SagaConsumeContext<ProcessState, TMessage> context)
        where TMessage : class
    {
        _logger.EnterMethod();

        var newCollectionId = Guid.NewGuid();

        _logger.LogInfoMessage(
            "Generated for saga '{id}' next collecting id for event collector: {cId}",
            LogHelpers.Arguments(context.CorrelationId, newCollectionId));

        context.Saga.Process.CurrentStep.CollectingId = newCollectionId;

        var message = new StartCollectingMessage
        {
            ExternalProcessId = context.Saga.CorrelationId.ToString(),
            CollectItemsAccount = null,
            CollectingId = newCollectionId,
            Dispatch = _syncConfiguration.SourceConfiguration.Dispatch
        };

        return _logger.ExitMethod(message);
    }

    private SyncStatus GenerateSyncStatusMessage(SagaConsumeContext<ProcessState> context)
    {
        _logger.EnterMethod();

        context.Saga.Process.UpdateProcessTime();

        var processView = _mapper.Map<ProcessView>(context.Saga);

        var syncStatus = new SyncStatus
        {
            Process = processView,
            IsRunning = processView?.FinishedAt == null
        };

        return _logger.ExitMethod(syncStatus);
    }

    private async Task ProcessEntityStep<TMessage, TEntity>(SagaConsumeContext<ProcessState, TMessage> context)
        where TMessage : class
        where TEntity : class, ISyncModel
    {
        _logger.EnterMethod();

        _logger.LogDebugMessage(
            "Process entity step for type {type}.",
            LogHelpers.Arguments(typeof(TEntity)));

        _logger.LogDebugMessage(
            "Try to create entity processor for type {type}.",
            LogHelpers.Arguments(typeof(TEntity)));

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        ISagaEntityProcessor<TEntity> processor = _serviceProvider
            .GetRequiredService<ISagaEntityProcessorFactory<TEntity>>()
            .Create(_serviceProvider, loggerFactory, _syncConfiguration);

        _logger.LogDebugMessage(
            "Handle sync for entity of type {type}.",
            LogHelpers.Arguments(typeof(TEntity)));

        Process process = context.Saga.Process;

        process.UpdateStepTime();

        // Action that should be used to reflect the current state during the treatment of the entities.
        // So it can be stored how many entities have already been analyzed and compared with the system. 
        Task SaveAction(Process p)
        {
            context.Saga.Version++;

            // TODO: Currently there are too many errors, so the code has been commented out for the time being. To check if the logic can be implemented differently.
            // Use IBus to ignore OutboxProcessor of state machine.
            //Task task = _ServiceProvider.GetRequiredService<IBus>()
            //                            .Publish(
            //                                new UpdateProcessMessage(
            //                                    context.Saga.CorrelationId,
            //                                    context.Saga.Version,
            //                                    p));

            return Task.CompletedTask;
        }

        await processor.HandleEntitySync(
            process,
            context.Saga.CorrelationId.ToString(),
            SaveAction,
            context.CancellationToken);

        Step currentStep = process.CurrentStep;

        _logger.LogInfoMessage(
            "Process of step {step}, system {system} and step handled. Current status will be switched from {status} to {nextStatus}.",
            LogHelpers.Arguments(currentStep.Id, process.Step, currentStep.Status, StepStatus.WaitingForResponse));

        process.SetStepStatus(StepStatus.WaitingForResponse);

        _logger.ExitMethod();
    }

    private async Task ProcessAddedRelationSyncMessage(
        SagaConsumeContext<ProcessState, AddedRelationSyncMessage> context)
    {
        _logger.LogInfoMessage("Start sync for add relations.", LogHelpers.Arguments());

        Models.State.System currentSystem = context.Saga.Process.CurrentSystem;

        context.Saga.Process.SetStepStatus(StepStatus.InProgress);

        await CreateRelationStep(context.Saga.Process, SynchronizationOperation.Add);

        _logger.LogInfoMessage(
            "Set sync state for add relations to {state} for system {system}",
            LogHelpers.Arguments(nameof(StepStatus.WaitingForResponse), currentSystem.Id));

        context.Saga.Process.SetStepStatus(StepStatus.WaitingForResponse);
    }

    private async Task ProcessDeletedRelationSyncMessage(
        SagaConsumeContext<ProcessState, DeletedRelationSyncMessage> context)
    {
        _logger.LogInfoMessage("Start sync for delete relations.", LogHelpers.Arguments());

        Models.State.System currentSystem = context.Saga.Process.CurrentSystem;

        context.Saga.Process.SetStepStatus(StepStatus.InProgress);

        await CreateRelationStep(context.Saga.Process, SynchronizationOperation.Delete);

        _logger.LogInfoMessage(
            "Set sync state for delete relations to {state} for system {system}",
            LogHelpers.Arguments(nameof(StepStatus.WaitingForResponse), currentSystem.Id));

        context.Saga.Process.SetStepStatus(StepStatus.WaitingForResponse);
    }

    private async Task CreateRelationStep(Process process, SynchronizationOperation addOrDeleteOperation)
    {
        foreach (string relationEntity in SyncConstants.SagaStep.AllEntitySteps)
        {
            if (process.CurrentSystem.Steps.TryGetValue(relationEntity, out Step syncStep))
            {
                bool relation = syncStep.Operations.HasFlag(addOrDeleteOperation)
                    || syncStep.Operations.HasFlag(SynchronizationOperation.Update);

                IRelationHandler handler = _relationFactory.CreateRelationHandler(
                    process.System,
                    relationEntity);

                ObjectType syncEntity = relationEntity.GetObjectType();

                if (handler == null || syncEntity == ObjectType.Unknown)
                {
                    _logger.LogDebug(
                        "No handler registered for the objet type {objectType}. Continue with next entity type.",
                        LogHelpers.Arguments(syncEntity.ToLogString()));

                    continue;
                }

                // delete relation
                if (addOrDeleteOperation.HasFlag(SynchronizationOperation.Delete))
                {
                    await handler.HandleRelationsAsync(process, false, relation, syncEntity);
                }

                // add relation
                else
                {
                    await handler.HandleRelationsAsync(process, relation, false, syncEntity);
                }
            }
        }
    }
}
