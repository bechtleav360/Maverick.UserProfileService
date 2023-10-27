using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    /// <summary>
    ///     Is used to compare organization and organization entity instances.
    /// </summary>
    public class TestingEqualityComparerForOrganizations : TestingEqualityComparerForEntitiesBase<Organization>
    {
        public TestingEqualityComparerForOrganizations(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        private bool CompareMembers(Organization x, Organization y)
        {
            List<Member> membersX = ExtractMembers(x);

            List<Member> membersY = ExtractMembers(y);

            return membersX.SequenceEqual(membersY, new TestingEqualityComparerForMembers());
        }

        private bool CompareMemberOf(Organization x, Organization y)
        {
            List<Member> memberOfX = ExtractMemberOf(x);

            List<Member> memberOfY = ExtractMemberOf(y);

            return memberOfX.SequenceEqual(memberOfY, new TestingEqualityComparerForMembers());
        }

        private static List<Member> ExtractMemberOf(Organization organization)
        {
            List<Member> memberOf = organization.MemberOf
                .OrderBy(m => m.Id)
                .ToList();

            return memberOf;
        }

        protected virtual bool TryExtractSpecificEntityMembers(Organization user, out List<Member> members)
        {
            members = null;

            return false;
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(Organization input)
        {
            return input?.Id;
        }

        protected override bool EqualsInternally(
            Organization x,
            Organization y)
        {
            return
                IsTrue(
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
                    AddOutput(x.IsSystem == y.IsSystem, x, y, g => g.IsSystem),
                    AddOutput(CompareMembers(x, y), x, y, g => g.Members),
                    AddOutput(CompareMemberOf(x, y), x, y, g => g.MemberOf));
        }

        protected List<Member> ExtractMembers(Organization organization)
        {
            if (TryExtractSpecificEntityMembers(organization, out List<Member> specificMembers))
            {
                return specificMembers;
            }

            List<Member> members = organization.Members
                .OrderBy(m => m.Id)
                .ToList();

            return members;
        }

        public override int GetHashCode(Organization obj)
        {
            unchecked
            {
                int hashCode = obj.CustomPropertyUrl != null ? obj.CustomPropertyUrl.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
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

                return hashCode;
            }
        }
    }
}
