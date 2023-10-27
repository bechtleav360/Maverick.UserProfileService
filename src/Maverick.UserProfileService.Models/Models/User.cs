using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     The complete profile of a user.
    /// </summary>
    public class User : UserBasic
    {
        /// <summary>
        ///     A link where to find the custom properties for a group.
        /// </summary>
        [UriRedirection("profiles/{Id}/customProperties")]
        public string CustomPropertyUrl { set; get; }

        /// <summary>
        ///     Assignment status of a user.
        /// </summary>
        public IList<Member> MemberOf { set; get; } = new List<Member>();

        private bool IsMemberOfEqual(IList<Member> other)
        {
            if (MemberOf == null)
            {
                return other == null;
            }

            if (Equals(MemberOf, other))
            {
                return true;
            }

            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(MemberOf, other))
            {
                return true;
            }

            return other.SequenceEqual(MemberOf);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="User" /> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="User" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current one, otherwise, <c>false</c>.</returns>
        protected bool Equals(User other)
        {
            return base.Equals(other)
                && IsMemberOfEqual(other.MemberOf)
                && CustomPropertyUrl == other.CustomPropertyUrl;
        }

        /// <summary>
        ///     Compare the current instance to another one using a specified equality comparer.
        /// </summary>
        /// <param name="other">The other instance to compare with.</param>
        /// <param name="equalityComparer">The equality comparer to be used.</param>
        /// <returns><c>true</c>, if the instances are equal, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="equalityComparer" /> is <c>null</c></exception>
        public bool Equals(
            User other,
            IEqualityComparer<User> equalityComparer)
        {
            if (equalityComparer == null)
            {
                throw new ArgumentNullException(nameof(equalityComparer));
            }

            return equalityComparer.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((User)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (MemberOf != null ? MemberOf.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CustomPropertyUrl != null ? CustomPropertyUrl.GetHashCode() : 0);

                return hashCode;
            }
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }
}
