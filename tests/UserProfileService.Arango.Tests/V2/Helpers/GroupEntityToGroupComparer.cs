using System;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Tests.Utilities.Comparers;
using Xunit;
using Xunit.Sdk;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    internal static class GroupEntityToGroupComparer
    {
        public static void Compare(
            GroupEntityModel entity,
            GroupBasic groupBasic)
        {
            if (entity == null && groupBasic == null)
            {
                return;
            }

            // only both SUTs should be null, otherwise an exception should be thrown
            Assert.NotNull(entity);
            Assert.NotNull(groupBasic);

            bool basicPropertyCheck =
                entity.Id == groupBasic.Id
                && entity.Name == groupBasic.Name
                && entity.DisplayName == groupBasic.DisplayName
                && entity.ExternalIds.CompareExternalIds(groupBasic.ExternalIds)
                && entity.Kind == groupBasic.Kind
                && entity.CreatedAt.Equals(groupBasic.CreatedAt)
                && entity.UpdatedAt.Equals(groupBasic.UpdatedAt)
                && entity.IsMarkedForDeletion == groupBasic.IsMarkedForDeletion
                && Nullable.Equals(entity.SynchronizedAt, groupBasic.SynchronizedAt)
                && ComparingHelpers.AboutEqual(entity.Weight, groupBasic.Weight)
                && entity.IsSystem == groupBasic.IsSystem;

            if (!basicPropertyCheck)
            {
                throw new XunitException("Comparison between entity and group failed. Basic properties not equal.");
            }
        }

        public static void Compare(
            GroupEntityModel entity,
            Group group)
        {
            Compare(entity, (GroupBasic)group);

            if ((entity.Members != null && group.Members == null) || (entity.Members == null && group.Members != null))
            {
                throw new XunitException(
                    "Comparison between entity and group failed. One members list is null, but not the other.");
            }

            if (entity.Members != null && group.Members != null)
            {
                Assert.Equal(entity.Members.Count, group.Members.Count);

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
                    group.Members,
                    new TestingEqualityComparerForMembers());
            }

            if ((entity.MemberOf != null && group.MemberOf == null)
                || (entity.MemberOf == null && group.MemberOf != null))
            {
                throw new XunitException(
                    "Comparison between entity and group failed. One member-of list is null, but not the other.");
            }

            if (entity.MemberOf != null && group.MemberOf != null)
            {
                Assert.Equal(entity.MemberOf.Count, group.MemberOf.Count);

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
                    group.MemberOf,
                    new TestingEqualityComparerForMembers());
            }
        }
    }
}
