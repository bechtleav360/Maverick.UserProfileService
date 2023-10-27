using System;
using Maverick.UserProfileService.Models.BasicModels;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForGroupBasic : TestingEqualityComparerForEntitiesBase<GroupBasic>
    {
        public TestingEqualityComparerForGroupBasic(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(GroupBasic input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            GroupBasic x,
            GroupBasic y)
        {
            return IsTrue(
                AddOutput(x.Id == y.Id, x, y, g => g.Id),
                AddOutput(x.Name == y.Name, x, y, g => g.Name),
                AddOutput(x.DisplayName == y.DisplayName, x, y, g => g.DisplayName),
                AddOutput(x.ExternalIds.CompareExternalIds(y.ExternalIds, OutputHelper), x, y, g => g.ExternalIds),
                AddOutput(x.Kind == y.Kind, x, y, g => g.Kind),
                AddOutput(x.CreatedAt.Equals(y.CreatedAt), x, y, g => g.CreatedAt),
                AddOutput(x.UpdatedAt.Equals(y.UpdatedAt), x, y, g => g.UpdatedAt),
                AddOutput(x.IsMarkedForDeletion == y.IsMarkedForDeletion, x, y, g => g.IsMarkedForDeletion),
                AddOutput(Nullable.Equals(x.SynchronizedAt, y.SynchronizedAt), x, y, g => g.SynchronizedAt),
                AddOutput(x.Weight == y.Weight, x, y, g => g.Weight),
                AddOutput(x.IsSystem == y.IsSystem, x, y, g => g.IsSystem));
        }

        public override int GetHashCode(GroupBasic obj)
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
            hashCode.Add(obj.IsMarkedForDeletion);
            hashCode.Add(obj.SynchronizedAt);
            hashCode.Add(obj.Weight);
            hashCode.Add(obj.ImageUrl);
            hashCode.Add(obj.IsSystem);

            return hashCode.ToHashCode();
        }
    }
}
