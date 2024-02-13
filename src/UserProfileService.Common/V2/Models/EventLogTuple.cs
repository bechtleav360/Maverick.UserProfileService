using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Common.V2.Enums;

namespace UserProfileService.Common.V2.Models;

/// <summary>
///     Deals as a wrapper for an
///     <see cref="IUserProfileServiceEvent" /> that includes a target
///     stream name and information about the event status.
/// </summary>
public class EventLogTuple : EventTuple
{
    /// <summary>
    ///     Id of the batch to which the event belongs.
    /// </summary>
    public string BatchId { get; set; }

    /// <summary>
    ///     Datetime at which the event was added to the batch.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Id of event log tuple.
    /// </summary>
    public string Id { set; get; }

    /// <summary>
    ///     Status of event.
    /// </summary>
    public EventStatus Status { get; set; }

    /// <summary>
    ///     Gets the event type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Datetime at which the event status was updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="EventLogTuple" />.
    /// </summary>
    public EventLogTuple()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventLogTuple"/> class
    ///     using the values from <paramref name="eventTuple"/> and the <paramref name="batchId"/>.
    /// </summary>
    /// <param name="eventTuple"></param>
    /// <param name="batchId"></param>
    public EventLogTuple(EventTuple eventTuple, string batchId)
    {
        Status = EventStatus.Initialized;
        CreatedAt = DateTime.UtcNow;
        BatchId = batchId ?? eventTuple.Event.MetaData.Batch.Id;
        Id = eventTuple.Event.EventId;
        Event = eventTuple.Event;
        Type = eventTuple.Event.Type;
        TargetStream = eventTuple.TargetStream;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventLogTuple"/> class with the specified
    ///     <paramref name="event"/> and <paramref name="streamName"/>.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="streamName"></param>
    public EventLogTuple(IUserProfileServiceEvent @event, string streamName)
    {
        Event = @event;
        TargetStream = streamName;
    }
}
