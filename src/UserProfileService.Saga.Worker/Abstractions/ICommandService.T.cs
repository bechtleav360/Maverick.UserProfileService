using UserProfileService.Events.Payloads;

namespace UserProfileService.Saga.Worker.Abstractions;

/// <summary>
///     Defines a service to handle command specific operations for a specific message type like validation or creation of
///     events.
/// </summary>
/// <typeparam name="TMessage">
///     Type of message the command service belongs to. <see cref="ICommandService" /> maps the
///     <see cref="object" /> to <typeparamref name="TMessage" />.
/// </typeparam>
internal interface ICommandService<TMessage> : ICommandService where TMessage : class, IPayload
{
}
