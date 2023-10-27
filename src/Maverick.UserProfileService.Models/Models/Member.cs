using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Short user object for the group list object.
    /// </summary>
    public class Member
    {
        /// <summary>
        ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
        ///     membership of this <see cref="Member" /> is always active.
        /// </summary>
        public IList<RangeCondition> Conditions { get; set; }

        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     The identifier of the group member.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     Determines if any condition of the list is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     Determines if user, group or organization.
        /// </summary>
        public ProfileKind Kind { get; set; }

        /// <summary>
        ///     The name of the group member.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     Determines if user, group or organization. It refers to the value of <see cref="Kind" /> to be still downward
        ///     compatible.
        /// </summary>
        public ProfileKind ProfileKind => Kind;

        /// <summary>
        ///     Determines whether the specified <see cref="Member" /> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="Member" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current one, otherwise, <c>false</c>.</returns>
        protected bool Equals(Member other)
        {
            return Id == other.Id && Name == other.Name && DisplayName == other.DisplayName && Kind == other.Kind;
        }

        /// <inheritdoc cref="object" />
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

            return Equals((Member)obj);
        }

        /// <inheritdoc cref="object" />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Id != null ? Id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Kind;

                return hashCode;
            }
        }

        /// <inheritdoc cref="object" />
        public override string ToString()
        {
            return Id;
        }
    }
}
