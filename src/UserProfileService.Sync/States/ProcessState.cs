using System;
using MassTransit;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.States;

/// <summary>
///     State context for a sync process.
/// </summary>
public class ProcessState :
    ISagaVersion,
    SagaStateMachineInstance
{
    /// <summary>
    ///     Internal correlation id of saga and is equal to sync process id.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    ///     Index of current state defined in method <see cref="ProcessStateMachine.DeclareStates" />.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public int CurrentState { get; set; }

    /// <summary>
    ///     Exception of saga, if one occurs.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public Exception Exception { get; set; }

    /// <summary>
    ///     Identifier of initiator who triggered the process
    /// </summary>
    public ActionInitiator Initiator { get; set; }

    /// <summary>
    ///     Process data of synchronization.
    /// </summary>
    public Process Process { get; set; }

    /// <summary>
    ///     Version of state context.
    ///     Is incremented each time the state is changed.
    ///     Should correspond to the number of different states within the state machine.
    ///     (Higher on error and retry)
    /// </summary>
    public int Version { get; set; }
}
