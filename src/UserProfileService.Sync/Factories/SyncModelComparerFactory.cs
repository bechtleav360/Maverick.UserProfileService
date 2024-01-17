using System;
using System.Linq;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Factories;

/// <summary>
///     Implementation of <see cref="ISyncModelComparerFactory" />
/// </summary>
public class SyncModelComparerFactory : ISyncModelComparerFactory
{
    private readonly ILogger<SyncModelComparerFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create an instance of <see cref="SyncModelComparerFactory" />
    /// </summary>
    /// <param name="serviceProvider">Provider for retrieving a service object.</param>
    /// <param name="logger">The logger.</param>
    public SyncModelComparerFactory(IServiceProvider serviceProvider, ILogger<SyncModelComparerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public ISyncModelComparer<TSyncModel> CreateComparer<TSyncModel>() where TSyncModel : ISyncModel
    {
        _logger.EnterMethod();

        var comparerImplementation = _serviceProvider.GetService<ISyncModelComparer<TSyncModel>>();

        if (comparerImplementation == null)
        {
            throw new ConfigurationException($"The comparer of type '{nameof(TSyncModel)}' is not registered!");
        }

        _logger.LogInfoMessage(
            "Found comparer for the given sync model type '{type}',",
            LogHelpers.Arguments(nameof(TSyncModel)));

        return _logger.ExitMethod(comparerImplementation);
    }
}
