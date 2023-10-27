using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Newtonsoft.Json.Linq;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    /// <summary>
    ///     Contains extension methods related to entity models of the UPS API like SecondLevelProjectionUser.
    /// </summary>
    public static class SecondLevelProjectionEntityExtensions
    {
        // helper for function as second level projection model
        private static Role Clone(this Role role)
        {
            return new Role
            {
                Name = role.Name,
                Id = role.Id,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                SynchronizedAt = role.SynchronizedAt,
                Source = role.Source,
                ExternalIds = role.ExternalIds?.Select(Clone).ToList(),
                IsSystem = role.IsSystem,
                DeniedPermissions = role.DeniedPermissions?.ToList(),
                Description = role.Description,
                Permissions = role.Permissions?.ToList()
            };
        }

        // only way to combine Moq with Fluent.Assertions => Expression tree of Moq method
        // cannot contain Should().AssignableTo() calls
        public static SecondLevelProjectionUser Clone(this SecondLevelProjectionUser user)
        {
            return new SecondLevelProjectionUser
            {
                Name = user.Name,
                Id = user.Id,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                SynchronizedAt = user.SynchronizedAt,
                DisplayName = user.DisplayName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Source = user.Source,
                UserName = user.UserName,
                UserStatus = user.UserStatus,
                ExternalIds = user.ExternalIds?.Select(Clone).ToList(),
                MemberOf = user.MemberOf?.Select(Clone).ToList()
            };
        }

        public static SecondLevelProjectionGroup Clone(this SecondLevelProjectionGroup group)
        {
            return new SecondLevelProjectionGroup
            {
                Name = group.Name,
                Id = group.Id,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                SynchronizedAt = group.SynchronizedAt,
                DisplayName = group.DisplayName,
                Source = group.Source,
                ExternalIds = group.ExternalIds?.Select(Clone).ToList(),
                IsMarkedForDeletion = group.IsMarkedForDeletion,
                //IsSystem = group.IsSystem,
                Weight = group.Weight
            };
        }

        public static SecondLevelProjectionOrganization Clone(this SecondLevelProjectionOrganization organization)
        {
            return new SecondLevelProjectionOrganization
            {
                Name = organization.Name,
                Id = organization.Id,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt,
                SynchronizedAt = organization.SynchronizedAt,
                DisplayName = organization.DisplayName,
                Source = organization.Source,
                ExternalIds = organization.ExternalIds?.Select(Clone).ToList(),
                IsMarkedForDeletion = organization.IsMarkedForDeletion,
                IsSystem = organization.IsSystem,
                Weight = organization.Weight,
                IsSubOrganization = organization.IsSubOrganization
            };
        }

        public static SecondLevelProjectionFunction Clone(this SecondLevelProjectionFunction function)
        {
            return new SecondLevelProjectionFunction
            {
                Id = function.Id,
                ExternalIds = function.ExternalIds?.Select(Clone).ToList(),
                Role = function.Role?.Clone(),
                RoleId = function.RoleId,
                Organization = function.Organization?.Clone(),
                OrganizationId = function.OrganizationId,
                CreatedAt = function.CreatedAt,
                UpdatedAt = function.UpdatedAt,
                SynchronizedAt = function.SynchronizedAt,
                Source = function.Source
            };
        }

        public static ExternalIdentifier Clone(this ExternalIdentifier identifier)
        {
            return new ExternalIdentifier
            {
                Id = identifier.Id,
                Source = identifier.Source
            };
        }

        public static Member Clone(this Member member)
        {
            return new Member
            {
                Id = member.Id,
                DisplayName = member.DisplayName,
                Kind = member.Kind,
                Name = member.Name,
                ExternalIds = member.ExternalIds?.Select(Clone).ToList(),
                Conditions = member.Conditions?.Select(Clone).ToList()
            };
        }

        public static RangeCondition Clone(this RangeCondition condition)
        {
            return new RangeCondition
            {
                Start = condition.Start,
                End = condition.End
            };
        }

        // helper for function as second level projection model
        public static Organization Clone(this Organization organization)
        {
            return new Organization
            {
                Name = organization.Name,
                Id = organization.Id,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt,
                SynchronizedAt = organization.SynchronizedAt,
                DisplayName = organization.DisplayName,
                Source = organization.Source,
                ExternalIds = organization.ExternalIds?.Select(Clone).ToList(),
                IsMarkedForDeletion = organization.IsMarkedForDeletion,
                IsSystem = organization.IsSystem,
                Weight = organization.Weight,
                IsSubOrganization = organization.IsSubOrganization,
                TagUrl = organization.TagUrl
            };
        }

        /// <summary>
        ///     Merges the member data with the <paramref name="entity" /> and returns a new object of the same with merged
        ///     property set.
        /// </summary>
        /// <remarks>
        ///     This method uses Newtonsoft JObject to do the merge.
        /// </remarks>
        public static TEntity Merge<TEntity>(
            this TEntity entity,
            Member member)
        {
            JObject jObj = JObject.FromObject(entity);
            JObject jObjMember = JObject.FromObject(member);
            jObj.Merge(jObjMember);

            return jObj.ToObject<TEntity>();
        }
    }
}
