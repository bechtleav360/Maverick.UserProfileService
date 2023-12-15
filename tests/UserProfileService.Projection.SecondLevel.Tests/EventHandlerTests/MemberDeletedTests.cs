using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.Tests.Extensions;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.Tests.EventHandlerTests;

public class MemberDeletedTests
{
    private readonly SecondLevelProjectionGroup _container;
    private readonly MemberDeleted _memberDeletedEvent;

    public MemberDeletedTests()
    {
        SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
        _container = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

        _memberDeletedEvent =
            AggregateResolvedModelConverter.CreateMemberDeleted(user, _container);
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

        var sut = ActivatorUtilities.CreateInstance<MemberDeletedEventHandler>(services);

        // act
        var objectIdent = new ObjectIdent
        {
            Type = ObjectType.Group
        };

        await sut.HandleEventAsync(
            _memberDeletedEvent.AddDefaultMetadata(services, objectIdent),
            _memberDeletedEvent.GenerateEventHeader(14));

        // assert
        repoMock.Verify(
            repo => repo.RemoveMemberAsync(
                It.Is<string>(c => c == _memberDeletedEvent.ContainerId),
                It.Is<ContainerType>(c => c == _container.ContainerType),
                It.Is<string>(m => m == _memberDeletedEvent.MemberId),
                It.IsAny<IList<RangeCondition>>(),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.UpdateProfilePropertiesAsync(
                It.Is(
                    _memberDeletedEvent.ContainerId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(ISecondLevelProjectionProfile.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(_container.ContainerType is ContainerType.Group or ContainerType.Organization ? 1 : 0));

        repoMock.Verify(
            repo => repo.UpdateRolePropertiesAsync(
                It.Is(
                    _memberDeletedEvent.ContainerId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(RoleBasic.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(_container.ContainerType is ContainerType.Role ? 1 : 0));

        repoMock.Verify(
            repo => repo.UpdateFunctionPropertiesAsync(
                It.Is(
                    _memberDeletedEvent.ContainerId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(FunctionBasic.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(_container.ContainerType is ContainerType.Function ? 1 : 0));

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

        var sut = ActivatorUtilities.CreateInstance<MemberDeletedEventHandler>(services);

        var objectIdent = new ObjectIdent
        {
            Type = ObjectType.User
        };

        MemberDeleted memberDeleted = new MemberDeleted().AddDefaultMetadata(services, objectIdent);

        // act
        await sut.HandleEventAsync(memberDeleted, memberDeleted.GenerateEventHeader(14));

        // assert
        repoMock.Verify(
            repo => repo.RemoveMemberAsync(
                It.Is<string>(m => memberDeleted.ContainerId == m),
                It.Is<ContainerType>(m => m == ContainerType.NotSpecified),
                It.Is<string>(m => memberDeleted.MemberId == m),
                It.IsAny<IList<RangeCondition>>(),
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

        var sut = ActivatorUtilities.CreateInstance<MemberDeletedEventHandler>(services);

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

        var sut = ActivatorUtilities.CreateInstance<MemberDeletedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new MemberDeleted(),
                null));
    }
}