using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Handlers;
using UserProfileService.Projection.SecondLevel.Tests.Comparer;
using UserProfileService.Projection.SecondLevel.Tests.Extensions;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;
using ApiEnums = Maverick.UserProfileService.Models.EnumModels;
using ApiModels = Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.SecondLevel.Tests.EventHandlerTests;

public class PropertiesChangedTests
{
    private readonly SecondLevelProjectionGroup _groupOfGroup;
    private readonly SecondLevelProjectionGroup _groupOfReferenceUser;
    private readonly ITestOutputHelper _output;
    private readonly SecondLevelProjectionFunction _referenceFunction;
    private readonly SecondLevelProjectionUser _referenceUser;

    public PropertiesChangedTests(ITestOutputHelper output)
    {
        _output = output;
        _referenceFunction = TestsDataStore.Instance.DataForPropertiesChangedTests.ReferenceFunction;
        _referenceUser = TestsDataStore.Instance.DataForPropertiesChangedTests.ReferenceUser;
        _groupOfReferenceUser = TestsDataStore.Instance.DataForPropertiesChangedTests.GroupOfReferenceUser;
        _groupOfGroup = TestsDataStore.Instance.DataForPropertiesChangedTests.GroupOfGroup;
    }

    private static PropertiesChanged GenerateNewChangedEvent(
        ObjectType objectType,
        string entityId,
        IDictionary<string, object> changedProperties,
        string relatedEntityId,
        PropertiesChangedContext context)
    {
        EventMetaData randomMetaData = MockDataGeneratorAggregateEvents.GenerateEventMetaData();

        return new PropertiesChanged
        {
            Id = entityId,
            ObjectType = objectType,
            Properties = changedProperties,
            EventId = "event-1",
            MetaData =
            {
                RelatedEntityId = relatedEntityId ?? entityId,
                CorrelationId = randomMetaData.CorrelationId,
                Initiator = randomMetaData.Initiator,
                VersionInformation = randomMetaData.VersionInformation,
                Timestamp = randomMetaData.Timestamp
            },
            RelatedContext = context
        };
    }

    private Mock<ISecondLevelProjectionRepository> GetRepository(
        MockDatabaseTransaction transaction)
    {
        var mock = new Mock<ISecondLevelProjectionRepository>();

        mock.ApplyWorkingTransactionSetup(transaction);

        mock.Setup(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _referenceUser.Id),
                    It.Is<IDatabaseTransaction>(t => transaction == t),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(_referenceUser.Clone());

        mock.Setup(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _groupOfReferenceUser.Id),
                    It.Is<IDatabaseTransaction>(t => transaction == t),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(_groupOfReferenceUser.Clone());

        mock.Setup(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _groupOfGroup.Id),
                    It.Is<IDatabaseTransaction>(t => transaction == t),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(_groupOfGroup.Clone());

        mock.Setup(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _referenceFunction.Organization.Id),
                    It.Is<IDatabaseTransaction>(t => transaction == t),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(AggregateResolvedModelConverter.CreateOrganization(_referenceFunction.Organization));

        mock.Setup(
                repo => repo.GetFunctionAsync(
                    It.Is<string>(id => id == _referenceFunction.Id),
                    It.Is<IDatabaseTransaction>(t => transaction == t),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(_referenceFunction.Clone());

        _output.WriteLine($"Configured GetFunction for id {_referenceFunction.Id}");

        return mock;
    }

    private Mock<IStreamNameResolver> GetStreamNameResolverMock()
    {
        var mock = new Mock<IStreamNameResolver>();

        mock.Setup(
                resolver => resolver.GetObjectIdentUsingStreamName(
                    It.Is<string>(name => name == _referenceFunction.Id)))
            .Returns(new ApiModels.ObjectIdent(_referenceFunction.Id, ApiEnums.ObjectType.Function));

        mock.Setup(
                resolver => resolver.GetObjectIdentUsingStreamName(
                    It.Is<string>(name => name == _referenceUser.Id)))
            .Returns(new ApiModels.ObjectIdent(_referenceUser.Id, ApiEnums.ObjectType.User));

        mock.Setup(
                resolver => resolver.GetObjectIdentUsingStreamName(
                    It.Is<string>(name => name == _referenceFunction.Organization.Id)))
            .Returns(
                new ApiModels.ObjectIdent(_referenceFunction.Organization.Id, ApiEnums.ObjectType.Organization));

        mock.Setup(
                resolver => resolver.GetObjectIdentUsingStreamName(
                    It.Is<string>(name => name == _groupOfReferenceUser.Id)))
            .Returns(new ApiModels.ObjectIdent(_groupOfReferenceUser.Id, ApiEnums.ObjectType.Group));

        mock.Setup(
                resolver => resolver.GetObjectIdentUsingStreamName(It.Is<string>(name => name == _groupOfGroup.Id)))
            .Returns(new ApiModels.ObjectIdent(_groupOfGroup.Id, ApiEnums.ObjectType.Group));

        return mock;
    }

    [Fact]
    public async Task Handle_message_changed_profile_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.RemoveAll<IStreamNameResolver>();
                s.AddSingleton(GetStreamNameResolverMock().Object);
            });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        PropertiesChanged changedEvent = GenerateNewChangedEvent(
            ObjectType.User,
            _referenceUser.Id,
            new Dictionary<string, object>()
                .AddChange("name", "whatever#1984")
                .AddChange(
                    "ExternalIds",
                    IdHelper.GenerateExternalIdList("new-id-42", "externalSource")
                        .AddIdentifier("S-ID", "AD")
                        .ToJArray()),
            null,
            PropertiesChangedContext.Self);

        StreamedEventHeader header = changedEvent.GenerateEventHeader(12U);

        SecondLevelProjectionUser modifiedProfile = _referenceUser.Clone();

        modifiedProfile.ExternalIds =
            ((JArray)changedEvent.Properties["ExternalIds"]).ToObject<List<ExternalIdentifier>>();

        modifiedProfile.Name = changedEvent.Properties["name"] as string;

        // act
        await sut.HandleEventAsync(changedEvent, header);

        // assert
        repoMock.VerifyWorkingTransactionMethods(transaction);

        repoMock.Verify(
            repo => repo.GetProfileAsync(
                It.Is(_referenceUser.Id, StringComparer.Ordinal),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.UpdateProfileAsync(
                It.Is(
                    modifiedProfile,
                    ProfileComparerForTest.CreateWithExcludeMember(
                        m => m.Path == nameof(ISecondLevelProjectionProfile.MemberOf))),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));
    }

    [Fact]
    public async Task Handle_message_changed_parent_group_of_user_should_work()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.RemoveAll<IStreamNameResolver>();
                s.AddSingleton(GetStreamNameResolverMock().Object);
            });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        PropertiesChanged changedEvent = GenerateNewChangedEvent(
            ObjectType.Group,
            _groupOfReferenceUser.Id,
            new Dictionary<string, object>()
                .AddChange("name", "whatever#1984")
                .AddChange(
                    "ExternalIds",
                    IdHelper.GenerateExternalIdList("new-id-42", "externalSource")
                        .AddIdentifier("S-ID", "AD")),
            _referenceUser.Id,
            PropertiesChangedContext.MemberOf);

        // act
        await sut.HandleEventAsync(changedEvent, changedEvent.GenerateEventHeader(88));

        // assert
        repoMock.VerifyWorkingTransactionMethods(transaction);

        repoMock.Verify(
            repo => repo.TryUpdateMemberOfAsync(
                It.Is<string>(related => related == _referenceUser.Id),
                It.Is<string>(member => member == _groupOfReferenceUser.Id),
                It.IsAny<IDictionary<string, object>>(),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.TryUpdateMemberAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IDatabaseTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        repoMock.Verify(
            repo => repo.UpdateProfileAsync(
                It.IsAny<ISecondLevelProjectionProfile>(),
                It.IsAny<IDatabaseTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(PropertiesChangedContext.Members)]
    [InlineData(PropertiesChangedContext.MemberOf)]
    public async Task Handle_message_changed_parent_group_of_child_group_should_work(
        PropertiesChangedContext context)
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.RemoveAll<IStreamNameResolver>();
                s.AddSingleton(GetStreamNameResolverMock().Object);
            });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        PropertiesChanged changedEvent = GenerateNewChangedEvent(
            ObjectType.Group,
            _groupOfReferenceUser.Id,
            new Dictionary<string, object>()
                .AddChange("name", "whatever#1984")
                .AddChange(
                    "ExternalIds",
                    IdHelper.GenerateExternalIdList("new-id-42", "externalSource")
                        .AddIdentifier("S-ID", "AD")),
            _groupOfGroup.Id,
            context);

        // act
        await sut.HandleEventAsync(changedEvent, changedEvent.GenerateEventHeader(88));

        // assert
        repoMock.VerifyWorkingTransactionMethods(transaction);

        if (context == PropertiesChangedContext.MemberOf)
        {
            repoMock.Verify(
                repo => repo.TryUpdateMemberOfAsync(
                    It.Is<string>(related => related == _groupOfGroup.Id),
                    It.Is<string>(member => member == _groupOfReferenceUser.Id),
                    ItShould.BeEquivalentTo(changedEvent.Properties),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(1));
        }

        if (context == PropertiesChangedContext.Members)
        {
            repoMock.Verify(
                repo => repo.TryUpdateMemberAsync(
                    It.Is<string>(related => related == _groupOfGroup.Id),
                    It.Is<string>(member => member == _groupOfReferenceUser.Id),
                    ItShould.BeEquivalentTo(changedEvent.Properties),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(1));
        }

        repoMock.Verify(
            repo => repo.UpdateProfileAsync(
                It.IsAny<ISecondLevelProjectionProfile>(),
                It.Is<IDatabaseTransaction>(
                    t =>
                        ((MockDatabaseTransaction)t).Id == transaction.Id),
                CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task Handle_message_without_a_resource_id_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s => { s.AddSingleton(repoMock.Object); });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        PropertiesChanged propertiesChanged = new PropertiesChanged
            {
                Properties = new Dictionary<string, object>
                {
                    { "Name", "new name" }
                }
            }
            .AddDefaultMetadata(services);

        // act & assert
        await Assert.ThrowsAsync<InvalidDomainEventException>(
            () => sut.HandleEventAsync(propertiesChanged, propertiesChanged.GenerateEventHeader(88)));
    }

    [Fact]
    public async Task Handle_message_with_null_values_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.RemoveAll<IStreamNameResolver>();
                s.AddSingleton(GetStreamNameResolverMock().Object);
            });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        PropertiesChanged propertiesChanged = new PropertiesChanged
            {
                Id = _referenceUser.Id,
                Properties = new Dictionary<string, object>
                {
                    { "null", null }
                }
            }
            .AddDefaultMetadata(_referenceUser.Id);

        // act & assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => sut.HandleEventAsync(propertiesChanged, propertiesChanged.GenerateEventHeader(88)));
    }

    [Fact]
    public async Task Handle_message_with_empty_dictionaries_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.RemoveAll<IStreamNameResolver>();
                s.AddSingleton(GetStreamNameResolverMock().Object);
            });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        PropertiesChanged propertiesChanged = new PropertiesChanged
            {
                Properties = new Dictionary<string, object>()
            }
            .AddDefaultMetadata(services);

        // act & assert
        await Assert.ThrowsAsync<InvalidHeaderException>(
            () => sut.HandleEventAsync(propertiesChanged, propertiesChanged.GenerateEventHeader(88)));
    }

    [Fact]
    public async Task Handle_null_message_should_fail()
    {
        //arrange
        var transaction = new MockDatabaseTransaction();
        Mock<ISecondLevelProjectionRepository> repoMock = GetRepository(transaction);

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.RemoveAll<IStreamNameResolver>();
                s.AddSingleton(GetStreamNameResolverMock().Object);
            });

        var sut = ActivatorUtilities.CreateInstance<PropertiesChangedEventHandler>(services);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(
                null,
                new StreamedEventHeader()));
    }
}