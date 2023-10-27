using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Events.Payloads.Contracts;

/// <summary>
///     Class to define well known assignments between two objects.
/// </summary>
public static class WellKnownAssignments
{
    /// <summary>
    ///     Defines the allowed assignments for different object types.
    /// </summary>
    public static Dictionary<ObjectType, ICollection<(ObjectType objectType, AssignmentType assType)>> Dict =>
        new Dictionary<ObjectType, ICollection<(ObjectType objectType, AssignmentType assType)>>
        {
            { ObjectType.Role, Role },
            { ObjectType.Function, Function },
            { ObjectType.User, User },
            { ObjectType.Group, Group },
            { ObjectType.Organization, Organization }
        };

    /// <summary>
    ///     Defines the allowed assignments for functions.
    /// </summary>
    public static ICollection<(ObjectType objectType, AssignmentType assType)> Function =>
        new List<(ObjectType objectType, AssignmentType assType)>
        {
            (ObjectType.Organization, AssignmentType.Unknown),
            (ObjectType.Group, AssignmentType.Unknown),
            (ObjectType.User, AssignmentType.Unknown)
        };

    /// <summary>
    ///     Defines the allowed assignments for groups.
    /// </summary>
    public static ICollection<(ObjectType objectType, AssignmentType assType)> Group =>
        new List<(ObjectType objectType, AssignmentType assType)>
        {
            (ObjectType.Organization, AssignmentType.ParentsToChild),
            (ObjectType.Group, AssignmentType.ParentsToChild),
            (ObjectType.Group, AssignmentType.ChildrenToParent),
            (ObjectType.User, AssignmentType.ChildrenToParent),
            (ObjectType.Function, AssignmentType.Unknown),
            (ObjectType.Role, AssignmentType.Unknown)
        };

    /// <summary>
    ///     Defines the allowed assignments for organizations.
    /// </summary>
    public static ICollection<(ObjectType objectType, AssignmentType assType)> Organization =>
        new List<(ObjectType objectType, AssignmentType assType)>
        {
            (ObjectType.Organization, AssignmentType.ParentsToChild),
            (ObjectType.Organization, AssignmentType.ChildrenToParent),
            (ObjectType.Group, AssignmentType.ChildrenToParent),
            (ObjectType.Function, AssignmentType.Unknown),
            (ObjectType.Role, AssignmentType.Unknown)
        };

    /// <summary>
    ///     Defines the allowed assignments for roles.
    /// </summary>
    public static ICollection<(ObjectType objectType, AssignmentType assType)> Role =>
        new List<(ObjectType objectType, AssignmentType assType)>
        {
            (ObjectType.Organization, AssignmentType.Unknown),
            (ObjectType.Group, AssignmentType.Unknown),
            (ObjectType.User, AssignmentType.Unknown)
        };

    /// <summary>
    ///     Defines the allowed assignments for users.
    /// </summary>
    public static ICollection<(ObjectType objectType, AssignmentType assType)> User =>
        new List<(ObjectType objectType, AssignmentType assType)>
        {
            (ObjectType.Organization, AssignmentType.ParentsToChild),
            (ObjectType.Group, AssignmentType.ParentsToChild),
            (ObjectType.Function, AssignmentType.Unknown),
            (ObjectType.Role, AssignmentType.Unknown)
        };
}
