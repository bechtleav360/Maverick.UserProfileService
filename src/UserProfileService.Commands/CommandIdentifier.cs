using System;

namespace UserProfileService.Commands;

/// <summary>
///     Contains information to identify the command in several systems.
/// </summary>
public class CommandIdentifier
{
    /// <summary>
    ///     The Id used by the event collector to collect events (of the same collecting process) each other.
    /// </summary>
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     Defines the id that should be used to assign the response to the requested command in external systems
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Creates an instance of <see cref="CommandIdentifier" />.
    /// </summary>
    public CommandIdentifier()
    {
    }

    /// <summary>
    ///     Creates an instance of <see cref="CommandIdentifier" />.
    /// </summary>
    /// <param name="id">Id that should be used to assign the response to the requested command in external systems.</param>
    public CommandIdentifier(string id)
    {
        Id = id;
    }

    /// <summary>
    ///     Creates an instance of <see cref="CommandIdentifier" />.
    /// </summary>
    /// <param name="id">Id that should be used to assign the response to the requested command in external systems.</param>
    /// <param name="collectingId">
    ///     The Id used by the event collector to collect events (of the same collecting process) each
    ///     other
    /// </param>
    public CommandIdentifier(string id, Guid collectingId)
    {
        Id = id;
        CollectingId = collectingId;
    }
}
