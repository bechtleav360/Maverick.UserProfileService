using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Common;

/// <summary>
///     Deals as a wrapper for an <see cref="IUserProfileServiceEvent" /> that includes a target stream name.
/// </summary>
public class EventTuple
{
    /// <summary>
    ///     The event that will be written to <see cref="TargetStream" />.
    /// </summary>
    public IUserProfileServiceEvent Event { get; set; }

    /// <summary>
    ///     The name of the target stream <see cref="Event" /> belongs to.
    /// </summary>
    public string TargetStream { set; get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="EventTuple" />.
    /// </summary>
    /// <param name="targetStream">The stream in which the event belongs to.</param>
    /// <param name="event">The event that should be written to the target stream.</param>
    public EventTuple(string targetStream, IUserProfileServiceEvent @event)
    {
        TargetStream = targetStream;
        Event = @event;
    }

    public EventTuple()
    {
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Event?.GetType().Name}#{TargetStream}";
    }
}
