using System;
using UserProfileService.Commands.Models;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Commands;

/// <summary>
///     Defines a response message indicating that the command was not projected or projected with errors.
/// </summary>
[Message(ServiceName = "saga-worker", ServiceGroup = "user-profile")]
public class CommandProjectionFailure : ICommand
{
    /// <summary>
    ///     Exception message why because the projection failed.
    /// </summary>
    public ExceptionInformation Exception { get; set; }

    /// <summary>
    ///     Information to identify the command in several systems.
    /// </summary>
    public CommandIdentifier Id { get; set; }

    /// <summary>
    ///     Exception message why because the projection failed.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="CommandProjectionFailure" />.
    /// </summary>
    public CommandProjectionFailure()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="CommandProjectionFailure" />.
    /// </summary>
    /// <param name="commandId">Id of related command.</param>
    /// <param name="message">Message of exception.</param>
    /// <param name="exception">Related exception of failure.</param>
    public CommandProjectionFailure(string commandId, string message, Exception exception)
    {
        Id = new CommandIdentifier(commandId);
        Message = message;
        Exception = new ExceptionInformation(exception);
    }

    /// <summary>
    ///     Create an instance of <see cref="CommandProjectionFailure" />.
    /// </summary>
    /// <param name="commandId">Id of related command.</param>
    /// <param name="message">Message of exception.</param>
    /// <param name="exception">Related exception of failure.</param>
    public CommandProjectionFailure(string commandId, string message, ExceptionInformation exception)
    {
        Id = new CommandIdentifier(commandId);
        Message = message;
        Exception = exception;
    }
}
