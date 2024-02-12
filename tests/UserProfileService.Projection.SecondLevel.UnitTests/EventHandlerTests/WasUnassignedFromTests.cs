using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class WasUnassignedFromTests
{
    private readonly WasUnassignedFrom _event;

    public WasUnassignedFromTests()
    {
        ISecondLevelProjectionProfile user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
        SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
        _event = AggregateResolvedModelConverter.CreateWasUnassignedFrom(user, group);
    }

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Fact]
    public async Task Handle_message_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromEventHandler>(services);

        // act
        await sut.HandleEventAsync(
            _event.AddDefaultMetadata(services),
            _event.GenerateEventHeader(14));

        string relatedProfileId = IdHelper.GetRelatedProfileId(_event.MetaData.RelatedEntityId);

        // assert
        repoMock.Verify(
            repo => repo.RemoveMemberOfAsync(
                It.Is<string>(t => t == relatedProfileId),
                It.Is<string>(t => t == _event.ChildId),
                It.Is<ContainerType>(t => t == _event.ParentType),
                It.Is<string>(t => t == _event.ParentId),
                ItShould.BeEquivalentTo<IList<RangeCondition>>(_event.Conditions),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.UpdateProfilePropertiesAsync(
                It.Is(
                    _event.ChildId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(ISecondLevelProjectionProfile.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_null_message_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                null,
                new StreamedEventHeader()));
    }

    [Fact]
    public async Task Handle_null_header_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new WasUnassignedFrom(),
                null));
    }
}