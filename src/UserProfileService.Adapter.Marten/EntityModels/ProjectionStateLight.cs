using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Marten.EntityModels;

/// <summary>
///     The Marten entity model of <see cref="ProjectionState" /> with less properties.
/// </summary>
public class ProjectionStateLightDbModel
{
    /// <summary>
    ///     A string that contains the message of the error that occurred during processing the last event.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     The name of the processed event.
    /// </summary>
    public string EventName { set; get; } = string.Empty;

    /// <summary>
    ///     The last event number in the main stream (event table) that was process by the projection.
    /// </summary>
    public ulong EventNumberSequence { set; get; }

    /// <summary>
    ///     The last event number in the stream that was process by the projection.
    /// </summary>
    public ulong EventNumberVersion { set; get; }

    /// <summary>
    ///     When the event was processed.
    /// </summary>
    public DateTimeOffset ProcessedOn { set; get; }

    /// <summary>
    ///     The name of the stream the current event belongs to.
    /// </summary>
    public string StreamName { get; set; } = string.Empty;
}
