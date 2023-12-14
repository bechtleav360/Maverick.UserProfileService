using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.StateMachine.Utilities;

internal class RangeConditionEqualityComparer : IEqualityComparer<RangeCondition>
{
    public bool Equals(RangeCondition? x, RangeCondition? y)
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

        DateTime? xStart = x.Start?.ToUniversalTime();
        DateTime? xEnd = x.End?.ToUniversalTime();
        DateTime? yStart = y.Start?.ToUniversalTime();
        DateTime? yEnd = y.End?.ToUniversalTime();

        return Nullable.Equals(xEnd, yEnd)
            && Nullable.Equals(xStart, yStart);
    }

    public int GetHashCode(RangeCondition obj)
    {
        return HashCode.Combine(obj.End?.ToUniversalTime(), obj.Start?.ToUniversalTime());
    }
}