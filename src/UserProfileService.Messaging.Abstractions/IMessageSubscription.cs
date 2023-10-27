using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Messaging.Abstractions.Configuration;

namespace UserProfileService.Messaging.Abstractions;

/// <summary>
///     Subscription to one or more queues, for which <see cref="MessageReceived" /> will be executed.
/// </summary>
/// <typeparam name="T">The type the subscription has.</typeparam>
public interface IMessageSubscription<T> : IMessageSubscription
{
    /// <summary>
    ///     Event that gets triggered once a new Message arrives in one of the pre-registered Queues
    /// </summary>
    Func<SubscriptionEventArgs<T>, string, Task> MessageReceived { get; }
}

/// <summary>
///     Base-interface for message-subscriptions allows for storing and pulling messages.
/// </summary>
public interface IMessageSubscription : IDisposable
{
    /// <summary>
    ///     List of queues for which <see cref="IMessageSubscription{T}.MessageReceived" /> will be called
    /// </summary>
    IReadOnlyList<string> Queues { get; }

    // @TODO: change this to thread-based polling? internal queue or something?!
    /// <summary>
    ///     Start long running task that periodically polls messages for consumption
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for stopping the subscription.</param>
    /// <returns></returns>
    Task StartSubscription(CancellationToken cancellationToken = default);
}
