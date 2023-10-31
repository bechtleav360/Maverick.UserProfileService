using MassTransit;
using UserProfileService.Commands;
using UserProfileService.StateMachine.StateMachine;
using ValidationResult = UserProfileService.Validation.Abstractions.ValidationResult;

namespace UserProfileService.StateMachine.StateMachineDefinitions;

/// <summary>
///     State context for a command that shall be processed.
/// </summary>
public class CommandProcessState :
    ISagaVersion,
    SagaStateMachineInstance
{
    /// <summary>
    ///     Command of state.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global => Masstransit is using this class modifier won't be change.
    public string Command { get; set; }

    /// <summary>
    ///     Identifier of related command.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global => Masstransit is using this class modifier won't be change.
    public CommandIdentifier CommandIdentifier { get; set; }

    /// <summary>
    ///     Internal correlation id of saga.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    ///     Index of current state defined in method <see cref="CommandProcessStateMachine.DeclareStates" />.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Masstransit is using this class modifier won't be change.
    public int CurrentState { get; set; }

    /// <summary>
    ///     Data for related <see cref="Command" />.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    ///     The id of the corresponding entity of the operation,
    ///     such as the newly generated id when creating a group.
    /// </summary>
    public string EntityId { get; set; }

    /// <summary>
    ///     Exception if error occurred.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Masstransit is using this class modifier won't be change.
    public Exception Exception { get; set; }

    /// <summary>
    ///     Initiator of command.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global => Masstransit is using this class modifier won't be change.
    public CommandInitiator Initiator { get; set; }

    /// <summary>
    ///     indicates whether the command is valid.
    ///     If no validation has been performed yet, the property is null.
    /// </summary>
    public ValidationResult ValidationResult { get; set; }

    /// <summary>
    ///     Version of state context.
    ///     Is incremented each time the state is changed.
    ///     Should correspond to the number of different states within the state machine.
    ///     (Higher on error and retry)
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="CommandProcessState" />.
    /// </summary>
    public CommandProcessState()
    {
    }
}
