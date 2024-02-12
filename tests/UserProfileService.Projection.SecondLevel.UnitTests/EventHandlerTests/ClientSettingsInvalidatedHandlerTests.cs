using System;
using System.Collections.Generic;
using System.Linq;
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

public class ClientSettingsInvalidatedHandlerTests
{
    private readonly ObjectIdent _role = new ObjectIdent("6E398974-FD76-42CE-99F7-A56D06020A0A", ObjectType.Role);

    public static readonly ClientSettingsInvalidated[] CalculatedClientSettingsInvalidate = new[]
    {
        new ClientSettingsInvalidated
        {
            ProfileId = "673C520D-F537-49B7-8A1C-05BAAF471F56",
            Keys = new[]
            {
                "Address-City", "DevOpsPermissions",
                "Favorite-Food", "Windows-License"
            }
        },
        new ClientSettingsInvalidated
        {
            ProfileId = "673C520D-F537-49B7-8A1C-05BAAF471F56",
            Keys = Array.Empty<string>()
        }
    };

    public static IEnumerable<object[]> GetCalculatedClientSettingsInvalidateArgs()
    {
        return CalculatedClientSettingsInvalidate.Select(ccsi => new object[] { ccsi });
    }

    private static Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        return mock;
    }

    [Theory]
    [MemberData(nameof(GetCalculatedClientSettingsInvalidateArgs))]
    public async Task Handle_created_event_without_value_throw_argument_exception(ClientSettingsInvalidated clientSettingsInvalidated)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsInvalidatedEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ClientSettingsInvalidated(),
                clientSettingsInvalidated.GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_role))));
    }

    [Theory]
    [MemberData(nameof(GetCalculatedClientSettingsInvalidateArgs))]
    public async Task Handle_message_with_values_should_throw_argument_exception(ClientSettingsInvalidated clientSettingsInvalidated)
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
                clientSettingsInvalidated.GenerateEventHeader(14)));
    }

    [Theory]
    [InlineData(null, new[] { "Test-Version", "Outlook-License" })]
    [InlineData("C8EC9B06-1F9A-412C-B226-D32D29BEF65E", null)]
    [InlineData("", new[] { "Test-Version", "Outlook-License" })]
    public async Task Handle_message_with_null_values_should_throw_argument_exception(
        string profileId,
        string[] keys)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsInvalidatedEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.HandleEventAsync(
                new ClientSettingsInvalidated
                {
                    ProfileId = profileId,
                    Keys = keys
                },
                CalculatedClientSettingsInvalidate[0].GenerateEventHeader(
                    14,
                    streamNameResolve.GetStreamName(_role))));
    }

    [Theory]
    [MemberData(nameof(GetCalculatedClientSettingsInvalidateArgs))]
    public async Task Handle_client_settings_should_work(ClientSettingsInvalidated clientSettingsInvalidated)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<ClientSettingsInvalidatedEventHandler>(services);

        var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

        await sut.HandleEventAsync(
            clientSettingsInvalidated,
            clientSettingsInvalidated.GenerateEventHeader(
                14,
                streamNameResolve.GetStreamName(_role)));

        // act & assert
        repoMock.Verify(
            repo => repo.InvalidateClientSettingsFromProfile(
                It.Is((string profile) => profile == clientSettingsInvalidated.ProfileId),
                It.Is((string[] key) => key.All(k => clientSettingsInvalidated.Keys.Contains(k))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Once);

        repoMock.VerifyWorkingTransactionMethods(transaction);
    }
}