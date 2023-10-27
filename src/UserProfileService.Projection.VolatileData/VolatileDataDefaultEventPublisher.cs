using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Events.Implementation.V3;
using UserProfileService.Events.Payloads.Contracts;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.VolatileData;

internal class VolatileDataDefaultEventPublisher : IEventPublisher
{
    private readonly IVolatileUserSettingsStore _DataStore;
    private readonly ILogger<VolatileDataDefaultEventPublisher> _Logger;
    private readonly IProjectionResponseService _ResponseService;

    /// <inheritdoc />
    public bool IsDefault => false;

    /// <inheritdoc />
    public string Type => WellKnownCustomCommandHandlers.VolatileDataService;

    public VolatileDataDefaultEventPublisher(
        IProjectionResponseService responseService,
        ILogger<VolatileDataDefaultEventPublisher> logger,
        IVolatileUserSettingsStore dataStore)
    {
        _ResponseService = responseService;
        _Logger = logger;
        _DataStore = dataStore;
    }

    private async Task HandleEventAsync(
        UserSettingsSectionCreatedEvent eventData,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        UserSettingSectionCreatedPayload newSection = eventData.Payload
            ?? throw new ArgumentException(
                "event data does not contain a payload",
                nameof(eventData));

        if (JsonNode.Parse(newSection.ValuesAsJsonString) is not JsonArray newSettings)
        {
            throw new ArgumentException(
                "Payload of provided event data does not contain a valid value (should be a JSON array)",
                nameof(eventData));
        }

        cancellationToken.ThrowIfCancellationRequested();

        await _DataStore.CreateUserSettingsAsync(
            newSection.UserId,
            newSection.SectionName,
            newSettings,
            cancellationToken);

        _Logger.LogInfoMessage(
            "Event data [id = {eventId}; type = {eventType}] written successfully - correlation id: {correlationId}",
            LogHelpers.Arguments(
                eventData.EventId,
                eventData.Type,
                eventData.MetaData.CorrelationId));

        _Logger.ExitMethod();
    }

    private async Task HandleEventAsync(
        UserSettingObjectUpdatedEvent eventData,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        UserSettingObjectUpdatedPayload payload = eventData.Payload
            ?? throw new ArgumentException(
                "event data does not contain a payload",
                nameof(eventData));

        if (JsonNode.Parse(payload.ValuesAsJsonString) is not JsonObject newSettings)
        {
            throw new ArgumentException(
                "Payload of provided event data does not contain a valid value (should be a JSON object)",
                nameof(eventData));
        }

        cancellationToken.ThrowIfCancellationRequested();

        await _DataStore.UpdateUserSettingsAsync(
            payload.UserId,
            payload.SectionName,
            payload.SettingObjectId,
            newSettings,
            cancellationToken);

        _Logger.LogInfoMessage(
            "Event data [id = {eventId}; type = {eventType}] written successfully - correlation id: {correlationId}",
            LogHelpers.Arguments(
                eventData.EventId,
                eventData.Type,
                eventData.MetaData.CorrelationId));

        _Logger.ExitMethod();
    }

    private async Task HandleEventAsync(
        UserSettingSectionDeletedEvent eventData,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        UserSettingSectionDeletedPayload payload = eventData.Payload
            ?? throw new ArgumentException(
                "event data does not contain a payload",
                nameof(eventData));

        cancellationToken.ThrowIfCancellationRequested();

        await _DataStore.DeleteSettingsSectionForUserAsync(
            payload.UserId,
            payload.SectionName,
            cancellationToken);

        _Logger.LogInfoMessage(
            "Event data [id = {eventId}; type = {eventType}] written successfully - correlation id: {correlationId}",
            LogHelpers.Arguments(
                eventData.EventId,
                eventData.Type,
                eventData.MetaData.CorrelationId));

        _Logger.ExitMethod();
    }

    private async Task HandleEventAsync(
        UserSettingObjectDeletedEvent eventData,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        UserSettingObjectDeletedPayload payload = eventData.Payload
            ?? throw new ArgumentException(
                "event data does not contain a payload",
                nameof(eventData));

        cancellationToken.ThrowIfCancellationRequested();

        await _DataStore.DeleteUserSettingsAsync(
            payload.UserId,
            payload.SectionName,
            payload.SettingObjectId,
            cancellationToken);

        _Logger.LogInfoMessage(
            "Event data [id = {eventId}; type = {eventType}] written successfully - correlation id: {correlationId}",
            LogHelpers.Arguments(
                eventData.EventId,
                eventData.Type,
                eventData.MetaData.CorrelationId));

        _Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task PublishAsync(
        IUserProfileServiceEvent eventData,
        EventPublisherContext context,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        try
        {
            switch (eventData)
            {
                case UserSettingsSectionCreatedEvent createdEvent:
                    await HandleEventAsync(createdEvent, cancellationToken);

                    break;
                case UserSettingObjectUpdatedEvent updatedPayload:
                    await HandleEventAsync(updatedPayload, cancellationToken);

                    break;
                case UserSettingObjectDeletedEvent objDeletedEvent:
                    await HandleEventAsync(objDeletedEvent, cancellationToken);

                    break;
                case UserSettingSectionDeletedEvent sectionDeletedEvent:
                    await HandleEventAsync(sectionDeletedEvent, cancellationToken);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(eventData),
                        eventData,
                        "Type of event type is not supported by this event publisher.");
            }

            await _ResponseService.ResponseAsync(eventData, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _Logger.LogInfoMessage(
                "Operation has been cancelled (event type: {eventType}; command: {commandName}; command id: {commandId}; collecting id: {collectingId}).",
                LogHelpers.Arguments(
                    eventData.GetType().Name,
                    context.CommandName,
                    context.CommandId,
                    context.CollectingId));
        }
        catch (Exception exc)
        {
            _Logger.LogErrorMessage(
                exc,
                "An error occurred while handling event with id {eventId}, type {eventType} and command name {commandName}. Response with exception will be send.",
                LogHelpers.Arguments(
                    eventData.EventId,
                    eventData.GetType().Name,
                    context.CommandName));

            await _ResponseService.ResponseAsync(eventData, exc, cancellationToken);

            throw;
        }

        _Logger.ExitMethod();
    }
}
