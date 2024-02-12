using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Tests.Utilities.Comparers;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    /// <summary>
    ///     Is used to compare group and group entity instances.
    /// </summary>
    public sealed class TestingEqualityComparerForGroups : IEqualityComparer<Group>
    {
        private bool CompareMembers(Group x, Group y)
        {
            (List<Member> members, List<Member> memberOf) memberInfoX = ExtractMemberInformation(x);

            (List<Member> members, List<Member> memberOf) memberInfoY = ExtractMemberInformation(y);

            return memberInfoX.members.SequenceEqual(memberInfoY.members)
                && memberInfoX.memberOf.SequenceEqual(memberInfoY.memberOf);
        }

        private static (List<Member> members, List<Member> memberOf) ExtractMemberInformation(Group group)
        {
            if (group is GroupEntityModel entity)
            {
                List<Member> membersEntity = entity.Members?
                        .Select(
                            p => new Member
                            {
                                DisplayName = p.DisplayName,
                                Id = p.Id,
                                Kind = p.Kind,
                                Name = p.Name
                            })
                        .OrderBy(m => m.Id)
                        .ToList()
                    ?? new List<Member>();

                List<Member> memberOfEntity = entity.MemberOf?
                        .Select(
                            p => new Member
                            {
                                DisplayName = p.DisplayName,
                                Id = p.Id,
                                Kind = p.Kind,
                                Name = p.Name
                            })
                        .OrderBy(m => m.Id)
                        .ToList()
                    ?? new List<Member>();

                return (membersEntity, memberOfEntity);
            }

            List<Member> members = group.Members
                .OrderBy(m => m.Id)
                .ToList();

            List<Member> memberOf = group.MemberOf
                .OrderBy(m => m.Id)
                .ToList();

            return (members, memberOf);
        }

        public bool Equals(
            Group x,
            Group y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Id == y.Id
                && x.Name == y.Name
                && x.DisplayName == y.DisplayName
                && x.ExternalIds.Equals(y.ExternalIds)
                && x.Kind == y.Kind
                && x.CreatedAt.Equals(y.CreatedAt)
                && x.UpdatedAt.Equals(y.UpdatedAt)
                && x.TagUrl == y.TagUrl
                && x.IsMarkedForDeletion == y.IsMarkedForDeletion
                && Nullable.Equals(x.SynchronizedAt, y.SynchronizedAt)
                && ComparingHelpers.AboutEqual(x.Weight, y.Weight)
                && x.ImageUrl == y.ImageUrl
                && x.IsSystem == y.IsSystem
                && CompareMembers(x, y);
        }

        public int GetHashCode(Group obj)
        {
            unchecked
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

                return hashCode;
            }
        }
    }
}
