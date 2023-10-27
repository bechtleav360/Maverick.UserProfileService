using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for groups started.
/// </summary>
[StateStep(SyncConstants.SagaStep.UserStep)]
public class UserSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="UserSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public UserSyncMessage(Guid id)
    {
        Id = id;
    }
}
