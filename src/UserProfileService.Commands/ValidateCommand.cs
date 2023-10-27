using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Commands;

/// <summary>
///     Command message to validate data of message.
/// </summary>
[Message]
public class ValidateCommand : ICommand
{
    /// <summary>
    ///     Identifier of command.
    /// </summary>
    public CommandIdentifier Id { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="ValidateCommand" />.
    /// </summary>
    public ValidateCommand()
    {
    }
}
