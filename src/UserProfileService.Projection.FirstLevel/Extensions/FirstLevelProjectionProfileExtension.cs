using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Projection.Abstractions;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ObjectTypeResolved = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Projection.FirstLevel.Extensions;

internal static class FirstLevelProjectionProfileExtension
{
    /// <summary>
    ///     Mapped the given container kind to object type.
    /// </summary>
    /// <param name="containerType"></param>
    /// <returns>Mapped object type of container kind.</returns>
    public static ObjectType ToObjectType(this ContainerType containerType)
    {
        return containerType switch
        {
            ContainerType.Group => ObjectType.Group,
            ContainerType.Organization => ObjectType.Organization,
            ContainerType.Role => ObjectType.Role,
            ContainerType.Function => ObjectType.Function,
            _ => throw new ArgumentOutOfRangeException(
                $"{nameof(ProfileKind)} could not be mapped to {nameof(ObjectType)}.")
        };
    }

    /// <summary>
    ///     Mapped the given container kind to object type.
    /// </summary>
    /// <param name="containerType"></param>
    /// <returns>Mapped object type of container kind.</returns>
    public static ObjectTypeResolved ToObjectTypeResolved(this ContainerType containerType)
    {
        return containerType switch
        {
            ContainerType.Group => ObjectTypeResolved.Group,
            ContainerType.Organization => ObjectTypeResolved.Organization,
            ContainerType.Role => ObjectTypeResolved.Role,
            ContainerType.Function => ObjectTypeResolved.Function,
            _ => throw new ArgumentOutOfRangeException(
                $"{nameof(ProfileKind)} could not be mapped to {nameof(ObjectType)}.")
        };
    }

    /// <summary>
    ///     Updates the <see cref="IFirstLevelProjectionProfile" /> with the properties from the
    ///     <see cref="PropertiesUpdatedPayload" />
    /// </summary>
    /// <typeparam name="T">Profile type (user, group, organization)</typeparam>
    /// <param name="profile"><see cref="IFirstLevelProjectionProfile" /> to update</param>
    /// <param name="payload"><see cref="PropertiesUpdatedPayload" /> containing the updates for the profile</param>
    /// <param name="upsEvent"><see cref="IUserProfileServiceEvent" /> source event</param>
    /// <param name="logger">Optional <see cref="ILogger" /></param>
    /// <param name="propertiesToIgnore">
    ///     Property names which shouldn't be updated in the profile even when listed in the
    ///     payload
    /// </param>
    /// <returns>The updated <see cref="IFirstLevelProjectionProfile" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     This exception will be thrown when the parameter <paramref name="profile" />,
    ///     <paramref name="payload" />, <paramref name="upsEvent" /> or <paramref name="propertiesToIgnore" /> is null.
    /// </exception>
    public static T UpdateProfileWithPayload<T>(
        this T profile,
        PropertiesUpdatedPayload payload,
        IUserProfileServiceEvent upsEvent,
        ILogger logger = null,
        params string[] propertiesToIgnore) where T : IFirstLevelProjectionProfile
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (upsEvent == null)
        {
            throw new ArgumentNullException(nameof(upsEvent));
        }

        if (propertiesToIgnore == null)
        {
            throw new ArgumentNullException(nameof(propertiesToIgnore));
        }

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        PropertyInfo[] profileProperties = profile.GetType().GetProperties();

        if (propertiesToIgnore.Any(x => !profileProperties.Select(y => y.Name).Contains(x)))
        {
            if (logger.IsEnabledFor(LogLevel.Debug))
            {
                logger?.LogDebugMessage(
                    "Following properties which should be ignored where not found on type {profileType}: {missingProperties}",
                    LogHelpers.Arguments(
                        typeof(T),
                        string.Join(
                            ',',
                            propertiesToIgnore.Where(x => !profileProperties.Select(y => y.Name).Contains(x)))));
            }
        }

        if (payload.Properties.Any())
        {
            logger?.LogDebugMessage(
                "Found {propertiesCountToUpdate} properties to update",
                LogHelpers.Arguments(payload.Properties.Count));

            foreach (KeyValuePair<string, object> propertyToUpdate in payload.Properties)
            {
                if (!propertiesToIgnore.Contains(propertyToUpdate.Key))
                {
                    PropertyInfo profilePropertyToUpdate =
                        profileProperties.FirstOrDefault(x => x.Name == propertyToUpdate.Key);

                    if (profilePropertyToUpdate != null)
                    {
                        logger?.LogDebugMessage(
                            "Updating property \"{propertyName}\"",
                            LogHelpers.Arguments(propertyToUpdate.Key));

                        if (logger.IsEnabledForTrace())
                        {
                            logger?.LogTraceMessage(
                                "Updating property \"{propertyName}\" with value \"{propertyValue}\"",
                                LogHelpers.Arguments(propertyToUpdate.Key, propertyToUpdate.Value));
                        }

                        if (propertyToUpdate.Value is JContainer container)
                        {
                            profilePropertyToUpdate.SetValue(
                                profile,
                                container.ToObject(profilePropertyToUpdate.PropertyType));
                        }
                        else
                        {
                            profilePropertyToUpdate.SetValue(profile, propertyToUpdate.Value);
                        }
                    }
                    else
                    {
                        logger?.LogWarnMessage(
                            "Unable to update profile property {propertyName} for type {profileType}",
                            LogHelpers.Arguments(propertyToUpdate.Key, typeof(T).Name));
                    }
                }
                else
                {
                    logger?.LogDebugMessage(
                        "Ignoring property update \"{propertyName}\"",
                        LogHelpers.Arguments(propertyToUpdate.Key));

                    if (logger.IsEnabledForTrace())
                    {
                        logger.LogTraceMessage(
                            "Ignoring property update \"{propertyName}\" with value \"{propertyValue}\"",
                            LogHelpers.Arguments(propertyToUpdate.Key, propertyToUpdate.Value));
                    }
                }
            }

            profile.UpdatedAt = upsEvent.MetaData.Timestamp;
        }

        return profile;
    }
}
