using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.Tests.Extensions;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Tests.EventHandlerTests;

public class ProfileClientSettingsUnsetHandlerTest
{
    private readonly ProfileClientSettingsUnset _clientSettingsUnsetDevOpsPermission = new ProfileClientSettingsUnset
    {
        ProfileId = "D88F7E64-27DD-4CE2-B307-D4BE0E887047",
        Key = "Azure-DevOps-Permissions"
    };
    private readonly ObjectIdent _organization = new ObjectIdent("D88F7E64-27DD-4CE2-B307-D4BE0E887047", ObjectType.Organization);

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Fact]
    public async Task Handle_created_event_without_value_throw_argument_exception()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsUnsetEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ProfileClientSettingsUnset(),
                _clientSettingsUnsetDevOpsPermission.GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_organization))));
    }

    [Fact]
    public async Task Handle_message_with_values_should_throw_null_argument_exception()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsUnsetEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                null,
                _clientSettingsUnsetDevOpsPermission.GenerateEventHeader(14)));
    }

    [Theory]
    [InlineData(null, "Azure-DevOps-Permissions")]
    [InlineData("86EA5F51-F010-4510-9A8B-251FB14F1A0A", null)]
    [InlineData("", "Azure-DevOps-Permissions")]
    [InlineData("86EA5F51-F010-4510-9A8B-251FB14F1A0A", "")]
    [InlineData("  ", "Azure-DevOps-Permissions")]
    [InlineData("86EA5F51-F010-4510-9A8B-251FB14F1A0A", " ")]
    public async Task Handle_message_with_null_values_should_throw_argument_exception(
        string profileId,
        string key)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsUnsetEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ProfileClientSettingsUnset
                {
                    ProfileId = profileId,
                    Key = key
                },
                _clientSettingsUnsetDevOpsPermission.GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_organization))));
    }

    [Fact]
    public async Task Handle_client_settings_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsUnsetEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        await sut.HandleEventAsync(
            _clientSettingsUnsetDevOpsPermission,
            _clientSettingsUnsetDevOpsPermission.GenerateEventHeader(
                14,
                streamNameResolve.GetStreamName(_organization)));

        // act & assert
        repoMock.Verify(
            repo => repo.UnsetClientSettingFromProfileAsync(
                It.Is((string profile) => profile == _clientSettingsUnsetDevOpsPermission.ProfileId),
                It.Is((string key) => key == _clientSettingsUnsetDevOpsPermission.Key),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Once);

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }
}