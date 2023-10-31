
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
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Payloads;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Exceptions;
using UserProfileService.StateMachine.Extension;
using UserProfileService.StateMachine.Utitlities;
using UserProfileService.Validation.Abstractions.Configuration;
using UserProfileService.Validation.Abstractions.Message;
using ValidationResult = UserProfileService.Validation.Abstractions.ValidationResult;

namespace UserProfileService.StateMachine.StateMachineDefinitions;

/// <summary>
///     State machine definition for the default process in Saga Worker
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global => The class is used with reflection.
public class CommandProcessStateMachine : MassTransitStateMachine<CommandProcessState>
{
    private static readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
    private readonly ILogger<CommandProcessStateMachine> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="CommandProjectionFailure" />.
    /// </summary>
    public MassTransit.Event<CommandProjectionFailure> CommandProjectionFailure { get; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="CommandProjectionSuccess" />.
    /// </summary>
    public MassTransit.Event<CommandProjectionSuccess> CommandProjectionSuccess { get; }

    /// <summary>
    ///     State if the command was written to the eventStore. Internal and external validation is successful.
    /// </summary>
    public State Executed { get; }

    /// <summary>
    ///     State if the command has been validated internally.
    ///     The next steps are the external validation or the sending the eventStore event.
    /// </summary>
    public State InternalValidated { get; }

    /// <summary>
    ///     State if an error occurred during the execution of the command.
    /// </summary>
    public State Rejected { get; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="SubmitCommand" />.
    /// </summary>
    public MassTransit.Event<SubmitCommand> SubmitCommand { get; private set; }

    /// <summary>
    ///     State when the command has been triggered
    ///     and the data has been prepared for the next steps.
    /// </summary>
    public State Submitted { get; }

    /// <summary>
    ///     State if the command is successful processed.
    /// </summary>
    public State Success { get; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="ValidateCommand" />.
    /// </summary>
    public MassTransit.Event<ValidateCommand> ValidateCommand { get; private set; }

    /// <summary>
    ///     Defines the event to be handled in the state. See also <see cref="ValidationCompositeResponse" />.
    /// </summary>
    public MassTransit.Event<ValidationCompositeResponse> ValidateCompositeResponse { get; private set; }

    /// <summary>
    ///     Create an instance of <see cref="CommandProcessStateMachine" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">Provider to retrieve services.</param>
    public CommandProcessStateMachine(
        ILogger<CommandProcessStateMachine> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
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
            Submitted,
            InternalValidated,
            Executed,
            Success,
            Rejected);
    }

    /// <summary>
    ///     Defines the events relevant for state machine.
    /// </summary>
    private void DeclareEvents()
    {
        Event(
            () => SubmitCommand,
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
            () => CommandProjectionSuccess,
            e => e.CorrelateById(
                c =>
                    Guid.TryParse(c.Message.Id.Id, out Guid id) ? id : Guid.Empty));

        Event(
            () => CommandProjectionFailure,
            e => e.CorrelateById(c => Guid.TryParse(c.Message.Id.Id, out Guid id) ? id : Guid.Empty)
                .OnMissingInstance(c => c.Discard()));

        // The correlation id of the saga is used to collect the responses.
        // Only one validation triggered per saga can be executed.
        // See also below where the ValidationTriggered is sent.
        Event(
            () => ValidateCompositeResponse,
            e => e.CorrelateById(c => c.Message.CollectingId)
                .OnMissingInstance(c => c.Discard()));
    }

    /// <summary>
    ///     Defines the process of the state machine.
    /// </summary>
    private void DeclareStateMachine()
    {
        Initially(
            When(SubmitCommand)
                .ThenAsync(ModifySubmitCommand)
                .Publish(
                    context => new ValidateCommand
                    {
                        Id = new CommandIdentifier(
                            context.Saga.CorrelationId.ToString(),
                            context.Message.Id.CollectingId)
                    })
                .TransitionTo(Submitted)
                .CatchException(
                    Rejected,
                    GenerateFailureMessage));

        During(
            Submitted,
            When(ValidateCommand)
                .ThenAsync(ValidateSubmitCommand)
                .IfElse(
                    elseActivityCallback =>
                        !IsInvalidValidationResult(elseActivityCallback)
                        && ExternalValidationToTrigger(elseActivityCallback),
                    ifBinder => ifBinder.Publish(
                        // The correlation id of the saga is used to collect the responses.
                        // Only one ValidationTriggered per saga can be executed
                        context => new ValidationTriggered(context.Saga.Data, context.Saga.Command)
                        {
                            CollectingId = context.Saga.CorrelationId
                        }),
                    elseBinder => elseBinder.Publish(
                        c =>
                            new ValidationCompositeResponse(
                                c.Saga.CorrelationId,
                                c.Saga.ValidationResult.IsValid,
                                c.Saga.ValidationResult.Errors)))
                .TransitionTo(InternalValidated)
                .CatchException(
                    Rejected,
                    GenerateFailureMessage));

        During(
            InternalValidated,
            When(ValidateCompositeResponse)
                .IfElse(
                    IsValidValidationResult,
                    ifBinder => ifBinder.ThenAsync(ExecuteCommandEventAsync)
                        .TransitionTo(Executed),
                    elseBinder => elseBinder
                        .Then(ParseMessage)
                        .Publish(
                            c =>
                                new SubmitCommandFailure(
                                    c.Saga.Command,
                                    c.Saga.CommandIdentifier.Id,
                                    c.Saga.CommandIdentifier.CollectingId,
                                    "Validation failed.",
                                    c.Saga.ValidationResult.Errors))
                        .TransitionTo(Rejected)
                        .Finalize())
                .CatchException(
                    Rejected,
                    GenerateFailureMessage));

        During(
            InternalValidated,
            When(ValidateCommand)
                .Publish(
                    c =>
                        new ValidationCompositeResponse(
                            c.Saga.CorrelationId,
                            c.Saga.ValidationResult.IsValid,
                            c.Saga.ValidationResult.Errors)));

        During(
            Executed,
            When(CommandProjectionSuccess)
                .Publish(SubmitCommandSuccess)
                .TransitionTo(Success)
                .Finalize());

        During(
            InternalValidated,
            When(CommandProjectionSuccess)
                .Publish(SubmitCommandSuccess)
                .TransitionTo(Success)
                .Finalize());

        During(
            Executed,
            When(CommandProjectionFailure)
                .Publish(
                    c => new SubmitCommandFailure(
                        c.Saga.Command,
                        c.Saga.CommandIdentifier.Id,
                        c.Saga.CommandIdentifier.CollectingId,
                        c.Message.Message,
                        c.Message.Exception))
                .TransitionTo(Rejected)
                .Finalize());

        During(
            InternalValidated,
            When(CommandProjectionFailure)
                .Publish(
                    c => new SubmitCommandFailure(
                        c.Saga.Command,
                        c.Saga.CommandIdentifier.Id,
                        c.Saga.CommandIdentifier.CollectingId,
                        c.Message.Message,
                        c.Message.Exception))
                .TransitionTo(Rejected)
                .Finalize());

        During(
            Executed,
            When(ValidateCompositeResponse)
                .IfElse(
                    IsValidValidationResult,
                    ifBinder => ifBinder.ThenAsync(ExecuteCommandEventAsync)
                        .TransitionTo(Executed),
                    elseBinder => elseBinder
                        .Then(ParseMessage)
                        .Publish(
                            c =>
                                new SubmitCommandFailure(
                                    c.Saga.Command,
                                    c.Saga.CommandIdentifier.Id,
                                    c.Saga.CommandIdentifier.CollectingId,
                                    "Validation failed.",
                                    c.Saga.ValidationResult.Errors))
                        .TransitionTo(Rejected)
                        .Finalize())
                .CatchException(
                    Rejected,
                    GenerateFailureMessage));

        // To remove instances from repo
        SetCompletedWhenFinalized();
    }

    /// <summary>
    ///     Get the <see cref="SubmitCommandSuccess" /> event, if command successful processed.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    /// <returns></returns>
    private static SubmitCommandSuccess SubmitCommandSuccess(MassTransit.BehaviorContext<CommandProcessState, CommandProjectionSuccess> context)
    {
        return new SubmitCommandSuccess(
            context.Saga.Command,
            context.Saga.CommandIdentifier.Id,
            context.Saga.CommandIdentifier.CollectingId)
        {
            EntityId = context.Saga.EntityId
        };
    }

    /// <summary>
    ///     Returns a message from an event exception
    /// </summary>
    private static SubmitCommandFailure
        GenerateFailureMessage<TCommand>(BehaviorExceptionContext<CommandProcessState, TCommand, Exception> context)
        where TCommand : class
    {
        return new SubmitCommandFailure(
            context.Saga.Command,
            context.Saga.CommandIdentifier.Id,
            context.Saga.CommandIdentifier.CollectingId,
            context.Exception.Message,
            context.Exception);
    }

    /// <summary>
    ///     Checks the context if the validation result is valid.
    /// </summary>
    /// <param name="context">Context to check.</param>
    /// <returns>True if validation result is valid, otherwise false.</returns>
    private static bool IsValidValidationResult(MassTransit.SagaConsumeContext<CommandProcessState, ValidationCompositeResponse> context)
    {
        return context.Message.IsValid;
    }

    /// <summary>
    ///     Checks the context if the validation result is invalid.
    /// </summary>
    /// <param name="context">Context to check.</param>
    /// <returns>True if validation result is invalid, otherwise false.</returns>
    private static bool IsInvalidValidationResult(MassTransit.SagaConsumeContext<CommandProcessState> context)
    {
        if (context.Saga.ValidationResult == null)
        {
            throw new InvalidStateException("The saga state is invalid: Validation result is missing");
        }

        return context.Saga.ValidationResult.IsValid == false;
    }

    /// <summary>
    ///     Parse content of <see cref="ValidationCompositeResponse" /> to state machine context.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    private void ParseMessage(MassTransit.SagaConsumeContext<CommandProcessState, ValidationCompositeResponse> context)
    {
        _logger.LogInfoMessage(
            "Validation composite response for '{message}' with saga id '{id}' is not valid.",
            LogHelpers.Arguments(nameof(ValidateCompositeResponse), context.CorrelationId));

        context.Saga.ValidationResult = context.Message;
    }

    /// <summary>
    ///     Method to modify command data for further process.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    /// <returns>Represents the async operation.</returns>
    private async Task ModifySubmitCommand(MassTransit.SagaConsumeContext<CommandProcessState, SubmitCommand> context)
    {
        _logger.LogInfoMessage(
            "Handle '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(SubmitCommand), context.CorrelationId));

        var commandDataModifierFactory = _serviceProvider.CreateScope()
            .ServiceProvider
            .GetRequiredService<
                ICommandServiceFactory>();

        ICommandService commandService =
            commandDataModifierFactory.CreateCommandService(context.Message.Command);

        if (commandService == null)
        {
            _logger.LogDebugMessage(
                "No modifier found for command '{command}'",
                context.Saga.Command.AsArgumentList());

            return;
        }

        _logger.LogDebugMessage(
            "Found modifier '{modifier}' for command '{command}'",
            LogHelpers.Arguments(commandService.GetType().Name, context.Saga.Command));

        object data = CommandUtilities.DeserializeCData(context.Message.Command, context.Message.Data, _logger);

        object modifiedData = await commandService.ModifyAsync(data, context.CancellationToken);

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

        _logger.LogInfoMessage(
            "Successfully handled '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(SubmitCommand), context.CorrelationId));
    }

    /// <summary>
    ///     Method to validate command data.
    /// </summary>
    /// <param name="context">Current context of state machine.</param>
    /// <returns>Represents the async operation.</returns>
    private async Task ValidateSubmitCommand(MassTransit.SagaConsumeContext<CommandProcessState, ValidateCommand> context)
    {
        _logger.LogInfoMessage(
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

        _logger.LogInfoMessage(
            "Successfully handled '{message}' with saga id '{id}'",
            LogHelpers.Arguments(nameof(ValidateCommand), context.CorrelationId));
    }

    /// <summary>
    ///     Publish event store event for current command.
    /// </summary>
    /// <param name="context">Context of current state machine and command.</param>
    /// <returns>Represents the async operation.</returns>
    /// <exception cref="DependencyResolveException">Will be thrown if resolving the interface of service type failed.</exception>
    private async Task ExecuteCommandEventAsync(MassTransit.SagaConsumeContext<CommandProcessState, ValidationCompositeResponse> context)
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "Handle '{message}' with saga id '{id}'. Next try to create event factory.",
            LogHelpers.Arguments(nameof(ValidateCompositeResponse), context.CorrelationId));

        object data;

        try
        {
            data = CommandUtilities.DeserializeCData(context.Saga.Command, context.Saga.Data, _logger);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "An error occurred while deserializing data for '{message}' with saga id '{id}'.",
                LogHelpers.Arguments(nameof(ValidateCompositeResponse), context.CorrelationId));

            throw;
        }

        using IServiceScope serviceScope = _serviceProvider.CreateScope();

        var commandServiceFactory = serviceScope
            .ServiceProvider
            .GetRequiredService<
                ICommandServiceFactory>();

        _logger.LogDebugMessage(
            "Created event factory and try to create event creator for '{message}' with saga id '{id}'.",
            LogHelpers.Arguments(nameof(ValidateCompositeResponse), context.CorrelationId));

        ICommandService commandService =
            commandServiceFactory.CreateCommandService(context.Saga.Command);

        if (commandService == null)
        {
            _logger.LogErrorMessage(
                null,
                "Found no event creator for '{message}' with saga id '{id}'.",
                LogHelpers.Arguments(nameof(ValidateCompositeResponse), context.CorrelationId));

            throw new DependencyResolveException(
                $"Found no event creator for '{nameof(ValidateCompositeResponse)}' with saga id '{context.CorrelationId}'.",
                typeof(ICommandService));
        }

        IUserProfileServiceEvent eventData = await commandService.CreateAsync(
            data,
            context.CorrelationId.ToString(),
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
                "Successfully handled '{message}' with saga id '{id}'",
                LogHelpers.Arguments(nameof(ValidateCompositeResponse), context.CorrelationId));
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
    private bool ExternalValidationToTrigger(SagaConsumeContext<CommandProcessState> context)
    {
        _logger.EnterMethod();

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

        var validatorFactory = _serviceProvider.CreateScope()
            .ServiceProvider.GetRequiredService<ICommandServiceFactory>();

        ICommandService commandService = validatorFactory.CreateCommandService(context.Saga.Command);

        if (commandService == null)
        {
            _logger.LogInfoMessage(
                "No validator defined for command '{command}'. Success validation response will be returned.",
                context.Saga.Command.AsArgumentList());

            return _logger.ExitMethod(new ValidationResult());
        }

        object deserializedData = CommandUtilities.DeserializeCData(
            context.Saga.Command,
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
