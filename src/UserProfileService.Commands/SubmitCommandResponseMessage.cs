using System;
using UserProfileService.EventCollector.Abstractions.Messages;

namespace UserProfileService.Commands;

/// <summary>
///     Define the response of a command for the ups saga worker.
/// </summary>
public class SubmitCommandResponseMessage : ICommandResponse, IEventCollectorMessage
{
    /// <summary>
    ///     Id to collect responses and combine them into one entire response.
    /// </summary>
    public Guid CollectingId => Id?.CollectingId ?? Guid.Empty;

    /// <summary>
    ///     Is true if the command was successful, otherwise false
    /// </summary>
    public virtual bool ErrorOccurred { get; set; }

    /// <summary>
    ///     Identifier of the command belonging to this response.
    /// </summary>
    public CommandIdentifier Id { get; set; }
}
