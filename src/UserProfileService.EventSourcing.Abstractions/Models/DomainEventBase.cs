using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Attributes;

namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Base class for all Maverick services as defined in ADR-004.
/// </summary>
/// <typeparam name="TPayload">The payload carried with this event</typeparam>
[EventVersion(2)] // Default version
public abstract class DomainEvent<TPayload> : IDomainEvent
{
    /// <inheritdoc />
    public string? CorrelationId { get; set; }

    /// <inheritdoc />
    public string EventId { get; set; }

    /// <inheritdoc />
    public EventInitiator? Initiator { get; set; }

    /// <inheritdoc />
    public EventMetaData MetaData { get; set; } = new EventMetaData();

    /// <summary>
    ///     The data that will be added to the event.
    /// </summary>
    public TPayload? Payload { set; get; }

    /// <inheritdoc />
    public string? RequestSagaId { get; set; }

    /// <inheritdoc />
    public DateTime Timestamp { get; set; }

    /// <inheritdoc />
    public string Type { set; get; }

    /// <inheritdoc />
    public long VersionInformation { get; set; }

    /// <summary>
    ///     Initializes a new instance of the event.
    /// </summary>
    protected DomainEvent()
    {
        EventId = $"E{Guid.NewGuid():D}";
        Type = GetType().Name;

        // Can not be null, because default attribute of class is set
        VersionInformation = GetType().GetCustomAttribute<EventVersionAttribute>(true)!.VersionInformation;
    }

    /// <summary>
    ///     Initializes a new instance of the event providing the <paramref name="timestamp" /> when the event occurred,
    ///     the identifier of the initializer who introduced the event in the first place and the <paramref name="payload" />.
    /// </summary>
    /// <param name="timestamp">The timestamp when the event occurred.</param>
    /// <param name="payload">The data that will be added to the event.</param>
    protected DomainEvent(DateTime timestamp, TPayload payload) : this()
    {
        Timestamp = timestamp;
        Payload = payload;
        Type = GetType().Name;
    }

    /// <summary>
    ///     Initializes a new instance of the event providing the <paramref name="timestamp" /> when the event occurred,
    ///     the identifier of the initializer who introduced the event in the first place and the <paramref name="payload" />.
    /// </summary>
    /// <param name="timestamp">The timestamp when the event occurred.</param>
    /// <param name="payload">The data that will be added to the event.</param>
    /// <param name="correlationId">The correlation id assigned to all related events.</param>
    protected DomainEvent(DateTime timestamp, TPayload payload, string correlationId) : this()
    {
        Timestamp = timestamp;
        Payload = payload;
        CorrelationId = correlationId;
        Type = GetType().Name;
    }

    /// <summary>
    ///     Checks the event content.
    /// </summary>
    /// <returns>True if the content is null, otherwise false.</returns>
    public virtual bool ValidateContent()
    {
        return Payload != null;
    }

    /// <summary>
    ///     Retrieves the EventType from an implementation of <see cref="DomainEvent{TPayload}" />.
    /// </summary>
    /// <typeparam name="T">Payload of the event.</typeparam>
    /// <returns>The retrieved EventType.</returns>
    public static string GetEventType<T>() where T : DomainEvent<T>
    {
        return typeof(T).Name;
    }

    /// <summary>
    ///     Generates a string representing this instance of an event.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Type;
    }
}
