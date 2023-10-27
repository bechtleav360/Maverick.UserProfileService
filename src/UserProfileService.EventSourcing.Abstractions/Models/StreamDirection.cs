namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Direction in which the given Stream should be Read
/// </summary>
public enum StreamDirection
{
    /// <summary>
    ///     Read events from oldest to newest
    /// </summary>
    Forwards,

    /// <summary>
    ///     Read events from newest to oldest
    /// </summary>
    Backwards
}
