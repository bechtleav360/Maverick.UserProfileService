using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Internal;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="AssignmentPayload" />.
/// </summary>
public class AssignmentPayloadValidator : AbstractValidator<AssignmentPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="AssignmentPayloadValidator" />.
    /// </summary>
    public AssignmentPayloadValidator()
    {
        RuleFor(x => x.Resource)
            .NotNull()
            .SetValidator(new ObjectIdentValidator());

        ValidateObjectList(t => t.Added, DateTime.UtcNow, true);
        ValidateObjectList(t => t.Removed, DateTime.MinValue, false);

        // Checks that at least one assignment is added or removed.
        RuleFor(x => x.Added)
            .NotNull()
            .Must((x, _) => x.Added.Length >= 1 || x.Removed.Length >= 1)
            .WithMessage("At least one object id must be specified to add or remove.");

        // Checks that the same assignment is not added as well as removed.
        RuleFor(x => x)
            .Must(
                x => x.Added.All(
                    o => !x.Removed.Any(
                        r =>
                        {
                            return r.Id == o.Id
                                && r.Conditions.Any(
                                    rc => o.Conditions.Any(oc => oc.Start == rc.Start && oc.End == rc.End));
                        })))
            .WithName(x => nameof(x.Added))
            .WithMessage("One or more assignments to add match the assignments to remove. Condition was also checked.");
    }

    private void ValidateObjectList(
        Expression<Func<AssignmentPayload, ConditionObjectIdent[]>> selector,
        DateTime conditionDateTime,
        bool checkOverlappingConditions)
    {
        // Checks that lists are not empty and validates elements in the list .
        RuleFor(selector)
            .Must(x => x != null)
            .ForEach(
                t =>
                    t.NotNull().SetValidator(new ConditionObjectIdentValidator(conditionDateTime)));

        // Checks that no conditions are specified for assignments between container profiles.
        RuleFor(x => x)
            .Must(
                x => !selector.Compile()
                    .Invoke(x)
                    .Any(
                        c => IsContainerProfileType(c.Type)
                            && c.Conditions.Any(ca => ca.Start != null || ca.End != null)))
            .When(x => x.Resource != null && IsContainerProfileType(x.Resource.Type))
            .WithName(_ => selector.GetMember().Name)
            .WithMessage("It is not allowed to attach conditions to assignments between container profiles.");

        // Checks whether multiple assignments have been made for an object. Must be combined as one assignment.
        RuleFor(selector)
            .ForEach(
                x => x
                    .Must((n, m) => n.Count(t => t.Id == m.Id) <= 1)
                    .WithMessage(
                        "Assignments have been defined several times for the same id. Must be combined into one assignment "));

        // Checks whether assignments have been defined with the same conditions. 
        RuleFor(selector)
            .ForEach(
                x => x
                    .Must(
                        (n, m) =>
                        {
                            return n
                                .Where(c => c.Id == m.Id)
                                .SelectMany(c => c.Conditions)
                                .GroupBy(
                                    t => new
                                    {
                                        t.Start,
                                        t.End
                                    })
                                .All(c => c.Count() <= 1);
                        })
                    .WithMessage("Assignments with the same conditions were defined."));

        // Checks whether assignments have been defined with overlapping conditions. 
        if (checkOverlappingConditions)
        {
            RuleFor(selector)
                .ForEach(
                    x => x
                        .Must(
                            (n, m) =>
                            {
                                List<RangeCondition> conditions = n
                                    .Where(c => c.Id == m.Id)
                                    .SelectMany(c => c.Conditions)
                                    .ToList();

                                if (conditions.Count <= 1)
                                {
                                    return true;
                                }

                                return !conditions.Any(
                                    c => conditions.Count(
                                            c2 =>
                                                GetDateTime(c.Start, true) < GetDateTime(c2.End)
                                                && GetDateTime(c.End) >= GetDateTime(c2.End))
                                        > 1);
                            })
                        .WithMessage("Assignments with overlapping constraints have been created."));
        }

        //Checks that the assignment is not done between the same object.
        RuleFor(selector)
            .Must((x, y) => y.All(o => o.Id != x.Resource?.Id))
            .WithMessage("The resource of the assignments matches one of the objects in the assignments.");

        // Checks that the assignments to an object are of the same type. Users, groups and organizations are considered as one type.
        RuleFor(selector)
            .Must(
                x =>
                    x
                        .Select(xa => GetObjectTypeForValidation(xa.Type))
                        .Distinct()
                        .Count()
                    < 2)
            .WithMessage(
                "Assignments between objects must be of the same type. Hint: Subtypes of profiles are considered as one type.");
    }

    private DateTime GetDateTime(DateTime? date, bool start = false)
    {
        return date ?? (start ? DateTime.MinValue : DateTime.MaxValue);
    }

    private protected static bool IsContainerProfileType(ObjectType objectType)
    {
        return objectType == ObjectType.Group || objectType == ObjectType.Organization;
    }

    private protected static ObjectType GetObjectTypeForValidation(ObjectType type)
    {
        if (type == ObjectType.Group || type == ObjectType.Organization || type == ObjectType.User)
        {
            return ObjectType.Profile;
        }

        return type;
    }
}
