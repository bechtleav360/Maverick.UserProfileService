using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Sync.Projection.Handlers;

namespace UserProfileService.Sync.Projection.Abstractions;

/// <summary>
///     Event used to skip events in the <see cref="NoActionEventHandler" />.
/// </summary>
internal class NoActionEvent : IUserProfileServiceEvent
{
    /// <inheritdoc />
    public string EventId { get; set; }

    /// <inheritdoc />
    public EventMetaData MetaData { get; set; }

    /// <inheritdoc />
    public string Type { get; }

    /// <summary>
    ///     Create an instance of <see cref="NoActionEvent" />.
    /// </summary>
    /// <param name="baseEvent">Base event where no action needs to be executed.</param>
    public NoActionEvent(IUserProfileServiceEvent baseEvent)
    {
        Type = baseEvent.Type;
        EventId = baseEvent.EventId;
        MetaData = baseEvent.MetaData;
    }
}
