using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForOrganizationView: TestingEqualityComparerForEntitiesBase<OrganizationView>
    {
        public TestingEqualityComparerForOrganizationView(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }      

        protected override string GetId(OrganizationView input)
        {
            return input.Id;
        }

        protected override bool EqualsInternally(OrganizationView x, OrganizationView y)
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
                AddOutput(Math.Abs(x.Weight - y.Weight) < double.Epsilon, x, y, g => g.Weight),
                AddOutput(x.IsSystem == y.IsSystem, x, y, g => g.IsSystem),
                AddOutput(x.Source == y.Source, x, y, g => g.Source),
                AddOutput(x.ChildrenCount == y.ChildrenCount, x, y, g => g.ChildrenCount),
                AddOutput(x.HasChildren == y.HasChildren, x, y, g => g.HasChildren));
        }
        

        public override int GetHashCode(OrganizationView obj)
        {    unchecked
            {

                int hashCode = obj.Name != null ? obj.Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.DisplayName != null ? obj.DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ExternalIds != null ? obj.ExternalIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)obj.Kind;
                hashCode = (hashCode * 397) ^ obj.CreatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UpdatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.TagUrl != null ? obj.TagUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.IsMarkedForDeletion.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.SynchronizedAt != null ? obj.SynchronizedAt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)obj.Weight;
                hashCode = (hashCode * 397) ^ (obj.ImageUrl != null ? obj.ImageUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.IsSystem.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.ChildrenCount.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.HasChildren.GetHashCode();
                return hashCode;
            }
        }
    }
}
