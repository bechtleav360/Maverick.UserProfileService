using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for functions started.
/// </summary>
[StateStep(SyncConstants.SagaStep.FunctionStep)]
public class FunctionSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="FunctionSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public FunctionSyncMessage(Guid id)
    {
        Id = id;
    }
}
