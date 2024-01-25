using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.States;
using UserProfileService.Sync.States.Messages;
using UserProfileService.Sync.Utilities;
using Xunit;

namespace UserProfileService.Sync.UnitTests.States
{
    public class ProcessStateMachineTests
    {
        [Fact]
        public async Task Initially_When_StartSyncCommand_WithoutSourceSystem()
        {
            // Arrange
            var config = new SyncConfiguration();

            var command = new StartSyncCommand
                          {
                              CorrelationId = Guid.NewGuid(),
                              InitiatorId = "initiator"
                          };

            StateMachineFacade facade = await SetupStateMachine(config);

            // Act
            await facade.Harness.Bus.Publish(command);

            // Assert
            Assert.True(await facade.Harness.Consumed.Any<StartSyncCommand>());
            Assert.True(await facade.Harness.Consumed.Any<SetNextStepMessage>());
            Assert.True(await facade.Harness.Consumed.Any<FinalizeSyncMessage>());

            facade.ConfigMock.VerifyGet(t => t.Value, Times.Exactly(1));

            ISagaStateMachineTestHarness<ProcessStateMachine, ProcessState> sagaHarness =
                facade.Harness.GetSagaStateMachineHarness<ProcessStateMachine, ProcessState>();

            ProcessState saga = sagaHarness.Sagas.Contains((Guid)command.CorrelationId);

            Assert.Equal(ProcessStatus.Success, saga.Process.Status);
            Assert.True(saga.Process.StartedAt < saga.Process.UpdatedAt);
            Assert.NotNull(saga.Process.FinishedAt);
        }

        [Fact]
        public async Task Initially_When_StartSyncCommand_SingleSourceSystem_WithoutStep()
        {
            // Arrange
            var config = new SyncConfiguration();
            var sourceSystemConfig = new SourceSystemConfiguration();
            config.SourceConfiguration.Systems.Add(SyncConstants.System.Ldap, sourceSystemConfig);

            var command = new StartSyncCommand
                          {
                              CorrelationId = Guid.NewGuid(),
                              InitiatorId = "initiator"
                          };

            StateMachineFacade facade = await SetupStateMachine(config);

            // Act
            await facade.Harness.Bus.Publish(command);

            // Assert
            Assert.True(await facade.Harness.Consumed.Any<StartSyncCommand>());
            Assert.True(await facade.Harness.Consumed.Any<SetNextStepMessage>());
            Assert.True(await facade.Harness.Consumed.Any<FinalizeSyncMessage>());

            facade.ConfigMock.VerifyGet(t => t.Value, Times.Once);

            ISagaStateMachineTestHarness<ProcessStateMachine, ProcessState> sagaHarness =
                facade.Harness.GetSagaStateMachineHarness<ProcessStateMachine, ProcessState>();

            ProcessState saga = sagaHarness.Sagas.Contains((Guid)command.CorrelationId);

            Assert.Equal(ProcessStatus.Success, saga.Process.Status);
            Assert.True(saga.Process.StartedAt < saga.Process.UpdatedAt);
            Assert.NotNull(saga.Process.FinishedAt);
        }

        [Fact]
        public async Task Initially_When_StartSyncCommand_SingleSourceSystem_Step_WithoutOperations()
        {
            // Arrange
            var config = new SyncConfiguration();

            var bonneaSystemConfig = new SourceSystemConfiguration
                                     {
                                         Source = new Dictionary<string, SynchronizationOperations>
                                                  {
                                                      {
                                                          SyncConstants.SagaStep.GroupStep,
                                                          new SynchronizationOperations()
                                                      }
                                                  }
                                     };

            config.SourceConfiguration.Systems.Add(SyncConstants.System.Ldap, bonneaSystemConfig);

            var command = new StartSyncCommand
                          {
                              CorrelationId = Guid.NewGuid(),
                              InitiatorId = "initiator"
                          };

            StateMachineFacade facade = await SetupStateMachine(config);

            // Act
            await facade.Harness.Bus.Publish(command);

            // Assert
            Assert.True(await facade.Harness.Consumed.Any<StartSyncCommand>());
            Assert.True(await facade.Harness.Consumed.Any<SetNextStepMessage>());
            Assert.True(await facade.Harness.Published.Any<StartCollectingMessage>());
            Assert.True(await facade.Harness.Consumed.Any<GroupSyncMessage>());
            Assert.True(await facade.Harness.Consumed.Any<WaitingForResponseMessage>());
            Assert.True(await facade.Harness.Consumed.Any<SetNextStepMessage>());
            Assert.True(await facade.Harness.Consumed.Any<FinalizeSyncMessage>());

            facade.ConfigMock.VerifyGet(t => t.Value, Times.Exactly(1));

            ISagaStateMachineTestHarness<ProcessStateMachine, ProcessState> sagaHarness =
                facade.Harness.GetSagaStateMachineHarness<ProcessStateMachine, ProcessState>();

            ProcessState saga = sagaHarness.Sagas.Contains((Guid)command.CorrelationId);

            Assert.Equal(ProcessStatus.Success, saga.Process.Status);
            Assert.True(saga.Process.StartedAt < saga.Process.UpdatedAt);
            Assert.NotNull(saga.Process.FinishedAt);
        }

        private static async Task<StateMachineFacade> SetupStateMachine(SyncConfiguration config)
        {
            // Facade
            var facade = new StateMachineFacade
            {
                // Sync config
                ConfigMock = new Mock<IOptions<SyncConfiguration>>()
            };
            
            facade.ConfigMock.SetupGet(s => s.Value).Returns(config);

            var relationFactory = new Mock<IRelationFactory>();

            relationFactory.Setup(p => p.CreateRelationHandler(It.IsAny<string>(), It.IsAny<string>()))
                           .Returns(() => null);

            facade.GroupProcessorMock = new Mock<ISagaEntityProcessor<GroupSync>>();
            var userProcessor = new Mock<ISagaEntityProcessor<UserSync>>();

            facade.UserServiceMock = new Mock<IProfileService>();

            var syncProcessSynchronizer = new Mock<ISyncProcessSynchronizer>();

            syncProcessSynchronizer
                .Setup(s
                    => s.TryStartSync(It.IsAny<StartSyncCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            syncProcessSynchronizer
                .Setup(s
                    => s.IsSyncLockAvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            syncProcessSynchronizer
                .Setup(s
                    => s.ReleaseLockForRunningProcessAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var userProcessorFactory = new Mock<ISagaEntityProcessorFactory<UserSync>>();

            userProcessorFactory.Setup(t => t.Create(It.IsAny<IServiceProvider>(), It.IsAny<ILoggerFactory>(), config))
                                .Returns(userProcessor.Object);

            var groupProcessorFactory = new Mock<ISagaEntityProcessorFactory<GroupSync>>();

            groupProcessorFactory.Setup(t => t.Create(It.IsAny<IServiceProvider>(), It.IsAny<ILoggerFactory>(), config))
                                 .Returns(facade.GroupProcessorMock.Object);

            // ServiceProvider
            ServiceProvider provider = new ServiceCollection()
                                       .AddLogging()
                                       .AddSingleton(relationFactory.Object)
                                       .AddAutoMapper(typeof(MappingProfiles).Assembly)
                                       .AddSingleton(facade.UserServiceMock.Object)
                                       .AddSingleton(facade.ConfigMock.Object)
                                       .AddSingleton(groupProcessorFactory.Object)
                                       .AddSingleton(userProcessorFactory.Object)
                                       .AddSingleton(syncProcessSynchronizer.Object)
                                       .AddSingleton(sp => sp.GetRequiredService<ITestHarness>().Bus)
                                       .AddMassTransitTestHarness(
                                           cfg => { cfg.AddSagaStateMachine<ProcessStateMachine, ProcessState>(); })
                                       .BuildServiceProvider(false);

            facade.Harness = provider.GetRequiredService<ITestHarness>();

            await facade.Harness.Start();

            return facade;
        }
    }

    public class StateMachineFacade
    {
        public ITestHarness Harness { get; set; }

        public Mock<IOptions<SyncConfiguration>> ConfigMock { get; set; }

        public Mock<ISagaEntityProcessor<GroupSync>> GroupProcessorMock { get; set; }

        public Mock<IProfileService> UserServiceMock { get; set; }
    }
}
