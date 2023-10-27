using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.Tests.Extensions;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Tests.EventHandlerTests;

public class TagsAddedTests
{
    private (ObjectIdent objectIdent, TagsAdded tagsAdded) GenerateEventTestData(
        IServiceProvider serviceProvider,
        int? number = 2)
    {
        ObjectIdent obj = MockDataGenerator.GenerateObjectIdentInstances().Single();

        Tag[] tags = number switch
        {
            null => null,
            0 => Array.Empty<Tag>(),
            _ => MockDataGenerator.GenerateTagAggregateModels((int)number).ToArray()
        };

        TagsAdded addedEvent =
            AggregateResolvedModelConverter.CreateTagsAdded(obj, tags);

        addedEvent = addedEvent.AddDefaultMetadata(serviceProvider, obj);

        return (obj, addedEvent);
    }

    private Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Fact]
    public async Task HandleEventAsync_Success()
    {
        // Arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<TagsAddedEventHandler>(services);

        (ObjectIdent _, TagsAdded addedEvent) = GenerateEventTestData(services);

        // Act
        await sut.HandleEventAsync(
            addedEvent,
            addedEvent.GenerateEventHeader(88));

        // Assert
        repoMock.Verify(
            repo => repo.AddTagToObjectAsync(
                addedEvent.Id,
                addedEvent.Id,
                addedEvent.ObjectType,
                addedEvent.Tags,
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleEventAsync_Throw_Exception_If_ResourceId_IsNullOrEmpty(string resourceId)
    {
        // Arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<TagsAddedEventHandler>(services);

        (ObjectIdent _, TagsAdded addedEvent) = GenerateEventTestData(services);

        addedEvent.Id = resourceId;

        // Act
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                addedEvent,
                addedEvent.GenerateEventHeader(88)));
    }

    [Fact]
    public async Task HandleEventAsync_Throw_Exception_If_ObjectType_IsUnknown()
    {
        // Arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<TagsAddedEventHandler>(services);

        (ObjectIdent _, TagsAdded addedEvent) = GenerateEventTestData(services);

        addedEvent.ObjectType = ObjectType.Unknown;

        // Act
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                addedEvent,
                addedEvent.GenerateEventHeader(88)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task HandleEventAsync_Throw_Exception_If_Tags_NullOrEmpty(int? number)
    {
        // Arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<TagsAddedEventHandler>(services);

        (ObjectIdent _, TagsAdded addedEvent) = GenerateEventTestData(services, number);

        addedEvent.ObjectType = ObjectType.Unknown;

        // Act
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                addedEvent,
                addedEvent.GenerateEventHeader(88)));
    }
}