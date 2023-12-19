// ReSharper disable UnassignedGetOnlyAutoProperty -> Properties will be used by MassTransit

using MassTransit;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UserProfileService.Commands;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.DependencyInjection;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Payloads;
using UserProfileService.Saga.Common;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Exceptions;
using UserProfileService.StateMachine.Extension;
using UserProfileService.StateMachine.Utilities;
using UserProfileService.Validation.Abstractions.Configuration;
using UserProfileService.Validation.Abstractions.Message;
using ValidationResult = UserProfileService.Validation.Abstractions.ValidationResult;

namespace UserProfileService.StateMachine.Definitions;

/// <summary>
///     State machine definition for the default process in Saga Worker
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global => The class is used with reflection.
public class CommandProcessStateMachine : MassTransitStateMachine<CommandProcessState>
{
    private static readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
    private readonly ILogger _logger;
    private readonly ILogger _stateMachineStatesLogger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="ProjectionFailed" />.
    /// </summary>
    public Event<CommandProjectionFailure>? ProjectionFailed { get; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="ProjectionSucceeded" />.
    /// </summary>
    public Event<CommandProjectionSuccess>? ProjectionSucceeded { get; }

    /// <summary>
    ///     State if the command has been validated internally.
    ///     The next steps are the external validation or the sending the eventStore event.
    /// </summary>
    public State? InternalValidated { get; }

    /// <summary>
    ///     State if an error occurred during the execution of the command.
    /// </summary>
    public State? Rejected { get; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="SubmitCommandReceived" />.
    /// </summary>
    public Event<SubmitCommand>? SubmitCommandReceived { get; private set; }

    /// <summary>
    ///     State when the command has been triggered
    ///     and the data has been prepared for the next steps.
    /// </summary>
    public State? Submitted { get; }

    /// <summary>
    ///     The state when the command has been completely validated (internal or internal AND external depending on the
    ///     configured scenario).
    /// </summary>
    public State? Validated { get; }

    /// <summary>
    ///     The state when a command has been prepared and validated and it's data projection has been started, but not
    ///     finished yet.
    /// </summary>
    public State? ProjectionSubmitted { get; }

    /// <summary>
    ///     The state when the complete process has been faulted by an error during validation or processing. Validation issues
    ///     will be represented by a different state.
    /// </summary>
    public State? Faulted { get; }

    /// <summary>
    ///     The state when the command was valid and all projections has been done. The next step will be creating a response
    ///     for the initiator (either APi or sync).
    /// </summary>
    public State? Projected { get; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="ValidateCommand" />.
    /// </summary>
    public Event<ValidateCommand>? ValidateCommand { get; private set; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="ValidationCompositeResponse" />.
    /// </summary>
    public Event<ValidationCompositeResponse>? ExternalValidationComplete { get; private set; }

    /// <summary>
    ///     Create an instance of <see cref="CommandProcessStateMachine" />.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="serviceProvider">Provider to retrieve services.</param>
    public CommandProcessStateMachine(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        _logger = loggerFactory.CreateLogger($"UserProfileService.{nameof(CommandProcessStateMachine)}");
        _stateMachineStatesLogger = loggerFactory.CreateLogger($"UserProfileService.{nameof(CommandProcessStateMachine)}.States");

        _serviceProvider = serviceProvider;

        DeclareStates();
        DeclareEvents();
        DeclareStateMachine();
    }

    /// <summary>
    ///     Defines the state relevant for state machine.
    /// </summary>
    private void DeclareStates()
    {
        InstanceState(
            x => x.CurrentState,
            Submitted, // 3
            InternalValidated, // 8
            Validated,
            ProjectionSubmitted,
            Rejected,
            Faulted,
            Projected);
    }

    /// <summary>
    ///     Defines the events relevant for state machine.
    /// </summary>
    private void DeclareEvents()
    {
        Event(
            () => SubmitCommandReceived,
            e =>
            {
                e.CorrelateById(_ => Guid.NewGuid());

                e.InsertOnInitial = true;

                e.SetSagaFactory(
                    context => new CommandProcessState
                    {
                        Data = context.Message.Data,
                        Command = context.Message.Command,
                        CorrelationId = context.CorrelationId ?? Guid.NewGuid(),
                        CommandIdentifier = context.Message.Id,
                        Initiator = context.Message.Initiator
                    });
            });

        Event(
            () => ValidateCommand,
            e => e.CorrelateById(c => Guid.TryParse(c.Message.Id.Id, out Guid id) ? id : Guid.Empty)
                .OnMissingInstance(c => c.Discard()));

        Event(
            () => ProjectionSucceeded,
            e => e.CorrelateById(
                c =>
                    Guid.TryParse(c.Message.Id.Id, out Guid id) ? id : Guid.Empty));

        Event(
            () => ProjectionFailed,
            e => e.CorrelateById(c => Guid.TryParse(c.Message.Id.Id, out Guid id) ? id : Guid.Empty)
                .OnMissingInstance(c => c.Discard()));

        // The correlation id of the saga is used to collect the responses.
        // Only one validation triggered per saga can be executed.
        // See also below where the ValidationTriggered is sent.
        Event(
            () => ExternalValidationComplete,
            e => e.CorrelateById(c => c.Message.CollectingId)
                .OnMissingInstance(c => c.Discard()));
    }

    
    /// <summary>
    ///     Defines the process of the state machine.
    /// </summary>                                          
    private void DeclareStateMachine()
    {
        Initially(
            When(SubmitCommandReceived)
                .ThenAsync(ModifySubmitCommand)
                .Transition(Initial, Submitted, _stateMachineStatesLogger)
                .CatchExceptionAndGoTo(Faulted));

        WhenEnter(Submitted,
            f => f
                .ThenAsync(ValidateInternally)
                // first check if command request has been marked as valid (till now, because external check will come next)
                .IfElse(
                    IsValidCommand,
                    then => then
                        .IfElse(
                            IsExternalValidationRequired,
                            // the command has been validated only partially => state = internalValidated
                            externalCheckRequired => externalCheckRequired
                                .Publish(
                                    // The correlation id of the saga is used to collect the responses.
                                    // Only one ValidationTriggered per saga can be executed
                                    context => new ValidationTriggered(context.Saga.Data, context.Saga.Command)
                                    {
                                        CollectingId = context.Saga.CorrelationId
                                    })
                                .Transition(Submitted, InternalValidated, _stateMachineStatesLogger),
                            // command is completely valid because external validation is not required at this point
                            elseWithoutExternalValidation => elseWithoutExternalValidation
                                .Transition(Submitted, Validated, _stateMachineStatesLogger)),
                    elseIfNotValid => elseIfNotValid
                        .Transition(Submitted, Rejected, _stateMachineStatesLogger))
                .CatchExceptionAndGoTo(Faulted));

        // At this point the external validation process has been returned its results
        During(
            InternalValidated,
            When(ExternalValidationComplete)
                .IfElse(IsValidCommand,
                    ifBinder => ifBinder.Transition(InternalValidated, Validated, _stateMachineStatesLogger),
                    elseBinder => elseBinder
                        .Then(ParseValidationFailedMessage)
                        .Transition(InternalValidated, Rejected, _stateMachineStatesLogger))
                .CatchExceptionAndGoTo(Faulted));

        WhenEnter(
            Validated,
            setup => setup
                // if the execution can be done without waiting for a result via bus
                .IfElse(
                    CanBeExecutedDirectly,
                    executeDirectly => executeDirectly
                        .ThenAsync(ExecuteCommandEvent)
                        .Transition(Validated, Projected, _stateMachineStatesLogger),
                        // ... else start the execution pipeline and wait for the result
                    otherwise => otherwise
                        .ThenAsync(ExecuteCommandEvent)
                        .Transition(Validated, ProjectionSubmitted, _stateMachineStatesLogger))
                .CatchExceptionAndGoTo(Faulted));
        
        During(ProjectionSubmitted,
            When(ProjectionSucceeded)
                .Transition(ProjectionSubmitted, Projected, _stateMachineStatesLogger));

        During(ProjectionSubmitted,
            When(ProjectionFailed)
                .Then(ParseFailureMessage)
                .Transition(ProjectionSubmitted, Faulted, _stateMachineStatesLogger));

        WhenEnter(Projected,
            projected => projected
                .PublishAsync(GenerateCommandSuccessMessage)
                .FinalizeSagaFromState(Projected, _stateMachineStatesLogger));

        WhenEnter(Faulted,
            isFaulted => isFaulted
                .PublishAsync(GenerateFailureMessage)
                .FinalizeSagaFromState(Faulted, _stateMachineStatesLogger));

        WhenEnter(Rejected,
            isInvalid => isInvalid
                .PublishAsync(GenerateValidationFailedMessage)
                .FinalizeSagaFromState(Rejected, _stateMachineStatesLogger));

        // To remove instances from repo
        SetCompletedWhenFinalized();
    }

    private bool CanBeExecutedDirectly(BehaviorContext<CommandProcessState> context)
    {
        using IServiceScope serviceScope = _serviceProvider.CreateScope();
        var setup = serviceScope.ServiceProvider.GetRequiredService<EventProcessingSetup>();

        return setup.DirectProcessedCommandTypes.Contains(context.Saga.Command);
    }

    /// <summary>
    ///     Get the <see cref="SubmitCommandSuccess" /> event, if command successful processed.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    /// <returns></returns>
    private static Task<SendTuple<SubmitCommandSuccess>> GenerateCommandSuccessMessage(
        BehaviorContext<CommandProcessState> context)
    {
        return
            new SubmitCommandSuccess(
                context.Saga.Command,
                context.Saga.CommandIdentifier.Id,
                context.Saga.CommandIdentifier.CollectingId) 
            {
                EntityId = context.Saga.EntityId
            }
            .AsSendTuple(context);
    }

    /// <summary>
    ///     Returns a message from an event exception
    /// </summary>
    private static Task<SendTuple<SubmitCommandFailure>> GenerateFailureMessage(
        BehaviorContext<CommandProcessState> context)
    {
        return
            new SubmitCommandFailure(
                    context.Saga.Command,
                    context.Saga.CommandIdentifier.Id,
                    context.Saga.CommandIdentifier.CollectingId,
                    context.Saga.Exception?.Message,
                    context.Saga.Exception)
                .AsSendTuple(context);
    }

    private static Task<SendTuple<SubmitCommandFailure>> GenerateValidationFailedMessage(
        BehaviorContext<CommandProcessState> context)
    {
        return
            new SubmitCommandFailure(
                    context.Saga.Command,
                    context.Saga.CommandIdentifier.Id,
                    context.Saga.CommandIdentifier.CollectingId,
                    "Validation failed.",
                    context.Saga.ValidationResult.Errors)
                .AsSendTuple(context);
    }

    /// <summary>
    ///     Checks the context if the validation result is invalid.
    /// </summary>
    /// <param name="context">Context to check.</param>
    /// <returns>True if validation result is invalid, otherwise false.</returns>
    private static bool IsValidCommand(SagaConsumeContext<CommandProcessState> context)
    {
        if (context.Saga.ValidationResult == null)
        {
            throw new InvalidStateException("The saga state is invalid: Validation result is missing");
        }

        return context.Saga.ValidationResult.IsValid;
    }


    /// <summary>
    ///     Checks the context if the validation result is invalid.
    /// </summary>
    /// <param name="context">Context to check.</param>
    /// <returns>True if validation result is invalid, otherwise false.</returns>
    private static bool IsValidCommand(SagaConsumeContext<CommandProcessState, ValidationCompositeResponse> context)
    {
        if (context.Message == null)
        {
            throw new InvalidStateException("The saga state is invalid: External validation result is missing");
        }

        return context.Message.IsValid;
    }
    /// <summary>
    ///     Parse content of <see cref="ValidationCompositeResponse" /> to state machine context.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    private void ParseValidationFailedMessage(SagaConsumeContext<CommandProcessState, ValidationCompositeResponse> context)
    {
        _logger.LogInfoMessage(
            "Response of external validation for '{message}' with saga id '{id}' is not valid.",
            LogHelpers.Arguments(nameof(ExternalValidationComplete), context.CorrelationId));

        context.Saga.ValidationResult = context.Message;
    }

    /// <summary>
    ///     Parse content of <see cref="ValidationCompositeResponse" /> to state machine context.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    private void ParseFailureMessage(SagaConsumeContext<CommandProcessState, CommandProjectionFailure> context)
    {
        _logger.LogInfoMessage(
            "Projection failed: '{command}' [correlation id = '{correlationId}'] could not be projected: {message}",
            LogHelpers.Arguments(context.Saga.Command, context.CorrelationId, context.Message.Message));

        context.Saga.Exception = context.Message.Exception;
    }

    /// <summary>
    ///     Method to modify command data for further process.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    /// <returns>Represents the async operation.</returns>
    private async Task ModifySubmitCommand(SagaConsumeContext<CommandProcessState, SubmitCommand> context)
    {
        _logger.LogInfoMessage(
            "Handle '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(SubmitCommandReceived), context.CorrelationId));

        IServiceScope serviceScope = _serviceProvider.CreateScope();

        var commandDataModifierFactory = serviceScope
                                         .ServiceProvider
                                         .GetRequiredService<
                                             ICommandServiceFactory>();

        ICommandService commandService =
            commandDataModifierFactory.CreateCommandService(context.Message.Command);

        _logger.LogDebugMessage(
            "Found modifier '{modifier}' for command '{command}'",
            LogHelpers.Arguments(commandService.GetType().Name, context.Saga.Command));

        var commandFactory = serviceScope.ServiceProvider.GetRequiredService<ISagaCommandFactory>();

        object? data = CommandUtilities.DeserializeCData(commandFactory.ConstructSagaCommand(context.Message.Command),
                                                         context.Message.Data, _logger);

        object? modifiedData = await commandService.ModifyAsync(data, context.CancellationToken);

        switch (modifiedData)
        {
            case null:
                _logger.LogInfoMessage(
                    "Modified data for command '{command}'is null. Initial data will be used for process.",
                    context.Saga.Command.AsArgumentList());
                _logger.ExitMethod();

                return;
            case ICreateModelPayload createModelPayload:
                context.Saga.EntityId = createModelPayload.Id;

                break;
        }

        string serializedData = CommandUtilities.SerializeCData(modifiedData, _logger);

        context.Saga.Data = serializedData;

        _logger.LogDebugMessage(
            "Successfully handled '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(SubmitCommandReceived), context.CorrelationId));
    }

    /// <summary>
    ///     Method to validate command data.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    /// <returns>Represents the async operation.</returns>
       private async Task ValidateInternally(BehaviorContext<CommandProcessState> context)
    {
        _logger.LogDebugMessage(
            "Handle '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(ValidateCommand), context.CorrelationId));

        ValidationResult validationResult = await ValidateCommandAsync(context);

        _logger.LogDebugMessage(
            "Validated saga with id '{id}' with result: Valid: {valid}, Results: {results}",
            LogHelpers.Arguments(
                context.CorrelationId,
                validationResult.IsValid,
                validationResult.Errors.Count));

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Validated saga with id '{id}' with result: Result: {result}",
                LogHelpers.Arguments(
                    context.CorrelationId,
                    JsonConvert.SerializeObject(validationResult)));
        }

        context.Saga.ValidationResult = validationResult;

        _logger.LogDebugMessage(
            "Successfully handled '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(ValidateCommand), context.CorrelationId));
    }

    /// <summary>
    ///     Publish event store event for current command.
    /// </summary>
    /// <param name="context">Context of current state machine and command.</param>
    /// <returns>Represents the async operation.</returns>
    /// <exception cref="DependencyResolveException">Will be thrown if resolving the interface of service type failed.</exception>
    private async Task ExecuteCommandEvent(BehaviorContext<CommandProcessState> context)
    {
        _logger.EnterMethod();

        _logger.LogDebugMessage(
            "Handle '{message}' with saga id '{id}'. Next try to create event factory.",
            LogHelpers.Arguments(nameof(ExternalValidationComplete), context.CorrelationId));

        object? data;

        using var serviceScope = _serviceProvider.CreateScope();

        var commandFactory = serviceScope.ServiceProvider.GetRequiredService<ISagaCommandFactory>();

        try
        {
            data = CommandUtilities.DeserializeCData(commandFactory.ConstructSagaCommand(context.Saga.Command),
                                                     context.Saga.Data, _logger);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                                    e,
                                    "An error occurred while deserializing data for '{message}' with saga id '{id}'.",
                                    LogHelpers.Arguments(nameof(ExternalValidationComplete), context.CorrelationId));

            throw;
        }

        if (data == null)
        {
            _logger.LogInfoMessage(
                "Deserialized data for command '{command}' is null. Initial data will be used for process.",
                context.Saga.Command.AsArgumentList());

            _logger.ExitMethod();

            return;
        }

        var commandServiceFactory = serviceScope
                                    .ServiceProvider
                                    .GetRequiredService<
                                        ICommandServiceFactory>();

        _logger.LogDebugMessage(
            "Created event factory and try to create event creator for '{message}' with saga id '{id}'.",
            LogHelpers.Arguments(nameof(ExternalValidationComplete), context.CorrelationId));

        ICommandService commandService =
            commandServiceFactory.CreateCommandService(context.Saga.Command);

        if (commandService == null)
        {
            _logger.LogErrorMessage(
                null,
                "Found no event creator for '{message}' with saga id '{id}'.",
                LogHelpers.Arguments(nameof(ExternalValidationComplete), context.CorrelationId));

            throw new DependencyResolveException(
                $"Found no event creator for '{nameof(ExternalValidationComplete)}' with saga id '{context.CorrelationId}'.",
                typeof(ICommandService));
        }

        IUserProfileServiceEvent eventData = await commandService.CreateAsync(
            data,
            (context.CorrelationId ?? Guid.Empty).ToString(),
            context.Saga.CorrelationId.ToString(),
            context.Saga.Initiator,
            context.CancellationToken);

        IEventPublisher eventPublisher = serviceScope.ServiceProvider
            .GetRequiredService<IEventPublisherFactory>()
            .GetPublisher(eventData);

        await _sync.WaitAsync(context.CancellationToken);

        try
        {
            await eventPublisher.PublishAsync(
                eventData,
                new EventPublisherContext
                {
                    CommandName = context.Saga.Command,
                    CommandId = context.Saga.CommandIdentifier.Id,
                    CollectingId = context.Saga.CommandIdentifier.CollectingId
                },
                context.CancellationToken);

            _logger.LogInfoMessage(
                "Successfully executed '{message}' with saga id '{id}'",
                LogHelpers.Arguments(nameof(ExternalValidationComplete), context.CorrelationId));
        }
        finally
        {
            _sync.Release();
            _logger.ExitMethod();
        }
    }

    /// <summary>
    ///     Check if the external validation should be triggered for the current command.
    /// </summary>
    /// <param name="context">Context of current command state machine.</param>
    /// <returns>True if external validation should be triggered, otherwise false.</returns>
    private bool IsExternalValidationRequired(SagaConsumeContext<CommandProcessState> context)
    {
        _logger.EnterMethod();

        // if the validation failed before, the next steps can be ignored => skipping method
        if (!IsValidCommand(context))
        {
            return false;
        }

        ValidationConfiguration validationConfiguration = _serviceProvider.CreateScope()
            .ServiceProvider
            .GetRequiredService<
                IOptions<ValidationConfiguration>>()
            .Value;

        bool result = context.Saga.ValidationResult.IsValid
            && validationConfiguration.Commands.External.TryGetValue(
                context.Saga.Command,
                out bool commandActivated)
            && commandActivated;

        return _logger.ExitMethod(result);
    }

    private async Task<ValidationResult> ValidateCommandAsync(
        SagaConsumeContext<CommandProcessState> context)
    {
        _logger.EnterMethod();

        var serviceScope = _serviceProvider.CreateScope();
        var validatorFactory = serviceScope
                               .ServiceProvider.GetRequiredService<ICommandServiceFactory>();

        ICommandService commandService = validatorFactory.CreateCommandService(context.Saga.Command);
        var commandFactory = serviceScope.ServiceProvider.GetRequiredService<ISagaCommandFactory>();

        var deserializedData =
            CommandUtilities.DeserializeCData(commandFactory.ConstructSagaCommand(context.Saga.Command),
                                              context.Saga.Data,
                                              _logger);

        ValidationResult result = await commandService.ValidateAsync(
            deserializedData,
            context.Saga.Initiator,
            context.CancellationToken);

        _logger.LogDebugMessage(
            "The internal validation was successful for command '{command}'",
            context.Saga.Command.AsArgumentList());

        return _logger.ExitMethod(result);
    }
}
