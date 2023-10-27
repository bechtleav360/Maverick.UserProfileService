namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Contains information about the global position of en event record.
/// </summary>
public struct GlobalPosition
{
    /// <summary>
    ///     The position in the event in the current stream.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    ///     The global position in the event store.
    /// </summary>
    public long SequencePosition { get; set; }
}
