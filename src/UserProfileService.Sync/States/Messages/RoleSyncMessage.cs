using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for groups started.
/// </summary>
[StateStep(SyncConstants.SagaStep.RoleStep)]
public class RoleSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="RoleSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public RoleSyncMessage(Guid id)
    {
        Id = id;
    }
}
