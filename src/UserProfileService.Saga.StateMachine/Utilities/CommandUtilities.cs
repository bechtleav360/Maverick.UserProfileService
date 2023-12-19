using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Commands.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Saga.Common;
using UserProfileService.Saga.Validation.Utilities;

namespace UserProfileService.StateMachine.Utilities;

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
    public static object? DeserializeCData(SagaCommand command, string data, ILogger? logger = null)
    {
        logger.EnterMethod();

        Guard.IsNotNull(command, nameof(command));
        Guard.IsNotNullOrEmpty(data, nameof(data));

        if (command.ExactType == null)
        {
            throw new
                ArgumentException("SagaCommand must contain an 'ExactType' property that is not null, but has not.",
                                  nameof(command));
        }

        var deserializedData = JsonConvert.DeserializeObject(data, command.ExactType);

        return logger.ExitMethod(deserializedData);
    }

    /// <summary>
    ///     Serialize the given message to command data.
    /// </summary>
    /// <param name="message">Message to serialize.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Serialized message as string.</returns>
    public static string SerializeCData(object message, ILogger? logger = null)
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
}
