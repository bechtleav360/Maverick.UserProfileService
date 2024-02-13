#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Common.V2.TicketStore.Enums;

namespace UserProfileService.Common.V2.TicketStore.Abstractions;

/// <summary>
///     Abstract base class for tickets used to track the status of api operations.
/// </summary>
public abstract class TicketBase
{
    /// <summary>
    ///     The correlation id related to the ticket.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    ///     A code of the error that has been occurred. It will be 0, if none occurred.
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    ///     A message of the error that has been occurred. It will be <c>null</c>, if none occurred.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     The point of time when the ticket has be finished.
    /// </summary>
    public DateTime Finished { get; set; } = DateTime.MinValue.ToUniversalTime();

    /// <summary>
    ///     The id of the ticket.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The point of time when the ticket has be started/created.
    /// </summary>
    public DateTime Started { get; set; }

    /// <summary>
    ///     The current status of the ticket.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public TicketStatus Status { get; set; }

    /// <summary>
    ///     A string used to identify the type of this item.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="TicketBase" /> an sets the id to specified string.
    /// </summary>
    /// <param name="id">The id of the ticket.</param>
    /// <param name="type">A string used to identify the type of this item.</param>
    protected TicketBase(string id, string type)
    {
        Id = id;
        Type = type;
        Started = DateTime.UtcNow;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="TicketBase" /> an sets the id to specified string
    ///     and a finished date and its status.
    /// </summary>
    /// <param name="id">The id of the ticket.</param>
    /// <param name="finished">The time the ticket was finished.</param>
    /// <param name="status">The status the ticket finished with.</param>
    protected TicketBase(string id, DateTime finished, TicketStatus status)
    {
        Id = id;
        Finished = finished;
        Status = status;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="TicketBase" /> with specified id, finished time and status.
    /// </summary>
    /// <param name="id">The id of the ticket.</param>
    /// <param name="type">A string used to identify the type of this item.</param>
    /// <param name="finished">The point of time when the ticket has been finished.</param>
    /// <param name="status">The status of the ticket.</param>
    protected TicketBase(string id, string type, DateTime finished, TicketStatus status)
    {
        Id = id;
        Type = type;
        Finished = finished;
        Status = status;
    }

    /// <summary>
    ///     Is used to initializes a ticket entry in failure state including error message and code.
    /// </summary>
    /// <param name="id">The id of the ticket.</param>
    /// <param name="type">A string used to identify the type of this item.</param>
    /// <param name="finished">The point of time when the ticket has been finished.</param>
    /// <param name="errorCode">The error code to be set.</param>
    /// <param name="errorMessage">The error message to be set.</param>
    protected TicketBase(
        string id,
        string type,
        DateTime finished,
        int errorCode,
        string errorMessage)
        : this(id, type, finished, TicketStatus.Failure)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
