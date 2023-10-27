using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Class to support the comparison of objects of type <see cref="ExternalIdentifier" /> for equality.
/// </summary>
public class ExternalIdentifierEqualityComparer : IEqualityComparer<ExternalIdentifier>
{
    /// <inheritdoc />
    public bool Equals(ExternalIdentifier x, ExternalIdentifier y)
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

        return x.Id == y.Id && x.Source == y.Source && x.IsConverted == y.IsConverted;
    }

    /// <inheritdoc />
    public int GetHashCode(ExternalIdentifier obj)
    {
        return HashCode.Combine(obj.Id, obj.Source);
    }
}
