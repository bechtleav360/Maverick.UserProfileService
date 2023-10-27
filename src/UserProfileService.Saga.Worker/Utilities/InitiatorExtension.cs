using System;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Commands;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Messaging.Abstractions.Models;
using EventInitiatorType = UserProfileService.EventSourcing.Abstractions.Models.InitiatorType;
using AggregateInitiatorType = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;
using InitiatorType = Maverick.UserProfileService.Models.EnumModels.InitiatorType;

namespace UserProfileService.Saga.Worker.Utilities;

/// <summary>
///     Extension method for <see cref="SagaInitiator" />
/// </summary>
public static class InitiatorExtension
{
    /// <summary>
    ///     Convert the current saga initiator to event initiator.
    /// </summary>
    /// <param name="initiator">Saga initiator to be converted.</param>
    /// <returns>Converted event initiator.</returns>
    public static EventInitiator ToEventInitiator(this CommandInitiator initiator)
    {
        if (initiator == null)
        {
            return null;
        }

        EventInitiatorType initiatorType = initiator.Type switch
        {
            CommandInitiatorType.ServiceAccount => EventInitiatorType
                .ServiceAccount,
            CommandInitiatorType.System => EventInitiatorType.System,
            CommandInitiatorType.User => EventInitiatorType.User,
            CommandInitiatorType.Unknown => EventInitiatorType.Unknown,
            _ => throw new NotSupportedException()
        };

        return new EventInitiator
        {
            Id = initiator.Id,
            Type = initiatorType
        };
    }

    /// <summary>
    ///     Convert the current saga initiator to aggregate event initiator.
    /// </summary>
    /// <param name="initiator">Saga initiator to be converted.</param>
    /// <returns>Converted event initiator.</returns>
    public static Maverick.UserProfileService.AggregateEvents.Common.EventInitiator ToAggregateEventInitiator(
        this CommandInitiator initiator)
    {
        if (initiator == null)
        {
            return null;
        }

        AggregateInitiatorType initiatorType = initiator.Type switch
        {
            CommandInitiatorType.ServiceAccount => AggregateInitiatorType
                .ServiceAccount,
            CommandInitiatorType.System => AggregateInitiatorType.System,
            CommandInitiatorType.User => AggregateInitiatorType.User,
            CommandInitiatorType.Unknown => AggregateInitiatorType.Unknown,
            _ => throw new NotSupportedException()
        };

        return new Maverick.UserProfileService.AggregateEvents.Common.EventInitiator
        {
            Id = initiator.Id,
            Type = initiatorType
        };
    }

    /// <summary>
    ///     Convert the current saga initiator to initiator.
    /// </summary>
    /// <param name="initiator">Saga initiator to be converted.</param>
    /// <returns>Converted initiator.</returns>
    public static Initiator ToInitiator(this CommandInitiator initiator)
    {
        if (initiator == null)
        {
            return new Initiator
            {
                Id = string.Empty,
                Type = InitiatorType.Unknown
            };
        }

        InitiatorType initiatorType = initiator.Type switch
        {
            CommandInitiatorType.ServiceAccount => InitiatorType.ServiceAccount,
            CommandInitiatorType.System => InitiatorType.System,
            CommandInitiatorType.User => InitiatorType.User,
            CommandInitiatorType.Unknown => InitiatorType.Unknown,
            _ => throw new NotSupportedException()
        };

        return new Initiator
        {
            Id = initiator.Id,
            Type = initiatorType
        };
    }
}
