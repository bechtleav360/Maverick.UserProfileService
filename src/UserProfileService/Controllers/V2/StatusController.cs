using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;
using UserProfileService.Common.V2.TicketStore.Models;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     All Status related methods.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]", Name = nameof(StatusController))]
[ApiVersion("2.0", Deprecated = false)]
public class StatusController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly ITicketStore _ticketStore;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public StatusController(
        ITicketStore ticketStore,
        ILogger<StatusController> logger,
        IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
        _ticketStore = ticketStore;
        _logger = logger;
    }

    private ProblemDetails ExtractProblemDetails(TicketBase ticket)
    {
        if (ticket is UserProfileOperationTicket upsTicket)
        {
            return _logger.ExitMethod(upsTicket.Details);
        }

        return _logger.ExitMethod(
            new ProblemDetails
            {
                Title = "An error occurred",
                Detail = ticket.ErrorMessage,
                Status = ticket.ErrorCode != 0 ? ticket.ErrorCode : StatusCodes.Status500InternalServerError
            });
    }

    /// <summary>
    ///     Gets the status of a previously started request identified by the parameter <paramref name="id" />.
    /// </summary>
    /// <param name="id">The id of the previously started asynchronous operation.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">If the running operation is still not finished.</response>
    /// <response code="302">
    ///     If the operation has been finished. It will be redirected to resource endpoint, where to get the
    ///     result.
    /// </response>
    /// <response code="404">If the asynchronous operation request could be found.</response>
    /// <response code="500">If an internal error occurred.</response>
    [HttpGet("{id}", Name = nameof(GetStatus))]
    [ProducesResponseType(typeof(UserProfileOperationTicket), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserProfileOperationTicket), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> GetStatus(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Ticket: {id}.",
                LogHelpers.Arguments(id));
        }

        cancellationToken.ThrowIfCancellationRequested();

        TicketBase ticket = await _ticketStore.GetTicketAsync(id, cancellationToken);

        if (ticket == null)
        {
            throw new InstanceNotFoundException("TICKET_NOT_FOUND", id, $"No ticket found with id {id}.");
        }

        if (ticket.Status == TicketStatus.Complete)
        {
            return _logger.ExitMethod(
                await ActionResultHelper.GetFinalResult(
                    _urlHelperFactory.GetUrlHelper(ControllerContext),
                    ticket as UserProfileOperationTicket,
                    _logger));
        }

        if (ticket.Status != TicketStatus.Failure)
        {
            Response.Headers.Add(HeaderNames.RetryAfter, "1");

            return _logger.ExitMethod(
                AcceptedAtRoute(
                    nameof(GetStatus),
                    new
                    {
                        id
                    },
                    ticket));
        }

        ProblemDetails problemDetails = ExtractProblemDetails(ticket);

        return _logger.ExitMethod(
            new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            });
    }

    /// <summary>
    ///     Gets the status of a previously started request identified by the parameter <paramref name="id" />.
    /// </summary>
    /// <param name="id">The id of the previously started asynchronous operation.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">If the running operation is still not finished.</response>
    /// <response code="302">
    ///     If the operation has been finished. It will be redirected to resource endpoint, where to get the
    ///     result.
    /// </response>
    /// <response code="404">If the asynchronous operation request could be found.</response>
    /// <response code="500">If an internal error occurred.</response>
    [HttpGet("{id}/raw", Name = nameof(GetRawStatus))]
    [ProducesResponseType(typeof(UserProfileOperationTicket), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserProfileOperationTicket), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> GetRawStatus(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Ticket: {id}.",
                LogHelpers.Arguments(id));
        }

        cancellationToken.ThrowIfCancellationRequested();

        TicketBase ticket = await _ticketStore.GetTicketAsync(id, cancellationToken);

        if (ticket == null)
        {
            throw new InstanceNotFoundException("TICKET_NOT_FOUND", id, $"No ticket found with id {id}.");
        }

        if (ticket.Status != TicketStatus.Failure)
        {
            Response.Headers.Add(HeaderNames.RetryAfter, "1");

            return _logger.ExitMethod(
                AcceptedAtRoute(
                    nameof(GetStatus),
                    new
                    {
                        id
                    },
                    ticket));
        }

        ProblemDetails problemDetails = ExtractProblemDetails(ticket);

        return _logger.ExitMethod(
            new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            });
    }
}
