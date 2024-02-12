using System;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Tests.Utilities.Comparers;
using Xunit;
using Xunit.Sdk;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    internal static class OrganizationEntityToOrganizationComparer
    {
        public static void Compare(
            OrganizationEntityModel entity,
            OrganizationBasic orgUnitBasic)
        {
            if (entity == null && orgUnitBasic == null)
            {
                return;
            }

            // only both SUTs should be null, otherwise an exception should be thrown
            Assert.NotNull(entity);
            Assert.NotNull(orgUnitBasic);

            bool basicPropertyCheck =
                entity.Id == orgUnitBasic.Id
                && entity.Name == orgUnitBasic.Name
                && entity.DisplayName == orgUnitBasic.DisplayName
                && entity.ExternalIds.CompareExternalIds(orgUnitBasic.ExternalIds)
                && entity.Kind == orgUnitBasic.Kind
                && entity.CreatedAt.Equals(orgUnitBasic.CreatedAt)
                && entity.UpdatedAt.Equals(orgUnitBasic.UpdatedAt)
                && entity.IsMarkedForDeletion == orgUnitBasic.IsMarkedForDeletion
                && Nullable.Equals(entity.SynchronizedAt, orgUnitBasic.SynchronizedAt)
                && entity.Weight == orgUnitBasic.Weight
                && entity.IsSystem == orgUnitBasic.IsSystem;

            if (!basicPropertyCheck)
            {
                throw new XunitException(
                    "Comparison between entity and organization failed. Basic properties not equal.");
            }
        }

        public static void Compare(
            OrganizationEntityModel entity,
            Organization organization)
        {
            Compare(entity, (OrganizationBasic)organization);

            if ((entity.Members != null && organization.Members == null)
                || (entity.Members == null && organization.Members != null))
            {
                throw new XunitException(
                    "Comparison between entity and organization failed. One members list is null, but not the other.");
            }

            if (entity.Members != null && organization.Members != null)
            {
                Assert.Equal(entity.Members.Count, organization.Members.Count);

                Assert.Equal(
                    entity.Members.Select(
                            m => new Member
                            {
                                Id = m.Id,
                                DisplayName = m.DisplayName,
                                Kind = m.Kind,
                                Name = m.Name
                            })
                        .ToList(),
                    organization.Members,
                    new TestingEqualityComparerForMembers());
            }

            if ((entity.MemberOf != null && organization.MemberOf == null)
                || (entity.MemberOf == null && organization.MemberOf != null))
            {
                throw new XunitException(
                    "Comparison between entity and organization failed. One member-of list is null, but not the other.");
            }

            if (entity.MemberOf != null && organization.MemberOf != null)
            {
                Assert.Equal(entity.MemberOf.Count, organization.MemberOf.Count);

                Assert.Equal(
                    entity.MemberOf.Select(
                            m => new Member
                            {
                                Id = m.Id,
                                DisplayName = m.DisplayName,
                                Kind = m.Kind,
                                Name = m.Name
                            })
                        .ToList(),
                    organization.MemberOf,
                    new TestingEqualityComparerForMembers());
            }

            Assert.True(
                string.IsNullOrEmpty(organization.CustomPropertyUrl),
                "organization.CustomPropertyUrl must be null or empty!");
        }

        public static void Compare(
            OrganizationEntityModel entity,
            OrganizationView organization)
        {
            Compare(entity, (OrganizationBasic)organization);

            Assert.Equal(
                entity.Members?.Count(m => m.Kind == ProfileKind.Organization) > 0,
                organization.HasChildren);

            if ((entity.Tags != null && organization.Tags == null)
                || (entity.Members == null && organization.Tags != null))
            {
                throw new XunitException(
                    "Comparison between entity and organization failed. One tags list is null, but not the other.");
            }

            if (entity.Tags != null && organization.Tags != null)
            {
                Assert.Equal(entity.Tags.Count, organization.Tags.Count);
            }
        }
    }
}
