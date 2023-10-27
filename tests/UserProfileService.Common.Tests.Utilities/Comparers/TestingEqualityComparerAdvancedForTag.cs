using System;
using Maverick.UserProfileService.Models.RequestModels;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerAdvancedForTag : TestingEqualityComparerForEntitiesBase<Tag>
    {
        public TestingEqualityComparerAdvancedForTag(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(Tag input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            Tag x,
            Tag y)
        {
            return IsTrue(
                AddOutput(x.Id == y.Id, x, y, g => g.Id),
                AddOutput(x.Name == y.Name, x, y, g => g.Name),
                AddOutput(x.Type == y.Type, x, y, g => g.Type));
        }

        public override int GetHashCode(Tag obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Type);

            return hashCode.ToHashCode();
        }
    }
}
