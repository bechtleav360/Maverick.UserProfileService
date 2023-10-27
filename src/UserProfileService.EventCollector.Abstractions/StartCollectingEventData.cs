using System;
using UserProfileService.EventCollector.Abstractions.Messages;

namespace UserProfileService.EventCollector.Abstractions;

/// <summary>
///     Data for starting collecting events according to collector.
/// </summary>
public class StartCollectingEventData
{
    /// <summary>
    ///     The id to be used to collect messages and for which a common response is to be sent.
    /// </summary>
    public Guid? CollectingId { get; set; }

    /// <summary>
    ///     The number of items (responses) that should be collected.
    ///     Sometimes this value is not known, in this case the variable is null.
    /// </summary>
    public int? CollectItemsAccount { get; set; }

    /// <summary>
    ///     Time when the event collecting has been completed. It will be null, if the event collecting is not completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    ///     The process id of the sender is the id associated with a specific process (such as a synchronization run).
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    public string ExternalProcessId { get; set; }

    /// <summary>
    ///     Time from which events are collected.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    ///     Defines when a status should be sent out for the already collected responses. Default modulo is 10.
    /// </summary>
    public StatusDispatch StatusDispatch { get; set; } = new StatusDispatch(10);
}
