using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for groups started.
/// </summary>
[StateStep(SyncConstants.SagaStep.GroupStep)]
public class GroupSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="GroupSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public GroupSyncMessage(Guid id)
    {
        Id = id;
    }
}
