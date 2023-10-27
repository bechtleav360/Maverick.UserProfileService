using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests
{
    public class AssignmentUserExtensionsTests
    {
        [Fact]
        public void Calculate_active_memberships_should_resolve_recursively()
        {
            const string groupAId = "group-a";
            const string groupBId = "group-b";
            const string userId = "test";

            var user = new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = groupAId,
                        Name = "Gruppe A",
                        ContainerType = ContainerType.Group
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = groupBId,
                        Name = "Gruppe B",
                        ContainerType = ContainerType.Group
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent(groupAId, ObjectType.Group),
                        Profile = new ObjectIdent(groupBId, ObjectType.Group)
                    },
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent(groupBId, ObjectType.Group),
                        Profile = new ObjectIdent(userId, ObjectType.User)
                    }
                }
            };

            ISet<ObjectIdent> activeMemberships = user.CalculateActiveMemberships();

            var expectedResult = new[]
            {
                new ObjectIdent(groupAId, ObjectType.Group), new ObjectIdent(groupBId, ObjectType.Group)
            };

            activeMemberships.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void Calculate_connected_containers_should_keep_function_role_oe()
        {
            const string userId = "user";
            const string functionId = "function";
            const string roleId = "role";
            const string oeId = "oe";

            var user = new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = roleId,
                        Name = "Rolle",
                        ContainerType = ContainerType.Role
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = oeId,
                        Name = "OE",
                        ContainerType = ContainerType.Organization
                    },
                    new SecondLevelAssignmentFunction
                    {
                        Id = functionId,
                        Name = "Function",
                        OrganizationId = oeId,
                        RoleId = roleId
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent(functionId, ObjectType.Function),
                        Profile = new ObjectIdent(userId, ObjectType.User)
                    }
                }
            };

            ISet<ObjectIdent> connectedContainers = user.GetConnectedContainers();

            var expectedResult = new List<ObjectIdent>
            {
                new ObjectIdent(roleId, ObjectType.Role),
                new ObjectIdent(oeId, ObjectType.Organization),
                new ObjectIdent(functionId, ObjectType.Function)
            };

            connectedContainers.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void Calculate_connected_containers_should_detect_not_connected_containers()
        {
            const string userId = "user";
            const string groupAId = "group-a";
            const string groupBId = "group-b";

            var user = new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = groupAId,
                        Name = "Gruppe A",
                        ContainerType = ContainerType.Group
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = groupBId,
                        Name = "Gruppe B",
                        ContainerType = ContainerType.Group
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent(groupAId, ObjectType.Group),
                        Profile = new ObjectIdent(userId, ObjectType.User)
                    }
                }
            };

            ISet<ObjectIdent> connectedContainers = user.GetConnectedContainers();

            var expectedResult = new List<ObjectIdent>
            {
                new ObjectIdent(groupAId, ObjectType.Group)
            };

            connectedContainers.Should().BeEquivalentTo(expectedResult);
        }
    }
}
