using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class FunctionTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public static IEnumerable<object[]> FunctionData =>
            new List<object[]>
            {
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single() }
            };

        public FunctionTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_function_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            await repo.CreateFunctionAsync(function);

            var dbFunction = await GetDocumentObjectAsync<FunctionObjectEntityModel>(function.Id);
            var convertedFunction = GetMapper().Map<SecondLevelProjectionFunction>(dbFunction);
            convertedFunction.Should().BeEquivalentTo(function);
        }

        [Theory]
        [MemberData(nameof(FunctionData))]
        public async Task DeleteFunction_should_work(SecondLevelProjectionFunction function)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            await repo.CreateFunctionAsync(function);
            var dbFunction = await GetDocumentObjectAsync<FunctionObjectEntityModel>(function.Id);
            dbFunction.Should().NotBeNull();
            await repo.DeleteFunctionAsync(function.Id);
            await Assert.ThrowsAsync<InstanceNotFoundException>(async () => await repo.GetFunctionAsync(function.Id));
        }

        [Fact]
        public async Task UpdateFunction_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionFunction function = MockDataGenerator
                .GenerateSecondLevelProjectionFunctions()
                .Single();

            function.Id = FunctionsTestData.UpdateFunction.FunctionId;
            function.Source = "Test";

            var referenceValue = _fixture.Mapper.Map<FunctionBasic>(function);

            await repo.UpdateFunctionAsync(function);

            FunctionBasic updatedFunction =
                await GetDocumentObjectAsync<FunctionBasic, FunctionObjectEntityModel>(function.Id);

            updatedFunction
                .Should()
                .BeEquivalentTo(
                    referenceValue,
                    o => o.Excluding(f => f.Organization)
                          .Excluding(f => f.Role)
                          .Excluding(f => f.RoleId)
                          .Excluding(f => f.OrganizationId)
                          .Excluding(f => f.CreatedAt)
                          .Excluding(f => f.SynchronizedAt));

            updatedFunction
                .Should()
                .NotBeEquivalentTo(
                    referenceValue,
                    o => o.Including(f => f.CreatedAt)
                          .Including(f => f.SynchronizedAt));
        }

        [Fact]
        public async Task TryUpdateLinkedObjectAsync_should_work()
        {
            //TODO: A Test with conditions
            // Arange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            await repo.CreateFunctionAsync(function);
            await repo.CreateProfileAsync(user);

            //var functionAsLinkedObject = GetMapper().Map<LinkedFunctionObject>(function);
            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = null
                }
            };

            await repo.AddMemberOfAsync(user?.Id, userAsMember.Id, conditions, function);

            var newProperties = new Dictionary<string, object>
            {
                { "Name", "Emmanuel-Function" }
            };

            //Act
            await repo.TryUpdateLinkedObjectAsync(user?.Id, function?.Id, newProperties);

            string collectionName = _fixture.GetCollectionName<IProfileEntityModel>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        user?.Id
                    }""
                    LET updatedMember = (
                        FOR element IN x.SecurityAssignments
                        FILTER element.Name == ""Emmanuel-Function""
                        RETURN element
                       )
                   RETURN LENGTH (updatedMember)";

            MultiApiResponse<int> response = await GetArangoClient().ExecuteQueryAsync<int>(aqlQuery);
            int count = response.QueryResult.FirstOrDefault();

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public async Task TryUpdateLinkedProfileAsync_should_work()
        {
            // Arange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            await repo.CreateFunctionAsync(function);
            await repo.CreateProfileAsync(user);
            var userAsMember = GetMapper().Map<Member>(user);
            await repo.AddMemberAsync(function?.Id, ContainerType.Function, userAsMember);

            var newProperties = new Dictionary<string, object>
            {
                { "Name", "Emmanuel" },
                { "DisplayName", "Herr Emmanuel" }
            };

            //Act
            await repo.TryUpdateLinkedProfileAsync(function?.Id, user?.Id, newProperties);

            string collectionName = _fixture.GetCollectionName<FunctionObjectEntityModel>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        function?.Id
                    }""
                    LET updatedMember = (
                        FOR element IN x.LinkedProfiles
                        FILTER element.Name == ""Emmanuel"" AND element.DisplayName == ""Herr Emmanuel""
                        RETURN element
                       )
                   RETURN LENGTH (updatedMember)";

            MultiApiResponse<int> response = await GetArangoClient().ExecuteQueryAsync<int>(aqlQuery);
            int count = response.QueryResult.FirstOrDefault();

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public async Task Add_linked_profiles_to_existing_profiles_set_of_function_should_work()
        {
            // arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            Member newMember = await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                FunctionsTestData.AddMember.NewGroupId);

            Member existingMember =
                await GetDocumentObjectAsync<Member, IProfileEntityModel>(FunctionsTestData.AddMember.ExistingGroupId);

            // act
            await repo.AddMemberAsync(
                FunctionsTestData.AddMember.FunctionId,
                ContainerType.Function,
                newMember.NormalizeRangeConditions());

            // assert
            var modifiedFunction =
                await GetDocumentObjectAsync<FunctionObjectEntityModel>(FunctionsTestData.AddMember.FunctionId);

            modifiedFunction.LinkedProfiles
                .Should()
                .BeEquivalentTo(
                    existingMember
                        .NormalizeRangeConditions()
                        .AsCollection(newMember));
        }

        [Fact]
        public async Task Add_linked_profiles_twice_to_function_should_work()
        {
            // arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            Member newMember = await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                FunctionsTestData.AddMemberAddTwice.NewGroupId);

            Member existingMember =
                await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                    FunctionsTestData.AddMemberAddTwice.ExistingGroupId);

            // act
            await repo.AddMemberAsync(
                FunctionsTestData.AddMemberAddTwice.FunctionId,
                ContainerType.Function,
                newMember.NormalizeRangeConditions());

            await repo.AddMemberAsync(
                FunctionsTestData.AddMemberAddTwice.FunctionId,
                ContainerType.Function,
                newMember.NormalizeRangeConditions());

            // assert
            var modifiedFunction =
                await GetDocumentObjectAsync<FunctionObjectEntityModel>(FunctionsTestData.AddMemberAddTwice.FunctionId);

            modifiedFunction.LinkedProfiles
                .Should()
                .BeEquivalentTo(newMember.AsCollection(existingMember.NormalizeRangeConditions()));
        }

        [Fact]
        public async Task Add_linked_profiles_with_range_merged_conditions()
        {
            // arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            Member newMember = await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                FunctionsTestData.AddMemberRangeConditions.NewGroupId);

            Member existingMember =
                await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                    FunctionsTestData.AddMemberRangeConditions.ExistingGroupId);

            newMember.AddRangeCondition(
                new List<RangeCondition>
                {
                    new RangeCondition
                    {
                        Start = DateTime.Today.ToUniversalTime(),
                        End = DateTime.Today.AddDays(20).ToUniversalTime()
                    },
                    new RangeCondition
                    {
                        Start = DateTime.Today.AddDays(23).ToUniversalTime(),
                        End = DateTime.Today.AddDays(100).ToUniversalTime()
                    }
                });

            // act
            await repo.AddMemberAsync(
                FunctionsTestData.AddMemberRangeConditions.FunctionId,
                ContainerType.Function,
                newMember);

            newMember.AddRangeCondition(
                new List<RangeCondition>
                {
                    new RangeCondition
                    {
                        Start = DateTime.Today.AddYears(3).ToUniversalTime(),
                        End = DateTime.Today.AddYears(3).ToUniversalTime()
                    }
                });

            await repo.AddMemberAsync(
                FunctionsTestData.AddMemberRangeConditions.FunctionId,
                ContainerType.Function,
                newMember);

            // assert
            var modifiedFunction =
                await GetDocumentObjectAsync<FunctionObjectEntityModel>(
                    FunctionsTestData.AddMemberRangeConditions.FunctionId);

            modifiedFunction.LinkedProfiles
                .Should()
                .BeEquivalentTo(newMember.AsCollection(existingMember.NormalizeRangeConditions()));
        }

        [Theory]
        [MemberData(nameof(FunctionData))]
        public async Task AddTagToFunction_should_work(SecondLevelProjectionFunction function)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            var tag = GetMapper().Map<Tag>(MockDataGenerator.GenerateCalculatedTags().Single());
            await repo.CreateTagAsync(tag);
            await repo.CreateFunctionAsync(function);

            IList<TagAssignment> tags = new List<TagAssignment>();

            tags.Add(
                new TagAssignment
                {
                    IsInheritable = false,
                    TagDetails = tag
                });

            await repo.AddTagToObjectAsync(function.Id, function.Id, ObjectType.Function, tags);

            string collectionName = _fixture.GetCollectionName<SecondLevelProjectionFunction>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Id == ""{
                       function.Id
                   }""
                   RETURN x.Tags";

            MultiApiResponse<IList<TagAssignment>> response =
                await GetArangoClient().ExecuteQueryAsync<IList<TagAssignment>>(aqlQuery);

            response.QueryResult.FirstOrDefault()
                ?.FirstOrDefault()
                ?
                .Should()
                .BeEquivalentTo(tags.FirstOrDefault());
        }
    }
}
