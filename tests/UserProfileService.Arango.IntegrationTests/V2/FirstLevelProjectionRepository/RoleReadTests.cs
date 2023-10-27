using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class RoleReadTests : ArangoFirstLevelRepoTestBase
    {
        /// <summary>
        ///     Data for <see cref="Get_all_relevant_objects_because_of_property_change_of_role_should_work" />.
        /// </summary>
        public static IList<object[]> RolePropertyChangeData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MitarbeiterRole,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged
                                             .AbteilungTiefbauMitarbeitFunction,
                            ObjectType.Function),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            ObjectType.User),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            ObjectType.User),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MitarbeiterRole,
                            ObjectType.Role)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.LesenRole,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MinisteriumLesenFunction,
                            ObjectType.Function),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LesenRole,
                            ObjectType.Role)
                    }
                }
            };

        /// <summary>
        ///     Data for <see cref="Get_all_children_of_role_should_work" />.
        /// </summary>
        public static IList<object[]> GetChildrenData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MitarbeiterRole,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            FirstLevelMemberRelation.IndirectMember)
                    }
                },
                new object[] { HierarchyTestData.PropertyChanged.LesenRole, Array.Empty<string>() }
            };

        /// <summary>
        ///     Data for <see cref="Get_direct_member_of_role_should_work" />.
        /// </summary>
        public static IList<object[]> GetContainerMembersData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.ContainerMembers.LernenRoleId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.ContainerMembers.PraktikantenGroupId,
                            ObjectType.Group),
                        new ObjectIdent(HierarchyTestData.ContainerMembers.MichaelRookyUserId, ObjectType.User)
                    }
                },
                new object[] { HierarchyTestData.ContainerMembers.NixRoleId, Array.Empty<ObjectIdent>() }
            };

        private readonly FirstLevelProjectionFixture _fixture;

        public RoleReadTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            FirstLevelProjectionRole role = await repo.GetRoleAsync(RoleReadTestData.ReadRole.Id);

            role.Should().BeEquivalentTo(RoleReadTestData.ReadRole);
        }

        [Fact]
        public async Task Get_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.GetRoleAsync("this-role-does-not-exists"));
        }

        [Theory]
        [MemberData(nameof(RolePropertyChangeData))]
        public async Task Get_all_relevant_objects_because_of_property_change_of_role_should_work(
            string profileId,
            ObjectIdent[] objects)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<ObjectIdentPath> profiles =
                await repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    new ObjectIdent(profileId, ObjectType.Role));

            profiles.Should().NotBeNull().And.BeEquivalentTo(objects);
        }

        [Fact]
        public async Task Get_all_relevant_objects_because_of_property_change_of_no_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    new ObjectIdent("this-role-does-not-exists", ObjectType.Role)));
        }

        [Theory]
        [MemberData(nameof(GetChildrenData))]
        public async Task Get_all_children_of_role_should_work(
            string roleId,
            RelationTestModel[] expectedChildren)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionProfile[] expectedProfiles = await Task.WhenAll(
                expectedChildren.Select(c => GetDocumentObjectAsync<IFirstLevelProjectionProfile>(c.ProfileId)));

            List<FirstLevelRelationProfile> expectedChildrenRelation = expectedChildren.Select(
                    profile => new FirstLevelRelationProfile(
                        expectedProfiles.FirstOrDefault(p => p.Id == profile.ProfileId),
                        profile.Relation))
                .ToList();

            IList<FirstLevelRelationProfile> profiles =
                await repo.GetAllChildrenAsync(new ObjectIdent(roleId, ObjectType.Role));

            profiles.Should().BeEquivalentTo(expectedChildrenRelation);
        }

        [Theory]
        [MemberData(nameof(GetContainerMembersData))]
        public async Task Get_direct_member_of_role_should_work(
            string roleId,
            ObjectIdent[] expectedMembers)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<ObjectIdent> profiles =
                await repo.GetContainerMembersAsync(new ObjectIdent(roleId, ObjectType.Role));

            profiles.Should().NotBeNull().And.BeEquivalentTo(expectedMembers);
        }

        [Fact]
        public async Task Get_all_children_of_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetAllChildrenAsync(new ObjectIdent("this-profile-does-not-exists", ObjectType.Profile)));
        }
        // GetAllChildren -> not relevant?
        // GetRoleAsync
    }
}
