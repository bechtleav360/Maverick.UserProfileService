using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.BasicModels
{
    /// <summary>
    ///     A simple representation of the user profile.
    /// </summary>
    public class UserBasic : IProfile
    {
        /// <inheritdoc cref="IProfile.CreatedAt" />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc cref="IProfile.DisplayName" />
        [Searchable]
        [DefaultFilterValue]
        [Modifiable]
        public string DisplayName { get; set; }

        /// <summary>
        ///     The domain of the user.
        /// </summary>
        [Modifiable]
        public string Domain { get; set; }

        /// <summary>
        ///     The email addresses of the user.
        /// </summary>
        [Searchable]
        [Modifiable]
        public string Email { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        [Modifiable]
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     The first name of the user.
        /// </summary>
        [Searchable]
        [Modifiable]
        public string FirstName { set; get; }

        /// <inheritdoc cref="IProfile.Id" />
        public string Id { get; set; }

        /// <summary>
        ///     The image url of the group.
        /// </summary>
        [UriRedirection("profiles/{Id}/image")]
        public string ImageUrl { set; get; }

        /// <inheritdoc cref="IProfile.Kind" />
        public ProfileKind Kind { get; set; } = ProfileKind.User;

        /// <summary>
        ///     The last name of the user.
        /// </summary>
        [Searchable]
        [Modifiable]
        public string LastName { set; get; }

        /// <inheritdoc cref="IProfile.Name" />
        [Searchable]
        [Modifiable]
        public string Name { get; set; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <inheritdoc cref="IProfile.SynchronizedAt" />
        public DateTime? SynchronizedAt { set; get; }

        /// <inheritdoc cref="IProfile.TagUrl" />
        [UriRedirection("users/{Id}/tags")]
        public string TagUrl { get; set; }

        /// <inheritdoc cref="IProfile.UpdatedAt" />
        public DateTime UpdatedAt { get; set; }
        
        /// <inheritdoc cref="IProfile.Path" />
        public IList<string> Paths { get; set; }
        
        /// <summary>
        ///     The name of the user.
        /// </summary>
        [Searchable]
        [Modifiable]
        public string UserName { set; get; }

        /// <summary>
        ///     The image url of the group.
        /// </summary>
        [Modifiable]
        public string UserStatus { set; get; }

        /// <summary>
        ///     Determines whether the specified <see cref="UserBasic" /> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="UserBasic" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current one, otherwise, <c>false</c>.</returns>
        protected bool Equals(UserBasic other)
        {
            return Id == other.Id
                && Name == other.Name
                && DisplayName == other.DisplayName
                && ExternalIds.Equals(other.ExternalIds)
                && Kind == other.Kind
                && CreatedAt.Equals(other.CreatedAt)
                && UpdatedAt.Equals(other.UpdatedAt)
                && TagUrl == other.TagUrl
                && UserName == other.UserName
                && FirstName == other.FirstName
                && LastName == other.LastName
                && Email == other.Email
                && Nullable.Equals(SynchronizedAt, other.SynchronizedAt)
                && ImageUrl == other.ImageUrl
                && UserStatus == other.UserStatus;
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

            return Equals((UserBasic)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            unchecked
            {
                int hashCode = Id != null ? Id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExternalIds != null ? ExternalIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Kind;
                hashCode = (hashCode * 397) ^ CreatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ UpdatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (TagUrl != null ? TagUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Email != null ? Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SynchronizedAt != null ? SynchronizedAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ImageUrl != null ? ImageUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserStatus != null ? UserStatus.GetHashCode() : 0);

                return hashCode;
            }
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }
}
