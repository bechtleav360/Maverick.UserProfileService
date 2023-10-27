using System;
using Maverick.UserProfileService.Models.BasicModels;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForUserBasic : TestingEqualityComparerForEntitiesBase<UserBasic>
    {
        public TestingEqualityComparerForUserBasic(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(UserBasic input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            UserBasic x,
            UserBasic y)
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
                AddOutput(x.UserStatus == y.UserStatus, x, y, u => u.UserStatus));
        }

        public override int GetHashCode(UserBasic obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.DisplayName);
            hashCode.Add(obj.ExternalIds);
            hashCode.Add((int)obj.Kind);
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.UpdatedAt);
            hashCode.Add(obj.TagUrl);
            hashCode.Add(obj.UserName);
            hashCode.Add(obj.FirstName);
            hashCode.Add(obj.LastName);
            hashCode.Add(obj.Email);
            hashCode.Add(obj.SynchronizedAt);
            hashCode.Add(obj.ImageUrl);
            hashCode.Add(obj.UserStatus);

            return hashCode.ToHashCode();
        }
    }
}
