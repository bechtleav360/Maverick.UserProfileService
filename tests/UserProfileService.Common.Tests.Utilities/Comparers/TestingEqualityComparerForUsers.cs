using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    /// <summary>
    ///     Used to compare user instances and user entity instances.
    /// </summary>
    public class TestingEqualityComparerForUsers : TestingEqualityComparerForEntitiesBase<User>
    {
        public TestingEqualityComparerForUsers(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected virtual bool TryExtractSpecificEntityMembers(User user, out List<Member> members)
        {
            members = null;

            return false;
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(User input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            User x,
            User y)
        {
            return IsTrue(
                AddOutput(x.Id == y.Id, x, y, u => u.Id),
                AddOutput(x.Name == y.Name, x, y, u => u.Name),
                AddOutput(x.DisplayName == y.DisplayName, x, y, u => u.DisplayName),
                AddOutput(x.ExternalIds.CompareExternalIds(y.ExternalIds, OutputHelper), x, y, u => u.ExternalIds),
                AddOutput(x.Kind == y.Kind, x, y, u => u.Kind),
                AddOutput(x.CreatedAt.Equals(y.CreatedAt), x, y, u => u.CreatedAt),
                AddOutput(x.UpdatedAt.Equals(y.UpdatedAt), x, y, u => u.UpdatedAt),
                AddOutput(x.UserName == y.UserName, x, y, u => u.UserName),
                AddOutput(x.FirstName == y.FirstName, x, y, u => u.FirstName),
                AddOutput(x.LastName == y.LastName, x, y, u => u.LastName),
                AddOutput(x.Email == y.Email, x, y, u => u.Email),
                AddOutput(Nullable.Equals(x.SynchronizedAt, y.SynchronizedAt), x, y, u => u.SynchronizedAt),
                AddOutput(x.UserStatus == y.UserStatus, x, y, u => u.UserStatus),
                AddOutput(
                    ExtractMemberInformation(x).SequenceEqual(ExtractMemberInformation(y)),
                    x,
                    y,
                    u => u.MemberOf));
        }

        protected List<Member> ExtractMemberInformation(User user)
        {
            if (TryExtractSpecificEntityMembers(user, out List<Member> specificMembers))
            {
                return specificMembers;
            }

            return user.MemberOf
                .OrderBy(m => m.Id)
                .ToList();
        }

        public override int GetHashCode(User obj)
        {
            unchecked
            {
                int hashCode = obj.Id != null ? obj.Id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.DisplayName != null ? obj.DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ExternalIds != null ? obj.ExternalIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)obj.Kind;
                hashCode = (hashCode * 397) ^ obj.CreatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UpdatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.TagUrl != null ? obj.TagUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.UserName != null ? obj.UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.FirstName != null ? obj.FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.LastName != null ? obj.LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Email != null ? obj.Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.SynchronizedAt != null ? obj.SynchronizedAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ImageUrl != null ? obj.ImageUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.UserStatus != null ? obj.UserStatus.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
