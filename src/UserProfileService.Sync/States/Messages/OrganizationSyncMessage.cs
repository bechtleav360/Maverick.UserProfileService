using System;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization steps for organization started.
/// </summary>
[StateStep(SyncConstants.SagaStep.OrgUnitStep)]
public class OrganizationSyncMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="OrganizationSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    public OrganizationSyncMessage(Guid id)
    {
        Id = id;
    }
}
