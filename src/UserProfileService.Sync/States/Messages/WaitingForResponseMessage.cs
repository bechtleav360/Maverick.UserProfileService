using System;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the sync process has to wait for the responses.
/// </summary>
public class WaitingForResponseMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="WaitingForResponseMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public WaitingForResponseMessage(Guid id)
    {
        Id = id;
    }
}
