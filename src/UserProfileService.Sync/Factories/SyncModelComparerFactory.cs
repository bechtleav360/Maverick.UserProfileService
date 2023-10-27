using System;
using System.Linq;
using System.Reflection;
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

    private TType CreateInstance<TType>(Type type)
    {
        _logger.EnterMethod();

        TypeInfo typeInfo = type.GetTypeInfo();
        ConstructorInfo constructor = typeInfo.GetConstructors().FirstOrDefault();

        TType instance;

        if (constructor != null)
        {
            _logger.LogDebugMessage(
                "Found constructor for type '{typeInfo.Name}'",
                LogHelpers.Arguments(typeInfo.Name));

            object[] args = constructor
                .GetParameters()
                .Select(o => _serviceProvider.GetRequiredService(o.ParameterType))
                .ToArray();

            instance = (TType)Activator.CreateInstance(typeInfo, args);

            return _logger.ExitMethod(instance);
        }

        _logger.LogDebugMessage(
            "No constructors found for type '{typeInfo.Name}'.",
            LogHelpers.Arguments(typeInfo.Name));

        instance = (TType)Activator.CreateInstance(typeInfo);

        return _logger.ExitMethod(instance);
    }

    /// <inheritdoc />
    public ISyncModelComparer<TSyncModel> CreateComparer<TSyncModel>() where TSyncModel : ISyncModel
    {
        _logger.EnterMethod();

        Type type = typeof(ISyncModelComparer<>);

        Type comparerType = type
            .Assembly
            .GetTypes()
            .FirstOrDefault(
                p =>
                    typeof(ISyncModelComparer<TSyncModel>).IsAssignableFrom(p)
                    && p.ImplementsGenericInterface(type, typeof(TSyncModel)));

        if (comparerType == null)
        {
            _logger.LogWarnMessage(
                "No comparer found for the given sync model type '{type}'",
                LogHelpers.Arguments(type.Name));

            return null;
        }

        var comparer = CreateInstance<ISyncModelComparer<TSyncModel>>(comparerType);

        _logger.LogDebugMessage(
            "Found comparer for the given sync model type '{type}',",
            LogHelpers.Arguments(type.Name));

        return _logger.ExitMethod(comparer);
    }
}
