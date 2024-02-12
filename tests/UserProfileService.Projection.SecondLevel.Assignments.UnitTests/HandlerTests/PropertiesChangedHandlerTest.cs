using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Handler;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Assignments.UnitTests.HandlerTests
{
    public class PropertiesChangedHandlerTest
    {
        private const string DefaultName = "Test";
        private readonly ObjectIdent _function = new ObjectIdent(
            "8925CA64-67C4-433B-BD68-B52DCD7046FE",
            Maverick.UserProfileService.Models.EnumModels.ObjectType.Function);
        private readonly ObjectIdent _group = new ObjectIdent(
            "55175384-9B20-4927-A1E5-7F11B258C5A7",
            Maverick.UserProfileService.Models.EnumModels.ObjectType.Group);
        private readonly ObjectIdent _orgUnit = new ObjectIdent(
            "ECD168C9-C3AF-4F31-AA5D-4237A87B9C1C",
            Maverick.UserProfileService.Models.EnumModels.ObjectType.Organization);
        private readonly ObjectIdent _role = new ObjectIdent(
            "478C8876-2E78-494E-9FE0-DC90434447D7",
            Maverick.UserProfileService.Models.EnumModels.ObjectType.Role);
        private readonly ObjectIdent _user = new ObjectIdent(
            "777DD1DA-2727-46D5-990A-0DD0A0F4A8AC",
            Maverick.UserProfileService.Models.EnumModels.ObjectType.User);

        private Mock<ISecondLevelAssignmentRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<ISecondLevelAssignmentRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            return mock;
        }

        private static PropertiesChanged GetNewEvent(
            IServiceProvider serviceProvider,
            string entityId,
            ObjectType type,
            Dictionary<string, object> properties = null,
            Action<PropertiesChanged> postModifications = null)
        {
            PropertiesChanged newEvent = new PropertiesChanged
            {
                EventId = "03d6a0f24f3340b984e63b0dceb69afb",
                Id = entityId,
                Properties = properties
                    ?? new Dictionary<string, object>
                    {
                        { nameof(IProfile.Name), DefaultName }
                    },
                ObjectType = type
            }.AddDefaultMetadata(serviceProvider);

            postModifications?.Invoke(newEvent);

            return newEvent;
        }

        private SecondLevelProjectionAssignmentsUser GetNewUser(string userId)
        {
            return new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = _group.Id,
                        Name = "Gruppe A",
                        ContainerType = ContainerType.Group
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = _role.Id,
                        Name = "Lesen",
                        ContainerType = ContainerType.Role
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = _orgUnit.Id,
                        Name = "999",
                        ContainerType = ContainerType.Organization
                    },
                    new SecondLevelAssignmentFunction
                    {
                        Id = _function.Id,
                        Name = "999 Lesen",
                        OrganizationId = _orgUnit.Id,
                        RoleId = _role.Id
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent(
                            _group.Id,
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.Group),
                        Profile = new ObjectIdent(
                            userId,
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.User)
                    },
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent(
                            _function.Id,
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.Function),
                        Profile = new ObjectIdent(
                            userId,
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.User)
                    }
                }
            };
        }

        [Fact]
        public async Task Handle_event_should_rename_group()
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

            var sut = ActivatorUtilities.CreateInstance<PropertiesChangedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            PropertiesChanged eventToHandle = GetNewEvent(services, _group.Id, ObjectType.Group);

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
            expectedUser.Containers.First(c => c.Id == _group.Id).Name = DefaultName;

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

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<PropertiesChangedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            PropertiesChanged eventToHandle = GetNewEvent(
                services,
                _group.Id, //TODO change
                ObjectType.Group);

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

            var sut = ActivatorUtilities.CreateInstance<PropertiesChangedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            PropertiesChanged eventToHandle = GetNewEvent(
                services,
                "group-1",
                ObjectType.Group);

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(
                        new ObjectIdent(
                            Guid.NewGuid().ToString("D"),
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.Group))),
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

        [Fact]
        public async Task Handle_event_without_name_change_should_not_call_repo()
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

            var sut = ActivatorUtilities.CreateInstance<PropertiesChangedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            PropertiesChanged eventToHandle = GetNewEvent(
                services,
                _group.Id,
                ObjectType.Group,
                new Dictionary<string, object>
                {
                    { nameof(IProfile.DisplayName), "Test1234" }
                });

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(_user)),
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

        [Fact]
        public async Task Handle_event_for_user_should_not_call_repo()
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

            var sut = ActivatorUtilities.CreateInstance<PropertiesChangedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            PropertiesChanged eventToHandle = GetNewEvent(
                services,
                _user.Id,
                ObjectType.User);

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(_user)),
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
