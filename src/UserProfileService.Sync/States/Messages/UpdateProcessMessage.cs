using System;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the sync process has to be updated.
/// </summary>
public class UpdateProcessMessage : ISyncMessage
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    ///     Updated process to update in sync process.
    /// </summary>
    public Process Process { get; set; }

    /// <summary>
    ///     The version used in the current saga that wants to update the state.
    ///     Necessary so that no version conflict occurs.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public int Version { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="DeletedRelationSyncMessage" />.
    /// </summary>
    /// <param name="id">Id of sync process.</param>
    /// <param name="version">The version used in the current saga that wants to update the state.</param>
    /// <param name="process">Updated process.</param>
    public UpdateProcessMessage(Guid id, int version, Process process)
    {
        Id = id;
        Version = version;
        Process = process;
    }
}
