namespace UserProfileService.Sync.Models;

/// <summary>
///     Contains the amount of operations on each supported entity.
/// </summary>
public class Operations
{
    /// <summary>
    ///     Amount of groups that have been synchronized.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public int Groups { get; set; }

    /// <summary>
    ///     Amount of organizations that have been synchronized.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public int Organizations { get; set; }

    /// <summary>
    ///     Amount of roles that have been synchronized.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public int Roles { get; set; }

    /// <summary>
    ///     Amount of users that have been synchronized.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public int Users { get; set; }
}
