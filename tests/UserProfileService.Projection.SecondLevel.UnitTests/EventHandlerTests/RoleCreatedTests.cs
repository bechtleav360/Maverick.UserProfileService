using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

public class RoleCreatedTests
{
    private readonly RoleCreated _createEvent;
    private readonly SecondLevelProjectionRole _role;

    public RoleCreatedTests()
    {
        _role = MockDataGenerator.GenerateSecondLevelProjectionRoles().Single();

        _createEvent =
            AggregateResolvedModelConverter.CreateRoleCreated(_role);
    }

    private Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        mock.SetReturnsDefault(Task.CompletedTask);

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

        var sut = ActivatorUtilities.CreateInstance<RoleCreatedEventHandler>(services);

        // act
        await sut.HandleEventAsync(
            _createEvent.AddDefaultMetadata(services),
            _createEvent.GenerateEventHeader(88));

        _role.DeniedPermissions ??= new List<string>();

        // assert
        repoMock.Verify(
            repo => repo.CreateRoleAsync(
                It.Is(_role, new RoleComparerForTest()),
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

        var sut = ActivatorUtilities.CreateInstance<RoleCreatedEventHandler>(services);
        RoleCreated roleCreated = new RoleCreated().AddDefaultMetadata(services);

        // act
        await sut.HandleEventAsync(roleCreated, roleCreated.GenerateEventHeader(88));

        // assert
        repoMock.Verify(
            repo => repo.CreateRoleAsync(
                It.Is(
                    new SecondLevelProjectionRole(),
                    new RoleComparerForTest()),
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

        var sut = ActivatorUtilities.CreateInstance<FunctionCreatedEventHandler>(services);

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

        var sut = ActivatorUtilities.CreateInstance<FunctionCreatedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new FunctionCreated(),
                null));
    }
}