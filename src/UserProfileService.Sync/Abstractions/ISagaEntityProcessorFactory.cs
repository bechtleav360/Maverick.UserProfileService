using System;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Services;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     A factory responsible for creating the coherent <see cref="SagaEntityProcessor{TSyncEntity}" />.
/// </summary>
/// <typeparam name="TEntity">The type of entity which to process.</typeparam>
public interface ISagaEntityProcessorFactory<TEntity> where TEntity : class, ISyncModel
{
    /// <summary>
    ///     Creates the <see cref="SagaEntityProcessor{TEntity}" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    /// <param name="loggerFactory">A <see cref="LoggerFactory" /> to use.</param>
    /// <param name="configuration">The <see cref="SyncConfiguration" /> of the current sync process.</param>
    /// <returns></returns>
    ISagaEntityProcessor<TEntity> Create(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        SyncConfiguration configuration);
}
