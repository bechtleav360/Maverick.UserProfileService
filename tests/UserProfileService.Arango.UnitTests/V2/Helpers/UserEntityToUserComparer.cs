using System;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Tests.Utilities.Comparers;
using Xunit;
using Xunit.Sdk;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    internal static class UserEntityToUserComparer
    {
        public static void Compare(
            UserEntityModel entity,
            UserBasic userBasic)
        {
            if (entity == null && userBasic == null)
            {
                return;
            }

            // only both SUTs should be null, otherwise an exception should be thrown
            Assert.NotNull(entity);
            Assert.NotNull(userBasic);

            bool basicPropertyCheck =
                entity.Id == userBasic.Id
                && entity.Name == userBasic.Name
                && entity.DisplayName == userBasic.DisplayName
                && entity.ExternalIds.CompareExternalIds(userBasic.ExternalIds)
                && entity.Kind == userBasic.Kind
                && entity.CreatedAt.Equals(userBasic.CreatedAt)
                && entity.UpdatedAt.Equals(userBasic.UpdatedAt)
                && entity.UserName == userBasic.UserName
                && entity.FirstName == userBasic.FirstName
                && entity.LastName == userBasic.LastName
                && entity.Email == userBasic.Email
                && Nullable.Equals(entity.SynchronizedAt, userBasic.SynchronizedAt)
                && entity.UserStatus == userBasic.UserStatus;

            if (!basicPropertyCheck)
            {
                throw new XunitException("Comparison between entity and user failed. Basic properties not equal.");
            }
        }

        public static void Compare(
            UserEntityModel entity,
            User user)
        {
            Compare(entity, (UserBasic)user);

            if ((entity.MemberOf == null && user.MemberOf != null)
                || (entity.MemberOf != null && user.MemberOf == null))
            {
                throw new XunitException(
                    "Comparison between entity and user list failed. One member-of list is null, but not the other.");
            }

            if (entity.MemberOf != null && user.MemberOf != null)
            {
                Assert.Equal(entity.MemberOf.Count, user.MemberOf.Count);

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
                    user.MemberOf,
                    new TestingEqualityComparerForMembers());
            }

            Assert.True(string.IsNullOrEmpty(user.CustomPropertyUrl), "user.CustomPropertyUrl must be null or empty!");
        }
    }
}
