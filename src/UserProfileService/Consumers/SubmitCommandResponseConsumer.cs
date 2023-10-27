using MassTransit;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Commands;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Enums;
using UserProfileService.Common.V2.TicketStore.Models;

namespace UserProfileService.Consumers;

/// <summary>
///     Consumer of command responses. Current <see cref="SubmitCommandSuccess" /> and <see cref="SubmitCommandFailure" />.
/// </summary>
// ReSharper disable once UnusedMember.Global => will be used by MassTransit (i.e. by reflection or something similar)
public class SubmitCommandResponseConsumer : IConsumer<SubmitCommandSuccess>, IConsumer<SubmitCommandFailure>
{
    private readonly ILogger<SubmitCommandResponseConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create an instance of <see cref="SubmitCommandResponseConsumer" />.
    /// </summary>
    /// <param name="serviceProvider">Service for retrieving service objects.</param>
    /// <param name="logger">The logger.</param>
    public SubmitCommandResponseConsumer(
        IServiceProvider serviceProvider,
        ILogger<SubmitCommandResponseConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc cref="IConsumer{SubmitCommandSuccess}" />
    public async Task Consume(ConsumeContext<SubmitCommandSuccess> context)
    {
        _logger.EnterMethod();

        using IServiceScope scopedService = _serviceProvider.CreateScope();

        var ticketStore = scopedService.ServiceProvider.GetRequiredService<ITicketStore>();

        if (string.IsNullOrWhiteSpace(context.Message.Id?.Id))
        {
            throw new ArgumentException(
                "The correlation / ticket id id of message is null or empty",
                nameof(context.Message.Id));
        }

        var ticket = (UserProfileOperationTicket)await ticketStore.GetTicketAsync(
            context.Message.Id.Id,
            context.CancellationToken);

        if (ticket == null)
        {
            _logger.LogDebugMessage(
                "No ticket found for given message with the correlationId {correlationId}, the correlation / ticket id {ticketId} and command {command}.",
                LogHelpers.Arguments(
                    context.CorrelationId,
                    context.Message.Id.Id,
                    context.Message.Command));

            return;
        }

        ticket.Finished = context.SentTime ?? DateTime.UtcNow;
        ticket.Status = TicketStatus.Complete;

        if (ticket.ObjectIds == null && !string.IsNullOrEmpty(context.Message.EntityId))
        {
            ticket.ObjectIds = new[] { context.Message.EntityId };
        }
        else if (ticket.ObjectIds != null
                 && !string.IsNullOrEmpty(context.Message.EntityId)
                 && ticket.ObjectIds.All(x => x != context.Message.EntityId))
        {
            ticket.ObjectIds = ticket.ObjectIds.Append(context.Message.EntityId).ToArray();
        }

        await ticketStore.AddOrUpdateEntryAsync(ticket);

        _logger.ExitMethod();
    }

    /// <inheritdoc cref="IConsumer{SubmitCommandFailure}" />
    public async Task Consume(ConsumeContext<SubmitCommandFailure> context)
    {
        _logger.EnterMethod();

        using IServiceScope scopedService = _serviceProvider.CreateScope();

        var ticketStore = scopedService.ServiceProvider.GetRequiredService<ITicketStore>();

        if (string.IsNullOrWhiteSpace(context.Message.Id?.Id))
        {
            throw new ArgumentException(
                "The correlation / ticket id id of message is null or empty",
                nameof(context.Message.Id));
        }

        TicketBase rawTicket = await ticketStore.GetTicketAsync(
            context.Message.Id.Id,
            context.CancellationToken);

        if (rawTicket == null)
        {
            _logger.LogDebugMessage(
                "No ticket found for given message with the correlationId {correlationId}, the commandId {commandId} and command {command}.",
                LogHelpers.Arguments(context.CorrelationId, context.Message.Id.Id, context.Message.Command));

            return;
        }

        rawTicket.Finished = context.SentTime ?? DateTime.UtcNow;
        rawTicket.Status = TicketStatus.Failure;
        rawTicket.ErrorMessage = context.Message.Message;
        rawTicket.ErrorCode = (int)context.Message.StatusCode;

        _logger.LogDebugMessage(
            "The Status: {status}, Finished: {finish}, ErrorMessage: {errorMessage}, ErrorCode: {errorCode}",
            LogHelpers.Arguments(
                rawTicket.Status,
                rawTicket.Finished,
                rawTicket.ErrorMessage,
                rawTicket.ErrorCode));

        if (rawTicket is UserProfileOperationTicket ticket)
        {
            ticket.Details = new ProblemDetails
            {
                Title = "Unable to complete the operation",
                Detail = context.Message.Message,
                Status = (int)context.Message.StatusCode,
                Extensions =
                {
                    { "StatusCode", context.Message.StatusCode }
                }
            };

            if (context.Message.Errors?.Any() == true)
            {
                ticket.Details.Extensions.Add("ValidationResults", context.Message.Errors);
            }

            if (context.Message.Exception != null)
            {
                ticket.Details.Extensions.Add("Exception", context.Message.Exception);
            }
        }

        await ticketStore.AddOrUpdateEntryAsync(rawTicket);

        _logger.ExitMethod();
    }
}
