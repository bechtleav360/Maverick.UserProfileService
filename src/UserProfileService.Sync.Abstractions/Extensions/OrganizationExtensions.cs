using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.Abstraction.Extensions;

/// <summary>
///     Contains some extensions method for the class <see cref="OrganizationSync" />
/// </summary>
public static class OrganizationExtensions
{
    private static void DeleteAssignments(
        ObjectRelation objectRelation,
        string id,
        IList<RangeCondition> conditionsToBeRemove)
    {
        if (objectRelation.Conditions == null || !objectRelation.Conditions.Any())
        {
            return;
        }

        if (objectRelation.MaverickId != id)
        {
            return;
        }

        if (conditionsToBeRemove == null || !conditionsToBeRemove.Any())
        {
            return;
        }

        foreach (RangeCondition cond in conditionsToBeRemove)
        {
            RangeCondition conditionToRemove =
                objectRelation.Conditions.FirstOrDefault(c => c.Start == cond.Start && c.End == cond.End);

            if (conditionToRemove != null)
            {
                objectRelation.Conditions.Remove(conditionToRemove);
            }
        }
    }

    /// <summary>
    ///     Add an object relation to the current organization.
    /// </summary>
    /// <param name="organization"> The current organization.</param>
    /// <param name="relation"> The object relation (slimmed down organization) that should be add to the organization.</param>
    /// <exception cref="ArgumentNullException"> Will be thrown if the current organization is null.</exception>
    public static void AddObjectRelation(this OrganizationSync organization, ObjectRelation relation)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (relation == null)
        {
            return;
        }

        if (organization.RelatedObjects == null)
        {
            organization.RelatedObjects = new List<ObjectRelation>
            {
                relation
            };

            return;
        }

        if (organization.RelatedObjects.All(r => r.MaverickId != relation.MaverickId))
        {
            organization.RelatedObjects.Add(relation);
        }
    }

    /// <summary>
    ///     Delete an object relation to the current organization.
    /// </summary>
    /// <param name="organization"> The current organization.</param>
    /// <param name="relationId">
    ///     The id of object relation (slimmed down organization)
    ///     that should be deleted from the current organization.
    /// </param>
    /// <param name="type"> The kind of the relation that is being deleted (<see cref="AssignmentType" /></param>
    /// <exception cref="ArgumentNullException"> Will be thrown if the current organization is null.</exception>
    public static void DeleteObjectRelation(
        this OrganizationSync organization,
        string relationId,
        AssignmentType type)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (string.IsNullOrWhiteSpace(relationId))
        {
            throw new ArgumentException(nameof(relationId));
        }

        organization.RelatedObjects?.RemoveAll(r => r.MaverickId == relationId && r.AssignmentType == type);
    }

    /// <summary>
    ///     Delete an object relation to the current organization.
    /// </summary>
    /// <param name="organization"> The current organization.</param>
    /// <param name="relationId">
    ///     The id of object relation (slimmed down organization)
    ///     that should be deleted from the current organization.
    /// </param>
    /// <exception cref="ArgumentNullException"> Will be thrown if the current organization is null.</exception>
    public static void DeleteObjectRelation(
        this OrganizationSync organization,
        string relationId)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (string.IsNullOrWhiteSpace(relationId))
        {
            throw new ArgumentException(nameof(relationId));
        }

        organization.RelatedObjects?.RemoveAll(r => r.MaverickId == relationId);
    }

    /// <summary>
    ///     Remove the conditions of an object relation to the current organization
    /// </summary>
    /// <param name="organization"> The current organization</param>
    /// <param name="relationId">
    ///     The id of object relation (slimmed down organization)
    ///     whose given conditions are being removed from the current organization.
    /// </param>
    /// <param name="conditions">   The conditions of the assignment that should be removed</param>
    /// <exception cref="ArgumentException"> Will be thrown if the current organization is null.</exception>
    public static void RemoveObjectRelation(
        this OrganizationSync organization,
        string relationId,
        IList<RangeCondition> conditions)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (string.IsNullOrWhiteSpace(relationId))
        {
            throw new ArgumentException("The relation id should not be null or whitespace", nameof(relationId));
        }

        if (organization.RelatedObjects == null
            || !organization.RelatedObjects.Any()
            || conditions == null
            || !conditions.Any())
        {
            return;
        }

        organization.RelatedObjects.ForEach(r => DeleteAssignments(r, relationId, conditions));
    }
}
