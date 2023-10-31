using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Utitlities;

namespace UserProfileService.StateMachine.Factories;

/// <summary>
///     Default implementation of <see cref="ICommandServiceFactory" />.
///     The implementation of the <see cref="ICommandService" /> must also implement
///     <see cref="ICommandService{TMessage}" />.
///     In addition, the generic type must define the <see cref="CommandAttribute" /> so that a mapping can be established
///     between the command and the implementation.
/// </summary>
public class CommandServiceFactory : ICommandServiceFactory
{
    private readonly ILogger<CommandServiceFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create an instance of <see cref="CommandServiceFactory" />.
    /// </summary>
    /// <param name="serviceProvider">Provider to retrieve services.</param>
    public CommandServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<CommandServiceFactory>>();
    }

    /// <inheritdoc />
    public ICommandService CreateCommandService(string command)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(command, nameof(command));

        Type serviceType = CommandUtilities.GetCommandServiceType(command, _logger);

        object commandService = ActivatorUtilities.CreateInstance(
            _serviceProvider,
            serviceType);

        _logger.LogDebugMessage(
            "Successful created instance of type {type} for command '{command}'.",
            LogHelpers.Arguments(serviceType.Name, command, nameof(ICommandService)));

        var service = (ICommandService)commandService;

        return _logger.ExitMethod(service);
    }
}
