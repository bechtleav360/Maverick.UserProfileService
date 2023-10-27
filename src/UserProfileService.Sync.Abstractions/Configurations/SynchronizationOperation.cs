using System;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     Defines possible operations during synchronization that can be done on side of the source.
/// </summary>
[Flags]
public enum SynchronizationOperation
{
    /// <summary>
    ///     Changes will be ignored.
    /// </summary>
    Nothing = 0,

    /// <summary>
    ///     New objects can be added.
    /// </summary>
    Add = 1,

    /// <summary>
    ///     Existing objects can be updated.
    /// </summary>
    Update = 2,

    /// <summary>
    ///     Objects can be deleted.
    /// </summary>
    Delete = 4,

    /// <summary>
    ///     All operations are possible.
    /// </summary>
    All = Add | Update | Delete
}
