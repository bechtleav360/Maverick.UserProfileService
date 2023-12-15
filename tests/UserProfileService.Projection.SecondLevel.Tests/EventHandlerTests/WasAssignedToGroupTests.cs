using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.Tests.Extensions;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;
using Xunit;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.SecondLevel.Tests.EventHandlerTests;

public class WasAssignedToGroupTests
{
    private readonly AggregateModels.RangeCondition[] _conditions;
    private readonly ISecondLevelProjectionProfile _memberToAdded;
    private readonly ObjectIdent _relatedStreamIdent;
    private readonly SecondLevelProjectionGroup _targetGroup;
    private readonly WasAssignedToGroup _wasAssignedToGroupEvent;

    public WasAssignedToGroupTests()

    {
        _memberToAdded = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
        _targetGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
        _conditions = MockDataGenerator.GenerateAggregateRangeConditions(20, 0.6f).ToArray();

        _wasAssignedToGroupEvent = AggregateResolvedModelConverter.GenerateWasAssignedToGroup(
            _targetGroup,
            _memberToAdded.Id,
            _conditions);

        _relatedStreamIdent = new ObjectIdent(_memberToAdded.Id, ObjectType.Group);
    }

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>(MockBehavior.Strict);

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Fact]
    public async Task Handle_should_work_with_group_to_group_assignment()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        repoMock.Setup(
                repo => repo.SaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None))
            .Returns(Task.CompletedTask);

        repoMock.Setup(
                repo => repo.AddMemberOfAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IList<AggregateModels.RangeCondition>>(),
                    It.IsAny<ISecondLevelProjectionContainer>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<WasAssignedToGroupEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        await sut.HandleEventAsync(
            _wasAssignedToGroupEvent,
            _wasAssignedToGroupEvent.GenerateEventHeader(
                14,
                streamNameResolve.GetStreamName(_relatedStreamIdent)),
            CancellationToken.None);

        repoMock.Verify(
            repo => repo.SaveProjectionStateAsync(
                It.IsAny<ProjectionState>(),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Once);

        repoMock.Verify(
            repo => repo.AddMemberOfAsync(
                It.Is((string id) => id == _memberToAdded.Id),
                It.Is((string memberId) => memberId == _memberToAdded.Id),
                ItShould.BeEquivalentTo(_conditions),
                ItShould.BeEquivalentTo(
                    _targetGroup,
                    opt => opt.Excluding(m => m.Paths).Excluding(m => m.MemberOf)),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Once);

        repoMock.Verify(
            repo => repo.UpdateProfilePropertiesAsync(
                It.Is(
                    _memberToAdded.Id,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(ISecondLevelProjectionProfile.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }

    [Fact]
    public async Task Handle_null_header_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<WasAssignedToGroupEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleEventAsync(new WasAssignedToGroup(), null));
    }

    [Fact]
    public async Task Handle_null_message_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<WasAssignedToGroupEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(null, new StreamedEventHeader()));
    }
}