using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.Tests.Extensions;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;
using Xunit;
using ResolvedEnums = Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.SecondLevel.Tests.EventHandlerTests;

public class AssignmentConditionTriggeredEventHandlerTests
{
    private readonly ObjectIdent _user;

    public AssignmentConditionTriggeredEventHandlerTests()
    {
        _user = new ObjectIdent("777DD1DA-2727-46D5-990A-0DD0A0F4A8AC", ObjectType.User);
    }

    private static AssignmentConditionTriggered GetNewEvent(
        IServiceProvider serviceProvider,
        Action<AssignmentConditionTriggered> postModifications = null,
        ResolvedEnums.ObjectType targetType = ResolvedEnums.ObjectType.Group)
    {
        AssignmentConditionTriggered newEvent = new AssignmentConditionTriggered
        {
            EventId = "03d6a0f24f3340b984e63b0dceb69afb",
            ProfileId = "7F100468-690F-4BBA-B7F9-D61B97AB13EF",
            TargetId = "86728907-4300-414e-8d5f-2c64ff211d6d",
            TargetObjectType = targetType,
            IsActive = true
        }.AddDefaultMetadata(serviceProvider);

        postModifications?.Invoke(newEvent);

        return newEvent;
    }

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Fact]
    public async Task Handle_event_without_value_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredEventHandler>(services);

        var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new AssignmentConditionTriggered(),
                new AssignmentConditionTriggered().GenerateEventHeader(
                    14,
                    streamNameResolver.GetStreamName(_user))));
    }

    [Fact]
    public async Task Handle_event_with_missing_profile_id_should_fail()
    {
        // arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => s.AddSingleton(repoMock.Object));

        var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredEventHandler>(services);
        var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

        AssignmentConditionTriggered eventToHandle = GetNewEvent(
            services,
            e => e.ProfileId = null);

        // act & assert
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user))));
    }

    [Fact]
    public async Task Handle_event_with_missing_target_id_should_fail()
    {
        // arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => s.AddSingleton(repoMock.Object));

        var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredEventHandler>(services);
        var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

        AssignmentConditionTriggered eventToHandle = GetNewEvent(
            services,
            e => e.TargetId = null);

        // act & assert
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user))));
    }

    [Fact]
    public async Task Handle_event_should_work()
    {
        // arrange
        var ct = new CancellationToken();
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => s.AddSingleton(repoMock.Object));

        var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredEventHandler>(services);
        var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

        AssignmentConditionTriggered eventToHandle = GetNewEvent(services);

        // act
        await sut.HandleEventAsync(
            eventToHandle,
            eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
            ct);

        // assert
        repoMock.Verify(
            r => r.RecalculateAssignmentsAsync(
                ItShould.BeEquivalentTo(_user),
                eventToHandle.ProfileId,
                eventToHandle.TargetId,
                eventToHandle.TargetObjectType,
                eventToHandle.IsActive,
                ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                ItShould.BeEquivalentTo(ct)));
    }

    [Fact]
    public async Task Handle_event_with_user_and_related_the_same_should_work()
    {
        // arrange
        var ct = new CancellationToken();
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => s.AddSingleton(repoMock.Object));

        var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredEventHandler>(services);
        var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

        AssignmentConditionTriggered eventToHandle = GetNewEvent(
            services,
            e => e.ProfileId = _user.Id);

        // act
        await sut.HandleEventAsync(
            eventToHandle,
            eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
            ct);

        // assert
        repoMock.Verify(
            r => r.RecalculateAssignmentsAsync(
                ItShould.BeEquivalentTo(_user),
                _user.Id,
                eventToHandle.TargetId,
                eventToHandle.TargetObjectType,
                eventToHandle.IsActive,
                ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                ItShould.BeEquivalentTo(ct)));
    }

    [Fact]
    public async Task Handle_cancelled_event_should_throw_cancelled_exception()
    {
        // arrange
        var ct = new CancellationToken(true);
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => s.AddSingleton(repoMock.Object));

        var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredEventHandler>(services);
        var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

        AssignmentConditionTriggered eventToHandle = GetNewEvent(services);

        // act & assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                ct));
    }
}