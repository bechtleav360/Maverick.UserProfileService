using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.DependencyInjection;

namespace UserProfileService.Projection.VolatileData;

internal class VolatileDataStorePublisherResolver : IEventPublisherTypeResolver
{
    private readonly IServiceProvider _ServiceProvider;
    private readonly HashSet<Type> _SupportedTypes;

    public VolatileDataStorePublisherResolver(
        IServiceProvider serviceProvider,
        IList<Type> supportedTypes)
    {
        _ServiceProvider = serviceProvider;
        _SupportedTypes = supportedTypes.ToHashSet();
    }

    public IEventPublisher? GetPublisher(IUserProfileServiceEvent eventData)
    {
        if (!_SupportedTypes.Contains(eventData.GetType()))
        {
            return null;
        }

        return _ServiceProvider.CreateScope()
            .ServiceProvider
            .GetRequiredService<VolatileDataDefaultEventPublisher>();
    }
}
