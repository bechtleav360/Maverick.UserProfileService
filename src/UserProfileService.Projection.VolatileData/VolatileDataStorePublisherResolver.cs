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
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<Type> _supportedTypes;

    public VolatileDataStorePublisherResolver(
        IServiceProvider serviceProvider,
        IList<Type> supportedTypes)
    {
        _serviceProvider = serviceProvider;
        _supportedTypes = supportedTypes.ToHashSet();
    }

    public IEventPublisher GetPublisher(IUserProfileServiceEvent eventData)
    {
        if (!_supportedTypes.Contains(eventData.GetType()))
        {
            return null;
        }

        return _serviceProvider.CreateScope()
            .ServiceProvider
            .GetRequiredService<VolatileDataDefaultEventPublisher>();
    }
}
