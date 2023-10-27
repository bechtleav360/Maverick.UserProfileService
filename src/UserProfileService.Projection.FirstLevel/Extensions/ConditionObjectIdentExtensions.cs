using System;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.FirstLevel.Handler.V2;

namespace UserProfileService.Projection.FirstLevel.Extensions;

/// <summary>
///     This extensions are for <see cref="ConditionObjectIdent" />s objects.
///     It is needed for the <see cref="ObjectAssignmentFirstLevelEventHandler" /> that assigns
///     profiles to profiles or profiles to container. The handler performs also
///     the the counter action and unassigns the object from each other.
/// </summary>
internal static class ConditionObjectIdentExtensions
{
    /// <summary>
    ///     The method prepare the assignments so that the profileId is always
    ///     a profile and the target is container, when the profile is assigned to a
    ///     function or a role. Ordering the assignments in such a way has the advantage
    ///     that you can perform easily a assignment or unassignments without to be worried
    ///     what type the profileId or TargetId is.
    /// </summary>
    /// <param name="assignments">The assignments that should be rearranged</param>
    /// <param name="resource">The resource of the objects.</param>
    /// <param name="assignmentType">The type of assignments: child-to-parent, or parent to child.</param>
    /// <param name="logger">An optional logger that logs important message for debug purposes.</param>
    /// <returns></returns>
    internal static Assignment[] GetAssignments(
        this ConditionObjectIdent[] assignments,
        ObjectIdent resource,
        AssignmentType assignmentType,
        ILogger logger = null)
    {
        logger.EnterMethod();

        if (assignments == null || !assignments.Any())
        {
            throw new ArgumentException(nameof(assignments));
        }

        if (resource == null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        // Assigning the following object:
        // function --> group, user, organization
        // role --> group, user, organization
        // Although oe are forbidden to attach to a role or function!
        if (!resource.Type.IsProfileType() && assignments.All(o => o.Type.IsProfileType()))
        {
            return assignments.Select(
                    p => new Assignment
                    {
                        ProfileId = p.Id,
                        Conditions = p.Conditions,
                        TargetId = resource.Id,
                        TargetType = resource.Type
                    })
                .ToArray();
        }

        // Assigning the following objects:
        // group --> functions, role
        // user --> functions, roles
        // organization --> functions, roles
        if (resource.Type.IsProfileType() && assignments.All(o => !o.Type.IsProfileType()))
        {
            return assignments.Select(
                    p => new Assignment
                    {
                        ProfileId = resource.Id,
                        Conditions = p.Conditions,
                        TargetId = p.Id,
                        TargetType = p.Type
                    })
                .ToArray();
        }

        // group --> user
        // group --> group
        // organization --> organization
        // Can go in both assignments directions. n-->1 or 1-->n.
        // n groups to one group, or 1 group to users and groups
        if (resource.Type.IsProfileType() && assignments.All(o => o.Type.IsProfileType()))
        {
            return assignments.Select(
                    p =>
                    {
                        // Assign n profiles ( assignment) to one container profile (resource)
                        if (assignmentType == AssignmentType.ChildrenToParent)
                        {
                            return new Assignment
                            {
                                ProfileId = p.Id,
                                Conditions = p.Conditions,
                                TargetId = resource.Id,
                                TargetType = resource.Type
                            };
                        }

                        // Assign one profile (assignments) to n containers (resource)
                        return new Assignment
                        {
                            ProfileId = resource.Id,
                            Conditions = p.Conditions,
                            TargetId = p.Id,
                            TargetType = p.Type
                        };
                    })
                .ToArray();
        }

        return logger.ExitMethod(Array.Empty<Assignment>());
    }
}
