// Will be used as response in the API
// ReSharper disable UnusedMember.Global
namespace UserProfileService.Proxy.Sync.Models;

/// <summary>
///     Defines a system step during synchronization process. This a view of <see cref="System" />.
/// </summary>
public class SystemView
{
    /// <summary>
    ///     Indicates whether the system was synchronized.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    ///     Steps of system.
    /// </summary>
    public IDictionary<string, StepView> Steps { get; set; } = new Dictionary<string, StepView>();
}
