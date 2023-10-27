using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;
using RangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.MethodTest
{
    public class ObjectAssignmentHandlerWasAssignedToMethodTests
    {
        private readonly IFirstLevelProjectionContainer _firstLevelContainer;
        private readonly ObjectAssignmentFirstLevelEventHandler _objectAssignmentFirstLevelEventHandler;
        private readonly ITestOutputHelper _outputHelper;

        public ObjectAssignmentHandlerWasAssignedToMethodTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            var repoMock = new Mock<IFirstLevelProjectionRepository>();
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            _firstLevelContainer = MockDataGenerator.GenerateFirstLevelProjectionGroup().Single();
            _objectAssignmentFirstLevelEventHandler = ActivatorUtilities.CreateInstance<ObjectAssignmentFirstLevelEventHandler>(services);
        }

        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.WasAssignedToData),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Was_Assigned_To(
            FirstLevelProjectionTreeEdgeRelation relationTreeEdge,
            IUserProfileServiceEvent wasAssignedResultEvent)
        {
            IUserProfileServiceEvent
                result = _objectAssignmentFirstLevelEventHandler.GenerateWasAssignedToEvent(relationTreeEdge);

            switch (wasAssignedResultEvent.Type)
            {
                case nameof(WasAssignedToGroup):
                {
                    var methodResult = (WasAssignedToGroup)result;
                    var resolvedResult = (WasAssignedToGroup)wasAssignedResultEvent;
                    methodResult.Should().BeEquivalentTo(resolvedResult);

                    break;
                }
                case nameof(WasAssignedToRole):
                {
                    var methodResult = (WasAssignedToRole)result;
                    var resolvedResult = (WasAssignedToRole)wasAssignedResultEvent;
                    methodResult.Should().BeEquivalentTo(resolvedResult);

                    break;
                }
                case nameof(WasAssignedToFunction):
                {
                    var methodResult = (WasAssignedToFunction)result;
                    var resolvedResult = (WasAssignedToFunction)wasAssignedResultEvent;
                    methodResult.Should().BeEquivalentTo(resolvedResult);

                    break;
                }
                case nameof(WasAssignedToOrganization):
                {
                    var methodResult = (WasAssignedToOrganization)result;
                    var resolvedResult = (WasAssignedToOrganization)wasAssignedResultEvent;
                    methodResult.Should().BeEquivalentTo(resolvedResult);

                    break;
                }
            }
        }

        [Fact]
        public void Testing_Assignments_Was_Assigned_To_Relation_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _objectAssignmentFirstLevelEventHandler.GenerateWasAssignedToEvent(null));
        }

        [Fact]
        public void Testing_Assignments_Was_Assigned_To_Unsupported_ObjectType()
        {
            // mocked container has as containerType "not specified"
            var parent = new MockedFirstLevelContainer
            {
                Id = Guid.NewGuid().ToString()
            };

            var relationEdge = new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = parent,
                Child = new ObjectIdent("id", ObjectType.Profile),
                ParentTags = new List<TagAssignment>(),
                Conditions = new List<RangeCondition>()
            };

            Assert.Throws<NotSupportedException>(
                () => _objectAssignmentFirstLevelEventHandler.GenerateWasAssignedToEvent(relationEdge));
        }

        [Fact]
        public void Testing_Assignments_Was_Assigned_To_With_Child_Is_Null()
        {
            var relationEdge = new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = _firstLevelContainer,
                Child = null,
                ParentTags = new List<TagAssignment>(),
                Conditions = new List<RangeCondition>()
            };

            Assert.Throws<ArgumentException>(
                () => _objectAssignmentFirstLevelEventHandler.GenerateWasAssignedToEvent(relationEdge));
        }

        [Fact]
        public void Testing_Assignments_Was_Assigned_To_With_Parent_Is_Null()
        {
            var relationEdge = new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = null,
                Child = new ObjectIdent("Id", ObjectType.Group),
                ParentTags = new List<TagAssignment>(),
                Conditions = new List<RangeCondition>()
            };

            Assert.Throws<ArgumentException>(
                () => _objectAssignmentFirstLevelEventHandler.GenerateWasAssignedToEvent(relationEdge));
        }
    }
}
