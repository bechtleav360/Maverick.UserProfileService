using System;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Interface to describe and specify all messages relevant for sync.
/// </summary>
public interface ISyncMessage
{
    /// <summary>
    ///     Id of sync process.
    /// </summary>
    public Guid Id { get; set; }
}
