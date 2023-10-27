using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     A group which is derived from the groupBasic.
    ///     It is used for storing all needed properties for a group profile.
    /// </summary>
    public class Group : GroupBasic
    {
        /// <summary>
        ///     The Ids of the groups where the group is a member of.
        /// </summary>
        public IList<Member> MemberOf { get; set; } = new List<Member>();

        /// <summary>
        ///     The ids from the members ( users or groups ).
        /// </summary>
        public IList<Member> Members { get; set; } = new List<Member>();

        private bool IsMembersEqual(IList<Member> other)
        {
            if (Members == null)
            {
                return other == null;
            }

            if (Equals(Members, other))
            {
                return true;
            }

            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(Members, other))
            {
                return true;
            }

            return other.SequenceEqual(Members);
        }

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
        ///     Determines whether the specified <see cref="Group" /> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="Group" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current one, otherwise, <c>false</c>.</returns>
        protected bool Equals(Group other)
        {
            return base.Equals(other)
                && IsMembersEqual(other.Members)
                && IsMemberOfEqual(other.MemberOf);
        }

        /// <summary>
        ///     Compare the current instance to another one using a specified equality comparer.
        /// </summary>
        /// <param name="other">The other instance to compare with.</param>
        /// <param name="equalityComparer">The equality comparer to be used.</param>
        /// <returns><c>true</c>, if the instances are equal, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="equalityComparer" /> is <c>null</c></exception>
        public bool Equals(
            Group other,
            IEqualityComparer<Group> equalityComparer)
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

            return Equals((Group)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Members != null ? Members.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
