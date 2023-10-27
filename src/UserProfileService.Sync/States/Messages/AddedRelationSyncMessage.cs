using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for relations started.
/// </summary>
[StateStep(SyncConstants.SagaStep.AddedRelationStep)]
public class AddedRelationSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="AddedRelationSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public AddedRelationSyncMessage(Guid id)
    {
        Id = id;
    }
}
