using System;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization process is to be finalized.
/// </summary>
public class FinalizeSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="FinalizeSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public FinalizeSyncMessage(Guid id)
    {
        Id = id;
    }
}
