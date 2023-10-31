using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Commands;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.StateMachine.Abstraction;

/// <summary>
///     Defines a service to handle command specific operations like validation or creation of events.
/// </summary>
public interface ICommandService
{
    /// <summary>
    ///     Create an <see cref="IUserProfileServiceEvent" /> for the given command message.
    /// </summary>
    /// <param name="message">Message to create the event for.</param>
    /// <param name="correlationId">Correlation id of process.</param>
    /// <param name="processId">Process id of command message. Several processes can belong to one correlation id.</param>
    /// <param name="initiator">Initiator of message / command.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The created event.</returns>
    public Task<IUserProfileServiceEvent> CreateAsync(
        object message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Modifies the data of the command for further processing.
    ///     As an example, the id for a newly created entity is created.
    /// </summary>
    /// <param name="message">The command message to modify.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The modified data.</returns>
    public Task<object> ModifyAsync(object message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validates the command data using internal validation algorithm.
    /// </summary>
    /// <param name="data">The command message to validate</param>
    /// <param name="initiator">The initiator of the command.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Validation result of process.</returns>
    public Task<ValidationResult> ValidateAsync(
        object data,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default);
}