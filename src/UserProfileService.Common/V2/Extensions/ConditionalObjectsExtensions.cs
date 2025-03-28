﻿using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="ConditionObjectIdent"/> and <see cref="ConditionAssignment"/>.
/// </summary>
public static class ConditionalObjectsExtensions
{
    /// <summary>
    ///     Add default condition to  <see cref="ConditionObjectIdent" />s, if none condition exists.
    /// </summary>
    /// <param name="conditionObjects"><see cref="ConditionObjectIdent" /> to check and processed.</param>
    public static void AddDefaultConditions(this ICollection<ConditionObjectIdent> conditionObjects)
    {
        foreach (ConditionObjectIdent conditionObject in conditionObjects)
        {
            conditionObject.Conditions ??= new[] { new RangeCondition() };

            if (!conditionObject.Conditions.Any())
            {
                conditionObject.Conditions = new[] { new RangeCondition() };
            }
        }
    }

    /// <summary>
    ///     Converts a collection of <see cref="ConditionObjectIdent"/> instances to a flat list of <see cref="ConditionAssignment"/>.
    /// </summary>
    /// <param name="conditionalObjects">The collection of conditional objects.</param>
    /// <returns>A flat list of condition assignments.</returns>
    public static IEnumerable<ConditionAssignment> AsFlatAssignmentList(this IEnumerable<ConditionObjectIdent> conditionalObjects)
    {
        return conditionalObjects?
                .Where(o => o != null)
                .SelectMany(
                    o => o.Conditions?
                            .Select(
                                c =>
                                    new ConditionAssignment
                                    {
                                        Id = o.Id,
                                        Conditions = c != null
                                            ? new[] { new RangeCondition(c.Start, c.End) }
                                            : null
                                    })
                        ?? new[]
                        {
                            new ConditionAssignment
                            {
                                Id = o.Id
                            }
                        })
            ?? Enumerable.Empty<ConditionAssignment>();
    }

    /// <summary>
    ///     Converts a collection of <see cref="ConditionAssignment"/> instances to a flat list.
    /// </summary>
    /// <param name="source">The source collection of condition assignments.</param>
    /// <returns>A flat list of condition assignments.</returns>
    public static IEnumerable<ConditionAssignment> AsFlatList(
        this IEnumerable<ConditionAssignment> source)
    {
        return source?
                .Where(o => o != null)
                .SelectMany(
                    o => o.Conditions?
                            .Select(
                                c =>
                                    new ConditionAssignment
                                    {
                                        Id = o.Id,
                                        Conditions = c != null
                                            ? new[] { new RangeCondition(c.Start, c.End) }
                                            : null
                                    })
                        ?? new[]
                        {
                            new ConditionAssignment
                            {
                                Id = o.Id
                            }
                        })
            ?? Enumerable.Empty<ConditionAssignment>();
    }
}
