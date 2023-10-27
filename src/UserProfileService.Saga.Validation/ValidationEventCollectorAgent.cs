using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventCollector;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.EventCollector.Configuration;
using UserProfileService.Validation.Abstractions;
using UserProfileService.Validation.Abstractions.Message;

namespace UserProfileService.Saga.Validation;

/// <summary>
///     Defines a agent (<see cref="MassTransit.IConsumer{TMessage}" />) for message of type
///     <see cref="ValidationResponse" />.
/// </summary>
// Hint: Must be public because otherwise MassTransit will not find and register the agent.
public class ValidationEventCollectorAgent : Agent<ValidationResponse, ValidationCompositeResponse>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    /// <param name="eventCollectionConfiguration"></param>
    public ValidationEventCollectorAgent(
        IServiceProvider serviceProvider,
        ILogger<ValidationEventCollectorAgent> logger,
        IOptionsMonitor<EventCollectorConfiguration> eventCollectionConfiguration) : base(
        serviceProvider,
        logger,
        eventCollectionConfiguration)
    {
    }

    /// <inheritdoc cref="Agent{TMessage,TCompositeResponseMessage}" />
    protected override ValidationCompositeResponse BuildCompositeResponseMessage(
        Guid collectingId,
        ICollection<EventData> eventDataCollection)
    {
        Logger.EnterMethod();

        if (eventDataCollection == null)
        {
            return new ValidationCompositeResponse(collectingId, true, false);
        }

        var validationResults = new List<ValidationAttribute>();
        var isValid = true;

        foreach (EventData eventData in eventDataCollection)
        {
            if (eventData == null)
            {
                Logger.LogWarnMessage(
                    "Event data should not be null while processing validation result.",
                    LogHelpers.Arguments());

                continue;
            }

            if (eventData.ErrorOccurred)
            {
                // validation is invalid because an error occurred.
                // Can be extended in the future with more configuration and more complex logic.
                var errorResponse = new ValidationCompositeResponse(collectingId, false, true);

                return Logger.ExitMethod(errorResponse);
            }

            try
            {
                // At this point no error should occur,
                // because the base class serializes the data and therefore the data should be correct.
                var data = JsonSerializer.Deserialize<ValidationResult>(eventData.Data);

                if (data == null)
                {
                    Logger.LogWarnMessage(
                        "Data should not be null while processing validation result of host {host} and process id {processId}. Event data will be skipped.",
                        LogHelpers.Arguments(eventData.Host, eventData.CollectingId));

                    continue;
                }

                validationResults.AddRange(data.Errors);
                isValid = isValid && data.IsValid;
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"An error occurred while processing validation result of host {eventData.Host} and process id {eventData.CollectingId}.",
                    e);
            }
        }

        var result = new ValidationCompositeResponse(collectingId, isValid, validationResults);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc cref="Agent{TMessage,TCompositeResponseMessage}" />
    public override EventData CreateEventData(ValidationResponse message, string host)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        Guid processId = message.CollectingId;

        try
        {
            return new EventData
            {
                CollectingId = processId,
                Data = JsonSerializer.Serialize(message),
                Host = host
            };
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "An error occurred while handling message for request {requestId} and host {host}.",
                LogHelpers.Arguments(processId, host));

            return new EventData
            {
                CollectingId = processId,
                ErrorOccurred = true,
                Host = host
            };
        }
    }
}
