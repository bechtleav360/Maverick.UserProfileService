using System;
using System.Collections.Generic;
using System.Net;
using UserProfileService.Commands.Models;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Commands;

/// <summary>
///     Define the failure response of a command for the ups saga worker.
/// </summary>
[Message(ServiceName = "saga-worker", ServiceGroup = "user-profile")]
public class SubmitCommandFailure : SubmitCommandResponseMessage
{
    /// <summary>
    ///     Command of ups worker.
    /// </summary>
    public string Command { get; set; }

    /// <inheritdoc />
    public override bool ErrorOccurred { get; set; } = true;

    /// <summary>
    ///     A optional collection of validation errors.
    /// </summary>
    public IList<ValidationAttribute> Errors { get; set; }

    /// <summary>
    ///     An optional exception object if error occurred.
    /// </summary>
    public ExceptionInformation Exception { get; set; }

    /// <summary>
    ///     Error message of command failure.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     Indicates http status code for error.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommandFailure" />.
    /// </summary>
    public SubmitCommandFailure()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommand" />.
    /// </summary>
    /// <param name="command">Data for related <see cref="Command" />.</param>
    /// <param name="commandId">The identifier of the command that will be added in the response to associate the response.</param>
    /// <param name="collectingId">Id to be used to collect messages and for which a common response should be sent.</param>
    /// <param name="message">Message of failure.</param>
    /// <param name="errors">A optional collection of errors</param>
    public SubmitCommandFailure(
        string command,
        string commandId,
        Guid collectingId,
        string message,
        IList<ValidationAttribute> errors = null)
    {
        Command = command;
        Id = new CommandIdentifier(commandId, collectingId);
        Message = message;
        StatusCode = HttpStatusCode.BadRequest;
        Errors = errors;
    }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommand" />.
    /// </summary>
    /// <param name="command">Data for related <see cref="Command" />.</param>
    /// <param name="commandId">The identifier of the command that will be added in the response to associate the response.</param>
    /// <param name="collectingId">id to be used to collect messages and for which a common response should be sent.</param>
    /// <param name="message">Message of failure.</param>
    /// <param name="exception">Error message of command failure.</param>
    public SubmitCommandFailure(
        string command,
        string commandId,
        Guid collectingId,
        string message,
        Exception exception) : this(command, commandId, collectingId, message, new ExceptionInformation(exception))
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommand" />.
    /// </summary>
    /// <param name="command">Data for related <see cref="Command" />.</param>
    /// <param name="commandId">The identifier of the command that will be added in the response to associate the response.</param>
    /// <param name="collectingId">id to be used to collect messages and for which a common response should be sent.</param>
    /// <param name="message">Message of failure.</param>
    /// <param name="exception">Error message of command failure.</param>
    public SubmitCommandFailure(
        string command,
        string commandId,
        Guid collectingId,
        string message,
        ExceptionInformation exception)
    {
        Command = command;
        Id = new CommandIdentifier(commandId, collectingId);
        Message = message;
        StatusCode = HttpStatusCode.InternalServerError;
        Exception = exception;
        Errors = new List<ValidationAttribute>();
    }
}
