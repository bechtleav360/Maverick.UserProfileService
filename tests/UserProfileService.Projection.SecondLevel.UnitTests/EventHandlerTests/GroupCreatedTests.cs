using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Comparer;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class GroupCreatedTests
{
    private readonly GroupCreated _createEvent;
    private readonly SecondLevelProjectionGroup _profile;

    public GroupCreatedTests()
    {
        _profile = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

        _createEvent =
            AggregateResolvedModelConverter.CreateProfileCreated(_profile);
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

        var sut = ActivatorUtilities.CreateInstance<GroupCreatedEventHandler>(services);

        // act
        await sut.HandleEventAsync(
            _createEvent.AddDefaultMetadata(services),
            _createEvent.GenerateEventHeader(14));

        // assert
        repoMock.Verify(
            repo => repo.CreateProfileAsync(
                It.Is(
                    _profile,
                    ProfileComparerForTest.CreateWithExcludeMember(
                        m => m.Path == nameof(ISecondLevelProjectionProfile.MemberOf)
                            || m.Path == nameof(ISecondLevelProjectionProfile.Paths))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }

    [Fact]
    public async Task Handle_message_with_null_values_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<GroupCreatedEventHandler>(services);

        GroupCreated groupCreated = new GroupCreated().AddDefaultMetadata(services);

        // act
        await sut.HandleEventAsync(groupCreated, groupCreated.GenerateEventHeader(14));

        // assert
        repoMock.Verify(
            repo => repo.CreateProfileAsync(
                It.Is(
                    new SecondLevelProjectionGroup
                    {
                        ExternalIds = new List<ExternalIdentifier>() // because it will always be set
                    },
                    new ProfileComparerForTest()),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }

    [Fact]
    public async Task Handle_null_message_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<GroupCreatedEventHandler>(services);

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

        var sut = ActivatorUtilities.CreateInstance<GroupCreatedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new GroupCreated(),
                null));
    }
}