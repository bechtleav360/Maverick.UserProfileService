using System;
using UserProfileService.Sync.Abstraction.Systems;

namespace UserProfileService.Sync.Abstraction.Annotations;

/// <summary>
///     Attribute used to identify the corresponding system.
///     Example: Mapping the configuration to the correct implementation of <see cref="ISynchronizationSourceSystem{T}" />.
/// </summary>
public class SystemAttribute : Attribute
{
    /// <summary>
    ///     Unique identifier of system.
    /// </summary>
    public string System { get; }

    /// <summary>
    ///     Create an instance of <see cref="SystemAttribute" />.
    /// </summary>
    /// <param name="system">Unique identifier of system.</param>
    public SystemAttribute(string system)
    {
        System = system;
    }
}
