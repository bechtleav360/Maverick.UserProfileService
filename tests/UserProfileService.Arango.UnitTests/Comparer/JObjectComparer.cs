using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Arango.UnitTests.Comparer
{
    /// <summary>
    ///     A Comparer for the type <see cref="JObject" />
    /// </summary>
    public class JObjectComparer : IEqualityComparer<JObject>
    {
        /// <inheritdoc />
        public bool Equals([AllowNull] JObject x, [AllowNull] JObject y)
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

            return Convert.ToString(x).Equals(Convert.ToString(y));
        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] JObject obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj);

            return hashCode.ToHashCode();
        }
    }
}
