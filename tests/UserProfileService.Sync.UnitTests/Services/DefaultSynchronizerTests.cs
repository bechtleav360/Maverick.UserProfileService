using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.Services;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Options;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Models;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Sync.UnitTests.Services;

public class DefaultSynchronizerTests
{
    private const string SynchronizationKey = SyncConstants.SynchronizationKeys.SynLockObject;

    [Theory]
    [AutoData]
    public async Task StartSync_should_fail_when_other_process_running(StartSyncCommand startCommand)
    {
        // Arrange
        var cacheStoreMock = new Mock<ICacheStore>();
        IOptions<SyncConfiguration> options = Options.Create(new SyncConfiguration());

        Mock<IServiceProvider> serviceProvider = CreateServiceProviderMock();
        var syncCleaner = new Mock<ISyncProcessCleaner>();

        serviceProvider
            .Setup(x => x.GetService(typeof(ISyncProcessCleaner)))
            .Returns(syncCleaner.Object);

        cacheStoreMock
            .Setup(
                c => c.GetAsync<SyncLock>(
                    It.Is<string>(i => i == SynchronizationKey),
                    It.IsAny<IJsonSerializerSettingsProvider>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new SyncLock
                {
                    IsReleased = false,
                    UpdatedAt = DateTime.Now
                });

        var synchronizer = new DefaultSynchronizer(
            new LoggerFactory().CreateLogger<DefaultSynchronizer>(),
            serviceProvider.Object,
            cacheStoreMock.Object,
            options);

        // Act
        bool result = await synchronizer.TryStartSync(startCommand);

        // Assert
        syncCleaner.Verify(cleaner => cleaner.UpdateAbortedProcessesStatusAsync(It.IsAny<CancellationToken>()),Times.Never());
        result.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public async Task StartSync_should_work(StartSyncCommand startCommand)
    {
        // Arrange
        var cacheStoreMock = new Mock<ICacheStore>();
        IOptions<SyncConfiguration> options = Options.Create(new SyncConfiguration());

        Mock<IServiceProvider> serviceProvider = CreateServiceProviderMock();
        var syncCleaner = new Mock<ISyncProcessCleaner>();

        serviceProvider
            .Setup(x => x.GetService(typeof(ISyncProcessCleaner)))
            .Returns(syncCleaner.Object);

        cacheStoreMock
            .Setup(
                c => c.GetAsync<SyncLock>(
                    It.Is<string>(i => i == SynchronizationKey),
                    It.IsAny<IJsonSerializerSettingsProvider>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new SyncLock
                {
                    IsReleased = true,
                    UpdatedAt = DateTime.Now
                });

        var synchronizer = new DefaultSynchronizer(
            new LoggerFactory().CreateLogger<DefaultSynchronizer>(),
            serviceProvider.Object,
            cacheStoreMock.Object,
            options);

        // Act
        bool result = await synchronizer.TryStartSync(startCommand);

        // Assert
        syncCleaner.Verify(cleaner => cleaner.UpdateAbortedProcessesStatusAsync(It.IsAny<CancellationToken>()), Times.Once());
        result.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public async Task StartSync_should_work_when_lock_not_exist_yet(StartSyncCommand startCommand)
    {
        // Arrange
        var cacheStoreMock = new Mock<ICacheStore>();
        var syncConfiguration = new SyncConfiguration();
        IOptions<SyncConfiguration> options = Options.Create(syncConfiguration);

        Mock<IServiceProvider> serviceProvider = CreateServiceProviderMock();
        var syncCleaner = new Mock<ISyncProcessCleaner>();

        serviceProvider
            .Setup(x => x.GetService(typeof(ISyncProcessCleaner)))
            .Returns(syncCleaner.Object);

        cacheStoreMock
            .Setup(
                c => c.GetAsync<SyncLock>(
                    It.Is<string>(i => i == SynchronizationKey),
                    It.IsAny<IJsonSerializerSettingsProvider>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        var synchronizer = new DefaultSynchronizer(
            new LoggerFactory().CreateLogger<DefaultSynchronizer>(),
            serviceProvider.Object,
            cacheStoreMock.Object,
            options);

        // Act
        bool result = await synchronizer.TryStartSync(startCommand);

        // Assert
        cacheStoreMock.Verify(
            c => c.SetAsync(
                SynchronizationKey,
                It.Is<SyncLock>(i => i.IsReleased == false),
                syncConfiguration.LockExpirationTime * 60,
                It.IsAny<ICacheTransaction>(),
                It.IsAny<IJsonSerializerSettingsProvider>(),
                It.IsAny<CancellationToken>()));

        syncCleaner.Verify(cleaner => cleaner.UpdateAbortedProcessesStatusAsync(It.IsAny<CancellationToken>()), Times.Once());
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReleaseLockForRunningProcesses_should_work()
    {
        // Arrange
        var cacheStoreMock = new Mock<ICacheStore>();
        var syncConfiguration = new SyncConfiguration();
        IOptions<SyncConfiguration> options = Options.Create(syncConfiguration);
        Mock<IServiceProvider> serviceProvider = CreateServiceProviderMock();
        
        cacheStoreMock
            .Setup(
                c => c.GetAsync<SyncLock>(
                    It.Is<string>(i => i == SynchronizationKey),
                    It.IsAny<IJsonSerializerSettingsProvider>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new SyncLock
                {
                    IsReleased = false,
                    UpdatedAt = DateTime.Now
                });

        var synchronizer = new DefaultSynchronizer(
            new LoggerFactory().CreateLogger<DefaultSynchronizer>(),
            serviceProvider.Object,
            cacheStoreMock.Object,
            options);

        // Act
        bool result = await synchronizer.ReleaseLockForRunningProcessAsync();

        // Assert
        cacheStoreMock.Verify(
            c => c.GetAsync<SyncLock>(
                SynchronizationKey,
                It.IsAny<IJsonSerializerSettingsProvider>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        cacheStoreMock.Verify(
            c => c.SetAsync(
                SynchronizationKey,
                It.Is<SyncLock>(i => i.IsReleased == true),
                syncConfiguration.LockExpirationTime * 60,
                It.IsAny<ICacheTransaction>(),
                It.IsAny<IJsonSerializerSettingsProvider>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
       
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReleaseLockForRunningProcesses_after_lock_expired_should_work()
    {
        // Arrange
        var cacheStoreMock = new Mock<ICacheStore>();
        var syncConfiguration = new SyncConfiguration();
        IOptions<SyncConfiguration> options = Options.Create(syncConfiguration);
        Mock<IServiceProvider> serviceProvider = CreateServiceProviderMock();

        cacheStoreMock
            .Setup(
                c => c.GetAsync<SyncLock>(
                    It.Is<string>(i => i == SynchronizationKey),
                    It.IsAny<IJsonSerializerSettingsProvider>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        var synchronizer = new DefaultSynchronizer(
            new LoggerFactory().CreateLogger<DefaultSynchronizer>(),
            serviceProvider.Object,
            cacheStoreMock.Object,
            options);

        // Act
        bool result = await synchronizer.ReleaseLockForRunningProcessAsync();

        // Assert
        cacheStoreMock.Verify(
            c => c.GetAsync<SyncLock>(
                SynchronizationKey,
                It.IsAny<IJsonSerializerSettingsProvider>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        cacheStoreMock.Verify(
            c => c.SetAsync(
                SynchronizationKey,
                It.Is<SyncLock>(i => i.IsReleased == true),
                syncConfiguration.LockExpirationTime * 60,
                It.IsAny<ICacheTransaction>(),
                It.IsAny<IJsonSerializerSettingsProvider>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.Should().BeTrue();
    }

    private static Mock<IServiceProvider> CreateServiceProviderMock()
    {
        var serviceProvider = new Mock<IServiceProvider>();

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);

        serviceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);
        return serviceProvider;
    }
}
