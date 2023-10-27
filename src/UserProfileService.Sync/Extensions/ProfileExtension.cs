using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="IProfile" />s and the corresponding implementations.
/// </summary>
public static class ProfileExtension
{
    /// <summary>
    ///     Extract relation objects of the current <see cref="Group" />.
    /// </summary>
    /// <param name="group">group the relations are extracted from.</param>
    /// <returns>All related objects saved in current <see cref="Group" /> object.</returns>
    public static IList<ObjectRelation> ExtractObjectRelations(this Group group)
    {
        var objectRelations = new List<ObjectRelation>();

        if (group.Members != null)
        {
            List<ObjectRelation> relations = group.Members
                .Select(
                    member => new ObjectRelation(
                        AssignmentType.ChildrenToParent,
                        member.GetKeyProperties(),
                        member.Id,
                        member.Kind.ToObjectType()))
                .ToList();

            objectRelations.AddRange(relations);
        }

        if (group.MemberOf != null)
        {
            List<ObjectRelation> relations = group.MemberOf
                .Select(
                    member => new ObjectRelation(
                        AssignmentType.ParentsToChild,
                        member.GetKeyProperties(),
                        member.Id,
                        member.Kind.ToObjectType()))
                .ToList();

            objectRelations.AddRange(relations);
        }

        return objectRelations;
    }

    /// <summary>
    ///     Extract relation objects of the current <see cref="Organization" />.
    /// </summary>
    /// <param name="organization">Organization the relations are extracted from.</param>
    /// <returns>All related objects saved in current <see cref="Organization" /> object.</returns>
    public static IList<ObjectRelation> ExtractObjectRelations(this Organization organization)
    {
        var objectRelations = new List<ObjectRelation>();

        if (organization.Members != null)
        {
            List<ObjectRelation> relations = organization.Members
                .Select(
                    member => new ObjectRelation(
                        AssignmentType.ChildrenToParent,
                        member.GetKeyProperties(),
                        member.Id,
                        member.Kind.ToObjectType()))
                .ToList();

            objectRelations.AddRange(relations);
        }

        if (organization.MemberOf != null)
        {
            List<ObjectRelation> relations = organization.MemberOf
                .Select(
                    member => new ObjectRelation(
                        AssignmentType.ParentsToChild,
                        member.GetKeyProperties(),
                        member.Id,
                        member.Kind.ToObjectType()))
                .ToList();

            objectRelations.AddRange(relations);
        }

        return objectRelations;
    }

    /// <summary>
    ///     Extract relation objects of the current <see cref="User" />.
    /// </summary>
    /// <param name="user">User the relations are extracted from.</param>
    /// <returns>All related objects saved in current <see cref="User" /> object.</returns>
    public static IList<ObjectRelation> ExtractObjectRelations(this User user)
    {
        var objectRelations = new List<ObjectRelation>();

        if (user.MemberOf != null)
        {
            List<ObjectRelation> relations = user.MemberOf
                .Select(
                    member => new ObjectRelation(
                        AssignmentType.ParentsToChild,
                        member.GetKeyProperties(),
                        member.Id,
                        member.Kind.ToObjectType()))
                .ToList();

            objectRelations.AddRange(relations);
        }

        return objectRelations;
    }
}
