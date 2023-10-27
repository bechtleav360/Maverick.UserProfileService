using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Tests.Utilities.Comparers;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.Comparers
{
    /// <summary>
    ///     Is used to compare group and group entity instances.
    /// </summary>
    public sealed class TestingEqualityComparerForGroupEntities : TestingEqualityComparerForGroups
    {
        internal TestingEqualityComparerForGroupEntities(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(Group input)
        {
            return input?.Id;
        }

        protected override bool TryExtractSpecificEntityMembers(Group group, out List<Member> members)
        {
            if (!(group is GroupEntityModel entity))
            {
                members = default;

                return false;
            }

            members = entity.Members?
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

            return true;
        }
    }
}
