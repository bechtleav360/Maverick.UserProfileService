using System;
using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Class to support the comparison of objects of type <see cref="KeyProperties" /> for equality.
/// </summary>
public class KeyPropertiesEqualityComparer : IEqualityComparer<KeyProperties>
{
    /// <inheritdoc />
    public bool Equals(KeyProperties x, KeyProperties y)
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

        return new ExternalIdentifierEqualityComparer().Equals(x, y); // && Equals(x.Filter, y.Filter);
    }

    /// <inheritdoc />
    public int GetHashCode(KeyProperties obj)
    {
        int baseHashCode = new ExternalIdentifierEqualityComparer().GetHashCode(obj);

        return HashCode.Combine(baseHashCode, obj.Filter != null ? obj.Filter.GetHashCode() : 0);
    }
}
