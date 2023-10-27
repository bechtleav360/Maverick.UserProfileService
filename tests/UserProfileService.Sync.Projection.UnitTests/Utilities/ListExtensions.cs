using System.Collections.Generic;

namespace UserProfileService.Sync.Projection.UnitTests.Utilities;

public static class ListExtensions
{
    public static List<TElem> AddConditionally<TElem>(
        this List<TElem> source,
        TElem newElement,
        bool condition)
    {
        if (source == null
            || newElement == null
            || !condition)
        {
            return source;
        }

        source.Add(newElement);

        return source;
    }
}
