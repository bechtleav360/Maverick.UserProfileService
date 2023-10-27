using System;
using UserProfileService.Messaging.Abstractions.Models;

namespace UserProfileService.Messaging.Abstractions.Configuration;

/// <summary>
///     EventArgs containing the newly arrived Message and some Metadata.
/// </summary>
public class SubscriptionEventArgs : EventArgs
{
    /// <summary>
    ///     Message that has been received.
    /// </summary>
    public SagaMessage Message { get; }

    /// <inheritdoc />
    public SubscriptionEventArgs(SagaMessage message)
    {
        Message = message;
    }
}

/// <summary>
///     EventArgs containing the newly arrived Message and some Metadata.
/// </summary>
public class SubscriptionEventArgs<T> : SubscriptionEventArgs
{
    /// <summary>
    ///     Message that has been received
    /// </summary>
    public new SagaMessage<T> Message { get; }

    /// <inheritdoc />
    public SubscriptionEventArgs(SagaMessage<T> message) : base(message)
    {
        Message = message;
    }
}
