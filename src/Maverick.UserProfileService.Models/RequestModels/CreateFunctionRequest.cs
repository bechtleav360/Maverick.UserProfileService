using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The request to create a function.
    /// </summary>
    public class CreateFunctionRequest
    {
        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     The desired name of the function.
        /// </summary>
        [Required]
        public string Name { set; get; }

        /// <summary>
        ///     The Id of the organization <see cref="OrganizationBasic" />
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string OrganizationId { get; set; }

        /// <summary>
        ///     Defines the id of the role that should be related to the function.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string RoleId { set; get; }

        /// <summary>
        ///     Tags to assign to function.
        /// </summary>
        public IList<TagAssignment> Tags { set; get; } = new List<TagAssignment>();

        /// <summary>
        ///     Determines whether the specified <see cref="CreateFunctionRequest" /> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="CreateFunctionRequest" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current one, otherwise, <c>false</c>.</returns>
        protected bool Equals(CreateFunctionRequest other)
        {
            return Name == other.Name && OrganizationId == other.OrganizationId && RoleId == other.RoleId;
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

            return Equals((CreateFunctionRequest)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (OrganizationId != null ? OrganizationId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RoleId != null ? RoleId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExternalIds != null ? ExternalIds.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
