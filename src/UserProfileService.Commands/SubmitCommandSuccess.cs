using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Commands;

/// <summary>
///     Define the success response of a command for the ups saga worker.
/// </summary>
[Message(ServiceName = "saga-worker", ServiceGroup = "user-profile")]
public class SubmitCommandSuccess : SubmitCommandResponseMessage
{
    /// <summary>
    ///     Command of ups worker.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    ///     Optional id of the entity that was processed.
    /// </summary>
    public string EntityId { get; set; }

    /// <inheritdoc />
    public override bool ErrorOccurred { get; set; } = false;

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommandSuccess" />.
    /// </summary>
    public SubmitCommandSuccess()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommand" />.
    /// </summary>
    /// <param name="command">Data for related <see cref="Command" />.</param>
    /// <param name="commandId">The identifier of the command that will be added in the response to associate the response.</param>
    /// <param name="collectingId">Id used to collect messages and for which a common response should be sent.</param>
    public SubmitCommandSuccess(string command, string commandId, Guid collectingId)
    {
        Command = command;
        Id = new CommandIdentifier(commandId, collectingId);
    }
}
