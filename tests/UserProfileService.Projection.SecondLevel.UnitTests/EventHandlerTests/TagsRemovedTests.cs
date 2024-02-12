using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class TagsRemovedTests
{
    private static (ObjectIdent objectIdent, TagsRemoved tagsRemoved) GenerateEventTestData(
        IServiceProvider serviceProvider,
        int? number = 2)
    {
        ObjectIdent obj = MockDataGenerator.GenerateObjectIdentInstances().Single();

        string[] tags = number switch
        {
            null => null,
            0 => Array.Empty<string>(),
            _ => MockDataGenerator.GenerateTagAggregateModels((int)number)
                .Select(t => t.Id)
                .ToArray()
        };

        TagsRemoved removedEvent =
            AggregateResolvedModelConverter.CreateTagsRemoved(obj, tags);

        removedEvent = removedEvent.AddDefaultMetadata(serviceProvider, obj);

        return (obj, removedEvent);
    }

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    private static bool IsObjectTypeProfileEntity(ObjectType objectType)
    {
        return objectType == ObjectType.User
            || objectType == ObjectType.Profile
            || objectType == ObjectType.Group
            || objectType == ObjectType.Organization;
    }

    [Fact]
    public async Task HandleEventAsync_Success()
    {
        // Arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<TagsRemovedEventHandler>(services);

        (ObjectIdent _, TagsRemoved removedEvent) = GenerateEventTestData(services);

        // Act
        await sut.HandleEventAsync(
            removedEvent,
            removedEvent.GenerateEventHeader(88));

        // Assert
        repoMock.Verify(
            repo => repo.RemoveTagFromObjectAsync(
                removedEvent.Id,
                removedEvent.Id,
                removedEvent.ObjectType,
                removedEvent.Tags,
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.UpdateProfilePropertiesAsync(
                It.Is(
                    removedEvent.Id,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(ISecondLevelProjectionProfile.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(IsObjectTypeProfileEntity(removedEvent.ObjectType) ? 1 : 0));

        repoMock.Verify(
            repo => repo.UpdateRolePropertiesAsync(
                It.Is(
                    removedEvent.Id,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(RoleBasic.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(removedEvent.ObjectType is ObjectType.Role ? 1 : 0));

        repoMock.Verify(
            repo => repo.UpdateFunctionPropertiesAsync(
                It.Is(
                    removedEvent.Id,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(FunctionBasic.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(removedEvent.ObjectType is ObjectType.Function ? 1 : 0));

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

        var sut = ActivatorUtilities.CreateInstance<TagsRemovedEventHandler>(services);

        (ObjectIdent _, TagsRemoved removedEvent) = GenerateEventTestData(services);

        removedEvent.Id = resourceId;

        // Act
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                removedEvent,
                removedEvent.GenerateEventHeader(88)));
    }

    [Fact]
    public async Task HandleEventAsync_Throw_Exception_If_ObjectType_IsUnknown()
    {
        // Arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<TagsRemovedEventHandler>(services);

        (ObjectIdent _, TagsRemoved removedEvent) = GenerateEventTestData(services);

        removedEvent.ObjectType = ObjectType.Unknown;

        // Act
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                removedEvent,
                removedEvent.GenerateEventHeader(88)));
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

        var sut = ActivatorUtilities.CreateInstance<TagsRemovedEventHandler>(services);

        (ObjectIdent _, TagsRemoved removedEvent) = GenerateEventTestData(services, number);

        removedEvent.ObjectType = ObjectType.Unknown;

        // Act
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(
                removedEvent,
                removedEvent.GenerateEventHeader(88)));
    }
}