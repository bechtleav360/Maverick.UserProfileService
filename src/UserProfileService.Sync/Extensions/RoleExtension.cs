using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="RoleView" />.
/// </summary>
public static class RoleExtension
{
    /// <summary>
    ///     Extract relation objects of the current <see cref="RoleView" />.
    /// </summary>
    /// <param name="role">Role the relations are extracted from.</param>
    /// <returns>All related objects saved in current <see cref="RoleView" /> object.</returns>
    public static IList<ObjectRelation> ExtractObjectRelations(this RoleView role)
    {
        var objectRelations = new List<ObjectRelation>();

        if (role.LinkedProfiles != null)
        {
            List<ObjectRelation> relations = role.LinkedProfiles
                .Select(
                    profile => new ObjectRelation(
                        AssignmentType.Unknown,
                        new KeyProperties(profile?.ExternalIds?.FirstOrDefault()?.Id, null),
                        profile.Id,
                        profile.ProfileKind.ToObjectType()))
                .ToList();

            objectRelations.AddRange(relations);
        }

        return objectRelations;
    }
}
