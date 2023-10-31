using UserProfileService.Commands.Attributes;

namespace UserProfileService.StateMachine.Abstraction;

/// <summary>
///     Factory to create command service for a specific command.
///     The implementation of the <see cref="ICommandService" /> must also implement
///     <see cref="ICommandService{TMessage}" />.
///     In addition, the generic type must define the <see cref="CommandAttribute" /> so that a mapping can be established
///     between the command and the implementation.
/// </summary>
public interface ICommandServiceFactory
{
    /// <summary>
    ///     Generate a instance of <see cref="ICommandService" /> for the given <paramref name="command" />.
    ///     See also description of <see cref="ICommandServiceFactory" /> for more information about implementation.
    /// </summary>
    /// <param name="command">Command to generate a service for.</param>
    /// <returns>The service for the given command.</returns>
    ICommandService CreateCommandService(string command);
}
