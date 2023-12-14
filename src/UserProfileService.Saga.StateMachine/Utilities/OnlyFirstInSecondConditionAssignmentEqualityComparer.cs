using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.StateMachine.Utilities;

// the condition sequences must not be equal - only all items in the first one, must be part of the second one
internal class OnlyFirstInSecondConditionAssignmentEqualityComparer : IEqualityComparer<ConditionAssignment>
{
    public bool Equals(ConditionAssignment? x, ConditionAssignment? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return EqualsUNotOrdered(x.Conditions, y.Conditions)
            && x.Id == y.Id;
    }

    private static bool EqualsUNotOrdered(
        RangeCondition[] x,
        RangeCondition[] y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        var comparer = new RangeConditionEqualityComparer();

        foreach (RangeCondition condition in x)
        {
            if (!y.Contains(condition, comparer))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ConditionAssignment? obj)
    {
        if (obj == null)
        {
            return 0;
        }

        var hash = new HashCode();
        hash.Add(obj.Id);

        if (obj.Conditions == null)
        {
            return hash.ToHashCode();
        }

        var comparer = new RangeConditionEqualityComparer();
        
        foreach (RangeCondition condition in obj.Conditions)
        {
            hash.Add(condition, comparer);
        }

        return hash.ToHashCode();
    }
}
