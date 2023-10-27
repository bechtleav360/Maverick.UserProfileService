using System;
using System.Linq;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Abstraction.Extensions;

/// <summary>
///     Contains extensions method for <see cref="ISyncProfile" />
/// </summary>
public static class SyncProfileExtensions
{
    /// <summary>
    ///     Cleans related object of profile after assignment deletion
    /// </summary>
    /// <param name="profile">The <see cref="ISyncProfile" /></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void CleanProfileAfterDeleteAssignments(this ISyncProfile profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        profile.RelatedObjects.RemoveAll(r => r.Conditions == null || !r.Conditions.Any());
    }
}
