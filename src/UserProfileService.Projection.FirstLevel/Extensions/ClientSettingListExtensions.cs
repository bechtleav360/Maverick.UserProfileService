using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Extensions;

/// <summary>
///     This extension is used to recalculate the given client setting for a list.
/// </summary>
internal static class ClientSettingListExtensions
{
    /// <summary>
    ///     Checks if a <see cref="RangeCondition" />s are valid.
    /// </summary>
    /// <param name="conditions">The range conditions that has to be validated.</param>
    /// <returns>True if at least one range condition valid, otherwise false.</returns>
    private static bool ValidateInTimeRange(IList<RangeCondition> conditions)
    {
        return conditions.Any(IsValid);
    }

    /// <summary>
    ///     Checks if one range if valid.
    /// </summary>
    /// <param name="condition">The range conditions that to be validated.</param>
    /// <returns>True if at the range is valid otherwise false.</returns>
    private static bool IsValid(RangeCondition condition)
    {
        return (condition.Start ?? DateTime.MinValue) <= DateTime.UtcNow
            && (condition.End ?? DateTime.MaxValue) >= DateTime.UtcNow;
    }

    /// <summary>
    ///     The  methods recalculates to a given list of <see cref="FirstLevelProjectionsClientSetting" /> the valid client
    ///     settings and
    ///     creates the needed <see cref="IUserProfileServiceEvent" />s.
    ///     First they are filtered through the <see cref="RangeCondition" />.
    ///     Then they are evaluated according to these properties:first the hops, than the weight
    ///     and at least updatedAt. The first of the list is the valid client setting.
    /// </summary>
    /// <param name="clientSettings"></param>
    /// <param name="profileId">The profileId whose clientSettings have to be changed.</param>
    /// <returns></returns>
    internal static List<IUserProfileServiceEvent> GetClientSettingsCalculatedEvents(
        this IList<FirstLevelProjectionsClientSetting> clientSettings,
        string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException(nameof(profileId));
        }

        if (clientSettings == null)
        {
            throw new ArgumentNullException(nameof(clientSettings));
        }

        List<IGrouping<string, FirstLevelProjectionsClientSetting>> validClientSettingsWithRangeConditions =
            clientSettings.Where(client => client.Conditions.Values.All(ValidateInTimeRange))
                .GroupBy(clientSetting => clientSetting.SettingsKey)
                .ToList();

        var clientSettingEvents = new List<IUserProfileServiceEvent>();

        foreach (IGrouping<string, FirstLevelProjectionsClientSetting> groupedClientSetting in
                 validClientSettingsWithRangeConditions)
        {
            FirstLevelProjectionsClientSetting validClientSettings = groupedClientSetting.OrderBy(cls => cls.Hops)
                .ThenByDescending(cls => cls.Weight)
                .ThenByDescending(cls => cls.UpdatedAt)
                .First();

            clientSettingEvents.Add(
                new ClientSettingsCalculated
                {
                    CalculatedSettings = validClientSettings.Value,
                    Key = validClientSettings.SettingsKey,
                    ProfileId = profileId,
                    IsInherited = validClientSettings.Hops > 0
                });
        }

        return clientSettingEvents;
    }
}
