using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Handler;
using UserProfileService.Projection.SecondLevel.Assignments.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Assignments.Tests.Mocks;
using UserProfileService.Projection.SecondLevel.Assignments.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using Organization = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests.HandlerTests
{
    public class WasAssignedToOrganizationHandlerTest
    {
        private const string OrganizationAName = "Organization A";
        private const string OrganizationBName = "Organization B";
        private readonly ObjectIdent _organizationA = new ObjectIdent(
            "ECD168C9-C3AF-4F31-AA5D-4237A87B9C1C",
            ObjectType.Organization);
        private readonly ObjectIdent _organizationB = new ObjectIdent(
            "8925CA64-67C4-433B-BD68-B52DCD7046FE",
            ObjectType.Organization);

        private readonly ObjectIdent _user = new ObjectIdent(
            "777DD1DA-2727-46D5-990A-0DD0A0F4A8AC",
            ObjectType.User);

        private Mock<ISecondLevelAssignmentRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<ISecondLevelAssignmentRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            return mock;
        }

        private static WasAssignedToOrganization GetNewEvent(
            IServiceProvider serviceProvider,
            string childId,
            string parentId,
            string organizationName,
            params RangeCondition[] assignments)
        {
            WasAssignedToOrganization newEvent = new WasAssignedToOrganization
            {
                EventId = "03d6a0f24f3340b984e63b0dceb69afb",
                ProfileId = childId,
                Target = new Organization
                {
                    Id = parentId,
                    Name = organizationName ?? "Test-Organization"
                },
                Conditions = assignments
            }.AddDefaultMetadata(serviceProvider);

            return newEvent;
        }

        private SecondLevelProjectionAssignmentsUser GetNewUser(string userId)
        {
            var user = new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = _organizationB.Id,
                        Name = OrganizationBName,
                        ContainerType = ContainerType.Organization
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new List<Maverick.UserProfileService.Models.Models.RangeCondition>
                        {
                            new Maverick.UserProfileService.Models.Models.RangeCondition(
                                DateTime.MinValue,
                                DateTime.Today)
                        },
                        Parent = new ObjectIdent(
                            _organizationB.Id,
                            ObjectType.Organization),
                        Profile = new ObjectIdent(
                            userId,
                            ObjectType.User)
                    }
                }
            };

            user.ActiveMemberships = user.CalculateActiveMemberships().ToList();

            return user;
        }

        [Fact]
        public async Task Handle_event_should_add_new_organization()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            SecondLevelProjectionAssignmentsUser savedUser = null;

            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id));

            repoMock.Setup(
                    r => r.SaveAssignmentUserAsync(
                        It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .Callback<SecondLevelProjectionAssignmentsUser, IDatabaseTransaction, CancellationToken>(
                    (user, _, _) => savedUser = user);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToOrganizationHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToOrganization eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _organizationA.Id,
                OrganizationAName,
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.MaxValue
                });

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                ct);

            // assert
            repoMock.Verify(
                r => r.SaveAssignmentUserAsync(
                    It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            repoMock.Verify(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            SecondLevelProjectionAssignmentsUser expectedUser = GetNewUser(_user.Id);

            expectedUser.Containers.Add(
                new SecondLevelAssignmentContainer
                {
                    Id = _organizationA.Id,
                    Name = OrganizationAName,
                    ContainerType = ContainerType.Organization
                });

            expectedUser.Assignments.Add(
                new SecondLevelProjectionAssignment
                {
                    Conditions = new List<Maverick.UserProfileService.Models.Models.RangeCondition>
                    {
                        new Maverick.UserProfileService.Models.Models.RangeCondition(
                            DateTime.Today,
                            DateTime.MaxValue)
                    },
                    Parent = _organizationA,
                    Profile = _user
                });

            expectedUser.ActiveMemberships.Add(_organizationA);

            savedUser.Should()
                .BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Handle_event_should_add_new_assignment()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            SecondLevelProjectionAssignmentsUser savedUser = null;

            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id));

            repoMock.Setup(
                    r => r.SaveAssignmentUserAsync(
                        It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .Callback<SecondLevelProjectionAssignmentsUser, IDatabaseTransaction, CancellationToken>(
                    (user, _, _) => savedUser = user);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToOrganizationHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToOrganization eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _organizationB.Id,
                OrganizationBName,
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.MaxValue
                });

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                ct);

            // assert
            repoMock.Verify(
                r => r.SaveAssignmentUserAsync(
                    It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            repoMock.Verify(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            SecondLevelProjectionAssignmentsUser expectedUser = GetNewUser(_user.Id);
            expectedUser.ActiveMemberships.Add(_organizationB);

            SecondLevelProjectionAssignment assignment =
                expectedUser.Assignments.First(a => a.Parent.Id == _organizationB.Id && a.Profile.Id == _user.Id);

            assignment.Conditions.Add(
                new Maverick.UserProfileService.Models.Models.RangeCondition(DateTime.Today, DateTime.MaxValue));

            savedUser.Should()
                .BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Handle_cancelled_event_should_throw_cancelled_exception()
        {
            // arrange
            var ct = new CancellationToken(true);
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToOrganizationHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToOrganization eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _organizationA.Id,
                OrganizationAName,
                new RangeCondition());

            // act & assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => sut.HandleEventAsync(
                    eventToHandle,
                    eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                    ct));
        }

        [Fact]
        public async Task Handle_event_for_group_should_not_call_repo()
        {
            // arrange
            var ct = new CancellationToken(false);
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToOrganizationHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToOrganization eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _organizationA.Id,
                OrganizationAName,
                new RangeCondition());

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(
                        new ObjectIdent(
                            Guid.NewGuid().ToString("D"),
                            ObjectType.Organization))),
                ct);

            // assert
            repoMock.Verify(
                r =>
                    r.GetAssignmentUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            repoMock.Verify(
                r =>
                    r.SaveAssignmentUserAsync(
                        It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            repoMock.Verify(
                r =>
                    r.RemoveAssignmentUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
