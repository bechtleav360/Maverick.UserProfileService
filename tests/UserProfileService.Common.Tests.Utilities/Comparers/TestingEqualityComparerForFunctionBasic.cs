using System;
using Maverick.UserProfileService.Models.BasicModels;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForFunctionBasic : TestingEqualityComparerForEntitiesBase<FunctionBasic>
    {
        public TestingEqualityComparerForFunctionBasic(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        private bool RolesEquals(
            RoleBasic x,
            RoleBasic y)
        {
            var comparer = new TestingEqualityComparerForRoleBasic(OutputHelper);

            return comparer.Equals(x, y);
        }

        private bool OrganizationsEquals(OrganizationBasic x, OrganizationBasic y)
        {
            var comparer = new TestingEqualityComparerForOrganizationBasic(OutputHelper);

            return comparer.Equals(x, y);
        }

        protected override string GetId(FunctionBasic input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            FunctionBasic x,
            FunctionBasic y)
        {
            return IsTrue(
                    AddOutput(x.Id == y.Id, x, y, f => f.Id)
                    && AddOutput(x.Name == y.Name, x, y, f => f.Name)
                    && AddOutput(x.Type == y.Type, x, y, f => f.Type)
                    && AddOutput(x.OrganizationId == y.OrganizationId, x, y, f => f.OrganizationId)
                    && AddOutput(x.CreatedAt.Equals(y.CreatedAt), x, y, f => f.CreatedAt)
                    && AddOutput(x.UpdatedAt.Equals(y.UpdatedAt), x, y, f => f.UpdatedAt)
                    && RolesEquals(x.Role, y.Role))
                && OrganizationsEquals(x.Organization, y.Organization);
        }

        public override int GetHashCode(FunctionBasic obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add((int)obj.Type);
            hashCode.Add(obj.Organization);
            hashCode.Add(obj.OrganizationId);
            hashCode.Add(obj.Role);
            hashCode.Add(obj.RoleId);
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.UpdatedAt);

            return hashCode.ToHashCode();
        }
    }
}
