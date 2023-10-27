using System;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForLinkedRoleObject : TestingEqualityComparerForEntitiesBase<LinkedRoleObject>
    {
        public TestingEqualityComparerForLinkedRoleObject(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override string GetId(LinkedRoleObject input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            LinkedRoleObject x,
            LinkedRoleObject y)
        {
            return IsTrue(
                AddOutput(x.Id == y.Id, x, y, f => f.Id)
                && AddOutput(x.Name == y.Name, x, y, f => f.Name)
                && AddOutput(x.Type == y.Type, x, y, f => f.Type)
                && AddOutput(x.IsActive == y.IsActive, x, y, f => f.IsActive)
                && AddOutput(
                    ComparingHelpers.CompareRangeConditions(x.Conditions, y.Conditions),
                    x,
                    y,
                    f => f.Conditions));
        }

        public override int GetHashCode(LinkedRoleObject obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Type);
            hashCode.Add(obj.Conditions);
            hashCode.Add(obj.IsActive);

            return hashCode.ToHashCode();
        }
    }
}
