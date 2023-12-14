using System;
using System.Collections.Generic;

namespace UserProfileService.Common.V2.DependencyInjection;

/// <summary>
///     Setup instance for event processing in command state machines.
/// </summary>
public class EventProcessingSetup
{
    /// <summary>
    ///     The list of command types that can be processed directly.
    /// </summary>
    public virtual HashSet<string> DirectProcessedCommandTypes { get; } 
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
