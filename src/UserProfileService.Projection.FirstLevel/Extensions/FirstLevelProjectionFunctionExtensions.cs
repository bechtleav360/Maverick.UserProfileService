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

internal static class FirstLevelProjectionFunctionExtensions
{
    /// <summary>
    ///     Updates the <see cref="FirstLevelProjectionFunction" /> with the properties from the
    ///     <see cref="PropertiesUpdatedPayload" />
    /// </summary>
    /// <typeparam name="T">Function Type</typeparam>
    /// <param name="function"><see cref="FirstLevelProjectionFunction" /> to update</param>
    /// <param name="upsEvent"><see cref="IUserProfileServiceEvent" /> source event</param>
    /// <param name="payload"><see cref="PropertiesUpdatedPayload" /> containing the updates for the function</param>
    /// <param name="logger">Optional <see cref="ILogger" /></param>
    /// <param name="propertiesToIgnore">
    ///     Property names which shouldn't be updated in the function even when listed in the
    ///     payload
    /// </param>
    /// <returns>The updated <see cref="FirstLevelProjectionFunction" /></returns>
    /// <exception cref="ArgumentNullException">
    ///     The exception is thrown when <paramref name="function" />, <paramref name="payload" />, or
    ///     <paramref name="upsEvent" /> is null.
    /// </exception>
    public static T UpdateFunctionWithPayload<T>(
        this T function,
        PropertiesUpdatedPayload payload,
        IUserProfileServiceEvent upsEvent,
        ILogger logger = null,
        params string[] propertiesToIgnore) where T : FirstLevelProjectionFunction
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (upsEvent == null)
        {
            throw new ArgumentNullException(nameof(upsEvent));
        }

        PropertyInfo[] functionProperties = function.GetType().GetProperties();

        if (propertiesToIgnore.Any(x => !functionProperties.Select(y => y.Name).Contains(x)))
        {
            if (logger.IsEnabledFor(LogLevel.Debug))
            {
                logger?.LogDebugMessage(
                    "Following properties which should be ignored where not found on type {profileType}: {missingProperties}",
                    LogHelpers.Arguments(
                        typeof(T),
                        string.Join(
                            ',',
                            propertiesToIgnore.Where(x => !functionProperties.Select(y => y.Name).Contains(x)))));
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
                        functionProperties.FirstOrDefault(x => x.Name == propertyToUpdate.Key);

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
                                function,
                                container.ToObject(profilePropertyToUpdate.PropertyType));
                        }
                        else
                        {
                            profilePropertyToUpdate.SetValue(function, propertyToUpdate.Value);
                        }
                    }
                    else
                    {
                        logger?.LogWarnMessage(
                            "Unable to update function property {propertyName} for type {profileType}",
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

            function.UpdatedAt = upsEvent.MetaData.Timestamp;
        }

        return function;
    }
}
