using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    /// <summary>
    ///     Contains extension methods related to entity models of the UPS API like UserBasic.
    /// </summary>
    public static class UpsApiEntityExtensions
    {
        public static UserBasic Clone(this UserBasic user)
        {
            return new UserBasic
            {
                Name = user.Name,
                Id = user.Id,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                SynchronizedAt = user.SynchronizedAt,
                Kind = user.Kind,
                DisplayName = user.DisplayName,
                Email = user.Email,
                FirstName = user.FirstName,
                ImageUrl = user.ImageUrl,
                LastName = user.LastName,
                Source = user.Source,
                TagUrl = user.TagUrl,
                UserName = user.UserName,
                UserStatus = user.UserStatus,
                ExternalIds = user.ExternalIds?.Select(Clone).ToList()
            };
        }

        public static GroupBasic Clone(this GroupBasic group)
        {
            return new GroupBasic
            {
                Name = group.Name,
                Id = group.Id,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                SynchronizedAt = group.SynchronizedAt,
                Kind = group.Kind,
                DisplayName = group.DisplayName,
                ImageUrl = group.ImageUrl,
                Source = group.Source,
                TagUrl = group.TagUrl,
                ExternalIds = group.ExternalIds?.Select(Clone).ToList(),
                IsMarkedForDeletion = group.IsMarkedForDeletion,
                IsSystem = group.IsSystem,
                Weight = group.Weight
            };
        }

        public static OrganizationBasic Clone(this OrganizationBasic organization)
        {
            return new OrganizationBasic
            {
                Name = organization.Name,
                Id = organization.Id,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt,
                SynchronizedAt = organization.SynchronizedAt,
                Kind = organization.Kind,
                DisplayName = organization.DisplayName,
                ImageUrl = organization.ImageUrl,
                Source = organization.Source,
                TagUrl = organization.TagUrl,
                ExternalIds = organization.ExternalIds?.Select(Clone).ToList(),
                IsMarkedForDeletion = organization.IsMarkedForDeletion,
                IsSystem = organization.IsSystem,
                Weight = organization.Weight,
                IsSubOrganization = organization.IsSubOrganization
            };
        }

        public static ExternalIdentifier Clone(this ExternalIdentifier identifier)
        {
            return new ExternalIdentifier
            {
                IsConverted = identifier.IsConverted,
                Id = identifier.Id,
                Source = identifier.Source
            };
        }

        /// <summary>
        ///     Sets the range condition list in a linked object to a default value, if not set or empty, and returns it again.
        /// </summary>
        public static TLinkedObject NormalizeRangeConditions<TLinkedObject>(
            this TLinkedObject source,
            DateTime? referenceDate = null)
            where TLinkedObject : ILinkedObject
        {
            if (source == null)
            {
                return source;
            }

            if (source.Conditions != null
                && source.Conditions.Count > 0)
            {
                source.IsActive = source.Conditions.AnyActive(referenceDate);

                return source;
            }

            source.Conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            source.IsActive = true;

            return source;
        }

        /// <summary>
        ///     Sets the range condition list in a linked object to a default value, if not set or empty, and returns it again.
        /// </summary>
        public static TLinkedObject SetIsActive<TLinkedObject>(
            this TLinkedObject source,
            bool isActive)
            where TLinkedObject : ILinkedObject
        {
            if (source == null)
            {
                return default;
            }

            source.IsActive = isActive;

            return source;
        }
    }
}
