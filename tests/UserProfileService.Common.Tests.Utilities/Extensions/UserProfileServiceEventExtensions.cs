using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.EventSourcing.Abstractions;
using InitiatorType = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class UserProfileServiceEventExtensions
    {
        public const string DefaultCorrelationId = "correlation-id-1";
        public const string DefaultProcessId = "saga-id-1";

        public static EventInitiator DefaultEventInitiator =>
            new EventInitiator
            {
                Id = "783121d42afc47eab4fa6c090640b068",
                Type = InitiatorType.System
            };

        public static TEvent AddDefaultMetadata<TEvent>(
            this TEvent domainEvent,
            IServiceProvider serviceProvider,
            ObjectIdent obj = null)
            where TEvent : class, IUserProfileServiceEvent
        {
            return AddDefaultMetadata(domainEvent, serviceProvider.GetRequiredService<IStreamNameResolver>(), obj);
        }

        public static TEvent AddDefaultMetadata<TEvent>(
            this TEvent domainEvent,
            IServiceProvider serviceProvider,
            ObjectType objType)
            where TEvent : class, IUserProfileServiceEvent
        {
            return AddDefaultMetadata(domainEvent, serviceProvider.GetRequiredService<IStreamNameResolver>(), objType);
        }

        public static TEvent AddDefaultMetadata<TEvent>(
            this TEvent domainEvent,
            IStreamNameResolver streamNameResolver = null,
            ObjectIdent obj = null,
            DateTime? timestamp = null)
            where TEvent : class, IUserProfileServiceEvent
        {
            if (domainEvent == null)
            {
                return null;
            }

            if (obj is { Id: null })
            {
                obj.Id = Guid.NewGuid().ToString();
            }

            domainEvent.MetaData.RelatedEntityId =
                streamNameResolver?.GetStreamName(
                    obj ?? new ObjectIdent("11ff8c2a-76d1-4ff0-9dcb-09434b9062a5", ObjectType.Group))
                ?? "group_11ff8c2a-76d1-4ff0-9dcb-09434b9062a5";

            domainEvent.MetaData.CorrelationId = "correlation-id-1";
            domainEvent.MetaData.HasToBeInverted = false;
            domainEvent.MetaData.Initiator = DefaultEventInitiator;
            domainEvent.MetaData.VersionInformation = 1;
            domainEvent.MetaData.Timestamp = timestamp ?? DateTime.UtcNow.AddMinutes(-10);
            domainEvent.MetaData.ProcessId = DefaultProcessId;

            return domainEvent;
        }

        public static TEvent AddDefaultMetadata<TEvent>(
            this TEvent domainEvent,
            IStreamNameResolver streamNameResolver = null,
            ObjectType objType = ObjectType.Group,
            DateTime? timestamp = null)
            where TEvent : class, IUserProfileServiceEvent
        {
            if (domainEvent == null)
            {
                return null;
            }

            domainEvent.MetaData.RelatedEntityId =
                streamNameResolver?.GetStreamName(new ObjectIdent("11ff8c2a-76d1-4ff0-9dcb-09434b9062a5", objType))
                ?? $"{objType.ToString().ToLowerInvariant()}_11ff8c2a-76d1-4ff0-9dcb-09434b9062a5";

            domainEvent.MetaData.CorrelationId = "correlation-id-1";
            domainEvent.MetaData.HasToBeInverted = false;
            domainEvent.MetaData.Initiator = DefaultEventInitiator;
            domainEvent.MetaData.VersionInformation = 1;
            domainEvent.MetaData.Timestamp = timestamp ?? DateTime.UtcNow.AddMinutes(-10);
            domainEvent.MetaData.ProcessId = DefaultProcessId;

            return domainEvent;
        }

        public static TEvent AddDefaultMetadata<TEvent>(
            this TEvent domainEvent,
            string relatedEntityId,
            DateTime? timestamp = null)
            where TEvent : class, IUserProfileServiceEvent
        {
            if (domainEvent == null)
            {
                return null;
            }

            domainEvent.MetaData.RelatedEntityId = relatedEntityId;
            domainEvent.MetaData.CorrelationId = DefaultCorrelationId;
            domainEvent.MetaData.HasToBeInverted = false;
            domainEvent.MetaData.Initiator = DefaultEventInitiator;
            domainEvent.MetaData.VersionInformation = 1;
            domainEvent.MetaData.Timestamp = timestamp ?? DateTime.UtcNow.AddMinutes(-10);
            domainEvent.MetaData.ProcessId = DefaultProcessId;

            return domainEvent;
        }
    }
}
