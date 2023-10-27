using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Common.V2.Extensions;

internal static class CursorExtensions
{
    internal static CursorState AdjustLastItemCount(this CursorState old)
    {
        if (old?.Id == null)
        {
            return new CursorState();
        }

        bool hasMore = old.LastItem + 2 * old.PageSize < old.TotalAmount;
        int lastItem = old.LastItem + old.PageSize;

        old.HasMore = hasMore;
        old.LastItem = lastItem;

        return old;
    }
}
