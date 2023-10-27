using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for relations started.
/// </summary>
[StateStep(SyncConstants.SagaStep.DeletedRelationStep)]
public class DeletedRelationSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="DeletedRelationSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public DeletedRelationSyncMessage(Guid id)
    {
        Id = id;
    }
}
