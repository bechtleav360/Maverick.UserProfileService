using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Interface for events for all Maverick services as defined in ADR-004.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    ///     A unique identifier to link series of events.
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    ///     An identifier of this event.
    /// </summary>
    string EventId { get; set; }

    /// <summary>
    ///     Some user or service that initiates the event.
    /// </summary>
    EventInitiator? Initiator { get; set; }

    /// <summary>
    ///     The meta data of the current event.
    ///     Actually this property is not used.
    /// </summary>
    EventMetaData MetaData { get; set; }

    /// <summary>
    ///     Saga Id of the saga that is or should start a new event.
    ///     This means that it is the parent saga id that starts an new event
    ///     to assign the response of the event to the related saga.
    /// </summary>
    string? RequestSagaId { get; set; }

    /// <summary>
    ///     The timestamp when the event occurred.
    /// </summary>
    DateTime Timestamp { get; set; }

    /// <summary>
    ///     A string that identifies the type of the event.
    /// </summary>
    string Type { set; get; }

    /// <summary>
    ///     Information about the used version of this event.
    /// </summary>
    long VersionInformation { get; set; }
}
