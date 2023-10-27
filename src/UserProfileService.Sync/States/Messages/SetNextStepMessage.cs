using System;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the next saga step is to be set.
/// </summary>
public class SetNextStepMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="SetNextStepMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public SetNextStepMessage(Guid id)
    {
        Id = id;
    }
}
