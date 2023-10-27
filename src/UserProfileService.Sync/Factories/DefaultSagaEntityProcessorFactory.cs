using System;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Services;

namespace UserProfileService.Sync.Factories;

internal class DefaultSagaEntityProcessorFactory<TEntity> : ISagaEntityProcessorFactory<TEntity>
    where TEntity : class, ISyncModel
{
    /// <inheritdoc />
    public ISagaEntityProcessor<TEntity> Create(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        SyncConfiguration configuration)
    {
        return new SagaEntityProcessor<TEntity>(serviceProvider, loggerFactory, configuration);
    }
}
