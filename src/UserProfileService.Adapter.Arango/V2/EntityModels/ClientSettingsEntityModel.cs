using System;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class ClientSettingsEntityModel : ClientSettingsBasic
{
    /// <summary>
    ///     Contains the minimum hops required to reach this config.
    /// </summary>
    [Obsolete("The hops calculation is already done in the first level projection")]
    public int Hops { get; set; }

    /// <summary>
    ///     Gives the information if the client setting is inherited
    /// </summary>
    public bool IsInherited { get; set; } = false;

    /// <summary>
    ///     Contains the type of the Profile for which the ClientSettings were set.
    /// </summary>
    [Obsolete]
    public ProfileKind Kind { get; set; }

    /// <summary>
    ///     Contains the date when the profile of which the config came was updated.
    /// </summary>
    [Obsolete]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     Contains the weight of the group.
    /// </summary>
    [Obsolete]
    public double Weight { get; set; }
}
