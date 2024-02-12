using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class ContainerDeletedEventHandlerTests
{
    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Theory]
    [InlineData(ContainerType.Organization, ObjectType.Organization)]
    [InlineData(ContainerType.Group, ObjectType.Group)]
    [InlineData(ContainerType.Function, ObjectType.Function)]
    [InlineData(ContainerType.Role, ObjectType.Role)]
    public async Task Handle_message_should_work(ContainerType containerType, ObjectType objectType)
    {
        //arrange
        var containerDeleted = new ContainerDeleted
        {
            ContainerType = containerType,
            ContainerId = Guid.NewGuid().ToString(),
            EventId = Guid.NewGuid().ToString(),
            MemberId = Guid.NewGuid().ToString(),
            MetaData = new EventMetaData
            {
                RelatedEntityId = Guid.NewGuid().ToString()
            }
        };

        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ContainerDeletedEventHandler>(services);

        // act
        var objectIdent = new ObjectIdent
        {
            Type = objectType
        };

        await sut.HandleEventAsync(
            containerDeleted.AddDefaultMetadata(services, objectIdent),
            containerDeleted.GenerateEventHeader(14));

        string relatedProfileId = IdHelper.GetRelatedProfileId(containerDeleted.MetaData.RelatedEntityId);

        // assert
        repoMock.Verify(
            repo => repo.RemoveMemberOfAsync(
                It.Is<string>(t => t == relatedProfileId),
                It.Is<string>(t => t == containerDeleted.MemberId),
                It.Is<ContainerType>(t => t == containerDeleted.ContainerType),
                It.Is<string>(t => t == containerDeleted.ContainerId),
                It.IsAny<IList<RangeCondition>>(),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));
    }

    [Fact]
    public async Task Handle_message_with_null_values_should_throw()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ContainerDeletedEventHandler>(services);

        var objectIdent = new ObjectIdent
        {
            Type = ObjectType.User
        };

        ContainerDeleted containerDeleted = new ContainerDeleted().AddDefaultMetadata(services, objectIdent);

        // act & assert
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                containerDeleted,
                containerDeleted.GenerateEventHeader(14)));
    }

    [Fact]
    public async Task Handle_null_message_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ContainerDeletedEventHandler>(services);

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

        var sut = ActivatorUtilities.CreateInstance<ContainerDeletedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new ContainerDeleted(),
                null));
    }
}