using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Maverick.UserProfileService.Models.BasicModels
{
    /// <summary>
    ///     The base model of a group.
    /// </summary>
    public class GroupBasic : IContainerProfile
    {
        /// <inheritdoc />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        [DefaultFilterValue]
        [Searchable]
        [Modifiable]
        [NotEmptyOrWhitespace]
        public string DisplayName { get; set; }

        /// <inheritdoc />
        [Modifiable]
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        ///     The image url of the group.
        /// </summary>
        [UriRedirection("profiles/{Id}/image")]
        public string ImageUrl { set; get; }

        /// <summary>
        ///     A boolean value that is true if the resource should be deleted but it is not possible cause of underlying
        ///     dependencies.
        /// </summary>
        [Modifiable]
        public bool IsMarkedForDeletion { set; get; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        [Modifiable]
        public bool IsSystem { set; get; }

        /// <inheritdoc />
        public ProfileKind Kind { get; set; } = ProfileKind.Group;

        /// <inheritdoc />
        [Searchable]
        [Modifiable]
        public string Name { get; set; }

        /// <summary>
        ///     The source name where the entity was transferred from (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <inheritdoc />
        [UriRedirection("groups/{Id}/tags")]
        public string TagUrl { get; set; }

        /// <inheritdoc />
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        ///     The weight of a group profile that can be used to sort a result set.
        /// </summary>
        [Modifiable]
        public double Weight { set; get; } = 0;

        /// <summary>
        ///     Determines whether the specified <see cref="GroupBasic" /> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="GroupBasic" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current one, otherwise, <c>false</c>.</returns>
        protected bool Equals(GroupBasic other)
        {
            return Id == other.Id
                && Name == other.Name
                && DisplayName == other.DisplayName
                && ExternalIds.Equals(other.ExternalIds)
                && Kind == other.Kind
                && CreatedAt.Equals(other.CreatedAt)
                && UpdatedAt.Equals(other.UpdatedAt)
                && TagUrl == other.TagUrl
                && IsMarkedForDeletion == other.IsMarkedForDeletion
                && Nullable.Equals(SynchronizedAt, other.SynchronizedAt)
                && Math.Abs(Weight - other.Weight) < double.Epsilon
                && ImageUrl == other.ImageUrl
                && IsSystem == other.IsSystem
                && Source == other.Source;
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

            return Equals((GroupBasic)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = CreatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExternalIds != null ? ExternalIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ImageUrl != null ? ImageUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsMarkedForDeletion.GetHashCode();
                hashCode = (hashCode * 397) ^ IsSystem.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Kind;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SynchronizedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (TagUrl != null ? TagUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UpdatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ Weight.GetHashCode();

                return hashCode;
            }
        }
    }
}
