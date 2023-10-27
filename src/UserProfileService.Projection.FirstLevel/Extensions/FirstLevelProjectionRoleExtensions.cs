using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Extensions;

internal static class FirstLevelProjectionRoleExtensions
{
    /// <summary>
    ///     Updates the <see cref="FirstLevelProjectionRole" /> with the properties from the
    ///     <see cref="PropertiesUpdatedPayload" />
    /// </summary>
    /// <typeparam name="T">Function Type</typeparam>
    /// <param name="role"><see cref="FirstLevelProjectionRole" /> to update</param>
    /// <param name="upsEvent"><see cref="IUserProfileServiceEvent" /> source event</param>
    /// <param name="payload"><see cref="PropertiesUpdatedPayload" /> containing the updates for the role</param>
    /// <param name="logger">Optional <see cref="ILogger" /></param>
    /// <param name="propertiesToIgnore">Property names which shouldn't be updated in the role even when listed in the payload</param>
    /// <returns>The updated <see cref="FirstLevelProjectionRole" /></returns>
    /// <exception cref="ArgumentNullException">
    ///     This exception will be thrown when the parameter <paramref name="role" />,
    ///     <paramref name="payload" />, <paramref name="upsEvent" /> or <paramref name="propertiesToIgnore" /> is null.
    /// </exception>
    public static T UpdateRoleWithPayload<T>(
        this T role,
        PropertiesUpdatedPayload payload,
        IUserProfileServiceEvent upsEvent,
        ILogger logger = null,
        params string[] propertiesToIgnore) where T : FirstLevelProjectionRole
    {
        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
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

        PropertyInfo[] roleProperties = role.GetType().GetProperties();

        if (propertiesToIgnore.Any(x => !roleProperties.Select(y => y.Name).Contains(x)))
        {
            if (logger.IsEnabledFor(LogLevel.Debug))
            {
                logger?.LogDebugMessage(
                    "Following properties which should be ignored where not found on type {profileType}: {missingProperties}",
                    LogHelpers.Arguments(
                        typeof(T),
                        string.Join(
                            ',',
                            propertiesToIgnore.Where(x => !roleProperties.Select(y => y.Name).Contains(x)))));
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
                    PropertyInfo rolePropertiesToUpdate =
                        roleProperties.FirstOrDefault(x => x.Name == propertyToUpdate.Key);

                    if (rolePropertiesToUpdate != null)
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
                            rolePropertiesToUpdate.SetValue(
                                role,
                                container.ToObject(rolePropertiesToUpdate.PropertyType));
                        }
                        else
                        {
                            rolePropertiesToUpdate.SetValue(role, propertyToUpdate.Value);
                        }
                    }
                    else
                    {
                        logger?.LogWarnMessage(
                            "Unable to update role property {propertyName} for type {profileType}",
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

            role.UpdatedAt = upsEvent.MetaData.Timestamp;
        }

        return role;
    }
}
