using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using AVTagType = Maverick.UserProfileService.Models.EnumModels.TagType;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class ArangoSecondLevelRepositoryTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public static IEnumerable<object[]> UpdateProfileTestArgs =>
            new List<object[]>
            {
                new object[] { ProfileTestData.UpdateGroup.GroupId, ProfileKind.Group },
                new object[] { ProfileTestData.UpdateOrganization.OrganizationId, ProfileKind.Organization },
                new object[] { ProfileTestData.UpdateUser.UserId, ProfileKind.User }
            };

        public static IEnumerable<object[]> ProfileData =>
            new List<object[]>
            {
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionUser().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionUser().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionUser().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionUser().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionGroup().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionGroup().Single() }
            };

        public ArangoSecondLevelRepositoryTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(ProfileData))]
        public async Task Create_profile_should_work(ISecondLevelProjectionProfile profile)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            await repo.CreateProfileAsync(profile);
            Thread.Sleep(2000);
            ISecondLevelProjectionProfile savedProfile = await repo.GetProfileAsync(profile.Id);

            profile.Paths = new List<string>
            {
                profile.Id
            };

            string collectionName = GetCollectionName<SecondLevelProjectionProfileVertexData>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }
                    FILTER x.ObjectId == ""{
                        profile.Id
                    }""                   
                    RETURN x";

            MultiApiResponse<SecondLevelProjectionProfileVertexData> response =
                await GetArangoClient().ExecuteQueryAsync<SecondLevelProjectionProfileVertexData>(aqlQuery);

            var expectedVertex = new SecondLevelProjectionProfileVertexData
            {
                ObjectId = profile.Id,
                RelatedProfileId = profile.Id
            };

            savedProfile.Should().BeEquivalentTo(profile);
            response.QueryResult.FirstOrDefault().Should().BeEquivalentTo(expectedVertex);
        }

        [Fact]
        public async Task Create_organization_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionOrganization organization =
                MockDataGenerator.GenerateSecondLevelProjectionOrganization().Single();

            organization.MemberOf = new List<Member>();

            await repo.CreateProfileAsync(organization);

            var dbOrganization = await GetDocumentObjectAsync<IProfileEntityModel>(organization.Id);
            var convertedOrganization = GetMapper().Map<SecondLevelProjectionOrganization>(dbOrganization);
            convertedOrganization.Should().BeEquivalentTo(organization);
        }

        [Fact]
        public async Task Create_tag_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            Tag tag = MockDataGenerator.GenerateTagAggregateModels().Single();

            await repo.CreateTagAsync(tag);

            var dbTag = await GetDocumentObjectAsync<Tag>(tag.Id);
            tag.Should().BeEquivalentTo(dbTag);
        }

        [Fact]
        public Task CalculateTags_should_work()
        {
            // todo Missing method
            return Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(ProfileData))]
        public async Task DeleteProfile_should_work(ISecondLevelProjectionProfile profile)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            await repo.CreateProfileAsync(profile);
            var dbProfile = await GetDocumentObjectAsync<IProfileEntityModel>(profile.Id);
            dbProfile.Should().NotBeNull();
            await repo.DeleteProfileAsync(profile.Id);
            await Assert.ThrowsAsync<InstanceNotFoundException>(async () => await repo.GetProfileAsync(profile.Id));
        }

        [Theory]
        [MemberData(nameof(UpdateProfileTestArgs))]
        public async Task UpdateProfile_should_work(string profileId, ProfileKind profileKind)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            string profileCollection = GetCollectionName<IProfileEntityModel>();
            IArangoDbClient client = GetArangoClient();

            ISecondLevelProjectionProfile profile
                = profileKind switch
                {
                    ProfileKind.Organization => MockDataGenerator.GenerateSecondLevelProjectionOrganization(
                        profileId,
                        $"Emmanuel {profileKind:G}",
                        2,
                        2),
                    ProfileKind.Group => MockDataGenerator.GenerateSecondLevelProjectionGroup(
                        profileId,
                        $"Emmanuel {profileKind:G}",
                        2,
                        2),
                    _ => MockDataGenerator.GenerateSecondLevelProjectionUser(
                        profileId,
                        $"Emmanuel {profileKind:G}",
                        2,
                        2)
                };

            profile.DisplayName = $"A/V Solutions - Bug Busters - {profileKind:G}";
            profile.Source = "Test";

            profile.Paths = new List<string>
            {
                "path1",
                "path2"
            };

            profile.SynchronizedAt = DateTime.Today.ToUniversalTime();
            var referenceValue = _fixture.Mapper.Map<IProfileEntityModel>(profile);

            await repo.UpdateProfileAsync(profile);

            MultiApiResponse<IProfileEntityModel> updatedProfileResponse =
                await client.ExecuteQueryAsync<IProfileEntityModel>(
                    $"FOR p IN {profileCollection} FILTER p.Id==\"{profileId}\" RETURN p");

            if (updatedProfileResponse.Error)
            {
                throw new Exception();
            }

            IProfileEntityModel updatedProfile = updatedProfileResponse.QueryResult.Single();

            updatedProfile.Should()
                .BeEquivalentTo(
                    referenceValue,
                    o =>
                        o.Excluding(p => p.Paths)
                            .Excluding(p => p.MemberOf)
                            .Excluding(p => p.SecurityAssignments)
                            .Excluding(p => p.SystemId)
                            .Excluding(p => p.TagUrl)
                            .Excluding(p => p.Tags));

            updatedProfile.Paths.Should().NotBeEquivalentTo(profile.Paths);
        }

        [Fact]
        public async Task RemoveTag_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            List<CalculatedTag> tags = MockDataGenerator.GenerateCalculatedTags(2);
            tags[0].Id = Guid.NewGuid().ToString();

            foreach (CalculatedTag tag in tags)
            {
                await repo.CreateTagAsync(GetMapper().Map<Tag>(tag));
            }

            string collectionName = _fixture.GetCollectionName<Tag>();

            await repo.RemoveTagAsync(tags[0].Id);

            var aqlQuery = @$" FOR x in {
                collectionName
            }                  
                   RETURN x";

            MultiApiResponse<Tag> response = await GetArangoClient().ExecuteQueryAsync<Tag>(aqlQuery);

            response.QueryResult.FirstOrDefault(x => x.Id == tags[0].Id).Should().BeNull();

            response.QueryResult.FirstOrDefault(x => x.Id == tags[1].Id)
                .Should()
                .BeEquivalentTo(GetMapper().Map<Tag>(tags[1]));
        }

        [Theory]
        [MemberData(nameof(ProfileData))]
        public async Task AddTagToProfile_should_work(ISecondLevelProjectionProfile profile)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            var firstTag = GetMapper().Map<Tag>(MockDataGenerator.GenerateCalculatedTags().Single());
            var secondTag = GetMapper().Map<Tag>(MockDataGenerator.GenerateCalculatedTags().Single());

            firstTag.Type = TagType.Security;

            await repo.CreateTagAsync(firstTag);
            await repo.CreateTagAsync(secondTag);

            await repo.CreateProfileAsync(profile);

            IList<TagAssignment> tags = new List<Tag>
                {
                    firstTag,
                    secondTag
                }.Select(
                    tag => new TagAssignment
                    {
                        IsInheritable = false,
                        TagDetails = tag
                    })
                .ToList();

            IList<CalculatedTag> calculatedTags = new List<Tag>
                {
                    firstTag,
                    secondTag
                }
                .Select(
                    tag => new CalculatedTag
                    {
                        IsInherited = false,
                        Name = tag.Name,
                        Id = tag.Id,
                        Type = GetMapper().Map<AVTagType>(tag.Type)
                    })
                .ToList();

            await repo.AddTagToObjectAsync(profile.Id, profile.Id, ObjectType.Profile, tags);

            string collectionName = _fixture.GetCollectionName<ISecondLevelProjectionProfile>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Id == ""{
                       profile.Id
                   }""
                   RETURN x.Tags";

            MultiApiResponse<IList<CalculatedTag>> response =
                await GetArangoClient().ExecuteQueryAsync<IList<CalculatedTag>>(aqlQuery);

            IList<CalculatedTag> result = response.QueryResult?.FirstOrDefault();

            result
                .Should()
                .BeEquivalentTo(calculatedTags);
        }

        [Theory]
        [MemberData(nameof(ProfileData))]
        public async Task RemoveTagToProfile_should_work(ISecondLevelProjectionProfile profile)
        {
            // Arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            var firstTag = GetMapper().Map<Tag>(MockDataGenerator.GenerateCalculatedTags().Single());
            var secondTag = GetMapper().Map<Tag>(MockDataGenerator.GenerateCalculatedTags().Single());

            await repo.CreateTagAsync(firstTag);
            await repo.CreateTagAsync(secondTag);

            await repo.CreateProfileAsync(profile);

            IList<CalculatedTag> tags = new List<Tag>
                {
                    firstTag,
                    secondTag
                }.Select(
                    tag => new CalculatedTag
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        IsInherited = false,
                        Type = GetMapper()
                            .Map<AVTagType>(tag.Type)
                    })
                .ToList();

            IList<TagAssignment> tagsAssignment = new List<Tag>
                {
                    firstTag,
                    secondTag
                }
                .Select(
                    tag => new TagAssignment
                    {
                        TagDetails = tag,
                        IsInheritable = false
                    })
                .ToList();

            await repo.AddTagToObjectAsync(profile.Id, profile.Id, ObjectType.Profile, tagsAssignment);

            // Act
            await repo.RemoveTagFromObjectAsync(
                profile.Id,
                profile.Id,
                ObjectType.Profile,
                new List<string>
                {
                    secondTag.Id
                });

            //Assert
            string collectionName = _fixture.GetCollectionName<ISecondLevelProjectionProfile>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Id == ""{
                       profile.Id
                   }""
                   RETURN x.Tags";

            MultiApiResponse<IList<CalculatedTag>> response =
                await GetArangoClient().ExecuteQueryAsync<IList<CalculatedTag>>(aqlQuery);

            response.QueryResult.Count.Should().Be(1);

            response.QueryResult.FirstOrDefault()
                ?.Should()
                .ContainEquivalentOf(tags.FirstOrDefault(t => t.Id == firstTag.Id));
        }

        [Fact]
        public async Task AddCustomPropertiesToProfile_should_work()
        {
            // Arange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            await repo.CreateProfileAsync(user);

            var customProperties = new Dictionary<string, string>
            {
                { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            };

            // Act
            await repo.AddCustomPropertiesToProfile(user?.Id, customProperties);

            string collectionName = _fixture.GetCollectionName<CustomPropertyEntityModel>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Related == ""{
                       _fixture.GetCollectionName<IProfileEntityModel>()
                   }/{
                       user?.Id
                   }""
                   RETURN x";

            MultiApiResponse<CustomPropertyEntityModel> response =
                await GetArangoClient().ExecuteQueryAsync<CustomPropertyEntityModel>(aqlQuery);

            Dictionary<string, string> resultDic = response.QueryResult.ToDictionary(k => k.Key, v => v.Value);

            resultDic.Should().BeEquivalentTo(customProperties);
        }

        [Fact]
        public async Task AddCustomPropertiesToProfile_store_twice_should_be_overwritten()
        {
            // Arange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            await repo.CreateProfileAsync(user);

            var customProperties = new Dictionary<string, string>
            {
                { "FavoritesFood", "Spaghetti" },
                { "Color", "sunSetOrange" },
                { "FavoriteHuman", "APATE-DE-ESO" }
            };

            await repo.AddCustomPropertiesToProfile(user.Id, customProperties);

            customProperties = new Dictionary<string, string>
            {
                { "FavoritesFood", "Chilli-Con-Carne" },
                { "Color", "Green" },
                { "FavoriteHuman", "El Clippi" }
            };

            // Act
            await repo.AddCustomPropertiesToProfile(user.Id, customProperties);

            string collectionName = _fixture.GetCollectionName<CustomPropertyEntityModel>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Related == ""{
                       _fixture.GetCollectionName<IProfileEntityModel>()
                   }/{
                       user?.Id
                   }""
                   RETURN x";

            MultiApiResponse<CustomPropertyEntityModel> response =
                await GetArangoClient().ExecuteQueryAsync<CustomPropertyEntityModel>(aqlQuery);

            Dictionary<string, string> resultDic = response.QueryResult.ToDictionary(k => k.Key, v => v.Value);

            resultDic.Should().BeEquivalentTo(customProperties);
        }

        [Fact]
        public async Task RemoveCustomPropertiesFromProfile_should_work()
        {
            // Arange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            await repo.CreateProfileAsync(user);
            IList<string> customPropertiesKey = MockDataGenerator.GenerateListOfGuiIds(5);

            Dictionary<string, string> customProperties =
                customPropertiesKey.ToDictionary(s => s, s => Guid.NewGuid().ToString());

            // Act
            await repo.AddCustomPropertiesToProfile(user?.Id, customProperties);

            await repo.RemoveCustomPropertiesFromProfile(
                user?.Id,
                new[] { customPropertiesKey[0], customPropertiesKey[1], customPropertiesKey[4] });

            string collectionName = _fixture.GetCollectionName<CustomPropertyEntityModel>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Related == ""{
                       _fixture.GetCollectionName<IProfileEntityModel>()
                   }/{
                       user?.Id
                   }""
                   RETURN x";

            MultiApiResponse<CustomPropertyEntityModel> response =
                await GetArangoClient().ExecuteQueryAsync<CustomPropertyEntityModel>(aqlQuery);

            Dictionary<string, string> resultDic = response.QueryResult.ToDictionary(k => k.Key, v => v.Value);

            resultDic.Should()
                .BeEquivalentTo(
                    customProperties.Where(c => c.Key == customPropertiesKey[2] || c.Key == customPropertiesKey[3])
                        .ToDictionary(k => k.Key, v => v.Value));
        }

        [Fact]
        public async Task CalculatePath_should_work()
        {
            // Arange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            SecondLevelProjectionGroup firstGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionGroup secondGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionGroup thirdGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionGroup fourthGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            string relatedProfileId = user.Id;
            DateTime now = DateTime.UtcNow;

            var defaultConditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = null,
                    End = null
                },
                new RangeCondition
                {
                    Start = new DateTime(2021, 12, 01),
                    End = new DateTime(2022, 5, 29)
                }
            };

            var testConditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = now.AddDays(2),
                    End = now.AddDays(30)
                }
            };

            await repo.CreateProfileAsync(user);
            await repo.CreateProfileAsync(firstGroup);
            await repo.CreateProfileAsync(secondGroup);
            await repo.CreateProfileAsync(thirdGroup);
            await repo.CreateProfileAsync(fourthGroup);

            await repo.AddMemberOfAsync(relatedProfileId, user.Id, defaultConditions, firstGroup);
            await repo.AddMemberOfAsync(relatedProfileId, firstGroup.Id, defaultConditions, secondGroup);
            await repo.AddMemberOfAsync(relatedProfileId, firstGroup.Id, defaultConditions, thirdGroup);
            await repo.AddMemberOfAsync(relatedProfileId, secondGroup.Id, testConditions, fourthGroup);

            var expectedResult = new List<string>
            {
                $"{secondGroup.Id}/{firstGroup.Id}/{user.Id}",
                $"{thirdGroup.Id}/{firstGroup.Id}/{user.Id}",
                user.Id
            };

            // Act
            IList<string> paths = await repo.GetPathOfProfileAsync(user.Id);

            // Assert
            paths.Should().NotBeNull();
            paths.Should().BeEquivalentTo(expectedResult);
        }
    }
}
