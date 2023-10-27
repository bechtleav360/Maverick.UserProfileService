using System;
using System.Collections.Generic;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.EventCollector.Abstractions.Messages.Responses;

/// <summary>
///     The message that is being sent from the event collector agent after all response from saga worker (for a defined
///     process) have been collected.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class CollectingItemsResponse<TSuccess, TFailure> : IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     The number of items (responses) that have been collected by the event collector agent.
    /// </summary>
    public int CurrentAccount => Successes.Count + Failures.Count;

    /// <summary>
    ///     The id of the external process that triggered the event.
    /// </summary>
    public string ExternalProcessId { get; set; }

    /// <summary>
    ///     Collection containing all collected failure responses from saga worker for the current process.
    /// </summary>
    public ICollection<TFailure> Failures { get; set; } = new List<TFailure>();

    /// <summary>
    ///     Collection containing all collected success responses from saga worker for the current process.
    /// </summary>
    public ICollection<TSuccess> Successes { get; set; } = new List<TSuccess>();
}
