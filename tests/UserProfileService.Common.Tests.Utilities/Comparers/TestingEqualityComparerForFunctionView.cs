using System;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForFunctionView : TestingEqualityComparerForEntitiesBase<FunctionView>
    {
        public TestingEqualityComparerForFunctionView(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        private bool RolesEquals(
            RoleBasic x,
            RoleBasic y)
        {
            var comparer = new TestingEqualityComparerForRoleBasic(OutputHelper);

            return comparer.Equals(x, y);
        }

        protected override string GetId(FunctionView input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            FunctionView x,
            FunctionView y)
        {
            return IsTrue(
                    AddOutput(x.Id == y.Id, x, y, f => f.Id)
                    && AddOutput(x.Name == y.Name, x, y, f => f.Name)
                    && AddOutput(x.Type == y.Type, x, y, f => f.Type)
                    &&
                    // TODO: Problems because of model change
                    //  AddOutput(ComparingHelpers.CompareTagLists(x.OrganizationId, y.OrganizationId), x, y, f => f.OrganizationId) &&
                    AddOutput(x.CreatedAt.Equals(y.CreatedAt), x, y, f => f.CreatedAt)
                    && AddOutput(x.UpdatedAt.Equals(y.UpdatedAt), x, y, f => f.UpdatedAt)
                    && RolesEquals(x.Role, y.Role))
                && AddOutput(
                    ComparingHelpers.CompareLinkedProfiles(x.LinkedProfiles, y.LinkedProfiles),
                    x,
                    y,
                    f => f.LinkedProfiles)
                && AddOutput(
                    x.ExternalIds.CompareExternalIds(y.ExternalIds, OutputHelper),
                    x,
                    y,
                    f => f.ExternalIds);
        }

        public override int GetHashCode(FunctionView obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add((int)obj.Type);
            // TODO: Problems because of model change
            hashCode.Add(obj.OrganizationId);
            hashCode.Add(obj.Role);
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.UpdatedAt);
            hashCode.Add(obj.LinkedProfiles);

            return hashCode.ToHashCode();
        }
    }
}
