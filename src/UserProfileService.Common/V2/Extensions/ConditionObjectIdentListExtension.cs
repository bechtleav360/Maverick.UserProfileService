using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="ICollection{T}" />s.
/// </summary>
public static class ConditionObjectIdentListExtension
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
}
