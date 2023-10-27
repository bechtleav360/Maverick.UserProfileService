using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Commands;

/// <summary>
///     Defines a command for the ups saga worker.
/// </summary>
[Message(ServiceName = "saga-worker", ServiceGroup = "user-profile")]
public class SubmitCommand
{
    /// <summary>
    ///     Command of ups worker.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    ///     Data for related <see cref="Command" />.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    ///     Information to identify the command in several systems.
    /// </summary>
    public CommandIdentifier Id { get; set; }

    /// <summary>
    ///     Initiator of command.
    /// </summary>
    public CommandInitiator Initiator { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommand" />.
    /// </summary>
    public SubmitCommand()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommand" />.
    /// </summary>
    /// <param name="command">Data for related <see cref="Command" />.</param>
    /// <param name="data">Command of ups worker.</param>
    /// <param name="id">The identifier of the command that will be added in the response to associate the response.</param>
    /// <param name="collectingId"> The id to be used to collect messages and for which a common response is to be sent. </param>
    /// <param name="initiator">Initiator of command.</param>
    public SubmitCommand(
        string command,
        string data,
        string id,
        Guid? collectingId = null,
        CommandInitiator initiator = null)
    {
        Id = new CommandIdentifier(id, collectingId ?? Guid.Empty);
        Command = command;
        Data = data;
        Initiator = initiator ?? new CommandInitiator(string.Empty);
    }
}
