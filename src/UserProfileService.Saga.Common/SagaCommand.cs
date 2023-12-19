namespace UserProfileService.Saga.Common;

/// <summary>
///     Represents a command in the saga pattern used in the UserProfileService.
/// </summary>
public class SagaCommand
{
    /// <summary>
    ///     Gets the exact type of the command.
    /// </summary>
    /// <value>
    ///     The exact type of the command.
    /// </value>
    public Type? ExactType { get; } = null;

    /// <summary>
    ///     Gets the name of the command.
    /// </summary>
    /// <value>
    ///     The name of the command.
    /// </value>
    public string CommandName { get; } = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SagaCommand" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor creates a SagaCommand object with default values.
    /// </remarks>
    public SagaCommand()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SagaCommand" /> class with the specified command name and exact type.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="exactType">The exact type of the command.</param>
    /// <remarks>
    ///     This constructor creates a SagaCommand object with the provided command name and exact type.
    /// </remarks>
    public SagaCommand(string commandName,
                       Type exactType)
    {
        CommandName = commandName;
        ExactType = exactType;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return CommandName;
    }
}