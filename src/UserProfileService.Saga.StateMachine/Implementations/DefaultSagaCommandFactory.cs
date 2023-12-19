using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Commands.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Saga.Common;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.StateMachine.Abstraction;

namespace UserProfileService.StateMachine.Implementations;

/// <summary>
///     Default implementation of the ISagaCommandFactory interface.
///     This class provides methods to create saga commands and determine their associated service types.
/// </summary>
public class DefaultSagaCommandFactory : ISagaCommandFactory
{
    private readonly ILogger<DefaultSagaCommandFactory> _logger;

    /// <summary>
    ///     Collection of command types that this factory can handle.
    /// </summary>
    protected virtual IReadOnlyCollection<Type> CommandTypes { get; }

    /// <summary>
    ///     Collection of command service types that this factory can handle.
    /// </summary>
    protected virtual IReadOnlyCollection<Type> CommandServiceTypes { get; }

    /// <summary>
    ///     Constructs a new instance of the DefaultSagaCommandFactory class.
    /// </summary>
    /// <param name="logger">The logger to use for logging.</param>
    public DefaultSagaCommandFactory(
        ILogger<DefaultSagaCommandFactory> logger)
    {
        _logger = logger;

        CommandTypes = typeof(FunctionCreatedMessage)
                       .Assembly
                       .GetTypes()
                       .Where(
                           t => t.IsDefined(typeof(CommandAttribute))
                               && !string.IsNullOrEmpty(
                                   t.GetCustomAttribute<CommandAttribute>()
                                    ?.Value))
                       .ToArray();

        CommandServiceTypes = GetAllRelevantTypesOfAppDomain()
            .ToArray();
    }

    /// <summary>
    ///     Retrieves all relevant types within the current application domain that implement the ICommandService interface.
    /// </summary>
    /// <param name="assemblies">
    /// (Optional) An IEnumerable of Assembly objects representing the assemblies to search. 
    /// If not provided, the method will search through all assemblies in the current application domain.
    /// </param>
    /// <returns>
    ///     An IEnumerable of Type objects representing the relevant types.
    /// </returns>
    /// <remarks>
    ///     This method searches through all assemblies in the current application domain,
    ///     filtering out types that are not concrete classes implementing the ICommandService interface.
    /// </remarks>
    protected static IEnumerable<Type> GetAllRelevantTypesOfAppDomain(
        IEnumerable<Assembly>? assemblies = null)
    {
        return (assemblies
                   ?? AppDomain
                      .CurrentDomain
                      .GetAssemblies())
               .SelectMany(x => x.GetTypes())
               .Where(
                   t => t is { IsInterface: false, IsAbstract: false, IsClass: true }
                       && typeof(ICommandService).IsAssignableFrom(t));
    }

    /// <inheritdoc />
    public SagaCommand ConstructSagaCommand(string commandName)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(commandName, nameof(commandName));

        List<Type> specificTypes = CommandTypes
                                   .Where(t => t.GetCustomAttribute<CommandAttribute>()?.Value == commandName)
                                   .ToList();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Found the following types for command '{command}', which implements the attribute '{attribute}': {types}",
                LogHelpers.Arguments(
                    commandName,
                    nameof(CommandAttribute),
                    JsonConvert.SerializeObject(
                        specificTypes
                            .Select(t => t.Name))));
        }

        if (specificTypes.Count == 0)
        {
            throw new
                InvalidOperationException(
                    $"Could not find any command defined by the attribute {nameof(CommandAttribute)} and related to {commandName}");
        }

        if (specificTypes.Count > 1)
        {
            throw new InvalidOperationException(
                $"Only one command may be defined with the attribute {nameof(CommandAttribute)} and the value {commandName}.");
        }

        Type exactType = specificTypes.First();

        _logger.LogDebugMessage(
            "Found type '{type}' for command '{command}' with attribute '{attribute}'",
            LogHelpers.Arguments(exactType.Name, commandName, nameof(CommandAttribute)));

        return _logger.ExitMethod(new SagaCommand(commandName, exactType));
    }

    /// <inheritdoc />
    public Type DetermineCommandServiceType(string commandName)
    {
        _logger.EnterMethod();

        Guard.IsNotNullOrEmpty(commandName, nameof(commandName));

        Type genericCommandService = typeof(ICommandService<>);
        SagaCommand command = ConstructSagaCommand(commandName);

        List<Type> types = CommandServiceTypes
                           .Where(
                               x =>
                               {
                                   bool implementsGenericInterface = x.ImplementsGenericInterface(
                                       genericCommandService,
                                       command.ExactType);

                                   return implementsGenericInterface
                                       && x is { IsInterface: false, IsAbstract: false };
                               })
                           .ToList();

        _logger.LogDebugMessage(
            "Found {count} types for command {command}",
            LogHelpers.Arguments(types.Count, command));

        if (types.Count == 0)
        {
            throw new
                ArgumentException(
                    $"Did not find any type for command '{commandName}' with implementation of '{genericCommandService.Name}'.");
        }

        if (types.Count > 1)
        {
            throw new ArgumentException(
                $"Find more than one type for command '{command}' with implementation of '{genericCommandService.Name}'.");
        }

        Type type = types.First();

        _logger.LogDebugMessage(
            "Found one type {type} for command '{command}' with implementation of '{commandService}'.",
            LogHelpers.Arguments(type.Name, command, genericCommandService.Name));

        return _logger.ExitMethod(type);
    }
}
