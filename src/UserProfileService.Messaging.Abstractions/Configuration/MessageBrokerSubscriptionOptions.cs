using System;
using System.Collections.Generic;

namespace UserProfileService.Messaging.Abstractions.Configuration;

/// <summary>
///     Bundles configurable options for message broker subscriptions.
/// </summary>
public class MessageBrokerSubscriptionOptions
{
    /// <summary>
    ///     Specifies the time a queue can last without active subscribers.
    ///     When set to <c>null</c> no time limit will be applied.
    /// </summary>
    public TimeSpan? Expiration { get; set; }

    /// <summary>
    ///     States whether the queue should be able to survive broker restarts.
    /// </summary>
    public bool IsDurable { get; set; } = true;

    /// <summary>
    ///     States whether an exclusive queue should be created on which only one consumer can read at the same time.
    ///     This can be used in oder to broadcast messages to all instances.
    /// </summary>
    public bool IsExclusive { get; set; }

    /// <summary>
    ///     Specifies the max amount of messages which can be in the queue at the same time.
    ///     If to many messages are within the queue, the first message will be dropped.
    ///     When set to <c>null</c> no limit will be applied.
    /// </summary>
    public int? MaxQueueLength { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="MessageBrokerSubscriptionOptions" /> with default values.
    /// </summary>
    public MessageBrokerSubscriptionOptions()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="MessageBrokerSubscriptionOptions" /> and copies all values from
    ///     <paramref name="old" />.
    /// </summary>
    /// <param name="old">A <see cref="MessageBrokerSubscriptionOptions" /> to delete.</param>
    public MessageBrokerSubscriptionOptions(MessageBrokerSubscriptionOptions old)
    {
        IsExclusive = old.IsExclusive;
        MaxQueueLength = old.MaxQueueLength;
        IsDurable = old.IsDurable;
        Expiration = old.Expiration;
    }

    /// <summary>
    ///     Creates a <see cref="IDictionary{TKey,TValue}" /> containing all amqp arguments for the queue creation.
    /// </summary>
    /// <returns>A <see cref="IDictionary{TKey,TValue}" />.</returns>
    public IDictionary<string, object> ToAmqpOptions()
    {
        var options = new Dictionary<string, object>();

        if (MaxQueueLength != null)
        {
            options.Add("x-max-length", MaxQueueLength);
        }

        if (Expiration != null)
        {
            options.Add("x-expires", (int)Expiration.Value.TotalMilliseconds);
        }

        if (IsExclusive)
        {
            options.Add("x-single-active-consumer", IsExclusive);
        }

        return options;
    }
}
