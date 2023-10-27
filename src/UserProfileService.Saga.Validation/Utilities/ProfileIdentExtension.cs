using System;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Contains extension methods related to <see cref="ProfileIdent" />.
/// </summary>
public static class ProfileIdentExtension
{
    /// <summary>
    ///     Transforms a <see cref="ProfileIdent" /> to an <see cref="ObjectIdent" />.
    /// </summary>
    /// <param name="profileIdent">The <see cref="ProfileIdent" /> that should be transformed.</param>
    /// <returns>An <see cref="ObjectIdent" /> that was transformed.</returns>
    public static ObjectIdent ToObjectIdent(this ProfileIdent profileIdent)
    {
        if (profileIdent == null)
        {
            throw new ArgumentNullException(nameof(profileIdent));
        }

        return new ObjectIdent(profileIdent.Id, profileIdent.ProfileKind.ToObjectType());
    }
}
