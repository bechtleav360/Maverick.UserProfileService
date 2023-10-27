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
    public class FunctionReadTests : ArangoFirstLevelRepoTestBase
    {
        /// <summary>
        ///     Data for <see cref="Get_all_relevant_objects_because_of_property_change_of_function_should_work" />.
        /// </summary>
        public static IList<object[]> FunctionPropertyChangeData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.PropertyChanged.AbteilungTiefbauMitarbeitFunction,
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
                            ObjectType.User)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MinisteriumLesenFunction,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MinisteriumLesenFunction,
                            ObjectType.Function)
                    }
                }
            };

        /// <summary>
        ///     Data for <see cref="Get_all_children_of_function_should_work" />.
        /// </summary>
        public static IList<object[]> GetChildrenData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.PropertyChanged.AbteilungTiefbauMitarbeitFunction,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            FirstLevelMemberRelation.DirectMember),
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
                new object[] { HierarchyTestData.PropertyChanged.MinisteriumLesenFunction, Array.Empty<string>() }
            };

        public static IList<object[]> GetMembersOfFunction = new List<object[]>
        {
            new object[]
            {
                HierarchyTestData.ContainerMembers
                                 .EinrabeitenLernenFunctionId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .NixKoennerGroupId,
                        ObjectType.Group),
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .StreberGroupId,
                        ObjectType.Group),
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .GregorSchnellLernerUserId,
                        ObjectType.User),
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .MichaelDickschaedelUserId,
                        ObjectType.User)
                }
            },
            new object[]
            {
                HierarchyTestData.ContainerMembers
                                 .NixFuerNixenFunctionId,
                Array.Empty<ObjectIdent>()
            }
        };

        private readonly FirstLevelProjectionFixture _fixture;

        public FunctionReadTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            FirstLevelProjectionFunction function = await repo.GetFunctionAsync(FunctionReadTestData.ReadFunction.Id);

            function.Should()
                .BeEquivalentTo(
                    FunctionReadTestData.ReadFunction,
                    o => o.Excluding(f => f.Organization).Excluding(f => f.Role));

            Assert.NotNull(function.Organization);
            Assert.NotNull(function.Role);
        }

        [Fact]
        public async Task Get_not_existing_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetFunctionAsync("this-function-does-not-exists"));
        }

        [Theory]
        [MemberData(nameof(FunctionPropertyChangeData))]
        public async Task Get_all_relevant_objects_because_of_property_change_of_function_should_work(
            string profileId,
            ObjectIdent[] objects)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<ObjectIdentPath> profiles =
                await repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    new ObjectIdent(profileId, ObjectType.Function));

            profiles.Should().NotBeNull().And.BeEquivalentTo(objects);
        }

        [Fact]
        public async Task Get_all_relevant_objects_because_of_property_change_of_no_existing_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    new ObjectIdent("this-function-does-not-exists", ObjectType.Function)));
        }

        [Theory]
        [MemberData(nameof(GetMembersOfFunction))]
        public async Task Get_all_members_of_function_should_work(
            string functionId,
            ObjectIdent[] expectedChildren)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<ObjectIdent> profiles =
                await repo.GetContainerMembersAsync(new ObjectIdent(functionId, ObjectType.Function));

            profiles.Should().NotBeNull().And.BeEquivalentTo(expectedChildren);
        }

        [Theory]
        [MemberData(nameof(GetChildrenData))]
        public async Task Get_all_children_of_function_should_work(
            string functionId,
            RelationTestModel[] expectedChildren)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionProfile[] expectedProfiles = await Task.WhenAll(
                expectedChildren.Select(c => GetDocumentObjectAsync<IFirstLevelProjectionProfile>(c.ProfileId)));

            List<FirstLevelRelationProfile> expectedRelationProfiles = expectedChildren.Select(
                    pr => new FirstLevelRelationProfile(
                        expectedProfiles.FirstOrDefault(p => p.Id == pr.ProfileId),
                        pr.Relation))
                .ToList();

            IList<FirstLevelRelationProfile> profiles =
                await repo.GetAllChildrenAsync(new ObjectIdent(functionId, ObjectType.Function));

            profiles.Should().NotBeNull().And.BeEquivalentTo(expectedRelationProfiles);
        }

        [Theory]
        [InlineData(HierarchyTestData.ContainerMembers.AbteilungNixFuerNixOrgUnitId, 1)]
        [InlineData(HierarchyTestData.ContainerMembers.AbteilungEinarbeitenOrgUnitId, 1)]
        [InlineData(FunctionWriteTestData.UpdateFunction.OrganizationId, 1)]
        public async Task Get_functions_of_organization_should_work(string organizationId, int expectedFunctions)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            ICollection<FirstLevelProjectionFunction> functions =
                await repo.GetFunctionsOfOrganizationAsync(organizationId);

            Assert.NotNull(functions);
            Assert.Equal(expectedFunctions, functions.Count);
        }
    }
}
