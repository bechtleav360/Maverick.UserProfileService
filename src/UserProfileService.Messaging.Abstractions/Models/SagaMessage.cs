using System;

namespace UserProfileService.Messaging.Abstractions.Models;

/// <summary>
///     The abstract saga message for saga
///     communication.
/// </summary>
public abstract class SagaMessage
{
    /// <summary>
    ///     The correlation id that is needed for logging purposes.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    ///     Some user or service that initiates or publish the message.
    /// </summary>
    public SagaInitiator Initiator { get; set; }

    /// <summary>
    ///     The payload the saga message contains.
    /// </summary>
    public object Payload { get; set; }

    /// <summary>
    ///     Identifier assigned to a saga that processes a series of saga messages.
    /// </summary>
    public string SagaId { get; set; }

    /// <summary>
    ///     A ModifiedAt when the message was created.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
///     Saga message that has as payload as a
///     generic type.
/// </summary>
/// <typeparam name="T">The generic type for the saga message.</typeparam>
public class SagaMessage<T> : SagaMessage
{
    public new T Payload { get; set; }
}
