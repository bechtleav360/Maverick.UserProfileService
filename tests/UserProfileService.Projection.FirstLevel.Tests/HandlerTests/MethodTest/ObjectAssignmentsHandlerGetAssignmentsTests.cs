using System;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using UserProfileService.Projection.FirstLevel.Extensions;
using Xunit;
using AssignmentCommon = UserProfileService.Common.V2.Contracts.Assignment;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.MethodTest
{
    public class ObjectAssignmentsHandlerGetAssignmentsTests
    {
        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.EmptyAndNullAssignmentsParameter),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Method_Empty_Or_Null_Assignments(ConditionObjectIdent[] assignmentsToRearrange)
        {
            Assert.Throws<ArgumentException>(
                () => assignmentsToRearrange.GetAssignments(
                    new ObjectIdent(Guid.NewGuid().ToString(), ObjectType.Group),
                    AssignmentType.ChildrenToParent));
        }

        [Fact]
        public void Testing_Assignments_Method_Object_Ident_null()
        {
            var assignmentsToRearrange = new[]
            {
                new ConditionObjectIdent(Guid.NewGuid().ToString(), ObjectType.Group)
            };

            Assert.Throws<ArgumentNullException>(
                () => assignmentsToRearrange.GetAssignments(
                    null,
                    AssignmentType.ChildrenToParent));
        }

        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.AddAssignmentsContainerToProfiles),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Method_Added_Remove_Container_To_Profile(
            ConditionObjectIdent[] assignmentsToRearrange,
            ObjectIdent conditional,
            AssignmentType type,
            AssignmentCommon[] resultAssignments)
        {
            AssignmentCommon[] rearrangeAssignment =
                assignmentsToRearrange.GetAssignments(conditional, type);

            rearrangeAssignment.Should()
                .BeEquivalentTo(resultAssignments);
        }

        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.AddAssignmentsProfileToContainer),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Method_Added_Remove_Profile_To_Container(
            ConditionObjectIdent[] assignmentsToRearrange,
            ObjectIdent conditional,
            AssignmentType type,
            AssignmentCommon[] resultAssignments)
        {
            AssignmentCommon[] rearrangeAssignment =
                assignmentsToRearrange.GetAssignments(conditional, type);

            rearrangeAssignment.Should()
                .BeEquivalentTo(resultAssignments);
        }

        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.AddAssignmentsOrganizationToProfiles),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Method_Added_Remove_Organization_To_Profile_Resource_As_Child_To_Parent(
            ConditionObjectIdent[] assignmentsToRearrange,
            ObjectIdent conditional,
            AssignmentType type,
            AssignmentCommon[] resultAssignments)
        {
            AssignmentCommon[] rearrangeAssignment =
                assignmentsToRearrange.GetAssignments(conditional, type);

            rearrangeAssignment.Should()
                .BeEquivalentTo(resultAssignments);
        }

        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.AddAssignmentsGroupsParentsToUser),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Method_Added_Remove_Groups_To_Profile_Resource_As_Parents_To_Child(
            ConditionObjectIdent[] assignmentsToRearrange,
            ObjectIdent conditional,
            AssignmentType type,
            AssignmentCommon[] resultAssignments)
        {
            AssignmentCommon[] rearrangeAssignment =
                assignmentsToRearrange.GetAssignments(conditional, type);

            rearrangeAssignment.Should()
                .BeEquivalentTo(resultAssignments);
        }

        [Theory]
        [MemberData(
            nameof(ArgumentsObjectAssignmentMethods.BadObjectionAssignmentParameter),
            MemberType = typeof(ArgumentsObjectAssignmentMethods))]
        public void Testing_Assignments_Method_Added_Bad_Data(
            ConditionObjectIdent[] assignmentsToRearrange,
            ObjectIdent conditional,
            AssignmentType type)
        {
            AssignmentCommon[] reArrangeAssignment =
                assignmentsToRearrange.GetAssignments(conditional, type);

            Assert.Empty(reArrangeAssignment);
        }
    }
}
