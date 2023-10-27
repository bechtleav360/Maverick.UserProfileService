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

public class ClientSettingsCalculatedHandlerTests
{
    private readonly ClientSettingsCalculated _calculatedClientSettingsOutlookLicense = new ClientSettingsCalculated
    {
        ProfileId = "6E398974-FD76-42CE-99F7-A56D06020A0A",
        Key = "Outlook-Version",
        CalculatedSettings = "O365-Premium"
    };
    private readonly ObjectIdent _user = new ObjectIdent("6E398974-FD76-42CE-99F7-A56D06020A0A", ObjectType.User);

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

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsCalculatedEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ClientSettingsCalculated(),
                _calculatedClientSettingsOutlookLicense.GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_user))));
    }

    [Fact]
    public async Task Handle_message_with_values_should_throw_argument_exception()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsCalculatedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                null,
                _calculatedClientSettingsOutlookLicense.GenerateEventHeader(14)));
    }

    [Theory]
    [InlineData(null, "Address-City", "Neckersulm")]
    [InlineData("26DB3C9F-89DF-43FE-AC4B-605FB8AC9236", null, "Neckersulm")]
    [InlineData("26DB3C9F-89DF-43FE-AC4B-605FB8AC9236", "Address-City", null)]
    [InlineData("", "Address-City", "Neckersulm")]
    [InlineData("26DB3C9F-89DF-43FE-AC4B-605FB8AC9236", "", "Neckersulm")]
    [InlineData("26DB3C9F-89DF-43FE-AC4B-605FB8AC9236", "Address-City", "")]
    [InlineData("  ", "Address-City", "Neckersulm")]
    [InlineData("26DB3C9F-89DF-43FE-AC4B-605FB8AC9236", "  ", "Neckersulm")]
    [InlineData("26DB3C9F-89DF-43FE-AC4B-605FB8AC9236", "Address-City", "  ")]
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

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsCalculatedEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ClientSettingsCalculated
                {
                    ProfileId = profileId,
                    Key = key,
                    CalculatedSettings = clientSettings
                },
                _calculatedClientSettingsOutlookLicense.GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_user))));
    }

    [Fact]
    public async Task Handle_client_settings_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsCalculatedEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        await sut.HandleEventAsync(
            _calculatedClientSettingsOutlookLicense,
            _calculatedClientSettingsOutlookLicense.GenerateEventHeader(
                14,
                streamNameResolve.GetStreamName(_user)));

        // act & assert
        repoMock.Verify(
            repo => repo.SetClientSettingsAsync(
                It.Is((string profile) => profile == _calculatedClientSettingsOutlookLicense.ProfileId),
                It.Is((string key) => key == _calculatedClientSettingsOutlookLicense.Key),
                It.Is(
                    (string clientSettings) =>
                        clientSettings == _calculatedClientSettingsOutlookLicense.CalculatedSettings),
                It.Is((bool isInherited) => isInherited == false),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Once);

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }
}