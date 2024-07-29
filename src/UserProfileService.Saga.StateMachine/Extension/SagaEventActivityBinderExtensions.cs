using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands.Models;
using UserProfileService.StateMachine.Definitions;

namespace UserProfileService.StateMachine.Extension;

/// <summary>
///     Contains extension methods related to a MassTransit <see cref="EventActivityBinder{TSaga}" />.
/// </summary>
internal static class SagaEventActivityBinderExtensions
{
    internal static EventActivityBinder<CommandProcessState> FinalizeSagaFromState(
        this EventActivityBinder<CommandProcessState> binder,
        State? fromState,
        ILogger logger)
    {
        return binder
            .Then(
                c =>
                {
                    logger.LogDebug(
                        "Finalized saga [fromState: {FromState}; command = {Command}; correlation id = {correlationId}]",
                        fromState?.Name,
                        c.Saga.Command,
                        c.CorrelationId);
                })
            .Finalize();
    }

    internal static EventActivityBinder<CommandProcessState, TCommand> Transition<TCommand>(
        this EventActivityBinder<CommandProcessState, TCommand> binder,
        State? fromState,
        State? toState,
        ILogger logger) where TCommand : class
    {
        return binder
            .Then(
                c =>
                {
                    logger.LogDebug(
                        "Transition in saga {FromState} -> {ToState} [command = {Command}; correlation id = {correlationId}]",
                        fromState?.Name,
                        toState?.Name,
                        c.Saga.Command,
                        c.CorrelationId);
                })
            .TransitionTo(toState);
    }

    internal static EventActivityBinder<CommandProcessState> Transition(
        this EventActivityBinder<CommandProcessState> binder,
        State? fromState,
        State? toState,
        ILogger logger)
    {
        return binder
            .Then(
                c =>
                {
                    logger.LogDebug(
                        "Transition in saga {FromState} -> {ToState} [command = {Command}; correlation id = {correlationId}]",
                        fromState?.Name,
                        toState?.Name,
                        c.Saga.Command,
                        c.CorrelationId);
                })
            .TransitionTo(toState);
    }

    /// <summary>
    ///     Adds the default error handling to the activity list of the current behaviour.
    /// </summary>
    /// <remarks>
    ///     Catches an <see cref="Exception" />, sets the <see cref="Exception" /> property of
    ///     <see cref="CommandProcessState" /> to the caught exception and sets the state to
    ///     <paramref name="errorState" />.
    /// </remarks>
    public static EventActivityBinder<CommandProcessState> CatchExceptionAndGoTo(
        this EventActivityBinder<CommandProcessState> source,
        State? errorState)
    {
        return source.Catch<Exception>(
            callback =>
                callback.Then(c => c.Saga.Exception = new ExceptionInformation(c.Exception))
                    .TransitionTo(errorState));
    }

    /// <summary>
    ///     Adds the default error handling to the activity list of the current behaviour.
    /// </summary>
    /// <remarks>
    ///     Catches an <see cref="Exception" />, sets the <see cref="Exception" /> property of
    ///     <see cref="CommandProcessState" /> to the caught exception and sets the state to
    ///     <paramref name="errorState" />.
    /// </remarks>
    public static EventActivityBinder<CommandProcessState, TCommand> CatchExceptionAndGoTo<TCommand>(
        this EventActivityBinder<CommandProcessState, TCommand> source,
        State? errorState) where TCommand : class
    {
        return source.Catch<Exception>(
            callback =>
                callback.Then(c => c.Saga.Exception = new ExceptionInformation(c.Exception))
                    .TransitionTo(errorState));
    }

    /// <summary>
    ///     Wraps a <paramref name="message" /> instance inside a <see cref="SendTuple{T}" /> using a behavior
    ///     <paramref name="context" />.
    /// </summary>
    /// <typeparam name="TCommand">The type of the message/command.</typeparam>
    /// <typeparam name="TSaga">The type of the saga/state.</typeparam>
    /// <param name="message">The message to be wrapped.</param>
    /// <param name="context">The context to be used to wrap the message.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. It contains a <see cref="SendTuple{T}" /> object that wraps
    ///     the provided <paramref name="message" />.
    /// </returns>
    public static Task<SendTuple<TCommand>> AsSendTuple<TCommand, TSaga>(
        this TCommand message,
        BehaviorContext<TSaga> context)
        where TCommand : class
        where TSaga : class, ISaga, SagaStateMachineInstance
    {
        return context.Init<TCommand>(message);
    }
}
