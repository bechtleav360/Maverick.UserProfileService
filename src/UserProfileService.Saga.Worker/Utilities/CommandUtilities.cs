using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Commands.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.Saga.Worker.Abstractions;

namespace UserProfileService.Saga.Worker.Utilities;

/// <summary>
///     Utilities for command related operations.
///     (<see cref="CommandAttribute" />, <see cref="ICommand" />)
/// </summary>
public static class CommandUtilities
{
    /// <summary>
    ///     Deserialize the given string data to the specific command message with attribute <see cref="CommandAttribute" />.
    /// </summary>
    /// <param name="command">The related command for the <paramref name="data" />.</param>
    /// <param name="data">Data to serialize as command message.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="InvalidOperationException">Will be thrown, if multiple command messages found for the given command.</exception>
    public static object DeserializeCData(string command, string data, ILogger logger = null)
    {
        logger?.EnterMethod();

        Guard.IsNotNullOrEmpty(command, nameof(command));
        Guard.IsNotNullOrEmpty(data, nameof(data));

        // TODO: Change assembly using dependency injection - Only the assemblies that are to be used for searching the ICommandServices are to be used. 
        List<Type> types = typeof(FunctionCreatedMessage)
            .Assembly
            .GetTypes()
            .Where(
                t => t.IsDefined(typeof(CommandAttribute))
                    && t.GetCustomAttribute<CommandAttribute>()?.Value == command)
            .ToList();

        if (logger?.IsEnabledForTrace() == true)
        {
            logger.LogTraceMessage(
                "Found the following types for command '{command}', which implements the attribute '{attribute}': {types}",
                LogHelpers.Arguments(
                    command,
                    nameof(CommandAttribute),
                    JsonConvert.SerializeObject(types.Select(t => t.Name))));
        }

        if (types.Count != 1)
        {
            throw new InvalidOperationException(
                $"Only one command may be defined with the attribute {nameof(CommandAttribute)} and the value {command}.");
        }

        Type exactType = GetCommandMessageType(command, logger);

        logger?.LogDebugMessage(
            "Found type '{type}' for command '{command}' with attribute '{attribute}'",
            LogHelpers.Arguments(exactType.Name, command, nameof(CommandAttribute)));

        object deserializedData = JsonConvert.DeserializeObject(data, exactType);

        return logger == null ? deserializedData : logger.ExitMethod(deserializedData);
    }

    /// <summary>
    ///     Serialize the given message to command data.
    /// </summary>
    /// <param name="message">Message to serialize.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Serialized message as string.</returns>
    public static string SerializeCData(object message, ILogger logger = null)
    {
        logger?.EnterMethod();

        Guard.IsNotNull(message, nameof(message));

        string data = JsonConvert.SerializeObject(message);

        if (logger?.IsEnabledForTrace() == true)
        {
            logger.LogTraceMessage("Serialized data for saga as: {data}", data.AsArgumentList());
        }

        return logger == null ? data : logger.ExitMethod<string>(data);
    }

    /// <summary>
    ///     Returns the corresponding service associated with the command.
    ///     For this, the service must use the interface <see cref="ICommandService{TMessage}" /> with the generic parameter of
    ///     TMessage.
    ///     This generic parameter TMessage must define the <see cref="CommandAttribute" /> with the value of the given
    ///     command,
    ///     so that an assignment is possible.
    /// </summary>
    /// <param name="command">Command to retrieve command service for.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The related service type of the given command.</returns>
    /// <exception cref="ArgumentException">Will be thrown, if multiple command services found for the given command.</exception>
    public static Type GetCommandServiceType(string command, ILogger logger = null)
    {
        logger?.EnterMethod();

        Guard.IsNotNullOrEmpty(command, nameof(command));

        Type genericCommandService = typeof(ICommandService<>);
        Type commandMessage = GetCommandMessageType(command, logger);

        // TODO: Change assembly using dependency injection - Only the assemblies that are to be used for searching the ICommandServices are to be used. 
        IEnumerable<Type> types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(
                x =>
                {
                    bool implementsGenericInterface = x.ImplementsGenericInterface(
                        genericCommandService,
                        commandMessage);

                    return implementsGenericInterface
                        && !x.IsInterface
                        && !x.IsAbstract;
                })
            .ToList();

        logger?.LogDebugMessage(
            "Found {count} types for command {command}",
            LogHelpers.Arguments(types.Count(), command));

        if (types.Count() != 1)
        {
            throw new ArgumentException(
                $"Find more or less than one type for command '{command}' with implementation of '{genericCommandService.Name}'.");
        }

        Type type = types.First();

        logger?.LogDebugMessage(
            "Found one type {type} for command '{command}' with implementation of '{commandService}'.",
            LogHelpers.Arguments(type.Name, command, genericCommandService.Name));

        return logger == null ? type : logger.ExitMethod(type);
    }

    /// <summary>
    ///     Returns the corresponding message associated with the command.
    ///     The message must define the <see cref="CommandAttribute" /> with the given command as value,
    ///     so that a unique assignment is possible.
    /// </summary>
    /// <param name="command">The command to retrieve a message type for.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Type of related message type.</returns>
    /// <exception cref="InvalidOperationException">Will be thrown, if multiple command messages found for the given command.</exception>
    private static Type GetCommandMessageType(string command, ILogger logger = null)
    {
        logger?.EnterMethod();

        Guard.IsNotNullOrEmpty(command, nameof(command));

        // TODO: Change assembly using dependency injection - Only the assemblies that are to be used for searching the ICommandServices are to be used. 
        List<Type> types = typeof(FunctionCreatedMessage)
            .Assembly
            .GetTypes()
            .Where(
                t => t.IsDefined(typeof(CommandAttribute))
                    && t.GetCustomAttribute<CommandAttribute>()?.Value
                    == command)
            .ToList();

        logger?.LogInfoMessage(
            "Found {count} types for command '{command}', which implements the attribute '{attribute}'.",
            LogHelpers.Arguments(
                types.Count,
                command,
                nameof(CommandAttribute)));

        if (logger?.IsEnabledForTrace() == true)
        {
            logger.LogTraceMessage(
                "Found the following types for command '{command}', which implements the attribute '{attribute}': {types}",
                LogHelpers.Arguments(
                    command,
                    nameof(CommandAttribute),
                    JsonConvert.SerializeObject(types.Select(t => t.Name))));
        }

        if (types.Count != 1)
        {
            throw new InvalidOperationException(
                $"Only one command may be defined with the attribute {nameof(CommandAttribute)} and the value {command}.");
        }

        Type result = types.First();

        return logger == null ? result : logger.ExitMethod(result);
    }
}
