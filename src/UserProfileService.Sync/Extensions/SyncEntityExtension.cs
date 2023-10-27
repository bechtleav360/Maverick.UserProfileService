using System.Linq;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Extensions;

internal static class SyncEntityExtension
{
    /// <summary>
    ///     Compare the internal and external ids of the given objects.
    /// </summary>
    /// <param name="source">Source sync model to be checked.</param>
    /// <param name="target">Target sync model to be checked.</param>
    /// <returns>True if internal or any external ids match, otherwise false.</returns>
    public static bool CompareInternalAndExternalId(this ISyncModel source, ISyncModel target)
    {
        if (source.Id == target.Id)
        {
            return true;
        }

        return source.ExternalIds.Any(
            sei =>
                target.ExternalIds.Any(tei => tei.Id == sei.Id && tei.Source == sei.Source));
    }
}
