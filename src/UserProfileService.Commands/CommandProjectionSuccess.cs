using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Commands;

/// <summary>
///     Defines a response message indicating that the command was successfully projected.
/// </summary>
[Message(ServiceName = "saga-worker", ServiceGroup = "user-profile")]
public class CommandProjectionSuccess : ICommand
{
    /// <summary>
    ///     Information to identify the command in several systems.
    /// </summary>
    public CommandIdentifier Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="CommandProjectionSuccess" />.
    /// </summary>
    /// <param name="commandId">Id of related command.</param>
    public CommandProjectionSuccess(string commandId)
    {
        Id = new CommandIdentifier(commandId);
    }

    /// <summary>
    ///     Creates an instance of <see cref="CommandProjectionSuccess" />
    /// </summary>
    public CommandProjectionSuccess()
    {
    }
}
