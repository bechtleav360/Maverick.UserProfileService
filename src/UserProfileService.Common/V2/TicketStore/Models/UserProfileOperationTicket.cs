using System;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;

namespace UserProfileService.Common.V2.TicketStore.Models;

/// <summary>
///     Ticket to handle process state of api operations.
/// </summary>
// TODO: Fix for entity framework usage as this class contains objects and lists.
public class UserProfileOperationTicket : TicketBase
{
    /// <summary>
    ///     Specifies the <see cref="TicketBase.Type" /> of <see cref="UserProfileOperationTicket" />.
    /// </summary>
    public const string TicketType = "OperationTicket";

    /// <summary>
    ///     Specifies additional query parameter
    ///     to extends the routing url.
    /// </summary>
    public string AdditionalQueryParameter { get; set; } = null;

    /// <summary>
    ///     Specifies the detail of the problem which occurred.
    /// </summary>
    public ProblemDetails Details { get; set; }

    /// <summary>
    ///     Specifies the id of the user who started the operation.
    ///     Can be <c>null</c> if the initiator is unknown.
    /// </summary>
    public string Initiator { get; set; }

    /// <summary>
    ///     The ids of the objects that will processed.
    /// </summary>
    public string[] ObjectIds { get; set; }

    /// <summary>
    ///     The executed operation.
    /// </summary>
    public string Operation { get; set; }

    /// <summary>
    ///     Default constructor for Newtonsoft.
    ///     Deletion will cause TicketStore Failure.
    /// </summary>
    public UserProfileOperationTicket() : base(null!, TicketType)
    {
    }

    public UserProfileOperationTicket(string id, string[] objectIds, string operation) : base(id, TicketType)
    {
        ObjectIds = objectIds;
        Operation = operation;
    }

    public UserProfileOperationTicket(
        string id,
        string[] objectIds,
        DateTime finished,
        TicketStatus status,
        string operation) : base(id, TicketType, finished, status)
    {
        ObjectIds = objectIds;
        Operation = operation;
    }
}
