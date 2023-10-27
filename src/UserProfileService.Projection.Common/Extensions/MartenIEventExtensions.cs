using System;
using System.Collections.Generic;
using Marten.Events;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.Common.Extensions;

/// <summary>
///     Contains som extensions methods for <see cref="IEvent" />
/// </summary>
public static class MartenIEventExtensions
{
    /// <summary>
    ///     Extracts <see cref="StreamedEventHeader" /> from <see cref="IEvent" />
    /// </summary>
    /// <param name="eventObject">  The marten event object <see cref="IEvent" /></param>
    /// <returns>   Extracted <see cref="StreamedEventHeader" /></returns>
    /// <exception cref="ArgumentException"> Will be thrown when the event object is not set</exception>
    public static StreamedEventHeader ExtractStreamedEventHeader(this IEvent eventObject)
    {
        if (eventObject == null)
        {
            throw new ArgumentException(nameof(eventObject));
        }

        return
            new StreamedEventHeader
            {
                EventNumberVersion = eventObject.Version,
                EventType = eventObject.EventTypeName,
                EventId = eventObject.Id,
                // DateTimeOffset.ToDateTime returns an unspecified dateTime kind
                Created = DateTime.SpecifyKind(eventObject.Timestamp.ToUniversalTime().DateTime, DateTimeKind.Utc),
                EventStreamId = eventObject.StreamKey ?? string.Empty,
                EventNumberSequence = eventObject.Sequence
            };
    }

    /// <summary>
    ///     Method used to identify already processed events.
    /// </summary>
    /// <param name="event">    The event that is being processed</param>
    /// <param name="lastEventNumbers">
    ///     A Map containing as key the stream name and
    ///     as value the number of the last event inside the stream.
    /// </param>
    /// <returns> return true if the event should be handled in the next step, otherwise false.</returns>
    public static bool FilterEvents(this IEvent @event, Dictionary<string, ulong> lastEventNumbers)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (!lastEventNumbers.TryGetValue(
                @event.StreamKey ?? string.Empty,
                out ulong lastEventNumber))
        {
            return true;
        }

        return @event.Version > (long)lastEventNumber;
    }
}
