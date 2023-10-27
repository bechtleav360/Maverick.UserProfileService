using System;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class
        TestingEqualityComparerForLinkedFunctionObject : TestingEqualityComparerForEntitiesBase<LinkedFunctionObject>
    {
        public TestingEqualityComparerForLinkedFunctionObject(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override string GetId(LinkedFunctionObject input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            LinkedFunctionObject x,
            LinkedFunctionObject y)
        {
            return IsTrue(
                AddOutput(x.Id == y.Id, x, y, f => f.Id)
                && AddOutput(x.Name == y.Name, x, y, f => f.Name)
                && AddOutput(x.Type == y.Type, x, y, f => f.Type)
                && AddOutput(x.OrganizationId == y.OrganizationId, x, y, f => f.OrganizationId));
        }

        public override int GetHashCode(LinkedFunctionObject obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Type);
            hashCode.Add(obj.OrganizationId);

            return hashCode.ToHashCode();
        }
    }
}
