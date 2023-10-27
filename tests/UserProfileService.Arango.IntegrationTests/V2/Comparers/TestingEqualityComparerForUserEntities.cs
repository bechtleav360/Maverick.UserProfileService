using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Tests.Utilities.Comparers;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.Comparers
{
    /// <summary>
    ///     Used to compare user instances and user entity instances.
    /// </summary>
    public sealed class TestingEqualityComparerForUserEntities : TestingEqualityComparerForUsers
    {
        internal TestingEqualityComparerForUserEntities(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override bool ShallCheckTypeEquality()
        {
            return false;
        }

        protected override string GetId(User input)
        {
            return input?.Id;
        }

        protected override bool TryExtractSpecificEntityMembers(User user, out List<Member> members)
        {
            if (!(user is UserEntityModel entity))
            {
                members = default;

                return false;
            }

            members = entity.MemberOf?
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
