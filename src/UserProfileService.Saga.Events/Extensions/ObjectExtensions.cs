using System;
using System.Reflection;
using Newtonsoft.Json;
using UserProfileService.Commands;
using UserProfileService.Commands.Attributes;

namespace UserProfileService.Saga.Events.Extensions;

/// <summary>
///     Defines extension methods for objects.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    ///     Convert a given object to <see cref="SubmitCommand" /> by using <see cref="CommandAttribute" />.
    /// </summary>
    /// <param name="obj">Object to convert.</param>
    /// <param name="commandId">
    ///     Defines the id that should be used to assign the response to the requested command in external
    ///     systems.
    /// </param>
    /// <param name="collectingId">
    ///     The Id used by the event collector to collect events (of the same collecting process) each other.
    ///     <see cref="CommandIdentifier.CollectingId" />
    /// </param>
    /// <param name="initiator">Initiator of command.</param>
    /// <returns>
    ///     Create <see cref="SubmitCommand" /> of
    ///     <param name="obj"></param>
    ///     .
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Is throws, if
    ///     <param name="obj"></param>
    ///     is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Is thrown if
    ///     <param name="obj"></param>
    ///     has no <see cref="CommandAttribute" />.
    /// </exception>
    public static SubmitCommand ToCommand(
        this object obj,
        string commandId,
        Guid? collectingId = null,
        CommandInitiator initiator = null)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (commandId == null)
        {
            throw new ArgumentNullException(nameof(commandId));
        }

        Type objectType = obj.GetType();
        string command = objectType.GetCustomAttribute<CommandAttribute>(true)?.Value;

        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException(
                $"The given object of type '{objectType.Name}' has no attribute of type '{nameof(CommandAttribute)}'");
        }

        string data = JsonConvert.SerializeObject(obj);

        return new SubmitCommand(command, data, commandId, collectingId, initiator);
    }
}
