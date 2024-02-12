using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class EntityDeletedEventHandlerTests
{
    private readonly EntityDeleted _entityDeleted = new EntityDeleted
    {
        EventId = Guid.NewGuid().ToString(),
        Id = Guid.NewGuid().ToString()
    };

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Theory]
    [InlineData(ObjectType.User)]
    [InlineData(ObjectType.Group)]
    [InlineData(ObjectType.Function)]
    [InlineData(ObjectType.Role)]
    [InlineData(ObjectType.Organization)]
    [InlineData(ObjectType.Profile)]
    [InlineData(ObjectType.Tag)]
    public async Task Handle_message_should_work(ObjectType objectType)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<EntityDeletedEventHandler>(services);

        // act
        var objectIdent = new ObjectIdent
        {
            Type = objectType
        };

        await sut.HandleEventAsync(
            _entityDeleted.AddDefaultMetadata(services, objectIdent),
            _entityDeleted.GenerateEventHeader(14));

        switch (objectType)
        {
            case ObjectType.User:
            case ObjectType.Organization:
            case ObjectType.Profile:
            case ObjectType.Group:

                // assert
                repoMock.Verify(
                    repo => repo.DeleteProfileAsync(
                        It.Is<string>(c => c == _entityDeleted.Id),
                        It.Is<IDatabaseTransaction>(
                            t =>
                                ((MockDatabaseTransaction)t).Id == transaction.Id),
                        CancellationToken.None),
                    Times.Exactly(1));

                break;
            case ObjectType.Function:

                // assert
                repoMock.Verify(
                    repo => repo.DeleteFunctionAsync(
                        It.Is<string>(c => c == _entityDeleted.Id),
                        It.Is<IDatabaseTransaction>(
                            t =>
                                ((MockDatabaseTransaction)t).Id == transaction.Id),
                        CancellationToken.None),
                    Times.Exactly(1));

                break;

            case ObjectType.Role:
                // assert
                repoMock.Verify(
                    repo => repo.DeleteRoleAsync(
                        It.Is<string>(c => c == _entityDeleted.Id),
                        It.Is<IDatabaseTransaction>(
                            t =>
                                ((MockDatabaseTransaction)t).Id == transaction.Id),
                        CancellationToken.None),
                    Times.Exactly(1));

                break;
            case ObjectType.Tag:
                // assert
                repoMock.Verify(
                    repo => repo.RemoveTagAsync(
                        It.Is<string>(c => c == _entityDeleted.Id),
                        It.Is<IDatabaseTransaction>(
                            t =>
                                ((MockDatabaseTransaction)t).Id == transaction.Id),
                        CancellationToken.None),
                    Times.Exactly(1));

                break;
        }

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }

    [Fact]
    public async Task Handle_message_should_with_not_supported_type_should_throw()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<EntityDeletedEventHandler>(services);

        // act & assert
        //TODO: just temporary solution for the test
        var objectIdent = new ObjectIdent
        {
            Type = ObjectType.Unknown
        };

        await Assert.ThrowsAsync<InvalidHeaderException>(
            async () => await sut.HandleEventAsync(
                _entityDeleted.AddDefaultMetadata(services, objectIdent),
                _entityDeleted.GenerateEventHeader(14)));
    }

    [Fact]
    public async Task Handle_message_with_null_values_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<EntityDeletedEventHandler>(services);

        EntityDeleted entityDeleted = new EntityDeleted().AddDefaultMetadata(services);

        // act
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.HandleEventAsync(entityDeleted, entityDeleted.GenerateEventHeader(14)));
    }

    [Fact]
    public async Task Handle_null_header_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<EntityDeletedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new EntityDeleted(),
                null));
    }
}