using System;
using System.Threading.Tasks;
using UserProfileService.Messaging.Abstractions.Configuration;
using UserProfileService.Messaging.Abstractions.Models;

namespace UserProfileService.Messaging.Abstractions;

/// <summary>
///     Component that communicates with the configured message broker.
/// </summary>
public interface IMessageBroker : IDisposable
{
    /// <summary>
    ///     Indicates whether the connection is active or not.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    ///     Ensure an Exchange with the name <paramref name="name" /> exists.
    /// </summary>
    /// <param name="name">The name of the exchange that has should be created.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous read operation and that contains the exported profiles.</returns>
    Task CreateExchange(string name);

    /// <summary>
    ///     Ensure a Queue with the name <paramref name="name" /> exists.
    /// </summary>
    /// <param name="name">The name of the queue that should be created.</param>
    /// <param name="options">Specifies additional options whilst creating the queue.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous read operation.</returns>
    Task<string> CreateQueue(string name, MessageBrokerSubscriptionOptions options = null);

    /// <summary>
    ///     Publish a new <see cref="SagaMessage{T}" /> to the given Exchange, with routing key=message.Payload.Type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    Task PublishMessage<T>(SagaMessage<T> message);

    /// <summary>
    ///     Ensure messages with routing key <paramref name="routingKey" /> are routed from <paramref name="exchange" /> to
    ///     <paramref name="queue" />.
    /// </summary>
    /// <param name="exchange">The exchange that the message should routed to.</param>
    /// <param name="queue">The queue that the message should be routed to.</param>
    /// <param name="routingKey">The routing key that will be set from exchange to a  specific queue.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    Task RouteExchangeToQueue(string exchange, string queue, string routingKey);

    /// <summary>
    ///     Create a new binding that ensure one copy of each new message is forwarded to <paramref name="destination" />.
    /// </summary>
    /// <param name="origin">Exchange that receives a new message.</param>
    /// <param name="destination">Exchange that should receive a copy of all messages.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    Task ForwardAllMessagesBetweenExchanges(string origin, string destination);

    /// <summary>
    ///     Provide a handler for new messages from the given queues.
    /// </summary>
    /// <typeparam name="T">Type of subscribed message - wrapped in <see cref="SagaMessage" />.</typeparam>
    /// <param name="action">Action to execute when a new message has been received.</param>
    /// <param name="queues">List of queues from which <typeparamref name="T" /> can be received</param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous read operation and that contains the
    ///     <see cref="IMessageSubscription" /> as return value.
    /// </returns>
    Task<IMessageSubscription<T>> SubscribeToSagaMessage<T>(
        Func<SubscriptionEventArgs<T>, string, Task> action,
        params string[] queues);
}
