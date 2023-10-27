using System;
using Maverick.UserProfileService.Models.BasicModels;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForRoleBasic : TestingEqualityComparerForEntitiesBase<RoleBasic>
    {
        public TestingEqualityComparerForRoleBasic(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override string GetId(RoleBasic input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            RoleBasic x,
            RoleBasic y)
        {
            return
                IsTrue(
                    AddOutput(x.Id == y.Id, x, y, r => r.Id),
                    AddOutput(x.Name == y.Name, x, y, r => r.Name),
                    AddOutput(x.Type == y.Type, x, y, r => r.Type),
                    AddOutput(x.Description == y.Description, x, y, r => r.Description),
                    AddOutput(
                        ComparingHelpers.CompareStringLists(x.Permissions, y.Permissions),
                        x,
                        y,
                        r => r.Permissions),
                    AddOutput(x.IsSystem == y.IsSystem, x, y, r => r.IsSystem));
        }

        public override int GetHashCode(RoleBasic obj)
        {
            return HashCode.Combine(
                obj.Id,
                obj.Name,
                obj.Description,
                (int)obj.Type,
                obj.Permissions,
                obj.IsSystem);
        }
    }
}
