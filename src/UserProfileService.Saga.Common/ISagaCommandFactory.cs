namespace UserProfileService.Saga.Common;

/// <summary>
///     Provides functionality to create saga commands and determine their associated service types.
/// </summary>
public interface ISagaCommandFactory
{
    /// <summary>
    ///     Creates a new instance of a saga command based on the provided command name.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <returns>A new instance of a saga command.</returns>
    SagaCommand ConstructSagaCommand(string commandName);

    /// <summary>
    ///     Determines the service type associated with a given command.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <returns>The service type associated with the command.</returns>
    Type DetermineCommandServiceType(string commandName);
}