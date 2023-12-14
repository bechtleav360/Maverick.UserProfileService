using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Payloads;
using UserProfileService.EventSourcing.Abstractions.Attributes;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Utilities;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Abstract implementation of <see cref="ICommandService" /> to bundle same methods.
/// </summary>
/// <typeparam name="TMessage">
///     Type of message the command service belongs to. <see cref="ICommandService" /> maps the
///     <see cref="object" /> to <typeparamref name="TMessage" />.
/// </typeparam>
public abstract class BaseCommandService<TMessage> : ICommandService<TMessage>
    where TMessage : class, IPayload
{
    private readonly IValidationService _validationService;

    /// <summary>
    ///     The logger that is used to log messages in various severities.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     Create an instance of <see cref="BaseCommandService{TMessage}" />.
    /// </summary>
    /// <param name="validationService">Service to validate <typeparamref name="TMessage" />.</param>
    /// <param name="logger">The logger.</param>
    protected BaseCommandService(IValidationService validationService, ILogger logger)
    {
        _validationService = validationService;
        Logger = logger;
    }

    /// <inheritdoc cref="ICommandService.ModifyAsync"/>
    public virtual Task<TMessage?> ModifyAsync(TMessage? message, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(message);
    }

    /// <inheritdoc />
    public async Task<object?> ModifyAsync(object? message, CancellationToken cancellationToken = default)
    {
        TMessage? modifiedData = await ModifyAsync((TMessage?)message, cancellationToken);

        return modifiedData;
    }

    /// <inheritdoc cref="ICommandService.CreateAsync"/>
    public abstract Task<IUserProfileServiceEvent> CreateAsync(
        TMessage message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public async Task<IUserProfileServiceEvent> CreateAsync(
        object message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        IUserProfileServiceEvent @event = await CreateAsync(
            (TMessage)message,
            correlationId,
            processId,
            initiator,
            cancellationToken);

        return @event;
    }

    /// <inheritdoc cref="ICommandService.ValidateAsync"/>
    public async Task<ValidationResult> ValidateAsync(
        TMessage message,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _validationService.ValidatePayloadAsync(message, initiator.ToInitiator(), cancellationToken);

            return new ValidationResult();
        }
        catch (ValidationException e)
        {
            return new ValidationResult(e.ValidationResults);
        }
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateAsync(
        object? data,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        ValidationResult result = data != null
            ? await ValidateAsync((TMessage)data, initiator, cancellationToken)
            : new ValidationResult(new ValidationAttribute(nameof(data), "data cannot be null"));

        return result;
    }

    private protected TEvent CreateEvent<TEvent, TPayload>(
        TPayload payload,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        Func<TPayload, string>? idSelector = null)
        where TEvent : DomainEvent<TPayload>, new()
        where TPayload : class, IPayload
    {
        Logger.EnterMethod();

        // Version comes from attribute from Domain Event
        long? version = typeof(TEvent).GetCustomAttribute<EventVersionAttribute>()?.VersionInformation;

        var @event = new TEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            RequestSagaId = processId,
            Timestamp = DateTime.UtcNow,
            Initiator = initiator.ToEventInitiator(),
            Payload = payload,
            Type = typeof(TEvent).Name,
            MetaData =
            {
                ProcessId = processId,
                Batch = null,
                CorrelationId = correlationId,
                Initiator = initiator.ToAggregateEventInitiator(),
                RelatedEntityId = idSelector?.Invoke(payload),
                HasToBeInverted = false,
                Timestamp = DateTime.UtcNow,
                VersionInformation = version ?? 2
            }
        };
        return Logger.ExitMethod(@event);
    }
}
