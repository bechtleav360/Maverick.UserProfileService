using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Validation.DependencyInjection;

namespace UserProfileService.Saga.Validation;

/// <summary>
///     Default factory for creating instances of <see cref="ICustomValidationService" />.
/// </summary>
internal class DefaultCustomValidationServiceFactory : ICustomValidationServiceFactory
{
    private readonly ILogger<DefaultCustomValidationServiceFactory> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CustomValidationServiceFactoryOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultCustomValidationServiceFactory" /> class with the specified
    ///     options and logger.
    /// </summary>
    /// <param name="options">The options to configure the behavior of the factory.</param>
    /// <param name="logger">The logger for the factory.</param>
    /// ///
    /// <param name="serviceProvider">The service provider for creating instances.</param>
    public DefaultCustomValidationServiceFactory(
        IServiceProvider serviceProvider,
        CustomValidationServiceFactoryOptions options,
        ILogger<DefaultCustomValidationServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ICustomValidationService CreateCustomValidationService<TMessage>(TMessage message)
    {
        _logger.EnterMethod();

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        Type messageType = message.GetType();

        if (_options.MessageTypeToValidationServiceMap.TryGetValue(messageType, out Type validationServiceType))
        {
            if (typeof(ICustomValidationService).IsAssignableFrom(validationServiceType))
            {
                _logger.LogDebugMessage(
                    "Creating an instance of {validationServiceType} for message type {messageType}",
                    LogHelpers.Arguments(validationServiceType.FullName, messageType.FullName));

                return _logger.ExitMethod(
                    (ICustomValidationService)ActivatorUtilities.CreateInstance(
                        _serviceProvider,
                        validationServiceType));
            }

            throw new InvalidOperationException(
                $"The specified validation service type '{validationServiceType?.FullName}' does not implement ICustomValidationService.");
        }

        _logger.LogWarnMessage("No custom validation service type configured for message type: {messageType.FullName}",
            messageType.FullName.AsArgumentList());

        return _logger.ExitMethod<ICustomValidationService>(null);
    }
}
