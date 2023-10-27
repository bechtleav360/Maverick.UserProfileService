using System.Collections.Generic;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Models.Views;

/// <summary>
///     Defines a system step during synchronization process. This a view of <see cref="System" />.
/// </summary>
public class SystemView
{
    /// <summary>
    ///     Indicates whether the system was synchronized.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public bool IsCompleted { get; set; }

    /// <summary>
    ///     Steps of system.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public IDictionary<string, StepView> Steps { get; set; } = new Dictionary<string, StepView>();
}
