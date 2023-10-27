using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
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
    public SyncSourceSystemFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<SyncSourceSystemFactory>();
    }

    private TType CreateInstance<TType>(TypeInfo typeInfo, SourceSystemConfiguration systemConfig)
    {
        _logger.EnterMethod();

        ConstructorInfo constructor = typeInfo.GetConstructors().FirstOrDefault();

        TType instance;

        if (constructor != null)
        {
            _logger.LogDebugMessage(
                "Found constructor for type '{typeName}'.",
                LogHelpers.Arguments(typeInfo.Name));

            object[] args = constructor
                .GetParameters()
                .Select(o => GetService(o, systemConfig))
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

    private object GetService(ParameterInfo parameterInfo, SourceSystemConfiguration systemConfig)
    {
        _logger.EnterMethod();

        Type parameterType = parameterInfo.ParameterType;

        _logger.LogDebugMessage(
            "Try to get service for parameter type '{parameterType.Name}'.",
            LogHelpers.Arguments(parameterType.Name));

        try
        {
            object service = parameterType.IsAssignableFrom(typeof(GeneralSystemConfiguration))
                ? systemConfig.Configuration
                : _serviceProvider.GetRequiredService(parameterInfo.ParameterType);

            return _logger.ExitMethod(service);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                $"Unable to find service for parameter {parameterType.Name}.",
                LogHelpers.Arguments(parameterType.Name));

            throw;
        }
    }

    private SourceSystemConfiguration GetConfiguration<T>(SyncConfiguration configuration, string sourceSystem)
        where T : ISyncModel
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "Get source system configuration for {typeof(T).Name}",
            LogHelpers.Arguments(typeof(T).Name));

        string currentEntityKey = typeof(T).GetCustomAttributeValue<ModelAttribute, string>(t => t.Model);

        if (string.IsNullOrWhiteSpace(currentEntityKey))
        {
            throw new ArgumentNullException($"No entity key found for {typeof(T).Name}'");
        }

        _logger.LogDebugMessage("Found entity key '{currentEntityKey}'.", LogHelpers.Arguments(currentEntityKey));

        if (!configuration.SourceConfiguration.Systems.TryGetValue(
                sourceSystem,
                out SourceSystemConfiguration entitySourceConfigs))
        {
            throw new ConfigurationException(
                $"The source system configuration for the entity '{currentEntityKey}' is missing or incorrectly configured.");
        }

        _logger.LogDebugMessage(
            "Found source system configuration for entity key '{currentEntityKey}'.",
            LogHelpers.Arguments(currentEntityKey));

        return _logger.ExitMethod(entitySourceConfigs);
    }

    /// <inheritdoc />
    public ISynchronizationSourceSystem<T> Create<T>(SyncConfiguration configuration, string sourceSystemName)
        where T : ISyncModel
    {
        _logger.EnterMethod();

        Type type = typeof(ISynchronizationSourceSystem<T>);

        _logger.LogInfoMessage("Create instance of type {typeName}.", LogHelpers.Arguments(type.Name));

        SourceSystemConfiguration systemConfiguration = GetConfiguration<T>(configuration, sourceSystemName);

        Type sourceSystemType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p))
            .FirstOrDefault(
                t =>
                {
                    return string.Equals(
                        t.GetCustomAttributeValue<SystemAttribute, string>(ssa => ssa.System),
                        sourceSystemName,
                        StringComparison.InvariantCultureIgnoreCase);
                });

        var sourceSystem =
            CreateInstance<ISynchronizationSourceSystem<T>>(sourceSystemType?.GetTypeInfo(), systemConfiguration);

        return _logger.ExitMethod(sourceSystem);
    }
}
