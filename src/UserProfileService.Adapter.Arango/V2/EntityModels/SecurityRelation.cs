using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Contains information about the relation between profiles, SecOs and functions or/and roles.
/// </summary>
internal class SecurityRelation
{
    /// <summary>
    ///     The functions assigned to the SecO and profile.
    /// </summary>
    [JsonProperty]
    internal List<FunctionBasic> Functions { get; set; }

    /// <summary>
    ///     Id of the profile.
    /// </summary>
    [JsonProperty]
    internal string ProfileId { get; }

    /// <summary>
    ///     The roles assigned to the SecO and profile.
    /// </summary>
    [JsonProperty]
    internal List<RoleBasic> Roles { get; set; }

    /// <summary>
    ///     Id of the SecO.
    /// </summary>
    [JsonProperty]
    internal string SecOId { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="SecurityRelation" />.
    /// </summary>
    /// <param name="secOId"></param>
    /// <param name="profileId"></param>
    public SecurityRelation(
        string secOId,
        string profileId)
    {
        SecOId = secOId;
        ProfileId = profileId;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(SecOId, ProfileId);
    }
}
