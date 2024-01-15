using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Systems;

namespace UserProfileService.Sync.Factories;

/// <summary>
///     The implementation of <see cref="ISyncSourceSystemFactory" />.
/// </summary>
public class SyncSourceSystemFactory : ISyncSourceSystemFactory
{
    private readonly ILogger<SyncSourceSystemFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates an instance of <see cref="SyncSourceSystemFactory" />.
    /// </summary>
    /// <param name="serviceProvider">The instance of <see cref="IServiceProvider" /> that is used to get required services.</param>
    /// <param name="logger"></param>
    public SyncSourceSystemFactory(
        IServiceProvider serviceProvider,
        ILogger<SyncSourceSystemFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public ISynchronizationSourceSystem<T> Create<T>(string sourceSystemName) where T : ISyncModel
    {
        _logger.EnterMethod();

        var sourceSystemImplementations = _serviceProvider.GetServices<ISynchronizationSourceSystem<T>>();

        if (!sourceSystemImplementations.Any())
        {
            throw new ConfigurationException(
                $"No registered services for the type {typeof(ISynchronizationSourceSystem<T>).Name} could be found");
        }

        var concreteSourceSystemImplementation = sourceSystemImplementations.FirstOrDefault(
            p => string.Equals(
                p.GetType().GetCustomAttribute<SystemAttribute>()?.System,
                sourceSystemName,
                StringComparison.InvariantCultureIgnoreCase));

        if (concreteSourceSystemImplementation == null)
        {
            throw new ConfigurationException(
                $"The source system implemenation for the system {sourceSystemName} could not be found.");
        }

        return _logger.ExitMethod(concreteSourceSystemImplementation);
    }
}
