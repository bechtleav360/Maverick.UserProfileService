using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.UnitTests.Extensions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class ProfileClientSettingsSetHandlerTest
{
    private readonly ProfileClientSettingsSet _clientSettingsSetWindowsLicense = new ProfileClientSettingsSet
    {
        ProfileId = "AB73A0ED-BC0D-4AD7-B46F-E5127A368285",
        Key = "Windows-Version",
        ClientSettings = "Windows_10"
    };
    private readonly ObjectIdent _group = new ObjectIdent("AB73A0ED-BC0D-4AD7-B46F-E5127A368285", ObjectType.Group);

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Fact]
    public async Task Handle_message_with_null_event_should_throw_argument_null_exception()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                null,
                _clientSettingsSetWindowsLicense.GenerateEventHeader(14)));
    }

    [Fact]
    public async Task Handle_created_event_without_value_throw_argument_exception()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ProfileClientSettingsSet(),
                _clientSettingsSetWindowsLicense.GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_group))));
    }

    [Theory]
    [InlineData(null, "Promotion", "Principle")]
    [InlineData("73D0E0FD-C96D-415B-93BA-EF6529471430", null, "Principle")]
    [InlineData("73D0E0FD-C96D-415B-93BA-EF6529471430", "Promotion", null)]
    [InlineData("", "Promotion", "Principle")]
    [InlineData("73D0E0FD-C96D-415B-93BA-EF6529471430", "", "Principle")]
    [InlineData("73D0E0FD-C96D-415B-93BA-EF6529471430", "Promotion", "")]
    [InlineData("  ", "Promotion", "Principle")]
    [InlineData("73D0E0FD-C96D-415B-93BA-EF6529471430", "  ", "Principle")]
    [InlineData("73D0E0FD-C96D-415B-93BA-EF6529471430", "Promotion", "  ")]
    public async Task Handle_message_with_null_values_should_throw_argument_exception(
        string profileId,
        string key,
        string clientSettings)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ProfileClientSettingsSet
                {
                    ProfileId = profileId,
                    Key = key,
                    ClientSettings = clientSettings
                },
                _clientSettingsSetWindowsLicense.GenerateEventHeader(14, streamNameResolve.GetStreamName(_group))));
    }

    [Fact]
    public async Task Handle_client_settings_set_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        await sut.HandleEventAsync(
            _clientSettingsSetWindowsLicense,
            _clientSettingsSetWindowsLicense.GenerateEventHeader(14, streamNameResolve.GetStreamName(_group)));

        // act & assert
        repoMock.Verify(
            repo => repo.SetClientSettingsAsync(
                It.Is((string profile) => profile == _clientSettingsSetWindowsLicense.ProfileId),
                It.Is((string key) => key == _clientSettingsSetWindowsLicense.Key),
                It.Is((string clientSettings) => clientSettings == _clientSettingsSetWindowsLicense.ClientSettings),
                It.Is((bool isInherited) => isInherited == false),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Once);

        repoMock.Verify(
            repo => repo.UpdateProfilePropertiesAsync(
                It.Is(
                    _clientSettingsSetWindowsLicense.ProfileId,
                    StringComparer.OrdinalIgnoreCase),
                It.Is<IDictionary<string, object>>(i => i.ContainsKey(nameof(ISecondLevelProjectionProfile.UpdatedAt))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }
}