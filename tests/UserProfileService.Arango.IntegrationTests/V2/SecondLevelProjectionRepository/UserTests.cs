using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class UserTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public UserTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_user_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();

            await repo.CreateProfileAsync(user);

            var dbUser = await GetDocumentObjectAsync<IProfileEntityModel>(user.Id);
            var convertedUser = GetMapper().Map<SecondLevelProjectionUser>(dbUser);

            convertedUser.Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task AddMemberOf_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            string relatedProfileId = user.Id;
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

            await repo.CreateProfileAsync(user);
            await repo.CreateProfileAsync(group);

            Member groupAsMember = GetMapper().Map<SecondLevelProjectionGroup, Member>(group);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = null,
                    End = null
                }
            };

            groupAsMember.Conditions = conditions;

            await repo.AddMemberOfAsync(relatedProfileId, user.Id, conditions, group);

            user.MemberOf.Add(groupAsMember);

            user.Paths = new List<string>
            {
                user.Id,
                $"{group.Id}/{user.Id}"
            };

            ISecondLevelProjectionProfile updatedUser = await repo.GetProfileAsync(relatedProfileId);
            user.Should().BeEquivalentTo(updatedUser);

            var entityModel = await GetDocumentObjectAsync<UserEntityModel>(user.Id);

            Maverick.UserProfileService.Models.Models.Member calculatedMember =
                entityModel.MemberOf.FirstOrDefault(m => m.Id == group.Id);

            calculatedMember.Should().NotBeNull().And.BeEquivalentTo(groupAsMember);
            calculatedMember?.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task AddMemberOf_invalid_assignment_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            string relatedProfileId = user.Id;
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

            await repo.CreateProfileAsync(user);
            await repo.CreateProfileAsync(group);

            Member groupAsMember = GetMapper().Map<SecondLevelProjectionGroup, Member>(group);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(1),
                    End = DateTime.Today.AddDays(30)
                }
            };

            groupAsMember.Conditions = conditions;

            await repo.AddMemberOfAsync(relatedProfileId, user.Id, conditions, group);

            user.MemberOf.Add(groupAsMember);

            user.Paths = new List<string>
            {
                user.Id
            };

            ISecondLevelProjectionProfile updatedUser = await repo.GetProfileAsync(relatedProfileId);
            user.Should().BeEquivalentTo(updatedUser);

            var entityModel = await GetDocumentObjectAsync<UserEntityModel>(user.Id);

            Maverick.UserProfileService.Models.Models.Member calculatedMember =
                entityModel.MemberOf.FirstOrDefault(m => m.Id == group.Id);

            calculatedMember.Should().NotBeNull().And.BeEquivalentTo(groupAsMember);
            calculatedMember?.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task AddMemberOf_WithTags_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            string relatedProfileId = user.Id;
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

            List<TagAssignment> tags = MockDataGenerator.GenerateTagAggregateModels(2)
                .Select(
                    t => new TagAssignment
                    {
                        TagDetails = t,
                        IsInheritable = true
                    })
                .ToList();

            await repo.CreateTagAsync(tags[0].TagDetails);
            await repo.CreateTagAsync(tags[1].TagDetails);
            await repo.CreateProfileAsync(user);
            await repo.CreateProfileAsync(group);

            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);
            Member groupAsMember = GetMapper().Map<SecondLevelProjectionGroup, Member>(group);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = null
                }
            };

            groupAsMember.Conditions = conditions;

            await repo.AddMemberOfAsync(relatedProfileId, userAsMember.Id, conditions, group);
            await repo.AddTagToObjectAsync(user.Id, group.Id, ObjectType.Group, tags);

            user.MemberOf.Add(groupAsMember);

            user.Paths = new List<string>
            {
                user.Id,
                $"{group.Id}/{user.Id}"
            };

            ISecondLevelProjectionProfile updatedUser = await repo.GetProfileAsync(relatedProfileId);
            updatedUser.Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task TryUpdateMemberOf_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var userAsMember = GetMapper().Map<Member>(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = null
                }
            };

            await repo.AddMemberOfAsync(user?.Id, userAsMember.Id, conditions, group);

            var newProperties = new Dictionary<string, object>
            {
                { "Name", "Emmanuel" },
                { "DisplayName", "Herr Emmanuel" }
            };

            // Act
            await repo.TryUpdateMemberOfAsync(user?.Id, group?.Id, newProperties);

            ISecondLevelProjectionProfile dbUser = await repo.GetProfileAsync(user?.Id);

            Member result =
                dbUser?.MemberOf.FirstOrDefault(d => d.Name == "Emmanuel" && d.DisplayName == "Herr Emmanuel");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RemoveMemberOf_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator
                .GenerateSecondLevelProjectionUser(minimumMemberOf: 0, maximumMemberOf: 0)
                .Single();

            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);
            Member groupAsMemberOf = GetMapper().Map<SecondLevelProjectionGroup, Member>(group);

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                },

                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            var toRemoveConditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            await repo.AddMemberOfAsync(user.Id, userAsMember.Id, conditions, group);

            await repo.RemoveMemberOfAsync(user.Id, user.Id, ContainerType.Group, group.Id, toRemoveConditions);
            ISecondLevelProjectionProfile userDb = await repo.GetProfileAsync(user.Id);
            Member memberDeleted = userDb.MemberOf?.FirstOrDefault(u => u.Id == group.Id);

            groupAsMemberOf.Conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                }
            };

            memberDeleted.Should().BeEquivalentTo(groupAsMemberOf);
        }

        [Fact]
        public async Task RemoveMemberOf_remove_all_conditions_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator
                .GenerateSecondLevelProjectionUser(minimumMemberOf: 0, maximumMemberOf: 0)
                .Single();

            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                },

                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            await repo.AddMemberOfAsync(user.Id, userAsMember.Id, conditions, group);

            await repo.RemoveMemberOfAsync(user.Id, user.Id, ContainerType.Group, group.Id, conditions);
            ISecondLevelProjectionProfile userDb = await repo.GetProfileAsync(user.Id);
            Member memberDeleted = userDb.MemberOf?.FirstOrDefault(u => u.Id == group.Id);
            memberDeleted.Should().BeNull();
        }

        [Fact]
        public async Task RemoveMemberOf_remove_no_conditions_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator
                .GenerateSecondLevelProjectionUser(minimumMemberOf: 0, maximumMemberOf: 0)
                .Single();

            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                },

                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            await repo.AddMemberOfAsync(user.Id, userAsMember.Id, conditions, group);

            await repo.RemoveMemberOfAsync(user.Id, user.Id, ContainerType.Group, group.Id);
            ISecondLevelProjectionProfile userDb = await repo.GetProfileAsync(user.Id);
            Member memberDeleted = userDb.MemberOf?.FirstOrDefault(u => u.Id == group.Id);
            memberDeleted.Should().BeNull();
        }
        
        [Fact]
        public async Task RemoveMemberOf_with_function_remove_conditions_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator
                .GenerateSecondLevelProjectionUser(minimumMemberOf: 0, maximumMemberOf: 0)
                .Single();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);

            await repo.CreateFunctionAsync(function);
            await repo.CreateProfileAsync(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                },
                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            var toRemoveConditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                }
            };

            await repo.AddMemberOfAsync(user.Id, userAsMember.Id, conditions, function);

            await repo.RemoveMemberOfAsync(
                user.Id,
                user.Id,
                ContainerType.Function,
                function.Id,
                toRemoveConditions);

            var userDb = await GetDocumentObjectAsync<UserEntityModel>(user.Id);
            ILinkedObject memberDeleted = userDb.SecurityAssignments?.FirstOrDefault(u => u.Id == function.Id);

            var expectedFunction = new LinkedFunctionObject
                                   {
                                       Conditions =
                                           new List<Maverick.UserProfileService.Models.Models.RangeCondition>
                                           {
                                               new Maverick.UserProfileService.Models.Models.RangeCondition
                                               {
                                                   Start = DateTime.Today.AddDays(-20),
                                                   End = DateTime.Today.AddDays(-10)
                                               }
                                           },
                                       Id = function.Id,
                                       OrganizationId = function.OrganizationId,
                                       IsActive = false,
                                       Name = $"{function.Organization?.Name} {function.Role?.Name}",
                                       RoleId = function.RoleId,
                                       OrganizationName = function.Organization?.Name ?? throw new DataException("Missing data during generating mock data in test method"),
                                       RoleName = function.Role?.Name ?? throw new DataException("Missing data during generating mock data in test method"),
                                   };

            memberDeleted.Should().BeEquivalentTo(expectedFunction);
        }

        [Fact]
        public async Task RemoveMemberOf_with_function_remove_all_conditions_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator
                .GenerateSecondLevelProjectionUser(minimumMemberOf: 0, maximumMemberOf: 0)
                .Single();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            Member userAsMember = GetMapper().Map<SecondLevelProjectionUser, Member>(user);

            await repo.CreateFunctionAsync(function);
            await repo.CreateProfileAsync(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                },

                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            await repo.AddMemberOfAsync(user.Id, userAsMember.Id, conditions, function);

            await repo.RemoveMemberOfAsync(user.Id, user.Id, ContainerType.Function, function.Id, conditions);
            var userDb = await GetDocumentObjectAsync<UserEntityModel>(user.Id);
            ILinkedObject memberDeleted = userDb.SecurityAssignments.FirstOrDefault(f => f.Id == function.Id);
            memberDeleted.Should().BeNull();
        }

        [Fact]
        public async Task RemoveMemberOf_with_function_remove_no_conditions_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator
                .GenerateSecondLevelProjectionUser(minimumMemberOf: 0, maximumMemberOf: 0)
                .Single();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            await repo.CreateFunctionAsync(function);
            await repo.CreateProfileAsync(user);

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(30)
                },

                new RangeCondition
                {
                    Start = DateTime.Today.AddDays(-20),
                    End = DateTime.Today.AddDays(-10)
                }
            };

            await repo.AddMemberOfAsync(user.Id, user.Id, conditions, function);

            await repo.RemoveMemberOfAsync(user.Id, user.Id, ContainerType.Function, function.Id);
            var userDb = await GetDocumentObjectAsync<UserEntityModel>(user.Id);
            ILinkedObject memberDeleted = userDb.SecurityAssignments.FirstOrDefault(f => f.Id == function.Id);
            memberDeleted.Should().BeNull();
        }
    }
}
