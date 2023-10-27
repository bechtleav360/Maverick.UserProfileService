using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Events;

/// <summary>
///     Helper class for event related methods.
/// </summary>
public static class EventHandlerHelper
{
    /// <summary>
    ///     Returns all event types that derive from <see cref="IUserProfileServiceEvent" /> in the <see cref="AppDomain" />.
    /// </summary>
    /// <returns>Collection of all derived events.</returns>
    public static ICollection<Type> GetAllEventTypes()
    {
        return AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(IUserProfileServiceEvent).IsAssignableFrom(p))
            .ToList();
    }
}
