using System;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.States;
using ExceptionInformation = UserProfileService.Sync.Models.State.ExceptionInformation;

namespace UserProfileService.Sync.Extensions
{
    /// <summary>
    ///     Contains extension methods related to a MassTransit <see cref="EventActivityBinder{TSaga}" />.
    /// </summary>
    internal static class SagaEventActivityBinderExtensions
    {

        /// <summary>
        ///     Adds the default error handling to the activity list of the current behaviour.
        /// </summary>
        /// <remarks>
        ///     Catches an <see cref="Exception" />, sets the <see cref="Exception" /> property of
        ///     <see cref="ProcessState" /> to the caught exception and sets the state to
        /// </remarks>
        public static EventActivityBinder<ProcessState, TCommand> CatchException<TCommand>(
            this EventActivityBinder<ProcessState, TCommand> source,
            ILogger logger = null) where TCommand : class
        {
            return source.Catch<Exception>(
                callback =>
                    callback.Then(
                        c =>
                        {
                            logger?.LogErrorMessage(
                                c.Exception,
                                "Exception happened in step {step} by synchronizing system {system}",
                                LogHelpers.Arguments(c.Saga.Process.Step, c.Saga.Process.System));

                            c.Saga.Exceptions.Add(
                                new ExceptionInformation
                                {
                                    ErrorMessage = c.Exception.Message,
                                    Step = c.Saga.Process.Step,
                                    System = c.Saga.Process.System,
                                    ExceptionType = c.Exception.GetType().Name,
                                    HResult = c.Exception.HResult,
                                    HelpLink = c.Exception.HelpLink,
                                    Source = c.Exception.Source,
                                    StackTrace = c.Exception.StackTrace
                                });

                            c.Saga.Process.CurrentStep.Status = StepStatus.Failure;
                        }));
        }


        /// <summary>
        /// Transition the state machine to the specified state
        /// </summary>
        public static EventActivityBinder<ProcessState, TMessage> LogAndTransitionTo<TMessage>(
            this EventActivityBinder<ProcessState, TMessage> source,
            State toState,
            ILogger logger = null)
            where TMessage : class
        {
            logger?.LogInfoMessage(
                "The state machine will execute transition to state: {state}",
                LogHelpers.Arguments(toState.Name));

            return source.TransitionTo(toState);
        }

        /// <summary>
        /// Adds a synchronous delegate activity to the event's behavior
        /// </summary>
        /// <typeparam name="TMessage">The event data type</typeparam>
        /// <param name="logger"> An instance of <see cref="ILogger"/></param>
        /// <param name="currentState">The current state of the saga</param>
        /// <param name="consumedEvent">The event message that is being consumed in the current state</param>
        /// <param name="binder">The event binder</param>
        public static EventActivityBinder<ProcessState, TMessage> LogEnterState<TMessage>(this EventActivityBinder<ProcessState, TMessage> binder,
            ILogger logger,
            State currentState,
            Event<TMessage> consumedEvent)
            where TMessage : class
        {
            return binder.Then(
                c =>
                {
                    logger.LogInfoMessage(
                        "Executing During statement: Entered in state: {state}, consumed message of type: {messageType} with correlation id: {corrId}",
                        LogHelpers.Arguments(currentState.Name, consumedEvent.Name, c.CorrelationId));

                });

        }

    }
}
