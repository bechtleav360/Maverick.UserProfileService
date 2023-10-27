using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Projection.VolatileData.DependencyInjection;

internal class VolatileDataStoreConfigBuilder : IVolatileDataStoreConfigBuilder
{
    public List<Type> SupportedTypes { get; } = new List<Type>();

    public IVolatileDataStoreConfigBuilder SupportEvent<TEvent>()
        where TEvent : IUserProfileServiceEvent
    {
        SupportedTypes.Add(typeof(TEvent));

        return this;
    }
}
