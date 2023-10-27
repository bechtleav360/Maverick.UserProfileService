using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Contains the type of the profile for which the ClientSettings were set.
/// </summary>
public class FirstLevelProjectionsClientSetting
{
    /// <summary>
    ///     Contains a dictionary with the id of the parent profile as the key and all <see cref="RangeCondition" />s for that
    ///     assignment.
    ///     In order to be valid at least one condition per entry must be valid.
    /// </summary>
    public IDictionary<string, IList<RangeCondition>> Conditions { get; set; }

    /// <summary>
    ///     Contains the minimum hops required to reach this config.
    /// </summary>
    public int Hops { get; set; }

    /// <summary>
    ///     Contains the id of the profile from which the ClientSettings were.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     Contains the key which is used to identify the ClientSettings.
    /// </summary>
    public string SettingsKey { get; set; }

    /// <summary>
    ///     Contains the date when the profile of which the config came was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     Contains the value of the ClientSettings.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     Contains the weight of the group.
    /// </summary>
    public double Weight { get; set; }
}
