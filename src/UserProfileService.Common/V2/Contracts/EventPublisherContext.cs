using System;

namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Defines the context of a event publishing context containing useful information about the operation.
/// </summary>
public class EventPublisherContext
{
    /// <summary>
    ///     The id of the batch.
    /// </summary>
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     The id of the command that will be executed.
    /// </summary>
    public string CommandId { get; set; }

    /// <summary>
    ///     The command name.
    /// </summary>
    public string CommandName { get; set; }
}
