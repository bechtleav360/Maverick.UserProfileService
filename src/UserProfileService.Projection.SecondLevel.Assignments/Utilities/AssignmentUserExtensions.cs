using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.Assignments.Utilities;

/// <summary>
///     Extends a <see cref="SecondLevelProjectionAssignmentsUser" />.
/// </summary>
public static class AssignmentUserExtensions
{
    /// <summary>
    ///     Checks if the given assignment is valid or not.
    /// </summary>
    /// <param name="condition">The <see cref="RangeCondition" /> to check.</param>
    /// <returns><c>true</c> if the assignment is active.</returns>
    private static bool IsValid(RangeCondition condition)
    {
        return (condition.Start ?? DateTime.MinValue) <= DateTime.UtcNow
            && (condition.End ?? DateTime.MaxValue) >= DateTime.UtcNow;
    }

    /// <summary>
    ///     Calculates all active memberships of the given assignment user recursively.
    /// </summary>
    /// <param name="user">The user of which to calculate the active assignments.</param>
    /// <param name="start">The object <see cref="ObjectIdent" /> to start from.</param>
    /// <param name="alreadyCalculated">A <see cref="ISet{T}" /> specifying all already calculated objects.</param>
    /// <param name="logger">A <see cref="ILogger" /> which can be sued for logging. Defaults to null (no logging)</param>
    /// <returns>All containers as <see cref="ObjectIdent" /> in which the user is part of.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if <paramref name="start" /> is null.</exception>
    internal static ISet<ObjectIdent> CalculateActiveMemberships(
        SecondLevelProjectionAssignmentsUser user,
        ObjectIdent start,
        ISet<ObjectIdent> alreadyCalculated = null,
        ILogger logger = null)
    {
        logger.EnterMethod();

        if (start == null)
        {
            throw new ArgumentNullException(nameof(start));
        }

        alreadyCalculated ??= new HashSet<ObjectIdent>();

        logger.LogDebugMessage(
            "Calculating active memberships for user {userId} with start {objectId} ({objectType})",
            LogHelpers.Arguments(user.ProfileId, start.Id, start.Type));

        ISet<ObjectIdent> parents = user.Assignments
            .Where(a => a.Profile.Id == start.Id)
            .Where(a => a.Conditions.Any(IsValid))
            .Select(a => a.Parent)
            .ToHashSet();

        if (logger.IsEnabledFor(LogLevel.Debug))
        {
            logger.LogDebugMessage(
                "Calculating assignments for {amountOfParent} parents",
                LogHelpers.Arguments(string.Join(", ", parents.Select(p => p.Id))));
        }

        List<ObjectIdent> parentsToCalculate = parents.Except(alreadyCalculated).ToList();
        alreadyCalculated.UnionWith(parents);

        parents.UnionWith(parentsToCalculate.SelectMany(p => CalculateActiveMemberships(user, p, alreadyCalculated)));

        return logger.ExitMethod(parents);
    }

    /// <summary>
    ///     Calculates all active memberships of the given assignment user recursively.
    /// </summary>
    /// <param name="user">The user of which to calculate the active assignments.</param>
    /// <param name="start">The object <see cref="ObjectIdent" /> to start from.</param>
    /// <param name="alreadyCalculated">A <see cref="ISet{T}" /> specifying all already calculated objects.</param>
    /// <param name="logger">A <see cref="ILogger" /> which can be sued for logging. Defaults to null (no logging)</param>
    /// <returns>All containers as <see cref="ObjectIdent" /> in which the user is part of.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if <paramref name="start" /> is null.</exception>
    internal static ISet<ObjectIdent> GetConnectedContainers(
        SecondLevelProjectionAssignmentsUser user,
        ObjectIdent start,
        ISet<ObjectIdent> alreadyCalculated = null,
        ILogger logger = null)
    {
        logger.EnterMethod();

        if (start == null)
        {
            throw new ArgumentNullException(nameof(start));
        }

        alreadyCalculated ??= new HashSet<ObjectIdent>();

        logger.LogDebugMessage(
            "Calculating connected containers for user {userId} with start {objectId} ({objectType})",
            LogHelpers.Arguments(user.ProfileId, start.Id, start.Type));

        ISet<ObjectIdent> parents = user.Assignments
            .Where(a => a.Profile.Id == start.Id)
            .Select(a => a.Parent)
            .ToHashSet();

        if (logger.IsEnabledFor(LogLevel.Debug))
        {
            logger.LogDebugMessage(
                "Calculating connected containers for {amountOfParent} parents",
                LogHelpers.Arguments(string.Join(", ", parents.Select(p => p.Id))));
        }

        List<ObjectIdent> parentsToCalculate = parents.Except(alreadyCalculated).ToList();
        alreadyCalculated.UnionWith(parents);

        parents.UnionWith(
            parentsToCalculate
                .SelectMany(p => GetConnectedContainers(user, p, alreadyCalculated)));

        if (user.Containers.FirstOrDefault(c => c.Id == start.Id) is SecondLevelAssignmentFunction container)
        {
            parents.Add(new ObjectIdent(container.RoleId, ObjectType.Role));
            parents.Add(new ObjectIdent(container.OrganizationId, ObjectType.Organization));
        }

        return logger.ExitMethod(parents);
    }

    /// <summary>
    ///     Calculates all active memberships of the given assignment user.
    /// </summary>
    /// <param name="user">The user of which to calculate the active assignments.</param>
    /// <param name="logger">A <see cref="ILogger" /> which can be sued for logging. Defaults to null (no logging)</param>
    /// <returns>All containers as <see cref="ObjectIdent" /> in which the user is part of.</returns>
    public static ISet<ObjectIdent> CalculateActiveMemberships(
        this SecondLevelProjectionAssignmentsUser user,
        ILogger logger = null)
    {
        logger.EnterMethod();

        ISet<ObjectIdent> memberships = CalculateActiveMemberships(
            user,
            new ObjectIdent(user.ProfileId, ObjectType.User));

        logger.LogInfoMessage(
            "Calculated {n} memberships for user {userId}",
            LogHelpers.Arguments(memberships.Count, user.ProfileId));

        return logger.ExitMethod(memberships);
    }

    /// <summary>
    ///     Calculates the containers which are connected to the given user.
    /// </summary>
    /// <param name="user">The user to calculate.</param>
    /// <param name="logger">A <see cref="Logger" />.</param>
    /// <returns>All connected containers as <see cref="ObjectIdent" />.</returns>
    public static ISet<ObjectIdent> GetConnectedContainers(
        this SecondLevelProjectionAssignmentsUser user,
        ILogger logger = null)
    {
        logger.EnterMethod();

        ISet<ObjectIdent> containers = GetConnectedContainers(
            user,
            new ObjectIdent(user.ProfileId, ObjectType.User));

        logger.LogInfoMessage(
            "Calculated {n} memberships for user {userId}",
            LogHelpers.Arguments(containers.Count, user.ProfileId));

        return logger.ExitMethod(containers);
    }
}
