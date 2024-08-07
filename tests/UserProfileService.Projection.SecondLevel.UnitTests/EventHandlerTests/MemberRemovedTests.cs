﻿using System;
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
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class MemberRemovedTests
{
    private readonly SecondLevelProjectionGroup _container;
    private readonly MemberRemoved _memberRemovedEvent;

    public MemberRemovedTests()
    {
        SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
        _container = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

        _memberRemovedEvent =
            AggregateResolvedModelConverter.CreateMemberRemoved(user, _container);
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

        var sut = ActivatorUtilities.CreateInstance<MemberRemovedEventHandler>(services);

        // act
        var objectIdent = new ObjectIdent
        {
            Type = ObjectType.User
        };

        await sut.HandleEventAsync(
            _memberRemovedEvent.AddDefaultMetadata(services, objectIdent),
            _memberRemovedEvent.GenerateEventHeader(14));

        // assert
        repoMock.Verify(
            repo => repo.RemoveMemberAsync(
                It.Is<string>(c => c == _memberRemovedEvent.ParentId),
                It.Is<ContainerType>(c => c == _container.ContainerType),
                It.Is<string>(m => m == _memberRemovedEvent.MemberId),
                ItShould.BeEquivalentTo(_memberRemovedEvent.Conditions),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.UpdateProfilePropertiesAsync(
                It.Is(
                    _memberRemovedEvent.ParentId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(ISecondLevelProjectionProfile.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(_memberRemovedEvent.ParentType is ContainerType.Group or ContainerType.Organization ? 1 : 0));

        repoMock.Verify(
            repo => repo.UpdateRolePropertiesAsync(
                It.Is(
                    _memberRemovedEvent.ParentId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(RoleBasic.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(_memberRemovedEvent.ParentType is ContainerType.Role ? 1 : 0));

        repoMock.Verify(
            repo => repo.UpdateFunctionPropertiesAsync(
                It.Is(
                    _memberRemovedEvent.ParentId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(FunctionBasic.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(_memberRemovedEvent.ParentType is ContainerType.Function ? 1 : 0));

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

        var sut = ActivatorUtilities.CreateInstance<MemberRemovedEventHandler>(services);

        var objectIdent = new ObjectIdent
        {
            Type = ObjectType.User
        };

        MemberRemoved memberRemoved = new MemberRemoved().AddDefaultMetadata(services, objectIdent);

        // act
        await sut.HandleEventAsync(memberRemoved, memberRemoved.GenerateEventHeader(14));

        // assert
        repoMock.Verify(
            repo => repo.RemoveMemberAsync(
                It.Is<string>(m => m == memberRemoved.ParentId),
                It.Is<ContainerType>(m => m == ContainerType.NotSpecified),
                It.Is<string>(m => memberRemoved.MemberId == m),
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

        var sut = ActivatorUtilities.CreateInstance<MemberRemovedEventHandler>(services);

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

        var sut = ActivatorUtilities.CreateInstance<MemberRemovedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                new MemberRemoved(),
                null));
    }
}